using System.Collections.Concurrent;

namespace SystemQMS.Services
{
    public class KeypadConnectionService
    {
        // Key: counterId, Value: connectionId
        private readonly ConcurrentDictionary<int, string> _connections = new();

        public void Connect(int counterId, string connectionId)
        {
            // Nếu connection đang kết nối quầy khác,
            // xóa kết nối cũ trước khi thêm kết nối mới.
            var oldCounterId = GetCounterByConnection(connectionId);

            if (oldCounterId.HasValue && oldCounterId.Value != counterId)
            {
                Disconnect(oldCounterId.Value, connectionId);
            }

            _connections[counterId] = connectionId;
        }

        public void Disconnect(int counterId, string connectionId)
        {
            if (_connections.TryGetValue(counterId, out var currentConnection)
                && currentConnection == connectionId)
            {
                _connections.TryRemove(counterId, out _);
            }
        }

        public bool IsConnected(int counterId)
        {
            return _connections.ContainsKey(counterId);
        }

        public int? GetCounterByConnection(string connectionId)
        {
            foreach (var item in _connections)
            {
                if (item.Value == connectionId)
                {
                    return item.Key;
                }
            }

            return null;
        }

        public int[] GetConnectedCounters()
        {
            return _connections.Keys
                .OrderBy(id => id)
                .ToArray();
        }
    }
}