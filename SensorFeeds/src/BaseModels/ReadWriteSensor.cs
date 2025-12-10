using System;
using System.IO;
using System.Collections.Generic;
using MauiSensorFeeds.BaseModels;

namespace MauiSensorFeeds.BaseModels
{
    /// <summary>
    /// A test sensor that supports reading or writing of data points of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ReadWriteSensor<T> : BaseSensor<T>, IDisposable
    {
        protected string readDataFile = string.Empty;
        protected string writeInputDataFile = string.Empty;
        protected string writeOutputDataFile = string.Empty;
        protected DateTime LastInputReadingTime = DateTime.MinValue;
        protected DateTime LastOutputReadingTime = DateTime.MinValue;

        #region public properties

        public bool IsReadingFromFile => !string.IsNullOrEmpty(readDataFile);
        public bool IsWritingInputToFile => !string.IsNullOrEmpty(writeInputDataFile);
        public bool IsWritingOutputToFile => !string.IsNullOrEmpty(writeOutputDataFile);

        public string ReadDataFile
        {
            get => readDataFile;
            set
            {
                ThrowIfMonitoring(nameof(ReadDataFile));
                if (!string.IsNullOrEmpty(value))
                {
                    CheckFileNames(value, writeInputDataFile, writeOutputDataFile);
                    readDataFile = value;
                    SensorReadData = GenericSensorData(readDataFile);
                    DetachEventListeners();
                }
                else
                {
                    readDataFile = string.Empty;
                    SensorReadData = null;
                    AttachEventListeners();
                }
            }
        }

        public string WriteInputDataFile
        {
            get => writeInputDataFile;
            set
            {
                ThrowIfMonitoring(nameof(WriteInputDataFile));
                CheckFileNames(readDataFile, value, writeOutputDataFile);
                writeInputDataFile = value;
                SensorInputData = GenericSensorData(writeInputDataFile);

            }
        }

        public string WriteOutputDataFile
        {
            get => writeOutputDataFile;
            set
            {
                ThrowIfMonitoring(nameof(WriteOutputDataFile));
                CheckFileNames(readDataFile, writeInputDataFile, value);
                writeOutputDataFile = value;
                SensorOutputData = GenericSensorData(writeOutputDataFile);

            }
        }

        public virtual SensorData<T>? SensorReadData { get; private set; }
        public virtual SensorData<T>? SensorInputData { get; private set; }
        public virtual SensorData<T>? SensorOutputData { get; private set; }

        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <exception cref="ArgumentException"></exception>
        public ReadWriteSensor(SensorType type): base(type)
        {
            AttachEventListeners();
        }

        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="TestFilename">input filename</param>
        /// <exception cref="ArgumentException"></exception>
        public ReadWriteSensor(SensorType type, string TestFilename): base(type)
        {
            this.ReadDataFile = TestFilename;
        }

        /// <summary>
        /// Begins reading data values from the test sequence, notifying listeners of each value as it is read.
        /// The process is contrlled by the value of IsMonitored.
        /// </summary>
        /// <remarks>This method processes all values in the test sequence, notifying listeners in real
        /// time according to the timing specified by each value. Reading will only occur if monitoring is enabled. The
        /// method blocks until all values have been read and notified.</remarks>
        public async Task StartReadingData()
        {
            if (this.IsReadingFromFile && !IsMonitoring)
            {
                try
                {
                    isMonitoring = true;
                    DetachEventListeners();
                    var disp = DispatcherProvider.Current.GetForCurrentThread();
                    if (disp == null)
                    {
                        throw new InvalidOperationException("No Dispatcher available for this UI thread.");
                    }

                    var readValues = SensorReadData?.TestValues;
                    if (readValues != null && readValues.Count > 0)
                    {
                        await Task.Run(async () =>
                        {
                            long startTics = SensorFeeds.FeedStart.Ticks;
                            int posn = 0;
                            while (readValues!.Count > posn)
                            {
                                if (!IsMonitoring)
                                {
                                    break;
                                }
                                long ticks = readValues!.Keys[posn];
                                T value = readValues![posn];
                                if (value != null)
                                {
                                    // A tick is 100 nanoseconds or 1/10000 microsecond
                                    var elapsedTicks = (DateTime.UtcNow.Ticks - startTics);
                                    if (elapsedTicks < ticks)
                                        Thread.Sleep((int)((ticks - elapsedTicks) * 10000));
                                    if (isMonitoring)
                                    {
                                        disp?.Dispatch(() =>
                                        {
                                            ReadingChangeEvent(value);
                                            posn++;
                                        });
                                    }
                                }
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    isMonitoring = false;
                }
            }
        }

        public virtual void Dispose()
        {
            SensorOutputData?.Flush();
            SensorInputData?.Flush();
        }

        #endregion

        #region protected methods

        /// <summary>
        /// ReadingChangeEvent must handle input events as from a native sensor.
        /// </summary>
        /// <param name="value"></param>
        protected virtual void ReadingChangeEvent(T value)
        {
            // Override to process messages from native sensor. Call base method.
            LastInputReadingTime = DateTime.UtcNow;
            LogInputValue(value);
            var result = CustomHandler(value);
            if (result != null && ValueOf(result) != 0)
            {
                CurrentValue = result!;
                LogOutputValue(result);
                LastOutputReadingTime = DateTime.UtcNow;
                NotifyReadingChange(result);
            }
        }

        protected virtual void DetachEventListeners()
        {
        }

        protected virtual void AttachEventListeners()
        {
        }

        /// <summary>
        /// Override for buffering and filtering of raw sensor values.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Null if app should ignore this value</returns>
        protected virtual T? CustomHandler(T value)
        {
            return value;
        }

        /// <summary>
        /// Call the log method when a reading inputs a new value.
        /// </summary>
        /// <param name="value"></param>
        protected void LogInputValue(T value)
        {
            if (IsWritingInputToFile)
            {
                SensorInputData?.AddValue(value);
            }
        }

        /// <summary>
        /// Call the log method when a reading outputs a new value.
        /// </summary>
        /// <param name="value"></param>
        protected void LogOutputValue(T value)
        {
            if (IsWritingOutputToFile)
            {
                SensorOutputData?.AddValue(value);
            }
        }

        /// <summary>
        /// Override this method to send a new read value to any targets or listening apps via your event.
        /// </summary>
        /// <param name="value"></param>
        protected abstract void NotifyReadingChange(T value);

        /// <summary>
        /// Overrides should return the correct SensorData type.
        /// Readers of test data should set loadFromFile to true.
        /// </summary>
        /// <returns></returns>
        public abstract SensorData<T> GenericSensorData(string dataFilename);

        #endregion

        private void CheckFileNames(string readFile, string writeInputFile, string writeOutputFile)
        {
            string errMsg = string.Empty;
            if (!string.IsNullOrEmpty(readFile) && 
                (string.Equals(readFile, writeInputFile, StringComparison.InvariantCultureIgnoreCase) ||
                 string.Equals(readFile, writeOutputFile, StringComparison.InvariantCultureIgnoreCase)))
            {
                errMsg = $"Sensor {this.GetType().Name} cannot both read and write to the same file {readFile}";
            }
            if (!string.IsNullOrEmpty(writeInputFile) && 
                string.Equals(writeInputFile, writeOutputFile, StringComparison.InvariantCultureIgnoreCase))
            {
                errMsg = $"Sensor {this.GetType().Name} cannot write both input and output to the same file {writeInputFile}";
            }
            if (errMsg.Length > 0)
            {
                throw new ArgumentException(errMsg);
            }
        }

    }
}
