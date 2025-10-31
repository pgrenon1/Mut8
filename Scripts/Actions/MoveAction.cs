using GoRogue.GameFramework;
using Mut8.Scripts.MapObjects;
using Mut8.Scripts.MapObjects.Components;
using Mut8.Scripts.Maps;
using SadRogue.Integration;

namespace Mut8.Scripts.Actions
{
    /// <summary>
    /// Action for moving an entity in a direction.
    /// </summary>
    internal class MoveAction : ActorAction
    {
        private readonly Direction _direction;

        /// <summary>
        /// Base time cost for moving one tile.
        /// </summary>
        public int BaseMoveCost = 100;

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

                // If this is the player, immediately center the camera on them
                if (Entity is Player)
                {
                    var gameMap = Entity.CurrentMap as GameMap;
                    if (gameMap == null)
                        return ActionResult.Failure;
                    
                    var renderer = gameMap.DefaultRenderer;
                    if (renderer != null)
                    {
                        renderer.Surface.View = renderer.Surface.View.WithCenter(Entity.Position);
                    }
                }

                Engine.MainGame?.MessagePanel?.AddMessage($"[{Engine.MainGame.GameLoop.TurnNumber} - {DateTime.Now.ToString("HH:mm:ss:ffff")}] {Entity.Name} moves {_direction.ToString().ToLower()}.");

                return ActionResult.SuccessWithTime(BaseMoveCost);
            }

            // Movement failed (wall, out of bounds, etc.)
            return ActionResult.Failure;
        }
    }
}