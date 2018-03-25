#region

using System;
using System.Linq;
using UnityEngine;

#endregion

namespace DemoGame.Entity.Projectile
{
    public class ProjectileRocketlauncher : ProjectileBase
    {
        protected override void KillFrameReached()
        {
            Debug.Log("killframe reached");
            var frame = CurrentFrame;
            var hitPoint = GetPositionAtFrame(frame);

            OnHitDirect(new RaycastHit(), hitPoint);

            // needs referal
            OnHitIndirect(UnityEngine.Physics.OverlapSphere(hitPoint, ExplosionRadius).ToList(), hitPoint);

            Destroy(gameObject);

            base.KillFrameReached();

            Destroy(gameObject);
        }

        // Check for collisions via 2 methods
        // 1 - Network collisions via hit boxes
        // 2 - Non network collisions (static non moving objects, i.e. mesh collider)
        protected override void ResolveCollisions(double frame, float frameDeltaTime)
        {
            // 1 - Network collision
            var collided = false;

            var origin = GetPositionAtFrame(frame);
            var distance = (Velocity * frameDeltaTime).magnitude;

            var ray = new Ray(origin, Velocity.normalized);
            var hits = UnityEngine.Physics.RaycastAll(ray).ToList();

            var hitPoint = origin;

            foreach (var hit in hits)
            {
                if (IgnoreGameObjects.Contains(hit.collider.gameObject) || hit.distance >= distance)
                    continue;

                collided = true;
                OnHitDirect(hit, origin + ray.direction * hit.distance);
            }

            // 2 - Non network collision (only process if hits nothing)
            if (!collided)
            {
                RaycastHit hit;

                if (UnityEngine.Physics.Raycast(ray, out hit, distance, 1 << LayerMask.NameToLayer("world")))
                {
                    hitPoint = hit.point;
                    // hit the world
                    OnHitDirect(new RaycastHit(), hit.point);
                    collided = true;
                }
            }
            else
            {
                var indirectHits = UnityEngine.Physics.OverlapSphere(hitPoint, ExplosionRadius).ToList();
                OnHitIndirect(indirectHits, hitPoint);
            }

            if (collided)
                Destroy(gameObject);
        }

        internal override void OnHitDirect(RaycastHit hit, Vector3 position)
        {
            if (CanImpact)
                base.OnHitDirect(hit, position);
        }
    }
}