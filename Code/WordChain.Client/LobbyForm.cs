using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WordChain.Client;

public partial class LobbyForm : Form
{
    public LobbyForm()
    {
        InitializeComponent();
        ApDungTrangThaiMacDinh();
    }

    // Chuẩn bị trạng thái ban đầu để form chờ mở lên nhìn giống giao diện thật.
    private void ApDungTrangThaiMacDinh()
    {
        lblRoomCode.Text = "Mã phòng: NT001";
        lblLobbyStatus.Text = "Chờ mọi người sẵn sàng để bắt đầu.";
        CapNhatQuyenChuPhong(false);
    }

    public void CapNhatNguoiChoiTrongPhong(IEnumerable<string> danhSachNguoiChoi)
    {
        lstRoomPlayers.Items.Clear();

        foreach (string nguoiChoi in danhSachNguoiChoi)
        {
            lstRoomPlayers.Items.Add(nguoiChoi);
        }
    }

    public void CapNhatQuyenChuPhong(bool laChuPhong)
    {
        btnStartGame.Enabled = laChuPhong;
        lblHostHint.Text = laChuPhong
            ? "Bạn là chủ phòng, có thể bắt đầu khi mọi người đã sẵn sàng."
            : "Chỉ chủ phòng mới có thể bắt đầu trận đấu.";
    }

    private void btnStartGame_Click(object? sender, EventArgs e)
    {
        // Toàn sẽ nối sự kiện này với luồng bắt đầu ván chơi thật.
    }
}
