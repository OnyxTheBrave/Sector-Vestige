namespace Content.Shared._SV.Fire;

/// <summary>
/// This handles...
/// </summary>
public sealed class FlammableFluidSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {

    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
    }

    public void Extinguish(EntityUid uid, FlammableFluidComponent fluidComponent)
    {

    }

    public void Ignite(EntityUid uid, FlammableFluidComponent fluidComponent)
    {

    }

}
