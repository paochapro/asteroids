﻿using Microsoft.Xna.Framework;
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
    private const float lifeTime = 1f;
    private const float boundsImmersion = 1f;
    private static readonly Color playerColor = Color.White;
    private static readonly Color ufoColor = Color.Red;
    
    private float dt;
    private Vector2 moveDirection;
    private Color color;
    
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
    }

    protected override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawPoint(hitbox.Position, color, size);
    }
    
    public Bullet(Vector2 pos, Vector2 moveDirection, bool playerBullet) : base(new RectangleF(pos, new Point2(size,size)), null)
    {
        color = playerBullet ? playerColor : ufoColor;
        bulletsGroup = playerBullet ? PlayerBullets : UfoBullets;
        
        this.moveDirection = moveDirection.NormalizedCopy();
        Event.Add(Destroy, lifeTime);
    }
}