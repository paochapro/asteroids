using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Asteroids;
using static Utils;

class Ufos : Group<Ufo>
{
    public static void Add(Vector2 side) => Add(new Ufo(side));
}

class Ufo : Entity, IRadiusCollider
{
    private const float collisionRadius = 16f;
    public float CollisionRadius { get; init; } = collisionRadius;
    public Point2 CollisionOrigin { get; private set; }
    
    private static Point2 size = new(32,32);
    private static Texture2D ufoTexture = Assets.LoadTexture("ufo");
    private const float speed = 200f;

    private const float boundsImmersion = 0.5f;

    private Vector2 moveDirection;

    public Ufo(Vector2 side)
        : base(new RectangleF(Point2.Zero, size), ufoTexture)
    {
        //x
        float x = 0;
        if (side == Vector2.UnitX) x = 0 - size.X * boundsImmersion; //Right side
        if (side == -Vector2.UnitX) x = MainGame.Screen.Y - (size.X - size.X * boundsImmersion); //Left side
        
        if(x == 0) Console.WriteLine("Ufo spawned incorrectly! Side parameter: " + side);
        hitbox.X = x;
        
        //y
        float offsetY = 20f;
        float topSide = offsetY;
        float bottomSide = MainGame.Screen.Y - size.Y - offsetY;
        hitbox.Y = RandomFloat(topSide, bottomSide);
        
        moveDirection = (-side).NormalizedCopy();
    }

    private void Move()
    {
        hitbox.Offset(moveDirection * speed * dt);
        CollisionOrigin = hitbox.Position;
    }

    private float dt;

    private const float shootInterval = 2f;
    private float shootTimer = 0f;
    
    protected override void Update(GameTime gameTime)
    {
        dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Move();
        InBounds(boundsImmersion);
        ShootLoop();
        UfoPlayerCollision();
    }

    private void ShootLoop()
    {
        if (shootTimer > shootInterval)
        {
            Shoot();
            shootTimer = 0f;
        }
        shootTimer += dt;
    }
    
    private Vector2 shootDirection;
    private void Shoot()
    {
        Point2 playerCenter = MainGame.Player.Hitbox.Center;
        shootDirection = playerCenter - hitbox.Center;
        shootDirection.Normalize();

        Bullets.Add(hitbox.Center, shootDirection, false);
    }
    
    private void UfoPlayerCollision()
    {
        IRadiusCollider playerCollider = (IRadiusCollider)MainGame.Player;
        
        if(playerCollider.CollidesWith(this))
            MainGame.GameOver();
    }

    protected override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawRectangle(hitbox, Color.Orange, 2f);

        if (MainGame.DebugMode)
        {
            spriteBatch.DrawLine(hitbox.Center, hitbox.Center + (shootDirection*10), Color.Red, 2f);
            spriteBatch.DrawCircle(hitbox.Center, CollisionRadius, 16, Color.Red);
        }
    }
    public override void Destroy() => Ufos.Remove(this);
}

