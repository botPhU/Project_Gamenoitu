using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using WordChain.Common;

namespace WordChain.Server
{
    class Program
    {
        static ConcurrentDictionary<string, (TcpClient tcp, PlayerInfo info)> _clients = new();

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== GAME NỐI TỪ - SERVER ===");

            var listener = new TcpListener(IPAddress.Any, 8888);
            listener.Start();
            Console.WriteLine("✅ Server đang chạy trên cổng 8888. Chờ Client kết nối...");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                Console.WriteLine("🔌 Có Client mới kết nối!");
                _ = Task.Run(() => HandleClient(client));
            }
        }

        static async Task HandleClient(TcpClient client)
        {
            var stream = client.GetStream();

            var reader = new StreamReader(stream, System.Text.Encoding.UTF8);

            var writer = new StreamWriter(stream, System.Text.Encoding.UTF8)
            {
                AutoFlush = true
            };

            string clientId = Guid.NewGuid().ToString();
        }
    }
}