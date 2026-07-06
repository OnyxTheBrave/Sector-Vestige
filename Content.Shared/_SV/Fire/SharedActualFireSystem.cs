using Content.Shared._SV.Utility;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Atmos.EntitySystems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Examine;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Shared._SV.Fire;

/// <summary>
/// This handles...
/// </summary>
public sealed partial class SharedActualFireSystem : EntitySystem
{

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActualFireComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(EntityUid uid, ActualFireComponent component, ref ExaminedEvent args)
    {
        throw new NotImplementedException();
    }
}
