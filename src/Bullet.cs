using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Timers;

namespace Asteroids;

class Bullet : Entity, IRadiusCollider
{
    public static Group<Bullet> PlayerBullets { get; private set; } = new(); 
    public static Group<Bullet> UfoBullets { get; private set; } = new();
    private Group<Bullet> bulletsGroup;

    public Point2 CollisionOrigin { get; private set; }
    public float CollisionRadius { get; init; } = collisionRadius;
    private const float collisionRadius = 2f;
    
    private static readonly int size = 2;
    private const float speed = 800f;
    private const float lifetime = 1f;
    private const float boundsImmersion = 1f;

    private float dt;
    private Vector2 moveDirection;
    private Color color;
    private float lifetimeTimer = 0f;
    
    private void Move()
    {
        hitbox.Offset(moveDirection * speed * dt);
        CollisionOrigin = hitbox.Center;
    }

    protected override void Update(GameTime gameTime)
    {
        dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Move();
        InBounds(boundsImmersion);

        lifetimeTimer += dt;
        
        if (lifetimeTimer > lifetime)
            Destroy();
    }

    protected override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawPoint(hitbox.Position, Color.White, size);
    }
    
    public Bullet(Vector2 pos, Vector2 moveDirection, bool playerBullet) : base(new RectangleF(pos, new Point2(size,size)), null)
    {
        bulletsGroup = playerBullet ? PlayerBullets : UfoBullets;
        this.moveDirection = moveDirection.NormalizedCopy();
    }
}