using SadRogue.Integration;
using SadRogue.Integration.Components;

namespace Mut8.Scripts.MapObjects.Components
{
    internal class Health : RogueLikeComponentBase
    {
        public int MaxHP { get; }

        private int _hp;
        public int HP
        {
            get => _hp;
            private set
            {
                if (_hp == value) return;

                _hp = Math.Clamp(value, 0, MaxHP);
                HPChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler? HPChanged;
        public event EventHandler? Died;

        public Health(int maxHP = 100) : base(false, false, false, false)
        {
            MaxHP = maxHP;
            HP = MaxHP;
        }
    }
}
