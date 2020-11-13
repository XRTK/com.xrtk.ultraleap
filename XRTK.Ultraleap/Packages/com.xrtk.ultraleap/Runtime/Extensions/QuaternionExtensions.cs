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
        /// Converts a <see cref="Leap.LeapQuaternion"/> object to a <see cref="Quaternion"/> object.
        /// </summary>
        /// <param name="vector">The <see cref="Leap.LeapQuaternion"/> to convert.</param>
        /// <returns>A <see cref="Quaternion"/> with identical values to the input <see cref="Leap.LeapQuaternion"/>.</returns>
        public static Quaternion ToQuaternion(this Leap.LeapQuaternion q) => new Quaternion(q.x, q.y, q.z, q.w);

        /// <summary>
        /// Converts a <see cref="LeapInternal.LEAP_QUATERNION"/> object to a <see cref="Quaternion"/> object.
        /// </summary>
        /// <param name="vector">The <see cref="LeapInternal.LEAP_QUATERNION"/> to convert.</param>
        /// <returns>A <see cref="Quaternion"/> with identical values to the input <see cref="LeapInternal.LEAP_QUATERNION"/>.</returns>
        public static Quaternion ToQuaternion(this LeapInternal.LEAP_QUATERNION q) => new Quaternion(q.x, q.y, q.z, q.w);
    }
}