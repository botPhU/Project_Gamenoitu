using System.Collections.Generic;
using System.Text.Json;

namespace WordChain.Common
{
    // =========================
    // Các loại gói tin
    // =========================
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

        QuickJoin,
        QuickJoinOK,
        QuickJoinFail,

        LeaveRoom,

        RoomList,
        RoomUpdate,

        // ===== Gameplay =====
        StartGame,         // Chủ phòng yêu cầu bắt đầu game
        GameStart,         // Server thông báo game bắt đầu

        SubmitWord,
        WordResult,
        WordDuplicate,

        NextTurn,
        TurnUpdate,

        TimeOut,

        PlayerListUpdate,

        GameOver
    }

    // =========================
    // Packet chung
    // =========================
    public class Packet
    {
        public PacketType Type { get; set; }

        public string Payload { get; set; } = "";

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        public static Packet? FromJson(string json)
        {
            return JsonSerializer.Deserialize<Packet>(json);
        }
    }

    // =========================
    // Thông tin người chơi
    // =========================
    public class PlayerInfo
    {
        public string Id { get; set; } = "";

        public string Nickname { get; set; } = "";

        public int Score { get; set; } = 0;
    }

    // =========================
    // Thông tin phòng
    // =========================
    public class RoomInfo
    {
        // Mã phòng
        public string RoomId { get; set; } = "";

        // Chủ phòng
        public string HostId { get; set; } = "";

        public string HostNickname { get; set; } = "";

        // Danh sách người chơi
        public List<PlayerInfo> Players { get; set; } = new();

        // Số người hiện tại
        public int CurrentPlayers
        {
            get { return Players.Count; }
        }

        // Số người tối đa
        public int MaxPlayers { get; set; } = 4;

        // Trạng thái game
        public bool IsPlaying { get; set; } = false;

        // Vị trí người đang tới lượt
        public int CurrentTurnIndex { get; set; } = 0;
    }

    // =========================
    // Tạo phòng
    // =========================
    public class CreateRoomResponse
    {
        public string RoomId { get; set; } = "";
    }

    // =========================
    // Tham gia phòng
    // =========================
    public class JoinRoomRequest
    {
        public string RoomId { get; set; } = "";
    }

    public class QuickJoinResponse
    {
        public string RoomId { get; set; } = "";
    }

    public class LeaveRoomRequest
    {
        public string RoomId { get; set; } = "";
    }

    // =========================
    // Cập nhật phòng
    // =========================
    public class RoomUpdateInfo
    {
        public RoomInfo Room { get; set; } = new();
    }

    // =========================
    // Gameplay
    // =========================
    public class SubmitWordRequest
    {
        public string Word { get; set; } = "";
    }

    public class WordResultResponse
    {
        public bool IsValid { get; set; }

        public string Message { get; set; } = "";

        public int ScoreAwarded { get; set; } = 0;
    }

    public class TurnUpdateInfo
    {
        public string PlayerId { get; set; } = "";

        public string Nickname { get; set; } = "";

        public int RemainingSeconds { get; set; }
    }

    public class TimeOutInfo
    {
        public string PlayerId { get; set; } = "";

        public string Nickname { get; set; } = "";
    }

    public class GameOverInfo
    {
        public string WinnerId { get; set; } = "";

        public string WinnerNickname { get; set; } = "";

        public int WinnerScore { get; set; }
    }
}

