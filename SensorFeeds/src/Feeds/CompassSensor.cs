using MauiSensorFeeds.BaseModels;
using System;
using System.Threading;
using System.Threading.Tasks;
using MauiSensorFeeds.Extensions;
using Sensors = Microsoft.Maui.Devices.Sensors;

namespace MauiSensorFeeds.Feeds
{
    public class CompassSensor : ReadWriteSensor<double>, Sensors.ICompass, IDisposable
    {
        protected Sensors.ICompass DefaultCompass = Sensors.Compass.Default;

        public override bool IsSupported => DefaultCompass.IsSupported;

        public override bool IsMonitoring => isMonitoring;

        public CompassSensor(string sourceDataFilename) : base(SensorType.Compass, sourceDataFilename)
        {
            DefaultCompass.ReadingChanged += this.HandleReadingChanged;
            SensorFeeds.GetSensorFeeds().SetCompass(this);
        }

        private void HandleReadingChanged(object? sender, CompassChangedEventArgs e)
        {
            ReadingChangeEvent(e.Reading.HeadingMagneticNorth);
        }

        public event EventHandler<CompassChangedEventArgs>? ReadingChanged;

        public override void Dispose()
        {
            Stop();
            base.Dispose();
        }

        public override void Start()
        {
            Start(sensorSpeed, false);
        }

        public void Start(SensorSpeed _sensorSpeed)
        {
            this.sensorSpeed = _sensorSpeed;
            Start(_sensorSpeed, false);
        }

        public void Start(SensorSpeed sensorSpeed, bool applyLowPassFilter)
        {
            if (this.IsReadingFromFile)
            {
                isMonitoring = true;
                StartReadingData().Wait();
            }
            else
            {
                DefaultCompass.Start(sensorSpeed, applyLowPassFilter);
                isMonitoring = DefaultCompass.IsMonitoring;
            }
        }

        public override void Stop()
        {
            if (this.IsReadingFromFile)
            {
                isMonitoring = false;
            }
            else
            {
                DefaultCompass.Stop();
                isMonitoring = DefaultCompass.IsMonitoring;
                SensorInputData?.SaveToFile();
                SensorOutputData?.SaveToFile();
            }
        }

        public override SensorData<double> GenericSensorData(string filenName)
        {
            return new DoubleData(filenName);
        }

        protected override double CreateNew()
        {
            return 0D;
        }

        protected override float ValueOf(double value)
        {
            return (float)(value != double.NaN ? value : 0);
        }

        protected override void NotifyReadingChange(double value)
        {
            var args = new CompassChangedEventArgs(new CompassData(value));
            ReadingChanged?.Invoke(null, args);
        }
    }
}
