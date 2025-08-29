using System.Collections.Generic;
using System.Threading.Tasks;
using TechBirdsApi.Interfaces;
using TechBirdsApi.Models;
using TechBirdsApi.Repositories;

namespace TechBirdsApi.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        public PostService(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        public async Task<IEnumerable<Post>> GetAllAsync()
        {
            return await _postRepository.GetAllAsync();
        }

        public async Task<Post?> GetByIdAsync(Guid id)
        {
            return await _postRepository.GetByIdAsync(id);
        }

        public async Task AddAsync(Post post)
        {
            await _postRepository.AddAsync(post);
        }

        public async Task UpdateAsync(Post post)
        {
            await _postRepository.UpdateAsync(post);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _postRepository.DeleteAsync(id);
        }
    }
}
