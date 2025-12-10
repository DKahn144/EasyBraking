using MauiSensorFeeds.BaseModels;
using MauiSensorFeeds.Interfaces;
using MauiSensorFeeds.Calculated;
using Microsoft.Maui.Devices.Sensors;
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
        public static DateTime FeedStart = DateTime.UtcNow;

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

        private SensorFeeds()
        {
        }

        public CalculatedModel Model => CalculatedModel.GetModel();


        #region Accelerometer

        private Sensors.IAccelerometer accelerometerSource = Sensors.Accelerometer.Default;

        public Sensors.IAccelerometer AccelerometerSource => accelerometerSource;

        /// <summary>
        /// Allow injection of an accelerometer for testing or simulation. If not set, it defaults to native.
        /// </summary>
        /// <param name="accelerometer"></param>
        public SensorFeeds SetAccelerometer(Sensors.IAccelerometer accelerometer)
        {
            if (accelerometer != null)
            {
                if (accelerometer != accelerometerSource)
                {
                    var monitoring = accelerometerSource.IsMonitoring;
                    accelerometerSource.ReadingChanged -= Model.Accelerometer_ReadingChanged;
                    if (monitoring)
                        accelerometerSource.Stop();
                    accelerometerSource = accelerometer;
                    if (monitoring)
                        accelerometer.Start(SensorSpeed.UI);
                }
                accelerometerSource.ReadingChanged += Model.Accelerometer_ReadingChanged;
                Model.Recalculate();
            }

            return this;
        }

        #endregion

        #region Orientation

        private Sensors.IOrientationSensor orientationSensorSource = Sensors.OrientationSensor.Default;

        public Sensors.IOrientationSensor OrientationSensorSource => orientationSensorSource;

        /// <summary>
        /// Allow injection of an OrientationSensor for testing or simulation.
        /// OrientationSensor must be injected before initializing manager instance, or it defaults to native.
        /// </summary>
        /// <param name="orientationSensor"></param>
        public SensorFeeds SetOrientationSensor(Sensors.IOrientationSensor orientationSensor)
        {
            if (orientationSensor != null)
            {
                if (orientationSensor != orientationSensorSource)
                {
                    var monitoring = orientationSensorSource.IsMonitoring;
                    if (monitoring)
                    {
                        orientationSensorSource.Stop();
                    }
                    orientationSensorSource.ReadingChanged -= Model.OrientationSensorSource_ReadingChanged;
                    orientationSensorSource = orientationSensor;
                    if (monitoring)
                    {
                        orientationSensorSource.Start(SensorSpeed.UI);
                    }
                }
                orientationSensorSource.ReadingChanged += Model.OrientationSensorSource_ReadingChanged;
                Model.Recalculate();
            }
            return this;
        }

        #endregion

        #region Location

        private Sensors.IGeolocation geolocationSource = Sensors.Geolocation.Default;

        public Sensors.IGeolocation GeolocationSource => geolocationSource;

        /// <summary>
        /// Allow injection of an location sensor for testing or simulation. If not set, it defaults to native.
        /// GeolocationSensor must be injected before initializing manager instance.
        /// </summary>
        /// <param name="geolocation"></param>
        public SensorFeeds SetGeolocationSensor(Sensors.IGeolocation geolocation)
        {
            if (geolocation != null)
            {
                if (geolocation != geolocationSource)
                {

                    var listening = geolocationSource.IsListeningForeground;
                    if (listening)
                    {
                        geolocationSource.StopListeningForeground();
                    }
                    geolocationSource.LocationChanged -= Model.GeolocationSource_LocationChanged;
                    geolocationSource = geolocation;
                    if (listening)
                    {
                        var request = new GeolocationListeningRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(1));
                        geolocationSource.StartListeningForegroundAsync(request).Wait();
                    }
                }
                geolocationSource.LocationChanged += Model.GeolocationSource_LocationChanged;
                Model.Recalculate();
            }
            return this;
        }

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


        #endregion

        #region Compass

        private Sensors.ICompass compassSource = Sensors.Compass.Default;

        public Sensors.ICompass CompassSource => compassSource;

        /// <summary>
        /// Allow injection of an OrientationSensor for testing or simulation.
        /// OrientationSensor must be injected before initializing manager instance, or it defaults to native.
        /// </summary>
        /// <param name="compass"></param>
        public SensorFeeds SetCompass(Sensors.ICompass compass)
        {
            if (compass != null)
            {
                if (compassSource != compass)
                {
                    bool monitoring = compassSource.IsMonitoring;
                    if (monitoring)
                    {
                        compassSource.Stop();
                    }
                    compassSource.ReadingChanged -= Model.Compass_ReadingChanged;
                    var speed = (compassSource as BaseSensor<Double>)?.SensorSpeed ?? SensorSpeed.Default;
                    compassSource = compass;
                    if (monitoring)
                    {
                        compassSource.Start(speed);
                    }
                }
                compassSource.ReadingChanged += Model.Compass_ReadingChanged;
            }
            else
            {
                SetCompass(Sensors.Compass.Default);
            }
            return this;
        }

        #endregion

    }
}
