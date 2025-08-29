using System.Threading.Tasks;
using TechBirdsApi.Models;

namespace TechBirdsApi.Services
{
    public interface ISettingsService
    {
        Task<Settings?> GetAsync();
        Task UpdateAsync(Settings settings);
    }
}
