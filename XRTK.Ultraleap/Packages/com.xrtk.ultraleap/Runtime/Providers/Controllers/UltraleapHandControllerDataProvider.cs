// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;
using XRTK.Ultraleap.Profiles;
using XRTK.Providers.Controllers.Hands;
using XRTK.Services;
using XRTK.Ultraleap.Definitions;
using XRTK.Interfaces.InputSystem;
using XRTK.Definitions.Platforms;
using XRTK.Attributes;
using XRTK.Definitions.Controllers.Hands;
using XRTK.Ultraleap.Extensions;
using XRTK.Utilities;

namespace XRTK.Ultraleap.Providers.Controllers
{
    [RuntimePlatform(typeof(UltraleapPlatform))]
    [System.Runtime.InteropServices.Guid("61cec407-ffa4-4a5c-b96a-5229348f85c2")]
    public class UltraleapHandControllerDataProvider : BaseHandControllerDataProvider
    {
        /// <inheritdoc />
        public UltraleapHandControllerDataProvider(string name, uint priority, UltraleapHandControllerDataProviderProfile profile, IMixedRealityInputSystem parentService)
            : base(name, priority, profile, parentService)
        {
            OperationMode = profile.OperationMode;
            LeapControllerOffset = profile.LeapControllerDesktopModeOffset;
            postProcessor = new HandDataPostProcessor(TrackedPoses);
        }

        private const int leapThumbIndex = (int)Finger.FingerType.TYPE_THUMB;
        private const int leapIndexFingerIndex = (int)Finger.FingerType.TYPE_INDEX;
        private const int leapMiddleFingerIndex = (int)Finger.FingerType.TYPE_MIDDLE;
        private const int leapRingFingerIndex = (int)Finger.FingerType.TYPE_RING;
        private const int leapLittleFingerIndex = (int)Finger.FingerType.TYPE_PINKY;

        private const int leapMetacarpalBoneIndex = (int)Bone.BoneType.TYPE_METACARPAL;
        private const int leapProximalBoneIndex = (int)Bone.BoneType.TYPE_PROXIMAL;
        private const int leapIntermediateBoneIndex = (int)Bone.BoneType.TYPE_INTERMEDIATE;
        private const int leapDistalBoneIndex = (int)Bone.BoneType.TYPE_DISTAL;

        private readonly HandDataPostProcessor postProcessor;
        private readonly Dictionary<Handedness, int> handIdMap = new Dictionary<Handedness, int>();
        private readonly Dictionary<Handedness, MixedRealityHandController> activeControllers = new Dictionary<Handedness, MixedRealityHandController>();
        private readonly Controller leapController = new Controller();
        private readonly MixedRealityPose[] jointPoses = new MixedRealityPose[HandData.JointCount];

        /// <summary>
        /// Gets the ultraleap controller's current operation mode.
        /// </summary>
        public UltraleapOperationMode OperationMode { get; }

        /// <summary>
        /// Offset applied to the rendered hands when in <see cref="UltraleapOperationMode.Desktop"/> mode.
        /// </summary>
        public Vector3 LeapControllerOffset { get; }

        /// <inheritdoc />
        public override void Enable()
        {
            base.Enable();

            if (!leapController.IsConnected)
            {
                leapController.StartConnection();
            }

            leapController.FrameReady += LeapController_FrameReady;
        }

        /// <inheritdoc />
        public override void Disable()
        {
            if (leapController.IsConnected)
            {
                leapController.StopConnection();
            }

            leapController.FrameReady -= LeapController_FrameReady;

            foreach (var activeController in activeControllers)
            {
                RemoveController(activeController.Key, false);
            }

            activeControllers.Clear();
            handIdMap.Clear();
        }

        #region Leap Controller Event Handlers

        private void LeapController_FrameReady(object sender, Leap.FrameEventArgs e)
        {
            bool isLeftHandTracked = false;
            bool isRightHandTracked = false;

            Leap.Frame frame = e.frame;
            for (int i = 0; i < frame.Hands.Count; i++)
            {
                var hand = frame.Hands[i];

                if (hand.IsLeft && VerifyHandId(Handedness.Left, hand.Id) && TryGetHandData(hand, out var leftHandData))
                {
                    isLeftHandTracked = true;


                    var controller = GetOrAddController(Handedness.Left, hand.Id);
                    leftHandData = postProcessor.PostProcess(Handedness.Left, leftHandData);
                    controller?.UpdateController(leftHandData);
                }
                else if (hand.IsRight && VerifyHandId(Handedness.Right, hand.Id) && TryGetHandData(hand, out var rightHandData))
                {
                    isRightHandTracked = true;
                    var controller = GetOrAddController(Handedness.Right, hand.Id);
                    rightHandData = postProcessor.PostProcess(Handedness.Right, rightHandData);
                    controller?.UpdateController(rightHandData);
                }
            }

            if (!isLeftHandTracked)
            {
                RemoveController(Handedness.Left);
            }

            if (!isRightHandTracked)
            {
                RemoveController(Handedness.Right);
            }
        }

