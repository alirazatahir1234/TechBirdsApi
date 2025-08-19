using Dapper;
using System.Collections.Generic;
using System.Linq;
using TechBirdsWebAPI.Data;
using TechBirdsWebAPI.Models;

namespace TechBirdsWebAPI.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly DapperContext _context;

        public CategoryRepository(DapperContext context)
        {
            _context = context;
        }

        public IEnumerable<Category> GetAll()
        {
            var sql = "SELECT * FROM categories";
            using var connection = _context.CreateConnection();
            return connection.Query<Category>(sql).ToList();
        }

        public Category GetById(int id)
        {
            var sql = "SELECT * FROM categories WHERE id = @Id";
            using var connection = _context.CreateConnection();
            var category = connection.QuerySingleOrDefault<Category>(sql, new { Id = id });
            return category ?? new Category();
        }

        public void Add(Category category)
        {
            var sql = "INSERT INTO categories (name) VALUES (@Name)";
            using var connection = _context.CreateConnection();
            connection.Execute(sql, category);
        }

        public void Update(Category category)
        {
            var sql = "UPDATE categories SET name = @Name WHERE id = @Id";
            using var connection = _context.CreateConnection();
            connection.Execute(sql, category);
        }

        public void Delete(int id)
        {
            var sql = "DELETE FROM categories WHERE id = @Id";
            using var connection = _context.CreateConnection();
            connection.Execute(sql, new { Id = id });
        }
    }
}