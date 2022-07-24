using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace Asteroids;
using static Utils;

internal class Player : Entity
{
    private static readonly Texture2D playerTexture = Assets.LoadTexture("player");
    public static readonly Point size = new(64,64);
    
    private Angle angle;
    private float rotationSpeed = 350f;
    private Vector2 velocity;
    public const float collisionRadius = 20f;
    
    private const float acc = 1000f;
    private const float friction = 0.97f;
    private const float maxVelocity = 600f;
    private const float minFriction = 1f;
    private const float boundsImmersion = 0.5f;

    private float dt;

    public Point2 Position  {
        get => hitbox.Position;
        set => hitbox.Position = value;
    }

    protected override void Update(GameTime gameTime)
    {
        dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Controls();
        
        velocity.X = clamp(velocity.X, -maxVelocity, maxVelocity);
        velocity.Y = clamp(velocity.Y, -maxVelocity, maxVelocity);
        
        hitbox.Offset(velocity * dt);

        AsteroidCollision();
        
        InBounds(boundsImmersion);
    }

    private void Death()
    {
        Console.WriteLine("Death!");
        MainGame.Reset();
    }
    
    private void AsteroidCollision()
    {
        foreach (Asteroid asteroid in Asteroid.List)
        {
            float distToAsteroid = Vector2.Distance(asteroid.Hitbox.Center, hitbox.Center);
            
            if (distToAsteroid < collisionRadius + asteroid.Radius)
            {
                Death();
                return;
            }
        }
    }

    private void Controls()
    {
        int direction = Convert.ToInt32(Input.IsKeyDown(Keys.D)) - Convert.ToInt32(Input.IsKeyDown(Keys.A));

        angle.Degrees -= direction * rotationSpeed * dt;
        angle.Wrap();

        if (Input.IsKeyDown(Keys.W))
        {
            velocity += angle.ToUnitVector() * acc * dt;
        }
        else
        {  
            velocity *= friction;
            
            if (Math.Abs(velocity.X) < minFriction && Math.Abs(velocity.Y) < minFriction)
                velocity = Vector2.Zero;
        }
        
        if(Input.Pressed(Keys.Space))
        {
            Bullet.Add(hitbox.Position + new Vector2(hitbox.Width * 0.5f, hitbox.Height * 0.5f), angle);
        }
    }
    
    protected override void Draw(SpriteBatch spriteBatch)
    {
        Rectangle final = (Rectangle)hitbox;
        Vector2 half = new(final.Width * 0.5f, final.Height * 0.5f);
        Vector2 origin = half;
        final.Location += half.ToPoint();

        if (MainGame.DebugMode)
        {
            spriteBatch.DrawRectangle(hitbox, Color.Orange);
            spriteBatch.DrawPoint(hitbox.Position, Color.White);
            spriteBatch.DrawPoint(origin + hitbox.Position, Color.Green);
        }
        
        spriteBatch.Draw(texture, final, null, Color.White, -angle.Radians, origin, SpriteEffects.None, 0);
    }

    
    public Player(Point2 pos) : base(new RectangleF(pos, size), playerTexture)
    {}
}