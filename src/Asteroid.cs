using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Asteroids;
using static Utils;

class Asteroid : Entity
{
    public static void Add() => list.Add(new Asteroid(3));
    
    private static List<Asteroid> list = new();
    public static List<Asteroid> List => list;

    private static readonly int biggestSize = 64+32;
    private const float biggestRadius = 32+16;
    private const float speed = 170f;
    private const float boundsImmersion = 0.9f;

    private float dt;
    public float Radius { get; private set; }
    private Angle angle;
    public Vector2 Dir => angle.ToUnitVector();
    
    private Asteroid(int smaller)
        : base( new RectangleF(Point2.NaN, new Point2(biggestSize / smaller, biggestSize / smaller)), null)
    {
        int randomX = Random(0-(int)(hitbox.Size.Width * boundsImmersion), MainGame.Screen.X);
        int randomY = Random(0-(int)(hitbox.Size.Height * boundsImmersion), MainGame.Screen.Y);
        hitbox.Position = new Point2(randomX, randomY);
        
        Radius = biggestRadius / smaller;
        angle = new Angle(Random(0,360), AngleType.Degree);
    }
    
    private void Move()
    {
        hitbox.Offset(Dir * speed * dt);
    }
    
    protected override void Update(GameTime gameTime)
    {
        dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Move();
        InBounds(boundsImmersion);
    }
    
    private const float drawThickness = 2f;
    private const int drawSides = 8;
    protected override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawCircle(hitbox.Center, Radius, drawSides, Color.White, drawThickness);
    }

    public override void Destroy()
    {
        list.Remove(this);
        base.Destroy();
    }
}