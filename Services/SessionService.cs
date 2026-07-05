using TillApp.Models;

namespace TillApp.Services
{
    public class SessionService : ISessionService
    {
        public string? CurrentUsername { get; private set; }
        public UserRole? CurrentRole { get; private set; }

        public void SetUser(string? username, UserRole? role)
        {
            CurrentUsername = username;
            CurrentRole = role;
        }
    }
}
