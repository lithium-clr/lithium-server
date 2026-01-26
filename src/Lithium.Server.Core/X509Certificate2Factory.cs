using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Lithium.Server.Core;

public static class X509Certificate2Factory
{
    public static X509Certificate2 GetOrCreateSelfSignedCertificate(string certificateFileName,
        string? certificatePassword = null)
    {
        if (File.Exists(certificateFileName))
        {
            try
            {
                return X509CertificateLoader.LoadPkcs12FromFile(certificateFileName, certificatePassword,
                    X509KeyStorageFlags.Exportable);
            }
            catch
            {
                // Re-generate if failed to load
            }
        }

        using var rsa = RSA.Create(2048);

        var certRequest =
            new CertificateRequest("CN=localhost", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        var subjectAlternativeName = new SubjectAlternativeNameBuilder();
        subjectAlternativeName.AddDnsName("localhost");
        subjectAlternativeName.AddIpAddress(IPAddress.Loopback);
        subjectAlternativeName.AddIpAddress(IPAddress.IPv6Loopback);
        certRequest.CertificateExtensions.Add(subjectAlternativeName.Build());

        certRequest.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
                true));

        certRequest.CertificateExtensions.Add(
            new X509EnhancedKeyUsageExtension(
                new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") },
                false));

        var serialNumber = new byte[8];
        RandomNumberGenerator.Fill(serialNumber);

        var notBefore = DateTimeOffset.UtcNow.AddYears(-1);
        var notAfter = new DateTimeOffset(9999, 12, 31, 23, 59, 59, TimeSpan.Zero);

        var certificate = certRequest.CreateSelfSigned(notBefore, notAfter);

        var pfxBytes = certificate.Export(X509ContentType.Pfx, certificatePassword);
        File.WriteAllBytes(certificateFileName, pfxBytes);

        return X509CertificateLoader.LoadPkcs12FromFile(certificateFileName, certificatePassword,
            X509KeyStorageFlags.Exportable);
    }

    public static string? ComputeCertificateFingerprint(X509Certificate certificate)
    {
        try
        {
            var certBytes = certificate.GetRawCertData();
            var hash = SHA256.HashData(certBytes);

            return Base64UrlEncode(hash);
        }
        catch (CryptographicException)
        {
            Console.WriteLine("SHA-256 algorithm not available");
            return null;
        }
        catch (Exception)
        {
            Console.WriteLine("Failed to encode certificate");
            return null;
        }
    }

    private static string Base64UrlEncode(byte[] data) => Convert.ToBase64String(data)
        .Replace('+', '-')
        .Replace('/', '_')
        .Replace("=", "");
}