using System.Collections.Generic;
using UnityEngine;

namespace DemoGame.Entity.Projectile
{
    public delegate void WeaponProjectileCallback(ProjectileBase projectile);

    public class ProjectileBase : MonoBehaviour
    {
        internal bool _canImpact = true;
        internal double _currentFrame;
        internal float _explosionRadius = 5f;
        internal int _frameLifetime;

        internal byte _healthImpact = 100;

        internal HashSet<GameObject> _ignoreGameObjects = new HashSet<GameObject>();
        internal float _impactForce = 25F;

        internal string _impactParticle;
        internal string _impactParticleResource;
        internal double _killFrame;

        internal Vector3 _origin;
        internal double _spawnFrame;
        internal Vector3 _velocity;

        public event WeaponProjectileCallback OnImpact;

        public virtual ProjectileBase Init()
        {
            return this;
        }

        public ProjectileBase SetHealthImpact(byte healthImpact)
        {
            _healthImpact = healthImpact;
            return this;
        }


        public ProjectileBase SetSpawnFrame(int spawnFrame)
        {
            _spawnFrame = spawnFrame;
            _killFrame = spawnFrame + _frameLifetime;
            return this;
        }

        public ProjectileBase SetIgnoreGameObjects(HashSet<GameObject> ignoreGameObjects)
        {
            _ignoreGameObjects = ignoreGameObjects;
            return this;
        }

        public ProjectileBase SetOrigin(Vector3 origin)
        {
            _origin = origin;
            transform.position = _origin;
            return this;
        }

        public ProjectileBase SetVelocity(Vector3 velocity)
        {
            _velocity = velocity;
            return this;
        }

        public ProjectileBase SetCanImpact(bool canImpact)
        {
            _canImpact = canImpact;
            return this;
        }

        public ProjectileBase SetKillFrame(int killFrame)
        {
            _killFrame = killFrame;
            return this;
        }

        public ProjectileBase SetCurrentFrame(int currentFrame)
        {
            _currentFrame = currentFrame;
            return this;
        }

        public ProjectileBase SetFrameLifetime(int frameLifetime)
        {
            _frameLifetime = frameLifetime;
            _killFrame = _spawnFrame + _frameLifetime;
            return this;
        }

        public ProjectileBase SetImpactParticle(string particle)
        {
            _impactParticle = particle;
            return this;
        }

        public ProjectileBase SetExplosionRadius(float explosionRadius)
        {
            _explosionRadius = explosionRadius;
            return this;
        }

        public ProjectileBase SetImpactForce(float impactForce)
        {
            _impactForce = impactForce;
            return this;
        }

        private void PlayFire()
        {
            //this.asset.PlayFire();
        }

        public ProjectileBase IgnoreGameObject(GameObject ignoreGameObject)
        {
            _ignoreGameObjects.Add(ignoreGameObject);
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
            if (_canImpact)
                for (var i = 0; i < hits.Count; i++)
                {
                    var hit = hits[i];

                    var distance = (hit.transform.position - gameObject.transform.position).magnitude;

                    if (UnityEngine.Network.isServer)
                    {
                        // apply indirect damage
                        var healthImpact = Mathf.Max(0, (1f - distance / _explosionRadius) * _healthImpact);
                        //hit.gameObject.transform.root.GetComponent<Controller>().ApplyDamage((byte)healthImpact);
                    }

                    // apply force from indirect hit
                    var multiplier = distance / _explosionRadius;

                    //hit.gameObject.transform.root.GetComponent<Motor>()
                    //    .AddImpact(hit.gameObject.transform.position - hitPoint, _impactForce * multiplier);
                }
        }


        public Vector3 GetPositionAtFrame(double frame)
        {
            var totalDelta = (float) (frame - _spawnFrame);
            return
                _origin +
                _velocity * Time.fixedDeltaTime * totalDelta;
        }

        private void FixedUpdate()
        {
            if (_canImpact)
            {
                var serverFrame = UnityEngine.Network.time;
                if (serverFrame > _killFrame)
                    KillFrameReached();

                for (; _currentFrame < serverFrame; _currentFrame++)
                    ResolveCollisions(_currentFrame, Time.fixedDeltaTime);

                transform.position =
                    GetPositionAtFrame(serverFrame);
            }
        }

        protected virtual void KillFrameReached()
        {
        }

        protected virtual void ResolveCollisions(double frame, float frameDeltaTime)
        {
        }
    }
}