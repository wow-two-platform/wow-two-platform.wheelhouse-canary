using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WheelhouseCanary.Domain.Pings.Entities;

namespace WheelhouseCanary.Persistence.Configurations;

/// <summary>Configures the pings table mapping.</summary>
public sealed class PingEntityConfiguration : IEntityTypeConfiguration<PingEntity>
{
    /// <summary>Maps <see cref="PingEntity"/> onto its table.</summary>
    public void Configure(EntityTypeBuilder<PingEntity> builder)
    {
        builder.ToTable(PingEntity.TableName);

        builder.HasKey(ping => ping.Id);
    }
}
