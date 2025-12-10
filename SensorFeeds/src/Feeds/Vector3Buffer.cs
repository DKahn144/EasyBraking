using MauiSensorFeeds.BaseModels;
using System.Numerics;
using System.Collections.Generic;

namespace MauiSensorFeeds.Feeds
{
    public partial class Vector3Buffer : SensorBuffer<Vector3>
    {
        public Vector3Buffer(ReadWriteSensor<Vector3> sensor, BufferingStrategy strategy) :
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
                var q1 = values[0];
                while (++n < values.Count)
                {
                    var q2 = values[n];
                    q1 = Vector3.Lerp(q1, q2, 0.5F);
                }
                return q1;
            }
            else
                return Vector3.Zero;
        }

    }
}
