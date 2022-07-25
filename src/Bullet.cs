using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Timers;

namespace Asteroids;

class Bullets : Group<Bullet>
{
    public static void Add(Point2 pos, Vector2 direction, bool playerBullet) => Add(new Bullet(pos, direction, playerBullet));
}

class Bullet : Entity, IRadiusCollider
{
    public Point2 CollisionOrigin { get; private set; }
    public float CollisionRadius { get; init; } = collisionRadius;
    private const float collisionRadius = 2f;
    
    private static readonly int size = 2;
    private const float speed = 800f;
    private const float lifeTime = 1f;
    private const float boundsImmersion = 1f;
    private static readonly Color playerColor = Color.White;
    private static readonly Color ufoColor = Color.Red;
    
    private float dt;
    private bool playerBullet;
    private Vector2 moveDirection;
    private Color color;
    private Action CheckHitFunction;

    private void UfoBulletFunction()
    {
        IRadiusCollider playerCollider = (IRadiusCollider)MainGame.Player;
        
        if (playerCollider.CollidesWith(this))
        {
            MainGame.GameOver();
        }
    }
    
    private void PlayerBulletFunction()
    {
        for (int i = 0; i < Asteroids.Count; ++i)
        {
            Asteroid asteroid = Asteroids.Get(i);
            IRadiusCollider asteroidCollider = (IRadiusCollider)asteroid;

            if (asteroidCollider.CollidesWith(this))
            {
                asteroid.Hit();
                this.Destroy();
                return;
            }
        }
        for (int i = 0; i < Ufos.Count; ++i)
        {
            Ufo ufo = Ufos.Get(i);
            IRadiusCollider ufoCollider = (IRadiusCollider)ufo;

            if (ufoCollider.CollidesWith(this))
            {
                ufo.Destroy();
                this.Destroy();
                return;
            }
        }
    }
    
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
        CheckHitFunction.Invoke();
    }

    protected override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawPoint(hitbox.Position, color, size);
    }
    
    public Bullet(Vector2 pos, Vector2 moveDirection, bool playerBullet) : base(new RectangleF(pos, new Point2(size,size)), null)
    {
        if (playerBullet) {
            color = playerColor;
            CheckHitFunction = PlayerBulletFunction;
        }
        else {
            color = ufoColor;
            CheckHitFunction = UfoBulletFunction;
        }
        
        this.moveDirection = moveDirection.NormalizedCopy();
        Event.Add(Destroy, lifeTime);
    }

    public override void Destroy() => Bullets.Remove(this);
}