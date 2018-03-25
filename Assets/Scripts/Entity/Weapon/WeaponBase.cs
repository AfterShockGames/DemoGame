#region

using System.Collections.Generic;
using UnityEngine;

#endregion

namespace DemoGame.Entity.Weapon
{
    public abstract class WeaponBase : MonoBehaviour
    {
        public float FireInterval;
        public GameObject FireOrigin;
        public float FireVelocity;
        public byte HealthImpact;
        internal float LastShot;

        public abstract bool CanFire();

        public abstract void Fire();
    }
}