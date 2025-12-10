using MauiSensorFeeds.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MauiSensorFeeds.BaseModels
{
    public class Vector3Data : SensorData<Vector3>
    {

        public Vector3Data(string filename) : base()
        {
            DataFileName = filename;
        }

        protected override string ConvertValueToCSV(Vector3 value)
        {
            return $"{value.X},{value.Y},{value.Z}";
        }

        protected override string CSVHeaderLine()
        {
            return "Ticks,X,Y,Z";
        }

        protected override Vector3 ParseValueFromCSV(string data)
        {
            var parts = data.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 3)
            {
                float.TryParse(parts[0], out float x);
                float.TryParse(parts[1], out float y);
                float.TryParse(parts[2], out float z);
                return new Vector3(x, y, z);
            }
            else
            {
                throw new ArgumentException($"Could not parse a Vector3 (X,Y,Z) from string \"{data}\"");
            }
        }
    }
}
