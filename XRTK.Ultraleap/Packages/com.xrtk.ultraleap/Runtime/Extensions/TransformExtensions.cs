// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using Leap;

namespace XRTK.Ultraleap.Extensions
{
    /// <summary>
    /// Ultraleap platform specific <see cref="Transform"/> extensions.
    /// </summary>
    public static class TransformExtensions
    {
        private const float millimetersToMeters = 1e-3f;

        /// <summary>
        /// Extracts a transform matrix containing translation, rotation, and scale from a Unity Transform object and returns a Leap Motion LeapTransform object.
        /// Use this matrix to transform Leap Motion tracking data to the Unity world relative to the specified transform.
        /// 
        /// In addition to applying the translation, rotation, and scale from the Transform object, the returned
        /// transformation changes the coordinate system from right- to left-handed and converts units from millimeters to meters by scaling.
        /// </summary>
        /// <param name="t">The Unity <see cref="Transform"/> the translation is relative to.</param>
        /// <returns>A <see cref="LeapTransform"/> object representing the specified transform from Leap Motion into Unity space.</returns>
        public static LeapTransform GetLeapMatrix(this Transform t)
        {
            Vector scale = new Vector(t.lossyScale.x * millimetersToMeters, t.lossyScale.y * millimetersToMeters, t.lossyScale.z * millimetersToMeters);
            LeapTransform transform = new LeapTransform(t.position.ToVector(), t.rotation.ToLeapQuaternion(), scale);
            transform.MirrorZ(); // Unity is left handed.
            return transform;
        }
    }
}