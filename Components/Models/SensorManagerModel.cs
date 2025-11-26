using EasyBraking.Components.Services;
using EasyBraking.Components.ViewModels;
using System.Diagnostics;
using System.Numerics;

namespace EasyBraking.Components.Models
{
    public class SensorManagerModel : IDisposable
    {
        public static double Rads2degs => 180 / Math.PI;
        private int MinMillisecsBetweenReadings = 100;
        private DateTime lastReadingUpdateTime = DateTime.MinValue;
        private static SensorManagerModel? model;  // singleton
        public EventHandler<ReadingsChangedEventArgs>? ReadingChanged;
        public static double Grav2DistancePHPS()
        {
            if (SettingsMgr.Service.GetDistanceUnit() == DistanceUnits.Miles)
                return 21.93759D;  // miles per hour per second, 9.807 meters/sec/sec *  0.00062137D miles / meter * 3600 seconds / hour
            else
                return 35.305D;   //  kilometers per hour per second, 9.807 meters/sec/sec *  0.001D km / meter * 3600 seconds / hour
        }

        #region Accelerometer

        private static int maxAccelerationSamplecount = 1000;
        private static IAccelerometer AccelerometerSource = Accelerometer.Default;
        private double horizontalAccel;
        private AccelerationModel accelSamples;

        public IAccelerometer AccelerometerSensor => SensorManagerModel.AccelerometerSource;
        public Vector3 Acceleration { get { return accelSamples.CurrentAvgValue; } }
        public double HorizontalAccel { get { return horizontalAccel; } }
        public int AccelerationPoints => accelSamples.BufferCount;

        // This must be set before opening or defaults to native.
        public static void SetAccelerometer(IAccelerometer accelerometer, int maxSamples)
        {
            if (model == null)
            {
                maxAccelerationSamplecount = maxSamples;
                if (accelerometer != null)
                    AccelerometerSource = accelerometer;
            }
            else
            {
                throw new NotSupportedException("Cannot set the accelerometer source after sensor manager has been created.");
            }
        }

        #endregion

        #region Orientation

        private static int maxOrientationSampleCount = 1000;
        private static IOrientationSensor OrientationSensorSource =
            Microsoft.Maui.Devices.Sensors.OrientationSensor.Default;
        private OrientationModel orientSamples;

        public IOrientationSensor OrientationSensor => SensorManagerModel.OrientationSensorSource;
        public Quaternion Orientation { get { return orientSamples.CurrentAvgValue; } }
        public int OrientationPoints => orientSamples.BufferCount;

        // This must be set before opening or defaults to native.
        public static void SetOrientationSensor(IOrientationSensor orientationSensor, int maxSamples)
        {
            if (model == null)
            {
                maxOrientationSampleCount = maxSamples;
                if (orientationSensor != null)
                    OrientationSensorSource = orientationSensor;
            }
            else
            {
                throw new NotSupportedException("Cannot set the OrientationSensor source after sensor manager has been created.");
            }
        }

        #endregion

        #region Location
        private LocationModel locationSamples;
        public Location Location { get { return locationSamples.GetAvgOfSamples(); } }
        public Tuple<double, double> vectorOfTravel = new(0D, 0D); // lat, long
        public double Speed { get { return locationSamples.Speed; } }
        public double AngleOfTravel => locationSamples.AngleOfTravel();
        public double AccelFromLocations => locationSamples.AccelRate;
        private static int maxLocationSampleCount = 1000;
        private static IGeolocation GeolocationSensorSource = Geolocation.Default;
        public IGeolocation GeolocationSensor => SensorManagerModel.GeolocationSensorSource;
        public int LocationPoints => locationSamples.BufferCount;
        public static void SetGeolocationSensor(IGeolocation geolocationSensor, int maxSamples)
        {
            if (model == null)
            {
                maxLocationSampleCount = maxSamples;
                if (geolocationSensor != null)
                    GeolocationSensorSource = geolocationSensor;
            }
            else
            {
                throw new NotSupportedException("Cannot set the GeolocationSensor source after sensor manager has been created.");
            }
        }
        #endregion

        #region Compass
        private bool compassSupported;
        private double magneticNorth;
        public double MagneticNorth { get { return this.magneticNorth; } }


        private void Compass_ReadingChanged(object? sender, CompassChangedEventArgs e)
        {
            magneticNorth = e.Reading.HeadingMagneticNorth;
            Recalculate();
        }
        #endregion

        public static SensorManagerModel GetSensorManagerModel()
        {
            if (model == null)
                model = new SensorManagerModel();
            return model;
        }

        private SensorManagerModel()
        {
            try
            {
                accelSamples = new AccelerationModel(this, maxAccelerationSamplecount);
                accelSamples.Configure();
                orientSamples = new OrientationModel(this, maxOrientationSampleCount);
                orientSamples.Configure();
                locationSamples = new LocationModel(this, maxLocationSampleCount);
                locationSamples.Configure();
                compassSupported = Compass.IsSupported;

                if (compassSupported)
                {
                    Compass.ReadingChanged += Compass_ReadingChanged;
                    Compass.Start(SensorSpeed.UI);
                    Debug.Print($"IsCompassMonitoring = {Compass.IsMonitoring}");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<ISensorReadingRecord> GetAllReadings()
        {
            var list = new List<ISensorReadingRecord>();
            var accelReadings = accelSamples.GetLogReadings();
            var orientReadings = orientSamples.GetLogReadings();
            var locationReadings = locationSamples.GetLogReadings();
            list.AddRange(accelReadings);
            list.AddRange(orientReadings);
            list.AddRange(locationReadings);
            list = list.OrderByDescending(r => r.Timestamp.Ticks).ToList();
            return list;
        }

        public static void ClearSensorManagerModel()
        {
            if (model != null)
            {
                model.accelSamples.Dispose();
                model.orientSamples.Dispose();
                model.locationSamples.Dispose();
            }
            model = null;
        }

        public void Dispose()
        {
            if (compassSupported)
                Compass.Stop();
            ClearSensorManagerModel();
        }

        /// <summary>
        /// Calculate display settings
        /// </summary>
        public void Recalculate()
        {
            if (lastReadingUpdateTime > DateTime.MinValue &&
                lastReadingUpdateTime.AddMilliseconds(MinMillisecsBetweenReadings) > DateTime.UtcNow)
            {
                return; // not enough time elapsed since last message.
            }
            else
                lastReadingUpdateTime = DateTime.UtcNow;

            try
            {
                var orientation = this.Orientation;
                var acceleration = this.Acceleration;
                var location = this.Location;

                this.orientSamples.ReportLastReading();
                this.accelSamples.ReportLastReading();
                this.locationSamples.ReportLastReading();

                if (ReadingChanged != null)
                {
                    ReadingChanged(this, new ReadingsChangedEventArgs(this));
                }

                // Recalculate();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// External pages can call this to trigger latest readings
        /// </summary>
        public void TriggerRecalc()
        {
            lastReadingUpdateTime = DateTime.MinValue;
            Recalculate();
        }

    }

    public class ReadingsChangedEventArgs : EventArgs
    {
        public SensorValuesModel Data;

        public ReadingsChangedEventArgs(SensorManagerModel model)
        {
            this.Data = new SensorValuesModel(model);
        }
    }
}
