using MauiSensorFeeds.BaseModels;
using MauiSensorFeeds.Data;
using MauiSensorFeeds.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiSensorFeeds.Feeds
{
    public abstract class SensorBuffer<T> : ISensorBuffer<T>, IDisposable
    {
        private DateTime lastReadingTime = DateTime.MinValue;
        private DateTime lastNotifyTime = DateTime.MinValue;
        private int minNotifyFrequencyMS = 10;

        #region protected

        protected ReadWriteSensor<T> _sensor;

        protected bool LimitBufferBySize =>
            BufferStrategy == BufferingStrategy.AverageOfLast10Readings ||
            BufferStrategy == BufferingStrategy.AverageOfLast100Readings ||
            BufferStrategy == BufferingStrategy.AverageOfLast1000Readings;

        protected bool LimitBufferByTime =>
            BufferStrategy == BufferingStrategy.AverageOfLast10Milliseconds ||
            BufferStrategy == BufferingStrategy.AverageOfLast100Milliseconds ||
            BufferStrategy == BufferingStrategy.AverageOfLast1000Milliseconds;

        protected T lastValue => Sensor.CurrentValue;

        protected SensorData<T> InputBuffer { get; }

        protected bool WaitLimitReached()
        {
            if (this.LimitBufferByTime)
            {
                int millisecs = this.MinNotifyFrequencyMS();
                return LastNotifyTime.AddMilliseconds(millisecs).Ticks < LastReadingTime.Ticks;
            }
            else if (this.LimitBufferBySize)
            {
                return this.BufferCount - 1 >= this.MaxBufferSize;
            }
            return true;
        }

        protected T GetAverageFromBufferValues()
        {
            List<T> values = InputBuffer.ValuesLastNo(MaxBufferSize);
            return GetAverageValue(values);
        }

        protected T GetAverageFromBufferValueTimes()
        {
            int millisecs = this.MinNotifyFrequencyMS();
            var fromTime = DateTime.UtcNow.AddMilliseconds(-millisecs);
            List<T> readings = InputBuffer.ValuesFrom(fromTime);
            if (readings.Count == 0)
                return lastValue;
            return GetAverageValue(readings);
        }

        internal virtual T? CustomHandler(T readValue)
        {
            lastReadingTime = DateTime.UtcNow;
            T? reportValue = default;
            InputBuffer.AddValue(readValue);
            if (WaitLimitReached())
            {
                if (LimitBufferBySize)
                {
                    reportValue = GetAverageFromBufferValues();
                }
                else if (LimitBufferByTime)
                {
                    reportValue = GetAverageFromBufferValueTimes();
                }
            }
            if (reportValue != null)
            {
                _sensor.currentValue = reportValue;
                lastNotifyTime = DateTime.UtcNow;
            }
            return reportValue;
        }

        #endregion  // protected

        #region public properties

        public ReadWriteSensor<T> Sensor => _sensor;

        public BufferingStrategy BufferStrategy { get; set; }

        public int MaxBufferSize
        {
            get
            {
                if (this.BufferStrategy == BufferingStrategy.AverageOfLast10Readings)
                    return 10;
                if (this.BufferStrategy == BufferingStrategy.AverageOfLast100Readings)
                    return 100;
                if (this.BufferStrategy == BufferingStrategy.AverageOfLast1000Readings)
                    return 1000;
                return int.MaxValue;
            }
        }

        public int MinNotifyFrequencyMS()
        {
            if (this.BufferStrategy == BufferingStrategy.AverageOfLast10Milliseconds)
                return 10;
            if (this.BufferStrategy == BufferingStrategy.AverageOfLast100Milliseconds)
                return 100;
            if (this.BufferStrategy == BufferingStrategy.AverageOfLast1000Milliseconds)
                return 1000;
            return 0;
        }

        public int BufferCount => InputBuffer.TestValues.Count;

        public DateTime LastReadingTime => lastReadingTime;

        public DateTime LastNotifyTime => lastNotifyTime;

        #endregion

        #region public methods

        /// <summary>
        /// sensor buffer handler constructor, set up by the sensor
        /// </summary>
        /// <param name="sensor">Sensor object using this buffer</param>
        /// <param name="strategy">Strategy for selecting the last reading</param>
        public SensorBuffer(ReadWriteSensor<T> sensor, BufferingStrategy strategy)
        {
            _sensor = sensor;
            InputBuffer = sensor.GenericSensorData(string.Empty);
            SetBufferStrategy(strategy);
        }

        public void SetBufferStrategy(BufferingStrategy strategy)
        {
            this.BufferStrategy = strategy;
        }

        public void Dispose()
        {
            //InputBuffer.Flush();
        }

        #endregion

        #region abstract methods

        /// <summary>
        /// Implementation of average calculation depends on value type.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public abstract T GetAverageValue(List<T> values);

        #endregion


    }
}
