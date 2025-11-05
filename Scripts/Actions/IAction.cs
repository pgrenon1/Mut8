namespace Mut8.Scripts.Actions;

/// <summary>
/// Represents an action that can be performed by an actor in the game.
/// </summary>
public interface IAction
{
    /// <summary>
    /// Performs the action and returns the result.
    /// </summary>
    ActionResult Perform();
}