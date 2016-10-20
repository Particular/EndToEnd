#if Version6
using System;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus.Pipeline;

public class ShortcutBehavior : Behavior<ITransportReceiveContext>
{
    public static bool Shortcut;
    public static long Count;

    public override Task Invoke(ITransportReceiveContext context, Func<Task> next)
    {
        if (Shortcut)
        {
            Interlocked.Increment(ref Count);
            return Task.FromResult(0);
        }
        return next();
    }
}

#else

using System;
using System.Threading;
using NServiceBus.Pipeline;
using NServiceBus.Pipeline.Contexts;

public class ShortcutBehavior : IBehavior<IncomingContext>
{
    public static volatile bool Shortcut;
    public static long Count;

    public void Invoke(IncomingContext context, Action next)
    {
        if (Shortcut)
        {
            Interlocked.Increment(ref Count);
            return;
        }
        next();
    }

    public class Step : RegisterStep
    {
        public Step() : base(typeof(Step).FullName, typeof(ShortcutBehavior), "Shortcuts pipeline.")
        {
            InsertBefore(WellKnownStep.CreateChildContainer);
        }
    }
}

#endif
