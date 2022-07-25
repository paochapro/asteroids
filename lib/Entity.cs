using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Asteroids;

//Static
abstract partial class Entity
{
    private static int lastUpdatedEntity;
    private static List<Entity> ents = new();
    public static List<Entity> All => ents;

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
    public static void RemoveEntity(Entity ent)
    {
        if (ent.updated) --lastUpdatedEntity;
        ents.Remove(ent);
    }
}

//Main
abstract partial class Entity
{
    private bool updated;

    protected RectangleF hitbox;
    protected Texture2D? texture;
    
    public RectangleF Hitbox => hitbox;
    public Texture2D? Texture => texture;
    
    protected abstract void Update(GameTime gameTime);

    protected virtual void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(texture, (Rectangle)hitbox, Color.White);
    }

    protected Entity(RectangleF hitbox, Texture2D? texture)
    {
        this.hitbox = hitbox;
        this.texture = texture;
        ents.Add(this);
    }

    public Entity() : this(new RectangleF(0, 0, 0, 0), null) { }

    public virtual void Destroy() {}
    
    protected void InBounds(float immersion)
    {
        float iw = hitbox.Size.Width * immersion;
        float ih = hitbox.Size.Height * immersion;
        float sw = MainGame.Screen.X - (hitbox.Size.Width - iw);
        float sh = MainGame.Screen.Y - (hitbox.Size.Height - ih);

        if (hitbox.X < -iw) hitbox.X = sw;
        if (hitbox.Y < -ih) hitbox.Y = sh;
        if (hitbox.X > sw) hitbox.X = -iw;
        if (hitbox.Y > sh) hitbox.Y = -ih;
    }
}

class Group<T> where T : Entity
{
    private static List<T> list = new();
    public static IEnumerable<T> All => list;

    public static int Count => list.Count;
    public static T Get(int i) => list[i];

    public static void Add(T obj) => list.Add(obj);
    
    public static void Remove(T obj)
    {
        list.Remove(obj);
        Entity.RemoveEntity(obj);
    }
    public static void Clear()
    {
        list.ForEach(obj => Entity.RemoveEntity(obj));
        list.Clear();
    }
}