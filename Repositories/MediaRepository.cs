using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechBirdsApi.Data;
using TechBirdsApi.Interfaces;
using TechBirdsApi.Models;

namespace TechBirdsApi.Repositories
{
    public class MediaRepository : IMediaRepository
    {
        private readonly ApplicationDbContext _context;
        public MediaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MediaItem>> GetAllAsync()
        {
            return await _context.MediaItems.ToListAsync();
        }

        public async Task<MediaItem?> GetByIdAsync(Guid id)
        {
            return await _context.MediaItems.FindAsync(id);
        }

        public async Task AddAsync(MediaItem media)
        {
            await _context.MediaItems.AddAsync(media);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(MediaItem media)
        {
            _context.MediaItems.Update(media);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var media = await _context.MediaItems.FindAsync(id);
            if (media != null)
            {
                _context.MediaItems.Remove(media);
                await _context.SaveChangesAsync();
            }
        }
    }
}
