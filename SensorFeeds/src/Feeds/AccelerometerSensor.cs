using MauiSensorFeeds.BaseModels;
using MauiSensorFeeds.Interfaces;
using Microsoft.Maui.Devices.Sensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MauiSensorFeeds.Feeds
{
    public partial class AccelerometerSensor : ReadWriteSensor<Vector3>, IAccelerometer
    {
        private IAccelerometer accelerometerSource = Accelerometer.Default;

        public event EventHandler<AccelerometerChangedEventArgs>? ReadingChanged;

        public event EventHandler? ShakeDetected;

        public override bool IsMonitoring => accelerometerSource.IsMonitoring;

        public override bool IsSupported => accelerometerSource.IsSupported;

        public override void Start()
        {
            this.Start(this.sensorSpeed);
        }

        public async void Start(SensorSpeed sensorSpeed)
        {
            if (!IsReadingFromFile)
            {
                if (!accelerometerSource.IsMonitoring)
                {
                    this.sensorSpeed = sensorSpeed;
                    accelerometerSource.Start(sensorSpeed);
                    isMonitoring = true;
                }
            }
            else
            {
                await StartReadingData();
            }
        }

        public override void Stop()
        {
            isMonitoring = false;
            if (!IsReadingFromFile)
            {
                accelerometerSource.Stop();
                SensorInputData?.SaveToFile();
                SensorOutputData?.SaveToFile();
            }
        }

        public AccelerometerSensor(string testDataFile) : base(SensorType.Accelerometer)
        {
            this.ReadDataFile = testDataFile;
            SensorFeeds.GetSensorFeeds().SetAccelerometer(this);
        }

        public AccelerometerSensor() : base(SensorType.Accelerometer)
        {
            AttachEventListeners();
            SensorFeeds.GetSensorFeeds().SetAccelerometer(this);
        }

        public Vector3 ShakeEvent => new Vector3(float.NaN, float.NaN, float.NaN);

        protected void Accelerometer_ReadingChanged(object? sender, AccelerometerChangedEventArgs e)
        {
            ReadingChangeEvent(e.Reading.Acceleration);
        }

        protected void Accelerometer_ShakeDetected(object? sender, EventArgs e)
        {
            ReadingChangeEvent(ShakeEvent);
            ShakeDetected?.Invoke(sender, e);
        }

        protected override void AttachEventListeners()
        {
            base.AttachEventListeners();
            accelerometerSource.ReadingChanged += Accelerometer_ReadingChanged;
            accelerometerSource.ShakeDetected += Accelerometer_ShakeDetected;
        }

        protected override void DetachEventListeners()
        {
            base.DetachEventListeners();
            accelerometerSource.ReadingChanged -= Accelerometer_ReadingChanged;
            accelerometerSource.ShakeDetected -= Accelerometer_ShakeDetected;
        }

        protected override void NotifyReadingChange(Vector3 value)
        {
            if (value.Length() == float.NaN)
            {
                ShakeDetected?.Invoke(this, new EventArgs());
                return;
            }
            /// Notify all subscribers of the reading change if not a shake event
            ReadingChanged?.Invoke(null, 
                new AccelerometerChangedEventArgs(
                    new AccelerometerData(value.X, value.Y, value.Z)));
        }

        protected override Vector3 CreateNew()
        {
            return new Vector3();
        }

        protected override float ValueOf(Vector3 value)
        {
            return value.Length();
        }

        public override SensorData<Vector3> GenericSensorData(string dataFilename)
        {
            return new Vector3Data(dataFilename);
        }

    }
}
