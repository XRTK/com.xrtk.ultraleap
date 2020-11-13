// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace XRTK.Ultraleap.Extensions
{
    /// <summary>
    /// Ultraleap platform specific <see cref="Vector3"/> extensions.
    /// </summary>
    public static class VectorExtensions
    {
        private const float millimeterToMeterFactor = 1e-3f;

        /// <summary>
        /// Converts a <see cref="Leap.Vector"/> object to a <see cref="Vector3"/> object. The input
        /// vector is converted to Unity's left-handed coordinate system and coordinates are scaled from millimeters to meters.
        /// </summary>
        /// <param name="v">The <see cref="Leap.Vector"/> to convert.</param>
        /// <returns>A <see cref="Vector3"/>.</returns>
        public static Vector3 ToLeftHandedUnityVector3(this Leap.Vector v) => millimeterToMeterFactor * new Vector3(v.x, v.y, -v.z);
    }
}