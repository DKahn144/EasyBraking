namespace MauiSensorFeeds.Interfaces
{
    public interface ISensorData
    {
        static abstract string DataSpacer { get; }
        string DataFileName { get; }
        bool IsCsv { get; }
        bool IsJson { get; }

        string GetFullPathname();
        void LoadFromFile();
        Stream WriteToStream();
    }
}