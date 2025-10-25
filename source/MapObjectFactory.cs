using SadRogue.Integration;
using SadRogue.Integration.FieldOfView.Memory;

namespace Mut8
{
    /// <summary>
    /// Simple class with some static functions for creating map objects.
    /// </summary>
    /// <remarks>
    /// CUSTOMIZATION:  This demonstrates how to create objects based on "composition," which means using components.
    /// The integration library offers a robust component system that integrates both SadConsole's and GoRogue's components
    /// into one interface. You can either add more functions to create more objects, or remove this and
    /// implement the "factory" system in the GoRogue.Factories namespace, which provides a more robust interface for it.
    ///
    /// Note that SadConsole components cannot be attached directly to `RogueLikeCell` or `MemoryAwareRogueLikeCell`
    /// instances for reasons pertaining to performance.
    ///
    /// Alternatively, you can remove this system and choose to use inheritance to create your objects instead - the
    /// integration library also supports creating subclasses or RogueLikeCell and RogueLikeEntity.
    /// </remarks>
    internal static class MapObjectFactory
    {
        public static MemoryAwareRogueLikeCell Floor(Point position)
            => new(position, Color.White, Color.Black, (17 * 49 + 46) /*1007*/, (int)MyGameMap.Layer.Terrain);

        public static MemoryAwareRogueLikeCell Wall(Point position)
            => new(position, Color.White, Color.Black, '#', (int)MyGameMap.Layer.Terrain, false, false);

        public static RogueLikeEntity Player()
        {
            // Return the new Player which configures its own components
            return new Player();
        }

        public static RogueLikeEntity Enemy()
        {
            var enemy = new RogueLikeEntity(Color.Red, Color.Black, 467, false, layer: (int)MyGameMap.Layer.Monsters);

            // Add AI component to path toward player when in view
            enemy.AllComponents.Add(new DemoEnemyAI());

            return enemy;
        }

    }
}