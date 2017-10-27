using System;
using System.Collections.Generic;

public interface IConfigureUnicastBus
{
    IEnumerable<Mapping> GenerateMappings();
}

public struct Mapping
{
    public Type MessageType;
    public string Endpoint;
}

