using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace SpeechMatrixManager.Repository.PingPongService
{
    public class PingPongMechanism
    {

        private readonly ClientWebSocket _webSocket;
        private readonly Uri _serverUri;
        private readonly Timer _pingTimer;

        public PingPongMechanism(Uri serverUri)
        {
            _serverUri = serverUri;
            _webSocket = new ClientWebSocket();

            // Set up a timer to periodically check the WebSocket state
            _pingTimer = new Timer(CheckWebSocketState, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

        public async Task StartWebSocketConnection()
        {
            await _webSocket.ConnectAsync(_serverUri, CancellationToken.None);
            Console.WriteLine("Connected to WebSocket server.");
        }

        private async void SendPing(object state)
        {
            try
            {
                if (_webSocket.State == WebSocketState.Open)
                {
                    // Send a ping message
                    byte[] pingBuffer = new byte[1] { 0x9 }; // Ping opcode
                    await _webSocket.SendAsync(new ArraySegment<byte>(pingBuffer), WebSocketMessageType.Binary, true, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending ping: {ex.Message}");
            }
        }
        private async void CheckWebSocketState(object state)
        {
            try
            {
                if (_webSocket.State != WebSocketState.Open)
                {
                    Console.WriteLine($"WebSocket state: {_webSocket.State}. Reconnecting...");
                    await StartWebSocketConnection();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking WebSocket state: {ex.Message}");
            }
        }
    }
}
