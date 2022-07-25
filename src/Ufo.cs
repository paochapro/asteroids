using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Asteroids;
using static Utils;

class Ufos : Group<Ufo>
{
    public static void Add(Vector2 side, bool small) => Add(new Ufo(side, small));
}

class Ufo : Entity, IRadiusCollider
{
    private static Texture2D bigUfoTexture = Assets.LoadTexture("ufo_big");
    private static Texture2D smallUfoTexture = Assets.LoadTexture("ufo_small");
    
    private const float bigSize = 64;
    private const float boundsImmersion = 0.5f;
    private const float bigCollisionRadius = bigSize / 2;
    private const float speed = 200f;
    
    private Vector2 moveDirection;
    private Point2 size;

    public float CollisionRadius { get; init; }
    public Point2 CollisionOrigin { get; private set; }
    
    public Ufo(Vector2 side, bool small)
        : base(new RectangleF(Point2.Zero, Point2.Zero), small ? smallUfoTexture : bigUfoTexture)
    {
        //size
        if (small) {
            size = new Point2(bigSize/2, bigSize/2);
            CollisionRadius = bigCollisionRadius / 2;
        }
        else {
            size = new Point2(bigSize,bigSize);
            CollisionRadius = bigCollisionRadius;
        }
        hitbox.Size = size;
        
        //x
        Point2 pos = Point2.Zero;
        if (side == Vector2.UnitX) pos.X = 0 - size.X * boundsImmersion; //Right side
        if (side == -Vector2.UnitX) pos.X = MainGame.Screen.Y - (size.X - size.X * boundsImmersion); //Left side
        
        //y
        float offsetY = 20f;
        float topSide = offsetY;
        float bottomSide = MainGame.Screen.Y - size.Y - offsetY;
        pos.Y = RandomFloat(topSide, bottomSide);

        if(pos.X == 0) Console.WriteLine("Ufo spawned incorrectly! Side parameter: " + side);

        hitbox.Position = pos;
        moveDirection = (-side).NormalizedCopy();
    }

    private void Move()
    {
        hitbox.Offset(moveDirection * speed * dt);
        CollisionOrigin = hitbox.Center;
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
    
    private Vector2 shootDirection = Vector2.UnitX;
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
            spriteBatch.DrawLine(hitbox.Center, hitbox.Center + (shootDirection * CollisionRadius), Color.Red);
            spriteBatch.DrawCircle(hitbox.Center, CollisionRadius, 16, Color.Red);
        }
    }
    public override void Destroy() => Ufos.Remove(this);
}

