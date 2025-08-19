using Dapper;
using System.Collections.Generic;
using System.Linq;
using TechBirdsWebAPI.Data;
using TechBirdsWebAPI.Models;

namespace TechBirdsWebAPI.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly DapperContext _context;

        public CommentRepository(DapperContext context)
        {
            _context = context;
        }

        public IEnumerable<Comment> GetByArticleId(int articleId)
        {
            var sql = "SELECT * FROM comments WHERE articleid = @ArticleId";
            using var connection = _context.CreateConnection();
            return connection.Query<Comment>(sql, new { ArticleId = articleId }).ToList();
        }

        public Comment GetById(int id)
        {
            var sql = "SELECT * FROM comments WHERE id = @Id";
            using var connection = _context.CreateConnection();
            var comment = connection.QuerySingleOrDefault<Comment>(sql, new { Id = id });
            if (comment == null)
            {
                // Handle not found case, e.g., return a default Comment or throw an exception
                // return new Comment(); // Uncomment if you want to return a default object
                throw new KeyNotFoundException($"Comment with Id {id} not found.");
            }
            return comment;
        }

        public void Add(Comment comment)
        {
            var sql = "INSERT INTO comments (articleid, authorname, content, createdat) VALUES (@ArticleId, @AuthorName, @Content, @CreatedAt)";
            using var connection = _context.CreateConnection();
            connection.Execute(sql, comment);
        }

        public void Delete(int id)
        {
            var sql = "DELETE FROM comments WHERE id = @Id";
            using var connection = _context.CreateConnection();
            connection.Execute(sql, new { Id = id });
        }
    }
}