using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using WordChain.Common;

namespace WordChain.Server
{
    class Program
    {
        static ConcurrentDictionary<string, (TcpClient tcp, PlayerInfo info, StreamWriter writer)> _clients = new();
        static ConcurrentDictionary<string, RoomInfo> _rooms = new();
        static ConcurrentDictionary<string, string> _clientRooms = new();
        static ConcurrentDictionary<string, string> _lastWords = new();

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
            var writer = new StreamWriter(stream, System.Text.Encoding.UTF8) { AutoFlush = true };

            string clientId = Guid.NewGuid().ToString();

            try
            {
                while (true)
                {
                    string? line = await reader.ReadLineAsync();
                    if (line == null) break;

                    var packet = Packet.FromJson(line);
                    if (packet == null) continue;

                    Console.WriteLine($"Nhận từ [{clientId}]: {packet.Type} - {packet.Payload}");

                    switch (packet.Type)
                    {
                        case PacketType.Connect:
                            var player = new PlayerInfo { Id = clientId, Nickname = packet.Payload };
                            _clients[clientId] = (client, player, writer);
                            Console.WriteLine($"Người chơi [{player.Nickname}] đã tham gia.");
                            await writer.WriteLineAsync(new Packet
                            {
                                Type = PacketType.ConnectOK,
                                Payload = "Kết nối thành công!"
                            }.ToJson());
                            await BroadcastRoomList();
                            break;

                        case PacketType.CreateRoom:
                            await HandleCreateRoom(clientId, writer);
                            break;

                        case PacketType.JoinRoom:
                            await HandleJoinRoom(clientId, packet.Payload, writer);
                            break;

                        case PacketType.QuickJoin:
                            await HandleQuickJoin(clientId, writer);
                            break;

                        case PacketType.LeaveRoom:
                            await HandleLeaveRoom(clientId);
                            break;

                        case PacketType.SubmitWord:
                            await HandleSubmitWord(clientId, packet.Payload, writer);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client [{clientId}] ngắt kết nối: {ex.Message}");
            }
            finally
            {
                await HandleLeaveRoom(clientId);
                _clients.TryRemove(clientId, out _);
                client.Close();
                Console.WriteLine($"Client [{clientId}] đã rời khỏi server.");
            }
        }

        static string GenerateRoomCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var random = new Random();
            return new string(Enumerable.Range(0, 4)
                .Select(_ => chars[random.Next(chars.Length)]).ToArray());
        }

        static async Task HandleCreateRoom(string clientId, StreamWriter writer)
        {
            if (!_clients.ContainsKey(clientId))
                return;

            if (_clientRooms.ContainsKey(clientId))
                await RemoveClientFromRoom(clientId);

            string roomCode;
            do
            {
                roomCode = GenerateRoomCode();
            } while (_rooms.ContainsKey(roomCode));

            var player = _clients[clientId].info;
            var room = new RoomInfo
            {
                RoomId = roomCode,
                HostNickname = player.Nickname,
                MaxPlayers = 4
            };
            room.Players.Add(new PlayerInfo { Id = player.Id, Nickname = player.Nickname });

            _rooms[roomCode] = room;
            _clientRooms[clientId] = roomCode;

            Console.WriteLine($"Phòng [{roomCode}] được tạo bởi [{player.Nickname}]");

            await writer.WriteLineAsync(new Packet
            {
                Type = PacketType.CreateRoomOK,
                Payload = JsonSerializer.Serialize(room)
            }.ToJson());

            await BroadcastRoomList();
            await BroadcastRoomUpdate(roomCode);
        }

