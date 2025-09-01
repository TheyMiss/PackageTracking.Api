using System.Security.Cryptography;

namespace PackageTracking.Api.Infrastructure;

public interface ITrackingNumberGenerator
{
    string Generate();
}

public class TrackingNumberGenerator: ITrackingNumberGenerator
{
    public string Generate()
    {
        var prefix = DateTime.UtcNow.ToString("yyDDD");
        Span<byte> bytes = stackalloc byte[8];
        RandomNumberGenerator.Fill(bytes);
        var safe = Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Replace("=", "");
        return $"{prefix}-{safe[..6].ToUpper()}";
    }
}


