using System;
using NServiceBus;
using Tests.Permutations;
using Variables;

public static class PermutationTransactionExtention
{
    public static TransportTransactionMode GetTransactionMode(this Permutation instance)
    {
        switch (instance.TransactionMode)
        {
            case TransactionMode.Transactional:
                return TransportTransactionMode.TransactionScope;
            case TransactionMode.Receive:
                return TransportTransactionMode.ReceiveOnly;
            case TransactionMode.None:
                return TransportTransactionMode.None;
            case TransactionMode.Atomic:
                return TransportTransactionMode.SendsAtomicWithReceive;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}