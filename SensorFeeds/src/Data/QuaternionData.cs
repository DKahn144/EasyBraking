using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MauiSensorFeeds.Data
{
    public class QuaternionData : SensorData<Quaternion>
    {
        public QuaternionData() { }
        public QuaternionData(string fileName) : base(fileName) 
        { 
        }

        protected override string ConvertValueToCSV(Quaternion value, long ticks)
        {
            return $"{ticks},{value.X},{value.Y},{value.Z},{value.W}";
        }

        protected override string CSVHeaderLine()
        {
            return "Ticks,X,Y,Z,W";
        }

        protected override Quaternion ParseValueFromCSV(string data, out long ticks)
        {
            string[] parts = data.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            ticks = 0;
            if (parts.Length >= 5)
            {
                long.TryParse(parts[0], out ticks);
                float.TryParse(parts[1], out float x);
                float.TryParse(parts[2], out float y);
                float.TryParse(parts[3], out float z);
                float.TryParse(parts[4], out float w);
                return new Quaternion(x, y, z, w);
            }
            return Quaternion.Zero;
        }
    }
}
