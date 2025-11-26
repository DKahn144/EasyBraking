namespace EasyBraking.Components.Models
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
    internal class AccelerationModel : Vector3Model
    {
        public AccelerationModel(SensorManagerModel model, int maxSamplecount) : base(model, maxSamplecount)
        {
            if (model == null)
                throw new ArgumentNullException("SensorViewModel model");
            if (model.AccelerometerSensor == null)
                throw new ArgumentNullException("model.AccelerometerSensor");
        }

        public override void Configure()
        {
            this.isSupported = sensorManagerModel.AccelerometerSensor.IsSupported;
            if (this.isSupported && !sensorManagerModel.AccelerometerSensor.IsMonitoring)
            {
                sensorManagerModel.AccelerometerSensor.ReadingChanged += Accelerometer_ReadingChanged;
                sensorManagerModel.AccelerometerSensor.Start(SensorSpeed.UI);
            }
        }

        public override void Dispose()
        {
            if (sensorManagerModel.AccelerometerSensor.IsMonitoring)
            {
                sensorManagerModel.AccelerometerSensor.Stop();
                sensorManagerModel.AccelerometerSensor.ReadingChanged -= Accelerometer_ReadingChanged;
            }
            base.Dispose();
        }

        public double HorizontalAccel
        {
            get
            {
                // remove gravity vector
                var lsq = this.CurrentAvgValue.LengthSquared();
                return lsq > 1 ? lsq - 1 : 0;
            }
        }

        private void Accelerometer_ReadingChanged(object? sender, AccelerometerChangedEventArgs e)
        {
            this.RecordReading(e.Reading.Acceleration);
        }
    }
}
