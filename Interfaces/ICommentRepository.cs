using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechBirdsApi.Models;

namespace TechBirdsApi.Interfaces
{
    public interface ICommentRepository
    {
        Task<IEnumerable<Comment>> GetAllAsync();
        Task<Comment?> GetByIdAsync(Guid id);
        Task AddAsync(Comment comment);
        Task UpdateAsync(Comment comment);
        Task DeleteAsync(Guid id);
    }
}
