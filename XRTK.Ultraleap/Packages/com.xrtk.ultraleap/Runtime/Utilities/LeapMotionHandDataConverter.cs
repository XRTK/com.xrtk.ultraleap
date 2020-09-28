// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;
using XRTK.Definitions.Controllers.Hands;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;
using XRTK.Ultraleap.Extensions;

namespace XRTK.Ultraleap.Utilities
{
    /// <summary>
    /// Converts leap motion hand data to <see cref="HandData"/>.
    /// </summary>
    public sealed class LeapMotionHandDataConverter
    {
        private const int THUMB_INDEX = 0;
        private const int INDEX_FINGER_INDEX = 1;
        private const int MIDDLE_FINGER_INDEX = 2;
        private const int RING_FINGER_INDEX = 3;
        private const int PINKY_FINGER_INDEX = 4;

        private Leap.Hand currentHand = null;

        /// <summary>
        /// Reads hand APIs for the current frame and converts it to agnostic <see cref="HandData"/>.
        /// </summary>
        /// <param name="handedness">The handedness of the hand to get <see cref="HandData"/> for.</param>
        /// <param name="includeMeshData">If set, hand mesh information will be included in <see cref="HandData.Mesh"/>.</param>
        /// <param name="handData">The output <see cref="HandData"/>.</param>
        /// <returns>True, if data conversion was a success.</returns>
        public bool TryGetHandData(Leap.Hand hand, bool includeMeshData, out HandData handData)
        {
            currentHand = hand;

            handData = new HandData();

            if (handData.TrackingState == TrackingState.Tracked)
            {
                UpdateHandJoints(handData.Joints);

                if (includeMeshData && TryGetUpdatedHandMeshData(out HandMeshData data))
                {
                    handData.Mesh = data;
                }
                else
                {
                    handData.Mesh = new HandMeshData();
                }
            }

            return true;
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
                    case TrackedHandJoint.ThumbMetacarpal:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[THUMB_INDEX].Bone(Leap.Bone.BoneType.TYPE_METACARPAL));
                        break;
                    case TrackedHandJoint.ThumbProximal:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[THUMB_INDEX].Bone(Leap.Bone.BoneType.TYPE_PROXIMAL));
                        break;
                    case TrackedHandJoint.ThumbDistal:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[THUMB_INDEX].Bone(Leap.Bone.BoneType.TYPE_DISTAL));
                        break;
                    case TrackedHandJoint.ThumbTip:
                        jointPoses[i] = ComputeTipPose(currentHand.Fingers[THUMB_INDEX].Bone(Leap.Bone.BoneType.TYPE_DISTAL));
                        break;
                    // Finger: Index
                    case TrackedHandJoint.IndexMetacarpal:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[INDEX_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_METACARPAL));
                        break;
                    case TrackedHandJoint.IndexProximal:
                        jointPoses[i] = ComputeKnucklePose(currentHand.Fingers[INDEX_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_METACARPAL));
                        break;
                    case TrackedHandJoint.IndexIntermediate:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[INDEX_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_INTERMEDIATE));
                        break;
                    case TrackedHandJoint.IndexDistal:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[INDEX_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_DISTAL));
                        break;
                    case TrackedHandJoint.IndexTip:
                        jointPoses[i] = ComputeTipPose(currentHand.Fingers[INDEX_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_DISTAL));
                        break;
                    // Finger: Middle
                    case TrackedHandJoint.MiddleMetacarpal:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[MIDDLE_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_METACARPAL));
                        break;
                    case TrackedHandJoint.MiddleProximal:
                        jointPoses[i] = ComputeKnucklePose(currentHand.Fingers[MIDDLE_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_METACARPAL));
                        break;
                    case TrackedHandJoint.MiddleIntermediate:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[MIDDLE_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_INTERMEDIATE));
                        break;
                    case TrackedHandJoint.MiddleDistal:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[MIDDLE_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_DISTAL));
                        break;
                    case TrackedHandJoint.MiddleTip:
                        jointPoses[i] = ComputeTipPose(currentHand.Fingers[MIDDLE_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_DISTAL));
                        break;
                    // Finger: Ring
                    case TrackedHandJoint.RingMetacarpal:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[RING_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_METACARPAL));
                        break;
                    case TrackedHandJoint.RingProximal:
                        jointPoses[i] = ComputeKnucklePose(currentHand.Fingers[RING_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_METACARPAL));
                        break;
                    case TrackedHandJoint.RingIntermediate:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[RING_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_INTERMEDIATE));
                        break;
                    case TrackedHandJoint.RingDistal:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[RING_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_DISTAL));
                        break;
                    case TrackedHandJoint.RingTip:
                        jointPoses[i] = ComputeTipPose(currentHand.Fingers[RING_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_DISTAL));
                        break;
                    // Finger: Pinky
                    case TrackedHandJoint.LittleMetacarpal:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[PINKY_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_METACARPAL));
                        break;
                    case TrackedHandJoint.LittleProximal:
                        jointPoses[i] = ComputeKnucklePose(currentHand.Fingers[PINKY_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_METACARPAL));
                        break;
                    case TrackedHandJoint.LittleIntermediate:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[PINKY_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_INTERMEDIATE));
                        break;
                    case TrackedHandJoint.LittleDistal:
                        jointPoses[i] = ComputeJointPose(currentHand.Fingers[PINKY_FINGER_INDEX].Bone(Leap.Bone.BoneType.TYPE_DISTAL));
                        break;
                    case TrackedHandJoint.LittleTip:
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