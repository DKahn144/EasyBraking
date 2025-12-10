using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiSensorFeeds.BaseModels
{
    public class DoubleData : SensorData<double>
    {
        public DoubleData(string dataFilename) : base(dataFilename)
        {
        }

        protected override string ConvertValueToCSV(double value)
        {
            return value.ToString();
        }

        protected override string CSVHeaderLine()
        {
            return "Ticks,Value";
        }

        protected override double ParseValueFromCSV(string data)
        {
            if (double.TryParse(data, out double value))
                return value;
            return 0.0F;
        }
    }
}