        static async Task HandleJoinRoom(string clientId, string roomCode, StreamWriter writer)
        {
            roomCode = roomCode.Trim().ToUpperInvariant();

            if (roomCode.Length != 4)
            {
                await writer.WriteLineAsync(new Packet
                {
                    Type = PacketType.JoinRoomFail,
                    Payload = "Mã phòng phải có đúng 4 ký tự!"
                }.ToJson());
                return;
            }

            if (!_rooms.TryGetValue(roomCode, out var room))
            {
                await writer.WriteLineAsync(new Packet
                {
                    Type = PacketType.JoinRoomFail,
                    Payload = "Mã phòng không tồn tại!"
                }.ToJson());
                return;
            }

            if (room.IsPlaying)
            {
                await writer.WriteLineAsync(new Packet
                {
                    Type = PacketType.JoinRoomFail,
                    Payload = "Phòng đang chơi, không thể tham gia!"
                }.ToJson());
                return;
            }

            if (room.CurrentPlayers >= room.MaxPlayers)
            {
                await writer.WriteLineAsync(new Packet
                {
                    Type = PacketType.JoinRoomFail,
                    Payload = "Phòng đã đầy!"
                }.ToJson());
                return;
            }

            if (_clientRooms.ContainsKey(clientId))
                await RemoveClientFromRoom(clientId);

            var player = _clients[clientId].info;
            if (!room.Players.Any(p => p.Id == player.Id))
            {
                room.Players.Add(new PlayerInfo { Id = player.Id, Nickname = player.Nickname });
            }

            _clientRooms[clientId] = roomCode;

            Console.WriteLine($"[{player.Nickname}] vào phòng [{roomCode}]");

            await writer.WriteLineAsync(new Packet
            {
                Type = PacketType.JoinRoomOK,
                Payload = JsonSerializer.Serialize(room)
            }.ToJson());

            await BroadcastRoomList();
            await BroadcastRoomUpdate(roomCode);
        }

        static async Task HandleQuickJoin(string clientId, StreamWriter writer)
        {
            var room = _rooms.Values
                .Where(r => !r.IsPlaying && r.CurrentPlayers < r.MaxPlayers)
                .OrderByDescending(r => r.CurrentPlayers)
                .ThenBy(r => r.RoomId)
                .FirstOrDefault();

            if (room is null)
            {
                await writer.WriteLineAsync(new Packet
                {
                    Type = PacketType.QuickJoinFail,
                    Payload = "Hiện chưa có phòng trống. Hãy tạo phòng mới nhé!"
                }.ToJson());
                return;
            }

            if (_clientRooms.ContainsKey(clientId))
                await RemoveClientFromRoom(clientId);

            var player = _clients[clientId].info;
            if (!room.Players.Any(p => p.Id == player.Id))
            {
                room.Players.Add(new PlayerInfo { Id = player.Id, Nickname = player.Nickname });
            }

            _clientRooms[clientId] = room.RoomId;

            Console.WriteLine($"[{player.Nickname}] tham gia nhanh vào phòng [{room.RoomId}]");

            await writer.WriteLineAsync(new Packet
            {
                Type = PacketType.QuickJoinOK,
                Payload = JsonSerializer.Serialize(room)
            }.ToJson());

            await BroadcastRoomList();
            await BroadcastRoomUpdate(room.RoomId);
        }

        static async Task HandleLeaveRoom(string clientId)
        {
            if (!_clientRooms.TryRemove(clientId, out string? roomCode))
                return;

            await RemoveClientFromRoom(clientId, roomCode);
        }

        static async Task RemoveClientFromRoom(string clientId, string? roomCode = null)
        {
            if (roomCode is null)
            {
                if (!_clientRooms.TryRemove(clientId, out roomCode))
                    return;
            }
            else
            {
                _clientRooms.TryRemove(clientId, out _);
            }

            if (!_rooms.TryGetValue(roomCode, out var room))
                return;

            if (_clients.TryGetValue(clientId, out var client))
            {
                room.Players.RemoveAll(p => p.Id == clientId);
                Console.WriteLine($"[{client.info.Nickname}] rời phòng [{roomCode}]");
            }

            if (room.Players.Count == 0)
            {
                _rooms.TryRemove(roomCode, out _);
                _lastWords.TryRemove(roomCode, out _);
                Console.WriteLine($"Phòng [{roomCode}] đã bị xóa vì không còn người chơi.");
            }

            await BroadcastRoomList();
            await BroadcastRoomUpdate(roomCode);
        }

