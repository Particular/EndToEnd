using System;
using System.Security.Cryptography;
using System.Text;

public static class DeterministicUuid
{
    public static Guid Create(string value)
    {
        //use MD5 hash to get a 16-byte hash of the string: 
        using (var provider = new MD5CryptoServiceProvider())
        {
            var inputBytes = Encoding.UTF8.GetBytes(value);
            var hashBytes = provider.ComputeHash(inputBytes);
            return new Guid(hashBytes);
        }
    }

}
