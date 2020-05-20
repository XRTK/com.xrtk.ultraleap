// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using XRTK.Definitions.Controllers.Hands;
using XRTK.Definitions.Utilities;
using XRTK.Providers.Controllers.Hands;
using XRTK.Ultraleap.Extensions;

namespace XRTK.Ultraleap.Utilities
{
    /// <summary>
    /// Converts leap motion hand data to <see cref="HandData"/>.
    /// </summary>
    public class LeapMotionHandDataConverter : BaseHandDataConverter
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="handedness">Handedness of the hand this converter is created for.</param>
        /// <param name="trackedPoses">The tracked poses collection to use for pose recognition.</param>
        public LeapMotionHandDataConverter(Handedness handedness, IReadOnlyList<HandControllerPoseDefinition> trackedPoses) : base(handedness, trackedPoses)
        { }

        private const int THUMB_INDEX = 0;
        private const int INDEX_FINGER_INDEX = 1;
        private const int MIDDLE_FINGER_INDEX = 2;
        private const int RING_FINGER_INDEX = 3;
        private const int PINKY_FINGER_INDEX = 4;

        private Leap.Hand currentHand = null;

        /// <summary>
        /// Gets or sets whether hand mesh data should be read and converted.
        /// </summary>
        public static bool HandMeshingEnabled { get; set; }

        /// <summary>
        /// Reads hand data for the current frame and converts it to agnostic hand data.
        /// </summary>
        /// <returns>Updated hand data.</returns>
        public HandData GetHandData(Leap.Hand hand)
        {
            currentHand = hand;

            HandData updatedHandData = new HandData
            {
                IsTracked = true,
                TimeStamp = DateTimeOffset.UtcNow.Ticks
            };

            if (updatedHandData.IsTracked)
            {
                UpdateHandJoints(updatedHandData.Joints);

                if (HandMeshingEnabled && TryGetUpdatedHandMeshData(out HandMeshData data))
                {
                    updatedHandData.Mesh = data;
                }
                else
                {
                    updatedHandData.Mesh = new HandMeshData();
                }
            }

            PostProcess(updatedHandData);
            return updatedHandData;
        }

