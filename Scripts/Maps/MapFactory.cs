﻿using GoRogue.MapGeneration;
using GoRogue.Random;
using Mut8.Scripts.MapObjects;
using SadRogue.Integration.FieldOfView.Memory;
using SadRogue.Primitives.GridViews;
using ShaiRandom.Generators;
using SadRogue.Integration;

namespace Mut8.Scripts.Maps
{
    /// <remarks>
    /// CUSTOMIZATION: Modify the functions as applicable to generate appropriate maps; both the map data generation
    /// (using GoRogue) and the translation to integration library structure occurs here.
    ///
    /// As is the case for map objects, you can use composition to create your objects by attaching components
    /// directly to the map.  The integration library also supports creating subclasses of RogueLikeMap (as we do
    /// here).
    ///
    /// Additionally, GoRogue's map generation framework supports adding arbitrary components to contexts, so the integration
    /// library map could be added as a component to the context, and then things like enemy placement could be their own
    /// custom GoRogue map generation steps.
    /// </remarks>
    internal static class MapFactory
    {
        public static GameMap GenerateDungeonMap(int width, int height)
        {
            // Generate a rectangular map for the sake of testing with GoRogue's map generation system.
            //
            // CUSTOMIZATION: Use a different set steps in AddSteps to generate a different type of map.
            var generator = new Generator(width, height)
                .ConfigAndGenerateSafe(gen =>
                {
                    gen.AddSteps(DefaultAlgorithms.CellularAutomataGenerationSteps());
                });

            var generatedMap = generator.Context.GetFirst<ISettableGridView<bool>>("WallFloor");

            // Set the generated map in MapObjectFactory for bitmask calculations
            MapObjectFactory.SetGeneratedMap(generatedMap);

            // Create actual integration library map.
            var map = new GameMap(generator.Context.Width, generator.Context.Height, null);

            // Add a component that will implement a character "memory" system, where tiles will be dimmed when they aren't seen by the player,
            // and remain visible exactly as they were when the player last saw them regardless of changes to their actual appearance,
            // until the player sees them again.
            //
            // CUSTOMIZATION: If you want to handle FOV visibility differently, you can create an instance of one of the
            // other classes in the FieldOfView namespace, or create your own by inheriting from FieldOfViewHandlerBase
            map.AllComponents.Add(new DimmingMemoryFieldOfViewHandler(0.6f));

            // Translate GoRogue's terrain data into actual integration library objects.  Our terrain must be of type
            // MemoryAwareRogueLikeCells because we are using the integration library's "memory-based" fov visibility
            // system.
            map.ApplyTerrainOverlay(generatedMap, (pos, val) => val ? MapObjectFactory.Floor(pos) : MapObjectFactory.Wall(pos));

            // Generate 5 enemies, placing them in random walkable locations for demo purposes.
            for (int i = 0; i < 5; i++)
            {
                Point position = GlobalRandom.DefaultRNG.RandomPosition(map.WalkabilityView, true);
                var enemy = MapObjectFactory.EntityFactory!.Create("Enemy", position);
                map.AddEntity(enemy);
            }

            return map;
        }

    }
}
