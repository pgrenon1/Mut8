using Mut8.Scripts.MapObjects.Components;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Integration;

namespace Mut8.Scripts.Screens.Surfaces;

internal class StatusPanel : ControlsConsole
{
    public ProgressBar? HPBar;
    
    private RogueLikeEntity _player;
    private readonly Dictionary<Gene, Label> _geneLabels = new();

    public StatusPanel(int width, int height) : base(width, height)
    {
        CreateHPBar();
        CreateGeneLabels();
    }

    public void SetPlayer(RogueLikeEntity player)
    {
        _player = player;

        Engine.MainGame!.Player.AllComponents.GetFirst<Health>().HPChanged += OnPlayerHPChanged;
        UpdateHPBar();

        var genome = Engine.MainGame!.Player.AllComponents.GetFirstOrDefault<Genome>();
        if (genome != null)
        {
            foreach (Gene gene in Enum.GetValues<Gene>())
            {
                genome.RegisterGeneChangedCallback(gene, OnGeneChanged);
            }
                
            UpdateGeneLabels();
        }

        // Register for GeneScanner updates if it exists
        var geneScanner = Engine.MainGame!.Player.AllComponents.GetFirstOrDefault<GeneScanner>();
        if (geneScanner != null)
        {
            Engine.MainGame!.Player.PositionChanged += OnPlayerPositionChanged;
        }
    }

    private void CreateHPBar()
    {
        HPBar = new ProgressBar(Width, 1, HorizontalAlignment.Left)
        {
            DisplayTextColor = Color.White,
            Position = (1, 1)
        };
        Colors hpBarColors = Colors.Default.Clone();
        hpBarColors.Appearance_ControlNormal.Foreground = Color.Green;
        hpBarColors.Appearance_ControlNormal.Background = Color.DarkRed;
        HPBar.SetThemeColors(hpBarColors);

        Controls.Add(HPBar);
    }

    private void CreateGeneLabels()
    {
        var geneValues = Enum.GetValues<Gene>();
        int startY = 3;

        for (int i = 0; i < geneValues.Length; i++)
        {
            var gene = geneValues[i];
            var label = new Label(Width - 2)
            {
                Position = (1, startY + i),
                TextColor = Color.LightGray
            };
            _geneLabels[gene] = label;
            Controls.Add(label);
        }
    }

    private void OnPlayerHPChanged(object? sender, EventArgs e)
    {
        UpdateHPBar();
    }

    private void OnPlayerPositionChanged(object? sender, ValueChangedEventArgs<Point> e)
    {
        UpdateGeneLabels();
    }

    private void OnGeneChanged(float currentValue, float oldValue)
    {
        UpdateGeneLabels();
    }

    private void UpdateHPBar()
    {
        var health = Engine.MainGame!.Player.AllComponents.GetFirst<Health>();
        HPBar.Progress = health.HP / health.MaxHP;
        HPBar.DisplayText = $"HP: {health.HP} / {health.MaxHP} (+{health.GetHealthRegen()})";
    }

    private void UpdateGeneLabels()
    {
        Genome? genome = Engine.MainGame!.Player.AllComponents.GetFirstOrDefault<Genome>();
        GeneScanner? geneScanner = Engine.MainGame!.Player.AllComponents.GetFirstOrDefault<GeneScanner>();
        if (genome == null) return;

        // Calculate surrounding gene values from scanner
        Dictionary<Gene, float> surroundingGenes = new Dictionary<Gene, float>();
        if (geneScanner != null)
        {
            foreach (Genome surroundingGenome in geneScanner.SurroundingGenomes)
            {
                (Gene, float)? highestGene = surroundingGenome.GetHighestGene();
                if (highestGene.HasValue)
                {
                    Gene gene = highestGene.Value.Item1;
                    float value = highestGene.Value.Item2;

                    if (!surroundingGenes.TryGetValue(gene, out float existing))
                    {
                        surroundingGenes[gene] = value;
                    }
                    else
                    {
                        surroundingGenes[gene] = existing + value;
                    }
                }
            }
        }

        foreach (KeyValuePair<Gene, Label> kvp in _geneLabels)
        {
            Gene gene = kvp.Key;
            Label label = kvp.Value;
            float value = genome.GetRawGene(gene);
            float surroundingValue = surroundingGenes.GetValueOrDefault(gene, 0f);
                
            if (value > 0f)
            {
                string displayText = $"{gene}: {value:F1}";
                if (surroundingValue > 0f)
                {
                    displayText += $" (+{surroundingValue:F1})";
                }
                label.DisplayText = displayText;
                label.TextColor = Color.Yellow;
            }
            else
            {
                string displayText = $"{gene}: -";
                if (surroundingValue > 0f)
                {
                    displayText += $" (+{surroundingValue:F1})";
                    label.TextColor = Color.Gray;
                }
                else
                {
                    label.TextColor = Color.DarkGray;
                }
                label.DisplayText = displayText;
            }
        }
    }

    public override string ToString()
    {
        return "Status Panel";
    }
}