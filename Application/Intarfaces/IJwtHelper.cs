using Domain.Entities.DataBase;

namespace Application.Intarfaces
{
    public interface IJwtHelper
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
    }
}
