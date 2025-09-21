using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebApplication4.Services
{
    public class WebSocketClientBackgroundService : BackgroundService
    {
        private readonly ILogger<WebSocketClientBackgroundService> _logger;
        private readonly WebSocketMessageStore _store;
        // 目标 WebSocket 服务器地址
        private readonly Uri _uri = new("ws://127.0.0.1:8008/api/v1/ws/server");

        // 可调参数
        private readonly TimeSpan _reconnectDelay = TimeSpan.FromSeconds(5);
        private const int ReceiveBufferSize = 8 * 1024;

        public WebSocketClientBackgroundService(
            ILogger<WebSocketClientBackgroundService> logger,
            WebSocketMessageStore store)
        {
            _logger = logger;
            _store = store;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WebSocket 后台服务启动, 目标: {Url}", _uri);

            while (!stoppingToken.IsCancellationRequested)
            {
                using var client = new ClientWebSocket();
                try
                {
                    client.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);
                    _logger.LogInformation("尝试连接 WebSocket...");
                    await client.ConnectAsync(_uri, stoppingToken);
                    _logger.LogInformation("WebSocket 已连接: {State}", client.State);

                    await ReceiveLoopAsync(client, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("停止请求收到，终止 WebSocket 循环。");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "WebSocket 连接或接收发生异常，将在 {Delay}s 后重试。",
                        _reconnectDelay.TotalSeconds);
                }

                if (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(_reconnectDelay, stoppingToken);
                    }
                    catch (OperationCanceledException) { }
                }
            }

            _logger.LogInformation("WebSocket 后台服务已结束。");
        }

        private async Task ReceiveLoopAsync(ClientWebSocket client, CancellationToken ct)
        {
            var buffer = new byte[ReceiveBufferSize];

            while (!ct.IsCancellationRequested && client.State == WebSocketState.Open)
            {
                using var ms = new MemoryStream();
                WebSocketReceiveResult? result;

                do
                {
                    var segment = new ArraySegment<byte>(buffer);
                    result = await client.ReceiveAsync(segment, ct);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogWarning("服务器请求关闭: {Desc}", result.CloseStatusDescription);
                        await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
                        return;
                    }

                    ms.Write(buffer, 0, result.Count);
                }
                while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var text = Encoding.UTF8.GetString(ms.ToArray());
                    _store.Set(text);
                    _logger.LogDebug("收到文本消息，长度: {Len}", text.Length);
                }
                else if (result.MessageType == WebSocketMessageType.Binary)
                {
                    _logger.LogDebug("忽略二进制消息，长度: {Len}", ms.Length);
                }
            }
        }
    }
}