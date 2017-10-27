using Tests.Permutations;

public interface IContext
{
    Permutation Permutation { get; }
    string EndpointName { get; }
    bool IsSendOnly { get; }
}