using MauiSensorFeeds.BaseModels;
using MauiSensorFeeds.Interfaces;
using MauiSensorFeeds.Data;
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

        public override bool IsMonitoring => accelerometerSource.IsMonitoring;

        public override bool IsSupported => accelerometerSource.IsSupported;

        public void Start()
        {
            this.Start(this.sensorSpeed);
        }

        public override void Start(SensorSpeed sensorSpeed = SensorSpeed.Default)
        {
            if (!IsReadingFromFile)
            {
                if (!accelerometerSource.IsMonitoring &&
                    accelerometerSource.IsSupported)
                {
                    this.sensorSpeed = sensorSpeed;
                    accelerometerSource.Start(sensorSpeed);
                    isMonitoring = true;
                }
            }
            else
            {
                StartReadingData();
            }
        }

        public override void Stop()
        {
            isMonitoring = false;
            if (!IsReadingFromFile)
            {
                accelerometerSource.Stop();
                //SensorInputData?.SaveToFile();
                //SensorOutputData?.SaveToFile();
            }
        }

        public AccelerometerSensor(string testDataFile) : base(SensorType.Accelerometer)
        {
            this.ReadDataFile = testDataFile;
        }

        public AccelerometerSensor() : base(SensorType.Accelerometer)
        {
            AttachEventListeners();
        }

        public Vector3 ShakeEvent => new Vector3(float.NaN, float.NaN, float.NaN);

        public object SensorInputStream { get; set; }

        protected void Accelerometer_ReadingChanged(object? sender, AccelerometerChangedEventArgs e)
        {
            ReadingChangeEvent(e.Reading.Acceleration);
        }

        protected void Accelerometer_ShakeDetected(object? sender, EventArgs e)
        {
            ReadingChangeEvent(ShakeEvent);
            ShakeDetected?.Invoke(sender, e);
        }

        #region protected overrides

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

        internal override float ValueOf(Vector3 value)
        {
            return value.Length();
        }

        public override SensorData<Vector3> GenericSensorData(string dataFilename)
        {
            return new Vector3Data(dataFilename);
        }

        #endregion

        #region events

        public event EventHandler<AccelerometerChangedEventArgs>? ReadingChanged;

        public event EventHandler? ShakeDetected;

        public override void SetHandlerToEvent(object handler)
        {
            if (handler is EventHandler<AccelerometerChangedEventArgs>)
                this.ReadingChanged += handler as EventHandler<AccelerometerChangedEventArgs>;
            else if (handler is EventHandler)
                this.ShakeDetected += handler as EventHandler;
        }

        public override void UnsetHandlerToEvent(object handler)
        {
            if (handler is EventHandler<AccelerometerChangedEventArgs>)
                this.ReadingChanged -= handler as EventHandler<AccelerometerChangedEventArgs>;
            else if (handler is EventHandler)
                this.ShakeDetected -= handler as EventHandler;
        }

        #endregion
    }
}
