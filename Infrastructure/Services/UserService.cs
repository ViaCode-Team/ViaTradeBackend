using Application.Intarfaces;
using Domain.Entities.DataBase;

namespace Infrastructure.Services
{
    public class UserService(IJwtHelper jwtHelper, IUserRepository userRepository) : IUserService
    {
        private readonly IJwtHelper jwtHelper = jwtHelper;
        private readonly IUserRepository _userRepository = userRepository;

        public async Task<User?> EnsureUser(int userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }

    }
}
