using System.Collections.Generic;
using System.Threading.Tasks;
using TechBirdsApi.Models;

namespace TechBirdsApi.Services
{
    public interface IPostService
    {
    Task<IEnumerable<Post>> GetAllAsync();
    Task<Post?> GetByIdAsync(Guid id);
    Task AddAsync(Post post);
    Task UpdateAsync(Post post);
    Task DeleteAsync(Guid id);
    }
}
