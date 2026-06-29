using System.Net;
using System.Net.Sockets;
using System.IO;
using WordChain.Common;

namespace WordChain.Server;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("=== GAME NỐI TỪ - SERVER ===");
        Console.WriteLine($"Từ điển: {VietnameseDictionary.SoLuongTu} cụm 2 từ tiếng Việt");

        var listener = new TcpListener(IPAddress.Any, 8888);
        listener.Start();
        Console.WriteLine("Server đang chạy trên cổng 8888. Chờ Client kết nối...");

        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            Console.WriteLine("Có Client mới kết nối!");
            _ = Task.Run(() => HandleClientAsync(client));
        }
    }

    static async Task HandleClientAsync(TcpClient tcpClient)
    {
        var stream = tcpClient.GetStream();
        var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
        var writer = new StreamWriter(stream, System.Text.Encoding.UTF8) { AutoFlush = true };

        string clientId = Guid.NewGuid().ToString();
        var client = new ConnectedClient
        {
            Id = clientId,
            Tcp = tcpClient,
            Writer = writer
        };

        ServerState.Clients[clientId] = client;

        try
        {
            while (true)
            {
                string? line = await reader.ReadLineAsync();
                if (line is null)
                {
                    break;
                }

                Packet? packet = Packet.FromJson(line);
                if (packet is null)
                {
                    continue;
                }

                Console.WriteLine($"[{client.Info.Nickname ?? "Unknown"}|{clientId[..8]}]: {packet.Type}");

                switch (packet.Type)
                {
                    case PacketType.Connect:
                        client.Info = new PlayerInfo
                        {
                            Id = clientId,
                            Nickname = string.IsNullOrWhiteSpace(packet.Payload)? "Unknown"
    :                       packet.Payload.Trim()
                        };
                        Console.WriteLine($"Người chơi [{client.Info.Nickname}] đã tham gia.");
                        await NetworkHelper.SendAsync(client, new Packet
                        {
                            Type = PacketType.ConnectOK,
                            Payload = "Kết nối thành công!"
                        });
                        await NetworkHelper.SendAsync(client, new Packet
                        {
                            Type = PacketType.RoomList,
                            Payload = System.Text.Json.JsonSerializer.Serialize(ServerState.GetPublicRoomList())
                        });
                        break;

                    case PacketType.CreateRoom:
                        await RoomService.HandleCreateRoomAsync(client, packet.Payload);
                        break;

                    case PacketType.JoinRoom:
                        await RoomService.HandleJoinRoomAsync(client, packet.Payload);
                        break;

                    case PacketType.QuickJoin:
                        await RoomService.HandleQuickJoinAsync(client);
                        break;

                    case PacketType.LeaveRoom:
                        await RoomService.HandleLeaveRoomAsync(client);
                        break;

                    case PacketType.StartGameRequest:
                        await GameService.HandleStartGameRequestAsync(client, packet.Payload);
                        break;

                    case PacketType.SubmitWord:
                        await GameService.HandleSubmitWordAsync(client, packet.Payload);
                        break;

                    case PacketType.Chat:
                        await GameService.HandleChatAsync(client, packet.Payload);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Client [{clientId}] lỗi: {ex.Message}");
        }
        finally
        {
            if (!string.IsNullOrWhiteSpace(client.RoomId) &&
                ServerState.Rooms.TryGetValue(client.RoomId, out GameRoom? room))
            {
                await RoomService.RemovePlayerFromRoomAsync(room, clientId);
            }

            ServerState.Clients.TryRemove(clientId, out _);
            tcpClient.Close();
            Console.WriteLine($"Client [{client.Info.Nickname}] đã rời khỏi server.");
        }
    }
}
