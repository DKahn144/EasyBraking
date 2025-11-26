using EasyBraking.Components.Services;

namespace EasyBraking.Components.Models
{
    public class LocationModel : SensorBufferModel<Location>
    {
        public Location CurrentLocation => this.CurrentValue;

        public LocationModel(SensorManagerModel model, int maxSampleCount) : base(model, maxSampleCount)
        {
            lastSample = CreateNew();
            lastSample.AltitudeReferenceSystem = AltitudeReferenceSystem.Ellipsoid;
            lastSample.ReducedAccuracy = false;
        }

        /// <summary>
        /// Return the direction of travel as the average of the angle from north
        /// </summary>
        /// <returns></returns>
        public double AngleOfTravel()
        {
            DistanceUnits units = SettingsMgr.Service.GetDistanceUnit();
            var recentValues = new List<double>();
            lock (samplesLock)
            {
                if (this.valueSamples.Count > 1)
                {
                    foreach (var loc in this.valueSamples)
                    {
                        recentValues.Add((lastSample.Longitude - loc.Longitude) / (lastSample.Latitude - loc.Latitude));
                    }
                    var avg = recentValues.Average();
                    return Math.Atan(avg);
                }
                else
                    return 0;
            }
        }

        public override void RecordReading(Location value)
        {
            lastSample = value;
            lastSample.AltitudeReferenceSystem = AltitudeReferenceSystem.Ellipsoid;
            base.RecordReading(value);
        }

        public override void AddWeightedSample(ref Location weightedAvg, int i, Location sample)
        {
            double lat = weightedAvg.Latitude + (i * sample.Latitude);
            double lon = weightedAvg.Longitude + (i * sample.Longitude);
            double alt = weightedAvg.Altitude.GetValueOrDefault(0) + (i * sample.Altitude.GetValueOrDefault(0));
            weightedAvg.Latitude = lat;
            weightedAvg.Longitude = lon;
            weightedAvg.Altitude = alt;
        }

        public override double CalculateSize(Location value)
        {
            return (long)((value.Longitude + value.Latitude + value.Altitude.GetValueOrDefault()) * 100);
        }

        public override async void Configure()
        {
            if (!Geolocation.IsListeningForeground || !this.isSupported)
            {
                GeolocationListeningRequest request = new GeolocationListeningRequest();
                request.DesiredAccuracy = GeolocationAccuracy.Best;
                request.MinimumTime = TimeSpan.FromSeconds(1);
                this.isSupported = await sensorManagerModel.GeolocationSensor.StartListeningForegroundAsync(request);
                Geolocation.LocationChanged += Geolocation_LocationChanged;
            }
        }

        private void Geolocation_LocationChanged(object? sender, GeolocationLocationChangedEventArgs e)
        {
            if (lastSample == null ||
                lastSample.Latitude != e.Location.Latitude ||
                lastSample.Longitude != e.Location.Longitude ||
                LastNotifyTime.AddMilliseconds(MinNotifyFrequencyMS) < DateTime.UtcNow)
            {
                this.RecordReading(e.Location);
            }
        }

        public override void Dispose()
        {
            if (sensorManagerModel.GeolocationSensor.IsListeningForeground)
            {
                sensorManagerModel.GeolocationSensor.StopListeningForeground();
                sensorManagerModel.GeolocationSensor.LocationChanged -= Geolocation_LocationChanged;
            }
            base.Dispose();
        }

        public override Location CreateNew()
        {
            var loc = new Location();
            return loc;
        }

        public override Location DivideWeightedByTotalWeight(Location weightedAvg, long m)
        {
            var avg = CreateNew();
            avg.Latitude = weightedAvg.Latitude / m;
            avg.Longitude = weightedAvg.Longitude / m;
            avg.Altitude = weightedAvg.Altitude / m;
            return avg;
        }

        public double AccelRate
        {
            get
            {
                double lastAccelRate = 0;
                if (valueSamples.Count > 2)
                {
                    Location lastLoc1;
                    Location lastLoc2;
                    Location lastLoc3;
                    long ticksInterval1;
                    long ticksInterval2;
                    double distance1;
                    double distance2;
                    lock (samplesLock)
                    {
                        lastLoc1 = valueSamples[valueSamples.Count - 1];
                        lastLoc2 = valueSamples[valueSamples.Count - 2];
                        lastLoc3 = valueSamples[valueSamples.Count - 3];
                        ticksInterval1 = lastLoc1.Timestamp.Ticks - lastLoc2.Timestamp.Ticks;
                        ticksInterval2 = lastLoc2.Timestamp.Ticks - lastLoc3.Timestamp.Ticks;
                        distance1 = lastLoc1.CalculateDistance(lastLoc2, DistanceUnits.Miles);
                        distance2 = lastLoc2.CalculateDistance(lastLoc3, DistanceUnits.Miles);
                    }
                    var speed1 = distance1 / (ticksInterval1 * 10000 * 3600);  // mph
                    var speed2 = distance2 / (ticksInterval2 * 10000 * 3600); // mph
                    var avgTime = (lastLoc1.Timestamp.Ticks - lastLoc2.Timestamp.Ticks) * 10000 / 2; // secs
                    lastAccelRate = (speed2 - speed1) / avgTime; // mph per second
                }
                return lastAccelRate;
            }
        }

        public double Speed { get; internal set; }
    }
}
