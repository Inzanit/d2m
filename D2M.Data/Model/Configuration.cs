using System.ComponentModel.DataAnnotations.Schema;

namespace D2M.Data.Model
{
    [Table("__Configurations")]
    public class Configuration
    {
        public int Id { get; set; }
        public string Property { get; set; } = null!;
        public string Value { get; set; } = null!;
        public ConfigurationType Type { get; set; }
    }

    public enum ConfigurationType
    {
        Char,
    }
}