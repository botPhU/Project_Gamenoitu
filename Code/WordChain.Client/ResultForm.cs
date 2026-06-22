using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WordChain.Client;

public partial class ResultForm : Form
{
    private readonly string _roomCode;
    private readonly StreamReader? _reader;
    private readonly StreamWriter? _writer;
    private readonly bool _laChuPhong;
    public sealed class KetQuaNguoiChoi
    {
        public string TenNguoiChoi { get; set; } = string.Empty;
        public int Diem { get; set; }
    }

    public event Action? QuayVeSanhCho;

    public ResultForm()
    {
        InitializeComponent();
        ApDungTrangThaiMacDinh();
    }

    private void ApDungTrangThaiMacDinh()
    {
        lblWinner.Text = "🏆 Người thắng: Chưa xác định";
    }

    public void HienThiKetQua(string tenNguoiThang, IEnumerable<KetQuaNguoiChoi> bangDiem, string? lyDo = null)
    {
        string tenThang = string.IsNullOrWhiteSpace(tenNguoiThang)
            ? "Chưa xác định"
            : tenNguoiThang.Trim();

        lblWinner.Text = string.IsNullOrWhiteSpace(lyDo)
            ? $"🏆 Người thắng: {tenThang}"
            : $"🏆 Người thắng: {tenThang}\n{lyDo}";

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
        QuayVeSanhCho?.Invoke();
        DialogResult = DialogResult.OK;
        Close();
    }

    private void lstFinalScores_SelectedIndexChanged(object? sender, EventArgs e)
    {
    }

    public ResultForm(string roomCode, StreamReader? reader, StreamWriter? writer, bool laChuPhong)
    {
        InitializeComponent();
        _roomCode = roomCode;
        _reader = reader;
        _writer = writer;
        _laChuPhong = laChuPhong;
    }
}
