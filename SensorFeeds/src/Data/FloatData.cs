using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiSensorFeeds.Data
{
    public class FloatData : SensorData<float>
    {
        public FloatData(string dataFilename) : base(dataFilename)
        { 
        }

        protected override string ConvertValueToCSV(float value, long ticks)
        {
            return $"{ticks},{value}";
        }

        protected override string CSVHeaderLine()
        {
            return "Ticks,Value";
        }

        protected override float ParseValueFromCSV(string data, out long ticks)
        {
            ticks = 0;
            var parts = data.Split(',');
            if (parts.Length >= 2)
            {
                long.TryParse(parts[0], out ticks);
                if (float.TryParse(parts[1], out float value))
                    return value;
            }
            return 0;
        }
    }
}
