namespace MauiSensorFeeds.BaseModels
{
    /// <summary>
    /// Determines how the app selects a recent sensor reading from a buffered list.
    /// </summary>
    public enum BufferingStrategy
    {
        Default,
        MostRecentReading,
        AverageOfLast10Readings,
        AverageOfLast100Readings,
        AverageOfLast1000Readings,
        AverageOfLast10Milliseconds,
        AverageOfLast100Milliseconds,
        AverageOfLast1000Milliseconds
    }
}
