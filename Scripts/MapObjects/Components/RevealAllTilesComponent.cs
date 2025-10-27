using GoRogue.Components.ParentAware;
using SadRogue.Integration;
using SadRogue.Integration.Components;
using SadRogue.Integration.FieldOfView.Memory;
using SadRogue.Primitives;
using SadConsole.Input;
using Mut8.Scripts.MapObjects;
using System.Linq;
using Mut8.Scripts.Maps;

namespace Mut8.Scripts.MapObjects.Components
{
    /// <summary>
    /// Component that handles F3 key press to mark all tiles as visited (revealed).
    /// This component is attached to the player entity and listens for F3 key presses.
    /// </summary>
    internal class RevealAllTilesComponent : RogueLikeComponentBase<RogueLikeEntity>
    {
        public RevealAllTilesComponent()
            : base(false, false, false, false)
        {

        }

        /// <summary>
        /// Marks all tiles in the current map as visited (revealed).
        /// </summary>
        public void RevealAllTiles()
        {
            if (Parent?.CurrentMap is not GameMap map)
                return;

            // Get the memory field of view handler
            var memoryHandler = map.AllComponents.GetFirstOrDefault<DimmingMemoryFieldOfViewHandler>();
            if (memoryHandler == null)
                return;

            // Iterate through all positions in the map
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    Point position = new Point(x, y);
                    
                    // Get the terrain cell at this position
                    var terrain = map.GetTerrainAt<MemoryAwareRogueLikeCell>(position);
                    if (terrain != null)
                    {
                        Parent?.CurrentMap?.PlayerFOV.CalculateAppend(position, 500);
                    }
                }
            }

            // Add a message to inform the player
            Engine.MainGame?.MessagePanel.AddMessage("All tiles have been revealed!");
        }
    }
}
