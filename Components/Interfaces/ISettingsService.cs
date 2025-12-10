using Microsoft.Maui.Devices.Sensors;

namespace EasyBraking.Components.Interfaces
{
    /// <summary>
    /// Injectible settings service.
    /// </summary>
    public interface ISettingsService
    {
        string GetStringSetting(string key);

        int GetIntSetting(string key);

        long GetLongSetting(string key);

        float GetFloatSetting(string key);

        bool GetBoolSetting(string key);

        DistanceUnits GetDistanceUnit();

        void SetDistanceUnit(DistanceUnits distance);
    }
}
