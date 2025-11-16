using Mut8.Scripts.Core;
using Mut8.Scripts.MapObjects.Components;
using Mut8.Scripts.UI.Controls;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Integration;

namespace Mut8.Scripts.Screens.Surfaces;

struct GeneProgressBar
{
    public LayeredProgressBar BaseBar;
    public ProgressBar ChildBar;
    public DrawingArea TextDrawingArea;

    public GeneProgressBar(LayeredProgressBar baseBar, ProgressBar childBar, DrawingArea textDrawingArea)
    {
        this.BaseBar = baseBar;
        this.ChildBar = childBar;
        this.TextDrawingArea = textDrawingArea;
    }
}

internal class StatusPanel : ControlsConsole
{
    public ProgressBar? HPBar;
    
    private RogueLikeEntity _player;
    private readonly Dictionary<Gene, GeneProgressBar> _geneBars = new();

    public StatusPanel(int width, int height) : base(width, height)
    {
        Surface.UsePrintProcessor = true;
        CreateHPBar();
        CreateGeneBars();
        FocusedMode = FocusBehavior.None;
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
                
            UpdateGeneBars();
        }

        // Register for GeneScanner updates if it exists
        var geneScanner = Engine.MainGame!.Player.AllComponents.GetFirstOrDefault<GeneScanner>();
        if (geneScanner != null)
        {
            geneScanner.OnGeneScannerUpdated += UpdateGeneBars;
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

    private void CreateGeneBars()
    {
        var geneValues = Enum.GetValues<Gene>();
        int startY = 3;
        int barWidth = Width;
        
        for (int i = 0; i < geneValues.Length; i++)
        {
            Gene gene = geneValues[i];

            // Base bar shows the player's own gene value (left to right)
            var baseBar = new LayeredProgressBar(barWidth, 1, HorizontalAlignment.Left)
            {
                Position = (1, startY + i),
                DisplayText = string.Empty
            };

            Colors baseColors = Colors.Default.Clone();
            baseColors.Appearance_ControlNormal.Foreground = Color.Yellow;
            baseColors.Appearance_ControlNormal.Background = Color.Transparent;
            baseBar.SetThemeColors(baseColors);

            // Child bar shows the surrounding potential, from end (right to left)
            var childBar = new ProgressBar(barWidth, 1, HorizontalAlignment.Right)
            {
                Position = baseBar.Position,
                DisplayText = string.Empty
            };

            Colors childColors = Colors.Default.Clone();
            childColors.Appearance_ControlNormal.Foreground = Color.LightGreen;
            childColors.Appearance_ControlNormal.Background = Color.Transparent;
            childBar.SetThemeColors(childColors);

            // Add the child to layered logic so its progress is clamped from the end
            baseBar.AddChildBar(childBar, LayeredProgressBar.ChildOffsetType.FromEnd);

            DrawingArea drawingArea = new DrawingArea(baseBar.Width, baseBar.Height)
            {
                Position = (baseBar.Position.X, baseBar.Position.Y),
            };
            drawingArea.Surface.UsePrintProcessor = true;
            
            // Add controls so they render layered; child after base so it draws on top, then drawing area to overlay the text
            Controls.Add(baseBar);
            Controls.Add(childBar);
            Controls.Add(drawingArea);

            _geneBars[gene] = new GeneProgressBar(baseBar, childBar, drawingArea);
        }
    }

    private void OnPlayerHPChanged(object? sender, EventArgs e)
    {
        UpdateHPBar();
    }

    private void OnPlayerPositionChanged(object? sender, ValueChangedEventArgs<Point> e)
    {
        UpdateGeneBars();
    }

    private void OnGeneChanged(float currentValue, float oldValue)
    {
        UpdateHPBar();
        UpdateGeneBars();
    }

    private void UpdateHPBar()
    {
        Health health = Engine.MainGame!.Player.AllComponents.GetFirst<Health>();
        HPBar.Progress = health.HP / health.MaxHP;
        string hpBarDisplayText = $"HP: {health.HP} / {health.MaxHP} (+{health.GetHealthRegen()})";
        HPBar.DisplayText = hpBarDisplayText;
        System.Diagnostics.Debug.WriteLine(hpBarDisplayText);
    }

    private void UpdateGeneBars()
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

        foreach (KeyValuePair<Gene, GeneProgressBar> kvp in _geneBars)
        {
            Gene gene = kvp.Key;
            LayeredProgressBar baseBar = kvp.Value.BaseBar;
            ProgressBar childBar = kvp.Value.ChildBar;
            DrawingArea drawingArea = kvp.Value.TextDrawingArea;

            float rawValue = genome.GetGeneRaw(gene);
            float baseProgress = genome.GetGeneNormalized(gene);
            float surroundingRaw = surroundingGenes.GetValueOrDefault(gene, 0f);
            float childProgress = MathF.Min(surroundingRaw / GameData.MaxGeneValue, 1f);

            baseBar.SetProgress(baseProgress);
            childBar.Progress = childProgress;
            
            baseBar.DisplayText = string.Empty;
            childBar.DisplayText = string.Empty;
            
            baseBar.IsDirty = true;
            childBar.IsDirty = true;

            string surroundText = surroundingRaw > 0f ? $" (+{surroundingRaw:F1})" : string.Empty;
            string text = $"{gene}: {rawValue:F1}% {surroundText}";
            
            // insert color changes based on the length of the bar + child bar
            int barsWidth = (int)(Math.Floor(baseBar.Width * baseBar.Progress) + Math.Floor(childBar.Width * childBar.Progress));

            string recolorWhite = "[c:r f:white][c:r b:transparent]";
            string recolorBlack = "[c:r f:black][c:r b:transparent]";
            string startingColorString = recolorWhite;
            int textLengthWithoutRecolor = text.Length;
            
            // set the starting color to black if the bar is not empty
            if (barsWidth > 0)
            {
                startingColorString = recolorBlack;
            }

            // insert the starting color at the beginning of the text
            text = text.Insert(0, startingColorString);

            // add a color change at the end if the text is longer than the bar
            if (text.Length - startingColorString.Length > barsWidth && barsWidth > 0)
                text = text.Insert(barsWidth + startingColorString.Length, recolorWhite);
            
            // fill the width with spaces
            text += new string(' ', Math.Clamp(Width - textLengthWithoutRecolor, 0, Width));
            
            drawingArea.Surface.Print(0, 0, text);
        }
    }

    public override string ToString()
    {
        return "Status Panel";
    }
}