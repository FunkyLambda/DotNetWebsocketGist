using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebsocketGist
{
    class Program
    {
        private ClientWebSocket _socket;
        private bool _continueReceiving;

        static void Main(string[] args)
        {
            // For compatibility with tools (like replit) that use C# v < 7.1,
            // and therefore don't support async Main.
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            var program = new Program();
            await program.keepConnectingUntilConnected();
            await program.stopReceiving();
        }

        private async Task keepConnectingUntilConnected()
        {
            _socket = new ClientWebSocket();

            try
            {
                bool socket_connected = false;
                do
                {
                    try
                    {
                        await _socket.ConnectAsync(new Uri("wss://ws.bitmex.com/realtime"), CancellationToken.None);
                        socket_connected = true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine("Waiting for 5 seconds before re-attempting connection...");
                        await Task.Delay(5000);
                    }
                }
                while (!socket_connected);

                _continueReceiving = true;
                await Task.Factory.StartNew(async () => await receive(), CancellationToken.None);

                await sendText(
                    "{\"op\": \"subscribe\", \"args\": [\"trade:XBTUSD\", \"orderBookL2:XBTUSD\"]}",
                    true,
                    CancellationToken.None
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private async Task receive()
        {
            //var buffer = new ArraySegment<byte>(new byte[1000 * 32]);
            var buffer = new ArraySegment<byte>(new byte[2048]);
            WebSocketReceiveResult result;

            //string foo = "{\"table\":\"trade\",\"action\":\"insert\",\"data\":[{\"timestamp\":\"2022-02-12T03:06:03.953Z\",\"symbol\":\"XBTUSD\",\"side\":\"Sell\",\"size\":15000,\"price\":42435.5,\"tickDirection\":\"ZeroMinusTick\",\"trdMatchID\":\"bc0e03b3-4e1a-4879-6c90-83a3f9b02505\",\"grossValue\":35347800,\"homeNotional\":0.353478,\"foreignNotional\":15000}]}";
            //byte[] fooBytes = System.Text.Encoding.UTF8.GetBytes(foo);

            do
            {
                using (var ms = new MemoryStream())
                {
                    try
                    {
                        do
                        {
                            result = await _socket.ReceiveAsync(
                                buffer,
                                CancellationToken.None
                            );
                            ms.Write(buffer.Array, buffer.Offset, result.Count);

                            //ms.Write(fooBytes, 0, fooBytes.Length);
                            //result = new WebSocketReceiveResult(1, WebSocketMessageType.Text, true);
                        }
                        while (!result.EndOfMessage);

                        if (result.MessageType != WebSocketMessageType.Close)
                        {
                            if (result.MessageType == WebSocketMessageType.Text)
                            {
                                // Commenting out the following, which is responsible
                                // for converting the byte array to string, can
                                // actually increase CPU usage.
                                await processTextMessage(ms);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Close received.");
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        break;
                    }
                }
            }
            while (_continueReceiving);

            _continueReceiving = false;
        }

        private async Task sendText(string message, bool endOfMessage, CancellationToken cancellationToken)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            ArraySegment<byte> byte_array = new ArraySegment<byte>(buffer);
            await _socket.SendAsync(byte_array, WebSocketMessageType.Text, endOfMessage, cancellationToken);
        }

        private async Task processTextMessage(MemoryStream ms)
        {
            ms.Seek(0, SeekOrigin.Begin);

            using (var reader = new StreamReader(ms, Encoding.UTF8))
            {
                var message = await reader.ReadToEndAsync();
                Console.WriteLine(message);
            }
        }

        private async Task stopReceiving()
        {
            while (true)
            {
                await Task.Delay(1000);
                if (!_continueReceiving)
                    break;
            }

            return;
        }
    }
}
