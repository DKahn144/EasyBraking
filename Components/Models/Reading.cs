namespace EasyBraking.Components.Models
{
    public class Reading
    {
        public Reading(double value)
        {
            Value = value;
            TimeStamp = DateTime.Now;
        }

        public double Value { get; private set; }
        public DateTime TimeStamp { get; private set; }

        public static int MaxCount = 100;
        public static int SinceMillisecs = 1000;

        public static double GetAvgValue(List<Reading> readings)
        {
            TrimOldReadings(readings);
            return readings.Count > 0 ? readings.Average(r => r.Value) : 0;
        }

        public static void AddValue(List<Reading> readings, Reading reading)
        {
            if (readings != null && reading != null)
            {
                while (readings.Count >= 100)
                {
                    readings.RemoveAt(readings.Count - 1);
                }
                readings.Insert(0, reading);
            }
        }

        private static void TrimOldReadings(List<Reading> readings)
        {
            var sinceTime = DateTime.Now.AddMilliseconds(-SinceMillisecs);
            foreach (var rdg in readings.ToArray())
            {
                if (rdg.TimeStamp < sinceTime)
                    readings.Remove(rdg);
            }
        }

    }
}