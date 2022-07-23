using System.Net.NetworkInformation;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Asteroids;

//Static
abstract partial class Entity
{
    private static List<Entity> ents = new();
    private static List<Entity> destroyed = new();
    private static List<Entity> added = new();
    
    public static void UpdateAll(GameTime gameTime)
    {
        added.ForEach(ent => ents.Add(ent));
        added.Clear();
        
        ents.ForEach(ent => ent.Update(gameTime));
        
        destroyed.ForEach(ent => ents.Remove(ent));
        destroyed.Clear();
    }
    public static void DrawAll(SpriteBatch spriteBatch)
    {
        ents.ForEach(ent => ent.Draw(spriteBatch));
    }
}

//Main
abstract partial class Entity
{
    protected RectangleF hitbox;
    protected Texture2D texture;
    
    public RectangleF Hitbox => hitbox;
    public Texture2D Texture => texture;

    protected abstract void Update(GameTime gameTime);

    protected virtual void Draw(SpriteBatch spriteBatch)
    {
        Rectangle final = (Rectangle)hitbox;
        final.Location -= MainGame.Camera.ToPoint();

        spriteBatch.Draw(texture, final, Color.White);
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
    
    public Entity(RectangleF hitbox, Texture2D texture)
    {
        this.hitbox = hitbox;
        this.texture = texture;
        added.Add(this);
    }

    public Entity() : this(new RectangleF(0, 0, 0, 0), null) { }

    public void Destroy() => destroyed.Add(this);
}

