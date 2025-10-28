using GoRogue.GameFramework;
using Mut8.Scripts.MapObjects.Components;
using SadRogue.Integration;

namespace Mut8.Scripts.Actions
{
    /// <summary>
    /// Action for moving an entity in a direction.
    /// </summary>
    internal class MoveAction : ActorAction
    {
        private readonly Direction _direction;

        public MoveAction(RogueLikeEntity entity, Direction direction) 
            : base(entity)
        {
            _direction = direction;
        }

        public override ActionResult Perform()
        {
            var newPosition = Entity.Position + _direction;

            // Check if movement is valid
            if (Entity.CanMoveIn(_direction))
            {
                // Check if there's another entity at the target position
                //var targetEntity = Entity.CurrentMap?.GetEntityAt<RogueLikeEntity>(newPosition);
                //if (targetEntity != null && targetEntity.AllComponents.Contains<Actor>())
                //{
                //    // Return alternate action: attack
                //    return ActionResult.AlternateAction(new AttackAction(Entity, targetEntity));
                //}

                // Perform the move
                Entity.Position = newPosition;
                return ActionResult.Success;
            }

            // Movement failed (wall, out of bounds, etc.)
            return ActionResult.Failure;
        }
    }
}