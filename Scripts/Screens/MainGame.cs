using GoRogue.Random;
using Mut8.Scripts.Core;
using Mut8.Scripts.MapObjects;
using Mut8.Scripts.MapObjects.Components;
using Mut8.Scripts.Maps;
using Mut8.Scripts.Screens.Surfaces;
using ShaiRandom.Generators;

namespace Mut8.Scripts.Screens
{
    internal class MainGame : ScreenObject
    {
        public GameMap? Map;
        public MessageLogPanel? MessagePanel;
        public Player? Player;
        public StatusPanel? StatusPanel;
        public GameLoop GameLoop;

        private const int MAP_WIDTH = 100;
        private const int MAP_HEIGHT = 60;
        private const int MESSAGE_LOG_HEIGHT = 5;
        private const int STATUS_PANEL_WIDTH = 30;
        private const string MAP_FONT_NAME = "kenney_combined";

        public event EventHandler? PlayerCreated;

        public MainGame()
        {
            GameLoop = new GameLoop();

            CreateMessagePanel();

            CreateStatusPanel();
        }

        public override void Update(TimeSpan delta)
        {
            base.Update(delta);

            GameLoop.Process();
        }

        private void CreateStatusPanel()
        {
            StatusPanel = new StatusPanel(STATUS_PANEL_WIDTH, Engine.WINDOW_HEIGHT)
            {
                Parent = this,
                Position = new(Engine.WINDOW_WIDTH - STATUS_PANEL_WIDTH, 0)
            };
        }

        private void CreateMessagePanel()
        {
            MessagePanel = new MessageLogPanel(Engine.WINDOW_WIDTH - STATUS_PANEL_WIDTH, MESSAGE_LOG_HEIGHT)
            {
                Parent = this,
                Position = new(0, Engine.WINDOW_HEIGHT - MESSAGE_LOG_HEIGHT)
            };
        }

        private void CreateMap()
        {
            Map = MapFactory.GenerateDungeonMap(MAP_WIDTH, MAP_HEIGHT);

            IFont font = GameHost.Instance.Fonts[MAP_FONT_NAME];
            Map.DefaultRenderer = Map.CreateRenderer(
                new Point(Engine.WINDOW_WIDTH - STATUS_PANEL_WIDTH, Engine.WINDOW_HEIGHT - MESSAGE_LOG_HEIGHT).TranslateFont(
                    GameHost.Instance.DefaultFont.GetFontSize(IFont.Sizes.One),
                    font.GetFontSize(IFont.Sizes.One)
                ),
                font
            );

            Children.Add(Map);
            Map.IsFocused = true;
        }

        private void CreatePlayer()
        {
            // Generate player, add to map at a random walkable position, and calculate initial FOV
            Player = new Player();
            Player.Position = GlobalRandom.DefaultRNG.RandomPosition(Map!.WalkabilityView, true);
            Map.AddEntity(Player);
            Player.AllComponents.GetFirst<PlayerFOVController>().CalculateFOV();
            var renderer = Map.DefaultRenderer;
            if (renderer != null)
            {
                renderer.Surface.View = renderer.Surface.View.WithCenter(Player.Position);
            }
            PlayerCreated?.Invoke(this, EventArgs.Empty);
        }

        internal void StartGame()
        {
            CreateMap();

            CreatePlayer();

            StatusPanel?.SetPlayer(Player);

            // @PG: removing this because it makes the camera update only after every turn is done instead of as soon as the position changes.
            // That made the camera lag behind a bit. Instead, in MoveAction, I recenter the camera ASAP
            // Center view on player as they move
            //SadConsole.Components.SurfaceComponentFollowTarget viewLock = new() { Target = Player };
            //Map.DefaultRenderer?.SadComponents.Add(viewLock);
        }
    }
}
