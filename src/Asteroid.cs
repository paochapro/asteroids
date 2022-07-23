using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Asteroids;
using static Utils;

class Asteroid : Entity
{
    public static void Add(Point2 pos, bool small) => list.Add(new Asteroid(pos, small));
    
    private static List<Asteroid> list = new();
    public static List<Asteroid> List => list;
    
    private static readonly Point sizeBig = new(64, 64);
    private static readonly Point sizeSmall = new(32, 32);
    private static readonly Texture2D bigTexture = Assets.LoadTexture("asteroid_big");
    private static readonly Texture2D smallTexture = Assets.LoadTexture("asteroid_small");
    private const float smallRadius = 16; 
    private const float bigRadius = 32;
    private const float speed = 170f;
    
    private float dt;
    public float Radius { get; private set; }
    private Angle direction;
    public Angle Dir => direction;
    
    public Asteroid(Point2 pos, bool small)
        : base( new RectangleF(pos, small ? sizeSmall : sizeBig), /*small ? smallTexture : bigTexture*/ null)
    {
        Radius = small ? smallRadius : bigRadius;
        direction = new Angle(Random(0,360), AngleType.Degree);
    }
    
    private void Move()
    {
        float x = (float)Math.Cos(direction.Radians);
        float y = (float)-Math.Sin(direction.Radians);
        hitbox.Offset(new Vector2(x,y) * speed * dt);
    }
    
    protected override void Update(GameTime gameTime)
    {
        dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Move();
    }

    protected override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawCircle(hitbox.Center, Radius, 16, Color.White, 2f);
    }

    public override void Destroy() => DestroyWithList(list);
}