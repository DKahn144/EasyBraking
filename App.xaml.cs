using Microsoft.Maui;
using Microsoft.Maui.Controls;
using MauiSensorFeeds;

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
            window.Deactivated += (s, e) =>
            {
                CloseSensors();
            };
            return window;
        }

        protected void CloseSensors()
        {
            var feeds = SensorFeeds.GetSensorFeeds();
            feeds.AccelerometerSource?.Stop();
            feeds.OrientationSensorSource?.Stop();
            feeds.GeolocationSource?.StopListeningForeground();
            feeds.CompassSource?.Stop();
        }

        public override void CloseWindow(Window window)
        {
            CloseSensors();
            base.CloseWindow(window);
        }
    }
}
