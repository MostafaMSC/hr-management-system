using HR.Application.Common.Interfaces;
using BCrypt.Net;

namespace HR.Infrastructure.Services;

public class BCryptPasswordHasherService : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword)) return false;
        
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        catch (SaltParseException)
        {
            // If the old password wasn't hashed with BCrypt, this catches the format exception
            return false;
        }
    }
}
