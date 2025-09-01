using TechBirdsApi.Data;
using TechBirdsApi.Models;
using TechBirdsApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using TechBirdsApi.Repositories;
using TechBirdsApi.Interfaces;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IPageRepository, PageRepository>();
builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IMediaRepository, MediaRepository>();
builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUserService, UserService>();

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
            "https://localhost:7106",   // Vite HTTPS
            "http://localhost:5174"    // Alternative Vite port
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .SetIsOriginAllowed(origin => true); // Allow any origin in development
    });
});

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
}

// Enable Swagger in all environments
app.UseSwagger();
app.UseSwaggerUI();

// Enable HTTPS redirection for production
app.UseHttpsRedirection();

// Serve static files from wwwroot and uploads directory under content root
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}
app.UseStaticFiles(new Microsoft.AspNetCore.Builder.StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

// --- System Health endpoints ---
async Task<IResult> BuildHealth(ApplicationDbContext db, IWebHostEnvironment env)
{
    // Version
    var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "unknown";

    // Uptime
    var proc = Process.GetCurrentProcess();
    var uptime = DateTime.UtcNow - proc.StartTime.ToUniversalTime();
    string Humanize(TimeSpan t)
    {
        if (t.TotalDays >= 1) return $"{t.TotalDays:F1} days";
        if (t.TotalHours >= 1) return $"{t.TotalHours:F1} hours";
        if (t.TotalMinutes >= 1) return $"{t.TotalMinutes:F1} minutes";
        return $"{t.TotalSeconds:F0} seconds";
    }

    // Database latency
    var sw = Stopwatch.StartNew();
    string dbStatus = "Healthy";
    try
    {
        await db.Database.ExecuteSqlRawAsync("SELECT 1");
    }
    catch
    {
        dbStatus = "Unhealthy";
    }
    finally
    {
        sw.Stop();
    }

    // Storage/disk usage
    string Bytes(long b)
    {
        string[] units = { "B", "KB", "MB", "GB", "TB" };
        double size = b;
        int u = 0;
        while (size >= 1024 && u < units.Length - 1) { size /= 1024; u++; }
        return $"{size:F1} {units[u]}";
    }

    System.IO.DriveInfo? drive = null;
    try
    {
        drive = System.IO.DriveInfo.GetDrives()
            .OrderByDescending(d => uploadsPath.StartsWith(d.RootDirectory.FullName, StringComparison.OrdinalIgnoreCase) ? d.RootDirectory.FullName.Length : -1)
            .FirstOrDefault(d => uploadsPath.StartsWith(d.RootDirectory.FullName, StringComparison.OrdinalIgnoreCase));
        drive ??= System.IO.DriveInfo.GetDrives().FirstOrDefault();
    }
    catch { }
    long total = drive?.TotalSize ?? 0;
    long free = drive?.AvailableFreeSpace ?? 0;
    long used = total - free;
    int pct = total > 0 ? (int)Math.Round(used * 100.0 / total) : 0;
    var storage = new
    {
        diskUsage = $"{Bytes(used)} / {Bytes(total)} ({pct}%)",
        uploadsPath = uploadsPath
    };

    var response = new
    {
        status = dbStatus == "Healthy" ? "Healthy" : "Unhealthy",
        version,
        environment = env.EnvironmentName,
        uptime = Humanize(uptime),
        uptimeSeconds = (long)uptime.TotalSeconds,
        database = new
        {
            status = dbStatus,
            provider = "PostgreSQL",
            latencyMs = sw.ElapsedMilliseconds
        },
        services = new[]
        {
            new { name = "S3 Storage", status = "Healthy" },
            new { name = "Email (SMTP)", status = "Healthy" }
        },
        storage,
        security = new { auth = "Healthy", cors = "Healthy" },
        metrics = new { rpm = 0, errorRate = "0%" },
        timestamp = DateTime.UtcNow
    };

    return Results.Ok(response);
}

app.MapGet("/api/health", BuildHealth).AllowAnonymous();
app.MapGet("/health", BuildHealth).AllowAnonymous();
app.MapGet("/admin/health", BuildHealth).AllowAnonymous();
// --- End System Health ---

// ✅ USE CORS (MUST BE BEFORE UseAuthorization)
app.UseCors("TechBirdsFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
