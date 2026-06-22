using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
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
    private string _tenNguoiChoi = string.Empty;

    public GameForm()
    {
        InitializeComponent();
        ApDungDuLieuMacDinh();
        FormClosing += GameForm_FormClosing;
    }

    public GameForm(string roomCode, StreamWriter? writer, string tenNguoiChoi)
        : this()
    {
        _tenNguoiChoi = tenNguoiChoi;
        GanKetNoi(roomCode, writer);
    }

    public event Action<string>? YeuCauMoManKetQua;

    private void ApDungDuLieuMacDinh()
    {
        lblCurrentWord.Text = "Từ hiện tại: ---";
        lblTurn.Text = "Đến lượt: ---";
        lblTimer.Text = "Thời gian: ---";
        lblMessage.Text = "Đang chờ server bắt đầu ván chơi...";
    }

    public void GanKetNoi(string roomCode, StreamWriter? writer)
    {
        _roomCode = roomCode?.Trim() ?? string.Empty;
        _writer = writer;

        if (!string.IsNullOrWhiteSpace(_roomCode))
        {
            Text = $"Ván chơi - {_roomCode}";
        }
    }

    public void DatTenNguoiChoi(string ten) => _tenNguoiChoi = ten?.Trim() ?? string.Empty;

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

    public void XuLyPacketTuServer(Packet packet)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => XuLyPacketTuServer(packet)));
            return;
        }

        switch (packet.Type)
        {
            case PacketType.UpdatePlayers:
                CapNhatDanhSachNguoiChoi(packet.Payload.Split(',', StringSplitOptions.RemoveEmptyEntries));
                break;

            case PacketType.GameStart:
                XuLyBatDauTran(packet.Payload);
                break;

            case PacketType.NextTurn:
                CapNhatLuotVaThoiGian(packet.Payload);
                break;

            case PacketType.Timeout:
                lblMessage.Text = $"⏰ {packet.Payload} đã hết giờ!";
                break;

            case PacketType.WordResult:
                XuLyKetQuaTu(packet.Payload);
                break;

            case PacketType.DuplicateWord:
                lblMessage.Text = "⚠️ Từ này đã được dùng!";
                break;

            case PacketType.Chat:
                XuLyChat(packet.Payload);
                break;

            case PacketType.GameEnd:
                ThongBaoMoManKetQua(packet.Payload);
                break;
        }
    }

    private void XuLyBatDauTran(string payload)
    {
        string[] parts = payload.Split('|', StringSplitOptions.TrimEntries);
        if (parts.Length >= 1)
        {
            lblCurrentWord.Text = $"Từ hiện tại: {parts[0]}";
        }

        if (parts.Length >= 2)
        {
            lblTurn.Text = $"Đến lượt: {parts[1]}";
        }

        if (parts.Length >= 3 && int.TryParse(parts[2], out int giay))
        {
            lblTimer.Text = $"Thời gian: {giay}s";
        }

        if (parts.Length >= 4)
        {
            CapNhatDanhSachNguoiChoi(parts[3].Split(',', StringSplitOptions.RemoveEmptyEntries));
        }

        lblMessage.Text = parts.Length >= 1
            ? $"Hãy nhập cụm 2 từ bắt đầu bằng \"{VietnameseDictionary.LayAmTietCuoi(parts[0])}\"."
            : "🎮 Game bắt đầu!";
    }

    private void CapNhatLuotVaThoiGian(string payload)
    {
        string[] parts = payload.Split('|', StringSplitOptions.TrimEntries);
        string tenNguoiChoi = parts.Length > 0 ? parts[0] : payload;
        lblTurn.Text = $"Đến lượt: {tenNguoiChoi}";

        if (parts.Length > 1 && int.TryParse(parts[1], out int soGiayConLai))
        {
            lblTimer.Text = $"Thời gian: {Math.Max(0, soGiayConLai)}s";
        }

        if (parts.Length > 2 && !string.IsNullOrWhiteSpace(parts[2]))
        {
            lblCurrentWord.Text = $"Từ hiện tại: {parts[2]}";
            bool laLuotCuaToi = tenNguoiChoi.Equals(_tenNguoiChoi, StringComparison.OrdinalIgnoreCase);
            lblMessage.Text = laLuotCuaToi
                ? $"Đến lượt bạn! Nhập cụm 2 từ bắt đầu bằng \"{VietnameseDictionary.LayAmTietCuoi(parts[2])}\"."
                : $"Đang chờ {tenNguoiChoi} nối từ...";
        }

        bool dangDenLuot = tenNguoiChoi.Equals(_tenNguoiChoi, StringComparison.OrdinalIgnoreCase);
        txtWordInput.Enabled = dangDenLuot;
        btnSubmitWord.Enabled = dangDenLuot;
    }

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
            lblMessage.Text = $"✅ {nguoiChoi} đã nối: {tuMoi}";
            XoaODeNhapTu();
            return;
        }

        lblMessage.Text = $"❌ {(parts.Length > 1 ? parts[1] : "Từ không hợp lệ.")}";
    }

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

    public void XoaODeNhapTu()
    {
        txtWordInput.Clear();
        if (txtWordInput.Enabled)
        {
            txtWordInput.Focus();
        }
    }

    public void XoaOChat() => txtChatInput.Clear();

    public void ThongBaoMoManKetQua(string payload) => YeuCauMoManKetQua?.Invoke(payload);

    private async void btnSubmitWord_Click(object? sender, EventArgs e)
    {
        string word = txtWordInput.Text.Trim();
        if (string.IsNullOrEmpty(word))
        {
            lblMessage.Text = "Bạn hãy nhập một cụm 2 từ trước khi gửi.";
            return;
        }

        if (!VietnameseDictionary.LaDungHaiTu(word))
        {
            int soTu = VietnameseDictionary.DemSoTu(word);
            lblMessage.Text = soTu < 2
                ? "Từ phải gồm đúng 2 tiếng (ví dụ: con mèo, cây tre)."
                : "Chỉ được nhập cụm 2 từ. Cụm 3 từ trở lên không hợp lệ.";
            return;
        }

        if (_writer is null)
        {
            lblMessage.Text = "Chưa có kết nối server để gửi từ.";
            return;
        }

        var packet = new Packet
        {
            Type = PacketType.SubmitWord,
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
            return;
        }

        if (_writer is null)
        {
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
