#region

using UnityEngine;

#endregion

namespace DemoGame.Entity.Weapon
{
    public abstract class WeaponProjectile : WeaponBase
    {
        public float ImpactForce;
        public GameObject Projectile;
        public int ProjectileLifetime;
    }
}