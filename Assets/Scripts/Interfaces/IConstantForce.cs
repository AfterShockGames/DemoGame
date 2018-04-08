using UnityEngine;

namespace DemoGame.Interfaces
{
    public interface IConstantForce
    {
        Vector3 Origin { get; set; }
        Vector3 Velocity { get; set; }
    }
}