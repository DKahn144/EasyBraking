using System.Numerics;

namespace EasyBraking.Components.Models
{
    public abstract class Vector3Model : SensorBufferModel<Vector3>
    {
        public Vector3Model(SensorManagerModel model, int maxSamplecount) : base(model, maxSamplecount)
        {
        }

        public override void AddWeightedSample(ref Vector3 weightedAvg, int i, Vector3 sample)
        {
            float X = weightedAvg.X + (i * sample.X);
            float Y = weightedAvg.Y + (i * sample.Y);
            float Z = weightedAvg.Z + (i * sample.Z);
            weightedAvg.X = X;
            weightedAvg.Y = Y;
            weightedAvg.Z = Z;
        }

        public override double CalculateSize(Vector3 value)
        {
            return (long)(value.Length() * 1000);
        }

        public override Vector3 CreateNew()
        {
            return new Vector3();
        }

        public override Vector3 DivideWeightedByTotalWeight(Vector3 weightedAvg, long m)
        {
            var avg = CreateNew();
            avg.X = weightedAvg.X / m;
            avg.Y = weightedAvg.Y / m;
            avg.Z = weightedAvg.Z / m;
            return avg;
        }
    }
}
