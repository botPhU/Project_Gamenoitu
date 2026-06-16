using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WordChain.Common;

namespace WordChain.Client;

public partial class LobbyForm : Form
{
    // Giữ các mã gói tin theo thứ tự nối tiếp phần room packet đang dùng ở client hiện tại.
    private static readonly PacketType _goiTinBatDauGame = (PacketType)15;
    private static readonly PacketType _goiTinCapNhatNguoiChoi = (PacketType)17;
    private static readonly PacketType _goiTinGameBatDau = (PacketType)18;

    private StreamReader? _reader;
    private StreamWriter? _writer;
    private CancellationTokenSource? _boHuyNhanTin;
    private string _currentRoomCode = string.Empty;

    public LobbyForm()
    {
        InitializeComponent();
        ApDungTrangThaiMacDinh();
        FormClosing += LobbyForm_FormClosing;
    }

    public LobbyForm(string roomCode, StreamReader? reader, StreamWriter? writer, bool laChuPhong)
        : this()
    {
        GanKetNoi(roomCode, reader, writer, laChuPhong);
    }

    public event Action<string>? GameStarted;

    // Chuẩn bị trạng thái ban đầu để form chờ mở lên nhìn giống giao diện thật.
    private void ApDungTrangThaiMacDinh()
    {
        lblRoomCode.Text = "Mã phòng: NT001";
        lblLobbyStatus.Text = "Chờ mọi người sẵn sàng để bắt đầu.";
        CapNhatQuyenChuPhong(false);
    }

    // Cắm thông tin phòng và luồng mạng để form hoạt động như màn hình phòng chờ thật.
    public void GanKetNoi(string roomCode, StreamReader? reader, StreamWriter? writer, bool laChuPhong)
    {
        _currentRoomCode = roomCode?.Trim() ?? string.Empty;
        _reader = reader;
        _writer = writer;

        if (!string.IsNullOrWhiteSpace(_currentRoomCode))
        {
            lblRoomCode.Text = $"Mã phòng: {_currentRoomCode}";
            Text = $"Phòng chờ - {_currentRoomCode}";
        }

        CapNhatQuyenChuPhong(laChuPhong);
        BatDauNhanTinTuServer();
    }

    public void CapNhatNguoiChoiTrongPhong(IEnumerable<string> danhSachNguoiChoi)
    {
        lstRoomPlayers.Items.Clear();

        foreach (string nguoiChoi in danhSachNguoiChoi)
        {
            if (!string.IsNullOrWhiteSpace(nguoiChoi))
            {
                lstRoomPlayers.Items.Add(nguoiChoi.Trim());
            }
        }
    }

    public void CapNhatQuyenChuPhong(bool laChuPhong)
    {
        btnStartGame.Enabled = laChuPhong;
        lblHostHint.Text = laChuPhong
            ? "Bạn là chủ phòng, có thể bắt đầu khi mọi người đã sẵn sàng."
            : "Chỉ chủ phòng mới có thể bắt đầu trận đấu.";
    }

    // Bật vòng lặp đọc packet nếu form phòng chờ đang quản lý kết nối.
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
            // Form đã đóng nên không cần làm gì thêm.
        }
        catch (IOException)
        {
            HienThiTrangThaiAnToan("Kết nối tới server đã bị ngắt.");
        }
        catch (InvalidOperationException)
        {
            HienThiTrangThaiAnToan("Không thể tiếp tục nhận dữ liệu phòng chờ.");
        }
    }

    // Cho phép form xử lý packet từ luồng nhận chung nếu sau này client gom việc đọc về một nơi.
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
                CapNhatNguoiChoiTrongPhong(packet.Payload.Split(',', StringSplitOptions.RemoveEmptyEntries));
                lblLobbyStatus.Text = "Danh sách người chơi vừa được cập nhật.";
                break;

            case var _ when packet.Type == _goiTinGameBatDau:
                lblLobbyStatus.Text = "🎮 Trận đấu đang bắt đầu!";
                GameStarted?.Invoke(_currentRoomCode);
                break;
        }
    }

    private async void btnStartGame_Click(object? sender, EventArgs e)
    {
        if (_writer is null)
        {
            lblLobbyStatus.Text = "Chưa có kết nối server để bắt đầu ván chơi.";
            return;
        }

        if (string.IsNullOrWhiteSpace(_currentRoomCode))
        {
            lblLobbyStatus.Text = "Chưa có mã phòng hợp lệ để gửi yêu cầu.";
            return;
        }

        var packet = new Packet
        {
            Type = _goiTinBatDauGame,
            Payload = _currentRoomCode
        };

        await _writer.WriteLineAsync(packet.ToJson());
        lblLobbyStatus.Text = "Đã gửi yêu cầu bắt đầu trận. Đang chờ server xác nhận...";
    }

    private void HienThiTrangThaiAnToan(string thongBao)
    {
        if (!IsHandleCreated)
        {
            return;
        }

        BeginInvoke(new Action(() => lblLobbyStatus.Text = thongBao));
    }

    private void LobbyForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _boHuyNhanTin?.Cancel();
        _boHuyNhanTin?.Dispose();
        _boHuyNhanTin = null;
    }
}
