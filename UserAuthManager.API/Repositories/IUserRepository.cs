using UserAuthManager.API.Models;

namespace UserAuthManager.API.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<ApplicationUser>> GetUsersAsync();
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        Task<ApplicationUser?> GetUserByIdAsync(string id);
        Task<ApplicationUser> AddUserAsync(ApplicationUser user);
        Task<ApplicationUser> CreateUserAsync(ApplicationUser user);
        Task<ApplicationUser?> UpdateUserAsync(ApplicationUser user);
        Task<ApplicationUser?> GetUserByEmailAsync(string email);
        Task<bool> DeleteUserAsync(string id);
    }
}
