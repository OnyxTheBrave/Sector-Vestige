// SPDX-FileCopyrightText: 2025 Lachryphage (GitHub)
// SPDX-FileCopyrightText: 2025 V <97265903+formlessnameless@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._EinsteinEngines.Supermatter.Monitor;

[Serializable, NetSerializable]
public enum SupermatterStatusType : sbyte
{
    Error = -1,
    Inactive = 0,
    Normal = 1,
    Caution = 2,
    Warning = 3,
    Danger = 4,
    Emergency = 5,
    Delaminating = 6
}
