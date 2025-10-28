using GoRogue.GameFramework;
using Mut8.Scripts.Actions;
using SadConsole.Input;
using SadRogue.Integration;
using SadRogue.Integration.Keybindings;

namespace Mut8.Scripts.MapObjects.Components
{
    /// <summary>
    /// Subclass of the integration library's keybindings component that handles player movement and moves enemies as appropriate when the player
    /// moves.
    /// </summary>
    /// <remarks>
    /// CUSTOMIZATION: This component is meant to be attached directly to the player entity; if you want to attach it to a renderer, map, or other
    /// surface instead, you'll need to edit the MotionHandler to reference the player directly instead of using the Parent property.  You may also
    /// want to change the parent class type parameter to specify a different type for the parent.
    ///
    /// CUSTOMIZATION: Components can also be attached to maps, so the code for calling TakeTurn on all entities could
    /// be moved to a map component as well so that it is more re-usable by code that doesn't pertain to movement.
    /// </remarks>
    internal class CustomKeybindingsComponent : KeybindingsComponent<RogueLikeEntity>
    {
        public CustomKeybindingsComponent(uint sortOrder = 5) : base(sortOrder)
        {
            // Bind F3 key to reveal all tiles action
            SetAction(Keys.F3, () =>
            {
                var revealComponent = Parent!.AllComponents.GetFirstOrDefault<RevealAllTilesComponent>();
                revealComponent?.RevealAllTiles();
            });
        }

        protected override void MotionHandler(Direction direction)
        {
            base.MotionHandler(direction);

            // Create a walk action
            var walkAction = new MoveAction(Parent!, direction);

            // Get the Actor component from the parent entity and queue the action
            var actor = Parent!.AllComponents.GetFirstOrDefault<Actor>();
            if (actor != null)
            {
                actor.SetNextAction(walkAction);
            }

            //if (!Parent!.CanMoveIn(direction)) return;

                //Parent!.Position += direction;

                //Engine.MainGame!.MessagePanel.AddMessage("You move " + direction + "!");

                //foreach (var entity in Parent.CurrentMap!.Entities.Items)
                //{
                //    var ai = entity.GoRogueComponents.GetFirstOrDefault<DemoEnemyAI>();
                //    ai?.TakeTurn();
                //}
        }
    }
}
