using MauiSensorFeeds.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;

namespace MauiSensorFeeds.BaseModels
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

        protected override string ConvertValueToCSV(Location value)
        {
            return $"{value.Latitude},{value.Longitude},{value.Altitude},{value.Course},{value.Speed},{value.VerticalAccuracy}";
        }

        protected override string CSVHeaderLine()
        {
            return $"Ticks,Latitude,Longitude,Altitude,Course,Speed,VerticalAccuracy";
        }

        protected override Location ParseValueFromCSV(string data)
        {
            var parts = data.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 6)
            {
                double.TryParse(parts[0], out double latitude);
                double.TryParse(parts[1], out double longitude);
                double.TryParse(parts[2], out double altitude);
                double.TryParse(parts[3], out double course);
                double.TryParse(parts[4], out double speed);
                double.TryParse(parts[5], out double verticalAccuracy);
                var loc = new Location(latitude, longitude, altitude);
                loc.Course = course;
                loc.Speed = speed;
                loc.VerticalAccuracy = verticalAccuracy;
                return loc;
            }
            else
            {
                throw new ArgumentException($"Could not parse a Location (Latitude,Longitude,Altitude,Course,Speed,VerticalAccuracy) from string \"{data}\"");
            }
        }
    }
}
