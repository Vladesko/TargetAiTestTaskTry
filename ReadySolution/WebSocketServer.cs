using System.Linq;
using System.Net;
using System.Net.WebSockets;

namespace ReadySolution
{
    public class WebSocketServer(HttpListener listener)
    {
        private const string FILE_PATH = "SoundRaw.raw";

        private readonly HttpListener _listener = listener;
        private readonly SharedMemoryManager _sharedMemory = new("AudioBuffer", 1024);
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(6);
        private Timer _inactivityTimer;
        private readonly CancellationTokenSource _cts = new();

        /// <summary>
        /// Start web socket server
        /// </summary>
        /// <param name="adress">Uri for connection to server</param>
        /// <returns></returns>
        public async Task StartAsync(string adress)
        {
            _listener.Prefixes.Add(adress);
            _listener.Start();
            Console.WriteLine("WebSocket сервер запущен!");

            while (!_cts.Token.IsCancellationRequested)
            {
                var context = await _listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    var wsContext = await context.AcceptWebSocketAsync(null);
                    await HandleConnectionAsync(wsContext.WebSocket, _cts.Token);
                }
            }
        }

        private async Task HandleConnectionAsync(WebSocket webSocket, CancellationToken token)
        {
            byte[] buffer = new byte[1024];
            _inactivityTimer = new Timer(async state => await CloseWebSocketOnTimeout(webSocket), null, _timeout, Timeout.InfiniteTimeSpan);
            while (webSocket.State == WebSocketState.Open && !token.IsCancellationRequested)
            {
                ResetInactivityTimer();
                var result = await webSocket.ReceiveAsync(buffer, token);
                Console.WriteLine($"Получено сообщение: {result.Count} байт");

                if (result.Count > 0)
                    await SaveDataToFile(FILE_PATH, buffer, result);

                if (webSocket.State == WebSocketState.Closed)
                {
                    _cts.Cancel();
                    break;
                }
            }
        }
        private async Task SaveDataToFile(string filePath, byte[] buffer, WebSocketReceiveResult result)
        {
            await _sharedMemory.WriteAsync(buffer.Take(result.Count).ToArray());
            Console.WriteLine("Данные записаны в SharedMemory");
            var data = await _sharedMemory.ReadAsync(1024);
            using var fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write);
            fileStream.Write(data, 0, data.Length);
            Console.WriteLine("Данные успешно сохранены в outputAudio.raw!");
        }
        private void ResetInactivityTimer()
        {
            _inactivityTimer?.Change(_timeout, Timeout.InfiniteTimeSpan);
        }

        private async Task CloseWebSocketOnTimeout(WebSocket webSocket)
        {
            Console.WriteLine("Соединение закрывается из-за отсутствия данных в течение заданного времени.");
            await CloseWebSocketAsync(webSocket);
        }

        private async Task CloseWebSocketAsync(WebSocket webSocket)
        {
            if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived)
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Закрыто из-за таймаута", CancellationToken.None);

            _inactivityTimer?.Dispose();
        }
    }
}
