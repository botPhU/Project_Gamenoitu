using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text.Json;
using WordChain.Common;

namespace WordChain.Server;

internal sealed class ConnectedClient
{
    public string Id { get; init; } = string.Empty;
    public TcpClient Tcp { get; init; } = null!;
    public StreamWriter Writer { get; init; } = null!;
    public PlayerInfo Info { get; set; } = new();
    public string? RoomId { get; set; }
}

internal sealed class GameRoom
{
    public string RoomId { get; init; } = string.Empty;
    public string HostClientId { get; set; } = string.Empty;
    public string HostNickname { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
    public int MaxPlayers { get; set; } = 4;
    public int SecondsPerTurn { get; set; } = 20;
    public bool IsPlaying { get; set; }
    public List<string> PlayerClientIds { get; } = [];
    public GameSession? Session { get; set; }
    public object SyncRoot { get; } = new();
}

internal sealed class GameSession
{
    public string CurrentWord { get; set; } = string.Empty;
    public string CurrentTurnClientId { get; set; } = string.Empty;
    public HashSet<string> UsedWords { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, int> Scores { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> WordHistory { get; } = [];
    public CancellationTokenSource? TurnTimerCts { get; set; }
}

internal static class ServerState
{
    public static readonly ConcurrentDictionary<string, ConnectedClient> Clients = new();
    public static readonly ConcurrentDictionary<string, GameRoom> Rooms = new();
    private static readonly Random Random = new();
    private const string RoomCodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string GenerateRoomCode()
    {
        for (int attempt = 0; attempt < 100; attempt++)
        {
            char[] code = new char[4];
            for (int i = 0; i < code.Length; i++)
            {
                code[i] = RoomCodeChars[Random.Next(RoomCodeChars.Length)];
            }

            string roomId = new(code);
            if (!Rooms.ContainsKey(roomId))
            {
                return roomId;
            }
        }

        return Guid.NewGuid().ToString("N")[..4].ToUpperInvariant();
    }

    public static RoomInfo ToRoomInfo(GameRoom room)
    {
        lock (room.SyncRoot)
        {
            var players = room.PlayerClientIds
                .Select(id => Clients.TryGetValue(id, out ConnectedClient? c) ? c : null)
                .Where(c => c is not null)
                .Select(c => c!.Info)
                .ToList();

            return new RoomInfo
            {
                RoomId = room.RoomId,
                HostNickname = room.HostNickname,
                Players = players,
                MaxPlayers = room.MaxPlayers,
                IsPlaying = room.IsPlaying,
                IsPrivate = room.IsPrivate,
                RoomName = room.RoomName,
                SecondsPerTurn = room.SecondsPerTurn
            };
        }
    }

    public static List<RoomInfo> GetPublicRoomList()
    {
        return Rooms.Values
            .Where(r => !r.IsPrivate && !r.IsPlaying)
            .Select(ToRoomInfo)
            .OrderByDescending(r => r.CurrentPlayers)
            .ToList();
    }
}

internal static class NetworkHelper
{
    public static async Task SendAsync(ConnectedClient client, Packet packet)
    {
        try
        {
            await client.Writer.WriteLineAsync(packet.ToJson());
        }
        catch
        {
            // Client đã ngắt kết nối.
        }
    }

    public static async Task BroadcastToRoomAsync(
        GameRoom room,
        Packet packet,
        string? excludeClientId = null)
    {
        List<ConnectedClient> targets;
        lock (room.SyncRoot)
        {
            targets = room.PlayerClientIds
                .Where(id => id != excludeClientId)
                .Select(id => ServerState.Clients.TryGetValue(id, out ConnectedClient? c) ? c : null)
                .Where(c => c is not null)
                .Select(c => c!)
                .ToList();
        }

        foreach (ConnectedClient client in targets)
        {
            await SendAsync(client, packet);
        }
    }

    public static async Task BroadcastRoomListToAllAsync()
    {
        string payload = JsonSerializer.Serialize(ServerState.GetPublicRoomList());
        var packet = new Packet { Type = PacketType.RoomList, Payload = payload };

        foreach (ConnectedClient client in ServerState.Clients.Values)
        {
            await SendAsync(client, packet);
        }
    }

    public static async Task BroadcastRoomUpdateAsync(GameRoom room)
    {
        string payload = JsonSerializer.Serialize(ServerState.ToRoomInfo(room));
        var packet = new Packet { Type = PacketType.UpdateRoom, Payload = payload };

        await BroadcastToRoomAsync(room, packet);

        IEnumerable<string> nicknames;
        lock (room.SyncRoot)
        {
            nicknames = room.PlayerClientIds
                .Select(id => ServerState.Clients.TryGetValue(id, out ConnectedClient? c) ? c.Info.Nickname : null)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n!)
                .ToList();
        }

        string playerNames = string.Join(',', nicknames);

        var updatePlayers = new Packet
        {
            Type = PacketType.UpdatePlayers,
            Payload = playerNames
        };

        await BroadcastToRoomAsync(room, updatePlayers);
    }
}

internal static class RoomService
{
    public static async Task HandleCreateRoomAsync(ConnectedClient client, string payload)
    {
        if (!string.IsNullOrWhiteSpace(client.RoomId))
        {
            await NetworkHelper.SendAsync(client, new Packet
            {
                Type = PacketType.JoinRoomFail,
                Payload = "Bạn đang ở trong một phòng khác."
            });
            return;
        }

        CreateRoomRequest request;
        try
        {
            request = string.IsNullOrWhiteSpace(payload)
                ? new CreateRoomRequest()
                : JsonSerializer.Deserialize<CreateRoomRequest>(payload) ?? new CreateRoomRequest();
        }
        catch
        {
            request = new CreateRoomRequest();
        }

        string roomName = string.IsNullOrWhiteSpace(request.RoomName)
            ? $"Phòng của {client.Info.Nickname}"
            : request.RoomName.Trim();

        var room = new GameRoom
        {
            RoomId = ServerState.GenerateRoomCode(),
            HostClientId = client.Id,
            HostNickname = client.Info.Nickname,
            RoomName = roomName,
            Password = request.IsPrivate ? request.Password.Trim() : string.Empty,
            IsPrivate = request.IsPrivate,
            MaxPlayers = Math.Clamp(request.MaxPlayers, 2, 8),
            SecondsPerTurn = Math.Clamp(request.SecondsPerTurn, 10, 60)
        };

        lock (room.SyncRoot)
        {
            room.PlayerClientIds.Add(client.Id);
        }

        client.RoomId = room.RoomId;
        ServerState.Rooms[room.RoomId] = room;

        RoomInfo roomInfo = ServerState.ToRoomInfo(room);
        await NetworkHelper.SendAsync(client, new Packet
        {
            Type = PacketType.CreateRoomOK,
            Payload = JsonSerializer.Serialize(roomInfo)
        });

        await NetworkHelper.BroadcastRoomUpdateAsync(room);
        await NetworkHelper.BroadcastRoomListToAllAsync();
    }

