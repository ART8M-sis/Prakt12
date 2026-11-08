using UserManager.DTO;
using UserManager.Models;

namespace UserManager.Services
{
    public interface IUserService
    {
        Task<List<UserDTO>> GetUsersAsync();
        Task<UserDTO?> GetUserByIdAsync(Guid id);
        Task<UserDTO> CreateUserAsync(CreateUserDTO user);
        Task<bool> DeleteUserAsync(Guid id);
    }
}