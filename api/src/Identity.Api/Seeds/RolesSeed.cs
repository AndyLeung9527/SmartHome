using Role = Identity.Api.Models.Role;

namespace Identity.Api.Seeds;

public class RolesSeed(ILogger<RolesSeed> logger, RoleManager<Role> roleManager, IdGenerator idGenerator) : IDbSeeder<ApplicationDbContext>
{
    public async Task SeedAsync(ApplicationDbContext context)
    {
        var adminRole = await roleManager.FindByNameAsync(RoleConsts.AdministratorRoleName);
        if (adminRole is null)
        {
            adminRole = new Role(idGenerator.CreateId(), RoleConsts.AdministratorRoleName);
            var result = await roleManager.CreateAsync(adminRole);
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(", ", result.Errors.Select(o => o.Description)));
            }
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("administrator role created");
            }
        }
        else
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("administrator role already exists");
            }
        }

        var guestRole = await roleManager.FindByNameAsync(RoleConsts.GuestRoleName);
        if (guestRole is null)
        {
            guestRole = new Role(idGenerator.CreateId(), RoleConsts.GuestRoleName);
            var result = await roleManager.CreateAsync(guestRole);
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(", ", result.Errors.Select(o => o.Description)));
            }
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("guest role created");
            }
        }
        else
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("guest role already exists");
            }
        }
    }
}
