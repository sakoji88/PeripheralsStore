using System.Security.Cryptography;

namespace PeripheralsStore.Services;

public class PasswordHasherService
{
    public string HashPassword(string password)
    {
        return Convert.ToBase64String(Microsoft.AspNetCore.Cryptography.KeyDerivation.KeyDerivation.Pbkdf2(
            password: password,
            salt: RandomNumberGenerator.GetBytes(16),
            prf: Microsoft.AspNetCore.Cryptography.KeyDerivation.KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 32));
    }

    public bool VerifyPassword(string password, string hash)
    {
        var passwordHash = HashPasswordDeterministic(password);
        return passwordHash == hash;
    }

    // Упрощенный учебный подход для объяснения новичкам.
    public string HashPasswordDeterministic(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
