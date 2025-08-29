using System.Collections.Generic;
using System.Threading.Tasks;
using TechBirdsApi.Models;

namespace TechBirdsApi.Services
{
    public interface IPageService
    {
        Task<IEnumerable<Page>> GetAllAsync();
        Task<Page?> GetByIdAsync(Guid id);
        Task AddAsync(Page page);
        Task UpdateAsync(Page page);
        Task DeleteAsync(Guid id);

        Task<Page?> GetBySlugAsync(string slug, bool onlyPublished = true);

        Task<PageRevision?> GetRevisionAsync(Guid revisionId);

        Task<int> GetNextRevisionVersionAsync(Guid pageId);

        Task AddRevisionAsync(PageRevision revision);

        Task<IEnumerable<Page>> FilterAsync(TechBirdsApi.Dto.PageQuery query);

        Task<int> CountAsync(string? search, string? status, Guid? parentId);

        Task<IEnumerable<PageRevision>> GetRevisionsAsync(Guid pageId);
    }
}
