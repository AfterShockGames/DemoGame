namespace DemoGame.Interfaces
{
    public delegate void TimerCallback(ITimer timer);
    public interface ITimer
    {
        double Start { get; set; }
        double Duration { get; set; }
        double Progress { get; set; }

        event TimerCallback OnTick;
        event TimerCallback OnComplete;
    }
}