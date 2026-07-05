using Content.Server.Atmos.EntitySystems;
using Content.Shared._SV.Fire;
using Content.Shared._SV.Utility;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;
using static Content.Shared.Atmos.Gas;

namespace Content.Server._SV.Fire;

/// <summary>
/// This handles...
/// </summary>
public sealed class ServerActualFireSystem : EntitySystem
{
    [Dependency] private AtmosphereSystem _atmosphere = default!;
    [Dependency] private EntityManager _entityManager = default!;
    [Dependency] private PrototypeManager _prototypeManager = default!;
    [Dependency] private GasAnalyzerSystem _analyzerSystem = default!;

    /// <summary>
    /// Minimum moles of a gas to be sent to the client.
    /// </summary>
    private const float UIMinMoles = 0.01f;

    private const float EffectiveOxygenOxidation = 21.8f;

    /// <inheritdoc/>
    public override void Initialize()
    {

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

            //Generate exhause gas list
            exhaust.AddRange(fluid.FlammableFluid.ExhaustedGases);

            finalTemp = (finalTemp + fluid.FlammableFluid.GeneratedHeat) / 2;
        }

        if (fuel == 0)
            return;

        //TODO: FIX THIS
        //This is a shit ass way of representing how oxidized the fire is. I need to somehow be able to use the air as an oxidizer, and have it be a ratio like airOxidizer + (oxidizer / fuel)
        fire.Oxidation = GetOxidation(uid) + oxidizer / fuel;

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
                }
            }
        }
        return oxidizer;
    }


}
