// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;
using XRTK.Ultraleap.Profiles;
using XRTK.Ultraleap.Utilities;
using XRTK.Providers.Controllers.Hands;
using XRTK.Services;
using XRTK.Ultraleap.Definitions;
using XRTK.Interfaces.InputSystem;
using System;
using XRTK.Definitions.Platforms;
using XRTK.Attributes;

namespace XRTK.Ultraleap.Providers.Controllers
{
    [RuntimePlatform(typeof(UltraleapPlatform))]
    [System.Runtime.InteropServices.Guid("61cec407-ffa4-4a5c-b96a-5229348f85c2")]
    public class LeapMotionHandControllerDataProvider : BaseHandControllerDataProvider
    {
        /// <inheritdoc />
        public LeapMotionHandControllerDataProvider(string name, uint priority, LeapMotionHandControllerDataProviderProfile profile, IMixedRealityInputSystem parentService)
            : base(name, priority, profile, parentService)
        {
            OperationMode = profile.OperationMode;
            LeapControllerOffset = profile.LeapControllerOffset;
            handDataProvider = new LeapMotionHandDataConverter();

            postProcessor = new HandDataPostProcessor(TrackedPoses)
            {
                PlatformProvidesPointerPose = true
            };
        }

        private readonly LeapMotionHandDataConverter handDataProvider;
        private readonly HandDataPostProcessor postProcessor;
        private readonly Dictionary<Handedness, int> handIdMap = new Dictionary<Handedness, int>();
        private readonly Dictionary<Handedness, MixedRealityHandController> activeControllers = new Dictionary<Handedness, MixedRealityHandController>();
        private readonly Leap.Controller leapController = new Leap.Controller();

        /// <summary>
        /// Gets the leap motion controller's current operation mode.
        /// </summary>
        private LeapMotionOperationMode OperationMode { get; }

        /// <summary>
        /// Offset applied to the rendered hands when in <see cref="LeapMotionOperationMode.Desk"/> mode.
        /// </summary>
        private Vector3 LeapControllerOffset { get; }

        /// <inheritdoc />
        public override void Enable()
        {
            base.Enable();

            if (!leapController.IsConnected)
            {
                leapController.StartConnection();
            }

            leapController.FrameReady += LeapController_FrameReady;
        }

        /// <inheritdoc />
        public override void Disable()
        {
            if (leapController.IsConnected)
            {
                leapController.StopConnection();
            }

            leapController.FrameReady -= LeapController_FrameReady;

            foreach (var activeController in activeControllers)
            {
                RemoveController(activeController.Key, false);
            }

            activeControllers.Clear();
            handIdMap.Clear();
        }

        #region Leap Controller Event Handlers

        private void LeapController_FrameReady(object sender, Leap.FrameEventArgs e)
        {
            bool isLeftHandTracked = false;
            bool isRightHandTracked = false;

            Leap.Frame frame = e.frame;
            for (int i = 0; i < frame.Hands.Count; i++)
            {
                var hand = frame.Hands[i];

                if (hand.IsLeft && VerifyHandId(Handedness.Left, hand.Id) &&
                    handDataProvider.TryGetHandData(hand, out var leftHandData))
                {
                    isLeftHandTracked = true;


                    var controller = GetOrAddController(Handedness.Left, hand.Id);
                    leftHandData = postProcessor.PostProcess(Handedness.Left, leftHandData);
                    controller?.UpdateController(leftHandData);
                }
                else if (hand.IsRight && VerifyHandId(Handedness.Right, hand.Id) &&
                    handDataProvider.TryGetHandData(hand, out var rightHandData))
                {
                    isRightHandTracked = true;
                    var controller = GetOrAddController(Handedness.Right, hand.Id);
                    rightHandData = postProcessor.PostProcess(Handedness.Right, rightHandData);
                    controller?.UpdateController(rightHandData);
                }
            }

            if (!isLeftHandTracked)
            {
                RemoveController(Handedness.Left);
            }

            if (!isRightHandTracked)
            {
                RemoveController(Handedness.Right);
            }
        }

        #endregion Leap Controller Event Handlers

        private bool TryGetController(Handedness handedness, out MixedRealityHandController controller)
        {
            if (activeControllers.ContainsKey(handedness))
            {
                var existingController = activeControllers[handedness];
                Debug.Assert(existingController != null, $"Hand Controller {handedness} has been destroyed but remains in the active controller registry.");
                controller = existingController;
                return true;
            }

            controller = null;
            return false;
        }

        private MixedRealityHandController GetOrAddController(Handedness handedness, int handId)
        {
            // If a device is already registered with the handedness, just return it.
            if (TryGetController(handedness, out var existingController))
            {
                return existingController;
            }

            MixedRealityHandController detectedController;
            try
            {
                detectedController = new MixedRealityHandController(this, TrackingState.Tracked, handedness, GetControllerMappingProfile(typeof(MixedRealityHandController), handedness));
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create {nameof(MixedRealityHandController)}!\n{e}");
                return null;
            }

            detectedController.TryRenderControllerModel();
            AddController(detectedController);
            activeControllers.Add(handedness, detectedController);
            handIdMap.Add(handedness, handId);
            MixedRealityToolkit.InputSystem?.RaiseSourceDetected(detectedController.InputSource, detectedController);

            return detectedController;
        }

        private void RemoveController(Handedness handedness, bool removeFromRegistry = true)
        {
            if (TryGetController(handedness, out var controller))
            {
                MixedRealityToolkit.InputSystem?.RaiseSourceLost(controller.InputSource, controller);

                if (removeFromRegistry)
                {
                    RemoveController(controller);
                    activeControllers.Remove(handedness);
                    handIdMap.Remove(handedness);
                }
            }
        }

        private bool VerifyHandId(Handedness handedness, int handId)
        {
            if (handIdMap.ContainsKey(handedness))
            {
                // A hand ID for the provided ID is already
                // registered. We can only update the controller
                // for the handedness if the IDs match.
                return handIdMap[handedness] == handId;
            }

            // A hand ID for the provided handedness is currently
            // not registered at all. That means it's safe to create
            // a new controller for the provided ID.
            return true;
        }
    }
}