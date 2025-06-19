namespace SmartHome.Api.Application.Commands;

public class UserLoggedInCommandHandler(ILogger<UserLoggedInCommandHandler> logger, IUserRepository userRepository) : IRequestHandler<UserLoggedInCommand, bool>
{
    public async Task<bool> Handle(UserLoggedInCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindByIdAsync(request.Id, cancellationToken);
        if (user is null)
        {
            user = new User(request.Id, request.Name, request.Email, request.dateOfBirth);
            userRepository.Add(user);
            return await userRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }
        else
        {
            user.UpdateLastLoginAt();
            userRepository.Update(user);
            return await userRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }
    }
}
