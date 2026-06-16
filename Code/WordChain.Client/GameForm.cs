using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WordChain.Common;

namespace WordChain.Client;

public partial class GameForm : Form
{
    // Giữ mã số gói tin cục bộ để phần client vẫn build được khi thư viện dùng chung chưa cập nhật.
    private static readonly PacketType _goiTinGuiTu = (PacketType)16;
    private static readonly PacketType _goiTinCapNhatNguoiChoi = (PacketType)17;
    private static readonly PacketType _goiTinBatDauTran = (PacketType)18;
    private static readonly PacketType _goiTinCapNhatLuot = (PacketType)19;
    private static readonly PacketType _goiTinHetGio = (PacketType)20;
    private static readonly PacketType _goiTinKetQuaTu = (PacketType)21;
    private static readonly PacketType _goiTinTrungTu = (PacketType)22;

    private StreamReader? _reader;
    private StreamWriter? _writer;
    private CancellationTokenSource? _boHuyNhanTin;
    private string _roomCode = string.Empty;

    public GameForm()
    {
        InitializeComponent();
        ApDungDuLieuMacDinh();
        FormClosing += GameForm_FormClosing;
    }

    public GameForm(string roomCode, StreamReader? reader, StreamWriter? writer)
        : this()
    {
        GanKetNoi(roomCode, reader, writer);
    }

    public event Action<string>? YeuCauMoManKetQua;

    // Gán dữ liệu khởi đầu để form mở lên luôn có nội dung rõ ràng và không bị trống.
    private void ApDungDuLieuMacDinh()
    {
        lblCurrentWord.Text = "Từ hiện tại: con mèo";
        lblTurn.Text = "Đến lượt: PlayerOne";
        lblTimer.Text = "Thời gian: 15s";
        lblMessage.Text = "Hãy nhập một từ mới bắt đầu bằng \"mèo\".";
    }

    // Cho phép form được gắn lại luồng mạng sau khi tạo để dễ nối với màn hình chờ.
    public void GanKetNoi(string roomCode, StreamReader? reader, StreamWriter? writer)
    {
        _roomCode = roomCode?.Trim() ?? string.Empty;
        _reader = reader;
        _writer = writer;

        if (!string.IsNullOrWhiteSpace(_roomCode))
        {
            Text = $"Ván chơi - {_roomCode}";
        }

        BatDauNhanTinTuServer();
    }

    public void CapNhatThongTinVanChoi(string tuHienTai, string denLuot, int soGiayConLai, string thongBao)
    {
        lblCurrentWord.Text = string.IsNullOrWhiteSpace(tuHienTai)
            ? "Từ hiện tại: ---"
            : $"Từ hiện tại: {tuHienTai}";
        lblTurn.Text = string.IsNullOrWhiteSpace(denLuot)
            ? "Đến lượt: ---"
            : $"Đến lượt: {denLuot}";
        lblTimer.Text = $"Thời gian: {Math.Max(0, soGiayConLai)}s";
        lblMessage.Text = string.IsNullOrWhiteSpace(thongBao)
            ? "Sẵn sàng cho lượt chơi tiếp theo."
            : thongBao;
    }

    public void CapNhatDanhSachNguoiChoi(IEnumerable<string> danhSachNguoiChoi)
    {
        lstPlayers.Items.Clear();

        foreach (string nguoiChoi in danhSachNguoiChoi)
        {
            if (!string.IsNullOrWhiteSpace(nguoiChoi))
            {
                lstPlayers.Items.Add(nguoiChoi.Trim());
            }
        }
    }

    public void CapNhatLichSuTu(IEnumerable<string> lichSuTu)
    {
        lstHistory.Items.Clear();

        foreach (string tu in lichSuTu)
        {
            if (!string.IsNullOrWhiteSpace(tu))
            {
                lstHistory.Items.Add(tu.Trim());
            }
        }
    }

    public void ThemTinNhanChat(string nguoiGui, string noiDung)
    {
        if (string.IsNullOrWhiteSpace(noiDung))
        {
            return;
        }

        string tenNguoiGui = string.IsNullOrWhiteSpace(nguoiGui) ? "Người chơi" : nguoiGui.Trim();
        lstChat.Items.Add($"{tenNguoiGui}: {noiDung.Trim()}");
        lstChat.TopIndex = lstChat.Items.Count - 1;
    }

    public string LayTuDangNhap()
    {
        return txtWordInput.Text.Trim();
    }

    public string LayTinNhanChatDangNhap()
    {
        return txtChatInput.Text.Trim();
    }

    public void XoaODeNhapTu()
    {
        txtWordInput.Clear();
        txtWordInput.Focus();
    }

    public void XoaOChat()
    {
        txtChatInput.Clear();
        txtChatInput.Focus();
    }

    // Khởi động vòng lặp nhận gói tin nếu form đã được gắn reader.
    public void BatDauNhanTinTuServer()
    {
        if (_reader is null)
        {
            return;
        }

        if (_boHuyNhanTin is not null)
        {
            return;
        }

        _boHuyNhanTin = new CancellationTokenSource();
        _ = Task.Run(() => ReceiveMessagesAsync(_boHuyNhanTin.Token));
    }