    public static async Task HandleJoinRoomAsync(ConnectedClient client, string payload)
    {
        JoinRoomRequest? request;
        try
        {
            request = JsonSerializer.Deserialize<JoinRoomRequest>(payload);
        }
        catch
        {
            request = null;
        }

        string roomId = request?.RoomId?.Trim().ToUpperInvariant() ?? payload.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(roomId))
        {
            await NetworkHelper.SendAsync(client, new Packet
            {
                Type = PacketType.JoinRoomFail,
                Payload = "Mã phòng không hợp lệ."
            });
            return;
        }

        if (!string.IsNullOrWhiteSpace(client.RoomId))
        {
            await NetworkHelper.SendAsync(client, new Packet
            {
                Type = PacketType.JoinRoomFail,
                Payload = "Bạn đang ở trong một phòng khác."
            });
            return;
        }

        if (!ServerState.Rooms.TryGetValue(roomId, out GameRoom? room))
        {
            await NetworkHelper.SendAsync(client, new Packet
            {
                Type = PacketType.JoinRoomFail,
                Payload = "Không tìm thấy phòng này."
            });
            return;
        }

        // Tách validation ra khỏi lock để tránh deadlock khi gọi async
        string? joinFailReason = null;

        lock (room.SyncRoot)
        {
            if (room.IsPlaying)
            {
                joinFailReason = "Phòng đang trong trận đấu.";
            }
            else if (room.PlayerClientIds.Count >= room.MaxPlayers)
            {
                joinFailReason = "Phòng đã đủ người.";
            }
            else if (room.IsPrivate &&
                !string.Equals(room.Password, request?.Password?.Trim() ?? string.Empty, StringComparison.Ordinal))
            {
                joinFailReason = "Mật khẩu phòng không đúng.";
            }
            else
            {
                room.PlayerClientIds.Add(client.Id);
            }
        }

        // Gửi thông báo lỗi bên ngoài lock — await an toàn
        if (joinFailReason is not null)
        {
            await NetworkHelper.SendAsync(client, new Packet
            {
                Type = PacketType.JoinRoomFail,
                Payload = joinFailReason
            });
            return;
        }

