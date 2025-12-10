using System;
using System.IO;
using System.Collections.Generic;
using MauiSensorFeeds.BaseModels;

namespace MauiSensorFeeds.BaseModels
{
    /// <summary>
    /// A test sensor that supports reading or writing of data points of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseSensor<T>
    {
        /// <summary>
        /// Most recent reading
        /// </summary>
        internal T CurrentValue { get; set; }

        /// <summary>
        /// Name of the sensor type (acceleration, etc.)
        /// </summary>
        protected SensorType sensorType;
        protected bool isSupported = true;
        protected bool isMonitoring = false;
        protected DateTime StartTime = DateTime.UtcNow;
        protected SensorSpeed sensorSpeed = SensorSpeed.Default;
        protected virtual void SetSensorSpeed(SensorSpeed _sensorSpeed)
        {
            ThrowIfMonitoring("SensorSpeed");
            sensorSpeed = _sensorSpeed;
        }

        protected void ThrowIfMonitoring(string fieldName)
        {
            if (isMonitoring)
            {
                throw new ApplicationException($"Cannot set value of {fieldName} while sensor is actively monitoring.");
            }
        }

        public SensorType SensorType => sensorType;

        public SensorSpeed SensorSpeed 
        {
            get => sensorSpeed;
            set => SetSensorSpeed(value);
        }

        public virtual bool IsSupported => isSupported;
        
        public virtual bool IsMonitoring => isMonitoring;

        public BaseSensor(SensorType type)
        {
            CurrentValue = CreateNew();
            sensorType = type;
        }

        public abstract void Start();

        public abstract void Stop();

        /// <summary>
        /// Create a new instance of type T.
        /// </summary>
        /// <returns>A new instance of type T.</returns>
        protected abstract T CreateNew();

        protected abstract float ValueOf(T? value);

    }
}
