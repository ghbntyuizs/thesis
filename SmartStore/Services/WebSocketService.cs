using System.Configuration;
using System.Net.WebSockets;
using System.Text;

namespace SmartStorePOS.Services
{
    public interface IWebSocketService
    {
        Task ConnectAsync(string? deviceId = null);
        Task DisconnectAsync();
        event EventHandler<string> MessageReceived;
    }

    public class WebSocketService : IWebSocketService
    {
        private ClientWebSocket _webSocket;
        private CancellationTokenSource _cts;
        private readonly string _baseUrl = "wss://reinir.mooo.com/ws/pos";
        private readonly string _deviceId;
        private readonly string _token;

        public event EventHandler<string> MessageReceived;

        public WebSocketService()
        {
            _deviceId = ConfigurationManager.AppSettings["DeviceId"];
            _token = ConfigurationManager.AppSettings["AppToken"];
        }

        public async Task ConnectAsync(string deviceId = null)
        {
            _webSocket = new ClientWebSocket();
            _cts = new CancellationTokenSource();

            try
            {
                var actualDeviceId = deviceId ?? _deviceId;
                var wsUrl = $"{_baseUrl}/{actualDeviceId}?token={_token}";
                await _webSocket.ConnectAsync(new Uri(wsUrl), _cts.Token);
                _ = ReceiveMessagesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"WebSocket connection failed: {ex.Message}");
            }
        }

        private async Task ReceiveMessagesAsync()
        {
            var buffer = new byte[4096];

            try
            {
                while (_webSocket.State == WebSocketState.Open)
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        MessageReceived?.Invoke(this, message);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"WebSocket receive error: {ex.Message}");
            }
        }

        public async Task DisconnectAsync()
        {
            if (_webSocket != null && _webSocket.State == WebSocketState.Open)
            {
                try
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    // Log exception
                    Console.WriteLine($"WebSocket disconnect error: {ex.Message}");
                }
            }

            _cts?.Cancel();
            _cts?.Dispose();
            _webSocket?.Dispose();
        }
    }
}
