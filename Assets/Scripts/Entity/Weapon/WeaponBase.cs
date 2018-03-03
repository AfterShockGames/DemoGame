using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DemoGame.Entity.Weapon
{
    public abstract class WeaponBase : MonoBehaviour
    {
        public GameObject FireOrigin;
        public float FireInterval;
        public float FireVelocity;
        internal float LastShot;
        public byte HealthImpact;

        public abstract bool CanFire();

        public abstract void Fire();
    }
}