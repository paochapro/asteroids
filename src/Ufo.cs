using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Asteroids;
using static Utils;

class Ufo : Entity, IRadiusCollider
{
    public static Group<Ufo> Group { get; private set; } = new();

    public static Texture2D BigUfoTexture;
    public static Texture2D SmallUfoTexture;
    
    private const float bigSize = 64;
    private const float boundsImmersion = 0.5f;
    private const float bigCollisionRadius = bigSize / 2;
    private const float speed = 150f;
    
    private Vector2 shootDirection = Vector2.UnitX;
    private Vector2 moveDirection;

    public float CollisionRadius { get; init; }
    public Point2 CollisionOrigin { get; private set; }
    
    private float dt;
    private const float shootInterval = 2f;
    private float shootTimer = 0f;

    //Diagonal movement
    private readonly Range<float> diagonalMovementIntervalRange = new(1f, 3f);
    private float diagonalMovementTimer = 0f;
    private float diagonalMovementInterval = 0f;
    
    private readonly Range<float> diagonalChangeDelayRange = new(1f, 5f);
    private float diagonalChangeDelay;
    private float diagonalChangeTimer;
    
    public Ufo(Vector2 side, bool small)
        : base(new RectangleF(Point2.Zero, Point2.Zero), small ? SmallUfoTexture : BigUfoTexture)
    {
        //size
        if (small) {
            hitbox.Size = new Point2(bigSize/2, bigSize/2);
            CollisionRadius = bigCollisionRadius / 2;
        }
        else {
            hitbox.Size = new Point2(bigSize,bigSize);
            CollisionRadius = bigCollisionRadius;
        }
        
        //x
        Point2 pos = Point2.Zero;
        if (side == Vector2.UnitX) pos.X = 0 - hitbox.Size.Width * boundsImmersion; //Right side
        if (side == -Vector2.UnitX) pos.X = MainGame.Screen.X - (hitbox.Size.Width - hitbox.Size.Width * boundsImmersion); //Left side
        
        //y
        float offsetY = 20f;
        float topSide = offsetY;
        float bottomSide = MainGame.Screen.Y - hitbox.Size.Height - offsetY;
        pos.Y = RandomFloat(topSide, bottomSide);

        if(pos.X == 0) Console.WriteLine("Ufo spawned incorrectly! Side parameter: " + side);

        hitbox.Position = pos;
        moveDirection = (-side).NormalizedCopy();
        RandomizeDiagonalMovement();
    }

    private void Move()
    {
        hitbox.Offset(moveDirection * speed * dt);
        CollisionOrigin = hitbox.Center;
    }

    private void RandomizeDiagonalMovement()
    {
        diagonalChangeDelay = RandomRange(diagonalChangeDelayRange);
        diagonalMovementInterval = RandomRange(diagonalMovementIntervalRange);
        diagonalMovementTimer = 0;
    }
    
    private void DiagonalMovement()
    {
        //Check if its time for diagonal movement, adding up or down vector randomly
        if (diagonalChangeTimer > diagonalChangeDelay)
        {
            Vector2 up = new Vector2(0,-1);
            Vector2 down = new Vector2(0,1);
            moveDirection += Chance(50) ? up : down;
        }
        diagonalChangeTimer += dt;
        
        //If we are moving diagonaly, then check if its time to move straight again
        if (moveDirection.Y != 0)
        {
            //If its time to move straight, randomize times for next diagonal movement, and make movement straight
            if (diagonalMovementTimer > diagonalMovementInterval)
            {
                moveDirection.Y = 0;
                RandomizeDiagonalMovement();
            }
            
            diagonalChangeTimer = 0f;
            diagonalMovementTimer += dt;
        }
    }
    
    protected override void Update(GameTime gameTime)
    {
        dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        DiagonalMovement();
        Move();
        InBounds(boundsImmersion);
        ShootLoop();
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
    
    private void Shoot()
    {
        if (MainGame.Players.Count == 0)
            return;
        
        Point2 playerCenter = (MainGame.Players.All.Single() as IRadiusCollider).CollisionOrigin;
        
        shootDirection = playerCenter - hitbox.Center;
        shootDirection.Normalize();

        Bullet.UfoBullets.Add(new Bullet(hitbox.Center, shootDirection, false));
    }

    protected override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(texture, (Rectangle)hitbox, Color.White);

        if (MainGame.DebugMode)
        {
            spriteBatch.DrawLine(hitbox.Center, hitbox.Center + (shootDirection * CollisionRadius), Color.Red);
            spriteBatch.DrawCircle(hitbox.Center, CollisionRadius, 16, Color.Red);
        }
    }

    public override void Destroy()
    {
        base.Destroy();
        MainGame.UfoDestroyed();
    }
}