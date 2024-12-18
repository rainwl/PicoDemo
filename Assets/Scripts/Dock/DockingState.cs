﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Dock
{
    /// <summary>
    /// The possible states of a <see cref="Dockable"/> object.
    /// </summary>
    public enum DockingState
    {
        Undocked = 0,
        Docking,
        Docked,
        Undocking
    }
}