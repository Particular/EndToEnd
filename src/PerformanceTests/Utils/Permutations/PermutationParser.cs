namespace Tests.Permutations
{
    using System;
    using System.Linq;
    using Variables;

    public static class PermutationParser
    {
        static readonly string Separator = ";";

        public static string ToArgs(Permutation instance)
        {
            return "--tests:" + string.Join(Separator, instance.Tests) +
                   " --category:" + instance.Category +
                   " --description:\"" + instance.Description + "\"" +
                   " --variables:" + ToString(instance);
        }

        public static Permutation FromCommandlineArgs()
        {
            var args = Environment.GetCommandLineArgs();
            var variables = GetVar(args, "variables");
            var tests = GetVar(args, "tests").SplitValues();
            var category = GetVar(args, "category");
            var description = GetVar(args, "description");

            var instance = Parse(variables);
            instance.Tests = tests;
            instance.Category = category;
            instance.Description = description;

            if (args.Any(x => x.StartsWith("--run"))) Settings.RunDuration = TimeSpan.Parse(GetVar(args, "run"));
            if (args.Any(x => x.StartsWith("--warmup"))) Settings.WarmupDuration = TimeSpan.Parse(GetVar(args, "warmup"));

            return instance;
        }

        static string[] SplitValues(this string value)
        {
            return value.Split(new[] { Separator }, StringSplitOptions.None);
        }
        static string GetVar(string[] args, string name)
        {
            return args.Single(a => a.StartsWith("--" + name)).Substring(name.Length + 3); // -- :
        }

        public static Permutation Parse(string data)
        {
            var values = data.SplitValues();

            var position = 0;

            return new Permutation
            {
                AuditMode = values.Parse<Audit>(position++),
                MessageSize = values.Parse<MessageSize>(position++),
                Version = values.Parse<NServiceBusVersion>(position++),
                OutboxMode = values.Parse<Outbox>(position++),
                Persister = values.Parse<Persistence>(position++),
                Platform = values.Parse<Platform>(position++),
                Serializer = values.Parse<Serialization>(position++),
                Transport = values.Parse<Transport>(position++),
                GarbageCollector = values.Parse<GarbageCollector>(position++),
                TransactionMode = values.Parse<TransactionMode>(position++),
                ConcurrencyLevel = values.Parse<ConcurrencyLevel>(position++),
                ScaleOut = values.Parse<ScaleOut>(position),
            };
        }

        static T Parse<T>(this string[] values, int index)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), values[index]);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Value {values[index]} does not match any enum value: {string.Join(",", Enum.GetNames(typeof(T)))}", ex);
            }
        }

        public static string ToString(Permutation p)
        {
            return $"{p.AuditMode};{p.MessageSize};{p.Version};{p.OutboxMode};{p.Persister};{p.Platform};{p.Serializer};{p.Transport};{p.GarbageCollector};{p.TransactionMode};{p.ConcurrencyLevel};{p.ScaleOut}";
        }

        public static string ToFriendlyString(Permutation p)
        {
            return $"{p.AuditMode.GetEnumDescription()};{p.MessageSize.GetEnumDescription()};{p.Version.GetEnumDescription()};{p.OutboxMode.GetEnumDescription()};{p.Persister.GetEnumDescription()};{p.Platform.GetEnumDescription()};{p.Serializer.GetEnumDescription()};{p.Transport.GetEnumDescription()};{p.GarbageCollector.GetEnumDescription()};{p.TransactionMode.GetEnumDescription()};{p.ConcurrencyLevel.GetEnumDescription()};{p.ScaleOut.GetEnumDescription()}";
        }
    }
}
