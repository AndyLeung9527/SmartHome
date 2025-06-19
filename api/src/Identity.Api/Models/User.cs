namespace Identity.Api.Models;

public class User : IdentityUser<long>
{
    public DateTimeOffset DateOfBirth { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? LastLoginAt { get; set; }

    public User(long id, string name, string email, DateTimeOffset dateOfBirth) : base(name)
    {
        Id = id;
        Email = email;
        DateOfBirth = dateOfBirth;
        CreatedAt = DateTimeOffset.Now;
        UpdatedAt = DateTimeOffset.Now;
        LastLoginAt = null;
    }

    protected User() { }
}
