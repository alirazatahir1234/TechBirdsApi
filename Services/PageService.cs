using System.Collections.Generic;
using System.Threading.Tasks;
using TechBirdsApi.Dto;
using TechBirdsApi.Interfaces;
using TechBirdsApi.Models;
using TechBirdsApi.Repositories;

namespace TechBirdsApi.Services
{
    public class PageService : IPageService
    {
        private readonly IPageRepository _pageRepository;
        public PageService(IPageRepository pageRepository)
        {
            _pageRepository = pageRepository;
        }

        public async Task<IEnumerable<Page>> GetAllAsync()
        {
            return await _pageRepository.GetAllAsync();
        }

        public async Task<Page?> GetByIdAsync(Guid id)
        {
            return await _pageRepository.GetByIdAsync(id);
        }

        public async Task AddAsync(Page page)
        {
            await _pageRepository.AddAsync(page);
        }

        public async Task UpdateAsync(Page page)
        {
            await _pageRepository.UpdateAsync(page);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _pageRepository.DeleteAsync(id);
        }

        public Task<Page?> GetBySlugAsync(string slug, bool onlyPublished = true)
        {
            return _pageRepository.GetBySlugAsync(slug, onlyPublished);
        }

        public Task<PageRevision?> GetRevisionAsync(Guid revisionId)
        {
            return _pageRepository.GetRevisionAsync(revisionId);
        }

        // public Task<int> GetCurrentRevisionVersionAsync(Guid pageId)
        // {
        //     return _pageRepository.GetCurrentRevisionVersionAsync(pageId);
        // }

        public Task<int> GetNextRevisionVersionAsync(Guid pageId)
        {
            return _pageRepository.GetNextRevisionVersionAsync(pageId);
        }

        public Task AddRevisionAsync(PageRevision revision)
        {
            return _pageRepository.AddRevisionAsync(revision);
        }

        public Task<IEnumerable<Page>> FilterAsync(PageQuery query)
        {
            return _pageRepository.FilterAsync(
                query.Search,
                query.Status,
                query.ParentId,
                query.SortBy,
                query.SortOrder,
                query.Page,
                query.Limit
            );
        }

        public Task<int> CountAsync(string? search, string? status, Guid? parentId)
        {
            return _pageRepository.CountAsync(search, status, parentId);
        }

        public Task<IEnumerable<PageRevision>> GetRevisionsAsync(Guid pageId)
        {
            return _pageRepository.GetRevisionsAsync(pageId);
        }
    }
}
