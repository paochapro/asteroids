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
    
    private static readonly Point defaultScreenSize = new(1200, 800);
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

        player = new Player();

        Reset();

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

    public static void Reset()
    {
        phase = startingPhase;
        
        player.Death();
        
        Asteroids.Clear();
        Ufos.Clear();
        Bullets.Clear();

        NextPhase();

        //Ufos.Add(-Vector2.UnitX, false);
    }

    private static int phase;
    private const int startingAsteroids = 4;
    private const int startingPhase = 2;
    private const int maxAsteroids = 10;
    
    public static void NextPhase()
    {
        phase++;

        int asteroids = startingAsteroids + phase / 3;
        if (asteroids > maxAsteroids) asteroids = maxAsteroids;

        var addAsteroids = () =>
        {
            for (int i = 0; i < asteroids; ++i)
                Asteroids.Add();
        };

        Event.Add(addAsteroids, 1f);
    }

    public static void GameOver()
    {
        Console.WriteLine("game over");
        Reset();
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
                spriteBatch.DrawRectangle( new RectangleF(Point2.Zero, screen), Color.Green);
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

interface IRadiusCollider
{
    public float CollisionRadius { get; }
    public Point2 CollisionOrigin { get; }

    public bool CollidesWith(IRadiusCollider collider)
    {
        float dist = Vector2.Distance(CollisionOrigin, collider.CollisionOrigin);

        if (dist < collider.CollisionRadius + CollisionRadius)
            return true;

        return false;
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
