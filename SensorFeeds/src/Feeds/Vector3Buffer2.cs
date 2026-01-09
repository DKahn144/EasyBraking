using MauiSensorFeeds.BaseModels;
using System.Numerics;
using System.Collections.Generic;

namespace MauiSensorFeeds.Feeds
{
    public partial class Vector3Buffer2 : SensorBuffer<Vector3>
    {
        public Vector3Buffer2(ReadWriteSensor<Vector3> sensor, BufferingStrategy strategy) :
            base(sensor, strategy)
        {
        }

        /// <summary>
        /// This is more like smoothing than averaging.
        /// Lerping gives greater weight to the more recent measures.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public override Vector3 GetAverageValue(List<Vector3> values)
        {
            if (values.Count > 0)
            {
                int n = 0;
                var xAvg = values.Select(v => v.X).Sum()/values.Count;
                var yAvg = values.Select(v => v.Y).Sum()/values.Count;
                var zAvg = values.Select(v => v.Z).Sum()/values.Count;
                var q1 = new Vector3(xAvg, yAvg, zAvg);
                return q1;
            }
            else
                return Vector3.Zero;
        }

    }
}
