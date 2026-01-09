using MauiSensorFeeds.BaseModels;
using Microsoft.Maui.Devices.Sensors;
using System;
using System.Numerics;

namespace MauiSensorFeeds.Feeds
{
    public partial class OrientationBuffer : MauiSensorFeeds.Feeds.Orientation_Sensor, IOrientationSensor
    {
        private QuaternionBuffer2 sensorBuffer;

        /*
          * See https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/device/sensors?view=net-maui-9.0&tabs=android
          */
        public OrientationBuffer(BufferingStrategy strategy = BufferingStrategy.AverageOfLast10Milliseconds) :
            base()
        {
            this.sensorBuffer = new QuaternionBuffer2(this, strategy);
        }

        protected override Quaternion CustomHandler(Quaternion value)
        {
            return this.sensorBuffer.CustomHandler(value);
        }

        public SensorBuffer<Quaternion> SensorBuffer => sensorBuffer;

        public override void Dispose()
        {
            if (this.IsMonitoring)
            {
                this.Stop();
                DetachEventListeners();
            }
            this.sensorBuffer.Dispose();
            base.Dispose();
        }

    }
}
