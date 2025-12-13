using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Text;
using Microsoft.Maui.Animations;
using MauiSensorFeeds.Interfaces;

namespace MauiSensorFeeds.Data
{
    public abstract partial class SensorData<T> : ISensorData
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
                testValues[SensorFeeds.TicksSinceFeedStart()] = value;
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
            string pathName = GetFullPathname();
            if (File.Exists(pathName))
            {
                string testData = File.ReadAllText(pathName);
                var sets = testData.Split(DataSpacer, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (var groups in sets)
                {
                    var rows = groups.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    rows.RemoveAt(0);  // header line
                    foreach (var row in rows)
                    {
                        foreach (var line in rows)
                        {
                            if (line.IndexOf(',') > 0)
                            {
                                long tick = 0;
                                T? value = ParseValueFromCSV(line, out tick);
                                if (value != null)
                                    TestValues[tick] = value;
                            }
                        }
                    }
                }
            }
        }

        protected abstract T? ParseValueFromCSV(string data, out long ticks);

        protected void LoadFromJsonFile()
        {
            TestValues.Clear();
            string pathName = GetFullPathname();
            if (File.Exists(pathName))
            {
                string testData = File.ReadAllText(pathName);
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
                            ex.Data.Add("SensorData JSON File", pathName);
                            ex.Data.Add($"testData[{FilePosition}],JsonTexts[{StringPosition}]", jsonStr);
                            throw new Exception($"Parsing of json text failed. error={ex.Message}", ex);
                        }
                    }
                }
            }
        }

        protected string GetExternalFilesDir()
        {
            object? docsDirectory = MauiSensorFeeds.SensorFeeds.FilePath;
            /*
#if ANDROID
            docsDirectory = Android.App.Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads);
#elif IOS
            docsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#elif WINDOWS
            docsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#endif
            */
            return docsDirectory?.ToString() ?? "";
        }

        public string GetFullPathname()
        {
            //string filepath = Path.Combine(
            //    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), DataFileName);
            //return filepath;
            var docsDirectory = GetExternalFilesDir();
            string filename = Path.Combine(docsDirectory.ToString(), DataFileName);
            Console.WriteLine($"//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////SensorData: GetFullPathname: {filename}");
            return filename;

        }

        public Stream WriteToStream()
        {
            string path = GetFullPathname();
            string data = "";
            lock (valueListLock)
            {
                if (this.IsJson)
                {
                    data = SaveJsonToString();
                }
                else if (this.IsCsv)
                {
                    data = SaveCSVToString();
                }
                else if (this.DataFileName.Length > 0)
                {
                    throw new NotSupportedException($"Only .json and .csv file formats are supported, not {this.DataFileName}");
                }
                testValues.Clear();
            }
            return new MemoryStream(data.Select(c => (byte)c).ToArray<byte>());
        }

        protected string SaveCSVToString()
        {
            StringBuilder SB = new StringBuilder(DataSpacer);
            SB.AppendLine(CSVHeaderLine());
            foreach (var kvp in testValues)
            {
                SB.AppendLine(ConvertValueToCSV(kvp.Value, kvp.Key));
            }
            return SB.ToString();
        }

        protected string SaveJsonToString()
        {
            var data = JsonSerializer.Serialize<SortedList<long, T>>(testValues);
            data = DataSpacer + data;
            return data;
        }

        protected abstract string ConvertValueToCSV(T value, long ticks = 0);

        protected abstract string CSVHeaderLine();

        public void LoadTestValues(SortedList<long, T> values)
        {
            lock (valueListLock)
            {
                testValues = values;
            }
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
