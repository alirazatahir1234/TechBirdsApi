using System.Threading.Tasks;
using TechBirdsApi.Models;

namespace TechBirdsApi.Interfaces
{
    public interface ISettingsRepository
    {
        Task<Settings?> GetAsync();
        Task UpdateAsync(Settings settings);
    }
}
