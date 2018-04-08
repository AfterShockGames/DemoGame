using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DemoGame.Interfaces
{
    public interface IWeaponProjectile {
        GameObject Object { get; set; }
        float ImpactForce { get; set; }
        int ProjectileLifetime { get; set; }
    }
}