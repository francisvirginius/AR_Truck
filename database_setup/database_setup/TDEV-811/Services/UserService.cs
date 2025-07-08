using TDEV_811.DTO;
using TDEV_811.Entities.Repositories;
using TDEV_811.Models;

namespace TDEV_811.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UserDTO>> GetAllUserAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(x => new UserDTO
            {
                Id = x.Id,
                Username = x.Username
            });
        }

        public async Task<UserDTO> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.FindByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            return new UserDTO
            {
                Id = user.Id,
                Username = user.Username
            };
        }

        public async Task AddUserAsync(UserDTO userDTO)
        {
            var user = new User
            {
                Username = userDTO.Username
            };

            await _userRepository.AddAsync(user);
        }

        public async Task UpdateUserAsync(int id, UserDTO userDTO)
        {
            var user = await _userRepository.FindByIdAsync(id);

            if (user == null)
                throw new KeyNotFoundException("User not found");

            user.Username = userDTO.Username;

            await _userRepository.UpdateAsync(user);
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _userRepository.FindByIdAsync(id);

            if (user == null)
                throw new KeyNotFoundException("User not found");

            await _userRepository.DeleteAsync(id);
        }
    }
}
