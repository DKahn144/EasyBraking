using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;

namespace MauiSensorFeeds.Data
{
    public class LocationData : SensorData<Location>
    {
        public LocationData(string filename) : base()
        {
            DataFileName = filename;
        }

        public LocationData() : base()
        {
            DataFileName = "";
        }

        protected override string ConvertValueToCSV(Location value, long ticks)
        {
            return $"{ticks},{value.Latitude},{value.Longitude},{value.Altitude},{value.Course},{value.Speed},{value.VerticalAccuracy}";
        }

        protected override string CSVHeaderLine()
        {
            return $"Ticks,Latitude,Longitude,Altitude,Course,Speed,VerticalAccuracy";
        }

        protected override Location? ParseValueFromCSV(string data, out long ticks)
        {
            ticks = 0;
            long locticks = 0;
            var parts = data.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 7)
            {
                long.TryParse(parts[0], out ticks);
                double.TryParse(parts[1], out double latitude);
                double.TryParse(parts[2], out double longitude);
                double.TryParse(parts[3], out double altitude);
                double.TryParse(parts[4], out double course);
                double.TryParse(parts[5], out double speed);
                double.TryParse(parts[6], out double verticalAccuracy);
                if (parts.Length > 7)
                    long.TryParse(parts[7], out locticks);
                var loc = new Location(latitude, longitude, altitude);
                loc.Course = course;
                loc.Speed = speed;
                loc.VerticalAccuracy = verticalAccuracy;
                loc.Timestamp = SensorFeeds.FeedStart.AddTicks(locticks);
                return loc;
            }
            return null;
        }
    }
}
