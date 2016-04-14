﻿namespace TransportCompatibilityTests.Common
{
    using System.Threading;

    public class SubscriptionStore
    {
        int numberOfSubscriptions;

        public int NumberOfSubscriptions => numberOfSubscriptions;

        public void Increment()
        {
            Interlocked.Increment(ref numberOfSubscriptions);
        }
    }
}