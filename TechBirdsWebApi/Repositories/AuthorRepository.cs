using Dapper;
using System.Collections.Generic;
using System.Linq;
using TechBirdsWebAPI.Data;
using TechBirdsWebAPI.Models;

namespace TechBirdsWebAPI.Repositories
{
    public class AuthorRepository : IAuthorRepository
    {
        private readonly DapperContext _context;

        public AuthorRepository(DapperContext context)
        {
            _context = context;
        }

        public IEnumerable<Author> GetAll()
        {
            var sql = "SELECT * FROM authors";
            using var connection = _context.CreateConnection();
            return connection.Query<Author>(sql).ToList();
        }

        public Author GetById(int id)
        {
            var sql = "SELECT * FROM authors WHERE id = @Id";
            using var connection = _context.CreateConnection();
            var author = connection.QuerySingleOrDefault<Author>(sql, new { Id = id });
            return author ?? new Author();
        }

        public void Add(Author author)
        {
            var sql = "INSERT INTO authors (name, bio, email, passwordhash) VALUES (@Name, @Bio, @Email, @PasswordHash)";
            using var connection = _context.CreateConnection();
            connection.Execute(sql, author);
        }

        public void Update(Author author)
        {
            var sql = "UPDATE authors SET name = @Name, bio = @Bio, email = @Email, passwordhash = @PasswordHash WHERE id = @Id";
            using var connection = _context.CreateConnection();
            connection.Execute(sql, author);
        }

        public void Delete(int id)
        {
            var sql = "DELETE FROM authors WHERE id = @Id";
            using var connection = _context.CreateConnection();
            connection.Execute(sql, new { Id = id });
        }
    }
}