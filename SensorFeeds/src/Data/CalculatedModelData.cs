using MauiSensorFeeds.Calculated;
using System.Numerics;
using System.Text;

namespace MauiSensorFeeds.Data
{
    public class CalculatedModelData : SensorData<CalculatedModel>
    {
        public CalculatedModelData(string fileName) : base(fileName)
        {
        }

        protected override string ConvertValueToCSV(CalculatedModel value, long timestamp = 0)
        {
            StringBuilder result = new StringBuilder();
            string[] labels = CSVHeaderLine().Split(',');
            foreach (string lbl in labels)
            {
                if (lbl == "Ticks")
                    result.Append($"{timestamp},");
                if (lbl == "Orientation.X")
                    result.Append($"{value.Orientation.X},");
                if (lbl == "Orientation.Y")
                    result.Append($"{value.Orientation.Y},");
                if (lbl == "Orientation.Z")
                    result.Append($"{value.Orientation.Z},");
                if (lbl == "Orientation.W")
                    result.Append($"{value.Orientation.W},");
                if (lbl == "Acceleration.X")
                    result.Append($"{value.Acceleration.X},");
                if (lbl == "Acceleration.Y")
                    result.Append($"{value.Acceleration.Y},");
                if (lbl == "Acceleration.Z")
                    result.Append($"{value.Acceleration.Z},");
                if (lbl == "Location.Latitude")
                    result.Append($"{value.Location.Latitude},");
                if (lbl == "Location.Longitude")
                    result.Append($"{value.Location.Longitude},");
                if (lbl == "Location.Altitude")
                    result.Append($"{value.Location.Altitude},");
                if (lbl == "Location.Course")
                    result.Append($"{value.Location.Course},");
                if (lbl == "Location.Speed")
                    result.Append($"{value.Location.Speed},");
                if (lbl == "AbsoluteAcceleration.X")
                    result.Append($"{value.AbsoluteAcceleration.X},");
                if (lbl == "AbsoluteAcceleration.Y")
                    result.Append($"{value.AbsoluteAcceleration.Y},");
                if (lbl == "AbsoluteAcceleration.Z")
                    result.Append($"{value.AbsoluteAcceleration.Z},");
                if (lbl == "AbsoluteAcceleration.W")
                    result.Append($"{value.AbsoluteAcceleration.W},");
            }var line = result.ToString().TrimEnd(',');
            return line;
        }

        protected override string CSVHeaderLine()
        {
            return "Ticks,Orientation.X,Orientation.Y,Orientation.Z,Orientation.W," +
                "Acceleration.X,Acceleration.Y,Acceleration.Z," +
                "Location.Latitude,Location.Longitude,Location.Altitude," +
                "Location.Course,Location.Speed,AbsoluteAcceleration.X," +
                "AbsoluteAcceleration.Y,AbsoluteAcceleration.Z,AbsoluteAcceleration.W";
        }

        protected override CalculatedModel ParseValueFromCSV(string data, out long ticks)
        {
            ticks = 0;
            string[] labels = CSVHeaderLine().Split(',');
            string[] values = data.Split(',');
            var modelValues = new Dictionary<string, string>();
            for(int i = 0; i < labels.Length; i++)
            {
                if (values.Length >= i + 1)
                    modelValues.Add(labels[i], values[i]);
            }
            CalculatedModel model = CalculatedModel.GetModel();
            var orient = new Quaternion();
            var accel = new Vector3();
            var loc = new Location();
            var absAccel = new Quaternion();
            foreach(var lbl in modelValues.Keys)
            {
                if (lbl == "Ticks" && long.TryParse(modelValues[lbl], out ticks))
                    model.TicksIndex = ticks;
                else if (lbl == "Orientation.X" && float.TryParse(modelValues[lbl], out float oriX))
                    orient.X = oriX;
                else if (lbl == "Orientation.Y" && float.TryParse(modelValues[lbl], out float oriY))
                    orient.Y = oriY;
                else if (lbl == "Orientation.Z" && float.TryParse(modelValues[lbl], out float oriZ))
                    orient.Z = oriZ;
                else if (lbl == "Orientation.W" && float.TryParse(modelValues[lbl], out float oriW))
                    orient.W = oriW;
                else if (lbl == "Acceleration.X" && float.TryParse(modelValues[lbl], out float accX))
                    accel.X = accX;
                else if (lbl == "Acceleration.Y" && float.TryParse(modelValues[lbl], out float accY))
                    accel.Y = accY;
                else if (lbl == "Acceleration.Z" && float.TryParse(modelValues[lbl], out float accZ))
                    accel.Z = accZ;
                else if (lbl == "Location.Latitude" && double.TryParse(modelValues[lbl], out double lat))
                    loc.Latitude = lat;
                else if (lbl == "Location.Longitude" && double.TryParse(modelValues[lbl], out double longit))
                    loc.Longitude = longit;
                else if (lbl == "Location.Altitude" && double.TryParse(modelValues[lbl], out double altit))
                    loc.Altitude = altit;
                else if (lbl == "Location.Course" && double.TryParse(modelValues[lbl], out double course))
                    loc.Course = course;
                else if (lbl == "Location.Speed" && double.TryParse(modelValues[lbl], out double speed))
                    loc.Speed = speed;
                else if (lbl == "Location.Timestamp" && long.TryParse(modelValues[lbl], out long tsticks))
                    loc.Timestamp = SensorFeeds.FeedStart.AddTicks(tsticks);
                else if (lbl == "AbsoluteAcceleration.X" && float.TryParse(modelValues[lbl], out float absX))
                    absAccel.X = absX;
                else if (lbl == "AbsoluteAcceleration.X" && float.TryParse(modelValues[lbl], out float absY))
                    absAccel.Y = absY;
                else if (lbl == "AbsoluteAcceleration.X" && float.TryParse(modelValues[lbl], out float absZ))
                    absAccel.Z = absZ;
                else if (lbl == "AbsoluteAcceleration.X" && float.TryParse(modelValues[lbl], out float absW))
                    absAccel.W = absW;
            }
            model.Orientation = orient;
            model.Acceleration = accel;
            model.Location = loc;
            model.AbsoluteAcceleration = absAccel;
            return model;
        }
    }
}