using System; using System.Net; using System.Net.Sockets; using System.Text;
using System.Collections; using System.Collections.Generic; using UnityEngine;
namespace NetworkAPI {
    public class NetworkComm {
        public delegate void MsgHandler(string message);
        public event MsgHandler MsgReceived;
        public void sendMessage(String message) {
            Socket mcastSocket = null;
            try {
                mcastSocket = new Socket(AddressFamily.InterNetwork,
                               SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("230.0.0.1"), 11000);
                mcastSocket.SendTo(ASCIIEncoding.ASCII.GetBytes(message), endPoint);
            } catch (Exception e) { Debug.Log("\n" + e.ToString()); }
            mcastSocket.Close();
        }
        public void ReceiveMessages() {
            Socket mcastSocket = null;
            try {
                mcastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                EndPoint localEP = (EndPoint)new IPEndPoint(IPAddress.Any, 11000);
                mcastSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                mcastSocket.Bind(localEP);
                MulticastOption mcastOption = new MulticastOption(IPAddress.Parse("230.0.0.1"), IPAddress.Any);
                mcastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, mcastOption);
                byte[] bytes = new Byte[1000];
                IPEndPoint groupEP = new IPEndPoint(IPAddress.Parse("230.0.0.1"), 11000);
                EndPoint remoteEP = (EndPoint)new IPEndPoint(IPAddress.Any, 0);

                while (true) {
                    mcastSocket.ReceiveFrom(bytes, ref remoteEP);
                    String message = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                    Debug.Log(message);
                    MsgReceived(message);                   
                }
                mcastSocket.Close();
            } catch (Exception e) { Debug.Log("\n" + e.ToString()); } } } }
