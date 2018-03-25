#region

using System.Collections.Generic;
using DemoGame.Entity.Projectile;
using UnityEngine;

#endregion

namespace DemoGame.Entity.Weapon
{
    public class RocketLauncher : WeaponProjectileExplosion
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

                // Calculate forward vector of aiming camera 

                var projectileRotation = Quaternion.LookRotation(forward);

                var instance = Instantiate(Projectile, originPosition, projectileRotation);

                var projectileRocketlauncher = instance.GetComponent<ProjectileRocketlauncher>();

                projectileRocketlauncher
                    .SetCurrentFrame(UnityEngine.Network.time)
                    .SetFrameLifetime(ProjectileLifetime)
                    .SetSpawnFrame(UnityEngine.Network.time)
                    .SetExplosionRadius(ExplosionRadius)
                    .SetCanImpact(true)
                    .SetHealthImpact(HealthImpact)
                    .SetOrigin(originPosition)
                    .SetImpactForce(ImpactForce)
                    .SetIgnoreGameObjects(new HashSet<GameObject>() {gameObject, FireOrigin.gameObject})
                    .SetVelocity(forward * FireVelocity);

                LastShot = Time.time;
            }
        }
    }
}