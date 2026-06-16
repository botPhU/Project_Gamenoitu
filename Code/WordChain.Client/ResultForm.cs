using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WordChain.Client;

public partial class ResultForm : Form
{
    public sealed class KetQuaNguoiChoi
    {
        public string TenNguoiChoi { get; set; } = string.Empty;
        public int Diem { get; set; }
    }

    public ResultForm()
    {
        InitializeComponent();
        ApDungTrangThaiMacDinh();
    }

    // Gán dữ liệu mặc định để form luôn mở ổn định kể cả khi chưa có kết quả thật.
    private void ApDungTrangThaiMacDinh()
    {
        lblWinner.Text = "🏆 Người thắng: Chưa xác định";
    }

    public void HienThiKetQua(string tenNguoiThang, IEnumerable<KetQuaNguoiChoi> bangDiem)
    {
        string tenThang = string.IsNullOrWhiteSpace(tenNguoiThang)
            ? "Chưa xác định"
            : tenNguoiThang.Trim();

        lblWinner.Text = $"🏆 Người thắng: {tenThang}";
        lstFinalScores.Items.Clear();

        foreach (KetQuaNguoiChoi nguoiChoi in bangDiem)
        {
            ListViewItem dong = new(nguoiChoi.TenNguoiChoi);
            dong.SubItems.Add(nguoiChoi.Diem.ToString());
            lstFinalScores.Items.Add(dong);
        }
    }

    private void btnBackToLobby_Click(object? sender, EventArgs e)
    {
        // Tạm đóng form để thuận tiện test giao diện.
        // Toàn có thể thay thế bằng điều hướng thật về phòng chờ sau.
        DialogResult = DialogResult.OK;
        Close();
    }
}
