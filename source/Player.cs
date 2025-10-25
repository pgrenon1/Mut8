using SadRogue.Integration;
using SadRogue.Integration.Keybindings;

namespace Mut8
{
    internal class Player : RogueLikeEntity
    {
        public Player()
            : base(Color.White, Color.Black, 25, false, layer: (int)MyGameMap.Layer.Monsters)
        {
            // Motion control
            var motionControl = new CustomKeybindingsComponent();
            motionControl.SetMotions(KeybindingsComponent.ArrowMotions);
            motionControl.SetMotions(KeybindingsComponent.NumPadAllMotions);
            AllComponents.Add(motionControl);

            // FOV controller
            AllComponents.Add(new PlayerFOVController());
        }
    }
}