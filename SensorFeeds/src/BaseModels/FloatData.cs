using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiSensorFeeds.BaseModels
{
    public class FloatData : SensorData<float>
    {
        public FloatData(string dataFilename) : base(dataFilename)
        { 
        }

        protected override string ConvertValueToCSV(float value)
        {
            return value.ToString();
        }

        protected override string CSVHeaderLine()
        {
            return "Ticks,Value";
        }

        protected override float ParseValueFromCSV(string data)
        {
            if (float.TryParse(data, out float value))
                return value;
            return 0.0F;
        }
    }
}
