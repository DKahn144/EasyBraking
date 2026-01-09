using MauiSensorFeeds.BaseModels;
using MauiSensorFeeds.Interfaces;
using Microsoft.Maui.Devices.Sensors;
using System;
using System.Numerics;

namespace MauiSensorFeeds.Feeds
{
    /*
     * From:  https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/device/sensors?view=net-maui-9.0&tabs=android
     * Accelerometer readings are reported back in G. 
     * A G is a unit of gravitation force equal to the gravity 
     * exerted by the earth's gravitational field.
     * 
     * The coordinate-system is defined relative to the screen 
     * of the device in its default orientation. The axes aren't 
     * swapped when the device's screen orientation changes.
     * 
     * The X axis is horizontal and points to the right, the Y axis 
     * is vertical and points up and the Z axis points towards the 
     * outside of the front face of the screen. In this system, 
     * coordinates behind the screen have negative Z values.
     * 
     * Examples:
     * 
     * When the device lies flat on a table and is pushed on its 
     * left side toward the right, the X acceleration value is positive.
     * 
     * When the device lies flat on a table, the acceleration value is 
     * +1.00 G or (+9.81 m/s^2), which corresponds to the acceleration 
     * of the device minus the force of gravity and normalized as in G.
     * 
     * When the device lies flat on a table and is pushed toward the sky 
     * with an acceleration of A, the acceleration value is equal to A+G
     * or, A-(-G), which corresponds to the acceleration of the device 
     * minus the (negative) force of gravity and normalized in G.
     */

    public partial class AccelerometerBuffer : AccelerometerSensor
    {
        private SensorBuffer<Vector3> sensorBuffer;

        public AccelerometerBuffer(BufferingStrategy strategy = BufferingStrategy.AverageOfLast100Milliseconds)
            : base()
        {
            this.sensorBuffer = new Vector3Buffer2(this, strategy);
        }

        public SensorBuffer<Vector3> SensorBuffer => sensorBuffer;

        protected override Vector3 CustomHandler(Vector3 value)
        {
            return this.sensorBuffer.CustomHandler(value);
        }

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
