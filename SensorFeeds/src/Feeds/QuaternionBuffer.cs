using MauiSensorFeeds.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MauiSensorFeeds.Feeds
{
    public class QuaternionBuffer : SensorBuffer<Quaternion>
    {
        public QuaternionBuffer(ReadWriteSensor<Quaternion> sensor, BufferingStrategy strategy)
            : base(sensor, strategy)
        {
        }

        /// <summary>
        /// This is more like smoothing than averaging.
        /// Lerping gives greater weight to the more recent measures.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public override Quaternion GetAverageValue(List<Quaternion> values)
        {
            if (values.Count > 0)
            {
                int n = 0;
                Quaternion q1 = values[0];
                while (++n < values.Count)
                {
                    Quaternion q2 = values[n];
                    q1 = Quaternion.Lerp(q1, q2, 0.5F);
                }
                return q1;
            }
            else
            {
                return Quaternion.Zero;
            }
        }
    }
}
