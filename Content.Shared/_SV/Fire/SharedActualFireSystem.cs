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

    public void TryLightFluidFire(EntityUid uid)
    {
        var entity = _entityManager.TryGetComponent<SolutionComponent>(uid, out var solution);

        if (solution == null || solution.Solution.Contents.Count == 0)
            return;

        if(!CheckFlammability(solution))
            return;

        //At this point, it should try to light
    }

    /// <summary>
    /// Update the component on both the server and client for prediction when the fire is examined
    /// </summary>
    /// <param name="uid">UID of the fire</param>
    /// <param name="fire">ActualFireComponent of the fire</param>
    public void UpdateData(EntityUid uid, ActualFireComponent fire)
    {
        if (!_entityManager.TryGetComponent<SolutionComponent>(fire.TargetEntity, out var solution) || solution.Solution.Contents.Count == 0)
            return;

        var oxidizer = 0f;
        var fuel = 0f;
        var totalFluid = 0f;
        var finalTemp = 0f;

        var exhaust = new List<GasSpawnEntry>();

        foreach (var reagent in solution.Solution.Contents)
        {
            if (reagent.Quantity == 0)
                continue;

            var fluid = _prototypeManager.Index<ReagentPrototype>(reagent.Reagent.Prototype);

            if (fluid.FlammableFluid.IsFlammable)
                fuel += reagent.Quantity.Value;

            if (fluid.FlammableFluid.IsOxidizer)
                oxidizer += reagent.Quantity.Value;

            totalFluid += reagent.Quantity.Value;

            //Generate exhause gas list
            exhaust.AddRange(fluid.FlammableFluid.ExhaustedGases);

            finalTemp = (finalTemp + fluid.FlammableFluid.GeneratedHeat) / 2;
        }

        if (fuel == 0)
            return;

        //TODO: Get oxidation from the air, and use that as oxidizer as well
        //We use a baseline of 21.8 mol of oxygen as 1 unit of oxidizer.
        //This does not use all of the oxygen in the tile as oxygen, but it allows us to have a baseline

        //TODO: FIX THIS
        //This is a shit ass way of representing how oxidized the fire is. I need to somehow be able to use the air as an oxidizer, and have it be a ratio like airOxidizer + (oxidizer / fuel)
        fire.Oxidation = oxidizer / fuel;

        fire.FireTemp = finalTemp * fire.Oxidation;

        fire.GasSpawnEntries = exhaust.ToArray();
        Dirty(uid, fire);
    }

    public bool CheckFlammability(SolutionComponent solution)
    {
        foreach (var reagent in solution.Solution.Contents)
        {
            if (reagent.Quantity == 0)
                continue;

            var fluid = _prototypeManager.Index<ReagentPrototype>(reagent.Reagent.Prototype);

            if (fluid.FlammableFluid.IsFlammable)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Gets the oxidation ratio of the current tile.
    /// A tile with 21.8 mol of oxygen will count as 1 "unit" of oxidizer. It will not eat the entire
    /// </summary>
    /// <param name="uid">UID of the fire</param>
    /// <param name="mixture">returns the mixture of oxidizing gases</param>
    /// <returns></returns>
    public float GetOxidation(EntityUid uid, out GasMixture? mixture)
    {
        if (!_atmosphere.TryGetExposedMixture(uid, out var airMixture) || !_atmosphere.IsMixtureOxidizer(airMixture))
        {
            mixture = null;
            return 0f;
        }

        var gasMixList = new List<GasMixEntry>();
        foreach (var gas in _atmosphere.)
        {
            gasMixList.Add(GenerateGasMix);
        }


    }

}
