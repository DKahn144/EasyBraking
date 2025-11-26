namespace EasyBraking.Components.Interfaces
{
    /// <summary>
    /// A signal source for sensor readings of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBufferedSensorSource<T> : ISensorSource<T>
    {
        T CurrentAvgValue { get; }

        int MaxBufferSize { get; set; }

        /// <summary>
        /// Wait time for sending change notifications
        /// </summary>
        int MinNotifyFrequencyMS { get; set; }

        /// <summary>
        /// Last time the sensor model was notfied of a value change
        /// </summary>
        DateTime LastNotifyTime { get; }

        /// <summary>
        /// Get an absolute value of the magnitude of value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        double CalculateSize(T value);
    }
}
