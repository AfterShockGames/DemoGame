using System.Collections;
using System.Collections.Generic;
using DemoGame.Entity.Projectile;
using UnityEngine;

namespace DemoGame.Entity.Weapon
{
    public class RocketLauncher : WeaponProjectile
    {
        public override bool CanFire()
        {
            return Time.time - LastShot >= FireInterval ? true : false;
        }

        public override void Fire()
        {
            if (Projectile)
            {
                var forward = transform.forward;
                var originPosition = FireOrigin.transform.position;

                var instance = Instantiate(Projectile, originPosition, Quaternion.identity);

                var projectileRocketlauncher = instance.GetComponent<ProjectileRocketlauncher>();
                projectileRocketlauncher
                    .SetCurrentFrame(UnityEngine.Network.time)
                    .SetFrameLifetime(ProjectileLifetime)
                    .SetSpawnFrame(UnityEngine.Network.time)
                    .SetExplosionRadius(10.0f)
                    .SetCanImpact(true)
                    .SetHealthImpact(HealthImpact)
                    .SetOrigin(originPosition)
                    .SetImpactForce(ImpactForce)
                    .SetVelocity(forward * FireVelocity);

                LastShot = Time.time;
            }
        }
    }
}