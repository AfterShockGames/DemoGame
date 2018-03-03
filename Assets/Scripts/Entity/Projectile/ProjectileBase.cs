#region

using System.Collections.Generic;
using UnityEngine;

#endregion

namespace DemoGame.Entity.Projectile
{
    public delegate void WeaponProjectileCallback(ProjectileBase projectile);

    public class ProjectileBase : MonoBehaviour
    {
        internal bool CanImpact = true;
        internal double CurrentFrame;
        internal float ExplosionRadius = 5f;
        internal int FrameLifetime;

        internal byte HealthImpact = 100;

        internal HashSet<GameObject> IgnoreGameObjects = new HashSet<GameObject>();
        internal float ImpactForce = 25F;

        internal string ImpactParticle;
        internal string ImpactParticleResource;
        internal double KillFrame;

        internal Vector3 Origin;
        internal double SpawnFrame;
        internal Vector3 Velocity;

        public event WeaponProjectileCallback OnImpact;

        public virtual ProjectileBase Init()
        {
            return this;
        }

        public ProjectileBase SetHealthImpact(byte healthImpact)
        {
            HealthImpact = healthImpact;
            return this;
        }


        public ProjectileBase SetSpawnFrame(int spawnFrame)
        {
            SpawnFrame = spawnFrame;
            KillFrame = spawnFrame + FrameLifetime;
            return this;
        }

        public ProjectileBase SetIgnoreGameObjects(HashSet<GameObject> ignoreGameObjects)
        {
            IgnoreGameObjects = ignoreGameObjects;
            return this;
        }

        public ProjectileBase SetOrigin(Vector3 origin)
        {
            Origin = origin;
            transform.position = Origin;
            return this;
        }

        public ProjectileBase SetVelocity(Vector3 velocity)
        {
            Velocity = velocity;
            return this;
        }

        public ProjectileBase SetCanImpact(bool canImpact)
        {
            CanImpact = canImpact;
            return this;
        }

        public ProjectileBase SetKillFrame(int killFrame)
        {
            KillFrame = killFrame;
            return this;
        }

        public ProjectileBase SetCurrentFrame(int currentFrame)
        {
            CurrentFrame = currentFrame;
            return this;
        }

        public ProjectileBase SetFrameLifetime(int frameLifetime)
        {
            FrameLifetime = frameLifetime;
            KillFrame = SpawnFrame + FrameLifetime;
            return this;
        }

        public ProjectileBase SetImpactParticle(string particle)
        {
            ImpactParticle = particle;
            return this;
        }

        public ProjectileBase SetExplosionRadius(float explosionRadius)
        {
            ExplosionRadius = explosionRadius;
            return this;
        }

        public ProjectileBase SetImpactForce(float impactForce)
        {
            ImpactForce = impactForce;
            return this;
        }

        private void PlayFire()
        {
            //this.asset.PlayFire();
        }

        public ProjectileBase IgnoreGameObject(GameObject ignoreGameObject)
        {
            IgnoreGameObjects.Add(ignoreGameObject);
            return this;
        }

        internal virtual void OnHitDirect(RaycastHit hit, Vector3 position)
        {
            if (OnImpact != null)
                OnImpact(this);


            /*if (AftershockState.IsServer)
            {
                if (hit.gameObject != null)
                    hit.gameObject.GetComponent<Controller>().ApplyDamage(_healthImpact);
            }*/
        }

        internal void OnHitIndirect(List<Collider> hits, Vector3 hitPoint)
        {
            if (!CanImpact)
                return;

            foreach (var hit in hits)
            {
                var distance = (hit.transform.position - gameObject.transform.position).magnitude;

                if (UnityEngine.Network.isServer)
                {
                    // apply indirect damage
                    var healthImpact = Mathf.Max(0, (1f - distance / ExplosionRadius) * HealthImpact);
                    //hit.gameObject.transform.root.GetComponent<Controller>().ApplyDamage((byte)healthImpact);
                }

                // apply force from indirect hit
                var multiplier = distance / ExplosionRadius;

                //hit.gameObject.transform.root.GetComponent<Motor>()
                //    .AddImpact(hit.gameObject.transform.position - hitPoint, _impactForce * multiplier);
            }
        }


        public Vector3 GetPositionAtFrame(double frame)
        {
            var totalDelta = (float) (frame - SpawnFrame);

            return
                Origin +
                Velocity * Time.fixedDeltaTime * totalDelta;
        }

        private void FixedUpdate()
        {
            if (!CanImpact)
                return;

            var serverFrame = UnityEngine.Network.time;
            if (serverFrame > KillFrame)
                KillFrameReached();

            for (; CurrentFrame < serverFrame; CurrentFrame++)
                ResolveCollisions(CurrentFrame, Time.fixedDeltaTime);

            transform.position =
                GetPositionAtFrame(serverFrame);
        }

        protected virtual void KillFrameReached()
        {
        }

        protected virtual void ResolveCollisions(double frame, float frameDeltaTime)
        {
        }
    }
}