using System.Threading.Tasks;
using TechBirdsApi.Interfaces;
using TechBirdsApi.Models;
using TechBirdsApi.Repositories;

namespace TechBirdsApi.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ISettingsRepository _settingsRepository;
        public SettingsService(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        public async Task<Settings?> GetAsync()
        {
            return await _settingsRepository.GetAsync();
        }

        public async Task UpdateAsync(Settings settings)
        {
            await _settingsRepository.UpdateAsync(settings);
        }
    }
}
