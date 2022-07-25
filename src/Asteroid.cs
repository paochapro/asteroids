using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Asteroids;
using static Utils;

class Asteroids : Group<Asteroid>
{
    public static void Add() => Add(new Asteroid());
    public static void HitAdd(Point2 pos, Angle angle, int size) => Add(new Asteroid(pos, angle, size));
}

class Asteroid : Entity
{
    private const float biggestRectSize = biggestRadius*2;
    private const float biggestRadius = 32+16;
    private const float speed = 170f;
    private const float boundsImmersion = 0.9f;

    private float dt;
    public float Radius { get; private set; }
    private Angle angle;
    public Vector2 Dir => angle.ToUnitVector();

    private int size = 0;
    private const int biggestSize = 3;

    //Biggest
    public Asteroid()
    {
        int randomX = Random(0-(int)(hitbox.Size.Width * boundsImmersion), MainGame.Screen.X);
        int randomY = Random(0-(int)(hitbox.Size.Height * boundsImmersion), MainGame.Screen.Y);

        Angle randomAngle = new Angle(Random(0,360), AngleType.Degree);
        
        Init(new Point(randomX, randomY), randomAngle, biggestSize);
    }
    
    //Hit asteroid
    public Asteroid(Point2 pos, Angle angle, int size) => Init(pos, angle, size);
    
    private void Init(Point2 pos, Angle angle, int size)
    {
        hitbox.Position = pos;
        
        float rectSize = (biggestRectSize / biggestSize) * size;
        hitbox.Size = new Point2(rectSize, rectSize);
        
        Radius = (biggestRadius / biggestSize) * size;
        this.angle = angle;
        this.size = size;
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

    public void Hit()
    {
        Destroy();
        
        int newSize = size - 1;
        if (newSize < 1) return;

        Angle rotatedAngle = new Angle(90f, AngleType.Degree);
        Angle angle1 = angle + rotatedAngle;
        Angle angle2 = angle - rotatedAngle;
        
        Asteroids.HitAdd(hitbox.Position, angle1, newSize);
        Asteroids.HitAdd(hitbox.Position, angle2, newSize);
    }
    
    private const float drawThickness = 2f;
    private const int drawSides = 32;
    protected override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawCircle(hitbox.Center, Radius, drawSides, Color.White, drawThickness);

        if (MainGame.DebugMode)
        {
            spriteBatch.DrawPoint(hitbox.Center, Color.Blue, 4f);
            spriteBatch.DrawRectangle(hitbox, Color.Orange, drawThickness);
        }
    }

    public override void Destroy() => Asteroids.Remove(this);
}