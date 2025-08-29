using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechBirdsApi.Data;
using TechBirdsApi.Models;
using TechBirdsApi.Repositories;
using TechBirdsApi.Services;
using TechBirdsApi.Dto;

namespace TechBirdsApi.Controllers
{
    [Route("api/admin/categories")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminCategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public AdminCategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllAsync();
                var response = categories.Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description ?? string.Empty,
                    PostCount = 0,
                    CreatedAt = DateTime.UtcNow
                });
                return Ok(response);
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
                // Check for duplicate category name (case-insensitive)
                var existing = (await _categoryService.GetAllAsync())
                    .FirstOrDefault(c => c.Name.Trim().ToLower() == request.Name.Trim().ToLower());
                if (existing != null)
                {
                    return Conflict(new { message = "Category with this name already exists." });
                }
                var category = new Category
                {
                    Name = request.Name,
                    Description = request.Description ?? string.Empty
                };
                await _categoryService.AddAsync(category);
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
                var category = await _categoryService.GetByIdAsync(id);
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
                var category = await _categoryService.GetByIdAsync(id);
                if (category == null)
                {
                    return NotFound(new { message = "Category not found" });
                }
                category.Name = request.Name;
                category.Description = request.Description ?? string.Empty;
                await _categoryService.UpdateAsync(category);
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
                var category = await _categoryService.GetByIdAsync(id);
                if (category == null)
                {
                    return NotFound(new { message = "Category not found" });
                }
                await _categoryService.DeleteAsync(id);
                return Ok(new { message = "Category deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting category", error = ex.Message });
            }
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreateCategories([FromBody] BulkCreateCategoryRequest request)
        {
            if (request?.Categories == null || !request.Categories.Any())
                return BadRequest(new { message = "No categories provided" });

            var allExisting = (await _categoryService.GetAllAsync())
                .Select(c => c.Name.Trim().ToLower())
                .ToHashSet();

            var created = new List<CategoryResponse>();
            var skipped = new List<string>();
            foreach (var cat in request.Categories)
            {
                var catName = cat.Name.Trim().ToLower();
                if (allExisting.Contains(catName))
                {
                    skipped.Add(cat.Name);
                    continue;
                }
                var category = new Category
                {
                    Name = cat.Name,
                    Description = cat.Description ?? string.Empty
                };
                await _categoryService.AddAsync(category);
                created.Add(new CategoryResponse
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    PostCount = 0,
                    CreatedAt = DateTime.UtcNow
                });
                allExisting.Add(catName);
            }
            return Ok(new
            {
                message = $"{created.Count} categories created, {skipped.Count} skipped (duplicates)",
                categories = created,
                skipped = skipped
            });
        }
    }



}
