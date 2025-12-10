using System;
using MauiSensorFeeds.BaseModels;

namespace MauiSensorFeeds.Interfaces
{
    /// <summary>
    /// A signal buffer for sensor readings of type T. This buffer collects all
    /// readings but only notifies listeners at an interval of MinNotifyFrequencyMS.
    /// </summary>
    /// <typeparam name="T">sensor data point type</typeparam>
    public interface ISensorBuffer<T>
    {
        /// <summary>
        /// Strategy for retrieving the best recent value from the buffer
        /// </summary>
        BufferingStrategy BufferStrategy { get; }

        ReadWriteSensor<T> Sensor { get; }

        T GetAverageValue(List<T> values);

        /// <summary>
        /// Number of data points in the buffer (buffer is cleared
        /// when listeners are notified)
        /// </summary>
        int BufferCount { get; }

        /// <summary>
        /// Limit of the buffer size allowed. New readings are saved at end of buffer.
        /// </summary>
        int MaxBufferSize { get; }

        /// <summary>
        /// Minimum wait time between sending change notifications
        /// </summary>
        int MinNotifyFrequencyMS();

        /// <summary>
        /// Last time the sensor model was notfied of a value change
        /// </summary>
        DateTime LastReadingTime { get; }

        /// <summary>
        /// Last time the sensor model notfied targets of a value change
        /// </summary>
        DateTime LastNotifyTime { get; }

    }
}
