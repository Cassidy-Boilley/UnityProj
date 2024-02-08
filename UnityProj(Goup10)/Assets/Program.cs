using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Globalization;
using System.Threading;
using System.Collections.Generic; // For List<T>

class TestMulticastOptionSender
{
    static IPAddress mcastAddress;
    static int mcastPort;
    static Socket mcastSocket;
    static IPEndPoint endPoint;

    static void BroadcastMessage(string message)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(message);
        mcastSocket.SendTo(bytes, endPoint);
    }

    static void Main()
    {
        mcastAddress = IPAddress.Parse("230.0.0.1");
        mcastPort = 11000;

        try
        {
            mcastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            endPoint = new IPEndPoint(mcastAddress, mcastPort);

            float x = 1.0f, y = 1.0f, z = 1.0f; // Initial positions

            // Define a simple pattern of moves
            var moves = new List<Action>
            {
                () => z += 0.1f, // Move forward
                () => z -= 0.1f, // Move backward
                () => x -= 0.1f, // Move left
                () => x += 0.1f  // Move right
            };

            int currentMove = 0;

            Console.WriteLine("Starting multicast sender. Press Ctrl+C to terminate.");
            while (true)
            {
                // Apply the current move
                moves[currentMove]();

                // Prepare and send the message using String.Format for older C# versions
                string message = String.Format("ID=2;{0},{1},{2}", x.ToString(CultureInfo.InvariantCulture), y.ToString(CultureInfo.InvariantCulture), z.ToString(CultureInfo.InvariantCulture));
                BroadcastMessage(message);

                // Move to the next move in the pattern, looping back to the start if necessary
                currentMove = (currentMove + 1) % moves.Count;

                Thread.Sleep(2000); // Update every 2 seconds
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("\n" + e.ToString());
        }
        finally
        {
            mcastSocket.Close();
        }
    }
}