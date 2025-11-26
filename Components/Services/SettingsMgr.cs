using EasyBraking.Components.Interfaces;

namespace EasyBraking.Components.Services
{
    internal static class SettingsMgr
    {
        static ISettingsService CurrentService = new DefaultSettingsService();

        public static void SetSettingsService(ISettingsService service)
        {
            if (service != null)
            {
                CurrentService = service;
            }
        }

        public static ISettingsService Service => CurrentService;
    }
}
