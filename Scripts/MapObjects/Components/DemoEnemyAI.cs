using GoRogue.GameFramework;
using Mut8.Scripts.MapObjects;
using SadRogue.Integration;
using SadRogue.Integration.Components;

namespace Mut8.Scripts.MapObjects.Components
{
    /// <summary>
    /// Simple component that moves its parent toward the player if the player is visible. It demonstrates the basic
    /// usage of the integration library's component system, as well as basic AStar pathfinding.
    /// </summary>
    internal class DemoEnemyAI : RogueLikeComponentBase<RogueLikeEntity>
    {
        public DemoEnemyAI()
            : base(false, false, false, false)
        { }

        public void TakeTurn()
        {
            if (Parent?.CurrentMap == null) return;
            if (!Parent.CurrentMap.PlayerFOV.CurrentFOV.Contains(Parent.Position)) return;

            Player? player = Engine.MainGame!.Player;
            if (player == null) return;

            var path = Parent.CurrentMap.AStar.ShortestPath(Parent.Position, player.Position);
            if (path == null) return;

            var firstPoint = path.GetStep(0);
            if (Parent.CanMove(firstPoint))
            {
                Engine.MainGame.MessagePanel.AddMessage($"An enemy moves {Direction.GetDirection(Parent.Position, firstPoint)}!");
                Parent.Position = firstPoint;
            }

        }
    }
}
