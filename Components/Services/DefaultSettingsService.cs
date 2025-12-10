using System;
using EasyBraking.Components.Interfaces;
using Microsoft.Maui.Devices.Sensors;

namespace EasyBraking.Components.Services
{
    internal class DefaultSettingsService : ISettingsService
    {
        // If Preferences is not defined elsewhere in your project, you need to implement or reference it.
        // Here is a minimal stub implementation for demonstration purposes.
        private static class Preferences
        {
            public static bool Get(string key, bool defaultValue) => defaultValue;
            public static string Get(string key, string defaultValue) => defaultValue;
            public static float Get(string key, float defaultValue) => defaultValue;
            public static int Get(string key, int defaultValue) => defaultValue;
            public static long Get(string key, long defaultValue) => defaultValue;
        }

        public bool GetBoolSetting(string key)
        {
            return Preferences.Get(key, false);
        }

        public DistanceUnits GetDistanceUnit()
        {
            var unit = Preferences.Get("DistanceUnit", "Miles");
            if (!Enum.TryParse(typeof(DistanceUnits), unit, out var pref))
                pref = DistanceUnits.Miles;
            return (DistanceUnits)pref;
        }

        public float GetFloatSetting(string key)
        {
            return Preferences.Get(key, 0.0F);
        }

        public int GetIntSetting(string key)
        {
            return Preferences.Get(key, 0);
        }

        public long GetLongSetting(string key)
        {
            return Preferences.Get(key, 0L);
        }

        public string GetStringSetting(string key)
        {
            return Preferences.Get(key, "");
        }

        public void SetDistanceUnit(DistanceUnits distance)
        {
            throw new NotImplementedException();
        }
    }
}
