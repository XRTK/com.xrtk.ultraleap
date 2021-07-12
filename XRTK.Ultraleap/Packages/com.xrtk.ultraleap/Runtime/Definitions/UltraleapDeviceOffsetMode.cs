// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace XRTK.Ultraleap.Definitions
{
    /// <summary>
    /// The device offset mode is only applicable for the <see cref="UltraleapOperationMode.HeadsetMounted"/>.
    /// It's used to match the physical position and orientation of the ultraleap sensor on a tracked device it
    /// is mounted on, such as a VR headset.
    /// </summary>
    public enum UltraleapDeviceOffsetMode
    {
        /// <summary>
        /// Uses default offset values.
        /// </summary>
        Default = 0,
        /// <summary>
        /// Allow manual adjustment of the ultraleap device's virtual offset and tilt.
        /// </summary>
        Manual
    }
}