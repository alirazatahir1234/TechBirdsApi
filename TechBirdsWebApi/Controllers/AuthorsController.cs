using Microsoft.AspNetCore.Mvc;
using TechBirdsWebAPI.Models;
using TechBirdsWebAPI.Repositories;

namespace TechBirdsWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _repository;

        public AuthorsController(IAuthorRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult Get() => Ok(_repository.GetAll());

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var author = _repository.GetById(id);
            if (author == null) return NotFound();
            return Ok(author);
        }

        [HttpPost]
        public IActionResult Post(Author author)
        {
            _repository.Add(author);
            return CreatedAtAction(nameof(Get), new { id = author.Id }, author);
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, Author author)
        {
            if (id != author.Id) return BadRequest();
            _repository.Update(author);
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