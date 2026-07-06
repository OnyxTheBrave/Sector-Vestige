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

    public void TryLightFluidFire(EntityUid uid)
    {
        var entity = _entityManager.TryGetComponent<SolutionComponent>(uid, out var solution);

        if (solution == null || solution.Solution.Contents.Count == 0)
            return;

        if(!CheckFlammability(solution))
            return;

        //At this point, it should try to light
    }

    public void OnIgnite(EntityUid uid, ActualFireComponent fire)
    {

        //check data of the fluid to light on fire
        //give the item the ActualFireComponent
        //use the component to add a flag for the client to know when to render the fire visuals

    }

    public void UpdateData(ActualFireComponent fire)
    {
        var entity = _entityManager.TryGetComponent<SolutionComponent>(fire.TargetEntity, out var solution);

        if (solution == null || solution.Solution.Contents.Count == 0)
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

        fire.GenratedHeat = finalTemp * fire.Oxidation;

        fire.GasSpawnEntries = exhaust.ToArray();
    }

    private bool CheckFlammability(SolutionComponent solution)
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
}
