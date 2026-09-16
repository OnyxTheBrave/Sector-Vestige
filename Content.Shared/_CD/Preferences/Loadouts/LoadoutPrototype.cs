// SPDX-FileCopyrightText: 2026 Cosmatic Drift contributors
// SPDX-FileCopyrightText: 2026 Sector Vestige contributors (modifications)
// SPDX-FileCopyrightText: 2026 lunarcomets <140772713+lunarcomets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 lunarcomets <lunarcomets2@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Silicons.Laws;
using Robust.Shared.Prototypes;

// ReSharper disable once CheckNamespace
namespace Content.Shared.Preferences.Loadouts;

public sealed partial class LoadoutPrototype
{
    [DataField]
    public EntProtoId? Brain { get; set; } = new();

    [DataField]
    public ProtoId<SiliconLawsetPrototype>? Lawset { get; set; } = new(); // SV
}
