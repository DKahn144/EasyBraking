using MauiSensorFeeds.BaseModels;
using MauiSensorFeeds.Interfaces;
using MauiSensorFeeds.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MauiSensorFeeds.Feeds
{
    public class GeolocationSensor : ReadWriteSensor<Location>, IGeolocation
    {
        protected IGeolocation geolocation = Geolocation.Default;

        private bool isListeningForeground = false;

        public bool IsListeningForeground => isListeningForeground;

        public GeolocationSensor(string testDataFile) : base(SensorType.Location) 
        {
            this.ReadDataFile = testDataFile;
        }

        public GeolocationSensor() : base(SensorType.Location) 
        {
        }

        public void GeolocationSensor_LocationChanged(object? sender, GeolocationLocationChangedEventArgs e)
        {
            ReadingChangeEvent(e.Location);
        }

        public void GeolocationSensor_ListeningFailed(object? sender, GeolocationListeningFailedEventArgs e)
        {
            var location = new Location();
            if (e.Error == GeolocationError.Unauthorized)
            {
                location.Latitude = 0;
                location.Longitude = 0;
                ReadingChangeEvent(location);
            }
        }

        public async Task<Location?> GetLastKnownLocationAsync()
        {
            return await geolocation.GetLastKnownLocationAsync();
        }

        public async Task<Location?> GetLocationAsync(GeolocationRequest request, CancellationToken cancelToken)
        {
            return await geolocation.GetLocationAsync(request, cancelToken);
        }

        public async override void Start(SensorSpeed speed = SensorSpeed.Default)
        {
            GeolocationListeningRequest request = 
                new GeolocationListeningRequest(GeolocationAccuracy.Default, TimeSpan.FromSeconds(1));
            await StartListeningForegroundAsync(request);
        }

        public async Task<bool> StartListeningForegroundAsync(GeolocationListeningRequest request)
        {
            if (IsReadingFromFile)
            {
                StartReadingData();
                isListeningForeground = true;
            }
            else
            {
                AttachEventListeners();
                if (!geolocation.IsListeningForeground)
                {
                    try
                    {
                        isListeningForeground = await geolocation.StartListeningForegroundAsync(request);
                    }
                    catch (Exception ex)
                    {
                        isListeningForeground = false;
                        var args = new GeolocationListeningFailedEventArgs(GeolocationError.PositionUnavailable);
                        ListeningFailed?.Invoke(this, args);
                    }
                }
            }
            return isListeningForeground;
        }

        public override void Stop()
        {
            StopListeningForeground();
        }

        public void StopListeningForeground()
        {
            isListeningForeground = false;
            if (!IsReadingFromFile)
            {
                geolocation.StopListeningForeground();
                DetachEventListeners();
                //SensorInputData?.SaveToFile();
                //SensorOutputData?.SaveToFile();
            }
        }
        
        protected override void AttachEventListeners()
        {
            base.AttachEventListeners();
            geolocation.LocationChanged += GeolocationSensor_LocationChanged;
            geolocation.ListeningFailed += GeolocationSensor_ListeningFailed;
        }

        protected override void DetachEventListeners()
        {
            base.DetachEventListeners();
            geolocation.LocationChanged -= GeolocationSensor_LocationChanged;
            geolocation.ListeningFailed -= GeolocationSensor_ListeningFailed;
        }

        protected override Location CreateNew()
        {
            return new Location();
        }

        protected override float ValueOf(Location? value)
        {
            return (float) (value?.Latitude + value?.Longitude ?? 0);
        }

        protected override void NotifyReadingChange(Location value)
        {
            LocationChanged?.Invoke(this, 
                new GeolocationLocationChangedEventArgs(value));
        }

        public override SensorData<Location> GenericSensorData(string dataFilename)
        {
            return new LocationData(dataFilename);
        }

        #region events

        public event EventHandler<GeolocationLocationChangedEventArgs>? LocationChanged;
        public event EventHandler<GeolocationListeningFailedEventArgs>? ListeningFailed;

        public override void SetHandlerToEvent(object handler)
        {
            if (handler is EventHandler<GeolocationLocationChangedEventArgs>)
                this.LocationChanged += handler as EventHandler<GeolocationLocationChangedEventArgs>;
            if (handler is EventHandler<GeolocationListeningFailedEventArgs>)
                this.ListeningFailed += handler as EventHandler<GeolocationListeningFailedEventArgs>;
        }

        public override void UnsetHandlerToEvent(object handler)
        {
            if (handler is EventHandler<GeolocationLocationChangedEventArgs>)
                this.LocationChanged -= handler as EventHandler<GeolocationLocationChangedEventArgs>;
            if (handler is EventHandler<GeolocationListeningFailedEventArgs>)
                this.ListeningFailed -= handler as EventHandler<GeolocationListeningFailedEventArgs>;
        }
        #endregion
    }
}
