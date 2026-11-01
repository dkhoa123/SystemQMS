
using System.Collections.Concurrent;
    namespace SystemQMS.Services
    {
        public class SessionService
        {
            private readonly ConcurrentDictionary<int, UserSession> _sessions = new();

            // Lấy phiên hiện tại của người dùng
            public UserSession? GetSession(int userId)
            {
                _sessions.TryGetValue(userId, out var session);
                return session;
            }
            public UserSession? GetSessionByConnection(string connectionId)
            {
                return _sessions.Values
                    .FirstOrDefault(x => x.ConnectionId == connectionId);
            }

        // Đăng ký phiên mới, trả về phiên cũ nếu có
        public UserSession? SetSession(
                int userId,
                string deviceType,
                string connectionId)
            {
                var newSession = new UserSession
                {
                    UserId = userId,
                    DeviceType = deviceType,
                    ConnectionId = connectionId
                };

                _sessions.TryGetValue(userId, out var oldSession);
                _sessions[userId] = newSession;

                return oldSession;
            }

            // Xóa phiên khi người dùng ngắt kết nối
            public void RemoveSession(int userId, string connectionId)
            {
                if (_sessions.TryGetValue(userId, out var session)
                    && session.ConnectionId == connectionId)
                {
                    _sessions.TryRemove(userId, out _);
                }
            }

            
    }


        public class UserSession
        {
            public int UserId { get; set; }
            public string DeviceType { get; set; } = string.Empty;
            public string ConnectionId { get; set; } = string.Empty;
        }
    
}

