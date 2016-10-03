#if Version6
using System;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus.Pipeline;

namespace NServiceBus.Performance
{
    public class StatisticsBehavior : Behavior<ITransportReceiveContext>
    {
        internal static bool Shortcut = false;
        internal static long ShortcutCount;
        private readonly Implementation provider;
        public StatisticsBehavior(Implementation provider)
        {
            this.provider = provider;
        }

        public override async Task Invoke(ITransportReceiveContext context, Func<Task> next)
        {
            if (Shortcut)
            {
                Interlocked.Increment(ref ShortcutCount);
                return;
            }
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