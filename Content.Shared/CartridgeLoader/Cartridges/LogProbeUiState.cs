// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 qu4drivium <aaronholiver@outlook.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared._CD.CartridgeLoader.Cartridges; // CD
using Robust.Shared.Serialization;

namespace Content.Shared.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class LogProbeUiState : BoundUserInterfaceState
{
    /// <summary>
    /// The name of the scanned entity.
    /// </summary>
    public string EntityName;

    /// <summary>
    /// The list of probed network devices
    /// </summary>
    public List<PulledAccessLog> PulledLogs;

    /// <summary>
    /// CD: The NanoChat data if a card was scanned, null otherwise
    /// </summary>
    public NanoChatData? NanoChatData { get; }

    public LogProbeUiState(string entityName, List<PulledAccessLog> pulledLogs, NanoChatData? nanoChatData = null) // CD - NanoChat support
    {
        EntityName = entityName;
        PulledLogs = pulledLogs;
        NanoChatData = nanoChatData; // CD
    }
}

[Serializable, NetSerializable, DataRecord]
public sealed partial class PulledAccessLog
{
    public readonly TimeSpan Time;
    public readonly string Accessor;

    public PulledAccessLog(TimeSpan time, string accessor)
    {
        Time = time;
        Accessor = accessor;
    }
}
