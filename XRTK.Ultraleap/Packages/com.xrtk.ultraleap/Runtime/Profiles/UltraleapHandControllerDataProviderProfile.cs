// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Definitions.Controllers.Hands;
using XRTK.Definitions.Utilities;
using XRTK.Ultraleap.Definitions;

namespace XRTK.Ultraleap.Profiles
{
    /// <summary>
    /// Configuration profile for the <see cref="Providers.Controllers.UltraleapHandControllerDataProvider"/> powering
    /// the <see cref="XRTK.Providers.Controllers.Hands.MixedRealityHandController"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "Mixed Reality Toolkit/Input System/Controller Data Providers/Ultraleap Hand", fileName = "UltraleapHandControllerDataProviderProfile", order = (int)CreateProfileMenuItemIndices.Input)]
    public class UltraleapHandControllerDataProviderProfile : BaseHandControllerDataProviderProfile
    {
        [SerializeField]
        [Tooltip("Specifies how the Ultraleap device is operated.")]
        private UltraleapOperationMode operationMode = UltraleapOperationMode.Desktop;

        /// <summary>
        /// Specifies how the Ultraleap device is operated.
        /// </summary>
        public UltraleapOperationMode OperationMode => operationMode;

        [SerializeField]
        [Tooltip("The maximum number of times the provider will attempt to reconnect to the service before giving up.")]
        private int maxReconnectionAttempts = 5;

        /// <summary>
        /// The maximum number of times the provider will attempt to reconnect to the service before giving up.
        /// </summary>
        public int MaxReconnectionAttempts => maxReconnectionAttempts;

        [SerializeField]
        [Tooltip("The number of frames to wait between each reconnection attempt.")]
        private int reconnectionInterval = 180;

        /// <summary>
        /// The number of frames to wait between each reconnection attempt.
        /// </summary>
        public int ReconnectionInterval => reconnectionInterval;

        [SerializeField]
        [Tooltip("Sets the frame optimization mode for the provider. When enabled, only one leap frame instead of two will be calculated.")]
        private UltraleapFrameOptimizationMode frameOptimizationMode = UltraleapFrameOptimizationMode.None;

        /// <summary>
        /// Gets the frame optimization mode for the provider. When enabled, only one leap frame instead of two will be calculated.
        /// </summary>
        public UltraleapFrameOptimizationMode FrameOptimizationMode => frameOptimizationMode;

        [SerializeField]
        [Tooltip("Applies only to UltraleapOperationMode.Desktop. Adds an offset to the recognized hand root position relative to the main camera, so hands are visible.")]
        private Vector3 leapControllerOffset = new Vector3(0, -0.2f, 0.3f);

        /// <summary>
        /// Applies only to UltraleapOperationMode.Desktop. Adds an offset to the recognized hand root position relative to the main camera, so hands are visible.
        /// </summary>
        public Vector3 LeapControllerOffset => leapControllerOffset;

        [SerializeField]
        [Tooltip("Applies only to UltraleapOperationMode.HeadsetMounted. Allows to specify the physical position and orientation on a mounted device.")]
        private UltraleapDeviceOffsetMode deviceOffsetMode = UltraleapDeviceOffsetMode.Default;

        /// <summary>
        /// Applies only to <see cref="UltraleapOperationMode.HeadsetMounted"/>. Allows to specify the physical position and orientation on a mounted device.
        /// </summary>
        public UltraleapDeviceOffsetMode DeviceOffsetMode => deviceOffsetMode;

        [SerializeField]
        [Range(-0.50F, 0.50F)]
        [Tooltip("Adjusts the Ultraleap device's virtual height offset from the tracked "
           + "headset position. This should match the vertical offset of the physical "
           + "device with respect to the headset in meters.")]
        private float deviceOffsetYAxis = 0f;

        /// <summary>
        /// Adjusts the Ultraleap device's virtual height offset from the tracked headset position.
        /// </summary>
        public float DeviceOffsetYAxis => deviceOffsetYAxis;

        [SerializeField]
        [Range(-0.50F, 0.50F)]
        [Tooltip("Adjusts the Ultraleap device's virtual depth offset from the tracked "
           + "headset position. This should match the forward offset of the physical "
           + "device with respect to the headset in meters.")]
        private float deviceOffsetZAxis = .12f;

        /// <summary>
        /// Adjusts the Ultraleap device's virtual depth offset from the tracked headset position.
        /// </summary>
        public float DeviceOffsetZAxis => deviceOffsetZAxis;

        [SerializeField]
        [Range(-90.0F, 90.0F)]
        [Tooltip("Adjusts the Ultraleap device's virtual X axis tilt. This should match "
           + "the tilt of the physical device with respect to the headset in degrees.")]
        private float deviceTiltXAxis = 5f;

        /// <summary>
        /// Adjusts the Ultraleap device's virtual X axis tilt.
        /// </summary>
        public float DeviceTiltXAxis => deviceTiltXAxis;
    }
}
