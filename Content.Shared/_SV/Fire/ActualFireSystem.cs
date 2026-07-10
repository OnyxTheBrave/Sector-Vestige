using Content.Shared._SV.Utility;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;

namespace Content.Shared._SV.Fire;

/// <summary>
/// This handles...
/// </summary>
public sealed partial class ActualFireSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private IEntityManager _entityManager = default!;
    [Dependency] private SharedMapSystem _mapSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {

    }

    public override void Update(float frameTime)
    {

    }

}
