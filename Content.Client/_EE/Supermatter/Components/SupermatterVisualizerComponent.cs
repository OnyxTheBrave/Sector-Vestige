// SPDX-FileCopyrightText: 2025 Lachryphage (GitHub)
// SPDX-FileCopyrightText: 2025 hivehum <ketchupfaced@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client._EE.Supermatter.Systems;
using Content.Shared._EE.Supermatter.Components;

namespace Content.Client._EE.Supermatter.Components;

[RegisterComponent]
[Access(typeof(SupermatterVisualizerSystem))]
public sealed partial class SupermatterVisualsComponent : Component
{
    [DataField("crystal", required: true)]
    public Dictionary<SupermatterCrystalState, PrototypeLayerData> CrystalVisuals = default!;
}
