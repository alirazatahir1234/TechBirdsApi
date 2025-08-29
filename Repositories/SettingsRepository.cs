using System.Threading.Tasks;
using TechBirdsApi.Data;
using TechBirdsApi.Interfaces;
using TechBirdsApi.Models;

namespace TechBirdsApi.Repositories
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly ApplicationDbContext _context;
        public SettingsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Settings?> GetAsync()
        {
            return await Task.FromResult(_context.Settings.FirstOrDefault());
        }

        public async Task UpdateAsync(Settings settings)
        {
            _context.Settings.Update(settings);
            await _context.SaveChangesAsync();
        }
    }
}
