using System.Text.Json;

namespace WordChain.Common
{
    // 1. Định nghĩa các loại hành động/gói tin giao tiếp giữa Client và Server
    public enum PacketType
    {
        Connect,    // Client gửi nickname lên Server khi mới vào game
        ConnectOK,  // Server xác nhận kết nối thành công và phản hồi lại
        Chat,       // Tin nhắn chat hoặc từ ngữ dùng để nối chữ
        Disconnect  // Báo ngắt kết nối
    }

    // 2. Cấu trúc chuẩn của một gói tin khi truyền qua mạng
    public class Packet
    {
        // Loại gói tin (thuộc enum PacketType ở trên)
        public PacketType Type { get; set; }

        // Nội dung chi tiết của gói tin (chuỗi text thường hoặc chuỗi JSON khác)
        public string Payload { get; set; } = ""; 

        // Hàm chuyển đổi đối tượng Packet thành chuỗi JSON để gửi đi (Serialize)
        public string ToJson() => JsonSerializer.Serialize(this);

        // Hàm tĩnh đọc chuỗi JSON nhận được và chuyển ngược lại thành đối tượng Packet (Deserialize)
        public static Packet? FromJson(string json) => JsonSerializer.Deserialize<Packet>(json);
    }

    // 3. Cấu trúc lưu trữ thông tin của một người chơi (nếu cần dùng ở các tính năng sau)
    public class PlayerInfo
    {
        public string Id { get; set; } = "";
        public string Nickname { get; set; } = "";
    }
}