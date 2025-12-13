using MauiSensorFeeds.BaseModels;

namespace MauiSensorFeeds.Interfaces
{
    public interface IBaseSensor
    {
        bool IsMonitoring { get; }
        bool IsSupported { get; }
        SensorSpeed SensorSpeed { get; set; }
        SensorType SensorType { get; }

        void Start(SensorSpeed speed = SensorSpeed.Default);
        void Stop();

        IList<object> RegisteredHandlers { get; }

        void RegisterHandler(object handler);
        void UnregisterHandler(object handler);


    }
}