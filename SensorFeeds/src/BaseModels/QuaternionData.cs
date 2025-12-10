using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MauiSensorFeeds.BaseModels
{
    public class QuaternionData : SensorData<Quaternion>
    {
        public QuaternionData() { }
        public QuaternionData(string fileName) : base(fileName) 
        { 
        }

        protected override string ConvertValueToCSV(Quaternion value)
        {
            return $"{value.X},{value.Y},{value.Z},{value.W}";
        }

        protected override string CSVHeaderLine()
        {
            return "Ticks,X,Y,Z,W";
        }

        protected override Quaternion ParseValueFromCSV(string data)
        {
            string[] parts = data.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 4)
            {
                float.TryParse(parts[0], out float x);
                float.TryParse(parts[1], out float y);
                float.TryParse(parts[2], out float z);
                float.TryParse(parts[3], out float w);
                return new Quaternion(x, y, z, w);
            }
            else
            {
                throw new ArgumentException($"Could not parse a Quaternion (X,Y,Z,W) from string \"{data}\"");
            }
        }
    }
}
