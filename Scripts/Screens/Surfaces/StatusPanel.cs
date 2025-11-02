using Mut8.Scripts.MapObjects;
using Mut8.Scripts.MapObjects.Components;
using SadConsole.UI;
using SadConsole.UI.Controls;

namespace Mut8.Scripts.Screens.Surfaces
{
    internal class StatusPanel : ControlsConsole
    {
        public ProgressBar? HPBar;
        private Player Player;
        private readonly Dictionary<Gene, Label> _geneLabels = new();

        public StatusPanel(int width, int height) : base(width, height)
        {
            CreateHPBar();
            CreateGeneLabels();
        }

        public void SetPlayer(Player player)
        {
            Player = player;

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

        private void OnGeneChanged(float currentValue, float oldValue)
        {
            UpdateGeneLabels();
        }

        private void UpdateHPBar()
        {
            var health = Engine.MainGame!.Player.AllComponents.GetFirst<Health>();
            HPBar.Progress = health.HP;
            HPBar.DisplayText = $"HP: {health.HP} / {health.MaxHP}";
        }

        private void UpdateGeneLabels()
        {
            var genome = Engine.MainGame!.Player.AllComponents.GetFirstOrDefault<Genome>();
            if (genome == null) return;

            foreach (var (gene, label) in _geneLabels)
            {
                var value = genome.GetGene(gene, 0f);
                if (value > 0f)
                {
                    label.DisplayText = $"{gene}: {value:F1}";
                    label.TextColor = Color.Yellow;
                }
                else
                {
                    label.DisplayText = $"{gene}: -";
                    label.TextColor = Color.DarkGray;
                }
            }
        }
    }
}
