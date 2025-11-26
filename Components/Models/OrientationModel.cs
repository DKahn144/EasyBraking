namespace EasyBraking.Components.Models
{
    public class OrientationModel : QuaternionModel
    {
        /*
         * See https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/device/sensors?view=net-maui-9.0&tabs=android
         */
        public OrientationModel(SensorManagerModel model, int count) : base(model, count)
        {
            if (model == null)
                throw new ArgumentNullException("SensorViewModel model");
            if (model.OrientationSensor == null)
                throw new ArgumentNullException("model.OrientationSensor");
        }

        public override void Configure()
        {
            this.isSupported = sensorManagerModel.OrientationSensor.IsSupported;
            if (this.isSupported)
            {
                sensorManagerModel.OrientationSensor.ReadingChanged += OrientationSensor_ReadingChanged;
                sensorManagerModel.OrientationSensor.Start(SensorSpeed.UI);
            }
        }

        public override void Dispose()
        {
            if (sensorManagerModel.OrientationSensor.IsMonitoring)
            {
                sensorManagerModel.OrientationSensor.Stop();
                sensorManagerModel.OrientationSensor.ReadingChanged -= OrientationSensor_ReadingChanged;
            }
            base.Dispose();
        }

        private void OrientationSensor_ReadingChanged(object? sender, OrientationSensorChangedEventArgs e)
        {
            RecordReading(e.Reading.Orientation);
        }
    }
}
