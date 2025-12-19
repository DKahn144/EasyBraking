using MauiSensorFeeds.BaseModels;
using System;
using System.IO;
using System.Collections.Generic;
using System.Numerics;
using Sensors = Microsoft.Maui.Devices.Sensors;
using MauiSensorFeeds.Interfaces;
using MauiSensorFeeds.Data;

namespace MauiSensorFeeds.Feeds
{
    public class Orientation_Sensor : ReadWriteSensor<Quaternion>, IOrientationSensor
    {
        private static IOrientationSensor orientationSensorSource = Sensors.OrientationSensor.Default;

        public Orientation_Sensor(string testDataFile) : base(SensorType.Location)
        {
            this.ReadDataFile = testDataFile;
        }

        public Orientation_Sensor() : base(SensorType.Orientation)
        {
            AttachEventListeners();
        }

        public override void Start(SensorSpeed speed = SensorSpeed.Default)
        {
            if (speed != SensorSpeed.Default)
                sensorSpeed = speed;
            if (!IsReadingFromFile)
            {
                AttachEventListeners();
                if (!orientationSensorSource.IsMonitoring &&
                    orientationSensorSource.IsSupported)
                {
                    orientationSensorSource.Start(sensorSpeed);
                    isMonitoring = true;
                }
            }
            else
            {
                DetachEventListeners();
                this.StartReadingData();
            }
        }

        public override void Stop()
        {
            if (!IsReadingFromFile && orientationSensorSource.IsMonitoring)
            {
                orientationSensorSource.Stop();
                //SensorInputData?.SaveToFile();
                //SensorOutputData?.SaveToFile();
            }
            isMonitoring = false;
        }

        public void OrientationSensor_ReadingChanged(object? sender, OrientationSensorChangedEventArgs e)
        {
            ReadingChangeEvent(e.Reading.Orientation);
        }

        #region protected overrides

        protected override void NotifyReadingChange(Quaternion value)
        {
            var data = new OrientationSensorData(value.X, value.Y, value.Z, value.W);
            ReadingChanged?.Invoke(this, new OrientationSensorChangedEventArgs(data));
        }

        protected override void AttachEventListeners()
        {
            base.AttachEventListeners();
            orientationSensorSource.ReadingChanged += OrientationSensor_ReadingChanged;
        }

        protected override void DetachEventListeners()
        {
            base.DetachEventListeners();
            orientationSensorSource.ReadingChanged -= OrientationSensor_ReadingChanged;
        }

        public override SensorData<Quaternion> GenericSensorData(string filenName)
        {
            return new QuaternionData(filenName);
        }

        protected override Quaternion CreateNew()
        {
            return new Quaternion();
        }

        internal override float ValueOf(Quaternion value)
        {
            return (value.X + value.Y + value.Z + value.W);
        }

        #endregion

        #region events

        public event EventHandler<OrientationSensorChangedEventArgs>? ReadingChanged;

        public override void SetHandlerToEvent(object handler)
        {
            if (handler is EventHandler<OrientationSensorChangedEventArgs>)
                this.ReadingChanged += handler as EventHandler<OrientationSensorChangedEventArgs>;
        }

        public override void UnsetHandlerToEvent(object handler)
        {
            if (handler is EventHandler<OrientationSensorChangedEventArgs>)
                this.ReadingChanged -= handler as EventHandler<OrientationSensorChangedEventArgs>;
        }

        #endregion
    }
}
