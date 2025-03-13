// SPDX-FileCopyrightText: 2025 Lachryphage (GitHub)
// SPDX-FileCopyrightText: 2025 V <97265903+formlessnameless@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 mqole <113324899+mqole@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._EinsteinEngines.Supermatter.Components;

namespace Content.Client._EinsteinEngines.Supermatter.Consoles;

public sealed class SupermatterConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private SupermatterConsoleWindow? _menu;

    protected override void Open()
    {
        base.Open();

        _menu = new SupermatterConsoleWindow(this, Owner);
        _menu.OpenCentered();
        _menu.OnClose += Close;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_menu == null || state is not SupermatterConsoleBoundInterfaceState msg)
            return;

        _menu?.UpdateUI(msg.Supermatters, msg.FocusData);
    }

    public void SendFocusChangeMessage(NetEntity? netEntity)
    {
        SendMessage(new SupermatterConsoleFocusChangeMessage(netEntity));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;

        _menu?.Parent?.RemoveChild(_menu);
    }
}
