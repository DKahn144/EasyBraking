namespace EasyBraking.Components.Models
{
    public interface ISensorReadingRecord
    {

        DateTime Timestamp { get; }

        bool Calculated { get; }

        bool Reported { get; }

        string SensorType { get; }

        string PrintReading();

        string FixedWidthDesc();

    }
}