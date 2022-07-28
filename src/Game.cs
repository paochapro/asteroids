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
    
    //General stuff
    private static GraphicsDeviceManager graphics;
    private static SpriteBatch spriteBatch;
    private static OrthographicCamera camera;
    
    public static bool DebugMode { get; private set; } = true;
    private static bool PlayerInvincible { get; set; } = false;

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
    
    static readonly Dictionary<GameState, Action<GameTime>> updateMethods = new()
    {
        [GameState.Menu] = UpdateMenu,
        [GameState.Game] = UpdateGame,
    };
    
    //UI
    private const string livesText = "{LIVES_ICON}x";
    private const int uiBottomOffset = 10;
    private static readonly Point buttonSize = new(200,65);

    //Game
    public static Group<Player> Players => Asteroids.Player.Group;
    
    private const int startingAsteroids = 4;
    private const int maxAsteroids = 10;
    private const int startLives = 5;
    private const int ufoScore = 50;
    private const float spawnSafetyRadius = 150f;
    private const float gameOverDelay = 2f;
    private const float respawnDelay = 2f;
    
    private static readonly int[] asteroidSizeScores = { 100, 50, 20 };
    private static readonly Range<int> lowChance_ufoSpawnRange = new(2, 8);
    private static readonly Range<int> highChance_ufoSpawnRange = new(8, 14);
    private static readonly Point2 spawn = center(defaultScreenSize, Player.size);
    private static readonly Point2 screenCenter = ((Point2)defaultScreenSize).Center(Point2.Zero);

    private static int phase;
    private static int lives;
    private static int score;
    private static bool trySpawnPlayer;
    private static Label scoreLabel;
    private static Label livesLabel;
    
    public static void UfoDestroyed()
    {
        NextUfoSpawn();
        ObjectDestroyed(ufoScore);
    }

    public static void AsteroidDestroyed(int size)
    {
        int index = size - 1;
        if (index > asteroidSizeScores.Length || index < 0)
            throw new Exception($"Unhandled asteroid size-{size}, index-{index}, Game/AsteroidDestroyed"); 
        
        ObjectDestroyed(asteroidSizeScores[index]);
    }

    private static void ObjectDestroyed(int givenScore)
    {
        score += givenScore;
        UpdateScoreLabel();
        
        if (Asteroid.Group.Count == 0 && Ufo.Group.Count == 0)
            NextPhase();
    }
    
    private static void NextPhase()
    {
        phase++;

        int asteroids = startingAsteroids + phase;
        if (asteroids > maxAsteroids) asteroids = maxAsteroids;

        var addAsteroids = () =>
        {
            for (int i = 0; i < asteroids; ++i)
                Asteroid.Group.Add();

            NextUfoSpawn();
        };

        Event.Add(addAsteroids, 1f);
    }
    
    public static void NextUfoSpawn()
    {
        void UfoSpawn()
        {
            if (Asteroid.Group.Count == 0 || Ufo.Group.Count != 0) 
                return;
            
            Vector2 rightSide = Vector2.UnitX;
            Vector2 leftSide = -Vector2.UnitX;
            Ufo.Group.Add(new Ufo(Chance(50) ? rightSide : leftSide, Chance(50)));
        }

        Range<int> spawnRange = Chance(75) ? highChance_ufoSpawnRange : lowChance_ufoSpawnRange;
        Event.Add(UfoSpawn, RandomRange(spawnRange));
    }
    
    private static bool CanPlayerSpawn()
    {
        bool canSpawn = true;
        
        Asteroid.Group.Iterate(asteroid =>
        {
            IRadiusCollider collider = asteroid as IRadiusCollider;
            float totalRadius = collider.CollisionRadius + spawnSafetyRadius;
            
            if (Vector2.Distance(collider.CollisionOrigin, screenCenter) < totalRadius)
                canSpawn = false;
        });

        return canSpawn;
    }

    private static void SpawnPlayer()
    {
        Ufo.Group.Clear();
        NextUfoSpawn();
        
        Player.Group.Clear();
        Player.Group.Add(new Player(spawn));
    }

    private static void GameOver()
    {
        State = GameState.Menu;
    }
    
    public static void Death()
    {
        if (PlayerInvincible) return;
        
        Console.WriteLine("death!");
        Player.Group.Clear();
        
        --lives;
        UpdateLivesLabel();

        if (lives < 1)
        {
            Event.Add(GameOver, gameOverDelay);
            return;
        }
        
        Event.Add(() => trySpawnPlayer = true, respawnDelay);
    }
    
    //Initialization
    private static void ChangeScreenSize(Point size)
    {
        screen = size;
        graphics.PreferredBackBufferWidth = size.X;
        graphics.PreferredBackBufferHeight = size.Y;
        graphics.ApplyChanges();
    }

    public static void Reset()
    {
        trySpawnPlayer = false;
        lives = startLives;
        phase = 0;
        score = 0;
        
        Entity.RemoveAll();
        Event.ClearEvents();
        SpawnPlayer();
        NextPhase();
        UpdateScoreLabel();
        UpdateLivesLabel();
    }
    
    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        Assets.Content = Content;
        UI.Font = Content.Load<SpriteFont>("bahnschrift");
        UI.window = Window;
        
        //UI style
        UI.BgDefaultColor = Color.Black;
        UI.BgSelectedColor = Color.Black;
        UI.MainDefaultColor = Color.White;
        
        //Content
        Player.PlayerTexture = Assets.LoadTexture("player");
        Ufo.BigUfoTexture = Assets.LoadTexture("ufo_big");
        Ufo.SmallUfoTexture = Assets.LoadTexture("ufo_small");

        Texture2D[] thrusterSheet = {   Assets.LoadTexture("thruster0"), 
                                        Assets.LoadTexture("thruster1"), 
                                        Assets.LoadTexture("thruster2"), 
                                        Assets.LoadTexture("thruster3") };
        
        Player.ThrusterStart = new Animation(thrusterSheet, true);
        
        CreateUi();
        Reset();

        State = GameState.Menu;
    }

    protected override void Initialize()
    {
        //TODO: sounds, particles
        
        Window.AllowUserResizing = resizable;
        Window.Title = gameName;
        IsMouseVisible = true;
        camera = new OrthographicCamera(GraphicsDevice);
        
        ChangeScreenSize(defaultScreenSize);

        SoundEffect.MasterVolume = defaultVolume;
        
        base.Initialize();
    }
    
    //Main
    private static void UpdateGame(GameTime gameTime)
    {
        Entity.UpdateAll(gameTime);
        Collisions.Update();
        Controls();

        if (trySpawnPlayer && CanPlayerSpawn())
        {
            SpawnPlayer();
            trySpawnPlayer = false;
        }
    }
    
    private static void UpdateMenu(GameTime gameTime)
    {
        
    }

    protected override void Update(GameTime gameTime)
    {
        //Exit
        if (Input.IsKeyDown(Keys.Escape)) Exit();

        UI.UpdateElements(Input.Keys, Input.Mouse);
        Event.ExecuteEvents(gameTime);
        updateMethods[gameState].Invoke(gameTime);
        Input.CycleEnd();

        base.Update(gameTime);
    }

    private static void Controls()
    {
        if (Input.Pressed(Keys.OemTilde)) 
            DebugMode = !DebugMode;
        
        if (DebugMode)
        {
            if (Input.Pressed(Keys.I))
            {
                PlayerInvincible = !PlayerInvincible;
                Console.WriteLine("Players is " + (PlayerInvincible ? "" : "not ") + "invincible");            
            }

            float zoom = Input.Mouse.ScrollWheelValue / 2000f + 1f;
            camera.Zoom = clamp(zoom, camera.MinimumZoom, camera.MaximumZoom);
        }
    }
    
    //Draw
    private static void DrawGame()
    {
        Entity.DrawAll(spriteBatch);
        
        if (DebugMode)
        {
            spriteBatch.DrawRectangle( new RectangleF(Point2.Zero, screen), Color.Green);
            spriteBatch.DrawCircle(screenCenter, spawnSafetyRadius, 32, Color.Aqua);
        }
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
        }
        spriteBatch.End();

        base.Draw(gameTime);
    }
    
    //UI
    public static void UpdateScoreLabel()
    {
        string text = score.ToString();
        Vector2 measure = UI.Font.MeasureString(text);
        scoreLabel.text = text;
        scoreLabel.Position = new Point((int)center(0, Screen.X/2, measure.X), (int)(Screen.Y - measure.Y - uiBottomOffset));
    }

    public static void UpdateLivesLabel()
    {
        string text = livesText + lives.ToString();
        Vector2 measure = UI.Font.MeasureString(text);
        livesLabel.text = text;
        livesLabel.Position = new Point((int)center(Screen.X/2, Screen.X, measure.X), (int)(Screen.Y - measure.Y - uiBottomOffset));
    }
    
    private static void CreateUi()
    {
        //Game
        scoreLabel = new Label(Point.Zero, "0", Color.White, 1);
        livesLabel = new Label(Point.Zero, livesText + "-", Color.White, 1);
        UI.Add(scoreLabel);
        UI.Add(livesLabel);
        
        //Menu
        //Start button
        Point pos = new(center(0, screen.X, buttonSize.X), screen.Y - percent(screen.Y, 20) - buttonSize.Y);
        Rectangle buttonRect = new Rectangle(pos, buttonSize);
        
        void startGame()  {
            State = GameState.Game;
            Reset();
        }
        UI.Add(new Button(buttonRect, startGame, "Insert coin", 0));
        
        //Title
        Vector2 measure = UI.Font.MeasureString(gameName);
        Point titlePos = new Point( (int)center(0, screen.X, measure.X), percent(screen.Y, 20));
        UI.Add(new Label(titlePos, gameName, Color.White, 0));
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