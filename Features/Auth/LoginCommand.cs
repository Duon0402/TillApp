using MediatR;
using TillApp.Models;

namespace TillApp.Features
{
    public record LoginCommand(string Username, string Password) : IRequest<LoginResult>;
    public record LoginResult(bool Success, UserRole Role, string? ErrorMessage);

    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
    {
        public Task<LoginResult> Handle(LoginCommand cmd, CancellationToken ct)
        {
            // NOTE: Hardcode test
            if (cmd.Username == "admin" && cmd.Password == "123")
                return Task.FromResult(new LoginResult(true, UserRole.Admin, null));
            if (cmd.Username == "cashier" && cmd.Password == "123")
                return Task.FromResult(new LoginResult(true, UserRole.Cashier, null));

            return Task.FromResult(new LoginResult(false, default, "Sai tài khoản hoặc mật khẩu"));
        }
    }
}
