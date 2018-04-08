using JetBrains.Annotations;

namespace DemoGame.Interfaces
{
    public interface IHealth
    {
        float Current { get; set; }
        float Min { get; set; }
        float Max { get; set; }

        void AddImpact(float impactAmount);
    }
}