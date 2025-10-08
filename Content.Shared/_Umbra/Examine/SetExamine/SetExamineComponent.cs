using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Umbra.Examine;

/// <summary>
/// Flavour text when this entity is examined. Set with an action.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedSetExamineSystem))]
public sealed partial class SetExamineComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Action;

    [DataField, AutoNetworkedField]
    public EntProtoId ActionPrototype; // = "ActionSetExtraExamine", but that was breaking things /shrug

    [DataField, AutoNetworkedField]
    public string ExamineText = string.Empty;
}