    // Vòng lặp nhận packet cho đúng vai trò của form game.
    private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && _reader is not null)
            {
                string? line = await _reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                Packet? packet = Packet.FromJson(line);
                if (packet is null)
                {
                    continue;
                }

                XuLyPacketTuServer(packet);
            }
        }
        catch (ObjectDisposedException)
        {
            // Form đã đóng nên không cần báo lỗi nữa.
        }
        catch (IOException)
        {
            HienThiThongBaoAnToan("Kết nối tới server đã bị ngắt trong lúc chơi.");
        }
        catch (InvalidOperationException)
        {
            HienThiThongBaoAnToan("Luồng nhận dữ liệu không còn khả dụng.");
        }
    }

    // Cho phép form nhận packet từ vòng lặp bên ngoài nếu sau này luồng đọc được tập trung ở Form1.
    public void XuLyPacketTuServer(Packet packet)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => XuLyPacketTuServer(packet)));
            return;
        }

        switch (packet.Type)
        {
            case var _ when packet.Type == _goiTinCapNhatNguoiChoi:
                lstPlayers.Items.Clear();
                foreach (string tenNguoiChoi in packet.Payload.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    lstPlayers.Items.Add(tenNguoiChoi.Trim());
                }
                break;

            case var _ when packet.Type == _goiTinBatDauTran:
                lblMessage.Text = "🎮 Game bắt đầu!";
                break;

            case var _ when packet.Type == _goiTinCapNhatLuot:
                CapNhatLuotVaThoiGian(packet.Payload);
                break;

            case var _ when packet.Type == _goiTinHetGio:
                lblMessage.Text = $"⏰ {packet.Payload} đã hết giờ!";
                break;

            case var _ when packet.Type == _goiTinKetQuaTu:
                XuLyKetQuaTu(packet.Payload);
                break;

            case var _ when packet.Type == _goiTinTrungTu:
                lblMessage.Text = "⚠️ Từ này đã được dùng!";
                break;

            case PacketType.Chat:
                XuLyChat(packet.Payload);
                break;
        }
    }

    // Tách riêng phần cập nhật lượt để chịu được cả payload đơn và payload kèm thời gian.
    private void CapNhatLuotVaThoiGian(string payload)
    {
        string[] parts = payload.Split('|', StringSplitOptions.TrimEntries);
        string tenNguoiChoi = parts.Length > 0 ? parts[0] : payload;

        lblTurn.Text = $"Đến lượt: {tenNguoiChoi}";

        if (parts.Length > 1 && int.TryParse(parts[1], out int soGiayConLai))
        {
            lblTimer.Text = $"Thời gian: {Math.Max(0, soGiayConLai)}s";
        }
    }

    // Cập nhật UI khi server trả về kết quả hợp lệ hoặc báo lỗi từ nối.
    private void XuLyKetQuaTu(string payload)
    {
        string[] parts = payload.Split('|', StringSplitOptions.TrimEntries);
        if (parts.Length < 2)
        {
            lblMessage.Text = "Không đọc được kết quả từ server.";
            return;
        }

        if (string.Equals(parts[0], "OK", StringComparison.OrdinalIgnoreCase))
        {
            string nguoiChoi = parts.Length > 1 ? parts[1] : "Người chơi";
            string tuMoi = parts.Length > 2 ? parts[2] : "---";

            lblCurrentWord.Text = $"Từ hiện tại: {tuMoi}";
            lstHistory.Items.Add($"{nguoiChoi}: {tuMoi}");
            lstHistory.TopIndex = lstHistory.Items.Count - 1;
            lblMessage.Text = "✅ Từ đã được ghi nhận.";
            XoaODeNhapTu();
            return;
        }

        lblMessage.Text = $"❌ {(parts.Length > 1 ? parts[1] : "Từ không hợp lệ.")}";
    }

    // Hỗ trợ gói chat dạng "người gửi|nội dung" để phần trò chuyện nhìn giống ứng dụng thật hơn.
    private void XuLyChat(string payload)
    {
        string[] parts = payload.Split('|', 2, StringSplitOptions.TrimEntries);
        if (parts.Length == 2)
        {
            ThemTinNhanChat(parts[0], parts[1]);
            return;
        }

        ThemTinNhanChat("Hệ thống", payload);
    }

    private async void btnSubmitWord_Click(object? sender, EventArgs e)
    {
        string word = txtWordInput.Text.Trim();
        if (string.IsNullOrEmpty(word))
        {
            lblMessage.Text = "Bạn hãy nhập một từ trước khi gửi nhé.";
            return;
        }

        if (_writer is null)
        {
            lblMessage.Text = "Chưa có kết nối server để gửi từ.";
            return;
        }

        var packet = new Packet
        {
            Type = _goiTinGuiTu,
            Payload = $"{_roomCode}|{word}"
        };

        await _writer.WriteLineAsync(packet.ToJson());
        txtWordInput.Clear();
        lblMessage.Text = "Đã gửi từ lên server, đang chờ phản hồi...";
    }

    private async void btnSendChat_Click(object? sender, EventArgs e)
    {
        string tinNhan = txtChatInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(tinNhan))
        {
            lblMessage.Text = "Bạn hãy nhập một tin nhắn ngắn rồi gửi nhé.";
            return;
        }

        if (_writer is null)
        {
            lblMessage.Text = "Chưa có kết nối server để gửi chat.";
            return;
        }

        var packet = new Packet
        {
            Type = PacketType.Chat,
            Payload = $"{_roomCode}|{tinNhan}"
        };

        await _writer.WriteLineAsync(packet.ToJson());
        XoaOChat();
    }

    // Dùng cho trường hợp server báo kết thúc trận và form cần mở màn hình kết quả sau này.
    public void ThongBaoMoManKetQua(string payload)
    {
        YeuCauMoManKetQua?.Invoke(payload);
    }

    private void HienThiThongBaoAnToan(string thongBao)
    {
        if (!IsHandleCreated)
        {
            return;
        }

        BeginInvoke(new Action(() => lblMessage.Text = thongBao));
    }

    private void GameForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _boHuyNhanTin?.Cancel();
        _boHuyNhanTin?.Dispose();
        _boHuyNhanTin = null;
    }
}
