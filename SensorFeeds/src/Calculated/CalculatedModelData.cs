using MauiSensorFeeds.BaseModels;

namespace MauiSensorFeeds.Calculated
{
    internal class CalculatedModelData : SensorData<CalculatedModel>
    {
        public CalculatedModelData(string fileName) : base(fileName)
        {
        }

        protected override string ConvertValueToCSV(CalculatedModel value)
        {
            throw new NotImplementedException();
        }

        protected override string CSVHeaderLine()
        {
            throw new NotImplementedException();
        }

        protected override CalculatedModel ParseValueFromCSV(string data)
        {
            throw new NotImplementedException();
        }
    }
}