using Mut8.Scripts.MapObjects.Components;
using SadRogue.Integration;

namespace Mut8.Scripts.Actions;

internal class MeleeAttackAction : AttackAction
{
    public MeleeAttackAction(RogueLikeEntity sourceEntity, RogueLikeEntity targetEntity) : base(sourceEntity, targetEntity)
    {
    }

    public override ActionResult Perform()
    {
        var attackerStats = Entity.GetSadComponent<Stats>();
        var defenderStats = TargetEntity.GetSadComponent<Stats>();
        var defenderHealth = TargetEntity.GetSadComponent<Health>();

        if (attackerStats == null)
        {
            Engine.MainGame?.MessagePanel?.AddMessage($"{Entity.Name} has no combat stats!");
            return ActionResult.Failure;
        }

        if (defenderHealth == null)
        {
            Engine.MainGame?.MessagePanel?.AddMessage($"{TargetEntity.Name} cannot be attacked!");
            return ActionResult.Failure;
        }

        // Calculate damage: AttackPower - Defense, with minimum damage
        float defense = defenderStats?.GetDefense() ?? 0f;
        float rawDamage = attackerStats.GetAttackPower() - defense;
        float damage = rawDamage;

        // Log the attack
        string attackMessage = $"{Entity.Name} attacks {TargetEntity.Name} for {damage:F1} damage!";
        Engine.MainGame?.MessagePanel?.AddMessage(attackMessage);
            
        defenderHealth.TakeDamage(damage);

        return ActionResult.SuccessWithTime(GetCost());
    }
}