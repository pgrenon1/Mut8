using Mut8.Scripts.Utils;
using SadRogue.Integration;
using SadRogue.Integration.Components;

namespace Mut8.Scripts.MapObjects.Components
{
    internal class CombatStats : RogueLikeComponentBase<RogueLikeEntity>
    {
        private Genome? _genome;

        private float BaseAttackPower { get; set; }
        private float BaseDefense { get; set; }

        public float AttackPower { get; private set; }
        public float Defense { get; private set; }

        public event EventHandler? StatsChanged;

        public CombatStats(float baseAttackPower = 10f, float baseDefense = 5f) : 
            base(false, false, false, false)
        {
            BaseAttackPower = baseAttackPower;
            BaseDefense = baseDefense;
            AttackPower = baseAttackPower;
            Defense = baseDefense;
        }

        public override void OnAdded(IScreenObject parent)
        {
            base.OnAdded(parent);
            _genome = parent.GetSadComponent<Genome>();

            if (_genome != null)
            {
                _genome.RegisterGeneChangedCallback(Gene.Strong, OnCombatGeneChanged);
                _genome.RegisterGeneChangedCallback(Gene.Resilient, OnCombatGeneChanged);
            }

            RecalculateStats();
        }

        public override void OnRemoved(IScreenObject parent)
        {
            if (_genome != null)
            {
                _genome.UnregisterGeneChangedCallback(Gene.Strong, OnCombatGeneChanged);
                _genome.UnregisterGeneChangedCallback(Gene.Resilient, OnCombatGeneChanged);
            }

            base.OnRemoved(parent);
        }

        private void OnCombatGeneChanged(float oldValue, float newValue)
        {
            RecalculateStats();
        }

        private void RecalculateStats()
        {
            float strongModifier = _genome?.GetGene(Gene.Strong, 0f) ?? 0f;
            float resilientModifier = _genome?.GetGene(Gene.Resilient, 0f) ?? 0f;

            float newAttackPower = BaseAttackPower + (GameData.StrongGeneAttackMultiplier * strongModifier);
            float newDefense = BaseDefense + (GameData.ResilientGeneDefenseMultiplier * resilientModifier);

            if (!newAttackPower.IsEqualWithTolerance(AttackPower) || 
                !newDefense.IsEqualWithTolerance(Defense))
            {
                AttackPower = newAttackPower;
                Defense = newDefense;
                StatsChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
