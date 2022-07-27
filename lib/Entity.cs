using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Asteroids;

//Static
abstract partial class Entity
{
    private static List<Entity> ents = new();
    public static int Count => ents.Count;

    private static int updatePosition = 0;

    public static void UpdateAll(GameTime gameTime)
    {
        updatePosition = 0;
        
        while(updatePosition < ents.Count)
        {
            ents[updatePosition++].Update(gameTime);
        }
    }
    public static void DrawAll(SpriteBatch spriteBatch)
    {
        ents.ForEach(ent => ent.Draw(spriteBatch));
    }
    public static void AddEntity(Entity ent)
    {
        ent.index = ents.Count;
        ents.Add(ent);
    }
    public static void RemoveEntity(Entity ent)
    {
        int index = ent.index;
        
        //Removing from entities
        //If entity was updated, update position should be lowered
        if (index <= updatePosition) --updatePosition;

        try
        {
            ents.RemoveAt(index);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Console.WriteLine("AOOE Exception in Entity/RemoveEntity: " + ex.Message);
            Console.WriteLine("{");
            Console.WriteLine("\t Index Var: " + index);
            Console.WriteLine("\t Index Entity: " + ent.index);
            Console.WriteLine("\t Ents Count: " + ents.Count);
            Console.WriteLine("}");
        }
        
        //Updating indexing
        for (int i = index; i < ents.Count; ++i)
            --(ents[i].index);

    }
    public static void RemoveAll()
    {
        foreach (GroupBase group in GroupBase.AllGroups)
            group.Clear();
        
        updatePosition = 0;
        ents.Clear();
    }
}

//Main
abstract partial class Entity
{
    private bool destroyed = false;
    private int index;
    public int Index => index;
    
    public Dictionary<GroupBase, int> group_index { get; private set; } = new();

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
    }

    public Entity() : this(new RectangleF(0, 0, 0, 0), null) { }

    public virtual void Destroy()
    {
        if (destroyed) return;
        destroyed = true;
        
        //Removing entity from all groups that its belongs to
        foreach (var kv in group_index)
            kv.Key.Remove(this);

        RemoveEntity(this);
    }
    
    public void InBounds(float immersion)
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

abstract class GroupBase
{
    private static List<GroupBase> groups = new();
    public static IEnumerable<GroupBase> AllGroups => groups; 

    private static int lastGroupIndex { get; set; }
    protected int groupIndex { get; private set; }
    
    public GroupBase()
    {
        groupIndex = lastGroupIndex++;
        groups.Add(this);    
    }

    public abstract void Remove(Entity item);
    public abstract void Clear();
}

class Group<T> : GroupBase where T : Entity
{
    private int iteratePosition = 0;
    
    public T this[int i] => list[i];
    
    private List<T> list = new();
    public IEnumerable<T> All => list;
    public int Count => list.Count;
    
    public void Iterate(Action<T> func)
    {
        iteratePosition = 0;
        
        while (iteratePosition < Count)
        {
            T item = this[iteratePosition++];
            func.Invoke(item);
        }
    }
    public void Add(T item)
    {
        int index = Count;
        
        item.group_index.Add(this, index); //Adding group index to item
        list.Add(item);                    //Adding to the list
        
        //Adding to the entities
        Entity.AddEntity(item);                  
    }
    public override void Remove(Entity item)
    {
        int index = item.group_index[this];
        
        //If item was updated, iteration position should be lowered
        if (index <= iteratePosition) 
            --iteratePosition;
        
        try
        {
            //Removing group from item, and removing item from group
            list.RemoveAt(index);
            item.group_index.Remove(this);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Console.WriteLine("AOOE Exception in Group/Remove: " + ex.Message);
            Console.WriteLine("{");
            Console.WriteLine("\t Group: " + this);
            Console.WriteLine("\t Index var: " + index);
            Console.WriteLine("\t Index group: " + item.group_index[this]);
            Console.WriteLine("\t List count: " + Count);
            Console.WriteLine("}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Other Exception: " + ex.Message);
        }
        
        //Updating indexing
        for (int i = index; i < Count; ++i)
            --(list[i].group_index[this]);
    }
    public override void Clear()
    {
        list.ForEach(ent => ent.group_index.Remove(this)); //Removing group from all members
        list.Clear(); //Clear the list
    }
    
    public IEnumerator<T> GetEnumerator() => list.GetEnumerator();
    public override string ToString() => $"group{groupIndex}-{typeof(T).GetTypeInfo().Name}";
}