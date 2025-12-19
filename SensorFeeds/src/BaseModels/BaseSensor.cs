using System;
using System.IO;
using System.Collections.Generic;
using MauiSensorFeeds.Interfaces;

namespace MauiSensorFeeds.BaseModels
{
    /// <summary>
    /// A test sensor that supports reading or writing of data points of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseSensor<T> : IBaseSensor
    {
        /// <summary>
        /// Most recent reading
        /// </summary>
        public T CurrentValue => currentValue;

        internal T currentValue { get; set; }
        /// <summary>
        /// Name of the sensor type (acceleration, etc.)
        /// </summary>
        protected SensorType sensorType;
        protected bool isSupported = true;
        protected bool isMonitoring = false;
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
            currentValue = CreateNew();
            sensorType = type;
            SensorFeeds.GetSensorFeeds().SetSensor(this);
        }

        public abstract void Start(SensorSpeed speed = SensorSpeed.Default);

        public abstract void Stop();

        /// <summary>
        /// Create a new instance of type T.
        /// </summary>
        /// <returns>A new instance of type T.</returns>
        protected abstract T CreateNew();

        internal abstract float ValueOf(T? value);

        public IList<object> RegisteredHandlers { get; } = new List<object>();

        public void RegisterHandler(object handler)
        {
            if (!RegisteredHandlers.Contains(handler))
            {
                RegisteredHandlers.Add(handler);
                SetHandlerToEvent(handler);
            }
        }
        public void UnregisterHandler(object handler)
        {
            if (RegisteredHandlers.Contains(handler))
            {
                UnsetHandlerToEvent(handler);
                RegisteredHandlers.Remove(handler);
            }
        }
        public void RegisterAllHandlers()
        {
            foreach (var handler in RegisteredHandlers)
            {
                SetHandlerToEvent(handler);
            }
        }
        public void UnregisterAllHandlers()
        {
            foreach (var handler in RegisteredHandlers)
            {
                UnsetHandlerToEvent(handler);
            }
        }

        public abstract void SetHandlerToEvent(object handler);
        public abstract void UnsetHandlerToEvent(object handler);

    }
}
