using HR.Domain.Entities;

namespace HR.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(UserInfo user);
    RefreshToken GenerateRefreshToken(int userId);
    string GenerateRandomToken(int length = 32);
}
