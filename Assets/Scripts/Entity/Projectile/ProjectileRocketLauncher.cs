#region

using System;
using System.Collections.Generic;
using System.Linq;
using DemoGame.Interfaces;
using UnityEngine;

#endregion

namespace DemoGame.Entity.Projectile
{
    public class ProjectileRocketLauncher : MonoBehaviour, IProjectile, IHealth, IConstantForce, IExplosion,
        IHealthImpact, ITimer
    {
        [SerializeField] private bool _canImpact = true;
        [SerializeField] private double _currentFrame;
        [SerializeField] private double _spawnFrame;
        [SerializeField] private HashSet<GameObject> _ignoreGameObjects;
        [SerializeField] private string _particle;
        [SerializeField] private string _particleResource;
        [SerializeField] private float _current;
        [SerializeField] private float _min;
        [SerializeField] private float _max;
        [SerializeField] private Vector3 _origin;
        [SerializeField] private Vector3 _velocity;
        [SerializeField] private float _radius;
        [SerializeField] private float _impact;
        [SerializeField] private double _start;
        [SerializeField] private double _duration;
        [SerializeField] private double _progress;

        public bool CanImpact
        {
            get { return _canImpact; }
            set { _canImpact = value; }
        }

        public double CurrentFrame
        {
            get { return _currentFrame; }
            set { _currentFrame = value; }
        }

        public double SpawnFrame
        {
            get { return _spawnFrame; }
            set { _spawnFrame = value; }
        }

        public HashSet<GameObject> IgnoreGameObjects
        {
            get { return _ignoreGameObjects; }
            set { _ignoreGameObjects = value; }
        }

        public string Particle
        {
            get { return _particle; }
            set { _particle = value; }
        }

        public string ParticleResource
        {
            get { return _particleResource; }
            set { _particleResource = value; }
        }

        public float Current
        {
            get { return _current; }
            set { _current = value; }
        }

        public float Min
        {
            get { return _min; }
            set { _min = value; }
        }

        public float Max
        {
            get { return _max; }
            set { _max = value; }
        }

        public Vector3 Origin
        {
            get { return _origin; }
            set { _origin = value; }
        }

        public Vector3 Velocity
        {
            get { return _velocity; }
            set { _velocity = value; }
        }

        public float Radius
        {
            get { return _radius; }
            set { _radius = value; }
        }

        public float Impact
        {
            get { return _impact; }
            set { _impact = value; }
        }

        public double Start
        {
            get { return _start; }
            set { _start = value; }
        }

        public double Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        public double Progress
        {
            get { return _progress; }
            set { _progress = value; }
        }

        public event WeaponProjectileCallback OnImpact;

        public event TimerCallback OnTick;

        public event TimerCallback OnComplete;

        protected void KillFrameReached()
        {
            Debug.Log("killframe reached");
            var frame = CurrentFrame;
            var hitPoint = GetPositionAtFrame(frame);

            OnHitDirect(new RaycastHit(), hitPoint);

            // needs referal
            OnHitIndirect(UnityEngine.Physics.OverlapSphere(hitPoint, Radius).ToList(), hitPoint);

            Destroy(gameObject);

            Destroy(gameObject);
        }

        // Check for collisions via 2 methods
        // 1 - Network collisions via hit boxes
        // 2 - Non network collisions (static non moving objects, i.e. mesh collider)
        protected void ResolveCollisions(double frame, float frameDeltaTime)
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
                var indirectHits = UnityEngine.Physics.OverlapSphere(hitPoint, Radius).ToList();
                OnHitIndirect(indirectHits, hitPoint);
            }

            if (collided)
                Destroy(gameObject);
        }

        internal void OnHitDirect(RaycastHit hit, Vector3 position)
        {
            if (CanImpact)
                if (OnImpact != null)
                    OnImpact(this);
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
                    var healthImpact = Mathf.Max(0, (1f - distance / Radius) * Impact);
                    //hit.gameObject.transform.root.GetComponent<Controller>().ApplyDamage((byte)healthImpact);
                }

                // apply force from indirect hit
                var multiplier = distance / Radius;

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
            if (serverFrame > Start + Duration)
                KillFrameReached();

            for (; CurrentFrame < serverFrame; CurrentFrame++)
                ResolveCollisions(CurrentFrame, Time.fixedDeltaTime);

            transform.position =
                GetPositionAtFrame(serverFrame);
        }

        public void AddImpact(float impactAmount)
        {
            throw new NotImplementedException();
        }
    }
}