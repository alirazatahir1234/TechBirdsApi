using System;

namespace TechBirdsWebAPI.Extension
{
    public static class Base64Extensions
    {
        public static byte[]? ToByteArray(this string? base64String)
        {
            if (string.IsNullOrWhiteSpace(base64String)) return null;
            try
            {
                return Convert.FromBase64String(base64String);
            }
            catch
            {
                return null;
            }
        }

        public static string? ToBase64String(this byte[]? bytes)
        {
            if (bytes == null || bytes.Length == 0) return null;
            return Convert.ToBase64String(bytes);
        }
    }
}
