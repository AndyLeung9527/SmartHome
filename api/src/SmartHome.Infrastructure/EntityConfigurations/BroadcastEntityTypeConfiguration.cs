namespace SmartHome.Infrastructure.EntityConfigurations;

public class BroadcastEntityTypeConfiguration : IEntityTypeConfiguration<Broadcast>
{
    public void Configure(EntityTypeBuilder<Broadcast> broadcastConfiguration)
    {
        broadcastConfiguration.ToTable("broadcasts");

        broadcastConfiguration.HasKey(o => o.Id);

        broadcastConfiguration.Property(o => o.UserId)
            .HasComment("用户ID")
            .IsRequired();

        broadcastConfiguration.Property(o => o.Message)
            .HasComment("消息内容")
            .HasMaxLength(500)
            .IsRequired(false);

        broadcastConfiguration.Property(o => o.Valid)
            .HasComment("是否有效")
            .IsRequired();

        broadcastConfiguration.Property(o => o.CreatedAt)
            .HasComment("创建时间")
            .IsRequired();

        broadcastConfiguration.Property(o => o.ExpirationDate)
            .HasComment("过期时间")
            .IsRequired();

        broadcastConfiguration.HasIndex(o => o.UserId);
        broadcastConfiguration.HasIndex(o => o.CreatedAt);
        broadcastConfiguration.HasIndex(o => new { o.Valid, o.ExpirationDate });
    }
}
