using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Timers;

namespace Asteroids;

class Bullet : Entity
{
    private static readonly Point size = new(2, 2);

    private const float speed = 500f;
        
    private Angle direction;
    private float dt;
    private const float lifeTime = 1f;

    public Bullet(Vector2 pos, Angle direction) : base(new RectangleF(pos, size), null)
    {
        this.direction = direction;
        Event.Add(Destroy, lifeTime);
    }
    
    protected override void Update(GameTime gameTime)
    {
        dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        float x = (float)Math.Cos(direction.Radians);
        float y = (float)-Math.Sin(direction.Radians);
        hitbox.Offset(new Vector2(x,y) * speed * dt);
        
        InBounds();
    }

    protected override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.FillRectangle(hitbox, Color.White);
    }
}