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
    private int _soNguoiChoi;
    private bool _laChuPhong;

    public LobbyForm()
    {
        InitializeComponent();
        ApDungTrangThaiMacDinh();
        FormClosing += LobbyForm_FormClosing;
    }

    public LobbyForm(string roomCode, StreamWriter? writer, bool laChuPhong)
        : this()
    {
        GanKetNoi(roomCode, writer, laChuPhong);
    }

    public event Action<string>? GameStarted;

    private void ApDungTrangThaiMacDinh()
    {
        lblRoomCode.Text = "Mã phòng: ---";
        lblLobbyStatus.Text = "Chờ mọi người sẵn sàng để bắt đầu.";
        CapNhatQuyenChuPhong(false, 0);
    }

    public void GanKetNoi(string roomCode, StreamWriter? writer, bool laChuPhong)
    {
        _currentRoomCode = roomCode?.Trim() ?? string.Empty;
        _writer = writer;
        _laChuPhong = laChuPhong;

        if (!string.IsNullOrWhiteSpace(_currentRoomCode))
        {
            lblRoomCode.Text = $"Mã phòng: {_currentRoomCode}";
            Text = $"Phòng chờ - {_currentRoomCode}";
        }

        CapNhatQuyenChuPhong(_laChuPhong, _soNguoiChoi);
    }

    public void CapNhatNguoiChoiTrongPhong(IEnumerable<string> danhSachNguoiChoi)
    {
        lstRoomPlayers.Items.Clear();
        _soNguoiChoi = 0;

        foreach (string nguoiChoi in danhSachNguoiChoi)
        {
            if (!string.IsNullOrWhiteSpace(nguoiChoi))
            {
                lstRoomPlayers.Items.Add(nguoiChoi.Trim());
                _soNguoiChoi++;
            }
        }

        bool laChuPhong = _laChuPhong;
        CapNhatQuyenChuPhong(laChuPhong, _soNguoiChoi);
    }

    public void CapNhatQuyenChuPhong(bool laChuPhong, int soNguoiChoi = 0)
    {
        _laChuPhong = laChuPhong;
        _soNguoiChoi = soNguoiChoi > 0 ? soNguoiChoi : _soNguoiChoi;
        btnStartGame.Enabled = laChuPhong && _soNguoiChoi >= 2;
        lblHostHint.Text = laChuPhong
            ? _soNguoiChoi >= 2
                ? "Bạn là chủ phòng. Nhấn BẮT ĐẦU khi mọi người đã sẵn sàng."
                : "Bạn là chủ phòng. Cần ít nhất 2 người để bắt đầu."
            : "Chỉ chủ phòng mới có thể bắt đầu trận đấu.";
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
                CapNhatNguoiChoiTrongPhong(packet.Payload.Split(',', StringSplitOptions.RemoveEmptyEntries));
                lblLobbyStatus.Text = $"Đã có {_soNguoiChoi} người trong phòng.";
                break;

            case PacketType.UpdateRoom:
                try
                {
                    RoomInfo? room = System.Text.Json.JsonSerializer.Deserialize<RoomInfo>(packet.Payload);
                    if (room is not null)
                    {
                        CapNhatNguoiChoiTrongPhong(room.Players.ConvertAll(p => p.Nickname));
                    }
                }
                catch
                {
                    // Bỏ qua payload lỗi.
                }
                break;

            case PacketType.GameStart:
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

        if (_soNguoiChoi < 2)
        {
            lblLobbyStatus.Text = "Cần ít nhất 2 người chơi mới có thể bắt đầu.";
            return;
        }

        var packet = new Packet
        {
            Type = PacketType.StartGameRequest,
            Payload = _currentRoomCode
        };

        await _writer.WriteLineAsync(packet.ToJson());
        lblLobbyStatus.Text = "Đã gửi yêu cầu bắt đầu trận. Đang chờ server xác nhận...";
    }

    private void LobbyForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _boHuyNhanTin?.Cancel();
        _boHuyNhanTin?.Dispose();
        _boHuyNhanTin = null;
    }
}
