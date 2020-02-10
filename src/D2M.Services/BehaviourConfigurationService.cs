using System;
using System.Linq;
using System.Threading.Tasks;
using D2M.Data;
using Microsoft.EntityFrameworkCore;

namespace D2M.Services
{
    public interface IBehaviourConfigurationService
    {
        bool HasValidConfiguration();
        Task Configure();
        Task SetPrefix(char newPrefix);
        char GetPrefix();
        ulong? GetStaffRoleId();
        ulong? GetCategoryId();
        ulong? GetLogChannelId();
        Task SetCategory(ulong categoryId);
        Task SetLogChannel(ulong logChannelId);
        Task SetStaffRole(ulong roleId);
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

        public bool HasValidConfiguration()
        {
            return _cachedBehaviourConfiguration.StaffRoleId != null
                   && _cachedBehaviourConfiguration.ParentCategoryId != null;
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

            _cachedBehaviourConfiguration.HasDoneInitialSetUp =
                GetBoolConfiguration(nameof(_cachedBehaviourConfiguration.HasDoneInitialSetUp), false);

            _cachedBehaviourConfiguration.Prefix =
                GetCharConfiguration(nameof(_cachedBehaviourConfiguration.Prefix), '?');

            _cachedBehaviourConfiguration.IsDisabled =
                GetBoolConfiguration(nameof(_cachedBehaviourConfiguration.IsDisabled), false);

            _cachedBehaviourConfiguration.ParentCategoryId =
                GetUlongConfiguration(nameof(_cachedBehaviourConfiguration.ParentCategoryId), null);

            _cachedBehaviourConfiguration.LogsChannelId =
                GetUlongConfiguration(nameof(_cachedBehaviourConfiguration.LogsChannelId), null);

            _cachedBehaviourConfiguration.StaffRoleId =
                GetUlongConfiguration(nameof(_cachedBehaviourConfiguration.StaffRoleId), null);

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

        public async Task SetPrefix(char newPrefix)
        {
            const string CONFIGURATION_NAME = nameof(_cachedBehaviourConfiguration.Prefix);

            var configuration = await _db
                .Configurations
                .Where(x => x.Property == CONFIGURATION_NAME)
                .SingleAsync();

            configuration.Update(newPrefix.ToString());

            await _db.SaveChangesAsync();

            _cachedBehaviourConfiguration.Prefix = newPrefix;
        }

        public async Task SetCategory(ulong categoryId)
        {
            const string CONFIGURATION_NAME = nameof(_cachedBehaviourConfiguration.ParentCategoryId);

            var configuration = await _db
                .Configurations
                .Where(x => x.Property == CONFIGURATION_NAME)
                .SingleAsync();

            configuration.Update(categoryId.ToString());

            await _db.SaveChangesAsync();

            _cachedBehaviourConfiguration.ParentCategoryId = categoryId;
        }

        public async Task SetLogChannel(ulong logChannelId)
        {
            const string CONFIGURATION_NAME = nameof(_cachedBehaviourConfiguration.LogsChannelId);

            var configuration = await _db
                .Configurations
                .Where(x => x.Property == CONFIGURATION_NAME)
                .SingleAsync();

            configuration.Update(logChannelId.ToString());

            await _db.SaveChangesAsync();

            _cachedBehaviourConfiguration.LogsChannelId = logChannelId;
        }

        public async Task SetStaffRole(ulong roleId)
        {
            const string CONFIGURATION_NAME = nameof(_cachedBehaviourConfiguration.StaffRoleId);

            var configuration = await _db
                .Configurations
                .Where(x => x.Property == CONFIGURATION_NAME)
                .SingleAsync();

            configuration.Update(roleId.ToString());

            await _db.SaveChangesAsync();

            _cachedBehaviourConfiguration.StaffRoleId = roleId;
        }

        public char GetPrefix()
        {
            return _cachedBehaviourConfiguration.Prefix;
        }

        public ulong? GetStaffRoleId()
        {
            return _cachedBehaviourConfiguration.StaffRoleId;
        }

        public ulong? GetCategoryId()
        {
            return _cachedBehaviourConfiguration.ParentCategoryId;
        }

        public ulong? GetLogChannelId()
        {
            return _cachedBehaviourConfiguration.LogsChannelId;
        }
    }
}