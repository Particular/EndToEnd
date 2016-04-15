using System;
using Variables;

public static class ConcurrencyLevelConverter
{
    public static int Convert(ConcurrencyLevel value)
    {
        switch (value)
        {
            case ConcurrencyLevel.Sequential:
                return 1;
            default:
                return Environment.ProcessorCount * (int)value;
        }
    }
}
