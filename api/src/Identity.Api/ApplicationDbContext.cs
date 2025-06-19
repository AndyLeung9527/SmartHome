using Role = Identity.Api.Models.Role;

namespace Identity.Api;

public class ApplicationDbContext : IdentityDbContext<User, Role, long>
{
    public ApplicationDbContext(DbContextOptions options, IdGenerator idGenerator) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>(b =>
        {
            b.Property(u => u.DateOfBirth).IsRequired();
            b.Property(u => u.CreatedAt).IsRequired();
            b.Property(u => u.UpdatedAt).IsRequired();
            b.Property(u => u.LastLoginAt);
        });
    }
}

