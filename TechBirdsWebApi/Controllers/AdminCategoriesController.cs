using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechBirdsWebAPI.Data;
using TechBirdsWebAPI.Models;

namespace TechBirdsWebAPI.Controllers
{
    [Route("api/admin/categories")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminCategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminCategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _context.Categories
                    .Select(c => new CategoryResponse
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description ?? string.Empty,
                        PostCount = 0, // You can implement this count later
                        CreatedAt = DateTime.UtcNow // Add this to your model if needed
                    })
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving categories", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            try
            {
                var category = new Category
                {
                    Name = request.Name,
                    Description = request.Description ?? string.Empty
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                var response = new CategoryResponse
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description ?? string.Empty,
                    PostCount = 0,
                    CreatedAt = DateTime.UtcNow
                };

                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating category", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return NotFound(new { message = "Category not found" });
                }

                var response = new CategoryResponse
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description ?? string.Empty,
                    PostCount = 0,
                    CreatedAt = DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving category", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return NotFound(new { message = "Category not found" });
                }

                category.Name = request.Name;
                category.Description = request.Description ?? string.Empty;

                _context.Categories.Update(category);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Category updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating category", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return NotFound(new { message = "Category not found" });
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Category deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting category", error = ex.Message });
            }
        }
    }

    // DTOs
    public class CreateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class UpdateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class CategoryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PostCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
