using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using WordChain.Common;
namespace WordChain.Server
{
    class Program
    {
        static ConcurrentDictionary<string, (TcpClient tcp, PlayerInfo info)> _clients = new();
        static ConcurrentDictionary<string, RoomInfo> _rooms = new();
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== GAME NỐI TỪ - SERVER ===");
            var listener = new TcpListener(IPAddress.Any, 8888);
            listener.Start();
            Console.WriteLine("Server đang chạy trên cổng 8888. Chờ Client kết nối...");
            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                Console.WriteLine(" Có Client mới kết nối!");
                _ = Task.Run(() => HandleClient(client));
            }
        }
        static string GenerateRoomCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var random = new Random();
            return new string(Enumerable.Range(0, 4)
            .Select(_ => chars[random.Next(chars.Length)]).ToArray());
        }
        static void HandleCreateRoom(string clientId, StreamWriter writer)
        {
            string roomCode = GenerateRoomCode();

            // Đảm bảo mã phòng không bị trùng
            while (_rooms.ContainsKey(roomCode))
            {
                roomCode = GenerateRoomCode();
            }

            var room = new RoomInfo
            {
                RoomId = roomCode,
                HostNickname = _clients[clientId].info.Nickname,
                CurrentPlayers = 1,
                MaxPlayers = 4
            };

            _rooms[roomCode] = room;
            Console.WriteLine($" Phòng [{roomCode}] được tạo bởi [{room.HostNickname}]");

            // Gửi mã phòng về cho Client
            var response = new Packet
            {
                Type = PacketType.CreateRoomOK,
                Payload = roomCode
            };
            writer.WriteLine(response.ToJson());
        }
        static void HandleJoinRoom(string clientId, string roomCode, StreamWriter writer)
        {
            if (!_rooms.ContainsKey(roomCode))
            {
                // Sai mã phòng
                writer.WriteLine(new Packet
                {
                    Type = PacketType.JoinRoomFail,
                    Payload = "Mã phòng không tồn tại!"
                }.ToJson());
                return;
            }

            var room = _rooms[roomCode];

            if (room.CurrentPlayers >= room.MaxPlayers)
            {
                // Phòng đầy
                writer.WriteLine(new Packet
                {
                    Type = PacketType.JoinRoomFail,
                    Payload = "Phòng đã đầy!"
                }.ToJson());
                return;
            }

            room.CurrentPlayers++;
            Console.WriteLine($" [{_clients[clientId].info.Nickname}] vào phòng [{roomCode}]");

            writer.WriteLine(new Packet
            {
                Type = PacketType.JoinRoomOK,
                Payload = roomCode
            }.ToJson());
        }

        static async Task HandleClient(TcpClient client) { }
    }
}