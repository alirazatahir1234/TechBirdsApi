using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TechBirdsWebAPI.Models;
using TechBirdsWebAPI.Utils;

namespace TechBirdsWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IConfiguration _config;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                Name = request.Name,
                Bio = request.Bio
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            
            if (result.Succeeded)
            {
                // Assign default "User" role
                await _userManager.AddToRoleAsync(user, "User");
                return Ok(new { message = "User created successfully" });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(AuthRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Unauthorized();

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
                return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);
            var token = JwtHelper.GenerateTokenForIdentityUser(user, roles.ToList(), _config["Jwt:Secret"]);
            
            return Ok(new { token, role = roles.FirstOrDefault() });
        }

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
                return BadRequest("Role does not exist");

            await _userManager.AddToRoleAsync(user, roleName);
            return Ok(new { message = $"Role {roleName} assigned to user" });
        }
    }
}