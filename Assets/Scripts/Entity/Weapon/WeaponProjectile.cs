using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DemoGame.Entity.Weapon
{
    public abstract class WeaponProjectile : WeaponBase
    {
        public GameObject Projectile;
        public int ProjectileLifetime;
        public float ImpactForce;
    }
}