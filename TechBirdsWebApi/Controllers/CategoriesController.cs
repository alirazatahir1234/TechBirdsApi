using Microsoft.AspNetCore.Mvc;
using TechBirdsWebAPI.Models;
using TechBirdsWebAPI.Repositories;

namespace TechBirdsWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _repository;

        public CategoriesController(ICategoryRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult Get() => Ok(_repository.GetAll());

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var category = _repository.GetById(id);
            if (category == null) return NotFound();
            return Ok(category);
        }

        [HttpPost]
        public IActionResult Post(Category category)
        {
            _repository.Add(category);
            return CreatedAtAction(nameof(Get), new { id = category.Id }, category);
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, Category category)
        {
            if (id != category.Id) return BadRequest();
            _repository.Update(category);
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