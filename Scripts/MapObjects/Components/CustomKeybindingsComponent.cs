using System.Collections.ObjectModel;
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
            
            // Debug keybindings for adding/removing genes
            var geneValues = Enum.GetValues<Gene>();
            var numberKeys = new[] { Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9, Keys.D0 };
            
            for (int i = 0; i < Math.Min(geneValues.Length, numberKeys.Length); i++)
            {
                var gene = geneValues[i];
                var key = numberKeys[i];
                
                // Number key: increment gene
                SetAction(key, () =>
                {
                    var genome = Parent!.AllComponents.GetFirstOrDefault<Genome>();
                    if (genome == null) 
                        return;
                    
                    var currentValue = genome.GetGene(gene, 0f);
                    genome.SetGene(gene, currentValue + 1f);
                });
                
                // Shift + Number key: decrement gene
                SetAction(new InputKey(key, KeyModifiers.Shift), () =>
                {
                    var genome = Parent!.AllComponents.GetFirstOrDefault<Genome>();
                    if (genome == null) 
                        return;
                    
                    var currentValue = genome.GetGene(gene, 0f);
                    var newValue = currentValue - 1f;
                        
                    if (newValue <= 0f)
                    {
                        genome.RemoveGene(gene);
                    }
                    else
                    {
                        genome.SetGene(gene, newValue);
                    }
                });
            }
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
