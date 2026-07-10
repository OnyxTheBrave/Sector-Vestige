using Content.Server.Atmos.EntitySystems;
using Content.Shared._SV.Fire;
using Content.Shared._SV.Utility;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using static Content.Shared.Atmos.Gas;

namespace Content.Server._SV.Fire;

/// <summary>
/// This handles...
/// </summary>
public sealed partial class ServerActualFireSystem : EntitySystem
{
    [Dependency] private AtmosphereSystem _atmosphere = default!;
    [Dependency] private EntityManager _entityManager = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private GasAnalyzerSystem _analyzerSystem = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;

    private const float EffectiveOxygenOxidation = 21.8f;
    private const float EffectiveFrezonOxidation = 5.3f;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActualFireComponent, ComponentInit>(OnInit);
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ActualFireComponent>();
        while (query.MoveNext(out var entity, out var fireComp))
        {
            if (_timing.CurTime < fireComp.TimeTillNextTick)
                continue;
            fireComp.TimeTillNextTick += TimeSpan.FromSeconds(fireComp.TimeBetweenFireTick);

            var tileMixture = _atmosphere.GetTileMixture(entity);

            //if there is an atmosphere, and the temperature of the air is less than the maximum heat of the fire; add heat to it.
            if (tileMixture != null && tileMixture.Temperature < fireComp.MaxFireTemp)
                _atmosphere.AddHeat(tileMixture, fireComp.GenratedHeat);

            foreach (var gas in fireComp.GasSpawnEntries)
            {
                _atmosphere.AdjustTileMixture(entity, gas.Gas, gas.Amount.Next(_random));
            }
        }
    }

    private void OnInit(EntityUid uid, ActualFireComponent component, ComponentInit args)
    {
        UpdateData(uid, component);
        Dirty(uid, component);
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
        var totalFluid = solution.Solution.Contents.Count;
        var generatedHeat = 0f;
        var maxFireHeat = 0f;

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

            //Generate exhause gas list
            exhaust.AddRange(fluid.FlammableFluid.ExhaustedGases);

            generatedHeat = (fluid.FlammableFluid.GeneratedHeat * reagent.Quantity.Value);
            maxFireHeat = (fluid.FlammableFluid.MaxHeat * reagent.Quantity.Value);
        }

        if (fuel == 0)
            return;

        //TODO: FIX THIS
        //This is a shit ass way of representing how oxidized the fire is. I need to somehow be able to use the air as an oxidizer, and have it be a ratio like airOxidizer + (oxidizer / fuel)
        fire.Oxidation = GetOxidation(uid) + oxidizer / fuel / totalFluid;

        //Weird way of calculating this, but this averages out how much heat there is from the fluid that is being burnt, then modify it based on the oxidation (clamped to stop it from getting too stupid)
        fire.GenratedHeat =  (generatedHeat / totalFluid) * Math.Clamp(fire.Oxidation, 0f, 25f);
        fire.MaxFireTemp = (maxFireHeat  / totalFluid) * Math.Clamp(fire.Oxidation, 0f, 25f);

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
    /// <returns></returns>
    public float GetOxidation(EntityUid uid)
    {
        var tileMixture = _atmosphere.GetTileMixture(uid);

        var oxidizer = 0f;

        if (tileMixture == null || tileMixture.TotalMoles == 0 || !_atmosphere.IsMixtureOxidizer(tileMixture))
            return oxidizer;

        var gasMixEntry =(_analyzerSystem.GenerateGasMixEntry(uid.ToString(), tileMixture));

        //for each oxidizing gas that exists, get the amount that exists in the tile, and then divide it by its EffectiveOxidation coefficient to get how effective the air is at oxidizing.
        //Yes this is overkill for the fact that we only have oxygen as an oxidizing gas, but one can dream.
        if (gasMixEntry.Gases != null)
        {
            foreach (var gas in gasMixEntry.Gases)
            {
                switch (gas.Gas)
                {
                    case Oxygen:
                        oxidizer += gas.Amount / EffectiveOxygenOxidation;
                        break;
                    case Frezon:
                        oxidizer += gas.Amount /  EffectiveFrezonOxidation;
                        break;
                }
            }
        }
        return oxidizer;
    }


}
