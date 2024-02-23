using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkAPI
{
    public class NetworkComm
    {
        public delegate void MsgHandler(string message);
        public event MsgHandler MsgReceived;

        public async Task SendMessageAsync(string message)
        {
            ClientWebSocket ws = null;
            try
            {
                ws = new ClientWebSocket();
                Uri serverUri = new Uri("ws://localhost:52758/ws.ashx");
                CancellationToken cancellationToken = CancellationToken.None;
                await ws.ConnectAsync(serverUri, cancellationToken);

                ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
                await ws.SendAsync(bytesToSend, WebSocketMessageType.Text, true, cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                if (ws != null)
                    ws.Dispose();
            }
        }

        public async Task ReceiveMessagesAsync()
        {
            ClientWebSocket ws = null;
            try
            {
                ws = new ClientWebSocket();
                Uri serverUri = new Uri("ws://localhost:52758/ws.ashx");
                CancellationToken cancellationToken = CancellationToken.None;
                await ws.ConnectAsync(serverUri, cancellationToken);

                var receiveBuffer = new byte[200];
                var iterationNo = 0;
                while (ws.State == WebSocketState.Open && iterationNo++ < 5)
                {
                    WebSocketReceiveResult result;
                    do
                    {
                        ArraySegment<byte> bytesReceived = new ArraySegment<byte>(receiveBuffer);
                        result = await ws.ReceiveAsync(bytesReceived, cancellationToken);
                        string message = Encoding.UTF8.GetString(receiveBuffer, 0, result.Count);
                        Console.WriteLine("Received: " + message);
                        MsgReceived?.Invoke(message);
                    } while (!result.EndOfMessage);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                if (ws != null)
                    ws.Dispose();
            }
        }
    }
}
