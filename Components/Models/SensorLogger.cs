using EasyBraking.Components.Interfaces;

namespace EasyBraking.Components.Models
{
    public class SensorLogger<T> : IDataLogger<T>
    {

        private List<SensorReading<T>> SendorReadings = new List<SensorReading<T>>();
        private object SensorReadingsLock = new object();

        public SensorReading<T>[] GetLogReadings(string typeName)
        {
            lock (SensorReadingsLock)
            {
                var readings = new SensorReading<T>[SendorReadings.Count];
                SendorReadings.ForEach(r => r.SensorType = typeName);
                SendorReadings.CopyTo(readings);
                SendorReadings.Clear();
                return readings;
            }
        }

        public void AddReading(DateTime timestamp, T reading, bool calculated)
        {
            lock (SensorReadingsLock)
            {
                SensorReading<T>? lastReading = null;
                if (SendorReadings.Count > 0)
                {
                    lastReading = SendorReadings[SendorReadings.Count - 1];
                }
                if (reading != null &&
                    (lastReading == null ||
                     lastReading.Reading == null ||
                     !String.Equals(lastReading.Reading.ToString(), reading.ToString())))
                {
                    var readingObj = new SensorReading<T>(timestamp, reading, calculated, false);
                    SendorReadings.Add(readingObj);
                }
            }
        }

        public void ReportLastReading()
        {
            lock (SensorReadingsLock)
            {
                if (SendorReadings.Count > 0)
                {
                    var lastReading = SendorReadings[SendorReadings.Count - 1];
                    lastReading.Reported = true;
                }
            }
        }
    }
}
