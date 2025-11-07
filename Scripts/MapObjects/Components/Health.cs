using Mut8.Scripts.Core;
using Mut8.Scripts.Utils;
using SadRogue.Integration;
using SadRogue.Integration.Components;

namespace Mut8.Scripts.MapObjects.Components;

internal class Health : RogueLikeComponentBase<RogueLikeEntity>
{
    private Genome? _genome;

    private float BaseMaxHP { get; set; }
    private float BaseHealthRegen { get; set; }
    
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

    public Health(float baseMaxHP = 100f, float baseHealthRegen = 0f) : base(false, false, false, false)
    {
        BaseMaxHP = baseMaxHP;
        BaseHealthRegen = baseHealthRegen;
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

        TurnEventActor? turnEventActor = Engine.MainGame?.GameLoop.GetTurnEventActor();
        if (turnEventActor != null)
        {
            turnEventActor.OnTurn += ApplyHealthRegen;
        }
    }
        
    public override void OnRemoved(IScreenObject parent)
    {
        if (_genome != null)
        {
            _genome.UnregisterGeneChangedCallback(Gene.Stout, OnStoutGeneChanged);
        }
            
        TurnEventActor? turnEventActor = Engine.MainGame?.GameLoop.GetTurnEventActor();
        if (turnEventActor != null)
        {
            turnEventActor.OnTurn -= ApplyHealthRegen;
        }
            
        base.OnRemoved(parent);
    }
        
    private void OnStoutGeneChanged(float oldValue, float newValue)
    {
        RecalculateMaxHP();
    }
        
    private void RecalculateMaxHP()
    {
        if (_genome == null)
            return;
        
        float stoutGeneValue = _genome.GetGeneNormalized(Gene.Stout);
        float maxHPMultiplier = 1f + (GameData.StoutGeneHPMultiplier - 1f) * stoutGeneValue;
        float newMaxHP = MathF.Round(BaseMaxHP * maxHPMultiplier);
            
        if (newMaxHP.IsEqualWithTolerance(MaxHP))
            return;

        SetMaxHP(newMaxHP);
                
        HPChanged?.Invoke(this, EventArgs.Empty);
    }

    public float GetHealthRegen()
    {
        if (_genome == null)
            return BaseHealthRegen;
        
        float photosyntheticGeneValue = _genome?.GetGeneNormalized(Gene.Photosynthetic) ?? 0f;
        float healthRegenModifier = 1f + (GameData.PhotosyntheticGeneRegenMultiplier - 1f) * photosyntheticGeneValue;
        return BaseHealthRegen * healthRegenModifier;
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

    public void SetMaxHP(float newMaxHP)
    {
        float hpRatio = HP / MaxHP;
        MaxHP = newMaxHP;
        HP = MathF.Round(hpRatio * MaxHP);
    }
    
    private void ApplyHealthRegen()
    {
        float healthRegen = GetHealthRegen();

        if (healthRegen <= 0f || HP >= MaxHP)
        {
            return;
        }

        Heal(healthRegen);
    }

    public void SetHealthRegen(float regen)
    {
        BaseHealthRegen = regen;
    }
}
