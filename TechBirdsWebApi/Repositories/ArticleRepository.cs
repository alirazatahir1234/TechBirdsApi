using Dapper;
using System.Collections.Generic;
using System.Linq;
using TechBirdsWebAPI.Data;
using TechBirdsWebAPI.Models;

namespace TechBirdsWebAPI.Repositories
{
    public class ArticleRepository : IArticleRepository
    {
        private readonly DapperContext _context;

        public ArticleRepository(DapperContext context)
        {
            _context = context;
        }

        public IEnumerable<Article> GetAll()
        {
            var sql = "SELECT * FROM articles";
            using var connection = _context.CreateConnection();
            return connection.Query<Article>(sql).ToList();
        }

        public Article GetById(int id)
        {
            var sql = "SELECT * FROM articles WHERE id = @Id";
            using var connection = _context.CreateConnection();
            return connection.QuerySingleOrDefault<Article>(sql, new { Id = id });
        }

        public void Add(Article article)
        {
            var sql = "INSERT INTO articles (title, content, authorid, categoryid, publishedat) VALUES (@Title, @Content, @AuthorId, @CategoryId, @PublishedAt)";
            using var connection = _context.CreateConnection();
            connection.Execute(sql, article);
        }

        public void Update(Article article)
        {
            var sql = "UPDATE articles SET title = @Title, content = @Content, authorid = @AuthorId, categoryid = @CategoryId, publishedat = @PublishedAt WHERE id = @Id";
            using var connection = _context.CreateConnection();
            connection.Execute(sql, article);
        }

        public void Delete(int id)
        {
            var sql = "DELETE FROM articles WHERE id = @Id";
            using var connection = _context.CreateConnection();
            connection.Execute(sql, new { Id = id });
        }
    }
}