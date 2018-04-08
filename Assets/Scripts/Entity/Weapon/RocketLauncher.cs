using System.Collections;
using System.Collections.Generic;
using DemoGame.Entity.Projectile;
using DemoGame.Interfaces;
using UnityEngine;

namespace DemoGame.Entity.Weapon
{
    public class RocketLauncher : MonoBehaviour, IWeapon, IWeaponProjectile
    {
        [SerializeField] private float _fireInterval;
        [SerializeField] private GameObject _fireOrigin;
        [SerializeField] private float _fireVelocity;
        [SerializeField] private float _lastShot;
        [SerializeField] private GameObject _object;
        [SerializeField] private float _impactForce;
        [SerializeField] private int _projectileLifetime;

        public float FireInterval
        {
            get { return _fireInterval; }
            set { _fireInterval = value; }
        }

        public GameObject FireOrigin
        {
            get { return _fireOrigin; }
            set { _fireOrigin = value; }
        }

        public float FireVelocity
        {
            get { return _fireVelocity; }
            set { _fireVelocity = value; }
        }

        public float LastShot
        {
            get { return _lastShot; }
            set { _lastShot = value; }
        }

        public GameObject Object
        {
            get { return _object; }
            set { _object = value; }
        }

        public float ImpactForce
        {
            get { return _impactForce; }
            set { _impactForce = value; }
        }

        public int ProjectileLifetime
        {
            get { return _projectileLifetime; }
            set { _projectileLifetime = value; }
        }

        public bool CanFire()
        {
            return Time.time - this.LastShot >= this.FireInterval ? true : false;
        }

        public void Fire()
        {

            if (Object)
            {
                var forward = transform.forward;
                var originPosition = FireOrigin.transform.position;

                // Calculate forward vector of aiming camera 

                var projectileRotation = Quaternion.LookRotation(forward);

                var instance = Instantiate(Object, originPosition, projectileRotation);
                var projectileRocketlauncher = instance.GetComponent<ProjectileRocketLauncher>();

                projectileRocketlauncher.Start = UnityEngine.Network.time;
                projectileRocketlauncher.Duration = ProjectileLifetime;
                projectileRocketlauncher.Radius = 10f;
                projectileRocketlauncher.CanImpact = true;
                projectileRocketlauncher.Impact = 50f;

                projectileRocketlauncher.IgnoreGameObjects = new HashSet<GameObject>()
                {
                    gameObject,
                    FireOrigin.gameObject
                };

                projectileRocketlauncher.Velocity = forward * FireVelocity;

                LastShot = Time.time;
            }
        }
    }
}