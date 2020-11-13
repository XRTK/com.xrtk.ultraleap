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
using XRTK.Utilities;

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

        private const int leapThumbIndex = (int)Finger.FingerType.TYPE_THUMB;
        private const int leapIndexFingerIndex = (int)Finger.FingerType.TYPE_INDEX;
        private const int leapMiddleFingerIndex = (int)Finger.FingerType.TYPE_MIDDLE;
        private const int leapRingFingerIndex = (int)Finger.FingerType.TYPE_RING;
        private const int leapLittleFingerIndex = (int)Finger.FingerType.TYPE_PINKY;

        private const int leapMetacarpalBoneIndex = (int)Bone.BoneType.TYPE_METACARPAL;
        private const int leapProximalBoneIndex = (int)Bone.BoneType.TYPE_PROXIMAL;
        private const int leapIntermediateBoneIndex = (int)Bone.BoneType.TYPE_INTERMEDIATE;
        private const int leapDistalBoneIndex = (int)Bone.BoneType.TYPE_DISTAL;

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
            for (var i = 0; i < HandData.JointCount; i++)
            {

                var trackedHandJoint = (TrackedHandJoint)i;
                switch (trackedHandJoint)
                {
                    case TrackedHandJoint.Wrist:
                        jointPoses[i] = new MixedRealityPose(
                            hand.WristPosition.ToLeftHandedUnityVector3(),
                            hand.Arm.Basis.rotation.ToLeftHandedUnityQuaternion());
                        break;
                    case TrackedHandJoint.Palm:
                        jointPoses[i] = new MixedRealityPose(
                            hand.PalmPosition.ToLeftHandedUnityVector3(),
                            hand.Basis.rotation.ToLeftHandedUnityQuaternion());
                        break;
                    case TrackedHandJoint.ThumbMetacarpal:
                        jointPoses[i] = new MixedRealityPose(
                            hand.Fingers[leapThumbIndex].bones[leapMetacarpalBoneIndex].NextJoint.ToLeftHandedUnityVector3(),
                            hand.Fingers[leapThumbIndex].bones[leapProximalBoneIndex].Rotation.ToLeftHandedUnityQuaternion());
                        break;
                    case TrackedHandJoint.ThumbProximal:
                        jointPoses[i] = new MixedRealityPose(
                            hand.Fingers[leapThumbIndex].bones[leapProximalBoneIndex].NextJoint.ToLeftHandedUnityVector3(),
                            hand.Fingers[leapThumbIndex].bones[leapIntermediateBoneIndex].Rotation.ToLeftHandedUnityQuaternion());
                        break;
                    case TrackedHandJoint.ThumbDistal:
                        jointPoses[i] = new MixedRealityPose(
                            hand.Fingers[leapThumbIndex].bones[leapIntermediateBoneIndex].NextJoint.ToLeftHandedUnityVector3(),
                            hand.Fingers[leapThumbIndex].bones[leapDistalBoneIndex].Rotation.ToLeftHandedUnityQuaternion());
                        break;
                    case TrackedHandJoint.ThumbTip:
                        jointPoses[i] = new MixedRealityPose(
                            hand.Fingers[leapThumbIndex].bones[leapDistalBoneIndex].NextJoint.ToLeftHandedUnityVector3(),
                            hand.Fingers[leapThumbIndex].bones[leapDistalBoneIndex].Rotation.ToLeftHandedUnityQuaternion());
                        break;
                    case TrackedHandJoint.IndexProximal:
                        jointPoses[i] = new MixedRealityPose(
                            hand.Fingers[leapIndexFingerIndex].bones[leapMetacarpalBoneIndex].NextJoint.ToLeftHandedUnityVector3(),
                            hand.Fingers[leapIndexFingerIndex].bones[leapProximalBoneIndex].Rotation.ToLeftHandedUnityQuaternion());
                        break;
                    case TrackedHandJoint.IndexIntermediate:
                        jointPoses[i] = new MixedRealityPose(
                            hand.Fingers[leapIndexFingerIndex].bones[leapProximalBoneIndex].NextJoint.ToLeftHandedUnityVector3(),
                            hand.Fingers[leapIndexFingerIndex].bones[leapIntermediateBoneIndex].Rotation.ToLeftHandedUnityQuaternion());
                        break;
                    case TrackedHandJoint.IndexDistal:
                        jointPoses[i] = new MixedRealityPose(
                            hand.Fingers[leapIndexFingerIndex].bones[leapIntermediateBoneIndex].NextJoint.ToLeftHandedUnityVector3(),
                            hand.Fingers[leapIndexFingerIndex].bones[leapDistalBoneIndex].Rotation.ToLeftHandedUnityQuaternion());
                        break;
                    case TrackedHandJoint.IndexTip:
                        jointPoses[i] = new MixedRealityPose(
                            hand.Fingers[leapIndexFingerIndex].bones[leapDistalBoneIndex].NextJoint.ToLeftHandedUnityVector3(),
                            hand.Fingers[leapIndexFingerIndex].bones[leapDistalBoneIndex].Rotation.ToLeftHandedUnityQuaternion());
                        break;
                    case TrackedHandJoint.MiddleProximal:
                        jointPoses[i] = new MixedRealityPose(
                            hand.Fingers[leapMiddleFingerIndex].bones[leapMetacarpalBoneIndex].NextJoint.ToLeftHandedUnityVector3(),
                            hand.Fingers[leapMiddleFingerIndex].bones[leapProximalBoneIndex].Rotation.ToLeftHandedUnityQuaternion());
                        break;
                    case TrackedHandJoint.MiddleIntermediate:
                        jointPoses[i] = new MixedRealityPose(
                            hand.Fingers[leapMiddleFingerIndex].bones[leapProximalBoneIndex].NextJoint.ToLeftHandedUnityVector3(),
                            hand.Fingers[leapMiddleFingerIndex].bones[leapIntermediateBoneIndex].Rotation.ToLeftHandedUnityQuaternion());
                        break;
                    case TrackedHandJoint.MiddleDistal:
                        jointPoses[i] = new MixedRealityPose(
                            hand.Fingers[leapMiddleFingerIndex].bones[leapIntermediateBoneIndex].NextJoint.ToLeftHandedUnityVector3(),
                            hand.Fingers[leapMiddleFingerIndex].bones[leapDistalBoneIndex].Rotation.ToLeftHandedUnityQuaternion());
                        break;
                    case TrackedHandJoint.MiddleTip:
                        jointPoses[i] = new MixedRealityPose(
                            hand.Fingers[leapMiddleFingerIndex].bones[leapDistalBoneIndex].NextJoint.ToLeftHandedUnityVector3(),
                            hand.Fingers[leapMiddleFingerIndex].bones[leapDistalBoneIndex].Rotation.ToLeftHandedUnityQuaternion());
                        break;
                    case TrackedHandJoint.RingProximal:
                        jointPoses[i] = new MixedRealityPose(
                            hand.Fingers[leapRingFingerIndex].bones[leapMetacarpalBoneIndex].NextJoint.ToLeftHandedUnityVector3(),
                            hand.Fingers[leapRingFingerIndex].bones[leapProximalBoneIndex].Rotation.ToLeftHandedUnityQuaternion());
                        break;
                    case TrackedHandJoint.RingIntermediate:
                        jointPoses[i] = new MixedRealityPose(
                            hand.Fingers[leapRingFingerIndex].bones[leapProximalBoneIndex].NextJoint.ToLeftHandedUnityVector3(),
                            hand.Fingers[leapRingFingerIndex].bones[leapIntermediateBoneIndex].Rotation.ToLeftHandedUnityQuaternion());
                        break;
                    case TrackedHandJoint.RingDistal:
                        jointPoses[i] = new MixedRealityPose(
                            hand.Fingers[leapRingFingerIndex].bones[leapIntermediateBoneIndex].NextJoint.ToLeftHandedUnityVector3(),
                            hand.Fingers[leapRingFingerIndex].bones[leapDistalBoneIndex].Rotation.ToLeftHandedUnityQuaternion());
                        break;
                    case TrackedHandJoint.RingTip:
                        jointPoses[i] = new MixedRealityPose(
                            hand.Fingers[leapRingFingerIndex].bones[leapDistalBoneIndex].NextJoint.ToLeftHandedUnityVector3(),
                            hand.Fingers[leapRingFingerIndex].bones[leapDistalBoneIndex].Rotation.ToLeftHandedUnityQuaternion());
                        break;
                    case TrackedHandJoint.LittleMetacarpal:
                        jointPoses[i] = new MixedRealityPose(
                            hand.Fingers[leapLittleFingerIndex].bones[leapMetacarpalBoneIndex].PrevJoint.ToLeftHandedUnityVector3(),
                            hand.Fingers[leapLittleFingerIndex].bones[leapProximalBoneIndex].Rotation.ToLeftHandedUnityQuaternion());
                        break;
                    case TrackedHandJoint.LittleProximal:
                        jointPoses[i] = new MixedRealityPose(
                            hand.Fingers[leapLittleFingerIndex].bones[leapMetacarpalBoneIndex].NextJoint.ToLeftHandedUnityVector3(),
                            hand.Fingers[leapLittleFingerIndex].bones[leapProximalBoneIndex].Rotation.ToLeftHandedUnityQuaternion());
                        break;
                    case TrackedHandJoint.LittleIntermediate:
                        jointPoses[i] = new MixedRealityPose(
                            hand.Fingers[leapLittleFingerIndex].bones[leapProximalBoneIndex].NextJoint.ToLeftHandedUnityVector3(),
                            hand.Fingers[leapLittleFingerIndex].bones[leapIntermediateBoneIndex].Rotation.ToLeftHandedUnityQuaternion());
                        break;
                    case TrackedHandJoint.LittleDistal:
                        jointPoses[i] = new MixedRealityPose(
                            hand.Fingers[leapLittleFingerIndex].bones[leapIntermediateBoneIndex].NextJoint.ToLeftHandedUnityVector3(),
                            hand.Fingers[leapLittleFingerIndex].bones[leapDistalBoneIndex].Rotation.ToLeftHandedUnityQuaternion());
                        break;
                    case TrackedHandJoint.LittleTip:
                        jointPoses[i] = new MixedRealityPose(
                            hand.Fingers[leapLittleFingerIndex].bones[leapDistalBoneIndex].NextJoint.ToLeftHandedUnityVector3(),
                            hand.Fingers[leapLittleFingerIndex].bones[leapDistalBoneIndex].Rotation.ToLeftHandedUnityQuaternion());
                        break;
                }
            }

            // Fill missing joints by estimating their pose.
            jointPoses[(int)TrackedHandJoint.RingMetacarpal] = HandUtilities.GetEstimatedRingMetacarpalPose(jointPoses);
            jointPoses[(int)TrackedHandJoint.MiddleMetacarpal] = HandUtilities.GetEstimatedMiddleMetacarpalPose(jointPoses);
            jointPoses[(int)TrackedHandJoint.IndexMetacarpal] = HandUtilities.GetEstimatedIndexMetacarpalPose(jointPoses);

            return jointPoses;
        }

        /// <summary>
        /// Gets a single joint's <see cref="MixedRealityPose"/> relative to the hand root pose.
        /// </summary>
        /// <param name="trackedHandJoint">The <see cref="TrackedHandJoint"/> Id for the joint to get a <see cref="MixedRealityPose"/> for.</param>
        /// <param name="handRootPose">The hand's root <see cref="MixedRealityPose"/>. Joint poses are always relative to the root pose.</param>
        /// <returns>Joint <see cref="MixedRealityPose"/> relative to the hand's root pose.</returns>
        private MixedRealityPose GetJointPose(TrackedHandJoint trackedHandJoint, MixedRealityPose handRootPose, MixedRealityPose jointPose)
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

                jointTransform.localPosition = playspaceTransform.InverseTransformPoint(playspaceTransform.position + playspaceTransform.rotation * jointPose.Position);
                jointTransform.localRotation = Quaternion.Inverse(playspaceTransform.rotation) * playspaceTransform.rotation * jointPose.Rotation;
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
            var rootPosition = playspaceTransform.InverseTransformPoint(playspaceTransform.position + playspaceTransform.rotation * hand.WristPosition.ToLeftHandedUnityVector3());
            var rootRotation = Quaternion.Inverse(playspaceTransform.rotation) * playspaceTransform.rotation * hand.Arm.Basis.rotation.ToLeftHandedUnityQuaternion();

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