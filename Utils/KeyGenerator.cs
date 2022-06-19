using System;
using System.Security.Cryptography;
using System.Text;

public class KeyGenerator
{
    internal static readonly char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    internal static readonly char[] numbers = "123456789".ToCharArray();

    public static string GetUniqueKey(int size)
    {
        byte[] data = new byte[4 * size];

        using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
        {
            crypto.GetBytes(data);
        }

        StringBuilder result = new StringBuilder(size);

        for (int i = 0; i < size; i++)
        {
            var rnd = BitConverter.ToUInt32(data, i * 4);
            var idx = rnd % chars.Length;

            result.Append(chars[idx]);
        }

        return result.ToString();
    }

    public static int GetUniqueInt(int size)
    {
        byte[] data = new byte[4 * size];

        using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
        {
            crypto.GetBytes(data);
        }

        StringBuilder result = new StringBuilder(size);

        for (int i = 0; i < size; i++)
        {
            var rnd = BitConverter.ToUInt32(data, i * 4);
            var idx = rnd % numbers.Length;

            result.Append(numbers[idx]);
        }

        return int.Parse(result.ToString());
    }

    public static long GetUniqueLong(int size)
    {
        byte[] data = new byte[4 * size];

        using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
        {
            crypto.GetBytes(data);
        }

        StringBuilder result = new StringBuilder(size);

        for (int i = 0; i < size; i++)
        {
            var rnd = BitConverter.ToUInt32(data, i * 4);
            var idx = rnd % numbers.Length;

            result.Append(numbers[idx]);
        }

        return long.Parse(result.ToString());
    }
}