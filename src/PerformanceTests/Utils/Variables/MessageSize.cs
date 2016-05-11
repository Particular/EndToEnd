namespace Variables
{
    public enum MessageSize
    {
        Tiny = 0,
        Small = 1024,
        Medium = 10 * Small,
        Large = 100 * Small,
        L256 = 256 * Small,
        L512 = 512 * Small,
        L1MB = 1024 * Small,
        L4MB = 4 * L1MB,
        L16MB = 16 * L1MB,
        L64MB = 64 * L1MB,
         
    }
}