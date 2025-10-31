using GoRogue.GameFramework;
using Mut8.Scripts.Actions;
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

        public IAction? GenerateAction()
        {
            if (Parent?.CurrentMap == null) return new WaitAction(Parent);
            if (!Parent.CurrentMap.PlayerFOV.CurrentFOV.Contains(Parent.Position)) return new WaitAction(Parent);

            Player? player = Engine.MainGame!.Player;
            if (player == null) return new WaitAction(Parent);

            var path = Parent.CurrentMap.AStar.ShortestPath(Parent.Position, player.Position);
            if (path == null) return new WaitAction(Parent);

            var firstPoint = path.GetStep(0);
            var direction = Direction.GetDirection(Parent.Position, firstPoint);

            return new MoveAction(Parent, direction);
        }
    }
}
