#if Version5
using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace NServiceBus.Performance
{
    using Features;
    using Logging;

    public class SimpleStatisticsFeature : Feature
    {
        static readonly string SettingKey = "NServiceBus/SimpleStatistics";

        public SimpleStatisticsFeature()
        {
            EnableByDefault();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Container.ConfigureComponent<Collector>(DependencyLifecycle.SingleInstance);
            RegisterStartupTask<StartupTask>();

            context.Container.ConfigureComponent<StatisticsBehavior>(DependencyLifecycle.SingleInstance);
            // Register the new step in the pipeline
            context.Pipeline.Register<StatisticsBehavior.Step>();
        }

        internal class StartupTask : FeatureStartupTask
        {
            public Collector Collector { get; set; }

            protected override void OnStart()
            {
                Collector.Start();
            }

            protected override void OnStop()
            {
                Collector.Stop();
            }
        }

        public class Collector : StatisticsBehavior.Implementation
        {
            static readonly ILog Log = LogManager.GetLogger("NServiceBus.SimpleStatistics");
            static readonly long TimeUnit = Stopwatch.Frequency;

            readonly string _titleFormat;
            readonly bool _outputTitle;
            readonly bool _outputLog;
            readonly int _updateInMilliSeconds = 2000;

            Timer _reportTimer;
            long _duration;
            long _failure;
            long _total;
            int _concurrency;

            Data _start;
            Data _last;
            Data _maxPerSecond;


            public Collector()
            {
                _outputTitle = true;//Environment.UserInteractive && string.Equals(ConfigurationManager.AppSettings[SettingKey + "/OutputConsoleTitle"], bool.TrueString, StringComparison.InvariantCultureIgnoreCase);
                _outputLog = string.Equals(ConfigurationManager.AppSettings[SettingKey + "/OutputLog"], bool.TrueString, StringComparison.InvariantCultureIgnoreCase);

                int interval;
                if (int.TryParse(ConfigurationManager.AppSettings[SettingKey + "/IntervalMilliSeconds"], out interval)) _updateInMilliSeconds = interval;

                if (_outputTitle)
                {
                    _titleFormat = Console.Title + " | Avg:{0:N}/s Last:{1:N}/s, Max:{2:N}/s";
                }

                Log.InfoFormat("Report Interval: {0:N0}ms", _updateInMilliSeconds);
                Log.InfoFormat("Console title output: {0}", _outputTitle);
                Log.InfoFormat("Log output: {0}", _outputLog);
                Reset();
            }

            public void Start()
            {
                _reportTimer = new Timer(HandleReportTimer, null, _updateInMilliSeconds, _updateInMilliSeconds);
            }

            public void Stop()
            {
                _last = Read();

                var period = _last.Subtract(_start);
                var average = period.Relative(TimeUnit);

                ToLog("Totals since uptime", period);
                ToLog("Averages per second", average);

                _reportTimer?.Dispose();
            }

            public long Timestamp() => Stopwatch.GetTimestamp();

            public void Inc()
            {
                Interlocked.Increment(ref _total);
            }

            public void ErrorInc()
            {
                Interlocked.Increment(ref _failure);
            }

            public void SuccessInc()
            {
            }

            public void DurationInc(long start, long end)
            {
                var duration = end - start;
                Interlocked.Add(ref _duration, duration);
            }

            public void ConcurrencyInc()
            {
                Interlocked.Increment(ref _concurrency);
            }

            public void ConcurrencyDec()
            {
                Interlocked.Decrement(ref _concurrency);
            }

            public void Reset()
            {
                Log.InfoFormat("Resetting statistics");
                Interlocked.Exchange(ref _failure, 0);
                Interlocked.Exchange(ref _duration, 0);
                Interlocked.Exchange(ref _total, 0);
                Interlocked.Exchange(ref _failure, 0);

                _maxPerSecond = _last = _start = new Data { Ticks = Stopwatch.GetTimestamp() };
            }

            public struct Data
            {
                public long Failure;
                public long Total;
                public long Success;
                public long Duration;
                public long Ticks;

                public double FailurePercentage => Total != 0 ? Failure * 100D / Total : 0;
                public double SuccessPercentage => Total != 0 ? Success * 100D / Total : 0;
                public double TotalSeconds => (double)Ticks / Stopwatch.Frequency;
                public long AverageDurationTicks => Duration / Total;
                public double AverageDurationMicroSeconds => Duration / (double)Stopwatch.Frequency / Total * 1000 * 1000;

                public Data Subtract(Data instance)
                {
                    return new Data
                    {
                        Success = Success - instance.Success,
                        Total = Total - instance.Total,
                        Failure = Failure - instance.Failure,
                        Duration = Duration - instance.Duration,
                        Ticks = Ticks - instance.Ticks
                    };
                }
                public Data Add(Data instance)
                {
                    return new Data
                    {
                        Success = Success + instance.Success,
                        Total = Total + instance.Total,
                        Failure = Failure + instance.Failure,
                        Duration = Duration + instance.Duration,
                        Ticks = Ticks + instance.Ticks
                    };
                }

                public static Data Max(Data a, Data b)
                {
                    return new Data
                    {
                        Failure = Math.Max(a.Failure, b.Failure),
                        Total = Math.Max(a.Total, b.Total),
                        Success = Math.Max(a.Success, b.Success),
                        Duration = Math.Max(a.Duration, b.Duration),
                    };
                }

                public Data Relative(long timeUnit)
                {
                    if (Ticks == 0) return new Data();

                    return new Data
                    {
                        // !! CAUTION: Integer math!
                        Failure = Failure * timeUnit / Ticks,
                        Total = Total * timeUnit / Ticks,
                        Success = Success * timeUnit / Ticks,
                        Duration = Duration * timeUnit / Ticks,
                        Ticks = timeUnit
                    };
                }
            }

            Data Read()
            {
                return new Data
                {
                    Failure = _failure,
                    Success = _total - _failure,
                    Total = _total,
                    Duration = _duration,
                    Ticks = Stopwatch.GetTimestamp()
                };
            }

            public void ToLog(string description, Data data)
            {
                Log.InfoFormat("{0}> Success: {1,8:N0} ({2,6:N2}%), Failure: {3,8:N0} ({4,6:N2}%) Total: {5,8:N0} Period: {6:G} Duration: {7:N}µs",
                    description,
                    data.Success,
                    data.SuccessPercentage,
                    data.Failure,
                    data.FailurePercentage,
                    data.Total,
                    TimeSpan.FromSeconds(data.TotalSeconds),
                    data.AverageDurationMicroSeconds
                    );
            }

            public void HandleReportTimer(object o)
            {
                var current = Read();
                var delta = current.Subtract(_last);
                _last = current;

                var currentPerSecond = delta.Relative(TimeUnit);
                _maxPerSecond = Data.Max(_maxPerSecond, currentPerSecond);

                if (current.Total > 0)
                {
                    var period = _last.Subtract(_start);
                    var average = period.Relative(TimeUnit);

                    if (_outputLog)
                    {
                        ToLog("Avg", average);
                        ToLog("Tot", period);
                        ToLog("Cur", currentPerSecond);
                        ToLog("Max", _maxPerSecond);
                        Log.InfoFormat("Uptime: {0}", period.TotalSeconds);
                    }

                    if (_outputTitle)
                    {
                        Console.Title = string.Format(CultureInfo.InvariantCulture, _titleFormat, average.Total, currentPerSecond.Total, _maxPerSecond.Total);
                    }
                }
            }
        }
    }
}
#endif