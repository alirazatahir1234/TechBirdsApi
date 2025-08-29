using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechBirdsApi.Models;

namespace TechBirdsApi.Interfaces
{
    public interface IMediaRepository
    {
        Task<IEnumerable<MediaItem>> GetAllAsync();
        Task<MediaItem?> GetByIdAsync(Guid id);
        Task AddAsync(MediaItem media);
        Task UpdateAsync(MediaItem media);
        Task DeleteAsync(Guid id);
    }
}
