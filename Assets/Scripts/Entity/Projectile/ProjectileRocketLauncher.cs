using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace DemoGame.Entity.Projectile
{
    public class ProjectileRocketlauncher : ProjectileBase
    {
        protected override void KillFrameReached()
        {
            Debug.Log("killframe reached");
            var frame = this._currentFrame;
            Vector3 hitPoint = this.GetPositionAtFrame(frame);

            this.OnHitDirect(new RaycastHit(), hitPoint);

            // needs referal
            OnHitIndirect(UnityEngine.Physics.OverlapSphere(hitPoint, _explosionRadius).ToList(), hitPoint);

            base.KillFrameReached();
        }

        // Check for collisions via 2 methods
        // 1 - Network collisions via hit boxes
        // 2 - Non network collisions (static non moving objects, i.e. mesh collider)
        protected override void ResolveCollisions(double frame, float frameDeltaTime)
        {
            // 1 - Network collision
            bool collided = false;

            Vector3 origin = this.GetPositionAtFrame(frame);
            float distance = (this._velocity * frameDeltaTime).magnitude;

            Ray ray = new Ray(origin, this._velocity.normalized);
            List<RaycastHit> hits = UnityEngine.Physics.RaycastAll(ray).ToList();

            Vector3 hitPoint = origin;

            for (int i = 0; i < hits.Count; i++)
            {
                RaycastHit hit = hits[i];

                if (this._ignoreGameObjects.Contains(hit.collider.gameObject) == false)
                {
                    if (hit.distance < distance)
                    {
                        collided = true;
                        this.OnHitDirect(hit, origin + (ray.direction * hit.distance));
                    }
                }
            }

            // 2 - Non network collision (only process if hits nothing)
            if (!collided)
            {
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, distance, (1 << LayerMask.NameToLayer("world"))))
                {
                    hitPoint = hit.point;
                    // hit the world
                    this.OnHitDirect(new RaycastHit(), hit.point);
                    collided = true;
                }
            }

            if (collided)
            {
                var indirectHits = UnityEngine.Physics.OverlapSphere(hitPoint, _explosionRadius).ToList();
                OnHitIndirect(indirectHits, hitPoint);
            }
        }

        internal override void OnHitDirect(RaycastHit hit, Vector3 position)
        {
            if (_canImpact)
            {
                base.OnHitDirect(hit, position);

                /*var impact = Instantiate(_impactParticle);

                impact.transform.position = position;
                impact.transform.rotation = Quaternion.identity;

                PoolingManager.Instance.Despawn("PROJECTILE_ROCKETLAUNCHER", this.gameObject);*/
            }
        }
    }
}