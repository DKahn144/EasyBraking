using MauiSensorFeeds.BaseModels;
using MauiSensorFeeds.Calculated;
using MauiSensorFeeds.Data;
using MauiSensorFeeds.Feeds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MauiSensorFeeds.Calculated
{
    public class CalculatedModelSensor : ReadWriteSensor<CalculatedModel>
    {

        protected SensorFeeds Feeds => SensorFeeds.GetSensorFeeds();

        public CalculatedModelSensor() : base(SensorType.Calculated)
        {
        }

        public CalculatedModelSensor(string writeDataFile) : base(SensorType.Calculated)
        {
            this.WriteOutputDataFile = writeDataFile;
        }

        public CalculatedModel GetLatestModel()
        {
            return CurrentValue;
        }

        #region public overrides

        public override SensorData<CalculatedModel> GenericSensorData(string dataFilename)
        {
            return new CalculatedModelData(dataFilename);
        }

        public override void Start(SensorSpeed speed = SensorSpeed.Default)
        {
            this.isMonitoring = true;
            AttachEventListeners();
        }

        public override void Stop()
        {
            this.isMonitoring = false;
            DetachEventListeners();
            //SensorInputData?.SaveToFile();
            //SensorOutputData?.SaveToFile();
        }

        #endregion

        #region Event listeners

        protected override void AttachEventListeners()
        {
            Feeds.Accelerometer!.ReadingChanged += Accelerometer_ReadingChanged;
            Feeds.OrientationSensor!.ReadingChanged += OrientationSensor_ReadingChanged;
            Feeds.Geolocation!.LocationChanged += Geolocation_LocationChanged;
            Feeds.Compass!.ReadingChanged += Compass_ReadingChanged;
        }

        protected override void DetachEventListeners()
        {
            Feeds.Accelerometer!.ReadingChanged -= Accelerometer_ReadingChanged;
            Feeds.OrientationSensor!.ReadingChanged -= OrientationSensor_ReadingChanged;
            Feeds.Geolocation!.LocationChanged -= Geolocation_LocationChanged;
            Feeds.Compass!.ReadingChanged -= Compass_ReadingChanged;
        }

        internal void Compass_ReadingChanged(object? sender, CompassChangedEventArgs e)
        {
            // var model = CurrentValue.GetModelCopy();
            CurrentValue.headingFromNorth = (float) e.Reading.HeadingMagneticNorth;
            CurrentValue.Recalculate();
            ReadingChangeEvent(CurrentValue);
        }

        internal void Geolocation_LocationChanged(object? sender, GeolocationLocationChangedEventArgs e)
        {
            // var model = CurrentValue.GetModelCopy();
            CurrentValue.Location = e.Location;
            CurrentValue.Recalculate();
            ReadingChangeEvent(CurrentValue);
        }

        internal void OrientationSensor_ReadingChanged(object? sender, OrientationSensorChangedEventArgs e)
        {
            // var model = CurrentValue.GetModelCopy();
            CurrentValue.Orientation = e.Reading.Orientation;
            CurrentValue.Recalculate();
            ReadingChangeEvent(CurrentValue);
        }

        internal void Accelerometer_ReadingChanged(object? sender, AccelerometerChangedEventArgs e)
        {
            // var model = CurrentValue.GetModelCopy();
            CurrentValue.Acceleration = e.Reading.Acceleration;
            CurrentValue.Recalculate();
            ReadingChangeEvent(CurrentValue);
        }

        #endregion

        #region protected overrides

        protected override CalculatedModel CreateNew()
        {
            return CalculatedModel.GetModel();
        }

        protected override void NotifyReadingChange(CalculatedModel value)
        {
            var args = new CalculatedModelChangedEventArgs(value);
            CalculatedValuesChanged?.Invoke(null, args);
        }

        internal override float ValueOf(CalculatedModel? value)
        {
            var orientationSensor = Feeds.OrientationSensor as Orientation_Sensor;
            return (value?.Acceleration.Length() +
                orientationSensor!.ValueOf(value?.Orientation ?? Quaternion.Zero))
                ?? 0;
        }

        #endregion

        #region events

        public event EventHandler<CalculatedModelChangedEventArgs>? CalculatedValuesChanged;

        public override void SetHandlerToEvent(object handler)
        {
            if (handler is EventHandler<CalculatedModelChangedEventArgs>)
                this.CalculatedValuesChanged += handler as EventHandler<CalculatedModelChangedEventArgs>;
        }

        public override void UnsetHandlerToEvent(object handler)
        {
            if (handler is EventHandler<CalculatedModelChangedEventArgs>)
                this.CalculatedValuesChanged -= handler as EventHandler<CalculatedModelChangedEventArgs>;
        }

        #endregion
    }
}
