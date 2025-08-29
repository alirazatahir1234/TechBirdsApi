using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechBirdsApi.Data;
using TechBirdsApi.Interfaces;
using TechBirdsApi.Models;

namespace TechBirdsApi.Repositories
{
    public class PageRepository : IPageRepository
    {
        private readonly ApplicationDbContext _context;
        public PageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Page>> GetAllAsync()
        {
            return await _context.Pages.Where(p => !p.IsDeleted).ToListAsync();
        }

        public async Task<Page?> GetByIdAsync(Guid id)
        {
            return await _context.Pages.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<Page?> GetBySlugAsync(string slug, bool onlyPublished = false)
        {
            var query = _context.Pages.AsNoTracking().Where(p => p.Slug == slug && !p.IsDeleted);
            if (onlyPublished)
                query = query.Where(p => p.Status == "published");
            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Page>> FilterAsync(string? search, string? status, Guid? parentId, string? sortBy, string? sortOrder, int page, int limit)
        {
            var query = _context.Pages.AsNoTracking().Where(p => !p.IsDeleted);
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Title.Contains(search) || (p.Excerpt != null && p.Excerpt.Contains(search)) || p.Content.Contains(search));
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(p => p.Status == status);
            if (parentId.HasValue)
                query = query.Where(p => p.ParentId == parentId);
            query = sortBy?.ToLower() switch
            {
                "updated" => sortOrder == "asc" ? query.OrderBy(p => p.UpdatedAt) : query.OrderByDescending(p => p.UpdatedAt),
                "title" => sortOrder == "asc" ? query.OrderBy(p => p.Title) : query.OrderByDescending(p => p.Title),
                "menu" => sortOrder == "asc" ? query.OrderBy(p => p.MenuOrder) : query.OrderByDescending(p => p.MenuOrder),
                _ => sortOrder == "asc" ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt)
            };
            return await query.Skip((page - 1) * limit).Take(limit).ToListAsync();
        }

        public async Task<int> CountAsync(string? search, string? status, Guid? parentId)
        {
            var query = _context.Pages.AsNoTracking().Where(p => !p.IsDeleted);
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Title.Contains(search) || (p.Excerpt != null && p.Excerpt.Contains(search)) || p.Content.Contains(search));
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(p => p.Status == status);
            if (parentId.HasValue)
                query = query.Where(p => p.ParentId == parentId);
            return await query.CountAsync();
        }

        public async Task AddAsync(Page page)
        {
            await _context.Pages.AddAsync(page);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Page page)
        {
            _context.Pages.Update(page);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var page = await _context.Pages.FindAsync(id);
            if (page != null)
            {
                _context.Pages.Remove(page);
                await _context.SaveChangesAsync();
            }
        }

        // Revision operations
        public async Task<IEnumerable<PageRevision>> GetRevisionsAsync(Guid pageId)
        {
            return await _context.PageRevisions.AsNoTracking().Where(r => r.PageId == pageId).OrderByDescending(r => r.Version).ToListAsync();
        }

        public async Task<PageRevision?> GetRevisionAsync(Guid revisionId)
        {
            return await _context.PageRevisions.AsNoTracking().FirstOrDefaultAsync(r => r.Id == revisionId);
        }

        public async Task AddRevisionAsync(PageRevision revision)
        {
            await _context.PageRevisions.AddAsync(revision);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetNextRevisionVersionAsync(Guid pageId)
        {
            var maxVersion = await _context.PageRevisions.Where(r => r.PageId == pageId).MaxAsync(r => (int?)r.Version);
            return (maxVersion ?? 0) + 1;
        }
    }
}
