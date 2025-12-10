namespace MauiSensorFeeds.Calculated
{
    public class CalculatedModelChangedEventArgs
    {
        public CalculatedModel Model;

        public CalculatedModelChangedEventArgs(CalculatedModel model) 
        {
            Model = model;
        }
    }
}