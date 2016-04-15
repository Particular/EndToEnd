#if Version5
using System;
using NServiceBus.Pipeline;
using NServiceBus.Pipeline.Contexts;

namespace NServiceBus.Performance
{
    public class StatisticsBehavior : IBehavior<IncomingContext>
    {
        readonly Implementation provider;
        public StatisticsBehavior(Implementation provider)
        {
            this.provider = provider;
        }

        public void Invoke(IncomingContext context, Action next)
        {
            var start = provider.Timestamp();
            try
            {
                provider.ConcurrencyInc();
                next();
                provider.SuccessInc();
            }
            catch
            {
                provider.ErrorInc();
            }
            finally
            {
                provider.DurationInc(start, provider.Timestamp());
                provider.ConcurrencyDec();
                provider.Inc();
            }
        }

        public static readonly string Name = "StatisticsStep";

        public class Step : RegisterStep
        {
            public Step() : base(Name, typeof(StatisticsBehavior), "Logs and displays statistics.")
            {
                InsertBefore(WellKnownStep.CreateChildContainer);
            }
        }

        public interface Implementation
        {
            void Inc();
            void ErrorInc();
            void SuccessInc();
            void DurationInc(long start, long end);
            void ConcurrencyInc();
            void ConcurrencyDec();
            long Timestamp();
        }
    }
}
#endif