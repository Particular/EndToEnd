﻿namespace Common.Scenarios
{
    using NServiceBus;

    interface ICreateSeedData
    {
#if Version5
        /// <summary>
        /// Sends or publishes a single message
        /// </summary>
        void SendMessage(ISendOnlyBus sendOnlyBus, string endpointName);
#else
        /// <summary>
        /// Sends or publishes a single message
        /// </summary>
        void SendMessage(IEndpointInstance endpointInstance, string endpointName);
#endif

    }
}
