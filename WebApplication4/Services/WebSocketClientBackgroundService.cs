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
        // Ŀ�� WebSocket ��������ַ
        private readonly Uri _uri = new("ws://127.0.0.1:8008/api/v1/ws/server");

        // �ɵ�����
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
            _logger.LogInformation("WebSocket ��̨��������, Ŀ��: {Url}", _uri);

            while (!stoppingToken.IsCancellationRequested)
            {
                using var client = new ClientWebSocket();
                try
                {
                    client.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);
                    _logger.LogInformation("�������� WebSocket...");
                    await client.ConnectAsync(_uri, stoppingToken);
                    _logger.LogInformation("WebSocket ������: {State}", client.State);

                    await ReceiveLoopAsync(client, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("ֹͣ�����յ�����ֹ WebSocket ѭ����");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "WebSocket ���ӻ���շ����쳣������ {Delay}s �����ԡ�",
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

            _logger.LogInformation("WebSocket ��̨�����ѽ�����");
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
                        _logger.LogWarning("����������ر�: {Desc}", result.CloseStatusDescription);
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
                    _logger.LogDebug("�յ��ı���Ϣ������: {Len}", text.Length);
                }
                else if (result.MessageType == WebSocketMessageType.Binary)
                {
                    _logger.LogDebug("���Զ�������Ϣ������: {Len}", ms.Length);
                }
            }
        }
    }
}