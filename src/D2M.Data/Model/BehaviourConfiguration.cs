﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace D2M.Data.Model
{
    [Table("__Configurations")]
    public class BehaviourConfiguration
    {
        public int Id { get; internal set; }
        public string Property { get; internal set; } = null!;
        public string Value { get; internal set; } = null!;
        public ConfigurationType Type { get; internal set; }

        public void Update(string value)
        {
            Value = value;
        }
    }

    public class ConfigurationEntityTypeConfiguration : IEntityTypeConfiguration<BehaviourConfiguration>
    {
        public void Configure(EntityTypeBuilder<BehaviourConfiguration> builder)
        {
            builder.HasData(new BehaviourConfiguration
            {
                Id = 1,
                Property = "HasDoneInitialSetUp",
                Value = "False",
                Type = ConfigurationType.Bool,
            }, new BehaviourConfiguration
            {
                Id = 2,
                Property = "Prefix",
                Value = "?",
                Type = ConfigurationType.Char,
            }, new BehaviourConfiguration
            {
                Id = 3,
                Property = "IsDisabled",
                Value = "False",
                Type = ConfigurationType.Bool,
            }, new BehaviourConfiguration
            {
                Id = 4,
                Property = "ParentCategoryId",
                Value = "",
                Type = ConfigurationType.Ulong,
            }, new BehaviourConfiguration
            {
                Id = 5,
                Property = "LogsChannelId",
                Value = "",
                Type = ConfigurationType.Ulong,
            }, new BehaviourConfiguration
            {
                Id = 6,
                Property = "StaffRoleId",
                Value = "",
                Type = ConfigurationType.Ulong,
            });
        }
    }

    public enum ConfigurationType
    {
        Char,
        Bool,
        Ulong
    }
}