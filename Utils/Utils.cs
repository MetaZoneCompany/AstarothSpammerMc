using AstarothSpammer.Properties;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using Leaf.xNet;

public partial class Utils
{
    public static Random rand = new Random();
    public static List<string> queue = new List<string>();
    public static string Chr = "abcedfghijklmnopqrstuvxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public static bool hideLiveLogs = false;

    public static HttpRequest CreateCleanRequest()
    {
        HttpRequest request = new HttpRequest();

        request.KeepTemporaryHeadersOnRedirect = false;
        request.EnableMiddleHeaders = false;
        request.ClearAllHeaders();
        request.AllowEmptyHeaderValues = false;
        request.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
        request.Proxy = null;
        request.Username = null;
        request.UserAgent = null;
        request.UseCookies = false;
        request.CookieSingleHeader = true;
        request.Authorization = null;
        request.BaseAddress = null;
        request.Referer = null;
        request.Reconnect = false;
        request.ReconnectDelay = 0;
        request.Password = null;
        request.KeepAlive = false;
        request.IgnoreInvalidCookie = true;
        request.IgnoreProtocolErrors = true;
        request.KeepTemporaryHeadersOnRedirect = false;
        request.MaximumKeepAliveRequests = 1;
        request.Cookies = null;
        request.CharacterSet = null;
        request.AcceptEncoding = null;
        request.Culture = null;
        request.AllowAutoRedirect = false;
        request.MaximumAutomaticRedirections = 1;

        return request;
    }

    public static long LongRandom(long min, long max, Random rand)
    {
        /*byte[] buf = new byte[8];
        rand.NextBytes(buf);
        return (Math.Abs(BitConverter.ToInt64(buf, 0) % (max - min)) + min);*/
        return KeyGenerator.GetUniqueLong(max.ToString().Length);
    }

    public static string GetRandomCookie(string lang)
    {
        return "__cfduid=" + RandomNormalString(1).ToLower() + GetRandomNumber(1, 9).ToString() + RandomNormalString(1).ToLower() + GetRandomNumber(100, 900).ToString() + RandomNormalString(5).ToLower() + GetRandomNumber(100, 900).ToString() + RandomNormalString(1).ToLower() + GetRandomNumber(1, 9).ToString() + RandomNormalString(1).ToLower() + GetRandomNumber(1000, 9000).ToString() + RandomNormalString(2).ToLower() + RandomNormalString(1).ToLower() + RandomNormalString(1).ToLower() + GetRandomNumber(10, 90).ToString() + RandomNormalString(2).ToLower() + LongRandom(10000000000000L, 90000000000000L, rand).ToString() + "; __dcfduid=" + GetRandomNumber(1, 9).ToString() + RandomNormalString(1).ToLower() + GetRandomNumber(1, 9).ToString() + RandomNormalString(1).ToLower() + Utils.GetRandomNumber(1000, 9000).ToString() + RandomNormalString(1).ToLower() + GetRandomNumber(1, 9).ToString() + RandomNormalString(3).ToString() + GetRandomNumber(10, 99).ToString() + RandomNormalString(1).ToLower() + Utils.GetRandomNumber(100, 900).ToString() + RandomNormalString(1).ToLower() + GetRandomNumber(1, 9).ToString() + RandomNormalString(1).ToLower() + GetRandomNumber(1, 9).ToString() + RandomNormalString(1).ToLower() + GetRandomNumber(100, 900).ToString() + RandomNormalString(1).ToLower() + GetRandomNumber(1000, 9000).ToString() + "; locale=" + lang;
    }

    public static string GetRandomCookie()
    {
        return GetRandomCookie("it");
    }

