namespace SmartHome.Infrastructure.EntityConfigurations;

public class UserEnityTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> userConfiguration)
    {
        userConfiguration.ToTable("users");

        userConfiguration.HasKey(o => o.Id);

        userConfiguration.Property(o => o.Id)
            .HasComment("用户ID")
            .IsRequired();

        userConfiguration.Property(o => o.Name)
            .HasComment("用户名")
            .HasMaxLength(50)
            .IsRequired(false);

        userConfiguration.Property(o => o.Email)
            .HasComment("邮箱")
            .HasMaxLength(100)
            .IsRequired(false);

        userConfiguration.Property(o => o.DateOfBirth)
            .HasComment("出生日期")
            .IsRequired();

        userConfiguration.Property(o => o.CreatedAt)
            .HasComment("创建时间")
            .IsRequired();

        userConfiguration.Property(o => o.LastLoginAt)
            .HasComment("最后登录时间")
            .IsRequired();

        userConfiguration.Property(o => o.LastReadBroadcastAt)
            .HasComment("最后看公告时间")
            .IsRequired();
    }
}