        private void UpdateHandJoints(MixedRealityPose[] jointPoses)
        {
            for (int i = 0; i < jointPoses.Length; i++)
            {
                TrackedHandJoint trackedHandJoint = (TrackedHandJoint)i;
                switch (trackedHandJoint)
                {
                    // Wrist and Palm
                    case TrackedHandJoint.Wrist:
                        jointPoses[i] = ComputeWristPose(currentHand.Arm.WristPosition);
                        break;
                    case TrackedHandJoint.Palm:
                        jointPoses[i] = ComputePalmPose(currentHand.PalmPosition, currentHand.PalmNormal);
                        break;
                    // Finger: Thumb
                    case TrackedHandJoint.ThumbMetacarpalJoint:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[THUMB_INDEX].Bone(Leap.Bone.BoneType.TYPE_METACARPAL));
                        break;
                    case TrackedHandJoint.ThumbProximalJoint:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[THUMB_INDEX].Bone(Leap.Bone.BoneType.TYPE_PROXIMAL));
                        break;
                    case TrackedHandJoint.ThumbDistalJoint:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[THUMB_INDEX].Bone(Leap.Bone.BoneType.TYPE_DISTAL));
                        break;
                    case TrackedHandJoint.ThumbTip:
                        jointPoses[i] = ComputeTipPose(currentHand.Fingers[THUMB_INDEX].Bone(Leap.Bone.BoneType.TYPE_DISTAL));
                        break;
                    // Finger: Index
                    case TrackedHandJoint.IndexMetacarpal:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[INDEX_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_METACARPAL));
                        break;
                    case TrackedHandJoint.IndexKnuckle:
                        jointPoses[i] = ComputeKnucklePose(currentHand.Fingers[INDEX_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_METACARPAL));
                        break;
                    case TrackedHandJoint.IndexMiddleJoint:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[INDEX_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_INTERMEDIATE));
                        break;
                    case TrackedHandJoint.IndexDistalJoint:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[INDEX_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_DISTAL));
                        break;
                    case TrackedHandJoint.IndexTip:
                        jointPoses[i] = ComputeTipPose(currentHand.Fingers[INDEX_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_DISTAL));
                        break;
                    // Finger: Middle
                    case TrackedHandJoint.MiddleMetacarpal:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[MIDDLE_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_METACARPAL));
                        break;
                    case TrackedHandJoint.MiddleKnuckle:
                        jointPoses[i] = ComputeKnucklePose(currentHand.Fingers[MIDDLE_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_METACARPAL));
                        break;
                    case TrackedHandJoint.MiddleMiddleJoint:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[MIDDLE_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_INTERMEDIATE));
                        break;
                    case TrackedHandJoint.MiddleDistalJoint:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[MIDDLE_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_DISTAL));
                        break;
                    case TrackedHandJoint.MiddleTip:
                        jointPoses[i] = ComputeTipPose(currentHand.Fingers[MIDDLE_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_DISTAL));
                        break;
                    // Finger: Ring
                    case TrackedHandJoint.RingMetacarpal:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[RING_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_METACARPAL));
                        break;
                    case TrackedHandJoint.RingKnuckle:
                        jointPoses[i] = ComputeKnucklePose(currentHand.Fingers[RING_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_METACARPAL));
                        break;
                    case TrackedHandJoint.RingMiddleJoint:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[RING_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_INTERMEDIATE));
                        break;
                    case TrackedHandJoint.RingDistalJoint:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[RING_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_DISTAL));
                        break;
                    case TrackedHandJoint.RingTip:
                        jointPoses[i] = ComputeTipPose(currentHand.Fingers[RING_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_DISTAL));
                        break;
                    // Finger: Pinky
                    case TrackedHandJoint.PinkyMetacarpal:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[PINKY_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_METACARPAL));
                        break;
                    case TrackedHandJoint.PinkyKnuckle:
                        jointPoses[i] = ComputeKnucklePose(currentHand.Fingers[PINKY_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_METACARPAL));
                        break;
                    case TrackedHandJoint.PinkyMiddleJoint:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[PINKY_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_INTERMEDIATE));
                        break;
                    case TrackedHandJoint.PinkyDistalJoint:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[PINKY_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_DISTAL));
                        break;
                    case TrackedHandJoint.PinkyTip:
                        jointPoses[i] = ComputeTipPose(currentHand.Fingers[PINKY_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_DISTAL));
                        break;
                }
            }
        }

        private MixedRealityPose ComputeWristPose(Leap.Vector wristPosition)
        {
            return new MixedRealityPose(wristPosition.ToVector3());
        }

        private MixedRealityPose ComputePalmPose(Leap.Vector palmPosition, Leap.Vector palmNormal)
        {
            var position = palmPosition.ToVector3();
            var rotation = Quaternion.LookRotation(Vector3.forward, palmNormal.ToVector3());

            return new MixedRealityPose(position, rotation);
        }

        private MixedRealityPose ComputeJointPose(Leap.Bone bone)
        {
            var position = bone.PrevJoint.ToVector3();
            var rotation = bone.Rotation.ToQuaternion();

            return new MixedRealityPose(position, rotation);
        }

        private MixedRealityPose ComputeKnucklePose(Leap.Bone bone)
        {
            var position = bone.PrevJoint.ToVector3();
            var rotation = bone.Rotation.ToQuaternion();

            return new MixedRealityPose(position, rotation);
        }

        private MixedRealityPose ComputeTipPose(Leap.Bone distalBone)
        {
            var position = distalBone.NextJoint.ToVector3();
            var rotation = distalBone.Rotation.ToQuaternion();

            return new MixedRealityPose(position, rotation);
        }

        private bool TryGetUpdatedHandMeshData(out HandMeshData data)
        {
            // TODO: Check if Leap motion actually supports hand meshing.
            throw new NotImplementedException();
        }
    }
}