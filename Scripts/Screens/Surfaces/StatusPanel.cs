
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

        public StatusPanel(int width, int height) : base(width, height)
        {
            CreateHPBar();
        }

        public void SetPlayer(Player player)
        {
            Player = player;

            Engine.MainGame!.Player.AllComponents.GetFirst<Health>().HPChanged += OnPlayerHPChanged;
            UpdateHPBar();
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

        private void OnPlayerHPChanged(object? sender, EventArgs e)
        {
            UpdateHPBar();
        }

        private void UpdateHPBar()
        {
            var health = Engine.MainGame!.Player.AllComponents.GetFirst<Health>();
            HPBar.Progress = health.HP;
            HPBar.DisplayText = $"HP: {health.HP} / {health.MaxHP}";
        }
    }
}
