using SadRogue.Integration;

namespace Mut8.Scripts.Actions;

internal class WaitAction : ActorAction
{
    public WaitAction(RogueLikeEntity entity) : base(entity)
    {
    }

    public override ActionResult Perform()
    {
        Engine.MainGame?.MessagePanel?.AddMessage(Entity, $"{Entity.Name} waits. [{GetCost()}]");
        
        return ActionResult.SuccessWithTime(GetCost());
    }
}