        #endregion Leap Controller Event Handlers

        private bool TryGetController(Handedness handedness, out MixedRealityHandController controller)
        {
            if (activeControllers.ContainsKey(handedness))
            {
                var existingController = activeControllers[handedness];
                Debug.Assert(existingController != null, $"Hand Controller {handedness} has been destroyed but remains in the active controller registry.");
                controller = existingController;
                return true;
            }

            controller = null;
            return false;
        }

        private MixedRealityHandController GetOrAddController(Handedness handedness, int handId)
        {
            // If a device is already registered with the handedness, just return it.
            if (TryGetController(handedness, out var existingController))
            {
                return existingController;
            }

            MixedRealityHandController detectedController;
            try
            {
                detectedController = new MixedRealityHandController(this, TrackingState.Tracked, handedness, GetControllerMappingProfile(typeof(MixedRealityHandController), handedness));
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create {nameof(MixedRealityHandController)}!\n{e}");
                return null;
            }

            detectedController.TryRenderControllerModel();
            AddController(detectedController);
            activeControllers.Add(handedness, detectedController);
            handIdMap.Add(handedness, handId);
            MixedRealityToolkit.InputSystem?.RaiseSourceDetected(detectedController.InputSource, detectedController);

            return detectedController;
        }

        private void RemoveController(Handedness handedness, bool removeFromRegistry = true)
        {
            if (TryGetController(handedness, out var controller))
            {
                MixedRealityToolkit.InputSystem?.RaiseSourceLost(controller.InputSource, controller);

                if (removeFromRegistry)
                {
                    RemoveController(controller);
                    activeControllers.Remove(handedness);
                    handIdMap.Remove(handedness);
                }
            }
        }

        private bool VerifyHandId(Handedness handedness, int handId)
        {
            if (handIdMap.ContainsKey(handedness))
            {
                // A hand ID for the provided ID is already
                // registered. We can only update the controller
                // for the handedness if the IDs match.
                return handIdMap[handedness] == handId;
            }

            // A hand ID for the provided handedness is currently
            // not registered at all. That means it's safe to create
            // a new controller for the provided ID.
            return true;
        }

        /// <summary>
        /// Reads hand APIs for the current frame and converts it to agnostic <see cref="HandData"/>.
        /// </summary>
        /// <param name="handedness">The handedness of the hand to get <see cref="HandData"/> for.</param>
        /// <param name="handData">The output <see cref="HandData"/>.</param>
        /// <returns>True, if data conversion was a success.</returns>
        public bool TryGetHandData(Hand hand, out HandData handData)
        {
            if (hand == null)
            {
                handData = default;
                return false;
            }

            handData = new HandData
            {
                TrackingState = TrackingState.Tracked,
                UpdatedAt = DateTimeOffset.UtcNow.Ticks
            };

            if (handData.TrackingState == TrackingState.Tracked)
            {
                var offsetPose = GetHandOffsetPose(OperationMode, LeapControllerOffset);
                var handRootPose = GetHandRootPose(hand);

                handData.RootPose = offsetPose + handRootPose;
                handData.Joints = GetJointPoses(hand, handRootPose, OperationMode, LeapControllerOffset);
                handData.Mesh = new HandMeshData();
            }

            return true;
        }

