using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.Controls; // Add this using directive

namespace EasyBraking
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            //OnStartup();
        }

        private bool OnStartup()
        {
            if (MauiSensorFeeds.SensorFeeds.FilePath == "?")
            {
                var result = FolderPicker.PickAsync(CancellationToken.None).Result;
                if (result.IsSuccessful)
                {
                    MauiSensorFeeds.SensorFeeds.FilePath = result.Folder.Path;
                    Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>" + MauiSensorFeeds.SensorFeeds.FilePath);
                    return true;
                }
            }
            return false;
        }
    }
}
