using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Lithium.Server.Core.Auth;

public static class CertificateUtil
{
    public static bool ValidateCertificateBinding(string? thumbprintClaim, X509Certificate2? clientCert)
    {
        // If no claim is present, there's nothing to validate.
        if (string.IsNullOrEmpty(thumbprintClaim))
            return true;

        // If a claim is present but no certificate is, validation fails.
        if (clientCert is null)
            return false;

        // Compute the SHA-256 thumbprint of the provided certificate.
        var thumbprintBytes = clientCert.GetCertHash(HashAlgorithmName.SHA256);
        
        // The GetCertHash method returns a hex string, but the 'x5t#S256' claim is Base64Url encoded.
        // So we must get the raw bytes and encode them ourselves.
        var certificateThumbprint = ToBase64Url(thumbprintBytes);

        return string.Equals(thumbprintClaim, certificateThumbprint, StringComparison.Ordinal);
    }

    private static string ToBase64Url(byte[] data)
    {
        return Convert.ToBase64String(data)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
