using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Security;

namespace Lithium.Server.Core.Protocol;

public static class PasswordChallengeUtility
{
    public static byte[] GenerateChallenge()
    {
        var challenge = new byte[32];
        new SecureRandom().NextBytes(challenge);
        
        return challenge;
    }

    public static byte[]? ComputePasswordHash(byte[] challenge, string password)
    {
        try
        {
            using var sha256 = SHA256.Create();
            sha256.TransformBlock(challenge, 0, challenge.Length, challenge, 0);
            
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            sha256.TransformFinalBlock(passwordBytes, 0, passwordBytes.Length);
            
            return sha256.Hash;
        }
        catch (CryptographicException)
        {
            return null;
        }
    }
}