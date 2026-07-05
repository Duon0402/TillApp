using TillApp.Models;

namespace TillApp.Services
{
    public interface ISessionService
    {
        string? CurrentUsername { get; }
        UserRole? CurrentRole { get; }
        void SetUser(string? username, UserRole? role);
    }
}
