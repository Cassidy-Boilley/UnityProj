using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class WebSocketSender
{
    static Uri serverUri = new Uri("ws://localhost:52758/ws.ashx"); // Update with your WebSocket server URI
    static ClientWebSocket ws = new ClientWebSocket();

    static async Task Main()
    {
        try
        {
            await ws.ConnectAsync(serverUri, CancellationToken.None);
            Console.WriteLine("Connected to WebSocket server.");

            float x = -4.0f, y = 1.0f, z = -0.5f; // Initialize coordinates

            Console.WriteLine("Press 'W', 'A', 'S', 'D' to move the cube. Press 'Q' to quit.");

            bool running = true;
            while (running)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    switch (key)
                    {
                        case ConsoleKey.W:
                            y -= 0.5f; // Move up
                            break;
                        case ConsoleKey.S:
                            y += 0.5f; // Move down
                            break;
                        case ConsoleKey.A:
                            x -= 0.5f; // Move left
                            break;
                        case ConsoleKey.D:
                            x += 0.5f; // Move right
                            break;
                        case ConsoleKey.Q:
                            running = false; // Quit program
                            break;
                    }

                    string message = "ID=2;" + x + "," + y + "," + z;
                    byte[] bytes = Encoding.ASCII.GetBytes(message);
                    Console.WriteLine("Sent: " + message); // Fix the output format
                    await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                await Task.Delay(10); // Small delay to reduce CPU usage
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
            Console.WriteLine("WebSocket sender closed.");
        }
    }
}
