using System.Collections.Generic;
using System.Threading.Tasks;
using TechBirdsApi.Interfaces;
using TechBirdsApi.Models;
using TechBirdsApi.Repositories;

namespace TechBirdsApi.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        public CommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task<IEnumerable<Comment>> GetAllAsync()
        {
            return await _commentRepository.GetAllAsync();
        }

        public async Task<Comment?> GetByIdAsync(Guid id)
        {
            return await _commentRepository.GetByIdAsync(id);
        }

        public async Task AddAsync(Comment comment)
        {
            await _commentRepository.AddAsync(comment);
        }

        public async Task UpdateAsync(Comment comment)
        {
            await _commentRepository.UpdateAsync(comment);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _commentRepository.DeleteAsync(id);
        }
    }
}
