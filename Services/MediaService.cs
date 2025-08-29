using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechBirdsApi.Interfaces;
using TechBirdsApi.Models;
using TechBirdsApi.Repositories;

namespace TechBirdsApi.Services
{
    public class MediaService : IMediaService
    {
        private readonly IMediaRepository _mediaRepository;
        public MediaService(IMediaRepository mediaRepository)
        {
            _mediaRepository = mediaRepository;
        }

        public async Task<IEnumerable<MediaItem>> GetAllAsync()
        {
            return await _mediaRepository.GetAllAsync();
        }

        public async Task<MediaItem?> GetByIdAsync(Guid id)
        {
            return await _mediaRepository.GetByIdAsync(id);
        }

        public async Task AddAsync(MediaItem media)
        {
            await _mediaRepository.AddAsync(media);
        }

        public async Task UpdateAsync(MediaItem media)
        {
            await _mediaRepository.UpdateAsync(media);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _mediaRepository.DeleteAsync(id);
        }
    }
}
