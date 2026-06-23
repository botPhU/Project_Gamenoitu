using System.Collections.Generic;
using System.Text.Json;

namespace WordChain.Common
{
    // Các loại gói tin giao tiếp giữa Client và Server
    public enum PacketType
    {
        // ===== Tuần 3 =====
        Connect,
        ConnectOK,
        Chat,
        Disconnect,

        // ===== Tuần 4 =====
        CreateRoom,      // Client yêu cầu tạo phòng
        CreateRoomOK,    // Server tạo phòng thành công
        JoinRoom,        // Client yêu cầu vào phòng
        JoinRoomOK,      // Vào phòng thành công
        JoinRoomFail,    // Vào phòng thất bại
        RoomList,        // Danh sách phòng
        SubmitWord,      // Gửi từ nối chữ
        WordResult,      // Kết quả kiểm tra từ
        GameStart,       // Bắt đầu game
        NextTurn         // Chuyển lượt chơi
    }

    // Cấu trúc Packet dùng để truyền dữ liệu
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

    // Thông tin người chơi
    public class PlayerInfo
    {
        public string Id { get; set; } = "";

        public string Nickname { get; set; } = "";
    }

    // Thông tin phòng chơi
    public class RoomInfo
    {
        // Mã phòng (VD: A1B2)
        public string RoomId { get; set; } = "";

        // Người tạo phòng
        public string HostNickname { get; set; } = "";

        // Danh sách người chơi trong phòng
        public List<PlayerInfo> Players { get; set; } = new();

        // Số người hiện tại
        public int CurrentPlayers
        {
            get { return Players.Count; }
        }

        // Số người tối đa
        public int MaxPlayers { get; set; } = 4;

        // Trạng thái phòng
        public bool IsPlaying { get; set; } = false;
    }

    // Payload khi tạo phòng thành công
    public class CreateRoomResponse
    {
        public string RoomId { get; set; } = "";
    }

    // Payload khi yêu cầu vào phòng
    public class JoinRoomRequest
    {
        public string RoomId { get; set; } = "";
    }

    // Payload gửi từ nối chữ
    public class SubmitWordRequest
    {
        public string Word { get; set; } = "";
    }

    // Payload trả kết quả từ đúng/sai
    public class WordResultResponse
    {
        public bool IsValid { get; set; }

        public string Message { get; set; } = "";
    }

    // Payload thông báo lượt chơi
    public class NextTurnInfo
    {
        public string Nickname { get; set; } = "";
    }
}
