// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Leap;
using UnityEngine;

namespace XRTK.Ultraleap.Extensions
{
    /// <summary>
    /// Ultraleap platform specific <see cref="Quaternion"/> extensions.
    /// </summary>
    public static class QuaternionExtensions
    {
        /// <summary>
        /// Converts a <see cref="LeapQuaternion"/> object to a UnityEngine <see cref="Quaternion"/> object.
        /// </summary>
        /// <param name="q">The <see cref="LeapQuaternion"/> to convert.</param>
        /// <returns></returns>
        public static Quaternion ToQuaternion(this LeapQuaternion q) => new Quaternion(q.x, q.y, q.z, q.w);

        /// <summary>
        /// Converts a <see cref="Quaternion"/> object to a <see cref="LeapQuaternion"/> object.
        /// </summary>
        /// <param name="q">The <see cref="Quaternion"/> to convert.</param>
        /// <returns></returns>
        public static LeapQuaternion ToLeapQuaternion(this Quaternion q) => new LeapQuaternion(q.x, q.y, q.z, q.w);
    }
}