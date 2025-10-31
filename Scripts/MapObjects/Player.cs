using Mut8.Scripts.MapObjects.Components;
using Mut8.Scripts.Maps;
using SadRogue.Integration;
using SadRogue.Integration.Keybindings;

namespace Mut8.Scripts.MapObjects
{
    internal class Player : RogueLikeEntity
    {
        public Player()
            : base(Color.White, Color.Black, 1792, false, layer: (int)GameMap.Layer.Monsters)
        {
            Name = "Player";

            // Actor component (enables turn-based actions)
            AllComponents.Add(new Actor(0));

            // Motion control
            var motionControl = new CustomKeybindingsComponent();
            motionControl.SetMotions(KeybindingsComponent.ArrowMotions);
            motionControl.SetMotions(KeybindingsComponent.NumPadAllMotions);
            AllComponents.Add(motionControl);

            // FOV controller
            AllComponents.Add(new PlayerFOVController());

            // Reveal all tiles component (F3 key)
            AllComponents.Add(new RevealAllTilesComponent());

            // Health
            AllComponents.Add(new Health(100));
        }
    }
}