using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DemoGame.Entity.Weapon
{
    public class WeaponProjectile : WeaponBase
    {
        public GameObject Projectile;
        public int ProjectileLifetime;
        public float ImpactForce;

        public override bool CanFire()
        {
            throw new System.NotImplementedException();
        }

        public override void Fire()
        {
            throw new System.NotImplementedException();
        }
    }
}