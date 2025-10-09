// SPDX-FileCopyrightText: 2025 qu4drivium <aaronholiver@outlook.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared._Moffstation.ReadyManifest;

namespace Content.Client._Moffstation.ReadyManifest;

public sealed class ReadyManifestSystem : EntitySystem
{
    public void RequestReadyManifest()
    {
        RaiseNetworkEvent(new RequestReadyManifestMessage());
    }
}
