// SPDX-FileCopyrightText: 2025 Lachryphage (GitHub)
// SPDX-FileCopyrightText: 2025 mqole <113324899+mqole@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client._EinsteinEngines.Supermatter.Systems;
using Content.Shared._EinsteinEngines.Supermatter.Components;

namespace Content.Client._EinsteinEngines.Supermatter.Components;

[RegisterComponent]
[Access(typeof(SupermatterVisualizerSystem))]
public sealed partial class SupermatterVisualsComponent : Component
{
    [DataField("crystal", required: true)]
    public Dictionary<SupermatterCrystalState, PrototypeLayerData> CrystalVisuals = default!;
}
