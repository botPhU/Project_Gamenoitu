using System.Collections.Generic;
using System.Text.Json;

namespace WordChain.Common
{
    public enum PacketType
    {
        Connect,
        ConnectOK,
        Chat,
        Disconnect,

        CreateRoom,
        CreateRoomOK,
        JoinRoom,
        JoinRoomOK,
        JoinRoomFail,
        RoomList,
        QuickJoin,
        QuickJoinOK,
        QuickJoinFail,
        UpdateRoom,
        LeaveRoom,
        StartGameRequest,
        SubmitWord,
        UpdatePlayers,
        GameStart,
        NextTurn,
        Timeout,
        WordResult,
        DuplicateWord,
        GameEnd
    }

    public class Packet
    {
        public PacketType Type { get; set; }
        public string Payload { get; set; } = "";

        public string ToJson() => JsonSerializer.Serialize(this);

        public static Packet? FromJson(string json) =>
            JsonSerializer.Deserialize<Packet>(json);
    }

    public class PlayerInfo
    {
        public string Id { get; set; } = "";
        public string Nickname { get; set; } = "";
    }

    public class RoomInfo
    {
        public string RoomId { get; set; } = "";
        public string HostNickname { get; set; } = "";
        public List<PlayerInfo> Players { get; set; } = new();
        public int CurrentPlayers => Players.Count;
        public int MaxPlayers { get; set; } = 4;
        public bool IsPlaying { get; set; }
        public bool IsPrivate { get; set; }
        public string RoomName { get; set; } = "";
        public int SecondsPerTurn { get; set; } = 20;
    }

    public class CreateRoomRequest
    {
        public string RoomName { get; set; } = "";
        public int MaxPlayers { get; set; } = 4;
        public int SecondsPerTurn { get; set; } = 20;
        public bool IsPrivate { get; set; }
        public string Password { get; set; } = "";
    }

    public class JoinRoomRequest
    {
        public string RoomId { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class SubmitWordRequest
    {
        public string RoomId { get; set; } = "";
        public string Word { get; set; } = "";
    }

    public class WordResultResponse
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = "";
    }

    public class NextTurnInfo
    {
        public string Nickname { get; set; } = "";
        public int SecondsRemaining { get; set; }
        public string CurrentWord { get; set; } = "";
    }

    public class ScoreEntry
    {
        public string Nickname { get; set; } = "";
        public int Score { get; set; }
    }

    public class GameEndResult
    {
        public string WinnerNickname { get; set; } = "";
        public string LoserNickname { get; set; } = "";
        public List<ScoreEntry> Scores { get; set; } = new();
        public string Reason { get; set; } = "";
    }
}

