namespace Mut8.Scripts.Actions
{
    /// <summary>
    /// Represents the result of performing an action.
    /// </summary>
    public class ActionResult
    {
        public static readonly ActionResult Success = new(true);
        public static readonly ActionResult Failure = new(false);

        /// <summary>
        /// An alternate action that should be performed instead of the one that failed.
        /// </summary>
        public IAction? Alternate { get; }

        /// <summary>
        /// True if the action was successful and energy should be consumed.
        /// </summary>
        public bool Succeeded { get; }

        private ActionResult(bool succeeded)
        {
            Succeeded = succeeded;
            Alternate = null;
        }

        private ActionResult(IAction alternate)
        {
            Succeeded = true;
            Alternate = alternate;
        }

        public static ActionResult AlternateAction(IAction action) => new(action);
    }
}