        static ConcurrentDictionary<string, HashSet<string>> _usedWords = new();

        static async Task HandleSubmitWord(string clientId, string payload, StreamWriter writer)
        {
            if (!_clientRooms.TryGetValue(clientId, out string? roomCode))
                return;

            var parts = payload.Split('|');
            string newWord = parts.Length > 1 ? parts[1] : parts[0];

            newWord = newWord.Trim();

            if (string.IsNullOrWhiteSpace(newWord))
            {
                await writer.WriteLineAsync(new Packet
                {
                    Type = PacketType.WordResult,
                    Payload = "FAIL|Từ rỗng"
                }.ToJson());

                return;
            }

            if (newWord.Length > 30)
            {
                await writer.WriteLineAsync(new Packet
                {
                    Type = PacketType.WordResult,
                    Payload = "FAIL|Từ quá dài"
                }.ToJson());

                return;
            }

            if (IsWordUsed(roomCode, newWord))
            {
                await writer.WriteLineAsync(new Packet
                {
                    Type = PacketType.WordResult,
                    Payload = $"FAIL|{newWord} đã được dùng"
                }.ToJson());

                return;
            }

            string lastWord = _lastWords.GetValueOrDefault(roomCode, "");

            bool valid = IsValidWord(lastWord, newWord);

            if (valid)
            {
                _lastWords[roomCode] = newWord;

                AddUsedWord(roomCode, newWord);

                Console.WriteLine($"✅ Từ hợp lệ: {newWord}");
            }

            await writer.WriteLineAsync(new Packet
            {
                Type = PacketType.WordResult,
                Payload = valid ? $"OK|{newWord}" : $"FAIL|{newWord}"
            }.ToJson());
        }

        static async Task BroadcastRoomList()
        {
            var list = _rooms.Values.ToList();
            var packet = new Packet
            {
                Type = PacketType.RoomList,
                Payload = JsonSerializer.Serialize(list)
            };

            foreach (var (_, _, writer) in _clients.Values)
            {
                try
                {
                    await writer.WriteLineAsync(packet.ToJson());
                }
                catch
                {
                    // Client có thể đã ngắt kết nối
                }
            }
        }

        static async Task BroadcastRoomUpdate(string roomCode)
        {
            if (!_rooms.TryGetValue(roomCode, out var room))
                return;

            var packet = new Packet
            {
                Type = PacketType.RoomUpdate,
                Payload = JsonSerializer.Serialize(room)
            };

            foreach (var player in room.Players)
            {
                if (_clients.TryGetValue(player.Id, out var client))
                {
                    try
                    {
                        await client.writer.WriteLineAsync(packet.ToJson());
                    }
                    catch
                    {
                        // Client có thể đã ngắt kết nối
                    }
                }
            }
        }

        static bool IsValidWord(string lastWord, string newWord)
        {
            if (string.IsNullOrEmpty(lastWord)) return true;
            string lastSyllable = lastWord.Trim().Split(' ').Last().ToLower();
            string firstSyllable = newWord.Trim().Split(' ').First().ToLower();
            return lastSyllable == firstSyllable;
        }

        static bool IsWordUsed(string roomCode, string word)
        {
            word = word.Trim().ToLower();

            if (!_usedWords.ContainsKey(roomCode))
                return false;

            return _usedWords[roomCode].Contains(word);
        }

        static void AddUsedWord(string roomCode, string word)
        {
            word = word.Trim().ToLower();

            if (!_usedWords.ContainsKey(roomCode))
                _usedWords[roomCode] = new HashSet<string>();

            _usedWords[roomCode].Add(word);
        }
    }
}
