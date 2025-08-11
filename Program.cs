using TechBirdsWebAPI.Data;
using TechBirdsWebAPI.Models;
using TechBirdsWebAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add Entity Framework for Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🔐 Add HttpContextAccessor for logging services
builder.Services.AddHttpContextAccessor();

// 🔐 Register logging services
builder.Services.AddScoped<IUserActivityService, UserActivityService>();
builder.Services.AddScoped<IExceptionLoggerService, ExceptionLoggerService>();

// ✅ ADD CORS CONFIGURATION
builder.Services.AddCors(options =>
{
    options.AddPolicy("TechBirdsFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",    // Vite dev server
            "https://localhost:5173",   // Vite HTTPS
            "http://localhost:5174",    // Alternative Vite port
            "https://localhost:5174",   // Alternative Vite HTTPS
            "http://localhost:5175",    // Another Vite port
            "http://localhost:5176",    // Current frontend port
            "http://localhost:3000",    // Alternative port
            "https://localhost:3000"    // Alternative HTTPS
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .SetIsOriginAllowed(origin => true); // Allow any origin in development
    });
});

// Remove Dapper dependencies - using only Entity Framework now
// builder.Services.AddSingleton<DapperContext>();
// builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
// builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
// builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
// builder.Services.AddScoped<ICommentRepository, CommentRepository>();
// builder.Services.AddScoped<DatabaseInitializer>();

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured")))
    };
});

var app = builder.Build();

// Seed admin user and roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        
        // Create roles if they don't exist
        string[] roles = { "Subscriber", "Contributor", "Author", "Admin", "Editor", "SuperAdmin" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = role });
            }
        }
        
        // Create default admin user if it doesn't exist
        var adminEmail = "admin@techbirds.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                Name = "System Administrator",
                FirstName = "System",
                LastName = "Administrator", 
                Bio = "Default system administrator with admin access",
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            
            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
    catch (Exception ex)
    {
        // Log the error (you might want to add proper logging)
        Console.WriteLine($"Error seeding data: {ex.Message}");
    }
    
    // Remove DatabaseInitializer - using Entity Framework migrations now
    // try
    // {
    //     var dbInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    //     await dbInitializer.InitializeAsync();
    // }
    // catch (Exception ex)
    // {
    //     Console.WriteLine($"Error initializing database schema: {ex.Message}");
    // }
}

// Enable Swagger in all environments
app.UseSwagger();
app.UseSwaggerUI();

// Enable HTTPS redirection for production
app.UseHttpsRedirection();

// ✅ USE CORS (MUST BE BEFORE UseAuthorization)
app.UseCors("TechBirdsFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
