using MauiSensorFeeds.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MauiSensorFeeds.Feeds
{
    public class QuaternionBuffer2 : SensorBuffer<Quaternion>
    {
        public QuaternionBuffer2(ReadWriteSensor<Quaternion> sensor, BufferingStrategy strategy)
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
                var xAvg = values.Select(q=> q.X).Sum() / values.Count;
                var yAvg = values.Select(q=> q.Y).Sum() / values.Count;
                var zAvg = values.Select(q=> q.Z).Sum() / values.Count;
                var wAvg = values.Select(q=> q.W).Sum() / values.Count;
                Quaternion q1 = new Quaternion(xAvg, yAvg, zAvg, wAvg);
                return q1;
            }
            else
            {
                return Quaternion.Zero;
            }
        }
    }
}