        client.RoomId = room.RoomId;

        RoomInfo roomInfo = ServerState.ToRoomInfo(room);
        await NetworkHelper.SendAsync(client, new Packet
        {
            Type = PacketType.JoinRoomOK,
            Payload = JsonSerializer.Serialize(roomInfo)
        });

        await NetworkHelper.BroadcastRoomUpdateAsync(room);
        await NetworkHelper.BroadcastRoomListToAllAsync();
    }

    public static async Task HandleQuickJoinAsync(ConnectedClient client)
    {
        if (!string.IsNullOrWhiteSpace(client.RoomId))
        {
            await NetworkHelper.SendAsync(client, new Packet
            {
                Type = PacketType.QuickJoinFail,
                Payload = "Bạn đang ở trong một phòng khác."
            });
            return;
        }

        GameRoom? room = ServerState.Rooms.Values
            .Where(r => !r.IsPrivate && !r.IsPlaying)
            .FirstOrDefault(r =>
            {
                lock (r.SyncRoot)
                {
                    return r.PlayerClientIds.Count < r.MaxPlayers;
                }
            });

        if (room is null)
        {
            await NetworkHelper.SendAsync(client, new Packet
            {
                Type = PacketType.QuickJoinFail,
                Payload = "Hiện chưa có phòng công khai nào còn chỗ."
            });
            return;
        }

        var request = new JoinRoomRequest { RoomId = room.RoomId };
        await HandleJoinRoomAsync(client, JsonSerializer.Serialize(request));
    }

    public static async Task HandleLeaveRoomAsync(ConnectedClient client)
    {
        if (string.IsNullOrWhiteSpace(client.RoomId))
        {
            return;
        }

        if (ServerState.Rooms.TryGetValue(client.RoomId, out GameRoom? room))
        {
            await RemovePlayerFromRoomAsync(room, client.Id);
        }

        client.RoomId = null;
    }

    public static async Task RemovePlayerFromRoomAsync(GameRoom room, string clientId)
    {
        bool wasHost;
        bool roomEmpty;
        bool wasPlaying;

        lock (room.SyncRoot)
        {
            wasPlaying = room.IsPlaying;
            wasHost = room.HostClientId == clientId;
            room.PlayerClientIds.Remove(clientId);

            if (ServerState.Clients.TryGetValue(clientId, out ConnectedClient? c))
            {
                c.RoomId = null;
            }

            roomEmpty = room.PlayerClientIds.Count == 0;

            if (!roomEmpty && wasHost)
            {
                room.HostClientId = room.PlayerClientIds[0];
                if (ServerState.Clients.TryGetValue(room.HostClientId, out ConnectedClient? newHost))
                {
                    room.HostNickname = newHost.Info.Nickname;
                }
            }
        }

        if (wasPlaying && room.Session is not null)
        {
            await GameService.EndGameAsync(room, clientId, "Người chơi đã rời phòng.");
            return;
        }

        if (roomEmpty)
        {
            room.Session?.TurnTimerCts?.Cancel();
            ServerState.Rooms.TryRemove(room.RoomId, out _);
        }
        else
        {
            await NetworkHelper.BroadcastRoomUpdateAsync(room);
        }

        await NetworkHelper.BroadcastRoomListToAllAsync();
    }
}

internal static class GameService
{
    public static async Task HandleStartGameRequestAsync(ConnectedClient client, string roomCode)
    {
        string roomId = roomCode.Trim().ToUpperInvariant();
        if (!ServerState.Rooms.TryGetValue(roomId, out GameRoom? room))
        {
            return;
        }

        lock (room.SyncRoot)
        {
            if (room.HostClientId != client.Id)
            {
                return;
            }

            if (room.IsPlaying)
            {
                return;
            }

            if (room.PlayerClientIds.Count < 2)
            {
                return;
            }
        }

        await StartGameAsync(room);
    }

    public static async Task StartGameAsync(GameRoom room)
    {
        string startWord = VietnameseDictionary.LayTuNgauNhien();
        string firstPlayerId;

        lock (room.SyncRoot)
        {
            room.IsPlaying = true;
            firstPlayerId = room.PlayerClientIds[0];

            room.Session = new GameSession
            {
                CurrentWord = startWord,
                CurrentTurnClientId = firstPlayerId
            };

            foreach (string playerId in room.PlayerClientIds)
            {
                if (ServerState.Clients.TryGetValue(playerId, out ConnectedClient? c))
                {
                    room.Session.Scores[c.Info.Nickname] = 0;
                }
            }

            room.Session.UsedWords.Add(VietnameseDictionary.ChuanHoaCumTu(startWord));
            room.Session.WordHistory.Add($"Hệ thống: {startWord}");
        }

        ConnectedClient? firstPlayer = ServerState.Clients.TryGetValue(firstPlayerId, out ConnectedClient? fp)
            ? fp
            : null;

        IEnumerable<string> playerNicknames;
        lock (room.SyncRoot)
        {
            playerNicknames = room.PlayerClientIds
                .Select(id => ServerState.Clients.TryGetValue(id, out ConnectedClient? c) ? c.Info.Nickname : null)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n!)
                .ToList();
        }

