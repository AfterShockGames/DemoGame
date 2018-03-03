using System.Linq;
using UnityEngine;

namespace DemoGame.Entity.Projectile
{
    public class ProjectileRocketlauncher : ProjectileBase
    {
        protected override void KillFrameReached()
        {
            Debug.Log("killframe reached");
            var frame = _currentFrame;
            var hitPoint = GetPositionAtFrame(frame);

            OnHitDirect(new RaycastHit(), hitPoint);

            // needs referal
            OnHitIndirect(Physics.OverlapSphere(hitPoint, _explosionRadius).ToList(), hitPoint);

            base.KillFrameReached();
        }

        // Check for collisions via 2 methods
        // 1 - Network collisions via hit boxes
        // 2 - Non network collisions (static non moving objects, i.e. mesh collider)
        protected override void ResolveCollisions(double frame, float frameDeltaTime)
        {
            // 1 - Network collision
            var collided = false;

            var origin = GetPositionAtFrame(frame);
            var distance = (_velocity * frameDeltaTime).magnitude;

            var ray = new Ray(origin, _velocity.normalized);
            var hits = Physics.RaycastAll(ray).ToList();

            var hitPoint = origin;

            for (var i = 0; i < hits.Count; i++)
            {
                var hit = hits[i];

                if (_ignoreGameObjects.Contains(hit.collider.gameObject) == false)
                    if (hit.distance < distance)
                    {
                        collided = true;
                        OnHitDirect(hit, origin + ray.direction * hit.distance);
                    }
            }

            // 2 - Non network collision (only process if hits nothing)
            if (!collided)
            {
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, distance, 1 << LayerMask.NameToLayer("world")))
                {
                    hitPoint = hit.point;
                    // hit the world
                    OnHitDirect(new RaycastHit(), hit.point);
                    collided = true;
                }
            }

            if (collided)
            {
                var indirectHits = Physics.OverlapSphere(hitPoint, _explosionRadius).ToList();
                OnHitIndirect(indirectHits, hitPoint);
            }
        }

        internal override void OnHitDirect(RaycastHit hit, Vector3 position)
        {
            if (_canImpact)
                base.OnHitDirect(hit, position);
        }
    }
}