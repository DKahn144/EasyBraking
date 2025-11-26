using EasyBraking.Components.Models;

namespace EasyBraking.Components.Interfaces
{
    /// <summary>
    /// Keep log data of readings and calculations.
    /// </summary>
    internal interface IDataLogger<T>
    {
        SensorReading<T>[] GetLogReadings(string typename);

        void AddReading(DateTime timestamp, T reading, bool calculated);

        void ReportLastReading();
    }
}