        string playerList = string.Join(',', playerNicknames);

        string gameStartPayload =
            $"{startWord}|{firstPlayer?.Info.Nickname ?? "???"}|{room.SecondsPerTurn}|{playerList}";

        var gameStartPacket = new Packet
        {
            Type = PacketType.GameStart,
            Payload = gameStartPayload
        };

        await NetworkHelper.BroadcastToRoomAsync(room, gameStartPacket);
        await NetworkHelper.BroadcastRoomListToAllAsync();
        await StartTurnTimerAsync(room);
    }

    public static async Task HandleSubmitWordAsync(ConnectedClient client, string payload)
    {
        string[] parts = payload.Split('|', 2, StringSplitOptions.TrimEntries);
        string roomId = parts.Length > 0 ? parts[0].ToUpperInvariant() : string.Empty;
        string word = parts.Length > 1 ? parts[1] : string.Empty;

        if (!ServerState.Rooms.TryGetValue(roomId, out GameRoom? room) ||
            room.Session is null ||
            !room.IsPlaying)
        {
            return;
        }

        GameSession session = room.Session;

        lock (room.SyncRoot)
        {
            if (session.CurrentTurnClientId != client.Id)
            {
                return;
            }
        }

        if (!VietnameseDictionary.LaDungHaiTu(word))
        {
            await BroadcastWordResultAsync(room, client, false, "Chỉ được nhập cụm đúng 2 từ.");
            await EndGameAsync(room, client.Id, "Nhập từ không hợp lệ.");
            return;
        }

        (bool hopLe, string thongBao) = VietnameseDictionary.KiemTraTuMoi(
            session.CurrentWord, word, session.UsedWords);

        if (!hopLe)
        {
            if (thongBao.Contains("đã được dùng"))
            {
                await NetworkHelper.BroadcastToRoomAsync(room, new Packet
                {
                    Type = PacketType.DuplicateWord,
                    Payload = client.Info.Nickname
                });
            }

            await BroadcastWordResultAsync(room, client, false, thongBao);
            await EndGameAsync(room, client.Id, thongBao);
            return;
        }

        string chuanHoa = VietnameseDictionary.ChuanHoaCumTu(word);

        lock (room.SyncRoot)
        {
            session.UsedWords.Add(chuanHoa);
            session.CurrentWord = chuanHoa;
            session.Scores[client.Info.Nickname] = session.Scores.GetValueOrDefault(client.Info.Nickname) + 1;
            session.WordHistory.Add($"{client.Info.Nickname}: {chuanHoa}");

            int currentIndex = room.PlayerClientIds.IndexOf(client.Id);
            int nextIndex = (currentIndex + 1) % room.PlayerClientIds.Count;
            session.CurrentTurnClientId = room.PlayerClientIds[nextIndex];
        }

        await BroadcastWordResultAsync(room, client, true, chuanHoa);
        await BroadcastNextTurnAsync(room);
        await StartTurnTimerAsync(room);
    }

    public static async Task HandleChatAsync(ConnectedClient client, string payload)
    {
        if (string.IsNullOrWhiteSpace(client.RoomId))
        {
            return;
        }

        if (!ServerState.Rooms.TryGetValue(client.RoomId, out GameRoom? room))
        {
            return;
        }

        string message = payload;
        if (payload.Contains('|'))
        {
            string[] parts = payload.Split('|', 2, StringSplitOptions.TrimEntries);
            message = parts.Length > 1 ? parts[1] : payload;
        }

        var packet = new Packet
        {
            Type = PacketType.Chat,
            Payload = $"{client.Info.Nickname}|{message}"
        };

        await NetworkHelper.BroadcastToRoomAsync(room, packet);
    }

    private static async Task BroadcastWordResultAsync(
        GameRoom room, ConnectedClient client, bool success, string detail)
    {
        string resultPayload = success
            ? $"OK|{client.Info.Nickname}|{detail}"
            : $"FAIL|{detail}";

        await NetworkHelper.BroadcastToRoomAsync(room, new Packet
        {
            Type = PacketType.WordResult,
            Payload = resultPayload
        });
    }

