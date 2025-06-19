namespace Identity.Api.Models;

public class Role : IdentityRole<long>
{
    public Role(long id, string name) : base(name)
    {
        Id = id;
    }

    protected Role() { }
}
