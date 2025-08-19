using TechBirdsWebAPI.Models;
using System.Collections.Generic;

namespace TechBirdsWebAPI.Repositories
{
    public interface ICommentRepository
    {
        IEnumerable<Comment> GetByArticleId(int articleId);
        Comment GetById(int id);
        void Add(Comment comment);
        void Delete(int id);
    }
}