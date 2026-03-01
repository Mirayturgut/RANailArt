using System.Security.Cryptography;

public static class TokenGen
{
    public static string CreateUrlSafeToken(int bytes = 32)
    {
        var data = RandomNumberGenerator.GetBytes(bytes);
        return Convert.ToBase64String(data)
            .Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
}