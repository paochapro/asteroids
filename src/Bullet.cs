using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Timers;

namespace Asteroids;

class Bullet : Entity
{
    private static readonly Point size = new(2, 2);
    private const float speed = 500f;
    
    private static List<Bullet> list = new();
    public static List<Bullet> List => list;

    public static void Add(Point2 pos, Angle direction) => list.Add(new Bullet(pos, direction));

    private Angle direction;
    private float dt;
    private const float lifeTime = 1f;

    private void CheckAsteroidsHit()
    {
        for (int i = 0; i < Asteroid.List.Count; ++i)
        {
            Asteroid asteroid = Asteroid.List[i];
            float dist = Vector2.Distance(hitbox.Center, asteroid.Hitbox.Center);

            if (dist < asteroid.Radius)
            {
                asteroid.Destroy();
                this.Destroy();
            }
        }
    }
    
    private void Move()
    {
        float x = (float)Math.Cos(direction.Radians);
        float y = (float)-Math.Sin(direction.Radians);
        hitbox.Offset(new Vector2(x,y) * speed * dt);
    }

    protected override void Update(GameTime gameTime)
    {
        dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Move();
        InBounds();
        CheckAsteroidsHit();
    }

    protected override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.FillRectangle(hitbox, Color.White);
    }
    
    private Bullet(Vector2 pos, Angle direction) : base(new RectangleF(pos, size), null)
    {
        this.direction = direction;
        Event.Add(Destroy, lifeTime);
    }

    public override void Destroy() => DestroyWithList(list);
}