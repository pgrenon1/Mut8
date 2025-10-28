using SadRogue.Integration;
using SadRogue.Integration.Components;

namespace Mut8.Scripts.MapObjects.Components
{
    internal class Health : RogueLikeComponentBase<RogueLikeEntity>
    {
        private Genome? _genome;
        
        public int BaseMaxHP { get; set; }
        
        public int MaxHP { get; private set; }

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

        public Health(int baseMaxHP = 100) : base(false, false, false, false)
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
                _genome.GenomeMutated += OnGenomeMutated;
                _genome.GeneChanged += OnGeneChanged;
            }
            
            RecalculateMaxHP();
            HP = MaxHP;
        }
        
        public override void OnRemoved(IScreenObject parent)
        {
            if (_genome != null)
            {
                _genome.GenomeMutated -= OnGenomeMutated;
                _genome.GeneChanged -= OnGeneChanged;
            }
            
            base.OnRemoved(parent);
        }
        
        private void OnGenomeMutated(object? sender, EventArgs e)
        {
            RecalculateMaxHP();
        }
        
        private void OnGeneChanged(object? sender, GeneChangedEventArgs e)
        {
            if (e.Gene == Gene.Stout)
            {
                RecalculateMaxHP();
            }
        }
        
        private void RecalculateMaxHP()
        {
            float stoutModifier = _genome?.GetGene(Gene.Stout, 0f) ?? 0f;
            int newMaxHP = (int)(BaseMaxHP * (1f + stoutModifier));
            
            if (newMaxHP != MaxHP)
            {
                MaxHP = newMaxHP;
                // Clamp current HP to new max
                HP = Math.Min(HP, MaxHP);
            }
        }
    }
}
