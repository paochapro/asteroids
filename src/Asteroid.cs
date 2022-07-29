using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using MonoGame.Extended;

namespace Asteroids;
using static Utils;

class AsteroidAll : Group<Asteroid>
{
    public void Add() => Add(new Asteroid());
    public void HitAdd(Point2 pos, Angle angle, int size) => Add(new Asteroid(pos, angle, size));
}

class Asteroid : Entity, IRadiusCollider
{
    public static AsteroidAll Group { get; private set; } = new();

    public Point2 CollisionOrigin { get; private set; }
    public float CollisionRadius { get; private set; }

    private const float biggestRectSize = biggestRadius * 2;
    private const float biggestRadius = 16 * biggestSize;
    private const float boundsImmersion = 0.9f;

    private int speed;
    private readonly Range<int> randomSpeedRange = new(100, 300);

    private float dt;
    private Angle angle;
    public Vector2 Dir => angle.ToUnitVector();

    private int size = 0;
    private const int biggestSize = 3;

    public Asteroid()
    {
        Point2 RandomPosition()
        {
            float immersionWidth = biggestRectSize * boundsImmersion;
            float immersionHeight = biggestRectSize * boundsImmersion;
        
            var sides = new {
                Top = 0 - immersionHeight,
                Bottom = MainGame.Screen.Y - (biggestRectSize - immersionHeight),
                Left = 0 - immersionWidth,
                Right = MainGame.Screen.X - (biggestRectSize - immersionWidth)
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
    }

    public void Hit(bool playerHit)
    {
        int newSize = size - 1;
        
        if (newSize > 0)
        {
            Group.HitAdd(hitbox.Position, new Angle(Random(0,360), AngleType.Degree), newSize);
            Group.HitAdd(hitbox.Position, new Angle(Random(0,360), AngleType.Degree), newSize);
        }

        Entity.AddEntity(new ParticleEmitter(hitbox.Center, 10*size, 100f, 1*size));

        Destroy();
        MainGame.AsteroidDestroyed(size, playerHit);
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
}