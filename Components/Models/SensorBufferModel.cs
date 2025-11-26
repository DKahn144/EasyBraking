using EasyBraking.Components.Interfaces;

namespace EasyBraking.Components.Models
{
    public abstract class SensorBufferModel<T> : IBufferedSensorSource<T>, IDataLogger<T>, IDisposable
    {
        private DateTime lastReadingTime = DateTime.Now;
        private int maxBufferSize = 1000;

        protected T lastSample;
        protected SensorManagerModel sensorManagerModel;
        protected List<T> valueSamples = new List<T>();
        protected List<DateTime> readingTimes = new List<DateTime>();
        protected object samplesLock = new object();
        protected bool isSupported = false;

        #region IBufferedSensorSource

        public int MaxBufferSize
        {
            get { return maxBufferSize; }
            set
            {
                lock (samplesLock)
                {
                    if (valueSamples.Count > value)
                        valueSamples.RemoveRange(0, valueSamples.Count - value);
                    maxBufferSize = value;
                }
            }
        }

        public int MinNotifyFrequencyMS { get; set; }

        public DateTime LastNotifyTime { get; set; }

        public int BufferCount => valueSamples.Count;

        #endregion

        #region ISensorSource

        public Action Notifier { get; set; }

        public bool IsSupported => this.isSupported;

        public T CurrentAvgValue => GetAvgOfSamples();

        public T CurrentValue => lastSample;

        public int MillisecsSinceLastReading => throw new NotImplementedException();

        public virtual void RecordReading(T value)
        {
            lastReadingTime = DateTime.UtcNow;
            lastSample = value;

            lock (samplesLock)
            {
                while (valueSamples.Count >= maxBufferSize)
                    valueSamples.RemoveAt(0);
                while (readingTimes.Count >= maxBufferSize)
                    readingTimes.RemoveAt(0);
                valueSamples.Add(value);
                readingTimes.Add(lastReadingTime);
            }
            SignalChanges(value);
        }

        #endregion

        /// <summary>
        /// sensor buffer constructor
        /// </summary>
        /// <param name="model">SensorViewModel singleton object</param>
        /// <param name="maxSampleCount">Max sample count. If zero, no smoothing or averaging.</param>
        public SensorBufferModel(SensorManagerModel model, int maxSampleCount)
        {
            sensorManagerModel = model;
            MinNotifyFrequencyMS = 100;
            maxBufferSize = maxSampleCount;
            lastSample = this.CreateNew();
            lastReadingTime = DateTime.MinValue;
            Notifier = model.Recalculate;
        }

        public virtual void Dispose() { }

        public void SignalChanges(T reading)
        {
            if (reading != null)
            {
                this.AddReading(DateTime.UtcNow, reading, false);
                if (MinNotifyFrequencyMS == 0 ||
                    LastNotifyTime.AddMilliseconds(MinNotifyFrequencyMS) < DateTime.UtcNow)
                {
                    Notifier();
                    LastNotifyTime = DateTime.UtcNow;
                }
            }
        }

        public int ReadingsInLastMinute
        {
            get
            {
                lock (samplesLock)
                {
                    if (readingTimes.Count > 1)
                    {
                        var now = DateTime.UtcNow;
                        int i = readingTimes.Count - 1;
                        while (i > 0 && readingTimes[i].AddMinutes(1) > now)
                            i--;
                        return i;
                    }
                    return readingTimes.Count;
                }
            }
        }

        /// <summary>
        /// Get a weighted average of the previously recorded samples, where more recent measurements are more heavily weighted.
        /// </summary>
        /// <returns></returns>
        public T GetAvgOfSamples()
        {
            T avg;
            if (MaxBufferSize == 0 || valueSamples.Count == 0)
            {
                avg = lastSample;
            }
            else
            {
                T weightedAvg = CreateNew();
                long m = 0;
                lock (samplesLock)
                {
                    int n = valueSamples.Count;
                    for (int i = 0; i < n; i++)
                    {
                        // each ith value should contribute a weight of i, then devide result by sum of i's = m
                        AddWeightedSample(ref weightedAvg, i + 1, valueSamples[i]);
                        m += i + 1;
                    }
                }
                avg = DivideWeightedByTotalWeight(weightedAvg, m);
                this.AddReading(DateTime.UtcNow, avg, true);
            }
            return avg;
        }

        #region abstract methods

        /// <summary>
        /// Configure from appropriate sensor static class.
        /// </summary>
        public abstract void Configure();

        /// <summary>
        /// Calculate an integer magnitude based on the values of the sample. 
        /// </summary>
        /// <param name="value">The input value used to calculate the result.</param>
        /// <returns>An integer representing the calculated magnitude.</returns>
        public abstract double CalculateSize(T value);

        /// <summary>
        /// Add a sample weighted by value i to the weighted average.
        /// </summary>
        /// <param name="weightedAvg">variable that holds the total of weighted samples</param>
        /// <param name="i">weight number to weigh this sample by</param>
        /// <param name="sample">the contributing sample</param>
        public abstract void AddWeightedSample(ref T weightedAvg, int i, T sample);

        /// <summary>
        /// Create an empty sample.
        /// </summary>
        /// <returns></returns>
        public abstract T CreateNew();

        /// <summary>
        /// divide the total of weighted samples by the total of all the weights
        /// </summary>
        /// <param name="weightedAvg">The total of all weighted samples</param>
        /// <param name="m">the total factor of weights to divide each element of the sample by</param>
        /// <returns></returns>
        public abstract T DivideWeightedByTotalWeight(T weightedAvg, long m);

        #endregion

        #region IDataLogger

        private SensorLogger<T> logger = new SensorLogger<T>();

        public SensorReading<T>[] GetLogReadings(string typename = "")
        {
            return logger.GetLogReadings(this.GetType().Name.Replace("Model", ""));
        }

        public void AddReading(DateTime timestamp, T reading, bool calculated)
        {
            logger.AddReading(timestamp, reading, calculated);
        }

        public void ReportLastReading()
        {
            logger.ReportLastReading();
        }

        #endregion

    }
}