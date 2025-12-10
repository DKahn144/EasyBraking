using MauiSensorFeeds.BaseModels;
using System;
using System.IO;
using System.Collections.Generic;
using System.Numerics;
using Sensors = Microsoft.Maui.Devices.Sensors;
using MauiSensorFeeds.Interfaces;

namespace MauiSensorFeeds.Feeds
{
    public class Orientation_Sensor : ReadWriteSensor<Quaternion>, IOrientationSensor
    {
        private static IOrientationSensor orientationSensorSource = Sensors.OrientationSensor.Default;

        public Orientation_Sensor(string testDataFile) : base(SensorType.Location)
        {
            this.ReadDataFile = testDataFile;
            SensorFeeds.GetSensorFeeds().SetOrientationSensor(this);
        }

        public Orientation_Sensor() : base(SensorType.Location)
        {
            AttachEventListeners();
            SensorFeeds.GetSensorFeeds().SetOrientationSensor(this);
        }

        public override void Start()
        {
            this.Start(sensorSpeed);
        }

        public async void Start(SensorSpeed sensorSpeed)
        {
            if (!IsReadingFromFile)
            {
                AttachEventListeners();
                if (!orientationSensorSource.IsMonitoring)
                {
                    orientationSensorSource.Start(sensorSpeed);
                }
                isMonitoring = true;
            }
            else
            {
                DetachEventListeners();
                await this.StartReadingData();
            }
        }

        public override void Stop()
        {
            if (!IsReadingFromFile && orientationSensorSource.IsMonitoring)
            {
                orientationSensorSource.Stop();
                SensorInputData?.SaveToFile();
                SensorOutputData?.SaveToFile();
            }
            isMonitoring = false;
        }

        public event EventHandler<OrientationSensorChangedEventArgs>? ReadingChanged;

        public void OrientationSensor_ReadingChanged(object? sender, OrientationSensorChangedEventArgs e)
        {
            ReadingChangeEvent(e.Reading.Orientation);
        }

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

        protected override float ValueOf(Quaternion value)
        {
            return value.Length();
        }

    }
}
