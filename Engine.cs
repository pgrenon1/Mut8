using Mut8.Scripts.Screens;
using SadConsole.Configuration;
using SadConsole.Input;

namespace Mut8
{
    /// <summary>
    /// ***********README***********
    ///
    /// The provided code is a simple template to demonstrate some integration library features and set up some boilerplate
    /// for you.  Feel free to use, or delete, any of it that you want; it shows one way of doing things, not the only way!
    ///
    /// The code contains a few comments beginning with "CUSTOMIZATION:", which show you some common points to modify in
    /// order to accomplish some common tasks.  The tags by no means represent a comprehensive guide to everything you
    /// might want to modify; they're simply designed to provide a "quick-start" guide that can help you accomplish some
    /// common tasks.
    /// </summary>
    internal static class Engine
    {
        public const int WINDOW_WIDTH = 80*2;
        public const int WINDOW_HEIGHT = 25*2;
        private const string MAP_FONT_PATH = "./Assets/Fonts/kenney_combined.font";
        
        public static MainGame? MainGame;

        private static void Main()
        {
            Settings.WindowTitle = "Mut8";

            // Configure how SadConsole starts up
            Builder startup = new Builder()
                    .SetWindowSizeInCells(WINDOW_WIDTH, WINDOW_HEIGHT)
                    .OnStart(Init)
                    .ConfigureFonts(SetupFonts)
                    .EnableImGuiDebugger(Keys.F2)
                ;

            // Setup the engine and start the game
            Game.Create(startup);
            Game.Instance.Run();
            Game.Instance.Dispose();
        }

        private static void Init(object? s, GameHost host)
        {
            MainGame = new MainGame();
            host.Screen = MainGame;

            MainGame.StartGame();

            // Create a MapScreen and set it as the active screen so that it processes input and renders itself.
            //GameScreen = new MapScreen(map);
            //host.Screen = GameScreen;

            SetupWindowResizeHandler();
        }

        private static void SetupFonts(FontConfig config, GameHost host)
        {
            config.UseBuiltinFontExtended();
            config.AddExtraFonts(MAP_FONT_PATH);
        }

        private static void SetupWindowResizeHandler()
        {
            Settings.ResizeMode = Settings.WindowResizeOptions.Fit;

            SadConsole.Host.Game monoGameInstance = (SadConsole.Host.Game)SadConsole.Game.Instance.MonoGameInstance;
            monoGameInstance.WindowResized += MonoGameInstance_WindowResized;
        }

        private static void MonoGameInstance_WindowResized(object? sender, EventArgs e)
        {
            //var root = (IScreenSurface)Game.Instance.Screen!;
            //var resizableSurface = (ICellSurfaceResize)root.Surface;

            //resizableSurface.Resize(width: SadConsole.Settings.Rendering.RenderWidth / root.FontSize.X,
            //                        height: SadConsole.Settings.Rendering.RenderHeight / root.FontSize.Y,
            //                        clear: false);


            //var mapScreenSurface = (IScreenSurface)GameScreen!.Map.DefaultRenderer!;
            //var resizableSurface = (ICellSurfaceResize)mapScreenSurface;

            //resizableSurface.Resize(width: SadConsole.Settings.Rendering.RenderWidth / mapScreenSurface.FontSize.X,
            //                        height: SadConsole.Settings.Rendering.RenderHeight / mapScreenSurface.FontSize.Y,
            //                        clear: false);


            //var mapScreenSurface = (IScreenSurface)GameScreen!.Map.DefaultRenderer!;
            //var resizableSurface = (ICellSurfaceResize)mapScreenSurface;

            //mapScreenSurface.Surface.View = mapScreenSurface.Surface.View.WithSize(
            //                     SadConsole.Settings.Rendering.RenderWidth / mapScreenSurface.FontSize.X,
            //                     SadConsole.Settings.Rendering.RenderHeight / mapScreenSurface.FontSize.Y);
        }
    }
}
