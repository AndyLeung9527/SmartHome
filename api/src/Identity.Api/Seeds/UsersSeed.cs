namespace Identity.Api.Seeds;

public class UsersSeed(ILogger<UsersSeed> logger, UserManager<User> userManager, IConfiguration configuration, IdGenerator idGenerator) : IDbSeeder<ApplicationDbContext>
{
    public async Task SeedAsync(ApplicationDbContext context)
    {
        var adminPassword = configuration[UserConsts.AdministratorPasswordConfigKey];
        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new ArgumentException($"Configuration key '{UserConsts.AdministratorPasswordConfigKey}' is not set or is empty.");
        }

        var admin = await userManager.FindByNameAsync(UserConsts.AdministratorUserName);
        if (admin is null)
        {
            admin = new User(idGenerator.CreateId(), UserConsts.AdministratorUserName, UserConsts.AdministratorEmail, UserConsts.AdministratorDateOfBirth)
            {
                EmailConfirmed = true
            };
            var createResult = await userManager.CreateAsync(admin, adminPassword);
            if (!createResult.Succeeded)
            {
                throw new Exception(string.Join(", ", createResult.Errors.Select(o => o.Description)));
            }
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("admin created");
            }

            var addRoleResult = await userManager.AddToRoleAsync(admin, RoleConsts.AdministratorRoleName);
            if (!addRoleResult.Succeeded)
            {
                throw new Exception(string.Join(", ", addRoleResult.Errors.Select(o => o.Description)));
            }
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("admin has added to administrator role");
            }
        }
        else
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("admin already exists");
            }
        }
    }
}
