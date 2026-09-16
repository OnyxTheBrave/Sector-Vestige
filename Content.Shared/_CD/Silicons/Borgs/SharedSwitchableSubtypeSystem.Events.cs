// SPDX-FileCopyrightText: 2026 Cosmatic Drift contributors
// SPDX-FileCopyrightText: 2026 Sector Vestige contributors (modifications)
// SPDX-FileCopyrightText: 2025 dffdff2423 <dffdff2423@gmail.com>
// SPDX-FileCopyrightText: 2026 lunarcomets (GitHub)
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CD.Silicons.Borgs;

[Serializable, NetSerializable]
public sealed class BorgSelectSubtypeMessage(ProtoId<EntityPrototype>? subtype) : BoundUserInterfaceMessage
{
    public ProtoId<EntityPrototype>? Subtype = subtype;
}

[ByRefEvent]
public record struct AfterBorgTypeSelectEvent;

[ByRefEvent]
public record struct BorgTypeUpdateVisualsOverrideEvent;
