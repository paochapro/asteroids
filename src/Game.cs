using System.Security.Claims;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using MonoGame.Extended;

namespace Asteroids;
using static Utils;

//////////////////////////////// Starting point
class MainGame : Game
{
    //More important stuff
    private static Point screen = defaultScreenSize;
    public static Point Screen => screen;
    
    private static readonly Point defaultScreenSize = new(800, 800);
    private const string gameName = "Asteroids";
    private const float defaultVolume = 0.3f;
    private const bool resizable = false;

    private static OrthographicCamera camera;
    
    //General stuff
    public static GraphicsDeviceManager Graphics => graphics;
    private static GraphicsDeviceManager graphics;
    private static SpriteBatch spriteBatch;

    public static bool DebugMode { get; private set; } = true;

    public enum GameState { Menu, Game }

    private static GameState gameState;
    public static GameState State
    {
        get => gameState;
        private set
        {
            gameState = value;
            UI.CurrentLayer = Convert.ToInt32(value);
        }
    }
    
    static readonly Dictionary<GameState, Action> drawMethods = new()
    {
        [GameState.Menu] = DrawMenu,
        [GameState.Game] = DrawGame,
    };
    

    //Game
    public static Player Player => player;
    private static Player player;

    //Initialization
    private static void ChangeScreenSize(Point size)
    {
        screen = size;
        graphics.PreferredBackBufferWidth = size.X;
        graphics.PreferredBackBufferHeight = size.Y;
        graphics.ApplyChanges();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        Assets.Content = Content;
        UI.Font = Content.Load<SpriteFont>("bahnschrift");
        UI.window = Window;
        CreateUi();
        
        player = new Player(center(screen, Player.size));
        
        Asteroid.Add(new Point(500, 400), true);
        Asteroid.Add(new Point(124, 421), true);
        Asteroid.Add(new Point(212, 361), true);
        Asteroid.Add(new Point(30, 135), false);
        
        State = GameState.Game;
    }

    protected override void Initialize()
    {
        Window.AllowUserResizing = resizable;
        Window.Title = gameName;
        IsMouseVisible = true;
        camera = new OrthographicCamera(GraphicsDevice);
        
        ChangeScreenSize(defaultScreenSize);

        SoundEffect.MasterVolume = defaultVolume;
        
        base.Initialize();
    }

    private static void Reset()
    {
    }

    //Main
    protected override void Update(GameTime gameTime)
    {
        //Exit
        if (Input.IsKeyDown(Keys.Escape)) Exit();

        UI.UpdateElements(Input.Keys, Input.Mouse);
        Event.ExecuteEvents(gameTime);

        if (State == GameState.Game)
        {
            Entity.UpdateAll(gameTime);
            Controls();
        }

        Input.CycleEnd();

        base.Update(gameTime);
    }

    private static void Controls()
    {
        if (Input.Pressed(Keys.OemTilde)) 
            DebugMode = !DebugMode;

        float zoom = Input.Mouse.ScrollWheelValue / 2000f + 1f;
        camera.Zoom = clamp(zoom, camera.MinimumZoom, camera.MaximumZoom);
    }
    //Draw
    private static void DrawGame()
    {
        Entity.DrawAll(spriteBatch);
    }

    private static void DrawMenu()
    {
    }

    protected override void Draw(GameTime gameTime)
    {
        graphics.GraphicsDevice.Clear(Color.Black);
        
        spriteBatch.Begin(transformMatrix: camera.GetViewMatrix());
        {
            drawMethods[State].Invoke();
            UI.DrawElements(spriteBatch);

            if (DebugMode)
            {
                foreach (var a in Asteroid.List)
                {
                    Vector2 origin = (Vector2)a.Hitbox.Center;
                    Vector2 dir = a.Dir.ToUnitVector();
                    spriteBatch.DrawLine(origin, origin + dir * a.Radius, Color.Red);
                }
            }
        }
        spriteBatch.End();

        base.Draw(gameTime);
    }
    
    private void CreateUi()
    {        
        
    }

    public MainGame() : base()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
    }
}

class Program
{
    public static void Main()
    {
        using (MainGame game = new MainGame())
            game.Run();
    }
}
