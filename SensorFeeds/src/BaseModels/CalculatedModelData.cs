using MauiSensorFeeds.BaseModels;
using MauiSensorFeeds.Calculated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiSensorFeeds.BaseModels
{
    public class CalculatedModelData : SensorData<CalculatedModel>
    {
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
