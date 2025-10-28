using Mut8.Scripts.MapObjects.Components;
using SadRogue.Integration;

namespace Mut8.Scripts.Actions
{
    /// <summary>
    /// Base class for actions that require an actor to perform them.
    /// </summary>
    internal abstract class ActorAction : IAction
    {
        public RogueLikeEntity Entity { get; }
        public Actor? ActorComponent { get; }

        protected ActorAction(RogueLikeEntity entity)
        {
            Entity = entity;
            ActorComponent = entity.AllComponents.GetFirstOrDefault<Actor>();
        }

        public abstract ActionResult Perform();
    }
}