    private static async Task BroadcastNextTurnAsync(GameRoom room)
    {
        if (room.Session is null)
        {
            return;
        }

        ConnectedClient? current = ServerState.Clients.TryGetValue(
            room.Session.CurrentTurnClientId, out ConnectedClient? c) ? c : null;

        if (current is null)
        {
            return;
        }

        string payload = $"{current.Info.Nickname}|{room.SecondsPerTurn}|{room.Session.CurrentWord}";
        await NetworkHelper.BroadcastToRoomAsync(room, new Packet
        {
            Type = PacketType.NextTurn,
            Payload = payload
        });
    }

    public static async Task StartTurnTimerAsync(GameRoom room)
    {
        if (room.Session is null)
        {
            return;
        }

        room.Session.TurnTimerCts?.Cancel();
        room.Session.TurnTimerCts?.Dispose();

        var cts = new CancellationTokenSource();
        room.Session.TurnTimerCts = cts;
        string turnClientId = room.Session.CurrentTurnClientId;
        int seconds = room.SecondsPerTurn;

        _ = Task.Run(async () =>
        {
            try
            {
                for (int i = seconds; i > 0 && !cts.Token.IsCancellationRequested; i--)
                {
                    if (ServerState.Clients.TryGetValue(turnClientId, out ConnectedClient? player))
                    {
                        await NetworkHelper.BroadcastToRoomAsync(room, new Packet
                        {
                            Type = PacketType.NextTurn,
                            Payload = $"{player.Info.Nickname}|{i}|{room.Session!.CurrentWord}"
                        }, excludeClientId: null);
                    }

                    await Task.Delay(1000, cts.Token);
                }

                if (!cts.Token.IsCancellationRequested)
                {
                    if (ServerState.Clients.TryGetValue(turnClientId, out ConnectedClient? timedOut))
                    {
                        await NetworkHelper.BroadcastToRoomAsync(room, new Packet
                        {
                            Type = PacketType.Timeout,
                            Payload = timedOut.Info.Nickname
                        });
                    }

                    await EndGameAsync(room, turnClientId, "Hết giờ.");
                }
            }
            catch (TaskCanceledException)
            {
                // Lượt mới đã bắt đầu.
            }
        });
    }

    public static async Task EndGameAsync(GameRoom room, string loserClientId, string reason)
    {
        GameSession? session;
        List<string> playerIds;

        lock (room.SyncRoot)
        {
            if (!room.IsPlaying)
            {
                return;
            }

            session = room.Session;
            room.IsPlaying = false;
            room.Session = null;
            session?.TurnTimerCts?.Cancel();
            playerIds = room.PlayerClientIds.ToList();
        }

        string loserNickname = ServerState.Clients.TryGetValue(loserClientId, out ConnectedClient? loser)
            ? loser.Info.Nickname
            : "???";

        // Lấy danh sách nickname của những người còn lại (không phải người thua)
        List<string> survivorNicknames = playerIds
            .Where(id => id != loserClientId)
            .Select(id => ServerState.Clients.TryGetValue(id, out ConnectedClient? c) ? c.Info.Nickname : null)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n!)
            .ToList();

        string winnerNickname;

        if (session is not null && survivorNicknames.Count > 0)
        {
            // Tìm người có điểm cao nhất trong số những người còn sống sót
            string topScorer = survivorNicknames
                .Select(nick => (Nickname: nick, Score: session.Scores.GetValueOrDefault(nick, 0)))
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Nickname)
                .Select(x => x.Nickname)
                .First();

            // Nếu không ai có điểm (ví dụ game kết thúc ngay lượt đầu),
            // vẫn chọn người đầu tiên trong danh sách sống sót làm winner
            winnerNickname = topScorer;
        }
        else
        {
            winnerNickname = survivorNicknames.FirstOrDefault() ?? "???";
        }

        var endResult = new GameEndResult
        {
            WinnerNickname = winnerNickname,
            LoserNickname = loserNickname,
            Reason = reason,
            Scores = session?.Scores
                .Select(kv => new ScoreEntry { Nickname = kv.Key, Score = kv.Value })
                .OrderByDescending(s => s.Score)
                .ToList() ?? []
        };

        await NetworkHelper.BroadcastToRoomAsync(room, new Packet
        {
            Type = PacketType.GameEnd,
            Payload = JsonSerializer.Serialize(endResult)
        });

        await NetworkHelper.BroadcastRoomUpdateAsync(room);
        await NetworkHelper.BroadcastRoomListToAllAsync();
    }
}
