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

class Asteroid : Entity, IRadiusCollider
{
    public Point2 CollisionOrigin { get; private set; }
    public float CollisionRadius { get; private set; }

    private const float biggestRectSize = biggestRadius * 2;
    private const float biggestRadius = 16 * biggestSize;
    private const float boundsImmersion = 0.9f;

    private int speed;
    private readonly Range<int> randomSpeedRange = new(140, 250);

    private float dt;
    private Angle angle;
    public Vector2 Dir => angle.ToUnitVector();

    private int size = 0;
    private const int biggestSize = 3;

    public Asteroid()
    {
        Point2 RandomPosition()
        {
            float immersionWidth = hitbox.Size.Width * boundsImmersion;
            float immersionHeight = hitbox.Size.Height * boundsImmersion;
        
            var sides = new {
                Top = 0 - immersionHeight,
                Bottom = MainGame.Screen.Y - (hitbox.Size.Height - immersionHeight),
                Left = 0 - immersionWidth,
                Right = MainGame.Screen.X - (hitbox.Size.Width - immersionWidth)
            };
        
            float randomX = RandomFloat(sides.Left, sides.Right);
            float randomY = RandomFloat(sides.Top, sides.Bottom);
        
            Point2 pos = new Point2(randomX, randomY);

            if(Chance(50)) 
                pos.X = Chance(50) ? sides.Left : sides.Right; //Left or right
            else
                pos.Y = Chance(50) ? sides.Top : sides.Bottom; //Top or bottom

            return pos;
        }
        
        Angle randomAngle = new Angle(Random(0,360), AngleType.Degree);
        Init(RandomPosition(), randomAngle, biggestSize);
    }
    
    //Hit asteroid
    public Asteroid(Point2 pos, Angle angle, int size) => Init(pos, angle, size);
    
    private void Init(Point2 pos, Angle angle, int size)
    {
        speed = Random(randomSpeedRange.Min, randomSpeedRange.Max);
        
        hitbox.Position = pos;
        
        float rectSize = (biggestRectSize / biggestSize) * size;
        hitbox.Size = new Point2(rectSize, rectSize);

        drawSides = (biggestDrawSides / biggestSize) * size;
        CollisionRadius = (biggestRadius / biggestSize) * size;
        this.angle = angle;
        this.size = size;
    }
    
    private void Move()
    {
        hitbox.Offset(Dir * speed * dt);
        CollisionOrigin = hitbox.Center;
    }
    
    protected override void Update(GameTime gameTime)
    {
        dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Move();
        InBounds(boundsImmersion);
        AsteroidPlayerCollision();
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
    
    private void AsteroidPlayerCollision()
    {
        IRadiusCollider playerCollider = (IRadiusCollider)MainGame.Player;

        if (playerCollider.CollidesWith(this))
            MainGame.GameOver();
    }
    
    private const int biggestDrawSides = 16;
    private const float drawThickness = 2f;
    private int drawSides = 16;
    
    protected override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawCircle(hitbox.Center, CollisionRadius, drawSides, Color.White, drawThickness);

        if (MainGame.DebugMode)
        {
            spriteBatch.DrawRectangle(hitbox, Color.Orange, 1f);
            
            Vector2 origin = (Vector2)hitbox.Center;
            spriteBatch.DrawLine(origin, origin + Dir * CollisionRadius, Color.Red);
        }
    }

    public override void Destroy() => Asteroids.Remove(this);
}