using System.Collections.Concurrent;
using DriveShare.API.Models;

namespace DriveShare.API.Services
{
    public class SessionManager
    {
        private static readonly object _lock = new object();
        private static SessionManager? _instance;
        private readonly ConcurrentDictionary<string, UserSession> _activeSessions;

        private SessionManager()
        {
            _activeSessions = new ConcurrentDictionary<string, UserSession>();
        }

        public static SessionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new SessionManager();
                    }
                }
                return _instance;
            }
        }

        public void AddSession(string userId, string token)
        {
            var session = new UserSession
            {
                UserId = userId,
                Token = token,
                LastActivity = DateTime.UtcNow
            };
            _activeSessions.AddOrUpdate(userId, session, (_, _) => session);
        }

        public bool RemoveSession(string userId)
        {
            return _activeSessions.TryRemove(userId, out _);
        }

        public bool IsSessionActive(string userId)
        {
            if (_activeSessions.TryGetValue(userId, out var session))
            {
                // Session expires after 24 hours of inactivity
                if ((DateTime.UtcNow - session.LastActivity).TotalHours > 24)
                {
                    RemoveSession(userId);
                    return false;
                }
                // Update last activity
                session.LastActivity = DateTime.UtcNow;
                return true;
            }
            return false;
        }

        public void UpdateLastActivity(string userId)
        {
            if (_activeSessions.TryGetValue(userId, out var session))
            {
                session.LastActivity = DateTime.UtcNow;
            }
        }

        public int GetActiveSessionCount()
        {
            // Clean up expired sessions before counting
            var expiredSessions = _activeSessions
                .Where(kvp => (DateTime.UtcNow - kvp.Value.LastActivity).TotalHours > 24)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var userId in expiredSessions)
            {
                RemoveSession(userId);
            }

            return _activeSessions.Count;
        }
    }

    public class UserSession
    {
        public required string UserId { get; set; }
        public required string Token { get; set; }
        public DateTime LastActivity { get; set; }
    }
} 