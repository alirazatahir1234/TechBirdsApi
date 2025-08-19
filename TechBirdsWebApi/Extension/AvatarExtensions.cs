namespace TechBirdsWebAPI.Extension
{
    public static class AvatarExtensions
    {
        // Converts a base64 string to byte[] safely
        public static byte[]? ToAvatarBytes(this string? base64Avatar)
        {
            if (string.IsNullOrEmpty(base64Avatar)) return null;
            try
            {
                return Convert.FromBase64String(base64Avatar);
            }
            catch
            {
                return null;
            }
        }

        // Converts a byte[] avatar to base64 string safely
        public static string? ToBase64Avatar(this byte[]? avatarBytes)
        {
            if (avatarBytes == null || avatarBytes.Length == 0) return null;
            return Convert.ToBase64String(avatarBytes);
        }
    }
}
