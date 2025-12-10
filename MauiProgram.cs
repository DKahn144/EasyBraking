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

namespace EasyBraking
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.UseMauiApp<App>();

            builder.Services.AddMauiBlazorWebView();

            AccelerometerSensor accelerometer = new AccelerometerBuffer();
            Orientation_Sensor orientationSensor = new OrientationBuffer();
            GeolocationSensor geolocation = new GeolocationSensor();

#if DEBUG
            builder.Services.AddHybridWebViewDeveloperTools();
            builder.Logging.AddDebug();

            // logging
            accelerometer.WriteInputDataFile = "accelerometerInputData.csv";
            accelerometer.WriteOutputDataFile = "accelerometerOutputData.csv";
            orientationSensor.WriteInputDataFile = "orientationInputData.csv";
            orientationSensor.WriteOutputDataFile = "orientationOutputData.csv";
            geolocation.WriteInputDataFile = "locationData.csv";

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

            return builder.Build();
        }
    }
}
