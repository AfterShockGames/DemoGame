using System.Collections.Generic;
using DemoGame.Entity.Projectile;
using UnityEngine;

namespace DemoGame.Interfaces
{
    public delegate void WeaponProjectileCallback(IProjectile projectile);

    public interface IProjectile
    {
        bool CanImpact { get; set; }
        double CurrentFrame { get; set; }
        double SpawnFrame { get; set; }
        HashSet<GameObject> IgnoreGameObjects { get; set; }

        string Particle { get; set; }
        string ParticleResource { get; set; }

        event WeaponProjectileCallback OnImpact;
    }
}