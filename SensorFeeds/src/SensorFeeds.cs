using MauiSensorFeeds.BaseModels;
using MauiSensorFeeds.Calculated;
using MauiSensorFeeds.Interfaces;
using Microsoft.Maui.Devices.Sensors;
using System.Numerics;
using Sensors = Microsoft.Maui.Devices.Sensors;

namespace MauiSensorFeeds
{

    /// <summary>
    /// SensorFeeds allows the swapping out of the sensor sources for testing and simulation, also 
    /// for recording sensor readings to a file.  TestFeed classes with names that start with "Logging" 
    /// all write their sensor readings to a json or csv file.
    /// </summary>
    public class SensorFeeds
    {

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. 
        private static SensorFeeds feeds;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. 

        /// <summary>
        /// A centralized starting time for all sensor feeds
        /// </summary>
        public static DateTime FeedStart = DateTime.MaxValue;

        public static long TicksSinceFeedStart()
        {
            return DateTime.UtcNow.Ticks - FeedStart.Ticks;
        }

        public static SensorFeeds GetSensorFeeds()
        {
            if (feeds == null)
            {
                feeds = new SensorFeeds();
            }
            return feeds;
        }

        public static void StartAll(SensorSpeed speed = SensorSpeed.Default)
        {
            var feeds = GetSensorFeeds();
            if (feeds.AccelerometerSource!.IsSupported)
                feeds.AccelerometerSource?.Start(speed);
            if (feeds.OrientationSensorSource!.IsSupported)
                feeds.OrientationSensorSource?.Start(speed);
            (feeds.GeolocationSource as IBaseSensor)?.Start();
            if (feeds.CompassSource!.IsSupported)
                feeds.CompassSource?.Start(speed);
            feeds.CalculatedModelSource?.Start();
            FeedStart = DateTime.UtcNow;
        }

        private SensorFeeds()
        {
        }

        #region model

        private IBaseSensor? calculatedModelSource = null;

        public IBaseSensor? CalculatedModelSource => calculatedModelSource;

        public CalculatedModelSensor? CalculatedModelSensor => calculatedModelSource as CalculatedModelSensor;

        public CalculatedModel? Model => CalculatedModelSensor?.CurrentValue;

        #endregion

        #region Accelerometer

        private BaseSensor<Vector3>? accelerometerSource = null;

        public BaseSensor<Vector3>? AccelerometerSource => accelerometerSource;

        public IAccelerometer? Accelerometer => accelerometerSource as IAccelerometer;

        #endregion

        #region Orientation

        private BaseSensor<Quaternion>? orientationSensorSource = null;

        public BaseSensor<Quaternion>? OrientationSensorSource => orientationSensorSource;

        public IOrientationSensor? OrientationSensor => orientationSensorSource as IOrientationSensor;

        #endregion

        #region Location

        private BaseSensor<Location>? geolocationSource = null;

        public BaseSensor<Location>? GeolocationSource => geolocationSource;

        public IGeolocation? Geolocation => geolocationSource as IGeolocation;

        #endregion

        #region Compass

        private BaseSensor<double>? compassSource = null;

        public BaseSensor<double>? CompassSource => compassSource;

        public ICompass? Compass => compassSource as ICompass;

        public static string FilePath { get; set; } = "?"; // "/storage/emulated/0/Documents";

        internal void SetSensor(IBaseSensor sensor)
        {
            bool monitoring = false;
            var source = GetSensor(sensor.SensorType);
            if (source is IBaseSensor)
            {
                IBaseSensor? currentSensor = (IBaseSensor)source;
                if (currentSensor != null)
                {
                    monitoring = currentSensor.IsMonitoring;
                    if (monitoring)
                    {
                        currentSensor.Stop();
                    }
                    foreach (var handler in currentSensor!.RegisteredHandlers)
                    {
                        currentSensor.UnregisterHandler(handler);
                        sensor.RegisterHandler(handler);
                    }
                }
            }
            ReplaceSensor(sensor);
            if (monitoring)
                sensor.Start();
        }

        private void ReplaceSensor(IBaseSensor sensor)
        {
            if (sensor != null)
            {
                switch (sensor.SensorType)
                {
                    case SensorType.Accelerometer:
                        {
                            if (sensor is IAccelerometer)
                                accelerometerSource = sensor as BaseSensor<Vector3>;
                            break;
                        }
                    case SensorType.Orientation:
                        {
                            if (sensor is IOrientationSensor)
                                orientationSensorSource = sensor as BaseSensor<Quaternion>;
                            break;
                        }
                    case SensorType.Location:
                        {
                            if (sensor is IGeolocation)
                                geolocationSource = sensor as BaseSensor<Location>;
                            break;
                        }
                    case SensorType.Compass:
                        {
                            if (sensor is ICompass)
                                compassSource = sensor as BaseSensor<double>;
                            break;
                        }
                    case SensorType.Calculated:
                        {
                            if (sensor is CalculatedModelSensor)
                                calculatedModelSource = (sensor as BaseSensor<CalculatedModel>);
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        private object? GetSensor(SensorType sensorType)
        {
            switch (sensorType)
            {
                case SensorType.Accelerometer:
                    return AccelerometerSource;
                case SensorType.Orientation:
                    return OrientationSensorSource;
                case SensorType.Location:
                    return GeolocationSource;
                case SensorType.Compass:
                    return CompassSource;
                case SensorType.Calculated:
                    return CalculatedModelSource;
                default:
                    return null;
            }
        }

        #endregion

        /*
        static ISettingsService CurrentService = new DefaultSettingsService();

        public static void SetSettingsService(ISettingsService service)
        {
            if (service != null)
            {
                CurrentService = service;
            }
        }

        public static ISettingsService Service => CurrentService;
        */

    }
}
