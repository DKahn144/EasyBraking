using System;
using Microsoft.Maui.Devices.Sensors;


namespace MauiSensorFeeds.Extensions
{
    public static class SensorExtensions
    {
        /// <summary>
        /// Returns angle from north in radians formed by moving from location0 to location.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="location0"></param>
        /// <returns></returns>
        public static double AngleOfTravel(this Location location, Location? location0)
        {
            if (location0 != null &&
                location0.Latitude != 0 &&
                location.Latitude != 0 &&
                location0.Latitude != location.Latitude)
            {
                double tangent = (location0.Longitude - location.Longitude) / (location0.Latitude - location.Latitude);
                return Math.Atan(tangent);
            }
            return double.NaN;
        }

        public static float Round(this double value, int digits)
        {
            return (float)Math.Round(value, digits);
        }
    }
}
