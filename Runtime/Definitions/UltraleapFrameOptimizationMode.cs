// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace XRTK.Ultraleap.Definitions
{
    /// <summary>
    /// When enabled, the provider will only calculate one leap frame instead of two.
    /// </summary>
    public enum UltraleapFrameOptimizationMode
    {
        None,
        ReuseUpdateForPhysics,
        ReusePhysicsForUpdate,
    }
}