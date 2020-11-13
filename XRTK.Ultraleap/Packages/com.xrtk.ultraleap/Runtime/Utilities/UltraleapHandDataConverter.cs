// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using XRTK.Definitions.Controllers.Hands;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;
using XRTK.Ultraleap.Extensions;
using XRTK.Services;
using XRTK.Extensions;
using XRTK.Ultraleap.Providers.Controllers;

namespace XRTK.Ultraleap.Utilities
{
    /// <summary>
    /// Converts Ultraleap hand data to <see cref="HandData"/>.
    /// </summary>
    public sealed class UltraleapHandDataConverter
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dataProvider">The <see cref="UltraleapHandControllerDataProvider"/> using this converter.</param>
        public UltraleapHandDataConverter(UltraleapHandControllerDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~UltraleapHandDataConverter()
        {
            if (!conversionProxyRootTransform.IsNull())
            {
                conversionProxyTransforms.Clear();
                conversionProxyRootTransform.Destroy();
            }
        }

        private readonly UltraleapHandControllerDataProvider dataProvider;
        private Transform conversionProxyRootTransform;
        private readonly Dictionary<TrackedHandJoint, Transform> conversionProxyTransforms = new Dictionary<TrackedHandJoint, Transform>();
        private readonly MixedRealityPose[] jointPoses = new MixedRealityPose[HandData.JointCount];

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
                handData.RootPose = GetHandRootPose(hand);
                handData.Joints = GetJointPoses(hand, handData.RootPose);
                //handData.PointerPose = GetPointerPose();
                handData.Mesh = new HandMeshData();
            }

