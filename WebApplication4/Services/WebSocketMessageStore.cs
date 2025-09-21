using System;
using System.Threading;

namespace WebApplication4.Services
{
    public class WebSocketMessageStore
    {
        private string? _lastMessage;
        private DateTime? _receivedAtUtc;
        private readonly object _lock = new();

        public void Set(string message)
        {
            if (message == null) return;
            lock (_lock)
            {
                _lastMessage = message;
                _receivedAtUtc = DateTime.UtcNow;
            }
        }

        public (string? Message, DateTime? ReceivedAtUtc) Get()
        {
            lock (_lock)
            {
                return (_lastMessage, _receivedAtUtc);
            }
        }
    }
}