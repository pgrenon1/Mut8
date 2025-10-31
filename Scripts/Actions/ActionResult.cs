namespace Mut8.Scripts.Actions
{
    /// <summary>
    /// Represents the result of performing an action.
    /// </summary>
    public class ActionResult
    {
        public static readonly ActionResult Success = new(true, 100);
        public static readonly ActionResult Failure = new(false, 0);

        /// <summary>
        /// An alternate action that should be performed instead of the one that failed.
        /// </summary>
        public IAction? Alternate { get; }

        /// <summary>
        /// True if the action was successful and time should be consumed.
        /// </summary>
        public bool Succeeded { get; }

        /// <summary>
        /// The time cost of performing this action in time units.
        /// </summary>
        public int TimeCost { get; }

        private ActionResult(bool succeeded, int timeCost)
        {
            Succeeded = succeeded;
            TimeCost = timeCost;
            Alternate = null;
        }

        private ActionResult(IAction alternate, int timeCost)
        {
            Succeeded = true;
            Alternate = alternate;
            TimeCost = timeCost;
        }

        /// <summary>
        /// Creates a successful action result with the specified time cost.
        /// </summary>
        public static ActionResult SuccessWithTime(int timeCost) => new(true, timeCost);

        /// <summary>
        /// Creates an alternate action result with the specified time cost for the original action.
        /// </summary>
        public static ActionResult AlternateAction(IAction action, int timeCost = 0) => new(action, timeCost);
    }
}