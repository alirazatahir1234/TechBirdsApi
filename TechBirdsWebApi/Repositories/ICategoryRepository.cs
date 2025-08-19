using TechBirdsWebAPI.Models;
using System.Collections.Generic;

namespace TechBirdsWebAPI.Repositories
{
    public interface ICategoryRepository
    {
        IEnumerable<Category> GetAll();
        Category GetById(int id);
        void Add(Category category);
        void Update(Category category);
        void Delete(int id);
    }
}