    public static string GetLagMessage()
    {
        return ":chains: :flag_ac: :chains: :flag_ac: :chains: :flag_ad: :laughing: :brain: :chains: :chains: :flag_ac: :chains: :flag_ac: :chains: :flag_ad: :laughing: :brain: :chains: :chains: :flag_ac: :chains: :flag_ac: :chains: :flag_ad: :laughing: :brain: :chains: :chains: :flag_ac: :chains: :flag_ac: :chains: :flag_ad: :laughing: :brain: :chains: :chains: :flag_ac: :chains: :flag_ac: :chains: :flag_ad: :laughing: :brain: :chains: :chains: :flag_ac: :chains: :flag_ac: :chains: :flag_ad: :laughing: :brain: :chains: :chains: :flag_ac: :chains: :flag_ac: :chains: :flag_ad: :laughing: :brain: :chains: :chains: :flag_ac: :chains: :flag_ac: :chains: :flag_ad: :laughing: :brain: :chains: :chains: :flag_ac: :chains: :flag_ac: :chains: :flag_ad: :laughing: :brain: :chains: :chains: :flag_ac: :chains: :flag_ac: :chains: :flag_ad: :laughing: :brain: :chains: :chains: :flag_ac: :chains: :flag_ac: :chains: :flag_ad: :laughing: :brain: :chains: :chains: :flag_ac: :chains: :flag_ac: :chains: :flag_ad: :laughing: :brain: :chains: :chains: :flag_ac: :chains: :flag_ac: :chains: :flag_ad: :laughing: :brain: :chains: :chains: :flag_ac: :chains: :flag_ac: :chains: :flag_ad: :laughing: :brain: :chains: :chains: :flag_ac: :chains: :flag_ac: :chains: :flag_ad: :laughing: :brain: :chains: :chains: :flag_ac: :chains: :flag_ac: :chains: :flag_ad: :laughing: :brain: :chains: :chains: :flag_ac: :chains: :flag_ac: :chains: :flag_ad: :laughing: :brain: :chains: :chains: :flag_ac: :chains: :flag_ac: :chains: :flag_ad: :laughing: :brain: :chains: :chains: :flag_ac: :chains: :flag_ac: :chains: :flag_ad: :laughing: :brain: :chains: :chains: :flag_ac: :chains: :flag_ac: :chains: :flag_ad: :laughing: :brain: :chains: :chains: :flag_ac: :chains: :flag_ac: :chains: :flag_ad: :laughing: :brain: :chains: :chains: :flag_ac: :chains: :flag_ac: :chains: :flag_ad: :laughing: :brain: :chains: :chains:";
    }

    public static long RandomNonce()
    {
        return LongRandom(800000000000000000L, 900000000000000000L, rand);
    }

