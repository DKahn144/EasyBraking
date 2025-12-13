using CommunityToolkit.Maui.Storage;
using MauiSensorFeeds;
using MauiSensorFeeds.Feeds;
using MauiSensorFeeds.Interfaces;
using MauiSensorFeeds.Calculated;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace EasyBraking
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new MainPage()) { Title = "EASY!" };
            window.Destroying += async (s, e) =>
            {
                await CloseSensorsAsync();
            };
            return window;
        }

        protected async Task CloseSensorsAsync()
        {
            var feeds = SensorFeeds.GetSensorFeeds();
            feeds.AccelerometerSource?.Stop();
            feeds.OrientationSensorSource?.Stop();
            feeds.GeolocationSource?.Stop();
            feeds.CompassSource?.Stop();
            feeds.CalculatedModelSource?.Stop();
        }

        public override async void CloseWindow(Window window)
        {
            await CloseSensorsAsync();
            base.CloseWindow(window);
        }
    }
}
