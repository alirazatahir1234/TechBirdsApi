using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechBirdsApi.Models;

namespace TechBirdsApi.Interfaces
{
    public interface IPageRepository
    {
        Task<IEnumerable<Page>> GetAllAsync();
        Task<Page?> GetByIdAsync(Guid id);
        Task<Page?> GetBySlugAsync(string slug, bool onlyPublished = false);
        Task<IEnumerable<Page>> FilterAsync(string? search, string? status, Guid? parentId, string? sortBy, string? sortOrder, int page, int limit);
        Task<int> CountAsync(string? search, string? status, Guid? parentId);
        Task AddAsync(Page page);
        Task UpdateAsync(Page page);
        Task DeleteAsync(Guid id);

        // Revision operations
        Task<IEnumerable<PageRevision>> GetRevisionsAsync(Guid pageId);
        Task<PageRevision?> GetRevisionAsync(Guid revisionId);
        Task AddRevisionAsync(PageRevision revision);
        Task<int> GetNextRevisionVersionAsync(Guid pageId);
    }
}
