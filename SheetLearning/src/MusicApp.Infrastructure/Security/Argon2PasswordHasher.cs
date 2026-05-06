using System.Security.Cryptography;
using System.Text;
using Isopoh.Cryptography.Argon2;
using MusicApp.Application.Interfaces;

namespace MusicApp.Infrastructure.Security;

public class Argon2PasswordHasher : IPasswordHasher
{
    public string Hash(string plaintext)
    {
        var config = new Argon2Config
        {
            Type = Argon2Type.DataIndependentAddressing,
            Version = Argon2Version.Nineteen,
            TimeCost = 3,
            MemoryCost = 65536,
            Lanes = 4,
            Threads = 4,
            Password = Encoding.UTF8.GetBytes(plaintext),
            Salt = RandomNumberGenerator.GetBytes(16),
            HashLength = 32
        };

        return Argon2.Hash(config);
    }

    public bool Verify(string plaintext, string hash)
    {
        try
        {
            return Argon2.Verify(hash, plaintext);
        }
        catch
        {
            return false;
        }
    }
}
