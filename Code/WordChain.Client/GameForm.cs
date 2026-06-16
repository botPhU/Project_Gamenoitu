using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WordChain.Client;

public partial class GameForm : Form
{
    public GameForm()
    {
        InitializeComponent();
        ApDungDuLieuMacDinh();
    }

    // Gán dữ liệu khởi đầu để form mở lên luôn có nội dung rõ ràng và không bị trống.
    private void ApDungDuLieuMacDinh()
    {
        lblCurrentWord.Text = "Từ hiện tại: con mèo";
        lblTurn.Text = "Đến lượt: PlayerOne";
        lblTimer.Text = "Thời gian: 15s";
        lblMessage.Text = "Hãy nhập một từ mới bắt đầu bằng \"mèo\".";
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
            lstPlayers.Items.Add(nguoiChoi);
        }
    }

    public void CapNhatLichSuTu(IEnumerable<string> lichSuTu)
    {
        lstHistory.Items.Clear();

        foreach (string tu in lichSuTu)
        {
            lstHistory.Items.Add(tu);
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

    private void btnSubmitWord_Click(object? sender, EventArgs e)
    {
        // Toàn sẽ nối sự kiện này với luồng gửi từ nối thật.
    }

    private void btnSendChat_Click(object? sender, EventArgs e)
    {
        // Toàn sẽ nối sự kiện này với luồng gửi chat thật.
    }
}
