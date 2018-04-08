using UnityEngine;

namespace DemoGame.Interfaces
{
    public interface IWeapon
    {
        float FireInterval { get; set; }
        GameObject FireOrigin { get; set; }
        float FireVelocity { get; set; }
        float LastShot { get; set; }

        bool CanFire();
        void Fire();
    }
}