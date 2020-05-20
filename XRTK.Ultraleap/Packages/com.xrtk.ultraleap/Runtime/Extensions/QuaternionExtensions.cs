// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace XRTK.Ultraleap.Extensions
{
    public static class QuaternionExtensions
    {
        /// <summary>
        /// Converts a <see cref="Leap.LeapQuaternion"/> object to a <see cref="Quaternion"/> object.
        /// </summary>
        /// <param name="vector">The <see cref="Leap.LeapQuaternion"/> to convert.</param>
        /// <returns>A <see cref="Quaternion"/>.</returns>
        public static Quaternion ToQuaternion(this Leap.LeapQuaternion q)
        {
            return new Quaternion(q.x, q.y, q.z, q.w);
        }
    }
}