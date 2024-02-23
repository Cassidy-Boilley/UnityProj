using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class TestWebsocketReceiver
{
    private static Uri serverUri = new Uri("ws://localhost:52758/ws.ashx"); // Update with your WebSocket server URI
    private static ClientWebSocket ws = new ClientWebSocket();

    public static async Task Main(string[] args)
    {
        try
        {
            await ws.ConnectAsync(serverUri, CancellationToken.None);
            Console.WriteLine("Connected to WebSocket server.");

            var receiveBuffer = new byte[200];

            while (true)
            {
                WebSocketReceiveResult result;
                do
                {
                    ArraySegment<byte> bytesReceived = new ArraySegment<byte>(receiveBuffer);
                    result = await ws.ReceiveAsync(bytesReceived, CancellationToken.None);
                    string message = Encoding.UTF8.GetString(receiveBuffer, 0, result.Count);
                    Console.WriteLine("Received coordinates from WebSocket server: " + message);
                } while (!result.EndOfMessage);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("\n" + e.ToString());
        }
        finally
        {
            if (ws.State == WebSocketState.Open || ws.State == WebSocketState.CloseSent)
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            Console.WriteLine("WebSocket receiver closed.");
        }
    }
}
