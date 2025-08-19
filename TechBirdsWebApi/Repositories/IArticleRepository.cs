using TechBirdsWebAPI.Models;
using System.Collections.Generic;

namespace TechBirdsWebAPI.Repositories
{
    public interface IArticleRepository
    {
        IEnumerable<Article> GetAll();
        Article GetById(int id);
        void Add(Article article);
        void Update(Article article);
        void Delete(int id);
    }
}