        private MixedRealityPose[] GetJointPoses(Hand hand, MixedRealityPose handRootPose, UltraleapOperationMode operationMode, Vector3 offset)
        {
            for (var i = 0; i < HandData.JointCount; i++)
            {
                var trackedHandJoint = (TrackedHandJoint)i;
                var position = Vector3.zero;
                var rotation = Quaternion.identity;

                switch (trackedHandJoint)
                {
                    case TrackedHandJoint.Wrist:
                        position = hand.WristPosition.ToLeftHandedUnityVector3();
                        rotation = hand.Arm.Basis.rotation.ToLeftHandedUnityQuaternion();
                        break;
                    case TrackedHandJoint.Palm:
                        position = hand.PalmPosition.ToLeftHandedUnityVector3();
                        rotation = hand.Basis.rotation.ToLeftHandedUnityQuaternion();
                        break;
                    case TrackedHandJoint.ThumbMetacarpal:
                        position = hand.Fingers[leapThumbIndex].bones[leapMetacarpalBoneIndex].NextJoint.ToLeftHandedUnityVector3();
                        rotation = hand.Fingers[leapThumbIndex].bones[leapProximalBoneIndex].Rotation.ToLeftHandedUnityQuaternion();
                        break;
                    case TrackedHandJoint.ThumbProximal:
                        position = hand.Fingers[leapThumbIndex].bones[leapProximalBoneIndex].NextJoint.ToLeftHandedUnityVector3();
                        rotation = hand.Fingers[leapThumbIndex].bones[leapIntermediateBoneIndex].Rotation.ToLeftHandedUnityQuaternion();
                        break;
                    case TrackedHandJoint.ThumbDistal:
                        position = hand.Fingers[leapThumbIndex].bones[leapIntermediateBoneIndex].NextJoint.ToLeftHandedUnityVector3();
                        rotation = hand.Fingers[leapThumbIndex].bones[leapDistalBoneIndex].Rotation.ToLeftHandedUnityQuaternion();
                        break;
                    case TrackedHandJoint.ThumbTip:
                        position = hand.Fingers[leapThumbIndex].bones[leapDistalBoneIndex].NextJoint.ToLeftHandedUnityVector3();
                        rotation = hand.Fingers[leapThumbIndex].bones[leapDistalBoneIndex].Rotation.ToLeftHandedUnityQuaternion();
                        break;
                    case TrackedHandJoint.IndexProximal:
                        position = hand.Fingers[leapIndexFingerIndex].bones[leapMetacarpalBoneIndex].NextJoint.ToLeftHandedUnityVector3();
                        rotation = hand.Fingers[leapIndexFingerIndex].bones[leapProximalBoneIndex].Rotation.ToLeftHandedUnityQuaternion();
                        break;
                    case TrackedHandJoint.IndexIntermediate:
                        position = hand.Fingers[leapIndexFingerIndex].bones[leapProximalBoneIndex].NextJoint.ToLeftHandedUnityVector3();
                        rotation = hand.Fingers[leapIndexFingerIndex].bones[leapIntermediateBoneIndex].Rotation.ToLeftHandedUnityQuaternion();
                        break;
                    case TrackedHandJoint.IndexDistal:
                        position = hand.Fingers[leapIndexFingerIndex].bones[leapIntermediateBoneIndex].NextJoint.ToLeftHandedUnityVector3();
                        rotation = hand.Fingers[leapIndexFingerIndex].bones[leapDistalBoneIndex].Rotation.ToLeftHandedUnityQuaternion();
                        break;
                    case TrackedHandJoint.IndexTip:
                        position = hand.Fingers[leapIndexFingerIndex].bones[leapDistalBoneIndex].NextJoint.ToLeftHandedUnityVector3();
                        rotation = hand.Fingers[leapIndexFingerIndex].bones[leapDistalBoneIndex].Rotation.ToLeftHandedUnityQuaternion();
                        break;
                    case TrackedHandJoint.MiddleProximal:
                        position = hand.Fingers[leapMiddleFingerIndex].bones[leapMetacarpalBoneIndex].NextJoint.ToLeftHandedUnityVector3();
                        rotation = hand.Fingers[leapMiddleFingerIndex].bones[leapProximalBoneIndex].Rotation.ToLeftHandedUnityQuaternion();
                        break;
                    case TrackedHandJoint.MiddleIntermediate:
                        position = hand.Fingers[leapMiddleFingerIndex].bones[leapProximalBoneIndex].NextJoint.ToLeftHandedUnityVector3();
                        rotation = hand.Fingers[leapMiddleFingerIndex].bones[leapIntermediateBoneIndex].Rotation.ToLeftHandedUnityQuaternion();
                        break;
                    case TrackedHandJoint.MiddleDistal:
                        position = hand.Fingers[leapMiddleFingerIndex].bones[leapIntermediateBoneIndex].NextJoint.ToLeftHandedUnityVector3();
                        rotation = hand.Fingers[leapMiddleFingerIndex].bones[leapDistalBoneIndex].Rotation.ToLeftHandedUnityQuaternion();
                        break;
                    case TrackedHandJoint.MiddleTip:
                        position = hand.Fingers[leapMiddleFingerIndex].bones[leapDistalBoneIndex].NextJoint.ToLeftHandedUnityVector3();
                        rotation = hand.Fingers[leapMiddleFingerIndex].bones[leapDistalBoneIndex].Rotation.ToLeftHandedUnityQuaternion();
                        break;
                    case TrackedHandJoint.RingProximal:
                        position = hand.Fingers[leapRingFingerIndex].bones[leapMetacarpalBoneIndex].NextJoint.ToLeftHandedUnityVector3();
                        rotation = hand.Fingers[leapRingFingerIndex].bones[leapProximalBoneIndex].Rotation.ToLeftHandedUnityQuaternion();
                        break;
                    case TrackedHandJoint.RingIntermediate:
                        position = hand.Fingers[leapRingFingerIndex].bones[leapProximalBoneIndex].NextJoint.ToLeftHandedUnityVector3();
                        rotation = hand.Fingers[leapRingFingerIndex].bones[leapIntermediateBoneIndex].Rotation.ToLeftHandedUnityQuaternion();
                        break;
                    case TrackedHandJoint.RingDistal:
                        position = hand.Fingers[leapRingFingerIndex].bones[leapIntermediateBoneIndex].NextJoint.ToLeftHandedUnityVector3();
                        rotation = hand.Fingers[leapRingFingerIndex].bones[leapDistalBoneIndex].Rotation.ToLeftHandedUnityQuaternion();
                        break;
                    case TrackedHandJoint.RingTip:
                        position = hand.Fingers[leapRingFingerIndex].bones[leapDistalBoneIndex].NextJoint.ToLeftHandedUnityVector3();
                        rotation = hand.Fingers[leapRingFingerIndex].bones[leapDistalBoneIndex].Rotation.ToLeftHandedUnityQuaternion();
                        break;
                    case TrackedHandJoint.LittleMetacarpal:
                        position = hand.Fingers[leapLittleFingerIndex].bones[leapMetacarpalBoneIndex].PrevJoint.ToLeftHandedUnityVector3();
                        rotation = hand.Fingers[leapLittleFingerIndex].bones[leapProximalBoneIndex].Rotation.ToLeftHandedUnityQuaternion();
                        break;
                    case TrackedHandJoint.LittleProximal:
                        position = hand.Fingers[leapLittleFingerIndex].bones[leapMetacarpalBoneIndex].NextJoint.ToLeftHandedUnityVector3();
                        rotation = hand.Fingers[leapLittleFingerIndex].bones[leapProximalBoneIndex].Rotation.ToLeftHandedUnityQuaternion();
                        break;
                    case TrackedHandJoint.LittleIntermediate:
                        position = hand.Fingers[leapLittleFingerIndex].bones[leapProximalBoneIndex].NextJoint.ToLeftHandedUnityVector3();
                        rotation = hand.Fingers[leapLittleFingerIndex].bones[leapIntermediateBoneIndex].Rotation.ToLeftHandedUnityQuaternion();
                        break;
                    case TrackedHandJoint.LittleDistal:
                        position = hand.Fingers[leapLittleFingerIndex].bones[leapIntermediateBoneIndex].NextJoint.ToLeftHandedUnityVector3();
                        rotation = hand.Fingers[leapLittleFingerIndex].bones[leapDistalBoneIndex].Rotation.ToLeftHandedUnityQuaternion();
                        break;
                    case TrackedHandJoint.LittleTip:
                        position = hand.Fingers[leapLittleFingerIndex].bones[leapDistalBoneIndex].NextJoint.ToLeftHandedUnityVector3();
                        rotation = hand.Fingers[leapLittleFingerIndex].bones[leapDistalBoneIndex].Rotation.ToLeftHandedUnityQuaternion();
                        break;
                }

                jointPoses[i] = new MixedRealityPose(position - handRootPose.Position, rotation);
            }

            // Fill missing joints by estimating their pose.
            jointPoses[(int)TrackedHandJoint.RingMetacarpal] = HandUtilities.GetEstimatedRingMetacarpalPose(jointPoses);
            jointPoses[(int)TrackedHandJoint.MiddleMetacarpal] = HandUtilities.GetEstimatedMiddleMetacarpalPose(jointPoses);
            jointPoses[(int)TrackedHandJoint.IndexMetacarpal] = HandUtilities.GetEstimatedIndexMetacarpalPose(jointPoses);

            return jointPoses;
        }

        private MixedRealityPose GetHandRootPose(Hand hand)
        {
            var position = hand.WristPosition.ToLeftHandedUnityVector3();
            var rotation = hand.Arm.Basis.rotation.ToLeftHandedUnityQuaternion();

            return new MixedRealityPose(position, rotation);
        }

        private MixedRealityPose GetHandOffsetPose(UltraleapOperationMode operationMode, Vector3 offset)
        {
            switch (operationMode)
            {
                case UltraleapOperationMode.Desktop:
                    var cameraTransform = MixedRealityToolkit.CameraSystem != null
                        ? MixedRealityToolkit.CameraSystem.MainCameraRig.PlayerCamera.transform
                        : CameraCache.Main.transform;

                    return new MixedRealityPose(cameraTransform.position + offset, Quaternion.identity);
                case UltraleapOperationMode.HeadsetMounted:
                default:
                    return MixedRealityPose.ZeroIdentity;
            }
        }
    }
}