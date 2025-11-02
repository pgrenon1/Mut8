using Mut8.Scripts.Utils;
using SadRogue.Integration;
using SadRogue.Integration.Components;

namespace Mut8.Scripts.MapObjects.Components
{
    internal class Health : RogueLikeComponentBase<RogueLikeEntity>
    {
        private Genome? _genome;

        private float BaseMaxHP { get; set; }
        
        public float MaxHP { get; private set; }

        private float _hp;
        public float HP
        {
            get => _hp;
            private set
            {
                if (_hp == value) return;

                _hp = Math.Clamp(value, 0f, MaxHP);
                HPChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler? HPChanged;
        public event EventHandler? Died;

        public Health(float baseMaxHP = 100f) : base(false, false, false, false)
        {
            BaseMaxHP = baseMaxHP;
            MaxHP = baseMaxHP;
        }
        
        public override void OnAdded(IScreenObject parent)
        {
            base.OnAdded(parent);
            _genome = parent.GetSadComponent<Genome>();
            
            if (_genome != null)
            {
                _genome.RegisterGeneChangedCallback(Gene.Stout, OnStoutGeneChanged);
            }
            
            RecalculateMaxHP();
            HP = MaxHP;
        }
        
        public override void OnRemoved(IScreenObject parent)
        {
            if (_genome != null)
            {
                _genome.UnregisterGeneChangedCallback(Gene.Stout, OnStoutGeneChanged);
            }
            
            base.OnRemoved(parent);
        }
        
        private void OnStoutGeneChanged(float oldValue, float newValue)
        {
            RecalculateMaxHP();
        }
        
        private void RecalculateMaxHP()
        {
            float stoutModifier = _genome?.GetGene(Gene.Stout, 0f) ?? 0f;
            float newMaxHP = BaseMaxHP + (GameData.StoutGeneHPMultiplier * stoutModifier);
            
            if (!newMaxHP.IsEqualWithTolerance(MaxHP))
            {
                float hpRatio = HP / MaxHP;
                MaxHP = newMaxHP;
                HP = Math.Min(HP, MaxHP);
                
                HPChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void TakeDamage(float damage)
        {
            if (damage < 0f)
            {
                damage = 0f;
            }

            HP -= damage;
            
            if (HP <= 0f)
            {
                Death();
            }
        }

        private void Death()
        {
            Died?.Invoke(this, EventArgs.Empty);
            
            Engine.MainGame?.MessagePanel?.AddMessage($"{Parent!.Name} has died.");
            
            Parent!.CurrentMap?.RemoveEntity(Parent);
        }

        public void Heal(float amount)
        {
            if (amount < 0f)
            {
                amount = 0f;
            }

            HP += amount;
        }
    }
}