    public static string Base64Encode(string plainText)
    {
        return System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(plainText));
    }

    public static string GetXCP(string guildId, string channelId, string channelType)
    {
        try
        {
            return Base64Encode("{\"location\":\"Join Guild\",\"location_guild_id\":\"" + guildId + "\",\"location_channel_id\":\"" + channelId + "\",\"location_channel_type\":" + channelType + "}");
        }
        catch
        {
            return "eyJsb2NhdGlvbiI6IkpvaW4gR3VpbGQiLCJsb2NhdGlvbl9ndWlsZF9pZCI6IjgyMjU4NDA5NTg5MTY1MjYyOSIsImxvY2F0aW9uX2NoYW5uZWxfaWQiOiI4MjI1ODQwOTYzNzA3MjA3NjgiLCJsb2NhdGlvbl9jaGFubmVsX3R5cGUiOjB9";
        }
    }

    public static string GetGroupXCP(string channelId, string channelType)
    {
        try
        {
            return Base64Encode("{\"location\":\"Invite Button Embed\",\"location_guild_id\":null,\"location_channel_id\":\"" + channelId + "\",\"location_channel_type\":" + channelType + ",\"location_message_id\":null}");
        }
        catch
        {
            return "eyJsb2NhdGlvbiI6Ikludml0ZSBCdXR0b24gRW1iZWQiLCJsb2NhdGlvbl9ndWlsZF9pZCI6bnVsbCwibG9jYXRpb25fY2hhbm5lbF9pZCI6IjgzNzM5NzUzMDAzODg5NDY0MiIsImxvY2F0aW9uX2NoYW5uZWxfdHlwZSI6MSwibG9jYXRpb25fbWVzc2FnZV9pZCI6IjgzNzU5MjQyMDAxNDA5NjM4NCJ9";
        }
    }

    public static string BypassAntiSpamFilter(string str)
    {
        string result = "";
        char[] characters = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        foreach (char c in str.ToCharArray())
        {
            bool found = false;

            foreach (char s in characters)
            {
                if (c == s)
                {
                    result += c + ".";
                    found = true;

                    break;
                }
            }

            if (!found)
            {
                result += c;
            }
        }

        return result;
    }

    public static string RandomNormalString(int length)
    {
        /*
        var sb = new StringBuilder();

        for (int i = 1, loopTo = length; i <= loopTo; i++)
        {
            int idx = rand.Next(0, Chr.Length);
            sb.Append(Chr.Substring(idx, 1));
        }

        return sb.ToString();*/
        return KeyGenerator.GetUniqueKey(length);
    }

    public static int GetRandomNumber(int cap)
    {
        //return rand.Next(0, cap);
        return KeyGenerator.GetUniqueInt(cap.ToString().Length);
    }

    public static int GetRandomNumber1(int cap)
    {
        return rand.Next(0, cap);
    }

    public static int GetRandomNumber(int min, int cap)
    {
        //return rand.Next(min, cap);
        return KeyGenerator.GetUniqueInt(cap.ToString().Length);
    }

    public static byte[] DecompressGZip(byte[] toDecompress)
    {
        MemoryStream stream = new MemoryStream(toDecompress);
        MemoryStream newStream = new MemoryStream();

        using (GZipStream decompressionStream = new GZipStream(stream, CompressionMode.Decompress))
        {
            decompressionStream.CopyTo(newStream);
        }

        return newStream.ToArray();
    }

    public static byte[] DecompressDeflate(byte[] toDecompress)
    {
        MemoryStream stream = new MemoryStream(toDecompress);
        MemoryStream newStream = new MemoryStream();

        using (DeflateStream decompressionStream = new DeflateStream(stream, CompressionMode.Decompress))
        {
            decompressionStream.CopyTo(newStream);
        }

        return newStream.ToArray();
    }

    public static byte[] DecompressBr(byte[] toDecompress)
    {
        return BrotliSharpLib.Brotli.DecompressBuffer(toDecompress, 0, toDecompress.Length);
    }

    public static string DecompressResponse(Leaf.xNet.HttpResponse response)
    {
        Dictionary<string, string>.Enumerator enumerator = response.EnumerateHeaders();
        Dictionary<string, string> theDictionary = new Dictionary<string, string>();

        string theKey = "a";

        while (theKey != null && theKey.Replace(" ", "").Replace('\t'.ToString(), "") != "")
        {
            try
            {
                enumerator.MoveNext();

                theDictionary.Add(enumerator.Current.Key.ToLower(), enumerator.Current.Value);

                theKey = enumerator.Current.Key;
            }
            catch
            {
                break;
            }
        }

        try
        {
            if (theDictionary["content-encoding"].Equals("br"))
            {
                return System.Text.Encoding.UTF8.GetString(DecompressBr(response.ToBytes()));
            }
            else if (theDictionary["content-encoding"].Equals("deflate"))
            {
                return System.Text.Encoding.UTF8.GetString(DecompressDeflate(response.ToBytes()));
            }
            else if (theDictionary["content-encoding"].Equals("gzip"))
            {
                return System.Text.Encoding.UTF8.GetString(DecompressGZip(response.ToBytes()));
            }
            else
            {
                return response.ToString();
            }
        }
        catch
        {
            return response.ToString();
        }
    }
}