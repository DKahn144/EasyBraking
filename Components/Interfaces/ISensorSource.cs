namespace EasyBraking.Components.Interfaces
{
    /// <summary>
    /// A signal source for sensor readings of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISensorSource<T> : IDisposable
    {
        T CurrentValue { get; }

        bool IsSupported { get; }

        int MillisecsSinceLastReading { get; }

        /// <summary>
        /// Save a new reading value from a sensor
        /// </summary>
        /// <param name="sample"></param>
        void RecordReading(T sample);

        /// <summary>
        /// Set a notify method for when sensor value changed
        /// </summary>
        /// <param name="notifier"></param>
        Action Notifier { get; set; }

    }
}
