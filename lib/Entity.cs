using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Asteroids;

//Static
abstract partial class Entity
{
    private static int lastUpdatedEntity;
    private static List<Entity> ents = new();
    public static List<Entity> List => ents;

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

    protected Entity(RectangleF hitbox, Texture2D texture)
    {
        this.hitbox = hitbox;
        this.texture = texture;
        ents.Add(this);
    }

    public Entity() : this(new RectangleF(0, 0, 0, 0), null) { }

    public virtual void Destroy()
    {
        if (updated) --lastUpdatedEntity;
        ents.Remove(this);
    }
    
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

/*class Group<T> where T : Entity
{
    protected Group() {}
    protected static Entities ents;
    
    protected List<T> list = new();
    public List<T> All => list;
    public int Count => list.Count;

    public Group(Entities entities) => ents = entities;
 
    public void Clear() => list.Clear();

    public void Add(T ent)
    {
        list.Add(ent);
        ents.Add(ent);
    }
    public void Remove(T ent)
    {
        list.Remove(ent);
        ents.Remove(ent);
    }
}*/


/*
class Entities : Group<Entity>
{
    private static int lastUpdatedEntity;
    
    public static Entities Create()
    {
        if (ents == null) ents = new Entities();
        return ents;
    }
    
    private Entities() {}
    
    public void UpdateAll(GameTime gameTime)
    {
        list.ForEach(ent => ent.updated = false);
        lastUpdatedEntity = -1;
        
        while(lastUpdatedEntity+1 < ents.Count)
        {
            list[++lastUpdatedEntity].updated = true;
            list[lastUpdatedEntity].Update(gameTime);
        }
    }
    public void DrawAll(SpriteBatch spriteBatch)
    {
        foreach (Entity ent in list)
            ent.Draw(spriteBatch);
    }
}
*/