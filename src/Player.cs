using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using MonoGame.Extended;

namespace Asteroids;
using static Utils;

internal class Player : Entity, IRadiusCollider
{
    public static Group<Player> Group { get; private set; } = new();
    public static readonly Point2 size = new(32,32);
    public static readonly Point2 thrusterSize = new(16,16);
    public static Texture2D PlayerTexture;
    public static Animation ThrusterAnimation;
    public static SoundEffect ThrustSound;

    public float CollisionRadius { get; init; } = collisionRadius;
    public Point2 CollisionOrigin { get; private set; }

    private float rotationSpeed = 350f;
    private Vector2 velocity;
    private Angle angle;
    
    private Animation thrusterAnimation;
    private RectangleF thrusterRect;

    private const float acc = 1000f;
    private const float friction = 0.97f;
    private const float maxVelocity = 600f;
    private const float minFriction = 1f;
    private const float boundsImmersion = 0.5f;
    private const float collisionRadius = 10;

    private float dt;
    
    protected override void Update(GameTime gameTime)
    {
        dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Controls();
        
        velocity.X = clamp(velocity.X, -maxVelocity, maxVelocity);
        velocity.Y = clamp(velocity.Y, -maxVelocity, maxVelocity);
        
        hitbox.Offset(velocity * dt);
        CollisionOrigin = hitbox.Center;
        thrusterAnimation.Update(gameTime);
        
        InBounds(boundsImmersion);
    }

    private SoundEffectInstance thrustSndInstance;


    private bool pressedW;
    private void Controls()
    {
        int direction = Convert.ToInt32(Input.IsKeyDown(Keys.D)) - Convert.ToInt32(Input.IsKeyDown(Keys.A));

        angle.Degrees -= direction * rotationSpeed * dt;
        angle.Wrap();

        if (Input.IsKeyDown(Keys.W) && !pressedW)
        {
            thrusterAnimation.PlayForward();
            thrustSndInstance.Pitch = RandomFloat(-0.1f, 0.1f);
            thrustSndInstance.Stop();
            thrustSndInstance.Play();
        }
        if (pressedW && !Input.IsKeyDown(Keys.W))
        {
            thrusterAnimation.PlayBackwards();
            thrustSndInstance.Stop();
        }
        
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
            Bullet.PlayerBullets.Add(new Bullet(hitbox.Center, angle.ToUnitVector(), true));
        }

        pressedW = Input.IsKeyDown(Keys.W);
    }
    
    protected override void Draw(SpriteBatch spriteBatch)
    {
        //Ship
        RectangleF dest = hitbox with { Position = hitbox.Center };
        Vector2 origin = texture.Bounds.Size.ToVector2() / 2;
        spriteBatch.Draw(texture, (Rectangle)dest, null, Color.White, -angle.Radians, origin, SpriteEffects.None, 0);
        
        //Thruster position
        float offset = 6f; 
        Vector2 thrusterDirection = -(angle.ToUnitVector());
        Vector2 thrusterCenter = (thrusterDirection * (size.X/2 + thrusterSize.X/2 - offset)) + hitbox.Center;
        thrusterRect.Position = thrusterCenter - (Vector2)(thrusterRect.Size / 2);

        //Thruster texture
        Texture2D thrusterTexture = thrusterAnimation.CurrentTexture;
        dest = thrusterRect with { Position = thrusterRect.Center };
        origin = thrusterTexture.Bounds.Size.ToVector2() / 2;
        spriteBatch.Draw(thrusterTexture, (Rectangle)dest, null, Color.White, -angle.Radians, origin, SpriteEffects.None, 0);
        
        if (MainGame.DebugMode)
        {
            spriteBatch.DrawRectangle(hitbox, Color.Orange);
            spriteBatch.DrawPoint(hitbox.Position, Color.White);
            spriteBatch.DrawCircle(hitbox.Center, collisionRadius, 16, Color.Red);
            spriteBatch.DrawRectangle(thrusterRect, Color.Aqua);
        }
    }

    public override void Destroy()
    {
        thrustSndInstance.Stop();
        base.Destroy();
    }

    public Player(Point2 pos)
        : base(new RectangleF(pos, size), PlayerTexture)
    {
        thrusterRect = new(Point2.Zero, thrusterSize);
        thrusterAnimation = new Animation(ThrusterAnimation);
        thrustSndInstance = ThrustSound.CreateInstance();
        thrustSndInstance.IsLooped = true;
        thrustSndInstance.Volume = 0.8f;
    }
}