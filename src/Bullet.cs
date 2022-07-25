using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Timers;

namespace Asteroids;

class Bullets : Group<Bullet>
{
    public static void Add(Point2 pos, Angle direction) => Add(new Bullet(pos, direction));
}

class Bullet : Entity
{
    private static readonly Point size = new(2, 2);
    private const float speed = 500f;
    private const float lifeTime = 1f;
    private const float boundsImmersion = 1f;
    
    private Angle angle;
    private float dt;

    private void CheckAsteroidsHit()
    {
        for (int i = 0; i < Asteroids.Count; ++i)
        {
            Asteroid asteroid = Asteroids.Get(i);
            float dist = Vector2.Distance(hitbox.Center, asteroid.Hitbox.Center);

            if (dist < asteroid.Radius)
            {
                asteroid.Destroy();
                this.Destroy();
                return;
            }
        }
    }
    
    private void Move()
    {
        hitbox.Offset(angle.ToUnitVector() * speed * dt);
    }

    protected override void Update(GameTime gameTime)
    {
        dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Move();
        InBounds(boundsImmersion);
        CheckAsteroidsHit();
    }

    protected override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.FillRectangle(hitbox, Color.White);
    }
    
    public Bullet(Vector2 pos, Angle angle) : base(new RectangleF(pos, size), null)
    {
        this.angle = angle;
        Event.Add(Destroy, lifeTime);
    }

    public override void Destroy() => Bullets.Remove(this);
}