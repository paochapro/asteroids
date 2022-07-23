using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Asteroids;

//Static
abstract partial class Entity
{
    private static int lastUpdatedEntity;
    private static List<Entity> ents = new();

    public static void UpdateAll(GameTime gameTime)
    {
        ents.ForEach(ent => ent.updated = false);
        lastUpdatedEntity = -1;
        
        while(lastUpdatedEntity+1 < ents.Count)
        {
            ents[++lastUpdatedEntity].updated = true;
            ents[lastUpdatedEntity].Update(gameTime);
        }
    }
    public static void DrawAll(SpriteBatch spriteBatch)
    {
        foreach (Entity ent in ents)
            ent.Draw(spriteBatch);
    }
}

//Main
abstract partial class Entity
{
    private bool updated;
    
    protected RectangleF hitbox;
    protected Texture2D texture;
    
    public RectangleF Hitbox => hitbox;
    public Texture2D Texture => texture;

    protected abstract void Update(GameTime gameTime);

    protected virtual void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(texture, (Rectangle)hitbox, Color.White);
    }
    
    protected void InBounds()
    {
        float w = hitbox.Size.Width * 0.5f;
        float h = hitbox.Size.Height * 0.5f;
        float sw = MainGame.Screen.X - w;
        float sh = MainGame.Screen.Y - h;

        if (hitbox.X < -w) hitbox.X = sw;
        if (hitbox.Y < -h) hitbox.Y = sh;
        if (hitbox.X > sw) hitbox.X = -w;
        if (hitbox.Y > sh) hitbox.Y = -h;
    }
    
    protected Entity(RectangleF hitbox, Texture2D texture)
    {
        this.hitbox = hitbox;
        this.texture = texture;
        ents.Add(this);
    }

    public Entity() : this(new RectangleF(0, 0, 0, 0), null) { }


    public virtual void Destroy() => EntityDestroy();
    
    protected void DestroyWithList<T>(List<T> list) where T : Entity
    {
        list.Remove(this as T);
        EntityDestroy();
    }
    
    protected void EntityDestroy()
    {
        if (updated) --lastUpdatedEntity;
        ents.Remove(this);
    }
}

