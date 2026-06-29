using System.Text.Json;
using WordChain.Common;

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

        string winnerNickname = playerIds
            .Where(id => id != loserClientId)
            .Select(id => ServerState.Clients.TryGetValue(id, out ConnectedClient? c) ? c.Info.Nickname : null)
            .FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)) ?? "???";

        if (session is not null)
        {
            string topScorer = session.Scores
                .OrderByDescending(kv => kv.Value)
                .ThenBy(kv => kv.Key)
                .Select(kv => kv.Key)
                .FirstOrDefault() ?? winnerNickname;

            if (session.Scores.GetValueOrDefault(topScorer) > 0)
            {
                winnerNickname = topScorer;
            }
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

