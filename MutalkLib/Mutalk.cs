using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace MutalkLib
{
    public class Mutalk: IDisposable
    {
        private readonly string _topic;
        private readonly Socket _socket;
        private readonly IPEndPoint _endpoint;
        
        public event EventHandler<MessageEvent> OnMessage;

        public Mutalk(string topic, int port = 51268)
        {
            _topic = topic ?? throw new ArgumentException("Specify topic where messages will be sent");
            
            var address = IPAddress.Parse(GetMulticastIpByTopic(_topic));
            _endpoint = new IPEndPoint(address, port);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);            
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(address));   
        }

        public void ReceiveMessages(CancellationToken cancellationToken)
        {
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _socket.Bind(_endpoint);

            while (!cancellationToken.IsCancellationRequested)
            {
                var buffer = new byte[4096];
                var recevied = _socket.Receive(buffer, SocketFlags.None);
                var json = JObject.Parse(Encoding.UTF8.GetString(buffer, 0, recevied));
                
                OnMessage?.Invoke(this, new MessageEvent((string)json["t"], Convert.FromBase64String((string)json["m"])));
            }
        }

        public void SendMessage(byte[] messageBytes)
        {
            if (!_socket.Connected) _socket.Connect(_endpoint);

            if (messageBytes.Length == 0) throw new ArgumentException("Message must not be empty");

            var bytes = Encoding.UTF8.GetBytes($"{{\"t\":\"{_topic}\",\"m\":\"{Convert.ToBase64String(messageBytes)}\"}}");

            _socket.Send(bytes, SocketFlags.None);
        }

        public void Dispose()
        {
            _socket?.Close();
        }

        private string GetMulticastIpByTopic(string topic)
        {
            int crc32Hash = Math.Abs(new Crc32().ComputeHashFromString(topic));
            int[] multicastIp = new int[4] { 224, 0, 0, 0 };

            for (var i = 0; i < 3; i++)
            {
                multicastIp[i + 1] += crc32Hash % 1000 % 256;
                crc32Hash /= 1000;
            }

            return string.Join(".", multicastIp);
        }
    }
}
