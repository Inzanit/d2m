using System.Threading.Tasks;

namespace D2M.Services
{
    public interface ICachedBehaviourConfiguration
    {
        bool HasDoneInitialSetUp { get; set; }
        char Prefix { get; set; }
        bool IsDisabled { get; set; }
        ulong? ParentCategoryId { get; set; }
        ulong? LogsChannelId { get; set; }
        ulong? StaffRoleId { get; set; }
    }

    public class CachedBehaviourConfiguration : ICachedBehaviourConfiguration
    {
        public bool HasDoneInitialSetUp { get; set; }
        public char Prefix { get; set; }
        public bool IsDisabled { get; set; }
        public ulong? ParentCategoryId { get; set; }
        public ulong? LogsChannelId { get; set; }
        public ulong? StaffRoleId { get; set; }
    }
}
