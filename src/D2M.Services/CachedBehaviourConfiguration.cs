using System.Linq;
using System.Threading.Tasks;
using D2M.Data;
using Microsoft.EntityFrameworkCore;

namespace D2M.Services
{
    public interface IBehaviourConfigurationService
    {
        Task Configure();
    }

    public class BehaviourConfigurationService : IBehaviourConfigurationService
    {
        private readonly EntityContext _db;
        private readonly ICachedBehaviourConfiguration _cachedBehaviourConfiguration;

        public BehaviourConfigurationService(EntityContext db, 
            ICachedBehaviourConfiguration cachedBehaviourConfiguration)
        {
            _db = db;
            _cachedBehaviourConfiguration = cachedBehaviourConfiguration;
        }

        public async Task Configure()
        {
            var configurations = await _db.Configurations
                .Select(x => new
                {
                    x.Property,
                    x.Value,
                    x.Type,
                }).ToListAsync();

            _cachedBehaviourConfiguration.Prefix =
                GetCharConfiguration(nameof(_cachedBehaviourConfiguration.Prefix), '?');

            _cachedBehaviourConfiguration.IsDisabled =
                GetBoolConfiguration(nameof(_cachedBehaviourConfiguration.IsDisabled), false);

            _cachedBehaviourConfiguration.LogsChannelId =
                GetUlongConfiguration(nameof(_cachedBehaviourConfiguration.LogsChannelId), null);

            ulong? GetUlongConfiguration(string propertyName, ulong? defaultValue)
            {
                var configuration = configurations.Single(x => x.Property == propertyName);

                return ulong.TryParse(configuration.Value, out var actual) ? actual : defaultValue;
            }

            bool GetBoolConfiguration(string propertyName, bool defaultValue)
            {
                var configuration = configurations.Single(x => x.Property == propertyName);

                return bool.TryParse(configuration.Value, out var actual) ? actual : defaultValue;
            }

            char GetCharConfiguration(string propertyName, char defaultValue)
            {
                var configuration = configurations.Single(x => x.Property == propertyName);

                return char.TryParse(configuration.Value, out var actual) ? actual : defaultValue;
            }
        }
    }

    public interface ICachedBehaviourConfiguration
    {
        char Prefix { get; set; }
        ulong? LogsChannelId { get; set; }
        bool IsDisabled { get; set; }
    }

    public class CachedBehaviourConfiguration : ICachedBehaviourConfiguration
    {
        public ulong? LogsChannelId { get; set; }
        public bool IsDisabled { get; set; }
        public char Prefix { get; set; }
    }
}