            return true;
        }

        private MixedRealityPose[] GetJointPoses(Hand hand, MixedRealityPose handRootPose)
        {
            jointPoses[(int)TrackedHandJoint.Wrist] = GetJointPose(TrackedHandJoint.Wrist, handRootPose);
            jointPoses[(int)TrackedHandJoint.Palm] = GetJointPose(TrackedHandJoint.Palm, handRootPose, hand);

            jointPoses[(int)TrackedHandJoint.ThumbMetacarpal] = GetJointPose(TrackedHandJoint.ThumbMetacarpal, handRootPose, null, hand.Fingers[(int)Finger.FingerType.TYPE_THUMB].Bone(Bone.BoneType.TYPE_METACARPAL));
            jointPoses[(int)TrackedHandJoint.ThumbProximal] = GetJointPose(TrackedHandJoint.ThumbProximal, handRootPose, null, hand.Fingers[(int)Finger.FingerType.TYPE_THUMB].Bone(Bone.BoneType.TYPE_PROXIMAL));
            jointPoses[(int)TrackedHandJoint.ThumbDistal] = GetJointPose(TrackedHandJoint.ThumbDistal, handRootPose, null, hand.Fingers[(int)Finger.FingerType.TYPE_THUMB].Bone(Bone.BoneType.TYPE_DISTAL));
            jointPoses[(int)TrackedHandJoint.ThumbTip] = GetJointPose(TrackedHandJoint.ThumbTip, handRootPose, null, null, hand.Fingers[(int)Finger.FingerType.TYPE_THUMB]);

            jointPoses[(int)TrackedHandJoint.IndexMetacarpal] = GetJointPose(TrackedHandJoint.IndexMetacarpal, handRootPose, null, hand.Fingers[(int)Finger.FingerType.TYPE_INDEX].Bone(Bone.BoneType.TYPE_METACARPAL));
            jointPoses[(int)TrackedHandJoint.IndexProximal] = GetJointPose(TrackedHandJoint.IndexProximal, handRootPose, null, hand.Fingers[(int)Finger.FingerType.TYPE_INDEX].Bone(Bone.BoneType.TYPE_METACARPAL));
            jointPoses[(int)TrackedHandJoint.IndexIntermediate] = GetJointPose(TrackedHandJoint.IndexIntermediate, handRootPose, null, hand.Fingers[(int)Finger.FingerType.TYPE_INDEX].Bone(Bone.BoneType.TYPE_INTERMEDIATE));
            jointPoses[(int)TrackedHandJoint.IndexDistal] = GetJointPose(TrackedHandJoint.IndexDistal, handRootPose, null, hand.Fingers[(int)Finger.FingerType.TYPE_INDEX].Bone(Bone.BoneType.TYPE_DISTAL));
            jointPoses[(int)TrackedHandJoint.IndexTip] = GetJointPose(TrackedHandJoint.IndexTip, handRootPose, null, null, hand.Fingers[(int)Finger.FingerType.TYPE_INDEX]);

            jointPoses[(int)TrackedHandJoint.MiddleMetacarpal] = GetJointPose(TrackedHandJoint.MiddleMetacarpal, handRootPose, null, hand.Fingers[(int)Finger.FingerType.TYPE_MIDDLE].Bone(Bone.BoneType.TYPE_METACARPAL));
            jointPoses[(int)TrackedHandJoint.MiddleProximal] = GetJointPose(TrackedHandJoint.MiddleProximal, handRootPose, null, hand.Fingers[(int)Finger.FingerType.TYPE_MIDDLE].Bone(Bone.BoneType.TYPE_METACARPAL));
            jointPoses[(int)TrackedHandJoint.MiddleIntermediate] = GetJointPose(TrackedHandJoint.MiddleIntermediate, handRootPose, null, hand.Fingers[(int)Finger.FingerType.TYPE_MIDDLE].Bone(Bone.BoneType.TYPE_INTERMEDIATE));
            jointPoses[(int)TrackedHandJoint.MiddleDistal] = GetJointPose(TrackedHandJoint.MiddleDistal, handRootPose, null, hand.Fingers[(int)Finger.FingerType.TYPE_MIDDLE].Bone(Bone.BoneType.TYPE_DISTAL));
            jointPoses[(int)TrackedHandJoint.MiddleTip] = GetJointPose(TrackedHandJoint.MiddleTip, handRootPose, null, null, hand.Fingers[(int)Finger.FingerType.TYPE_MIDDLE]);

            jointPoses[(int)TrackedHandJoint.RingMetacarpal] = GetJointPose(TrackedHandJoint.RingMetacarpal, handRootPose, null, hand.Fingers[(int)Finger.FingerType.TYPE_RING].Bone(Bone.BoneType.TYPE_METACARPAL));
            jointPoses[(int)TrackedHandJoint.RingProximal] = GetJointPose(TrackedHandJoint.RingProximal, handRootPose, null, hand.Fingers[(int)Finger.FingerType.TYPE_RING].Bone(Bone.BoneType.TYPE_METACARPAL));
            jointPoses[(int)TrackedHandJoint.RingIntermediate] = GetJointPose(TrackedHandJoint.RingIntermediate, handRootPose, null, hand.Fingers[(int)Finger.FingerType.TYPE_RING].Bone(Bone.BoneType.TYPE_INTERMEDIATE));
            jointPoses[(int)TrackedHandJoint.RingDistal] = GetJointPose(TrackedHandJoint.RingDistal, handRootPose, null, hand.Fingers[(int)Finger.FingerType.TYPE_RING].Bone(Bone.BoneType.TYPE_DISTAL));
            jointPoses[(int)TrackedHandJoint.RingTip] = GetJointPose(TrackedHandJoint.RingTip, handRootPose, null, null, hand.Fingers[(int)Finger.FingerType.TYPE_RING]);

            jointPoses[(int)TrackedHandJoint.LittleMetacarpal] = GetJointPose(TrackedHandJoint.LittleMetacarpal, handRootPose, null, hand.Fingers[(int)Finger.FingerType.TYPE_PINKY].Bone(Bone.BoneType.TYPE_METACARPAL));
            jointPoses[(int)TrackedHandJoint.LittleProximal] = GetJointPose(TrackedHandJoint.LittleProximal, handRootPose, null, hand.Fingers[(int)Finger.FingerType.TYPE_PINKY].Bone(Bone.BoneType.TYPE_METACARPAL));
            jointPoses[(int)TrackedHandJoint.LittleIntermediate] = GetJointPose(TrackedHandJoint.LittleIntermediate, handRootPose, null, hand.Fingers[(int)Finger.FingerType.TYPE_PINKY].Bone(Bone.BoneType.TYPE_INTERMEDIATE));
            jointPoses[(int)TrackedHandJoint.LittleDistal] = GetJointPose(TrackedHandJoint.LittleDistal, handRootPose, null, hand.Fingers[(int)Finger.FingerType.TYPE_PINKY].Bone(Bone.BoneType.TYPE_DISTAL));
            jointPoses[(int)TrackedHandJoint.LittleTip] = GetJointPose(TrackedHandJoint.LittleTip, handRootPose, null, null, hand.Fingers[(int)Finger.FingerType.TYPE_PINKY]);

            return jointPoses;
        }

        /// <summary>
        /// Gets a single joint's <see cref="MixedRealityPose"/> relative to the hand root pose.
        /// </summary>
        /// <param name="trackedHandJoint">The <see cref="TrackedHandJoint"/> Id for the joint to get a <see cref="MixedRealityPose"/> for.</param>
        /// <param name="handRootPose">The hand's root <see cref="MixedRealityPose"/>. Joint poses are always relative to the root pose.</param>
        /// <param name="hand"><see cref="Hand"/> retrieved from the platform.</param>
        /// <param name="bone"><see cref="Bone"/> retrieved from the platform.</param>
        /// <param name="finger"><see cref="Finger"/> retrieved from the platform.</param>
        /// <returns>Joint <see cref="MixedRealityPose"/> relative to the hand's root pose.</returns>
        private MixedRealityPose GetJointPose(TrackedHandJoint trackedHandJoint, MixedRealityPose handRootPose, Hand hand = null, Bone bone = null, Finger finger = null)
        {
            var jointTransform = GetProxyTransform(trackedHandJoint);
            var playspaceTransform = MixedRealityToolkit.CameraSystem.MainCameraRig.PlayspaceTransform;

            if (trackedHandJoint == TrackedHandJoint.Wrist)
            {
                jointTransform.localPosition = handRootPose.Position;
                jointTransform.localRotation = handRootPose.Rotation;
            }
            else
            {
                jointTransform.parent = playspaceTransform;

                Vector3 jointPosition;
                Quaternion jointRotation;

                if (trackedHandJoint == TrackedHandJoint.ThumbTip ||
                    trackedHandJoint == TrackedHandJoint.IndexTip ||
                    trackedHandJoint == TrackedHandJoint.MiddleTip ||
                    trackedHandJoint == TrackedHandJoint.RingTip ||
                    trackedHandJoint == TrackedHandJoint.LittleTip)
                {
                    jointPosition = finger.TipPosition.ToVector3();
                    jointRotation = Quaternion.Euler(finger.Direction.ToVector3());
                }
                else if (trackedHandJoint == TrackedHandJoint.Palm)
                {
                    jointPosition = hand.PalmPosition.ToVector3();
                    jointRotation = Quaternion.LookRotation(handRootPose.Forward, -1 * hand.PalmNormal.ToVector3());
                }
                else
                {
                    jointPosition = bone.PrevJoint.ToVector3();
                    jointRotation = bone.Rotation.ToQuaternion();
                }

                jointTransform.localPosition = playspaceTransform.InverseTransformPoint(playspaceTransform.position + playspaceTransform.rotation * jointPosition);
                jointTransform.localRotation = Quaternion.Inverse(playspaceTransform.rotation) * playspaceTransform.rotation * jointRotation;
                jointTransform.parent = conversionProxyRootTransform;
            }

            return new MixedRealityPose(
                conversionProxyRootTransform.TransformPoint(jointTransform.position),
                Quaternion.Inverse(conversionProxyRootTransform.rotation) * jointTransform.rotation);
        }

        /// <summary>
        /// Gets the hand's root <see cref="MixedRealityPose"/>.
        /// </summary>
        /// <param name="hand">The <see cref="Hand"/> data object for the current frame.</param>
        /// <returns>The hands <see cref="HandData.RootPose"/> value.</returns>
        private MixedRealityPose GetHandRootPose(Hand hand)
        {
            var playspaceTransform = MixedRealityToolkit.CameraSystem.MainCameraRig.PlayspaceTransform;
            var rootPosition = playspaceTransform.InverseTransformPoint(playspaceTransform.position + playspaceTransform.rotation * (hand.Arm.WristPosition.ToVector3()));
            var rootRotation = Quaternion.Inverse(playspaceTransform.rotation) * playspaceTransform.rotation * Quaternion.identity;

            return new MixedRealityPose(rootPosition, rootRotation);
        }

        private Transform GetProxyTransform(TrackedHandJoint handJointKind)
        {
            if (conversionProxyRootTransform.IsNull())
            {
                conversionProxyRootTransform = new GameObject("Ultraleap Hand Conversion Proxy").transform;
                conversionProxyRootTransform.transform.SetParent(MixedRealityToolkit.CameraSystem.MainCameraRig.PlayspaceTransform, false);
                conversionProxyRootTransform.gameObject.SetActive(false);
            }

            if (dataProvider.OperationMode == Definitions.UltraleapOperationMode.Desktop)
            {
                conversionProxyRootTransform.position =
                    MixedRealityToolkit.CameraSystem.MainCameraRig.PlayerCamera.transform.position +
                    dataProvider.LeapControllerOffset;
            }

            if (handJointKind == TrackedHandJoint.Wrist)
            {
                return conversionProxyRootTransform;
            }

            if (conversionProxyTransforms.ContainsKey(handJointKind))
            {
                return conversionProxyTransforms[handJointKind];
            }

            var transform = new GameObject($"Ultraleap Hand {handJointKind} Proxy").transform;
            transform.SetParent(MixedRealityToolkit.CameraSystem.MainCameraRig.PlayspaceTransform, false);
            conversionProxyTransforms.Add(handJointKind, transform);

            return transform;
        }
    }
}