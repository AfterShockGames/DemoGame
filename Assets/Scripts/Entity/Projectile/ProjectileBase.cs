using UnityEngine;
using System.Collections.Generic;
using DemoGame.Player;

namespace DemoGame.Entity.Projectile
{

    public delegate void WeaponProjectileCallback(ProjectileBase projectile);

    public class ProjectileBase : MonoBehaviour
    {
        internal int _frameLifetime;
        internal double _spawnFrame;
        internal double _killFrame;
        internal double _currentFrame;

        internal Vector3 _origin;
        internal Vector3 _velocity;

        internal byte _healthImpact = 100;
        internal float _impactForce = 25F;
        internal float _explosionRadius = 5f;

        internal string _impactParticle;
        internal string _impactParticleResource;

        internal bool _canImpact = true;

        internal HashSet<GameObject> _ignoreGameObjects = new HashSet<GameObject>();

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
            this._ignoreGameObjects.Add(ignoreGameObject);
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
            {
                for (int i = 0; i < hits.Count; i++)
                {
                    var hit = hits[i];

                    var distance = (hit.transform.position - gameObject.transform.position).magnitude;

                    if (UnityEngine.Network.isServer)
                    {
                        // apply indirect damage
                        var healthImpact = Mathf.Max(0, (1f - (distance / _explosionRadius)) * _healthImpact);
                        //hit.gameObject.transform.root.GetComponent<Controller>().ApplyDamage((byte)healthImpact);
                    }

                    // apply force from indirect hit
                    var multiplier = distance / _explosionRadius;

                    //hit.gameObject.transform.root.GetComponent<Motor>()
                    //    .AddImpact(hit.gameObject.transform.position - hitPoint, _impactForce * multiplier);
                }
            }
        }


        public Vector3 GetPositionAtFrame(double frame)
        {
            float totalDelta = (float)(frame - this._spawnFrame);
            return
                this._origin +
                (this._velocity * UnityEngine.Time.fixedDeltaTime * totalDelta);
        }

        void FixedUpdate()
        {
            if (_canImpact)
            {
                double serverFrame = UnityEngine.Network.time;
                if (serverFrame > this._killFrame)
                    KillFrameReached();

                for (; this._currentFrame < serverFrame; this._currentFrame++)
                    this.ResolveCollisions(this._currentFrame, UnityEngine.Time.fixedDeltaTime);

                this.transform.position =
                    this.GetPositionAtFrame(serverFrame);
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