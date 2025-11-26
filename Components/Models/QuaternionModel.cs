using System.Numerics;

namespace EasyBraking.Components.Models
{
    public abstract class QuaternionModel : SensorBufferModel<Quaternion>
    {
        public QuaternionModel(SensorManagerModel model, int count) : base(model, count)
        {
        }

        public override void AddWeightedSample(ref Quaternion weightedAvg, int i, Quaternion sample)
        {
            float X = weightedAvg.X + (i * sample.X);
            float Y = weightedAvg.Y + (i * sample.Y);
            float Z = weightedAvg.Z + (i * sample.Z);
            float W = weightedAvg.W + (i * sample.W);
            weightedAvg.X = X;
            weightedAvg.Y = Y;
            weightedAvg.Z = Z;
            weightedAvg.W = W;
        }

        public override double CalculateSize(Quaternion value)
        {
            return (long)(value.Length() * 1000);
        }

        public override Quaternion CreateNew()
        {
            return new Quaternion();
        }

        public override Quaternion DivideWeightedByTotalWeight(Quaternion weightedAvg, long m)
        {
            var avg = CreateNew();
            avg.X = weightedAvg.X / m;
            avg.Y = weightedAvg.Y / m;
            avg.Z = weightedAvg.Z / m;
            avg.W = weightedAvg.W / m;
            return avg;
        }
    }
}
