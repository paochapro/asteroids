using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Asteroids;

interface IRadiusCollider
{
    public float CollisionRadius { get; }
    public Point2 CollisionOrigin { get; }

    public bool CollidesWith(IRadiusCollider collider)
    {
        float dist = Vector2.Distance(CollisionOrigin, collider.CollisionOrigin);

        if (dist < collider.CollisionRadius + CollisionRadius)
            return true;

        return false;
    }
}

static class Collisions
{
    public static void Update()
    {
        AsteroidCollision();
        UfoCollision();
        BulletCollision();
    }
    
    private static void AsteroidCollision()
    {
        Asteroid.Group.Iterate(asteroid =>
        {
            MainGame.Players.Iterate(player =>
            {
                IRadiusCollider playerCollider = player as IRadiusCollider;
                if (playerCollider.CollidesWith(asteroid))
                {
                    MainGame.Death();
                    return;
                }
            });

            Ufo.Group.Iterate(ufo =>
            {
                IRadiusCollider ufoCollider = (IRadiusCollider)ufo;

                if (ufoCollider.CollidesWith(asteroid))
                {
                    ufo.Hit(false);
                    asteroid.Hit(false);
                }
            });
        });
    }
    
    private static void UfoCollision()
    {
        Ufo.Group.Iterate(ufo =>
        {
            MainGame.Players.Iterate(player =>
            {
                IRadiusCollider playerCollider = player as IRadiusCollider;
                if (playerCollider.CollidesWith(ufo))
                {
                    MainGame.Death();
                    return;
                }
            });
        });
    }
    
    private static void BulletCollision()
    {
        void UfoBullet(Bullet bullet)
        {
            MainGame.Players.Iterate(player =>
            {
                IRadiusCollider playerCollider = player as IRadiusCollider;
                if (playerCollider.CollidesWith(bullet))
                {
                    MainGame.Death();
                    return;
                }
            });
            
            AsteroidCollision(bullet, false);
        }
        void PlayerBullet(Bullet bullet)
        {
            Ufo.Group.Iterate(ufo =>
            {
                IRadiusCollider ufoCollider = (IRadiusCollider)ufo;

                if (ufoCollider.CollidesWith(bullet))
                {
                    ufo.Hit(true);
                    bullet.Destroy();
                    return;
                }
            });
            
            AsteroidCollision(bullet, true);
        }
        void AsteroidCollision(Bullet bullet, bool isPlayerBullet)
        {
            Asteroid.Group.Iterate(asteroid =>
            {
                IRadiusCollider asteroidCollider = (IRadiusCollider)asteroid;

                if (asteroidCollider.CollidesWith(bullet))
                {
                    asteroid.Hit(isPlayerBullet);
                    bullet.Destroy();
                    return;
                }
            });
        }
        Bullet.PlayerBullets.Iterate(PlayerBullet);
        Bullet.UfoBullets.Iterate(UfoBullet);
    }
}