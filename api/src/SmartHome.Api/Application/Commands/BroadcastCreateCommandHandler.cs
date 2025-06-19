namespace SmartHome.Api.Application.Commands;

public class BroadcastCreateCommandHandler(IdGenerator idGenerator, IUserRepository userRepository, IBroadcastRepository broadcastRepository) : IRequestHandler<BroadcastCreateCommand, (bool, string)>
{
    public async Task<(bool, string)> Handle(BroadcastCreateCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return (false, $"用户不存在，Id：{request.UserId}");
        }

        var broadcast = new Broadcast(idGenerator.CreateId(), request.UserId, request.Message, DateTimeOffset.Now.AddMonths(1));
        broadcastRepository.Add(broadcast);
        bool result = await broadcastRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return (result, result ? string.Empty : "公告发布失败，保存异常");
    }
}
