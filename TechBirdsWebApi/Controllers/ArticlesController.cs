using Microsoft.AspNetCore.Mvc;
using TechBirdsWebAPI.Models;
using TechBirdsWebAPI.Repositories;

namespace TechBirdsWebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleRepository _repository;

        public ArticlesController(IArticleRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult Get() => Ok(_repository.GetAll());

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var article = _repository.GetById(id);
            if (article == null) return NotFound();
            return Ok(article);
        }

        [HttpPost]
        public IActionResult Post(Article article)
        {
            _repository.Add(article);
            return CreatedAtAction(nameof(Get), new { id = article.Id }, article);
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, Article article)
        {
            if (id != article.Id) return BadRequest();
            _repository.Update(article);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _repository.Delete(id);
            return NoContent();
        }
    }
}