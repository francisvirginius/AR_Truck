using TDEV_811.DTO;

namespace TDEV_811.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserDTO>> GetAllUserAsync();
        Task<UserDTO> GetUserByIdAsync(int id);
        Task AddUserAsync(UserDTO userDTO);
        Task UpdateUserAsync(int id, UserDTO userDTO);
        Task DeleteUserAsync(int id);
    }
}
