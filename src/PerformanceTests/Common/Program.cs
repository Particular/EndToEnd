namespace Host
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Runtime;
    using System.Threading;
    using System.Threading.Tasks;
    using log4net.Config;
    using Microsoft.Win32;
#if Version5
    using NServiceBus.Log4Net;
#else
    using NServiceBus;
#endif

    using NServiceBus.Logging;
    using Tests.Permutations;

    class Program
    {
        static ILog Log;

        static string endpointName = "PerformanceTest";

        static int Main()
        {
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.DefaultConnectionLimit = Settings.DefaultConnectionLimit; // Querying of DefaultConnectionLimit on dotnet core does not return assigned value

            var entryAssembly = Assembly.GetEntryAssembly();
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            var logRepository = log4net.LogManager.GetRepository(entryAssembly);
            var configFileName = Path.GetFileName(new Uri(entryAssembly.CodeBase).LocalPath);
            XmlConfigurator.Configure(logRepository, new FileInfo($"{configFileName}.config"));
            
            return MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task<int> MainAsync()
        {
            GCSettings.LatencyMode = GCLatencyMode.Batch; // https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/latency
            LogManager.Use<Log4NetFactory>();

            Log = LogManager.GetLogger(typeof(Program));

#if NET452
            VisualStudioDebugHelper.DebugAttacher.AttachDebuggerToVisualStudioProcessFromCommandLineParameter();
#endif

            InitAppDomainEventLogging();
            Powerplan.CheckPowerPlan();
            CheckIfWindowsDefenderIsRunning();
            CheckProcessorScheduling();

            InitBatchHelper();

            try
            {
                var permutation = PermutationParser.FromCommandlineArgs();
                LogPermutation(permutation);

                InvokeSetupImplementations(permutation);

                using (Statistics.Initialize(permutation))
                {
                    EnvironmentStats.Write();

                    ValidateServicePointManager(permutation);

                    if (Environment.UserInteractive && Environment.OSVersion.Platform == PlatformID.Win32NT) 
                    {
#pragma warning disable PC001 // this code has a platform check
                        Console.Title = PermutationParser.ToFriendlyString(permutation);
#pragma warning restore PC001
                    }

                    var runnerTypes = AssemblyScanner.GetAllTypes<BaseRunner>().ToArray();

                    foreach (var t in runnerTypes) 
                    {
                        Log.DebugFormat("Found runner: {0}", t.FullName);
                    }

                    var runnerType = runnerTypes.First(x => x.Name.Contains(permutation.Tests[0]));

                    var runnableTest = (BaseRunner)Activator.CreateInstance(runnerType);

                    Log.InfoFormat("Executing scenario: {0}", runnableTest);
                    await runnableTest.Execute(permutation, endpointName).ConfigureAwait(false);

                    PostChecks();
                }
            }
            catch (NotSupportedException nsex)
            {
                Log.Warn("Not supported", nsex);
                return (int)ReturnCodes.NotSupported;
            }
            catch (Exception ex)
            {
                Log.Fatal("Main", ex);
                throw;
            }
            finally
            {
                Log.Info("Fin!");
            }
            return (int)ReturnCodes.OK;
        }

        static void InitBatchHelper()
        {
#if Version5
            BatchHelper.Instance = new BatchHelper.ParallelFor();
#endif

#if Version6 || Version7
            BatchHelper.Instance = new BatchHelper.TaskWhenAll();
#endif
        }

        static void CheckIfWindowsDefenderIsRunning()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
#pragma warning disable PC001 // API not supported on all platforms
                var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender\Real-Time Protection");

                if (key != null && 0 == (int)key.GetValue("DisableRealtimeMonitoring", 1))
                {
                    Log.Warn("Windows Defender is running, consider disabling real-time protection!");
                    Thread.Sleep(3000);
                }
#pragma warning restore PC001 // API not supported on all platforms
            }
        }

        static void InvokeSetupImplementations(Permutation permutation)
        {
            foreach (var instance in AssemblyScanner.GetAll<ISetup>())
            {
                var p = instance as INeedPermutation;
                if (p != null)
                {
                    p.Permutation = permutation;
                }
                Log.InfoFormat("Invoke setup: {0}", instance);
                instance.Setup();
            }
        }

        static void LogPermutation(Permutation permutation)
        {
            var log = LogManager.GetLogger("Permutation");
            log.InfoFormat("Category: {0}", permutation.Category);
            log.InfoFormat("Description: {0}", permutation.Description);
            log.InfoFormat("Fixture: {0}", permutation.Fixture);
            log.InfoFormat("Code: {0}", permutation.Code);
            log.InfoFormat("Id: {0}", permutation.Id);

            log.InfoFormat("AuditMode: {0}", permutation.AuditMode);
            log.InfoFormat("MessageSize: {0}", permutation.MessageSize);
            log.InfoFormat("Version: {0}", permutation.Version);
            log.InfoFormat("Outbox: {0}", permutation.OutboxMode);
            log.InfoFormat("Persistence: {0}", permutation.Persister);
            log.InfoFormat("Platform: {0}", permutation.Platform);
            log.InfoFormat("Serializer: {0}", permutation.Serializer);
            log.InfoFormat("Transport: {0}", permutation.Transport);
            log.InfoFormat("GC: {0}", permutation.GarbageCollector);
            log.InfoFormat("TransactionMode: {0}", permutation.TransactionMode);
            log.InfoFormat("ConcurrencyLevel: {0}", permutation.ConcurrencyLevel);
            log.InfoFormat("ScaleOut: {0}", permutation.ScaleOut);
        }

        static void ValidateServicePointManager(Permutation permutation)
        {
            var value = ConcurrencyLevelConverter.Convert(permutation.ConcurrencyLevel);
            if (ServicePointManager.DefaultConnectionLimit < value)
            {
                Log.WarnFormat("ServicePointManager.DefaultConnectionLimit value {0} is lower then maximum concurrency limit of {1} this can limit performance.",
                    ServicePointManager.DefaultConnectionLimit,
                    value
                    );
            }

            if (ServicePointManager.Expect100Continue)
            {
                Log.WarnFormat("ServicePointManager.Expect100Continue is set to True, consider setting this value to False to increase PUT operations.");
            }

            if (ServicePointManager.UseNagleAlgorithm)
            {
                Log.WarnFormat("ServicePointManager.UseNagleAlgorithm is set to True, consider setting this value to False to decrease Latency.");
            }
        }

        static void PostChecks()
        {
            if (Statistics.Instance.NumberOfMessages == 0)
            {
                Log.Error("NumberOfMessages equals 0, expected atleast one message to be processed.");
            }

            if (Statistics.Instance.NumberOfRetries > Statistics.Instance.NumberOfMessages)
            {
                Log.Error("NumberOfRetries is great than NumberOfMessages, too many errors occured during processing.");
            }
        }



        static void InitAppDomainEventLogging()
        {
            var firstChanceLog = LogManager.GetLogger("FirstChanceException");
            var unhandledLog = LogManager.GetLogger("UnhandledException");
            var domain = AppDomain.CurrentDomain;

            domain.FirstChanceException += (o, ea) => { firstChanceLog.Debug(ea.Exception.Message, ea.Exception); };
            domain.UnhandledException += (o, ea) =>
            {
                var exception = ea.ExceptionObject as Exception;
                if (exception != null)
                {
                    unhandledLog.Error(exception.Message, exception);
                }
            };
        }

        static void CheckProcessorScheduling()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
#pragma warning disable PC001 // API not supported on all platforms
                var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\PriorityControl");

                if (key == null || 24 != (int)key.GetValue("Win32PrioritySeparation", 2))
                {
                    Log.WarnFormat("Processor scheduling is set to 'Programs', consider setting this to 'Background services'!");
                    Thread.Sleep(3000);
                }
#pragma warning restore PC001 // API not supported on all platforms
            }
        }
    }
}
