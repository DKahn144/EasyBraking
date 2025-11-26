using EasyBraking.Components.Interfaces;

namespace EasyBraking.Components.Services
{
    internal class DefaultSettingsService : ISettingsService
    {
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
