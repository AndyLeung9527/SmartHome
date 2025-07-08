namespace SmartHome.Api.Application.Commands;

public class BroadcastReadCommandHandler(IUserRepository userRepository, IBroadcastRepository broadcastRepository) : IRequestHandler<BroadcastReadCommand, (bool, string, List<BroadcastResponse>)>
{
    public async Task<(bool, string, List<BroadcastResponse>)> Handle(BroadcastReadCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return (false, $"用户不存在，Id：{request.UserId}", []);
        }

        var broadcasts = await broadcastRepository.GetAllAsync(cancellationToken);
        var userIds = broadcasts.Select(o => o.UserId).Distinct().ToArray();

        var users = await userRepository.GetByIdsAsync(userIds, cancellationToken);
        var result = (true, string.Empty, broadcasts.Select(o => new BroadcastResponse(users.FirstOrDefault(u => u.Id == o.UserId)?.Name ?? string.Empty, o.Message, o.CreatedAt, o.CreatedAt <= user.LastReadBroadcastAt)).ToList());

        user.UpdateLastReadBroadcastAt();
        userRepository.Update(user);
        await userRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return result;
    }
}
