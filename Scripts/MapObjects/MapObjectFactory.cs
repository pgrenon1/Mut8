using Mut8.Scripts.MapObjects.Components;
using Mut8.Scripts.Maps;
using SadRogue.Integration;
using SadRogue.Integration.FieldOfView.Memory;
using GoRogue.Factories;
using GoRogue.GameFramework;
using SadRogue.Primitives.GridViews;

namespace Mut8.Scripts.MapObjects
{
    /// Note that SadConsole components cannot be attached directly to `RogueLikeCell` or `MemoryAwareRogueLikeCell`
    /// instances for reasons pertaining to performance.

    internal static class MapObjectFactory
    {
        public static AdvancedFactory<string, Point, RogueLikeCell>? TerrainFactory;
        public static AdvancedFactory<string, Point, RogueLikeEntity>? EntityFactory;
        
        // Store the generated map for bitmask calculations
        private static ISettableGridView<bool>? _generatedMap;

        static MapObjectFactory()
        {
            CreateTerrainFactory();

            CreateEntityFactory();
        }

        /// <summary>
        /// Sets the generated map for bitmask calculations.
        /// </summary>
        /// <param name="generatedMap">The GoRogue generated map (true = wall, false = floor)</param>
        public static void SetGeneratedMap(ISettableGridView<bool> generatedMap)
        {
            _generatedMap = generatedMap;
        }

        internal static IGameObject Floor(Point pos) => TerrainFactory!.Create("Floor", pos);
        internal static IGameObject Wall(Point pos) => TerrainFactory!.Create("Wall", pos);

        private static void CreateEntityFactory()
        {
            EntityFactory = new AdvancedFactory<string, Point, RogueLikeEntity>()
            {
                new LambdaAdvancedFactoryBlueprint<string, Point, RogueLikeEntity>(
                    "Enemy",
                    pos =>
                    {
                        var enemy = new RogueLikeEntity(
                            Color.Red,
                            Color.Black,
                            2306,
                            false,
                            layer: (int)GameMap.Layer.Monsters
                        )
                        {
                            Position = pos
                        };
                        enemy.AllComponents.Add(new DemoEnemyAI());
                        enemy.AllComponents.Add(new Actor(0));
                        enemy.AllComponents.Add(new Health(1));
                        enemy.AllComponents.Add(new CombatStats(1, 0));
                        enemy.Name = "Gnome";
                        return enemy;
                    }
                )
            };
        }

        private static void CreateTerrainFactory()
        {
            TerrainFactory = new AdvancedFactory<string, Point, RogueLikeCell>()
            {
                new LambdaAdvancedFactoryBlueprint<string, Point, RogueLikeCell>(
                    "Floor",
                    pos => 
                    {
                        return new MemoryAwareRogueLikeCell(
                            pos,
                            Color.White,
                            Color.Black,
                            2782,
                            (int)GameMap.Layer.Terrain
                        );
                    }
                ),

                new LambdaAdvancedFactoryBlueprint<string, Point, RogueLikeCell>(
                    "Wall",
                    pos =>
                    {
                        // Define wall sprite array for bitmask-based selection
                        // 1034 974 1031 978
                        // 1032 976 807 977
                        // 975 750 864 921
                        // 862 919 863 920
                        
                        int[] wallSprites = {
                            920, 863, 919, 862, 921, 864, 750, 975, 977, 807, 976, 1032, 978, 1031, 974, 1034};

                        int spriteIndex = GetWallSpriteIndex(pos, wallSprites, 2060);
                        
                        var wall = new MemoryAwareRogueLikeCell(
                            pos,
                            Color.White,
                            Color.Black,
                            spriteIndex,
                            (int)GameMap.Layer.Terrain,
                            walkable: false,
                            transparent: false
                        );

                        return wall;
                    }
                )
            };
        }

        /// <summary>
        /// Helper method to determine which sprite index to use based on a bitmask value.
        /// This method maps bitmask values (0-15) to sprite indices using a provided sprite array.
        /// </summary>
        /// <param name="bitmask">The bitmask value (0-15) indicating which neighbors match</param>
        /// <param name="spriteIndexArray">Array of sprite indices to choose from</param>
        /// <param name="defaultSpriteIndex">Default sprite index to use if bitmask is out of bounds</param>
        /// <returns>The sprite index to use for the given bitmask</returns>
        private static int GetSpriteIndexFromBitmask(int bitmask, int[] spriteIndexArray, int defaultSpriteIndex = 0)
        {
            // Validate bitmask range
            if (bitmask < 0 || bitmask >= spriteIndexArray.Length)
            {
                return defaultSpriteIndex;
            }

            return spriteIndexArray[bitmask];
        }

        /// <summary>
        /// Calculates a bitmask based on which neighbors have the same terrain type in the generated map.
        /// </summary>
        /// <param name="x">X position of the tile</param>
        /// <param name="y">Y position of the tile</param>
        /// <param name="isWall">Whether we're checking for wall neighbors (true) or floor neighbors (false)</param>
        /// <returns>Bitmask value (0-15) indicating which neighbors match the terrain type</returns>
        private static int CalculateBitmask(int x, int y, bool isWall)
        {
            if (_generatedMap == null)
                return 0;

            int mask = 0;

            // Check Up (bit 0)
            if (HasMatchingNeighbor(x, y - 1, isWall))
                mask |= 1;

            // Check Left (bit 1)
            if (HasMatchingNeighbor(x - 1, y, isWall))
                mask |= 2;

            // Check Right (bit 2)
            if (HasMatchingNeighbor(x + 1, y, isWall))
                mask |= 4;

            // Check Down (bit 3)
            if (HasMatchingNeighbor(x, y + 1, isWall))
                mask |= 8;

            return mask;
        }

        /// <summary>
        /// Checks if a neighboring cell has the same terrain type in the generated map.
        /// </summary>
        /// <param name="x">X coordinate to check</param>
        /// <param name="y">Y coordinate to check</param>
        /// <param name="isWall">Whether we're checking for wall (true) or floor (false)</param>
        /// <returns>True if the neighbor has the same terrain type</returns>
        private static bool HasMatchingNeighbor(int x, int y, bool isWall)
        {
            if (_generatedMap == null)
                return false;

            // Check bounds
            if (x < 0 || x >= _generatedMap.Width || y < 0 || y >= _generatedMap.Height)
                return false;

            // Check if the neighbor has the same terrain type
            return _generatedMap[x, y] == isWall;
        }

        /// <summary>
        /// Gets the appropriate sprite index for a wall tile based on its neighbors.
        /// </summary>
        /// <param name="pos">Position of the tile</param>
        /// <param name="wallSpriteArray">Array of wall sprite indices</param>
        /// <param name="defaultWallSprite">Default wall sprite index</param>
        /// <returns>The sprite index to use for the wall tile</returns>
        private static int GetWallSpriteIndex(Point pos, int[] wallSpriteArray, int defaultWallSprite = 0)
        {
            int bitmask = CalculateBitmask(pos.X, pos.Y, true); // true = checking for wall neighbors
            
            return GetSpriteIndexFromBitmask(bitmask, wallSpriteArray, defaultWallSprite);
        }
    }
}