// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace XRTK.Ultraleap.Extensions
{
    /// <summary>
    /// Ultraleap platform specific <see cref="Quaternion"/> extensions.
    /// </summary>
    public static class QuaternionExtensions
    {
        /// <summary>
        /// Converts a <see cref="Leap.LeapQuaternion"/> object to a <see cref="Quaternion"/> object and transforms
        /// it to Unity's left handed coordinate system.
        /// </summary>
        /// <param name="vector">The <see cref="Leap.LeapQuaternion"/> to convert.</param>
        /// <returns>A <see cref="Quaternion"/> with identical values to the input <see cref="Leap.LeapQuaternion"/>.</returns>
        public static Quaternion ToLeftHandedUnityQuaternion(this Leap.LeapQuaternion q)
        {
            var quaternion = new Quaternion(q.x, q.y, q.z, q.w);
            var euler = quaternion.eulerAngles;
            euler.x = -euler.x;
            euler.y = -euler.y;

            return Quaternion.Euler(euler);
        }

        /// <summary>
        /// Converts a <see cref="LeapInternal.LEAP_QUATERNION"/> object to a <see cref="Quaternion"/> object and transforms
        /// it to Unity's left handed coordinate system.
        /// </summary>
        /// <param name="vector">The <see cref="LeapInternal.LEAP_QUATERNION"/> to convert.</param>
        /// <returns>A <see cref="Quaternion"/> with identical values to the input <see cref="LeapInternal.LEAP_QUATERNION"/>.</returns>
        public static Quaternion ToLeftHandedUnityQuaternion(this LeapInternal.LEAP_QUATERNION q)
        {
            var quaternion = new Quaternion(q.x, q.y, q.z, q.w);
            var euler = quaternion.eulerAngles;
            euler.x = -euler.x;
            euler.y = -euler.y;

            return Quaternion.Euler(euler);
        }
    }
}