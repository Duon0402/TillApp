using MediatR;

namespace TillApp.Features
{
    public record PingCommand : IRequest<string>;

    public class PingCommandHandler : IRequestHandler<PingCommand, string>
    {
        public Task<string> Handle(PingCommand request, CancellationToken ct)
            => Task.FromResult("Pong");
    }
}
