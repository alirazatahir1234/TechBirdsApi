using System.Collections.Generic;
using System.Threading.Tasks;
using TechBirdsApi.Models;

namespace TechBirdsApi.Interfaces
{
    public interface IUserRepository
    {
    IQueryable<ApplicationUser> GetAll();
        Task<ApplicationUser?> GetByIdAsync(string id);
        Task AddAsync(ApplicationUser user);
        Task UpdateAsync(ApplicationUser user);
        Task DeleteAsync(string id);
    }
}
