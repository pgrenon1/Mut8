using Mut8.Scripts.Core;
using SadRogue.Integration;
using SadRogue.Integration.Components;

namespace Mut8.Scripts.MapObjects.Components;

internal class Stats : RogueLikeComponentBase<RogueLikeEntity>
{
    private Genome? _genome;

    private float BaseAttackPower { get; set; }
    private float BaseDefense { get; set; }

    public Stats(float baseAttackPower = 10f, float baseDefense = 5f) : 
        base(false, false, false, false)
    {
        BaseAttackPower = baseAttackPower;
        BaseDefense = baseDefense;
    }

    public override void OnAdded(IScreenObject parent)
    {
        base.OnAdded(parent);
        
        _genome = parent.GetSadComponent<Genome>();
    }
    
    public float GetDefense()
    {
        float resilientGeneValue = _genome?.GetGeneNormalized(Gene.Resilient) ?? 0f;
        float defenseModifier = 1f + (GameData.ResilientGeneDefenseMultiplier - 1f) * resilientGeneValue;
        return BaseDefense * defenseModifier;
    }

    public float GetAttackPower()
    {
        float strongGeneValue = _genome?.GetGeneNormalized(Gene.Strong) ?? 0f;
        float attackPowerModifier = 1f + (GameData.StrongGeneAttackMultiplier - 1f) * strongGeneValue;
        return BaseAttackPower * attackPowerModifier;
    }

    public float GetSpeedMultiplier()
    {
        float quickGeneValue = _genome?.GetGeneNormalized(Gene.Quick) ?? 0f;
        return 1.0f / (1.0f + GameData.QuickGeneSpeedMultiplier * quickGeneValue);
    }
}