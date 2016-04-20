﻿using System.Threading.Tasks;
using NServiceBus;

/// <summary>
/// Performs a continious test where a batch of messages is send via the bus without
/// a transaction and a handler processes these in parallel. Once all messages are
/// received it repeats this. Due to the fact that the sending is not transactional
/// the handler will already process messages while the batch is still being send.
/// </summary>
partial class GatedSendLocalRunner : LoopRunner
{
    protected override Task SendMessage()
    {
        return SendLocal(new Command
        {
            Data = Data
        });
    }
    
    public class Command : ICommand
    {
        public string OrderId { get; set; }
        public byte[] Data { get; set; }
    }
}
