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
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private IEntityManager _entityManager = default!;
    [Dependency] private SharedMapSystem _mapSystem = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedAtmosphereSystem _atmosphere = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActualFireComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<ActualFireComponent, EntityPausedEvent>(OnPause);
        SubscribeLocalEvent<ActualFireComponent, EntityUnpausedEvent>(OnUnpause);
        SubscribeLocalEvent<ActualFireComponent, ComponentInit>(OnInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
    }

    private void OnExamine(EntityUid uid, ActualFireComponent component, ref ExaminedEvent args)
    {
        throw new NotImplementedException();
    }

    private void OnInit(EntityUid uid, ActualFireComponent component, ref ComponentInit args)
    {
        throw new NotImplementedException();
    }

    private void OnUnpause(EntityUid uid, ActualFireComponent component, ref EntityUnpausedEvent args)
    {
        throw new NotImplementedException();
    }

    private void OnPause(EntityUid uid, ActualFireComponent component, ref EntityPausedEvent args)
    {
        throw new NotImplementedException();
    }



}
