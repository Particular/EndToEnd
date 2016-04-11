using System.IO;
using System.Runtime.InteropServices;

class SymbolicLink
{
    class Win32
    {
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, LinkType dwFlags);
    }

    enum LinkType
    {
        File = 0,
        Directory = 1
    }

    public static bool Create(string source, string destination)
    {
        if (!File.Exists(source)) throw new FileNotFoundException("Source file not found.", source);
        return Win32.CreateSymbolicLink(destination, source, LinkType.File);
    }

    public static bool IsSymbolic(string path)
    {
        var pathInfo = new FileInfo(path);
        return pathInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
    }
}
