using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Text;
using Microsoft.Maui.Animations;

namespace MauiSensorFeeds.BaseModels
{
    public abstract partial class SensorData<T>
    {
        private object valueListLock = new object();

        public static string DataSpacer => "\n---START OUTPUT---\n\r";

        protected SortedList<long, T> testValues = new SortedList<long, T>();

        public string DataFileName { get; internal set; } = String.Empty;

        public bool IsJson => DataFileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase);

        public bool IsCsv => DataFileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);

        public SensorData() { }

        public SensorData(string fileName) 
        { 
            DataFileName = fileName ?? String.Empty;
        }

        public SortedList<long, T> TestValues => testValues;

        public void AddValue(T value)
        {
            lock (valueListLock)
            {
                testValues.Add(SensorFeeds.TicksSinceFeedStart(), value);
            }
        }

        public void LoadFromFile()
        {
            lock (valueListLock)
            {
                if (this.IsJson)
                {
                    LoadFromJsonFile();
                }
                else if (this.IsCsv)
                {
                    LoadFromCSVFile();
                }
                else if (this.DataFileName.Length > 0)
                {
                    throw new NotSupportedException($"Only .json and .csv file formats are supported, not {DataFileName}");
                }
            }
        }

        protected void LoadFromCSVFile()
        {
            TestValues.Clear();
            if (File.Exists(DataFileName))
            {
                string testData = File.ReadAllText(DataFileName);
                var sets = testData.Split(DataSpacer, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (var groups in sets)
                {
                    var rows = groups.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    rows.RemoveAt(0);  // header line
                    foreach (var row in rows)
                    {
                        var lines = testData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        foreach (var line in lines)
                        {
                            if (line.IndexOf(',') > 0)
                            {
                                var tickNo = line.Substring(0, line.IndexOf(','));
                                long.TryParse(tickNo, out long tick);
                                var dataPart = line.Substring(line.IndexOf(',') + 1);
                                T value = ParseValueFromCSV(dataPart);
                                TestValues.Add(tick, value);
                            }
                        }
                    }
                }
            }
        }

        protected abstract T ParseValueFromCSV(string data);

        protected void LoadFromJsonFile()
        {
            TestValues.Clear();
            if (File.Exists(DataFileName))
            {
                string testData = File.ReadAllText(DataFileName);
                List<string> jsonTexts = new List<string>();
                string jsonStr = "";
                int FilePosition = 0;
                foreach (var item in testData.Split(DataSpacer, StringSplitOptions.RemoveEmptyEntries))
                {
                    jsonStr = item;
                    FilePosition++;
                    int StringPosition = 0;
                    foreach (string json in jsonTexts)
                    {
                        jsonStr = json;
                        StringPosition++;
                        try
                        {
                            SortedList<long, T>? values = JsonSerializer.Deserialize<SortedList<long, T>>(json);
                            if (values != null)
                            {
                                foreach (var kvp in values)
                                {
                                    TestValues.Add(kvp.Key, kvp.Value);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.Data.Add("SensorData JSON File", DataFileName);
                            ex.Data.Add($"testData[{FilePosition}],JsonTexts[{StringPosition}]", jsonStr);
                            throw new Exception($"Parsing of json text failed. error={ex.Message}", ex);
                        }
                    }
                }
            }
        }

        protected object GetExternalFilesDir()
        {
            object? docsDirectory = null;
#if ANDROID
            docsDirectory = Android.App.Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments);
#elif IOS
            docsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#elif WINDOWS
            docsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#endif
            return docsDirectory;
        }

        public void SaveToFile()
        {
            var docsDirectory = GetExternalFilesDir();
            string filePath = docsDirectory?.ToString() + Path.DirectorySeparatorChar + DataFileName;
            lock (valueListLock)
            {
                if (this.IsJson)
                {
                    SaveJsonToFile(filePath);
                }
                else if (this.IsCsv)
                {
                    SaveCSVToFile(filePath);
                }
                else if (this.DataFileName.Length > 0)
                {
                    throw new NotSupportedException($"Only .json and .csv file formats are supported, not {filePath}");
                }
                testValues.Clear();
            }
        }

        protected void SaveCSVToFile(string csvFilename)
        {
            StringBuilder SB = new StringBuilder(DataSpacer + CSVHeaderLine());
            foreach (var kvp in testValues)
            {
                SB.AppendLine($"{kvp.Key},{ConvertValueToCSV(kvp.Value)}");
            }
            File.AppendAllText(csvFilename, SB.ToString());
        }

        protected void SaveJsonToFile(string jsonFilename)
        {
            var data = JsonSerializer.Serialize<SortedList<long, T>>(testValues);
            data = DataSpacer + data;
            File.AppendAllText(jsonFilename, data);
        }

        protected abstract string ConvertValueToCSV(T value);
        
        protected abstract string CSVHeaderLine();

        public void LoadTestValues(SortedList<long, T> values)
        {
            lock(valueListLock)
            {
                testValues = values;
            }
        }

        public void Flush()
        {
            SaveToFile();
        }

        public List<T> ValuesLastNo(int maxBufferSize)
        {
            lock (valueListLock)
            {
                if (testValues.Count > maxBufferSize)
                    return testValues
                        .Skip(testValues.Count - maxBufferSize)
                        .Select(kp => kp.Value)
                        .ToList();
                else
                    return testValues.Values.ToList();
            }
        }

        public List<T> ValuesFrom(DateTime fromTime)
        {
            lock (valueListLock)
            {
                long fromTicks = fromTime.Ticks - SensorFeeds.FeedStart.Ticks;
                var list = testValues
                    .Where(kvp => kvp.Key >= fromTicks)
                    .Select(kvp => kvp.Value)
                    .ToList();
                if (list.Count == 0 && testValues.Count > 0)
                    list.Add(testValues.Values[testValues.Count - 1]);
                return list;
            }
        }
    }
}
