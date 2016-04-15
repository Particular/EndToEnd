namespace Tests.Permutations
{
    using System;
    using System.Linq;
    using Variables;

    public static class PermutationParser
    {
        static readonly string Seperator = ";";

        public static string ToArgs(Permutation instance)
        {
            return "--tests:" + string.Join(Seperator, instance.Tests) + 
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
            return instance;
        }

        static string[] SplitValues(this string value)
        {
            return value.Split(new[] { Seperator }, StringSplitOptions.None);
        }
        static string GetVar(string[] args, string name)
        {
            return args.Single(a => a.StartsWith("--" + name)).Substring(name.Length + 3); // -- :
        }

        public static Permutation Parse(string data)
        {
            var values = data.SplitValues();

            return new Permutation
            {
                AuditMode = values.Parse<Audit>(0),
                DTCMode = values.Parse<DTC>(1),
                IOPS = values.Parse<IOPS>(2),
                MessageSize = values.Parse<MessageSize>(3),
                Version = values.Parse<NServiceBusVersion>(4),
                OutboxMode = values.Parse<Outbox>(5),
                Persister = values.Parse<Persistence>(6),
                Platform = values.Parse<Platform>(7),
                Serializer = values.Parse<Serialization>(8),
                Transport = values.Parse<Transport>(9),
                GarbageCollector = values.Parse<GarbageCollector>(10),
                TransactionMode = values.Parse<TransactionMode>(11),
                ConcurrencyLevel = values.Parse<ConcurrencyLevel>(12),
                ScaleOut = values.Parse<ScaleOut>(13)
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
            return $"{p.AuditMode};{p.DTCMode};{p.IOPS};{p.MessageSize};{p.Version};{p.OutboxMode};{p.Persister};{p.Platform};{p.Serializer};{p.Transport};{p.GarbageCollector};{p.TransactionMode};{p.ConcurrencyLevel};{p.ScaleOut}";
        }

        public static string ToFriendlyString(Permutation p)
        {
            return $"{p.AuditMode.GetEnumDescription()};{p.DTCMode.GetEnumDescription()};{p.IOPS.GetEnumDescription()};{p.MessageSize.GetEnumDescription()};{p.Version.GetEnumDescription()};{p.OutboxMode.GetEnumDescription()};{p.Persister.GetEnumDescription()};{p.Platform.GetEnumDescription()};{p.Serializer.GetEnumDescription()};{p.Transport.GetEnumDescription()};{p.GarbageCollector.GetEnumDescription()};{p.TransactionMode.GetEnumDescription()};{p.ConcurrencyLevel.GetEnumDescription()};{p.ScaleOut.GetEnumDescription()}";
        }
    }
}
