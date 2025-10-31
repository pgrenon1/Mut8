using SadRogue.Integration;

namespace Mut8.Scripts.Actions
{
    internal class WaitAction : ActorAction
    {
        /// <summary>
        /// Time cost for waiting/passing the turn.
        /// </summary>
        public const int WaitTimeCost = 100;

        public WaitAction(RogueLikeEntity entity) : base(entity)
        {
        }

        public override ActionResult Perform()
        {
            return ActionResult.SuccessWithTime(WaitTimeCost);
        }
    }
}
