using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace DocuArchi.Api.Infrastructure.Features
{
    public interface IFeatureToggleService
    {
        Task<bool> IsEnabledAsync(string featureName);
    }

    public sealed class FeatureToggleService : IFeatureToggleService
    {
        private readonly IConfiguration _configuration;

        public FeatureToggleService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<bool> IsEnabledAsync(string featureName)
        {
            var enabled = _configuration.GetValue<bool>($"FeatureFlags:{featureName}");
            return Task.FromResult(enabled);
        }
    }
}
