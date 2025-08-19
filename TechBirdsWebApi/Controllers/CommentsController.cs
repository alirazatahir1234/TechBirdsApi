using Microsoft.AspNetCore.Mvc;
using TechBirdsWebAPI.Models;
using TechBirdsWebAPI.Repositories;

namespace TechBirdsWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentRepository _repository;

        public CommentsController(ICommentRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("article/{articleId}")]
        public IActionResult GetByArticleId(int articleId) => Ok(_repository.GetByArticleId(articleId));

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var comment = _repository.GetById(id);
            if (comment == null) return NotFound();
            return Ok(comment);
        }

        [HttpPost]
        public IActionResult Post(Comment comment)
        {
            comment.CreatedAt = DateTime.UtcNow;
            _repository.Add(comment);
            return CreatedAtAction(nameof(Get), new { id = comment.Id }, comment);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _repository.Delete(id);
            return NoContent();
        }
    }
}