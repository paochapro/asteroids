using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace Asteroids;
using static Utils;

internal class Player : Entity, IRadiusCollider
{
    public float CollisionRadius { get; init; } = collisionRadius;
    public Point2 CollisionOrigin { get; private set; }

    private static readonly Texture2D playerTexture = Assets.LoadTexture("player_v2");
    public static readonly Point2 size = new(32,32);

    public static readonly Point2 centerPosition = center(MainGame.Screen, size);
    
    private Angle angle;
    private float rotationSpeed = 350f;
    private Vector2 velocity;
    public const float collisionRadius = 10;
    
    private const float acc = 1000f;
    private const float friction = 0.97f;
    private const float maxVelocity = 600f;
    private const float minFriction = 1f;
    private const float boundsImmersion = 0.5f;

    private float dt;

    protected override void Update(GameTime gameTime)
    {
        dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Controls();
        
        velocity.X = clamp(velocity.X, -maxVelocity, maxVelocity);
        velocity.Y = clamp(velocity.Y, -maxVelocity, maxVelocity);
        
        hitbox.Offset(velocity * dt);
        CollisionOrigin = hitbox.Center;
        
        InBounds(boundsImmersion);
    }

    public void Death()
    {
        hitbox.Position = centerPosition;
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
            Bullets.Add(hitbox.Center, angle.ToUnitVector(), true);
        }
    }
    
    protected override void Draw(SpriteBatch spriteBatch)
    {
        RectangleF dest = hitbox with { Position = hitbox.Center };
        
        if (MainGame.DebugMode)
        {
            spriteBatch.DrawRectangle(hitbox, Color.Orange);
            spriteBatch.DrawPoint(hitbox.Position, Color.White);
            spriteBatch.DrawPoint(hitbox.Center, Color.Green);
            spriteBatch.DrawCircle( hitbox.Center, collisionRadius, 16, Color.Red);
        }
        
        spriteBatch.Draw(texture, (Rectangle)dest, null, Color.White, -angle.Radians, new Vector2(32,32), SpriteEffects.None, 0);
    }


    public Player()
        : base(new RectangleF(Point2.Zero, size), playerTexture)
    {
        Death();
    }
}