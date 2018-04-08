#region

using DemoGame.Entity.Weapon.bak;
using UnityEngine;

#endregion

namespace DemoGame.Entity.Weapon.bak
{
    public abstract class WeaponProjectile : WeaponBase
    {
        public float ImpactForce;
        public GameObject Projectile;
        public int ProjectileLifetime;
    }
}