using System.Text;

namespace EasyBraking.Components.Models
{
    public class SensorReading<T> : ISensorReadingRecord
    {
        public static StringBuilder SB = new StringBuilder();

        public SensorReading(DateTime timestamp, T reading, bool calculated, bool reported)
        {
            Timestamp = timestamp;
            Reading = reading;
            Calculated = calculated;
            Reported = reported;
            SensorType = String.Empty;
        }

        public DateTime Timestamp { get; set; }
        public T Reading { get; set; }
        public string SensorType { get; set; }
        public bool Calculated { get; set; }
        public bool Reported { get; set; }

        public string PrintReading()
        {
            var value = Reading?.ToString() ?? "none";
            return (value.Length > 25 ? value.Substring(0, 25) : value);
        }

        public string FixedWidthDesc()
        {
            SB.Clear();
            SB.Append(Timestamp.ToString("HH:mm:ss "));
            SB.Append(SensorType.PadRight(13));
            SB.Append(PrintReading().PadRight(25) + " ");
            SB.Append(Calculated ? " X  " : "    ");
            SB.Append(Reported ? " X " : "   ");
            return SB.ToString();
        }

    }
}
