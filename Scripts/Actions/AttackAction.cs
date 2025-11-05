using SadRogue.Integration;

namespace Mut8.Scripts.Actions;

internal abstract class AttackAction : ActorAction
{
    public RogueLikeEntity TargetEntity { get; }
    public AttackAction(RogueLikeEntity sourceEntity, RogueLikeEntity targetEntity) : base(sourceEntity)
    {
        TargetEntity = targetEntity;
    }
}