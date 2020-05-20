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
            leftHandConverter = new LeapMotionHandDataConverter(Handedness.Left, TrackedPoses);
            rightHandConverter = new LeapMotionHandDataConverter(Handedness.Right, TrackedPoses);
        }

        private readonly Dictionary<Handedness, int> handIdMap = new Dictionary<Handedness, int>();
        private readonly Dictionary<Handedness, MixedRealityHandController> activeControllers = new Dictionary<Handedness, MixedRealityHandController>();
        private readonly LeapMotionHandDataConverter leftHandConverter;
        private readonly LeapMotionHandDataConverter rightHandConverter;
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

            leapController.Connect += LeapController_Connect;
            leapController.Disconnect += LeapController_Disconnect;
            leapController.Device += LeapController_Device;
            leapController.DeviceLost += LeapController_DeviceLost;
            leapController.DeviceFailure += LeapController_DeviceFailure;
            leapController.FrameReady += LeapController_FrameReady;
        }

        /// <inheritdoc />
        public override void Disable()
        {
            if (leapController.IsConnected)
            {
                leapController.StopConnection();
            }

            leapController.Connect -= LeapController_Connect;
            leapController.Disconnect -= LeapController_Disconnect;
            leapController.Device -= LeapController_Device;
            leapController.DeviceLost -= LeapController_DeviceLost;
            leapController.DeviceFailure -= LeapController_DeviceFailure;
            leapController.FrameReady -= LeapController_FrameReady;

            foreach (var activeController in activeControllers)
            {
                RemoveController(activeController.Key, false);
            }

            activeControllers.Clear();
            handIdMap.Clear();
        }

        #region Leap Controller Event Handlers

        private void LeapController_Device(object sender, Leap.DeviceEventArgs e)
        {
            if (Debug.isDebugBuild)
            {
                Debug.Log($"{typeof(Leap.Controller)} | Device connected | {e.Device.SerialNumber}");
            }
        }

        private void LeapController_DeviceFailure(object sender, Leap.DeviceFailureEventArgs e)
        {
            if (Debug.isDebugBuild)
            {
                Debug.Log($"{typeof(Leap.Controller)} | Device failure | {e.DeviceSerialNumber} | {e.ErrorMessage}");
            }
        }

        private void LeapController_DeviceLost(object sender, Leap.DeviceEventArgs e)
        {
            if (Debug.isDebugBuild)
            {
                Debug.Log($"{typeof(Leap.Controller)} | Device lost | {e.Device.SerialNumber}");
            }
        }

        private void LeapController_Connect(object sender, Leap.ConnectionEventArgs e)
        {
            if (Debug.isDebugBuild)
            {
                Debug.Log($"{typeof(Leap.Controller)} | Service connected");
            }
        }

        private void LeapController_Disconnect(object sender, Leap.ConnectionLostEventArgs e)
        {
            if (Debug.isDebugBuild)
            {
                Debug.Log($"{typeof(Leap.Controller)} | Service disconnected");
            }
        }

        private void LeapController_FrameReady(object sender, Leap.FrameEventArgs e)
        {
            bool isLeftHandTracked = false;
            bool isRightHandTracked = false;

            Leap.Frame frame = e.frame;
            for (int i = 0; i < frame.Hands.Count; i++)
            {
                var hand = frame.Hands[i];

                if (hand.IsLeft && VerifyHandId(Handedness.Left, hand.Id))
                {
                    isLeftHandTracked = true;

                    if (TryGetController(Handedness.Left, out MixedRealityHandController leftHandController))
                    {
                        leftHandController.UpdateController(leftHandConverter.GetHandData(hand));
                    }
                    else
                    {
                        leftHandController = CreateController(Handedness.Left, hand.Id);
                        leftHandController.UpdateController(leftHandConverter.GetHandData(hand));
                    }
                }
                else if (hand.IsRight && VerifyHandId(Handedness.Right, hand.Id))
                {
                    isRightHandTracked = true;

                    if (TryGetController(Handedness.Right, out MixedRealityHandController rightHandController))
                    {
                        rightHandController.UpdateController(rightHandConverter.GetHandData(hand));
                    }
                    else
                    {
                        rightHandController = CreateController(Handedness.Right, hand.Id);
                        rightHandController.UpdateController(rightHandConverter.GetHandData(hand));
                    }
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

        private MixedRealityHandController CreateController(Handedness handedness, int handId)
        {
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
                return handIdMap[handedness] == handId;
            }

            return true;
        }
    }
}