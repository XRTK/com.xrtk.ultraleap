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
        [Tooltip("Adds an offset to the game object with LeapServiceProvider attached.  This offset is only applied if the leapControllerOrientation" +
        "is LeapControllerOrientation.Desk and is necessary for the hand to appear in front of the main camera. If the leap controller is on the " +
        "desk, the LeapServiceProvider is added to the scene instead of the LeapXRServiceProvider. The anchor point for the hands is the position of the" +
        "game object with the LeapServiceProvider attached.")]
        private Vector3 leapControllerOffset = new Vector3(0, -0.2f, 0.2f);

        /// <summary>
        /// Adds an offset to the game object with LeapServiceProvider attached.  This offset is only applied if the leapControllerOrientation
        /// is LeapControllerOrientation.Desk and is necessary for the hand to appear in front of the main camera. If the leap controller is on the 
        /// desk, the LeapServiceProvider is added to the scene instead of the LeapXRServiceProvider. The anchor point for the hands is the position of the 
        /// game object with the LeapServiceProvider attached.
        /// </summary>
        public Vector3 LeapControllerOffset => leapControllerOffset;
    }
}
