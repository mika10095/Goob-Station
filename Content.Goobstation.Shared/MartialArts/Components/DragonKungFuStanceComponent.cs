using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;


namespace Content.Goobstation.Shared.MartialArts.Components;



[RegisterComponent, NetworkedComponent]
public sealed partial class DragonKungFuStanceComponent : Component
{
    [DataField]
    public KungFuStances? SelectedMove;

    public readonly List<EntProtoId> KungFuMoves = new()
    {
        "KungFuClawStance",
        "KungFuScaleStance",
        "KungFuTailStance",
    };
    public readonly List<EntityUid> KungFuMoveEntities = new()
    {
    };
}
public enum KungFuStances
{
    ClawStance,
    ScaleStance,
    TailStance,
}
