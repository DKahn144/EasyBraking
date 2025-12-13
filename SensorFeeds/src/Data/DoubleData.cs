using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiSensorFeeds.Data
{
    public class DoubleData : SensorData<double>
    {
        public DoubleData(string dataFilename) : base(dataFilename)
        {
        }

        protected override string ConvertValueToCSV(double value, long ticks)
        {
            return $"{ticks},{value}";
        }

        protected override string CSVHeaderLine()
        {
            return "Ticks,Value";
        }

        protected override double ParseValueFromCSV(string data, out long ticks)
        {
            ticks = 0;
            var parts = data.Split(',');
            if (parts.Length >= 2)
            {
                long.TryParse(parts[0], out ticks);
                if (double.TryParse(parts[1], out double value))
                    return value;
            }
            return 0;
        }
    }
}
