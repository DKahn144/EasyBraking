using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MauiSensorFeeds.Data
{
    public class Vector3Data : SensorData<Vector3>
    {

        public Vector3Data(string filename) : base()
        {
            DataFileName = filename;
        }

        protected override string ConvertValueToCSV(Vector3 value, long ticks)
        {
            return $"{ticks},{value.X},{value.Y},{value.Z}";
        }

        protected override string CSVHeaderLine()
        {
            return "Ticks,X,Y,Z";
        }

        protected override Vector3 ParseValueFromCSV(string data, out long ticks)
        {
            ticks = 0;
            var parts = data.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 4)
            {
                long.TryParse(parts[0], out ticks);
                float.TryParse(parts[1], out float x);
                float.TryParse(parts[2], out float y);
                float.TryParse(parts[3], out float z);
                return new Vector3(x, y, z);
            }
            return Vector3.Zero;
        }
    }
}
