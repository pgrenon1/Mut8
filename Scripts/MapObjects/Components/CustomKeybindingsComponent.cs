using System.Collections.ObjectModel;
using System.Security.Cryptography;
using GoRogue.GameFramework;
using Mut8.Scripts.Actions;
using SadConsole.Input;
using SadRogue.Integration;
using SadRogue.Integration.Keybindings;

namespace Mut8.Scripts.MapObjects.Components;

/// <summary>
/// Subclass of the integration library's keybindings component that handles player movement and moves enemies as appropriate when the player
/// moves.
/// </summary>
/// <remarks>
/// CUSTOMIZATION: This component is meant to be attached directly to the player entity; if you want to attach it to a renderer, map, or other
/// surface instead, you'll need to edit the MotionHandler to reference the player directly instead of using the Parent property.  You may also
/// want to change the parent class type parameter to specify a different type for the parent.
///
/// CUSTOMIZATION: Components can also be attached to maps, so the code for calling TakeTurn on all entities could
/// be moved to a map component as well so that it is more re-usable by code that doesn't pertain to movement.
/// </remarks>
internal class CustomKeybindingsComponent : KeybindingsComponent<RogueLikeEntity>
{
    public CustomKeybindingsComponent(uint sortOrder = 5) : base(sortOrder)
    {
        SetAction(Keys.OemPeriod, Wait);

        SetAction(Keys.Space, Mutate);

        SetDebugActions();
    }

    private void Mutate()
    {
        MutateAction mutateAction = new MutateAction(Parent!);
        Actor? actor = Parent!.AllComponents.GetFirstOrDefault<Actor>();
        if (actor != null)
        {
            actor.SetNextAction(mutateAction);
        }
    }

    private void Wait()
    {
        WaitAction waitAction = new WaitAction(Parent!);
        Actor? actor = Parent!.AllComponents.GetFirstOrDefault<Actor>();
        if (actor != null)
        {
            actor.SetNextAction(waitAction);
        }
    }

    private void RevealAllTiles()
    {
        RevealAllTilesComponent? revealComponent = Parent!.AllComponents.GetFirstOrDefault<RevealAllTilesComponent>();
        revealComponent?.RevealAllTiles();
    }

    private void SetDebugActions()
    {
        // Bind F1 key to reveal all tiles action
        SetAction(Keys.F1, RevealAllTiles);

        SetAction(Keys.Z, DamageSelf);

        SetAction(Keys.X, HealSelf);

        // Debug keybindings for adding/removing genes
        SetGeneDebugActions();
    }

    private void HealSelf()
    {
        Health? health = Parent!.AllComponents.GetFirstOrDefault<Health>();
        if (health == null)
            return;

        health.Heal(10f);
    }

    private void DamageSelf()
    {
        Health? health = Parent!.AllComponents.GetFirstOrDefault<Health>();
        if (health == null)
            return;

        health.TakeDamage(10f);
    }

    private void IncrementGene(Gene gene)
    {
        var genome = Parent!.AllComponents.GetFirstOrDefault<Genome>();
        if (genome == null)
            return;

        float currentValue = genome.GetGeneNormalized(gene, 0f);
        genome.SetGene(gene, currentValue + 1f);
    }

    private void DecrementGene(Gene gene)
    {
        var genome = Parent!.AllComponents.GetFirstOrDefault<Genome>();
        if (genome == null)
            return;

        float currentValue = genome.GetGeneNormalized(gene, 0f);
        float newValue = currentValue - 1f;

        if (newValue <= 0f)
        {
            genome.RemoveGene(gene);
        }
        else
        {
            genome.SetGene(gene, newValue);
        }
    }

    private void SetGeneDebugActions()
    {
        var geneValues = Enum.GetValues<Gene>();
        var numberKeys = new[]
            { Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9, Keys.D0 };

        for (int i = 0; i < Math.Min(geneValues.Length, numberKeys.Length); i++)
        {
            var gene = geneValues[i];
            var key = numberKeys[i];

            // Number key: increment gene
            SetAction(key, () => IncrementGene(gene));

            // Shift + Number key: decrement gene
            SetAction(new InputKey(key, KeyModifiers.Shift), () => DecrementGene(gene));
        }
    }

    protected override void MotionHandler(Direction direction)
    {
        base.MotionHandler(direction);

        // Create a walk action
        var walkAction = new MoveAction(Parent!, direction);

        // Get the Actor component from the parent entity and queue the action
        var actor = Parent!.AllComponents.GetFirstOrDefault<Actor>();
        if (actor != null)
        {
            actor.SetNextAction(walkAction);
        }
    }
}