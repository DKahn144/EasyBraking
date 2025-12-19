using MauiSensorFeeds.BaseModels;
using MauiSensorFeeds.Feeds;
using Microsoft.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Hosting;
using System;
using MauiSensorFeeds.Interfaces;
using Sensors =  Microsoft.Maui.Devices.Sensors;
using MauiSensorFeeds.Calculated;
using MauiSensorFeeds;
using CommunityToolkit.Maui;

namespace EasyBraking
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder.UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            AccelerometerSensor accelerometer;
            Orientation_Sensor orientationSensor;
            GeolocationSensor geolocation;
            CalculatedModelSensor calculatedModelSensor;
            CompassSensor compass;
            try
            {
                accelerometer = new AccelerometerBuffer();
                orientationSensor = new OrientationBuffer();
                geolocation = new GeolocationSensor();
                compass = new CompassSensor();
                calculatedModelSensor = new CalculatedModelSensor();
            }
            catch (Exception ex)
            {
                throw new Exception("Error initializing sensors: " + ex.Message);
            }
#if DEBUG
            builder.Services.AddHybridWebViewDeveloperTools();
            builder.Logging.AddDebug();

            // logging
            /*
            accelerometer.WriteInputDataFile = "accelerometerInputData.csv";
            orientationSensor.WriteInputDataFile = "orientationInputData.csv";
            geolocation.WriteInputDataFile = "locationData.csv";
            accelerometer.ReadDataFile = "accelerometerInputData.csv";
            accelerometer.WriteOutputDataFile = "accelerometerOutputData.csv";
            orientationSensor.ReadDataFile = "orientationInputData.csv";
            calculatedModelSensor.WriteOutputDataFile = "calculatedModelOutputData.csv";
            */

            // testing
            /*
            accelerometer.ReadDataFile = "accelerometerTestData.cvs";
            orientationSensor.ReadDataFile = "orientationTestData.cvs";
            geolocation.ReadDataFile = "locationTestData.cvs";
            */

#else

#endif
            builder.Services.AddSingleton(typeof(IAccelerometer), accelerometer);
            builder.Services.AddSingleton(typeof(IOrientationSensor), orientationSensor);               
            builder.Services.AddSingleton(typeof(IGeolocation), geolocation);
            builder.Services.AddSingleton(typeof(CalculatedModelSensor), calculatedModelSensor);

            return builder.Build();
        }
    }
}
