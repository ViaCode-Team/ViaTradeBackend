using Application.Interfaces;
using Domain.Entities.DataBase;
using Infrastructure.Repositoryes.DataBase;

namespace Infrastructure.Services
{
    public class UserService(UserRepository userRepository) : IUserService
    {
        private readonly UserRepository _userRepository = userRepository;

        public async Task<User> EnsureUserAsync(int userId, CancellationToken cancellationToken)
        {
            return await _userRepository.GetByIdAsync(userId, cancellationToken)
                ?? throw new UnauthorizedAccessException();
        }

    }
}
