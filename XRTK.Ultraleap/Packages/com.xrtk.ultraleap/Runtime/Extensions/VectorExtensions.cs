// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace XRTK.Ultraleap.Extensions
{
    public static class VectorExtensions
    {
        /// <summary>
        /// Converts a <see cref="Leap.Vector"/> object to a <see cref="Vector3"/> object.
        /// </summary>
        /// <param name="v">The <see cref="Leap.Vector"/> to convert.</param>
        /// <returns>A <see cref="Vector3"/>.</returns>
        public static Vector3 ToVector3(this Leap.Vector v)
        {
            return new Vector3(v.x, v.y, -v.z);
        }
    }
}