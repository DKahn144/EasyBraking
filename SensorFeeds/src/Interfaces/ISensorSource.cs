using MauiSensorFeeds.BaseModels;
using System;

namespace MauiSensorFeeds.Interfaces
{
    /// <summary>
    /// A signal source for sensor readings of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISensorSource<T> : IDisposable
    {
        /// <summary>
        /// Most recent reading
        /// </summary>
        T CurrentValue { get; }

        /// <summary>
        /// Name of the sensor type (acceleration, etc.)
        /// </summary>
        SensorType SensorType { get; }

        bool IsSupported { get; }

        bool IsMonitoring { get; }

        DateTime StartTime { get; }

        ISensorSource<T>? TargetSensor { get; }
        ISensorSource<T>? SourceSensor { get; }

        /// <summary>
        /// Create a new instance of type T.
        /// </summary>
        /// <returns>A new instance of type T.</returns>
        T CreateNew();

        /// <summary>
        /// Transmit received value to targets. This should be fired when the source 
        /// listener detects a new value.
        /// </summary>
        /// <param name="value"></param>
        void NotifyTargets(T value);

        /// <summary>
        /// Save a new reading value from a sensor. Assign this to reading-changed events
        /// </summary>
        /// <param name="sample"></param>
        void RecordReading(T sample);
        
    }
}
