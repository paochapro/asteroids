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
    
    //Game
    private static readonly Range<int> lowChance_ufoSpawnRange = new(2, 8);
    private static readonly Range<int> highChance_ufoSpawnRange = new(8, 14);
    
    public static Group<Player> Players => Asteroids.Player.Group;
    
    private static int phase;
    private const int startingAsteroids = 4;
    private const int startingPhase = 0;
    private const int maxAsteroids = 10;

    private const int startLives = 5;
    
    private static int lives;
    private static int score;
    private static bool trySpawnPlayer;
    private static Label scoreLabel;
    private static Label livesLabel;

    private const int ufoScore = 50;
    private const int asteroidScore = 100;

    private static readonly Point2 spawn = center(defaultScreenSize, Player.size);  
    private const float spawnSafetyRadius = 200f;

    public static void UfoDestroyed()
    {
        NextUfoSpawn();
        ObjectDestroyed(ufoScore);
    }
    
    public static void AsteroidDestroyed() => ObjectDestroyed(asteroidScore);
    
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

        int asteroids = startingAsteroids + phase / 2;
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
    
    public static void Reset()
    {
        trySpawnPlayer = false;
        phase = startingPhase;
        lives = startLives;
        score = 0;
        
        Entity.RemoveAll();
        Event.ClearEvents();
        SpawnPlayer();
        NextPhase();
        UpdateScoreLabel();
        UpdateLivesLabel();
    }

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
        
        //Content
        Player.PlayerTexture = Assets.LoadTexture("player_v2");
        Ufo.BigUfoTexture = Assets.LoadTexture("ufo_big");
        Ufo.SmallUfoTexture = Assets.LoadTexture("ufo_small");
        
        CreateUi();
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
    
    private static bool CanPlayerSpawn()
    {
        bool canSpawn = true;
        
        Asteroid.Group.Iterate(asteroid =>
        {
            IRadiusCollider collider = asteroid as IRadiusCollider;
            float totalRadius = collider.CollisionRadius + spawnSafetyRadius;
            
            if (Vector2.Distance(collider.CollisionOrigin, spawn) < totalRadius)
                canSpawn = false;
        });

        return canSpawn;
    }

    private static void SpawnPlayer()
    {
        Player.Group.Clear();
        Player.Group.Add(new Player(spawn));
    }

    private static void GameOver()
    {
        Console.WriteLine("game over!");
        Reset();
    }

    private const float gameOverDelay = 2f;
    private const float respawnDelay = 2f;
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
        
        Event.Add(() => {
            Ufo.Group.Clear();
            NextUfoSpawn();
        }, respawnDelay/2);
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
            Collisions.Update();
            Controls();

            if (trySpawnPlayer && CanPlayerSpawn())
            {
                SpawnPlayer();
                trySpawnPlayer = false;
            }
        }

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
    
    const int uiBottomOffset = 10;

    
    public static void UpdateScoreLabel()
    {
        string text = score.ToString();
        Vector2 measure = UI.Font.MeasureString(text);
        scoreLabel.text = text;
        scoreLabel.Position = new Point((int)center(Screen.X/2, Screen.X, measure.X), (int)(Screen.Y - measure.Y - uiBottomOffset));
    }
    
    public static void UpdateLivesLabel()
    {
        string text = "Lives: " + lives.ToString();
        Vector2 measure = UI.Font.MeasureString(text);
        livesLabel.text = text;
        livesLabel.Position = new Point((int)center(0, Screen.X/2, measure.X), (int)(Screen.Y - measure.Y - uiBottomOffset));
    }
    
    private static void CreateUi()
    {
        scoreLabel = new Label(Point.Zero, "0", Color.White, 1);
        livesLabel = new Label(Point.Zero, "Lives: -", Color.White, 1);
        UI.Add(scoreLabel);
        UI.Add(livesLabel);
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