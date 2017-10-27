#if Version6 || Version7
using System;
using NServiceBus.Pipeline;

namespace NServiceBus.Performance
{
    using System.Threading.Tasks;

    public class StatisticsBehavior : Behavior<ITransportReceiveContext>
    {
        private readonly Implementation provider;
        public StatisticsBehavior(Implementation provider)
        {
            this.provider = provider;
        }

        public override async Task Invoke(ITransportReceiveContext context, Func<Task> next)
        {
            var start = provider.Timestamp();
            try
            {
                provider.ConcurrencyInc();
                await next().ConfigureAwait(false);
                provider.SuccessInc();
            }
            catch
            {
                provider.ErrorInc();
                throw;
            }
            finally
            {
                provider.DurationInc(start, provider.Timestamp());
                provider.ConcurrencyDec();
                provider.Inc();
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