using System.Collections.Generic;
using System.Threading.Tasks;
using TechBirdsApi.Models;

namespace TechBirdsApi.Services
{
    public interface IUserService
    {
        Task<IEnumerable<ApplicationUser>> GetAllAsync();
        Task<ApplicationUser?> GetByIdAsync(string id);
        Task AddAsync(ApplicationUser user);
        Task UpdateAsync(ApplicationUser user);
        Task DeleteAsync(string id);
    }
}
