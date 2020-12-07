// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using Leap;

namespace XRTK.Ultraleap.Extensions
{
    /// <summary>
    /// Ultraleap platform specific <see cref="Vector3"/> extensions.
    /// </summary>
    public static class VectorExtensions
    {
        /// <summary>
        /// Converts a Leap <see cref="Vector"/> object to a UnityEngine <see cref="Vector3"/> object.
        /// Does not convert to the Unity left-handed coordinate system or scale the coordinates from millimeters to meters.
        /// </summary>
        /// <param name="vector">The <see cref="Vector"/> to convert.</param>
        /// <returns>The <see cref="Vector3"/> object with the same coordinate values as the <see cref="Vector"/>.</returns>
        public static Vector3 ToVector3(this Vector vector) => new Vector3(vector.x, vector.y, vector.z);

        /// <summary>
        /// Converts a UnityEngine <see cref="Vector3"/> object to a Leap <see cref="Vector"/> object.
        /// </summary>
        /// <param name="vector">The <see cref="Vector3"/> to convert.</param>
        /// <returns></returns>
        public static Vector ToVector(this Vector3 vector) => new Vector(vector.x, vector.y, vector.z);
    }
}