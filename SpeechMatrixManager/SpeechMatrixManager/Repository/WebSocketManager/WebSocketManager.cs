using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace SpeechMatrixManager.Repository.WebSocketManager
{
    public class SocketManager
    {

        private const string SpeechmaticsWebSocketUrl = "wss://neu.rt.speechmatics.com/v2";

        private readonly string _authToken; // Speechmatics authentication token

        private int sequenceNumber = 0;

        public SocketManager(string authToken)
        {
            _authToken = authToken;
        }

        public async Task<string> StartWebSocketAsync(byte[] audioData)
        {
           
            using (ClientWebSocket webSocket = new ClientWebSocket())
            {
                // Add authentication headers
                webSocket.Options.SetRequestHeader("Authorization", $"Bearer {_authToken}");

                Console.WriteLine("Connecting to Speechmatics WebSocket...");

                var response = webSocket.ConnectAsync(new Uri(SpeechmaticsWebSocketUrl), CancellationToken.None);

                try
                {
                    await response;
                    Console.WriteLine("WebSocket connected to Speechmatics.");

                }
                catch (Exception e)
                {
                    Console.WriteLine("Connection failed");
                    return ("Connection failed!");
                }

                // Sending configuration message (optional, based on Speechmatics API requirements)
                var configMessage = new
                {
                    message = "StartRecognition",
                    audio_format = new
                    {
                        type = "raw",
                        encoding = "pcm_s16le",
                        sample_rate = 441000
                    },
                    transcription_config = new
                    {
                        language = "en",
                        operating_point = "enhanced",
                        output_locale = "en-US",
                        additional_vocab = new[] { "gnocchi", "bucatini", "bigoli" },
                        diarization = "speaker",
                        enable_partials = true
                    },
                    translation_config = new
                    {
                        target_languages = new[] { "es", "de" },
                        enable_partials = true
                    }
                    // Add other configuration parameters as needed
                };

                string configJson = JsonConvert.SerializeObject(configMessage);
                await SendWebSocketMessage(webSocket, configJson);



               var recognitionResponse = await ReceiveWebSocketResponse(webSocket);
                
                Console.WriteLine(recognitionResponse);
                var responseObj = JsonConvert.DeserializeObject<dynamic>(recognitionResponse);

                // Get the value of the "message" field
                string MessageRecieved = responseObj.message;
                if (MessageRecieved.StartsWith("RecognitionStarted"))
                {
                    try
                    {
                        await SendWebSocketAudioData(webSocket, audioData);
                        var Responsee = await ReceiveWebSocketResponse(webSocket);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error : {0}", ex);
                    }

                    var Response = await ReceiveWebSocketResponse(webSocket);
                  
                    var responseObj1 = JsonConvert.DeserializeObject<dynamic>(Response);

                    // Get the value of the "message" field
                    string MessageRecieved1 = responseObj1.message;
                    if (MessageRecieved1.StartsWith("AudioAdded"))
                    {
                        var Transcription = await ReceiveWebSocketResponse(webSocket);
                        return Transcription;
                    }
                    else
                        return (" Failed to Add Audio!");
                }
                else
                    return (" Recognition Failed!");

            }
        }




        public async Task SendWebSocketAudioData(ClientWebSocket webSocket, byte[] audioData)
        {
            try
            {

                // Send audio data to Speechmatics
                await webSocket.SendAsync(new ArraySegment<byte>(audioData),
                                           WebSocketMessageType.Binary, true, CancellationToken.None);

            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., connection closed, send failures)
                Console.WriteLine($"Error sending audio data: {ex.Message}");
            }
        }

        public async Task<string> ReceiveWebSocketResponse(ClientWebSocket webSocket)
        {
            try
            {
                var buffer = new byte[50 * 1024];
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text && !result.CloseStatus.HasValue)
                {
                    return Encoding.UTF8.GetString(buffer, 0, result.Count);
                }

                // Handle other message types if necessary (Binary, Close, etc.)
                return "Unexpected message type received.";
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., receive failures, connection closed)
                Console.WriteLine($"Error receiving WebSocket response: {ex.Message}");
                return null;
            }
        }

        private async Task SendWebSocketMessage(ClientWebSocket webSocket, string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

    }
}
