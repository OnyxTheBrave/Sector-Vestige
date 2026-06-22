using Content.Shared._SV.Utility;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._SV.Fire;

/// <summary>
/// This is used for how reagents should react to being burned
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause, AutoGenerateComponentState]
public sealed partial class FlammableFluidComponent : Component
{
    /// <summary>
    /// Should the reagent be flammable
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public bool IsFlammable;

    /// <summary>
    /// Is the reagent an oxidizer
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public bool IsOxidizer;

    /// <summary>
    /// How much heat, in Joules, does the reagent produce when 1u of fluid is burnt
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public float GeneratedHeat;

    /// <summary>
    /// What is the maximum temperature the reagent fire should reach
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public float MaxHeat;

    /// <summary>
    /// The list of gasses that the fluid should produce when burnt
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public List<GasSpawnEntry> ExhaustedGases;

    /// <summary>
    /// Is it on fire? Admins should be able to manipulate this to start the fire
    /// </summary>
    [DataField]
    private bool _isOnFire;

    /// <summary>
    /// The UID of the fire
    /// </summary>
    [DataField, AutoNetworkedField]
    private EntityUid _entityUid;

    /// <summary>
    /// How long in-between fire ticks should there be
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan TimeBetweenFireTick = TimeSpan.FromSeconds(1);

    /// <summary>
    /// When the next fire tick should happen
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField, AutoNetworkedField, ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan TimeSinceLastFire = TimeSpan.Zero;
}

