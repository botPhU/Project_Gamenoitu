using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WordChain.Common;

using System.Drawing;
using System.Drawing.Drawing2D;

namespace WordChain.Client;

public partial class Form1 : Form
{
    private readonly Color _mauNenChinh = Color.FromArgb(248, 246, 255);
    private readonly Color _mauNenDam = Color.FromArgb(92, 101, 156);
    private readonly Color _mauThe = Color.FromArgb(255, 252, 255);
    private readonly Color _mauVien = Color.FromArgb(229, 223, 244);
    private readonly Color _mauNhan = Color.FromArgb(110, 124, 212);
    private readonly Color _mauNhanDam = Color.FromArgb(86, 99, 184);
    private readonly Color _mauChu = Color.FromArgb(55, 60, 103);
    private readonly Color _mauPhu = Color.FromArgb(108, 112, 143);
    private readonly Color _mauLoi = Color.FromArgb(198, 90, 118);
    private readonly Color _mauThanhCong = Color.FromArgb(73, 165, 110);
    private readonly Color _mauHongNhat = Color.FromArgb(254, 242, 246);
    private readonly Color _mauVangNhat = Color.FromArgb(252, 245, 228);
    private readonly Color _mauXanhNhat = Color.FromArgb(241, 246, 252);
    private readonly Color _mauTimNhat = Color.FromArgb(244, 241, 250);

    private Label? _lblTrangThaiDangNhap;
    private Label? _lblTrangThaiDangKy;
    private Label? _lblTrangThaiChoiNhanh;
    private Panel? pnlConnect;

    // Khai báo các đối tượng kết nối mạng TCP
    private TcpClient? _client;
    private NetworkStream? _stream;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private bool _isConnected = false;
    private readonly List<PhongClient> _danhSachPhong = [];
    private PhongClient? _phongHienTai;
    private string _tenNguoiChoiHienTai = "PlayerOne";
    private int _soThuTuPhong = 1;

    private sealed class NguoiChoiPhong
    {
        public string TenHienThi { get; set; } = string.Empty;
        public bool LaChuPhong { get; set; }

        public override string ToString()
        {
            return LaChuPhong ? $"{TenHienThi} (chủ phòng)" : TenHienThi;
        }
    }

    private sealed class PhongClient
    {
        public string MaPhong { get; set; } = string.Empty;
        public string TenPhong { get; set; } = string.Empty;
        public string MatKhau { get; set; } = string.Empty;
        public string CheDo { get; set; } = "Thường";
        public int SoGiayMoiLuot { get; set; } = 20;
        public int SoNguoiToiDa { get; set; } = 4;
        public bool LaPhongRieng { get; set; }
        public string TuHienTai { get; set; } = "--";
        public string TrangThai { get; set; } = "Đang chờ thêm người chơi";
        public List<NguoiChoiPhong> NguoiChoi { get; } = [];
        public List<string> TinNhan { get; } = [];

        public override string ToString()
        {
            string quyenRieng = LaPhongRieng ? "Riêng tư" : "Công khai";
            return $"{MaPhong} • {TenPhong} • {NguoiChoi.Count}/{SoNguoiToiDa} • {CheDo} • {quyenRieng}";
        }
    }

    private sealed class HopThoaiTaoPhong : Form
    {
        private readonly TextBox _txtMaPhong;
        private readonly TextBox _txtMatKhau;
        private readonly TextBox _txtTenPhong;
        private readonly ComboBox _cboCheDo;
        private readonly NumericUpDown _numSoNguoi;
        private readonly NumericUpDown _numSoGiay;
        private readonly CheckBox _chkRiengTu;

        public string MaPhong => _txtMaPhong.Text.Trim().ToUpperInvariant();
        public string MatKhau => _txtMatKhau.Text.Trim();
        public string TenPhong => _txtTenPhong.Text.Trim();
        public string CheDo => _cboCheDo.SelectedItem?.ToString() ?? "Thường";
        public int SoNguoiToiDa => (int)_numSoNguoi.Value;
        public int SoGiayMoiLuot => (int)_numSoGiay.Value;
        public bool LaPhongRieng => _chkRiengTu.Checked;

        public HopThoaiTaoPhong(string tenMacDinh, string maPhongMacDinh)
        {
            Text = "Tạo phòng mới";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            ClientSize = new Size(420, 420);
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(249, 246, 255);

            Label lblTieuDe = new()
            {
                Text = "Thiết lập phòng chơi",
                Font = new Font("Bahnschrift SemiBold", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(55, 60, 103),
                Location = new Point(24, 20),
                AutoSize = true
            };

            Label lblTenPhong = new()
            {
                Text = "Tên phòng",
                Location = new Point(28, 124),
                AutoSize = true,
                ForeColor = Color.FromArgb(82, 97, 126)
            };

            Label lblMaPhong = new()
            {
                Text = "Mã phòng",
                Location = new Point(28, 72),
                AutoSize = true,
                ForeColor = Color.FromArgb(82, 97, 126)
            };

            _txtMaPhong = new TextBox
            {
                Location = new Point(28, 94),
                Size = new Size(364, 31),
                Font = new Font("Segoe UI", 11F),
                CharacterCasing = CharacterCasing.Upper,
                Text = maPhongMacDinh
            };

            _txtTenPhong = new TextBox
            {
                Location = new Point(28, 146),
                Size = new Size(364, 31),
                Font = new Font("Segoe UI", 11F),
                Text = $"Phòng của {tenMacDinh}"
            };

            Label lblCheDo = new()
            {
                Text = "Chế độ",
                Location = new Point(28, 190),
                AutoSize = true,
                ForeColor = Color.FromArgb(82, 97, 126)
            };

            _cboCheDo = new ComboBox
            {
                Location = new Point(28, 212),
                Size = new Size(170, 31),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10.5F)
            };
            _cboCheDo.Items.AddRange(["Thường", "Nhanh", "Thư giãn"]);
            _cboCheDo.SelectedIndex = 0;

            Label lblSoNguoi = new()
            {
                Text = "Số người tối đa",
                Location = new Point(222, 190),
                AutoSize = true,
                ForeColor = Color.FromArgb(82, 97, 126)
            };

            _numSoNguoi = new NumericUpDown
            {
                Location = new Point(222, 212),
                Size = new Size(170, 31),
                Minimum = 2,
                Maximum = 8,
                Value = 4,
                Font = new Font("Segoe UI", 10.5F)
            };

            Label lblSoGiay = new()
            {
                Text = "Thời gian mỗi lượt",
                Location = new Point(28, 256),
                AutoSize = true,
                ForeColor = Color.FromArgb(82, 97, 126)
            };

            _numSoGiay = new NumericUpDown
            {
                Location = new Point(28, 278),
                Size = new Size(170, 31),
                Minimum = 10,
                Maximum = 60,
                Value = 20,
                Increment = 5,
                Font = new Font("Segoe UI", 10.5F)
            };

            _chkRiengTu = new CheckBox
            {
                Text = "Phòng riêng tư",
                Location = new Point(222, 281),
                AutoSize = true,
                Font = new Font("Segoe UI", 10.5F),
                ForeColor = Color.FromArgb(82, 97, 126)
            };

            Label lblMatKhau = new()
            {
                Text = "Mật khẩu phòng",
                Location = new Point(28, 320),
                AutoSize = true,
                ForeColor = Color.FromArgb(82, 97, 126)
            };

            _txtMatKhau = new TextBox
            {
                Location = new Point(28, 342),
                Size = new Size(364, 31),
                Font = new Font("Segoe UI", 11F),
                UseSystemPasswordChar = true,
                Enabled = false
            };

            _chkRiengTu.CheckedChanged += (_, _) => _txtMatKhau.Enabled = _chkRiengTu.Checked;

            Button btnHuy = new()
            {
                Text = "Hủy",
                Location = new Point(208, 382),
                Size = new Size(88, 34),
                DialogResult = DialogResult.Cancel
            };

            Button btnDongY = new()
            {
                Text = "Tạo phòng",
                Location = new Point(304, 382),
                Size = new Size(88, 34),
                BackColor = Color.FromArgb(110, 124, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };
            btnDongY.FlatAppearance.BorderSize = 0;
            btnDongY.Click += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(MaPhong) || string.IsNullOrWhiteSpace(TenPhong))
                {
                    MessageBox.Show("Bạn hãy nhập tên phòng trước khi tiếp tục.", "Thiếu thông tin",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.None;
                }

                if (DialogResult != DialogResult.None && LaPhongRieng && string.IsNullOrWhiteSpace(MatKhau))
                {
                    MessageBox.Show("PhÃ²ng riÃªng tÆ° cáº§n cÃ³ máº­t kháº©u Ä‘á»ƒ báº£o vá»‡.", "Thiáº¿u máº­t kháº©u",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.None;
                }
            };

            AcceptButton = btnDongY;
            CancelButton = btnHuy;

            Controls.AddRange([lblTieuDe, lblMaPhong, _txtMaPhong, lblTenPhong, _txtTenPhong, lblCheDo, _cboCheDo,
                lblSoNguoi, _numSoNguoi, lblSoGiay, _numSoGiay, _chkRiengTu, lblMatKhau, _txtMatKhau, btnHuy, btnDongY]);
        }
    }

    private sealed class HopThoaiNhapMaPhong : Form
    {
        private readonly TextBox _txtMaPhong;
        private readonly TextBox _txtMatKhau;

        public string MaPhong => _txtMaPhong.Text.Trim().ToUpperInvariant();
        public string MatKhau => _txtMatKhau.Text.Trim();

        public HopThoaiNhapMaPhong(string? maPhongMacDinh = null, bool hienMatKhau = true)
        {
            Text = "Tham gia phòng";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            ClientSize = new Size(360, 240);
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(249, 246, 255);

            Label lblTieuDe = new()
            {
                Text = "Nhập mã phòng",
                Font = new Font("Bahnschrift SemiBold", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(55, 60, 103),
                Location = new Point(24, 20),
                AutoSize = true
            };

            Label lblHuongDan = new()
            {
                Text = "Bạn có thể dán mã phòng bạn bè đã gửi cho bạn.",
                Location = new Point(24, 58),
                Size = new Size(312, 32),
                ForeColor = Color.FromArgb(82, 97, 126)
            };

            _txtMaPhong = new TextBox
            {
                Location = new Point(24, 96),
                Size = new Size(312, 31),
                CharacterCasing = CharacterCasing.Upper,
                Font = new Font("Segoe UI", 11F),
                Text = maPhongMacDinh ?? string.Empty
            };

            Label lblMatKhau = new()
            {
                Text = "Mật khẩu phòng",
                Location = new Point(24, 136),
                AutoSize = true,
                ForeColor = Color.FromArgb(82, 97, 126)
            };

            _txtMatKhau = new TextBox
            {
                Location = new Point(24, 158),
                Size = new Size(312, 31),
                Font = new Font("Segoe UI", 11F),
                UseSystemPasswordChar = true,
                Enabled = hienMatKhau
            };

            Button btnDongY = new()
            {
                Text = "Vào phòng",
                Location = new Point(244, 198),
                Size = new Size(92, 30),
                BackColor = Color.FromArgb(110, 124, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };
            btnDongY.FlatAppearance.BorderSize = 0;
            btnDongY.Click += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(MaPhong))
                {
                    MessageBox.Show("Bạn hãy nhập mã phòng trước khi tiếp tục.", "Thiếu thông tin",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.None;
                }
            };

            Controls.AddRange([lblTieuDe, lblHuongDan, _txtMaPhong, lblMatKhau, _txtMatKhau, btnDongY]);
            AcceptButton = btnDongY;
        }
    }



    public Form1()
    {
        InitializeComponent();
        CauHinhCheDoChoiNhanh();
        DoubleBuffered = true;
        ApDungThietKeTaste();
        KhoiTaoTrangThaiBanDau();
        KhoiTaoTuongTacClient();
        HienManHinh(tabChoiNhanh);
    }

    // Giữ đúng một lối vào là chơi nhanh để giao diện đầu tiên gọn và dễ hiểu hơn.
    private void CauHinhCheDoChoiNhanh()
    {
        if (tabMain.TabPages.Contains(tabDangNhap))
        {
            tabMain.TabPages.Remove(tabDangNhap);
        }

        if (tabMain.TabPages.Contains(tabDangKy))
        {
            tabMain.TabPages.Remove(tabDangKy);
        }

        btnTabDangNhap.Visible = false;
        btnTabDangKy.Visible = false;
        lnkDangKyNgay.Visible = false;
        lblChuaCoTaiKhoan.Visible = false;
        lnkDangNhapNgay.Visible = false;
        lblDaCoTaiKhoan.Visible = false;
    }


    // Lắng nghe tin từ Server
    private async Task ReceiveMessagesAsync()
    {
        try
        {
            while (_isConnected)
            {
                string? line = await _reader!.ReadLineAsync();
                if (line == null) break;

                var packet = Packet.FromJson(line);
                if (packet == null) continue;

                this.Invoke(new Action(() =>
                {
                    if (packet.Type == PacketType.ConnectOK)
                        lblStatus.Text = "🟢 " + packet.Payload;
                }));
            }
        }
        catch
        {
            this.Invoke(new Action(() =>
            {
                lblStatus.Text = "⚠️ Mất kết nối với Server.";
                if (btnConnect is not null)
                {
                    btnConnect.Enabled = true;
                }
                _isConnected = false;
            }));
        }
    }

    // Gom toàn bộ phần làm đẹp vào một luồng duy nhất để dễ chỉnh theme.
    private void KhoiTaoTuongTacClient()
    {
        btnTaoPhong.Click -= btnTaoPhong_Click;
        btnThamGiaPhong.Click -= btnThamGiaPhong_Click;
        btnGuiTu.Click -= btnGuiTu_Click;
        btnGuiChat.Click -= btnGuiChat_Click;
        btnRoiPhong.Click -= btnRoiPhong_Click;

        btnTaoPhong.Click += (_, _) => MoQuyTrinhTaoPhong();
        btnThamGiaPhong.Click += (_, _) => MoQuyTrinhThamGiaPhong();
        btnGuiTu.Click += (_, _) => GuiTuNoiBo();
        btnGuiChat.Click += (_, _) => GuiChatNoiBo();
        btnRoiPhong.Click += (_, _) => RoiPhongNoiBo();
        btnMenuSanhCho.Click += (_, _) => MoSanhCho();
        btnMenuTaoPhong.Click += (_, _) => MoQuyTrinhTaoPhong();
        btnMenuThamGiaPhong.Click += (_, _) => MoQuyTrinhThamGiaPhong();
        lstPhongNoiBat.DoubleClick += (_, _) => ThamGiaPhongDangChon();
        txtNhapTu.KeyDown += txtNhapTu_KeyDown;
        txtTinNhan.KeyDown += txtTinNhan_KeyDown;
        CapNhatTrangThaiMenuSanhCho(btnMenuSanhCho);
        LamMoiDanhSachPhong();
    }

    private void ApDungThietKeTaste()
    {
        TaoNhanTrangThai();
        ApDungBoMauVaFont();
        ApDungBoCucXacLap();
        ApDungSuKienVeNen();
        BoTronDieuKhien();
    }

    // Tạo nhãn trạng thái ngay trong thẻ giao diện thay cho hộp thoại bật lên.
    private void TaoNhanTrangThai()
    {
        _lblTrangThaiDangNhap = TaoNhanTrangThai(30, 305);
        _lblTrangThaiDangKy = TaoNhanTrangThai(48, 358);
        _lblTrangThaiChoiNhanh = TaoNhanTrangThai(104, 462);

        cardDangNhap.Controls.Add(_lblTrangThaiDangNhap);
        cardDangKy.Controls.Add(_lblTrangThaiDangKy);
        cardChoiNhanh.Controls.Add(_lblTrangThaiChoiNhanh);
    }

    private Label TaoNhanTrangThai(int x, int y)
    {
        return new Label
        {
            AutoSize = false,
            Location = new Point(x, y),
            Size = new Size(460, 32),
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
            ForeColor = _mauLoi,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft,
            Visible = false
        };
    }

    // Bộ nhận diện màu sắc và chữ cho toàn bộ ứng dụng.
    private void ApDungBoMauVaFont()
    {
        BackColor = _mauNenChinh;
        pnlTopBar.BackColor = Color.FromArgb(242, 238, 252);
        lblTenUngDung.ForeColor = _mauChu;
        lblTenUngDung.Font = new Font("Bahnschrift SemiBold", 11F, FontStyle.Bold);
        lblTenUngDung.Text = "Game nối từ online";

        ApDungThe(cardDangNhap);
        ApDungThe(cardDangKy);
        ApDungThe(cardChoiNhanh);
        ApDungThe(cardTaoPhong, _mauHongNhat);
        ApDungThe(cardThamGiaPhong, _mauVangNhat);
        ApDungThe(cardGame, _mauThe);
        ApDungThe(cardChat, _mauXanhNhat);
        ApDungThe(cardLichSuVaNguoiChoi, _mauTimNhat);
        ApDungThe(cardThongTinTranDau, Color.FromArgb(245, 241, 255));
        pnlSanhChoMenu.BackColor = Color.FromArgb(252, 250, 255);
        pnlPhongTop.BackColor = Color.FromArgb(252, 250, 255);

        ApDungNutChinh(btnDangNhap);
        ApDungNutChinh(btnDangKy);
        ApDungNutChinh(btnBatDauChoiNhanh);
        ApDungNutChinh(btnTaoPhong);
        ApDungNutChinh(btnThamGiaPhong);
        ApDungNutChinh(btnGuiTu);
        ApDungNutChinh(btnGuiChat, 68);

        ApDungNutVien(btnRoiPhong);
        ApDungNutVien(btnThoat);

        ApDungNutMenu(btnMenuSanhCho, true);
        ApDungNutMenu(btnMenuTaoPhong, false);
        ApDungNutMenu(btnMenuThamGiaPhong, false);
        ApDungNutMenu(btnBanBe, false);
        ApDungNutMenu(btnBangXepHang, false);
        ApDungNutMenu(btnCaiDat, false);

        ApDungNutChuyenTab(btnTabDangNhap, false);
        ApDungNutChuyenTab(btnTabDangKy, false);
        ApDungNutChuyenTab(btnTabChoiNhanh, true);

        ApDungHopNhap(txtDangNhapTenDangNhap);
        ApDungHopNhap(txtDangNhapMatKhau);
        ApDungHopNhap(txtDangKyTenDangNhap);
        ApDungHopNhap(txtDangKyMatKhau);
        ApDungHopNhap(txtDangKyNhapLaiMatKhau);
        ApDungHopNhap(txtChoiNhanhTenNguoiChoi);
        ApDungHopNhap(txtNhapTu);
        ApDungHopNhap(txtTinNhan, 9.5F);

        ApDungDanhSach(lstPhongNoiBat);
        ApDungDanhSach(lstNguoiChoi);
        ApDungDanhSach(lstChatTrongPhong, 10.5F);
        lstPhongNoiBat.BackColor = Color.FromArgb(255, 250, 252);
        lstNguoiChoi.BackColor = Color.FromArgb(247, 243, 255);
        lstChatTrongPhong.BackColor = Color.FromArgb(244, 249, 255);

        lblLogoGame.Visible = false;
        lblLogoOnline.Visible = false;

        lblDangKyTieuDe.Font = new Font("Bahnschrift SemiBold", 22F, FontStyle.Bold);
        lblDangKyTieuDe.ForeColor = _mauChu;
        lblDangKyTieuDe.Text = "Tạo tài khoản thật nhanh";

        lblChoiNhanhTieuDe.Font = new Font("Bahnschrift SemiBold", 22F, FontStyle.Bold);
        lblChoiNhanhTieuDe.ForeColor = _mauChu;
        lblChoiNhanhTieuDe.Text = "Chơi ngay";
        lblChoiNhanhMoTa.AutoSize = false;
        lblChoiNhanhMoTa.Font = new Font("Segoe UI", 11F);
        lblChoiNhanhMoTa.ForeColor = _mauPhu;
        lblChoiNhanhMoTa.Text = "Nhập tên hiển thị, chọn màu bạn thích và vào sảnh chờ ngay. Không cần tạo tài khoản để bắt đầu.";
        lblChoiNhanhMoTa.TextAlign = ContentAlignment.TopCenter;
        btnTabDangNhap.Visible = false;
        btnTabDangKy.Visible = false;
        lnkDangKyNgay.Visible = false;
        lblChuaCoTaiKhoan.Visible = false;
        lnkDangNhapNgay.Visible = false;
        lblDaCoTaiKhoan.Visible = false;

        lblTieuDeSanhCho.Font = new Font("Bahnschrift SemiBold", 28F, FontStyle.Bold);
        lblTieuDeSanhCho.ForeColor = _mauChu;
        lblTieuDeSanhCho.Text = "Sảnh chờ";
        lblMoTaSanhCho.AutoSize = false;
        lblMoTaSanhCho.Font = new Font("Segoe UI", 11.5F);
        lblMoTaSanhCho.ForeColor = _mauPhu;
        lblMoTaSanhCho.Text = "Chọn cách chơi phù hợp với bạn rồi rủ bạn bè vào cùng cho thật đông vui.";
        lblMoTaSanhCho.TextAlign = ContentAlignment.TopLeft;
        lblPhongNoiBat.Font = new Font("Bahnschrift SemiBold", 18F, FontStyle.Bold);
        lblPhongNoiBat.ForeColor = _mauChu;
        lblPhongNoiBat.Text = "Phòng đang mở ✦";

        lblTenNguoiChoiSanh.Font = new Font("Bahnschrift SemiBold", 16F, FontStyle.Bold);
        lblTenNguoiChoiSanh.ForeColor = _mauChu;
        lblTrangThaiOnline.ForeColor = _mauThanhCong;
        lblTrangThaiOnline.Text = "Sẵn sàng chơi";

        lblThongTinPhong.Font = new Font("Bahnschrift SemiBold", 18F, FontStyle.Bold);
        lblThongTinPhong.ForeColor = _mauChu;
        lblDanhSachNguoiChoi.Font = new Font("Bahnschrift SemiBold", 16F, FontStyle.Bold);
        lblDanhSachNguoiChoi.ForeColor = _mauChu;
        lblDanhSachNguoiChoi.Text = "Mọi người trong phòng";
        lblChat.Font = new Font("Bahnschrift SemiBold", 16F, FontStyle.Bold);
        lblChat.ForeColor = _mauChu;
        lblChat.Text = "Trò chuyện";
        lblLuotChoi.Font = new Font("Bahnschrift SemiBold", 16F, FontStyle.Bold);
        lblLuotChoi.ForeColor = _mauChu;
        lblBoDem.Font = new Font("Consolas", 16F, FontStyle.Bold);
        lblBoDem.ForeColor = _mauNhanDam;
        lblTieuDeTuHienTai.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblTieuDeTuHienTai.ForeColor = _mauPhu;
        lblTieuDeTuHienTai.Text = "Từ hiện tại";
        lblTuHienTai.Font = new Font("Bahnschrift SemiBold", 30F, FontStyle.Bold);
        lblTuHienTai.ForeColor = _mauChu;
        lblHuongDanNoiTu.Font = new Font("Segoe UI", 11F);
        lblHuongDanNoiTu.ForeColor = _mauPhu;
        lblNhapTu.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblNhapTu.ForeColor = _mauChu;
        lblNhapTu.Text = "Đến lượt bạn";
        lblThongTinChiTiet.Font = new Font("Segoe UI", 11F);
        lblThongTinChiTiet.ForeColor = _mauPhu;
        lblThongTinChiTiet.AutoSize = false;

        lblTrangThaiPhong.ForeColor = _mauPhu;
        lblTrangThaiNguoiChoi.ForeColor = _mauPhu;
        lblTrangThaiChat.ForeColor = _mauPhu;

        // Các vòng màu nhỏ giúp màn chơi nhanh trông vui mắt hơn.
        avatar1.BackColor = Color.FromArgb(255, 185, 202);
        avatar2.BackColor = Color.FromArgb(255, 214, 153);
        avatar3.BackColor = Color.FromArgb(170, 219, 165);
        avatar4.BackColor = Color.FromArgb(157, 206, 255);
        avatar5.BackColor = Color.FromArgb(198, 183, 255);
    }

    // Căn lại bố cục để giao diện thoáng hơn và nhìn bớt "ô lưới" cứng.
    private void ApDungBoCucXacLap()
    {
        cardDangNhap.Location = new Point(806, 164);
        cardDangNhap.Size = new Size(470, 382);
        cardDangKy.Location = new Point(806, 156);
        cardDangKy.Size = new Size(470, 404);
        cardChoiNhanh.Location = new Point(820, 170);
        cardChoiNhanh.Size = new Size(454, 406);

        btnTabDangNhap.Text = "Đăng nhập";
        btnTabDangKy.Text = "Đăng ký";
        btnTabChoiNhanh.Text = "Chơi nhanh";
        btnDangNhap.Text = "Vào sảnh chờ";
        btnDangKy.Text = "Tạo tài khoản";
        btnBatDauChoiNhanh.Text = "Bắt đầu chơi";
        btnTaoPhong.Text = "Tạo phòng";
        btnThamGiaPhong.Text = "Vào phòng";
        btnGuiTu.Text = "Gửi từ";

        btnTabDangNhap.Location = new Point(22, 22);
        btnTabDangKy.Location = new Point(164, 22);
        btnTabChoiNhanh.Location = new Point(306, 22);
        btnTabDangNhap.Size = new Size(132, 34);
        btnTabDangKy.Size = new Size(132, 34);
        btnTabChoiNhanh.Size = new Size(142, 34);

        txtDangNhapTenDangNhap.Location = new Point(26, 92);
        txtDangNhapTenDangNhap.Size = new Size(418, 34);
        txtDangNhapMatKhau.Location = new Point(26, 144);
        txtDangNhapMatKhau.Size = new Size(418, 34);
        chkGhiNhoDangNhap.Location = new Point(28, 194);
        lnkQuenMatKhau.Location = new Point(320, 196);
        btnDangNhap.Location = new Point(26, 238);
        btnDangNhap.Size = new Size(418, 46);
        if (_lblTrangThaiDangNhap is not null)
        {
            _lblTrangThaiDangNhap.Location = new Point(26, 298);
            _lblTrangThaiDangNhap.Size = new Size(418, 40);
        }

        lblDangKyTieuDe.Location = new Point(92, 36);
        lblDangKyTieuDe.Size = new Size(290, 42);
        txtDangKyTenDangNhap.Location = new Point(28, 100);
        txtDangKyTenDangNhap.Size = new Size(414, 34);
        txtDangKyMatKhau.Location = new Point(28, 152);
        txtDangKyMatKhau.Size = new Size(414, 34);
        txtDangKyNhapLaiMatKhau.Location = new Point(28, 204);
        txtDangKyNhapLaiMatKhau.Size = new Size(414, 34);
        btnDangKy.Location = new Point(28, 266);
        btnDangKy.Size = new Size(414, 46);
        if (_lblTrangThaiDangKy is not null)
        {
            _lblTrangThaiDangKy.Location = new Point(28, 324);
            _lblTrangThaiDangKy.Size = new Size(414, 40);
        }
        lblDaCoTaiKhoan.Location = new Point(108, 370);
        lnkDangNhapNgay.Location = new Point(233, 370);

        lblChoiNhanhTieuDe.Location = new Point(126, 32);
        lblChoiNhanhTieuDe.Size = new Size(202, 42);
        lblChoiNhanhTieuDe.TextAlign = ContentAlignment.MiddleCenter;
        lblChoiNhanhMoTa.Location = new Point(52, 82);
        lblChoiNhanhMoTa.Size = new Size(350, 52);
        lblAvatarMau.Location = new Point(160, 136);
        txtChoiNhanhTenNguoiChoi.Location = new Point(52, 228);
        txtChoiNhanhTenNguoiChoi.Size = new Size(350, 34);
        avatar1.Location = new Point(80, 288);
        avatar2.Location = new Point(146, 288);
        avatar3.Location = new Point(212, 288);
        avatar4.Location = new Point(278, 288);
        avatar5.Location = new Point(344, 288);
        btnBatDauChoiNhanh.Location = new Point(52, 340);
        btnBatDauChoiNhanh.Size = new Size(350, 48);
        if (_lblTrangThaiChoiNhanh is not null)
        {
            _lblTrangThaiChoiNhanh.Location = new Point(52, 390);
            _lblTrangThaiChoiNhanh.Size = new Size(350, 36);
        }

        pnlSanhChoMenu.Visible = false;
        pnlSanhChoMenu.Width = 0;
        pnlSanhChoMenu.Padding = new Padding(0);
        pnlSanhChoNoiDung.Dock = DockStyle.Fill;
        pnlSanhChoNoiDung.Location = new Point(0, 0);
        lblAvatarSanhCho.Location = new Point(18, 34);
        lblTenNguoiChoiSanh.Location = new Point(18, 98);
        lblTrangThaiOnline.Location = new Point(20, 130);
        btnMenuSanhCho.Location = new Point(18, 182);
        btnMenuTaoPhong.Location = new Point(18, 226);
        btnMenuThamGiaPhong.Location = new Point(18, 270);
        btnBanBe.Location = new Point(18, 314);
        btnBangXepHang.Location = new Point(18, 358);
        btnCaiDat.Location = new Point(18, 402);
        btnThoat.Location = new Point(18, 742);
        btnThoat.Size = new Size(176, 38);

        pnlSanhChoNoiDung.Padding = new Padding(72, 34, 72, 34);
        cardTaoPhong.Location = new Point(74, 164);
        cardTaoPhong.Size = new Size(540, 224);
        cardThamGiaPhong.Location = new Point(680, 164);
        cardThamGiaPhong.Size = new Size(540, 224);
        lblPhongNoiBat.Location = new Point(72, 436);
        lstPhongNoiBat.Location = new Point(78, 496);
        lstPhongNoiBat.Size = new Size(1142, 184);
        lblTrangThaiPhong.Location = new Point(78, 500);
        lblMoTaSanhCho.Size = new Size(620, 48);

        pnlPhongTop.Height = 84;
        cardLichSuVaNguoiChoi.Location = new Point(28, 30);
        cardLichSuVaNguoiChoi.Size = new Size(294, 652);
        cardGame.Location = new Point(348, 30);
        cardGame.Size = new Size(720, 470);
        cardThongTinTranDau.Location = new Point(348, 524);
        cardThongTinTranDau.Size = new Size(720, 158);
        cardChat.Location = new Point(1094, 30);
        cardChat.Size = new Size(304, 652);

        lblThongTinChiTiet.Location = new Point(28, 28);
        lblThongTinChiTiet.Size = new Size(650, 76);
        txtNhapTu.Size = new Size(456, 34);
        txtNhapTu.Location = new Point(42, 321);
        btnGuiTu.Location = new Point(524, 318);
        btnGuiTu.Size = new Size(144, 40);
        txtTinNhan.Location = new Point(20, 595);
        txtTinNhan.Size = new Size(190, 31);
        btnGuiChat.Location = new Point(222, 594);
        btnGuiChat.Size = new Size(62, 32);
        lstChatTrongPhong.Size = new Size(260, 500);
        lblTrangThaiChat.Size = new Size(240, 38);
    }

    // Gắn các lớp nền mềm để giao diện bớt phẳng và thân thiện hơn.
    private void ApDungSuKienVeNen()
    {
        tabDangNhap.Paint += VeNenTaiKhoanMoi;
        tabDangKy.Paint += VeNenTaiKhoanMoi;
        tabChoiNhanh.Paint += VeNenTaiKhoanMoi;
        tabSanhCho.Paint += VeNenSang;
        tabPhongChoi.Paint += VeNenSang;
    }

    // Bo góc đồng nhất để giao diện nhìn mềm và dễ gần hơn.
    private void BoTronDieuKhien()
    {
        BoTron(cardDangNhap, 24);
        BoTron(cardDangKy, 24);
        BoTron(cardChoiNhanh, 24);
        BoTron(cardTaoPhong, 28);
        BoTron(cardThamGiaPhong, 28);
        BoTron(cardGame, 24);
        BoTron(cardChat, 24);
        BoTron(cardLichSuVaNguoiChoi, 24);
        BoTron(cardThongTinTranDau, 24);

        BoTron(btnDangNhap, 18);
        BoTron(btnDangKy, 18);
        BoTron(btnBatDauChoiNhanh, 18);
        BoTron(btnTaoPhong, 18);
        BoTron(btnThamGiaPhong, 18);
        BoTron(btnGuiTu, 16);
        BoTron(btnGuiChat, 16);
        BoTron(btnRoiPhong, 16);
        BoTron(btnThoat, 16);
        BoTron(btnMenuSanhCho, 18);
        BoTron(btnMenuTaoPhong, 18);
        BoTron(btnMenuThamGiaPhong, 18);
        BoTron(btnBanBe, 18);
        BoTron(btnBangXepHang, 18);
        BoTron(btnCaiDat, 18);
        BoTron(btnTabDangNhap, 16);
        BoTron(btnTabDangKy, 16);
        BoTron(btnTabChoiNhanh, 16);
        BoTron(lblLogoOnline, 12);
        BoTron(avatar1, 18);
        BoTron(avatar2, 18);
        BoTron(avatar3, 18);
        BoTron(avatar4, 18);
        BoTron(avatar5, 18);
    }

    private void ApDungThe(Panel panel, Color? backColor = null)
    {
        panel.BackColor = backColor ?? _mauThe;
    }

    private void ApDungNutChinh(Button button, int width = 0)
    {
        button.BackColor = _mauNhan;
        button.ForeColor = Color.White;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = _mauNhanDam;
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(25, 58, 150);
        button.Font = new Font("Bahnschrift SemiBold", 12.5F, FontStyle.Bold);
        if (width > 0)
        {
            button.Width = width;
        }
    }

    private void ApDungNutVien(Button button)
    {
        button.BackColor = Color.FromArgb(255, 252, 255);
        button.ForeColor = _mauChu;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = _mauVien;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(247, 249, 254);
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(234, 238, 248);
    }

    private void ApDungNutMenu(Button button, bool dangChon)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Width = 176;
        button.Height = 38;
        button.Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold);
        button.TextAlign = ContentAlignment.MiddleLeft;
        button.BackColor = dangChon ? Color.FromArgb(235, 230, 255) : Color.Transparent;
        button.ForeColor = dangChon ? _mauNhanDam : _mauChu;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(245, 241, 255);
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(233, 228, 248);
    }

    private void ApDungNutChuyenTab(Button button, bool dangChon)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Font = new Font("Bahnschrift SemiBold", 10.5F, FontStyle.Bold);
        button.BackColor = dangChon ? Color.FromArgb(237, 233, 255) : Color.White;
        button.ForeColor = dangChon ? _mauNhanDam : _mauPhu;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(247, 243, 255);
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(236, 230, 248);
    }

    private void ApDungHopNhap(TextBox textBox, float size = 11.5F)
    {
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.BackColor = Color.FromArgb(249, 250, 253);
        textBox.ForeColor = _mauChu;
        textBox.Font = new Font("Segoe UI", size, FontStyle.Regular);
    }

    private void ApDungDanhSach(ListBox listBox, float size = 11.5F)
    {
        listBox.BackColor = _mauThe;
        listBox.BorderStyle = BorderStyle.None;
        listBox.Font = new Font("Segoe UI", size, FontStyle.Regular);
        listBox.ForeColor = _mauChu;
    }

    private void BoTron(Control control, int radius)
    {
        using GraphicsPath path = new();
        int diameter = radius * 2;

        path.StartFigure();
        path.AddArc(0, 0, diameter, diameter, 180, 90);
        path.AddArc(control.Width - diameter, 0, diameter, diameter, 270, 90);
        path.AddArc(control.Width - diameter, control.Height - diameter, diameter, diameter, 0, 90);
        path.AddArc(0, control.Height - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        control.Region = new Region(path);
    }

    private void VeNenTaiKhoan(object? sender, PaintEventArgs e)
    {
        if (sender is not Control control)
        {
            return;
        }

        using LinearGradientBrush brush = new(
            control.ClientRectangle,
            Color.FromArgb(98, 109, 184),
            Color.FromArgb(167, 189, 225),
            LinearGradientMode.ForwardDiagonal);

        e.Graphics.FillRectangle(brush, control.ClientRectangle);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using SolidBrush phuSang = new(Color.FromArgb(22, 255, 255, 255));
        using SolidBrush phuHong = new(Color.FromArgb(20, 240, 193, 212));
        using SolidBrush phuVang = new(Color.FromArgb(18, 243, 224, 174));
        using SolidBrush phuTim = new(Color.FromArgb(18, 192, 186, 236));
        using SolidBrush saoNho = new(Color.FromArgb(170, 255, 247, 210));
        e.Graphics.FillEllipse(phuTim, 26, 480, 280, 280);
        e.Graphics.FillEllipse(phuSang, 170, 94, 160, 160);
        e.Graphics.FillEllipse(phuHong, 520, 46, 220, 220);
        e.Graphics.FillEllipse(phuVang, 580, 420, 180, 180);
        VeNgoiSao(e.Graphics, saoNho, 162, 122, 10);
        VeNgoiSao(e.Graphics, saoNho, 462, 152, 8);
        VeNgoiSao(e.Graphics, saoNho, 420, 560, 9);

        using Font fontTieuDe = new("Bahnschrift SemiBold", 35F, FontStyle.Bold);
        using Font fontThuongHieu = new("Bahnschrift SemiBold", 32F, FontStyle.Bold);
        using Font fontPhu = new("Segoe UI Semibold", 12.5F, FontStyle.Bold);
        using Font fontChip = new("Segoe UI Semibold", 10F, FontStyle.Bold);
        using Font fontTag = new("Segoe UI Semibold", 10F, FontStyle.Bold);
        using Font fontIcon = new("Bahnschrift SemiBold", 18F, FontStyle.Bold);
        using SolidBrush chuTrang = new(Color.FromArgb(248, 250, 255));
        using SolidBrush chuPhu = new(Color.FromArgb(235, 239, 248));
        using SolidBrush nenChip = new(Color.FromArgb(178, 248, 245, 255));
        using SolidBrush chuChip = new(Color.FromArgb(74, 81, 126));
        using SolidBrush nenTag = new(Color.FromArgb(244, 238, 255));
        using SolidBrush chuTag = new(Color.FromArgb(86, 99, 184));
        using Pen vienNhe = new(Color.FromArgb(30, 255, 255, 255), 1.2F);
        using Pen duongNoi = new(Color.FromArgb(180, 236, 240, 252), 3.2F);
        duongNoi.StartCap = LineCap.Round;
        duongNoi.EndCap = LineCap.Round;
        using SolidBrush nenIcon1 = new(Color.FromArgb(255, 246, 228, 235));
        using SolidBrush nenIcon2 = new(Color.FromArgb(255, 236, 243, 255));
        using SolidBrush nenIcon3 = new(Color.FromArgb(255, 233, 247, 228));
        using SolidBrush chuIcon = new(Color.FromArgb(84, 92, 148));
        using SolidBrush nenChu1 = new(Color.FromArgb(235, 244, 255));
        using SolidBrush nenChu2 = new(Color.FromArgb(255, 238, 244));
        using SolidBrush nenChu3 = new(Color.FromArgb(245, 239, 255));
        using SolidBrush nenChu4 = new(Color.FromArgb(241, 250, 232));
        using SolidBrush chuTrangTri = new(Color.FromArgb(78, 85, 132));
        using Pen duongCham = new(Color.FromArgb(120, 240, 242, 248), 2.4F);
        duongCham.DashPattern = [2f, 5f];
        duongCham.StartCap = LineCap.Round;
        duongCham.EndCap = LineCap.Round;
        using Font fontChuLon = new("Bahnschrift SemiBold", 23F, FontStyle.Bold);
        using Font fontChuNho = new("Bahnschrift SemiBold", 18F, FontStyle.Bold);

        Rectangle khoiThuongHieu = new(108, 168, 300, 112);
        Rectangle tagLine = new(110, 298, 278, 30);
        using GraphicsPath pathThuongHieu = TaoDuongBoTron(khoiThuongHieu, 26);
        using GraphicsPath pathTag = TaoDuongBoTron(tagLine, 15);
        using LinearGradientBrush nenThuongHieu = new(
            khoiThuongHieu,
            Color.FromArgb(46, 88, 188),
            Color.FromArgb(71, 112, 213),
            LinearGradientMode.ForwardDiagonal);

        e.Graphics.FillPath(nenThuongHieu, pathThuongHieu);
        e.Graphics.DrawPath(vienNhe, pathThuongHieu);
        e.Graphics.FillPath(nenTag, pathTag);
        e.Graphics.DrawString("Nối từ\nonline", fontThuongHieu, chuTrang, new RectangleF(126, 184, 220, 86));
        e.Graphics.DrawString(
            "Nhẹ nhàng, đáng yêu và thật dễ chơi",
            fontTag,
            chuTag,
            tagLine,
            new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            });

        VeCumIconNoiChu(
            e.Graphics,
            new Point(116, 348),
            duongNoi,
            nenIcon1,
            nenIcon2,
            nenIcon3,
            chuIcon,
            fontIcon);

        e.Graphics.DrawString("Nối một từ,\nvui cả phòng", fontTieuDe, chuTrang, new RectangleF(108, 404, 420, 108));
        e.Graphics.DrawString(
            "Tạo biệt danh dễ thương, chọn một phòng phù hợp và bắt đầu những lượt chơi thật rộn ràng.",
            fontPhu,
            chuPhu,
            new RectangleF(112, 520, 438, 62));

        Rectangle chip1 = new(110, 606, 168, 34);
        Rectangle chip2 = new(290, 606, 168, 34);
        Rectangle chip3 = new(110, 650, 158, 34);
        VeChip(e.Graphics, chip1, nenChip, chuChip, fontChip, "tạo phòng riêng");
        VeChip(e.Graphics, chip2, nenChip, chuChip, fontChip, "chơi cùng hội");
        VeChip(e.Graphics, chip3, nenChip, chuChip, fontChip, "chat rộn ràng");

        // Cụm ô chữ nổi giúp khoảng giữa bớt trống và bám đúng chủ đề nối chữ.
        VeCumChuTrangTri(
            e.Graphics,
            new Point(560, 248),
            nenChu1,
            nenChu2,
            nenChu3,
            nenChu4,
            chuTrangTri,
            duongCham,
            fontChuLon,
            fontChuNho);
    }

    private void VeNenSang(object? sender, PaintEventArgs e)
    {
        if (sender is not Control control)
        {
            return;
        }

        using LinearGradientBrush brush = new(
            control.ClientRectangle,
            Color.FromArgb(249, 246, 252),
            Color.FromArgb(242, 246, 251),
            LinearGradientMode.Vertical);

        e.Graphics.FillRectangle(brush, control.ClientRectangle);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using SolidBrush diemXanh = new(Color.FromArgb(16, 110, 124, 205));
        using SolidBrush diemSang = new(Color.FromArgb(16, 238, 183, 200));
        using SolidBrush diemVang = new(Color.FromArgb(12, 239, 214, 160));
        e.Graphics.FillEllipse(diemXanh, 1120, 44, 240, 240);
        e.Graphics.FillEllipse(diemSang, 930, 540, 320, 320);
        e.Graphics.FillEllipse(diemVang, 120, 80, 200, 200);
    }

    private void VeChip(Graphics graphics, Rectangle rectangle, Brush nen, Brush chu, Font font, string text)
    {
        using GraphicsPath path = TaoDuongBoTron(rectangle, 15);

        graphics.FillPath(nen, path);
        graphics.DrawString(text, font, chu, rectangle, new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        });
    }

    // Vẽ cụm biểu tượng chữ cái được nối với nhau để nhấn đúng chủ đề "nối từ".
    private void VeCumIconNoiChu(
        Graphics graphics,
        Point gocTrai,
        Pen duongNoi,
        Brush nen1,
        Brush nen2,
        Brush nen3,
        Brush chu,
        Font font)
    {
        Rectangle o1 = new(gocTrai.X, gocTrai.Y, 42, 42);
        Rectangle o2 = new(gocTrai.X + 54, gocTrai.Y + 8, 42, 42);
        Rectangle o3 = new(gocTrai.X + 108, gocTrai.Y, 42, 42);

        graphics.DrawLine(duongNoi, o1.Right - 2, o1.Y + 21, o2.X + 2, o2.Y + 21);
        graphics.DrawLine(duongNoi, o2.Right - 2, o2.Y + 21, o3.X + 2, o3.Y + 21);

        VeOChu(graphics, o1, nen1, chu, font, "N");
        VeOChu(graphics, o2, nen2, chu, font, "Ố");
        VeOChu(graphics, o3, nen3, chu, font, "I");
    }

    private void VeOChu(Graphics graphics, Rectangle rectangle, Brush nen, Brush chu, Font font, string kyTu)
    {
        using GraphicsPath path = TaoDuongBoTron(rectangle, 14);
        using Pen vien = new(Color.FromArgb(160, 255, 255, 255), 1.1F);
        graphics.FillPath(nen, path);
        graphics.DrawPath(vien, path);
        graphics.DrawString(
            kyTu,
            font,
            chu,
            rectangle,
            new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            });
    }

    // Vẽ ngôi sao 4 cánh nhỏ để phần minh họa trông dễ thương và bớt trống.
    private void VeNgoiSao(Graphics graphics, Brush brush, int centerX, int centerY, int size)
    {
        Point[] points =
        [
            new(centerX, centerY - size),
            new(centerX + size / 3, centerY - size / 3),
            new(centerX + size, centerY),
            new(centerX + size / 3, centerY + size / 3),
            new(centerX, centerY + size),
            new(centerX - size / 3, centerY + size / 3),
            new(centerX - size, centerY),
            new(centerX - size / 3, centerY - size / 3)
        ];

        graphics.FillPolygon(brush, points);
    }

    // Vẽ cụm ô chữ để trang trí khoảng trống giữa hero và khung thao tác.
    private void VeCumChuTrangTri(
        Graphics graphics,
        Point gocTrai,
        Brush nen1,
        Brush nen2,
        Brush nen3,
        Brush nen4,
        Brush chu,
        Pen duongNoi,
        Font fontLon,
        Font fontNho)
    {
        Rectangle o1 = new(gocTrai.X, gocTrai.Y, 72, 72);
        Rectangle o2 = new(gocTrai.X + 92, gocTrai.Y + 26, 58, 58);
        Rectangle o3 = new(gocTrai.X + 22, gocTrai.Y + 112, 58, 58);
        Rectangle o4 = new(gocTrai.X + 114, gocTrai.Y + 122, 72, 72);
        Rectangle o5 = new(gocTrai.X + 52, gocTrai.Y + 210, 58, 58);

        graphics.DrawBezier(
            duongNoi,
            new Point(o1.Right - 6, o1.Y + 36),
            new Point(o1.Right + 36, o1.Y + 18),
            new Point(o2.X - 18, o2.Bottom + 14),
            new Point(o2.X + 6, o2.Y + 28));

        graphics.DrawBezier(
            duongNoi,
            new Point(o2.X + 16, o2.Bottom - 2),
            new Point(o2.X + 6, o2.Bottom + 36),
            new Point(o3.Right - 12, o3.Y - 16),
            new Point(o3.X + 18, o3.Y + 6));

        graphics.DrawBezier(
            duongNoi,
            new Point(o3.Right - 4, o3.Y + 28),
            new Point(o3.Right + 34, o3.Y + 10),
            new Point(o4.X - 24, o4.Y + 36),
            new Point(o4.X + 8, o4.Y + 24));

        graphics.DrawBezier(
            duongNoi,
            new Point(o4.X + 26, o4.Bottom - 2),
            new Point(o4.X + 12, o4.Bottom + 34),
            new Point(o5.Right - 12, o5.Y - 18),
            new Point(o5.X + 22, o5.Y + 10));

        VeOChuTrangTri(graphics, o1, nen1, chu, fontLon, "N");
        VeOChuTrangTri(graphics, o2, nen2, chu, fontNho, "Ố");
        VeOChuTrangTri(graphics, o3, nen3, chu, fontNho, "I");
        VeOChuTrangTri(graphics, o4, nen4, chu, fontLon, "T");
        VeOChuTrangTri(graphics, o5, nen2, chu, fontNho, "Ừ");
    }

    private void VeOChuTrangTri(Graphics graphics, Rectangle rectangle, Brush nen, Brush chu, Font font, string kyTu)
    {
        using GraphicsPath path = TaoDuongBoTron(rectangle, 18);
        using Pen vien = new(Color.FromArgb(132, 255, 255, 255), 1.1F);
        graphics.FillPath(nen, path);
        graphics.DrawPath(vien, path);
        graphics.DrawString(
            kyTu,
            font,
            chu,
            rectangle,
            new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            });
    }

    // Vẽ lại phần minh họa đầu trang theo một cụm thống nhất để tổng thể đỡ rời rạc.
    private void VeNenTaiKhoanMoi(object? sender, PaintEventArgs e)
    {
        if (sender is not Control control)
        {
            return;
        }

        using LinearGradientBrush brush = new(
            control.ClientRectangle,
            Color.FromArgb(98, 109, 184),
            Color.FromArgb(167, 189, 225),
            LinearGradientMode.ForwardDiagonal);

        e.Graphics.FillRectangle(brush, control.ClientRectangle);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using SolidBrush phuSang = new(Color.FromArgb(22, 255, 255, 255));
        using SolidBrush phuHong = new(Color.FromArgb(20, 240, 193, 212));
        using SolidBrush phuVang = new(Color.FromArgb(18, 243, 224, 174));
        using SolidBrush phuTim = new(Color.FromArgb(18, 192, 186, 236));
        using SolidBrush saoNho = new(Color.FromArgb(170, 255, 247, 210));
        using SolidBrush chuTrang = new(Color.FromArgb(248, 250, 255));
        using SolidBrush chuPhu = new(Color.FromArgb(227, 232, 247));
        using SolidBrush nenChip = new(Color.FromArgb(178, 248, 245, 255));
        using SolidBrush chuChip = new(Color.FromArgb(74, 81, 126));
        using SolidBrush nenNhan = new(Color.FromArgb(64, 255, 255, 255));
        using SolidBrush chuNhan = new(Color.FromArgb(78, 88, 148));
        using SolidBrush nenIcon1 = new(Color.FromArgb(255, 246, 228, 235));
        using SolidBrush nenIcon2 = new(Color.FromArgb(255, 236, 243, 255));
        using SolidBrush nenIcon3 = new(Color.FromArgb(255, 233, 247, 228));
        using SolidBrush chuIcon = new(Color.FromArgb(84, 92, 148));
        using SolidBrush nenChu1 = new(Color.FromArgb(255, 240, 246, 255));
        using SolidBrush nenChu2 = new(Color.FromArgb(255, 255, 239, 246));
        using SolidBrush nenChu3 = new(Color.FromArgb(255, 244, 238, 255));
        using SolidBrush nenMay = new(Color.FromArgb(76, 255, 255, 255));
        using SolidBrush bongMay = new(Color.FromArgb(18, 83, 95, 159));
        using SolidBrush chuTrangTri = new(Color.FromArgb(78, 85, 132));
        using Pen vienNhe = new(Color.FromArgb(32, 255, 255, 255), 1.2F);
        using Pen duongNoi = new(Color.FromArgb(180, 236, 240, 252), 3.2F);
        using Pen duongCham = new(Color.FromArgb(132, 240, 242, 248), 2.6F);
        using Pen netMat = new(Color.FromArgb(112, 123, 186), 2F);
        using Font fontTieuDe = new("Bahnschrift SemiBold", 33F, FontStyle.Bold);
        using Font fontPhu = new("Segoe UI Semibold", 12.5F, FontStyle.Bold);
        using Font fontChip = new("Segoe UI Semibold", 10F, FontStyle.Bold);
        using Font fontNhan = new("Segoe UI Semibold", 10.5F, FontStyle.Bold);
        using Font fontIcon = new("Bahnschrift SemiBold", 18F, FontStyle.Bold);
        using Font fontChuLon = new("Bahnschrift SemiBold", 21F, FontStyle.Bold);
        using Font fontChuNho = new("Bahnschrift SemiBold", 17F, FontStyle.Bold);

        duongNoi.StartCap = LineCap.Round;
        duongNoi.EndCap = LineCap.Round;
        duongCham.DashPattern = [2.4f, 5.2f];
        duongCham.StartCap = LineCap.Round;
        duongCham.EndCap = LineCap.Round;
        netMat.StartCap = LineCap.Round;
        netMat.EndCap = LineCap.Round;

        e.Graphics.FillEllipse(phuTim, 26, 480, 280, 280);
        e.Graphics.FillEllipse(phuSang, 170, 94, 160, 160);
        e.Graphics.FillEllipse(phuHong, 520, 46, 220, 220);
        e.Graphics.FillEllipse(phuVang, 580, 420, 180, 180);
        VeNgoiSao(e.Graphics, saoNho, 162, 122, 10);
        VeNgoiSao(e.Graphics, saoNho, 462, 152, 8);
        VeNgoiSao(e.Graphics, saoNho, 420, 560, 9);

        Rectangle nhanNho = new(110, 170, 238, 34);
        using GraphicsPath pathNhan = TaoDuongBoTron(nhanNho, 17);
        e.Graphics.FillPath(nenNhan, pathNhan);
        e.Graphics.DrawPath(vienNhe, pathNhan);
        e.Graphics.DrawString(
            "Game nho nhỏ dịu dàng cho cả nhóm",
            fontNhan,
            chuNhan,
            nhanNho,
            new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            });

        VeCumIconNoiChuMoi(
            e.Graphics,
            new Point(112, 226),
            duongNoi,
            nenIcon1,
            nenIcon2,
            nenIcon3,
            chuIcon,
            fontIcon);

        e.Graphics.DrawString(
            "Nối một từ,\nvui cả phòng",
            fontTieuDe,
            chuTrang,
            new RectangleF(108, 286, 418, 110));

        e.Graphics.DrawString(
            "Tạo biệt danh dễ thương, chọn một phòng phù hợp và bắt đầu những lượt chơi thật rộn ràng.",
            fontPhu,
            chuPhu,
            new RectangleF(112, 408, 454, 66));

        Rectangle chip1 = new(110, 492, 168, 34);
        Rectangle chip2 = new(290, 492, 168, 34);
        Rectangle chip3 = new(110, 536, 168, 34);
        VeChip(e.Graphics, chip1, nenChip, chuChip, fontChip, "tạo phòng riêng");
        VeChip(e.Graphics, chip2, nenChip, chuChip, fontChip, "chơi cùng hội");
        VeChip(e.Graphics, chip3, nenChip, chuChip, fontChip, "chat rộn ràng");

        VeMinhHoaNoiChuMoi(
            e.Graphics,
            new Point(520, 196),
            nenChu1,
            nenChu2,
            nenChu3,
            nenMay,
            bongMay,
            chuTrangTri,
            duongCham,
            netMat,
            fontChuLon,
            fontChuNho);
    }

    // Dùng icon nối chữ gọn hơn để phần đầu không bị tách thành quá nhiều lớp trang trí.
    private void VeCumIconNoiChuMoi(
        Graphics graphics,
        Point gocTrai,
        Pen duongNoi,
        Brush nen1,
        Brush nen2,
        Brush nen3,
        Brush chu,
        Font font)
    {
        Rectangle o1 = new(gocTrai.X, gocTrai.Y, 40, 40);
        Rectangle o2 = new(gocTrai.X + 52, gocTrai.Y + 8, 40, 40);
        Rectangle o3 = new(gocTrai.X + 104, gocTrai.Y, 40, 40);

        graphics.DrawLine(duongNoi, o1.Right - 2, o1.Y + 20, o2.X + 2, o2.Y + 20);
        graphics.DrawLine(duongNoi, o2.Right - 2, o2.Y + 20, o3.X + 2, o3.Y + 20);

        VeOChu(graphics, o1, nen1, chu, font, "N");
        VeOChu(graphics, o2, nen2, chu, font, "Ố");
        VeOChu(graphics, o3, nen3, chu, font, "I");
    }

    // Cụm mây và ô chữ giúp khoảng giữa có điểm nhấn nhưng vẫn nhìn như một minh họa liền khối.
    private void VeMinhHoaNoiChuMoi(
        Graphics graphics,
        Point gocTrai,
        Brush nen1,
        Brush nen2,
        Brush nen3,
        Brush nenMay,
        Brush bongMay,
        Brush chu,
        Pen duongNoi,
        Pen netMat,
        Font fontLon,
        Font fontNho)
    {
        Rectangle bongDo = new(gocTrai.X + 66, gocTrai.Y + 188, 232, 96);
        Rectangle thanMay = new(gocTrai.X + 40, gocTrai.Y + 150, 252, 108);
        Rectangle o1 = new(gocTrai.X + 18, gocTrai.Y + 36, 82, 82);
        Rectangle o2 = new(gocTrai.X + 130, gocTrai.Y + 2, 68, 68);
        Rectangle o3 = new(gocTrai.X + 116, gocTrai.Y + 106, 68, 68);
        Rectangle o4 = new(gocTrai.X + 220, gocTrai.Y + 56, 82, 82);

        VeDamMayMoi(graphics, bongDo, bongMay);
        VeDamMayMoi(graphics, thanMay, nenMay);

        graphics.DrawBezier(
            duongNoi,
            new Point(o1.Right - 8, o1.Y + 42),
            new Point(o1.Right + 24, o1.Y + 14),
            new Point(o2.X - 24, o2.Bottom + 18),
            new Point(o2.X + 10, o2.Y + 26));

        graphics.DrawBezier(
            duongNoi,
            new Point(o2.X + 14, o2.Bottom - 2),
            new Point(o2.X + 8, o2.Bottom + 34),
            new Point(o3.Right - 10, o3.Y - 12),
            new Point(o3.X + 20, o3.Y + 4));

        graphics.DrawBezier(
            duongNoi,
            new Point(o3.Right - 6, o3.Y + 34),
            new Point(o3.Right + 26, o3.Y + 18),
            new Point(o4.X - 22, o4.Y + 42),
            new Point(o4.X + 12, o4.Y + 28));

        VeTheChuMoi(graphics, o1, nen1, chu, fontLon, "N");
        VeTheChuMoi(graphics, o2, nen2, chu, fontNho, "Ố");
        VeTheChuMoi(graphics, o3, nen3, chu, fontNho, "I");
        VeTheChuMoi(graphics, o4, nen2, chu, fontLon, "Từ");

        // Mặt cười nhỏ giúp cụm mây thân thiện hơn mà không làm rối bố cục.
        graphics.DrawLine(netMat, gocTrai.X + 144, gocTrai.Y + 190, gocTrai.X + 151, gocTrai.Y + 190);
        graphics.DrawLine(netMat, gocTrai.X + 184, gocTrai.Y + 190, gocTrai.X + 191, gocTrai.Y + 190);
        graphics.DrawArc(netMat, gocTrai.X + 152, gocTrai.Y + 198, 32, 18, 8, 164);
        VeNgoiSao(graphics, nen3, gocTrai.X + 40, gocTrai.Y + 176, 7);
        VeNgoiSao(graphics, nen2, gocTrai.X + 304, gocTrai.Y + 142, 8);
    }

    private void VeDamMayMoi(Graphics graphics, Rectangle rectangle, Brush nen)
    {
        graphics.FillEllipse(nen, rectangle.X + 8, rectangle.Y + 24, 92, 56);
        graphics.FillEllipse(nen, rectangle.X + 66, rectangle.Y + 2, 104, 76);
        graphics.FillEllipse(nen, rectangle.X + 146, rectangle.Y + 18, 90, 58);
        graphics.FillEllipse(nen, rectangle.X + 34, rectangle.Y + 44, 172, 46);
    }

    private void VeTheChuMoi(Graphics graphics, Rectangle rectangle, Brush nen, Brush chu, Font font, string kyTu)
    {
        using GraphicsPath path = TaoDuongBoTron(rectangle, 18);
        using Pen vien = new(Color.FromArgb(142, 255, 255, 255), 1.2F);
        graphics.FillPath(nen, path);
        graphics.DrawPath(vien, path);
        graphics.DrawString(
            kyTu,
            font,
            chu,
            rectangle,
            new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            });
    }

    private GraphicsPath TaoDuongBoTron(Rectangle rectangle, int radius)
    {
        GraphicsPath path = new();
        int diameter = radius * 2;
        path.AddArc(rectangle.X, rectangle.Y, diameter, diameter, 180, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Y, diameter, diameter, 270, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rectangle.X, rectangle.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }

    private void HienThiTrangThai(Label? label, string noiDung, Color mau)
    {
        if (label is null)
        {
            return;
        }

        label.Text = noiDung;
        label.ForeColor = mau;
        label.Visible = !string.IsNullOrWhiteSpace(noiDung);
    }

    // Thiết lập trạng thái mặc định để giao diện thân thiện ngay cả khi chưa có nội dung.
    private void KhoiTaoTrangThaiBanDau()
    {
        lstPhongNoiBat.Items.Clear();
        lstNguoiChoi.Items.Clear();
        lstChatTrongPhong.Items.Clear();
        lblTrangThaiPhong.Text = "Hiện chưa có phòng nào sẵn sàng.";
        lblTrangThaiNguoiChoi.Text = "Chưa có người chơi trong phòng.";
        lblTrangThaiChat.Text = "Tin nhắn sẽ xuất hiện tại đây khi mọi người bắt đầu trò chuyện.";
        lblThongTinChiTiet.Text = "Thông tin lượt chơi, thời gian và số lượng người tham gia sẽ hiện ở đây để bạn theo dõi thật dễ.";
        HienThiTrangThai(_lblTrangThaiDangNhap, string.Empty, _mauLoi);
        HienThiTrangThai(_lblTrangThaiDangKy, string.Empty, _mauLoi);
        HienThiTrangThai(_lblTrangThaiChoiNhanh, string.Empty, _mauLoi);
    }

    // Chuyển tab và đồng thời cập nhật trạng thái nút để người dùng dễ nhận biết màn hiện tại.
    private void HienManHinh(TabPage tabPage)
    {
        if (tabPage == tabDangNhap || tabPage == tabDangKy)
        {
            tabPage = tabChoiNhanh;
        }

        tabMain.SelectedTab = tabPage;
        ApDungNutChuyenTab(btnTabDangNhap, tabPage == tabDangNhap);
        ApDungNutChuyenTab(btnTabDangKy, tabPage == tabDangKy);
        ApDungNutChuyenTab(btnTabChoiNhanh, tabPage == tabChoiNhanh);
    }

    // Cập nhật tên hiển thị ở những vị trí chính sau khi người chơi nhập xong.
    private void CapNhatTenNguoiChoi(string tenMacDinh = "PlayerOne")
    {
        string tenNguoiChoi = string.IsNullOrWhiteSpace(txtChoiNhanhTenNguoiChoi.Text)
            ? tenMacDinh
            : txtChoiNhanhTenNguoiChoi.Text.Trim();

        _tenNguoiChoiHienTai = tenNguoiChoi;
        lblTenNguoiChoiSanh.Text = tenNguoiChoi;
        lblLuotChoi.Text = $"Lượt của: {tenNguoiChoi}";
        lblThongTinPhong.Text = "Bạn chưa vào phòng nào";
    }

    // Cập nhật trạng thái menu trái để người dùng biết mình đang thao tác ở nhóm nào.
    private void CapNhatTrangThaiMenuSanhCho(Button nutDangChon)
    {
        ApDungNutMenu(btnMenuSanhCho, nutDangChon == btnMenuSanhCho);
        ApDungNutMenu(btnMenuTaoPhong, nutDangChon == btnMenuTaoPhong);
        ApDungNutMenu(btnMenuThamGiaPhong, nutDangChon == btnMenuThamGiaPhong);
    }

    private void MoSanhCho()
    {
        CapNhatTrangThaiMenuSanhCho(btnMenuSanhCho);
        HienManHinh(tabSanhCho);
        LamMoiDanhSachPhong();
    }

    // Tạo phòng ngay trên client để luồng giao diện có thể chạy và test độc lập.
    private void MoQuyTrinhTaoPhong()
    {
        CapNhatTrangThaiMenuSanhCho(btnMenuTaoPhong);

        using HopThoaiTaoPhong hopThoai = new(_tenNguoiChoiHienTai, XemTruocMaPhongTiepTheo());
        if (hopThoai.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        PhongClient phong = new()
        {
            MaPhong = hopThoai.MaPhong,
            MatKhau = hopThoai.MatKhau,
            TenPhong = hopThoai.TenPhong,
            CheDo = hopThoai.CheDo,
            SoNguoiToiDa = hopThoai.SoNguoiToiDa,
            SoGiayMoiLuot = hopThoai.SoGiayMoiLuot,
            LaPhongRieng = hopThoai.LaPhongRieng
        };

        if (_danhSachPhong.Any(item => item.MaPhong.Equals(phong.MaPhong, StringComparison.OrdinalIgnoreCase)))
        {
            lblTrangThaiPhong.Text = "Mã phòng này đã tồn tại. Bạn hãy chọn mã khác nhé.";
            lblTrangThaiPhong.Visible = true;
            return;
        }

        _soThuTuPhong++;

        GanNguoiChoiVaoPhong(phong, _tenNguoiChoiHienTai, true);
        ThemTinNhanHeThong(phong, $"Phòng {phong.MaPhong} vừa được tạo.");

        _danhSachPhong.Insert(0, phong);
        LamMoiDanhSachPhong(phong.MaPhong);
        VaoPhong(phong, true);
    }

    private void MoQuyTrinhThamGiaPhong()
    {
        CapNhatTrangThaiMenuSanhCho(btnMenuThamGiaPhong);
        ThamGiaPhongDangChon();
    }

    private void ThamGiaPhongDangChon()
    {
        if (lstPhongNoiBat.SelectedItem is PhongClient phongDaChon)
        {
            using HopThoaiNhapMaPhong hopThoaiDaChon = new(phongDaChon.MaPhong, phongDaChon.LaPhongRieng);
            if (hopThoaiDaChon.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            if (phongDaChon.LaPhongRieng &&
                !string.Equals(phongDaChon.MatKhau, hopThoaiDaChon.MatKhau, StringComparison.Ordinal))
            {
                lblTrangThaiPhong.Text = "Mật khẩu phòng chưa đúng. Bạn hãy thử lại nhé.";
                lblTrangThaiPhong.Visible = true;
                return;
            }

            VaoPhong(phongDaChon, false);
            return;
        }

        if (_danhSachPhong.Count == 0)
        {
            lblTrangThaiPhong.Text = "Bạn chưa có phòng nào để tham gia. Hãy tạo phòng trước nhé.";
            lblTrangThaiPhong.Visible = true;
            return;
        }

        using HopThoaiNhapMaPhong hopThoai = new(null, true);
        if (hopThoai.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        PhongClient? phongTheoMa = _danhSachPhong.FirstOrDefault(
            phong => phong.MaPhong.Equals(hopThoai.MaPhong, StringComparison.OrdinalIgnoreCase));

        if (phongTheoMa is null)
        {
            lblTrangThaiPhong.Text = "Mã phòng này chưa có trong danh sách hiện tại.";
            lblTrangThaiPhong.Visible = true;
            return;
        }

        if (phongTheoMa.LaPhongRieng &&
            !string.Equals(phongTheoMa.MatKhau, hopThoai.MatKhau, StringComparison.Ordinal))
        {
            lblTrangThaiPhong.Text = "Mật khẩu phòng chưa đúng. Bạn hãy thử lại nhé.";
            lblTrangThaiPhong.Visible = true;
            return;
        }

        VaoPhong(phongTheoMa, false);
    }

    private void VaoPhong(PhongClient phong, bool laPhongMoi)
    {
        if (phong.NguoiChoi.Count >= phong.SoNguoiToiDa &&
            phong.NguoiChoi.All(nguoiChoi => !nguoiChoi.TenHienThi.Equals(_tenNguoiChoiHienTai, StringComparison.OrdinalIgnoreCase)))
        {
            lblTrangThaiPhong.Text = "Phòng này đã đủ người. Bạn hãy chọn phòng khác nhé.";
            lblTrangThaiPhong.Visible = true;
            return;
        }

        GanNguoiChoiVaoPhong(phong, _tenNguoiChoiHienTai, laPhongMoi && phong.NguoiChoi.Count == 1);
        _phongHienTai = phong;

        if (!laPhongMoi)
        {
            ThemTinNhanHeThong(phong, $"{_tenNguoiChoiHienTai} vừa vào phòng.");
        }

        CapNhatGiaoDienPhongHienTai();
        HienManHinh(tabPhongChoi);
    }

    private void CapNhatGiaoDienPhongHienTai()
    {
        if (_phongHienTai is null)
        {
            DatLaiTrangThaiPhongHienTai();
            return;
        }

        lblThongTinPhong.Text = $"{_phongHienTai.TenPhong} • {_phongHienTai.MaPhong}";
        lblLuotChoi.Text = $"Lượt của: {_tenNguoiChoiHienTai}";
        lblBoDem.Text = $"{_phongHienTai.SoGiayMoiLuot} GIÂY";
        lblTuHienTai.Text = _phongHienTai.TuHienTai;
        lblHuongDanNoiTu.Text = _phongHienTai.TrangThai;
        lblThongTinChiTiet.Text =
            $"THÔNG TIN TRẬN ĐẤU{Environment.NewLine}" +
            $"Chế độ: {_phongHienTai.CheDo} | Thời gian: {_phongHienTai.SoGiayMoiLuot} giây/lượt{Environment.NewLine}" +
            $"Ngôn ngữ: Tiếng Việt | Số người tối đa: {_phongHienTai.SoNguoiToiDa}";

        lstNguoiChoi.Items.Clear();
        foreach (NguoiChoiPhong nguoiChoi in _phongHienTai.NguoiChoi)
        {
            lstNguoiChoi.Items.Add(nguoiChoi);
        }

        lstChatTrongPhong.Items.Clear();
        foreach (string tinNhan in _phongHienTai.TinNhan)
        {
            lstChatTrongPhong.Items.Add(tinNhan);
        }

        if (lstChatTrongPhong.Items.Count > 0)
        {
            lstChatTrongPhong.TopIndex = lstChatTrongPhong.Items.Count - 1;
        }

        LamMoiDanhSachPhong(_phongHienTai.MaPhong);
        CapNhatTrangThaiDanhSach();
    }

    private void DatLaiTrangThaiPhongHienTai()
    {
        lblThongTinPhong.Text = "Bạn chưa vào phòng nào";
        lblLuotChoi.Text = $"Lượt của: {_tenNguoiChoiHienTai}";
        lblBoDem.Text = "15 GIÂY";
        lblTuHienTai.Text = "--";
        lblHuongDanNoiTu.Text = "Khi trò chơi bắt đầu, từ đầu tiên sẽ xuất hiện tại đây.";
        lblThongTinChiTiet.Text =
            $"THÔNG TIN TRẬN ĐẤU{Environment.NewLine}" +
            "Chế độ: Chưa xác định | Thời gian: Chưa xác định" + Environment.NewLine +
            "Ngôn ngữ: Tiếng Việt | Số người tối đa: Chưa xác định";
        lstNguoiChoi.Items.Clear();
        lstChatTrongPhong.Items.Clear();
        CapNhatTrangThaiDanhSach();
    }

    private void LamMoiDanhSachPhong(string? maPhongDangChon = null)
    {
        lstPhongNoiBat.BeginUpdate();
        lstPhongNoiBat.Items.Clear();

        foreach (PhongClient phong in _danhSachPhong
                     .OrderByDescending(item => item.NguoiChoi.Count)
                     .ThenBy(item => item.TenPhong))
        {
            lstPhongNoiBat.Items.Add(phong);
        }

        if (!string.IsNullOrWhiteSpace(maPhongDangChon))
        {
            for (int i = 0; i < lstPhongNoiBat.Items.Count; i++)
            {
                if (lstPhongNoiBat.Items[i] is PhongClient phong &&
                    phong.MaPhong.Equals(maPhongDangChon, StringComparison.OrdinalIgnoreCase))
                {
                    lstPhongNoiBat.SelectedIndex = i;
                    break;
                }
            }
        }

        lstPhongNoiBat.EndUpdate();
        CapNhatTrangThaiDanhSach();
    }

    private void GanNguoiChoiVaoPhong(PhongClient phong, string tenNguoiChoi, bool laChuPhong)
    {
        NguoiChoiPhong? nguoiChoiTonTai = phong.NguoiChoi.FirstOrDefault(
            item => item.TenHienThi.Equals(tenNguoiChoi, StringComparison.OrdinalIgnoreCase));

        if (nguoiChoiTonTai is null)
        {
            phong.NguoiChoi.Add(new NguoiChoiPhong
            {
                TenHienThi = tenNguoiChoi,
                LaChuPhong = laChuPhong || phong.NguoiChoi.Count == 0
            });
            return;
        }

        if (laChuPhong)
        {
            nguoiChoiTonTai.LaChuPhong = true;
        }
    }

    private void BoNguoiChoiKhoiPhong(PhongClient phong, string tenNguoiChoi)
    {
        NguoiChoiPhong? nguoiChoiCanXoa = phong.NguoiChoi.FirstOrDefault(
            item => item.TenHienThi.Equals(tenNguoiChoi, StringComparison.OrdinalIgnoreCase));

        if (nguoiChoiCanXoa is null)
        {
            return;
        }

        bool laChuPhong = nguoiChoiCanXoa.LaChuPhong;
        phong.NguoiChoi.Remove(nguoiChoiCanXoa);

        if (laChuPhong && phong.NguoiChoi.Count > 0)
        {
            phong.NguoiChoi[0].LaChuPhong = true;
        }
    }

    private void ThemTinNhanHeThong(PhongClient phong, string noiDung)
    {
        phong.TinNhan.Add($"Hệ thống: {noiDung}");
    }

    private string XemTruocMaPhongTiepTheo()
    {
        return $"NT{_soThuTuPhong:000}";
    }

    private void txtNhapTu_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
        {
            return;
        }

        e.SuppressKeyPress = true;
        btnGuiTu.PerformClick();
    }

    private void txtTinNhan_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
        {
            return;
        }

        e.SuppressKeyPress = true;
        btnGuiChat.PerformClick();
    }

    private void GuiTuNoiBo()
    {
        if (_phongHienTai is null)
        {
            lblHuongDanNoiTu.Text = "Bạn hãy vào một phòng trước khi gửi từ nhé.";
            return;
        }

        if (string.IsNullOrWhiteSpace(txtNhapTu.Text))
        {
            lblHuongDanNoiTu.Text = "Bạn hãy nhập một từ hợp lý để tiếp tục lượt chơi nhé.";
            return;
        }

        _phongHienTai.TuHienTai = txtNhapTu.Text.Trim();
        _phongHienTai.TrangThai = "Từ của bạn đã được ghi nhận. Cùng chờ lượt tiếp theo nào.";
        ThemTinNhanHeThong(_phongHienTai, $"{_tenNguoiChoiHienTai} vừa nối từ: {_phongHienTai.TuHienTai}");
        CapNhatGiaoDienPhongHienTai();
        txtNhapTu.Clear();
    }

    private void GuiChatNoiBo()
    {
        if (_phongHienTai is null)
        {
            lblTrangThaiChat.Text = "Bạn hãy vào phòng trước khi trò chuyện nhé.";
            lblTrangThaiChat.Visible = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(txtTinNhan.Text))
        {
            lblTrangThaiChat.Text = "Bạn hãy viết một tin nhắn ngắn trước khi gửi nhé.";
            lblTrangThaiChat.Visible = true;
            return;
        }

        _phongHienTai.TinNhan.Add($"{_tenNguoiChoiHienTai}: {txtTinNhan.Text.Trim()}");
        lblTrangThaiChat.Text = "Tin nhắn của bạn đã xuất hiện trong cuộc trò chuyện.";
        CapNhatGiaoDienPhongHienTai();
        txtTinNhan.Clear();
    }

    private void RoiPhongNoiBo()
    {
        if (_phongHienTai is not null)
        {
            BoNguoiChoiKhoiPhong(_phongHienTai, _tenNguoiChoiHienTai);

            if (_phongHienTai.NguoiChoi.Count == 0)
            {
                _danhSachPhong.Remove(_phongHienTai);
            }
            else
            {
                ThemTinNhanHeThong(_phongHienTai, $"{_tenNguoiChoiHienTai} vừa rời phòng.");
            }
        }

        _phongHienTai = null;
        DatLaiTrangThaiPhongHienTai();
        LamMoiDanhSachPhong();
        MoSanhCho();
    }

    private void btnTabDangNhap_Click(object sender, EventArgs e)
    {
        HienManHinh(tabDangNhap);
    }

    private void btnTabDangKy_Click(object sender, EventArgs e)
    {
        HienManHinh(tabDangKy);
    }

    private void btnTabChoiNhanh_Click(object sender, EventArgs e)
    {
        HienManHinh(tabChoiNhanh);
    }
    // cập nhật lại đăng nhập kết nối với server
    private async void btnDangNhap_Click(object sender, EventArgs e)
    {
        string username = txtDangNhapTenDangNhap.Text.Trim();
        string password = txtDangNhapMatKhau.Text.Trim();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            HienThiTrangThai(_lblTrangThaiDangNhap,
                "Bạn hãy nhập đủ tên đăng nhập và mật khẩu nhé.", _mauLoi);
            return;
        }

        // TODO: kiểm tra mật khẩu với server nếu có
        HienThiTrangThai(_lblTrangThaiDangNhap,
            "Đăng nhập xong rồi. Mời bạn vào sảnh chờ nhé.", _mauThanhCong);

        // Kết nối server, dùng username làm nickname
        await ConnectToServer(username);

        CapNhatTenNguoiChoi();
        HienManHinh(tabSanhCho);
        CapNhatTrangThaiDanhSach();
    }

// kết nối tới server 
    private async Task ConnectToServer(string nickname)
    {
        string ip = "127.0.0.1";   // Ẩn, cố định
        int port = 8888;           // Ẩn, cố định

        try
        {
            _client = new TcpClient();
            await _client.ConnectAsync(ip, port);

            _stream = _client.GetStream();
            _reader = new StreamReader(_stream, System.Text.Encoding.UTF8);
            _writer = new StreamWriter(_stream, System.Text.Encoding.UTF8) { AutoFlush = true };
            _isConnected = true;

            var packet = new Packet { Type = PacketType.Connect, Payload = nickname };
            await _writer.WriteLineAsync(packet.ToJson());

            lblStatus.Text = $"✅ Đã kết nối! Xin chào {nickname}";
            _ = Task.Run(ReceiveMessagesAsync);
        }
        catch (Exception ex)
        {
            lblStatus.Text = "❌ Kết nối thất bại!";
            _isConnected = false;
            MessageBox.Show($"Lỗi: {ex.Message}");
        }
    }


    private void btnMoDangNhapTuDangKy_Click(object sender, EventArgs e)
    {
        HienManHinh(tabDangNhap);
    }

    private void btnMoDangNhapTuChoiNhanh_Click(object sender, EventArgs e)
    {
        HienManHinh(tabDangNhap);
    }

    private void btnDangKy_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtDangKyTenDangNhap.Text) ||
            string.IsNullOrWhiteSpace(txtDangKyMatKhau.Text) ||
            string.IsNullOrWhiteSpace(txtDangKyNhapLaiMatKhau.Text))
        {
            HienThiTrangThai(_lblTrangThaiDangKy, "Bạn hãy điền đủ thông tin trước khi tạo tài khoản nhé.", _mauLoi);
                return;
        }

        if (txtDangKyMatKhau.Text != txtDangKyNhapLaiMatKhau.Text)
        {
            HienThiTrangThai(_lblTrangThaiDangKy, "Hai ô mật khẩu chưa giống nhau, mình kiểm tra lại nhé.", _mauLoi);
                return;
        }

        HienThiTrangThai(_lblTrangThaiDangKy, "Xong rồi, tài khoản của bạn đã sẵn sàng để tiếp tục.", _mauThanhCong);
        txtDangNhapTenDangNhap.Text = txtDangKyTenDangNhap.Text.Trim();
        HienManHinh(tabDangNhap);
    }

    // cập nhật username chơi nhanh với server
    private async void btnBatDauChoiNhanh_Click(object sender, EventArgs e)
    {
        string quickName = txtChoiNhanhTenNguoiChoi.Text.Trim();

        if (string.IsNullOrWhiteSpace(quickName))
        {
            HienThiTrangThai(_lblTrangThaiChoiNhanh,
                "Bạn hãy nhập tên hiển thị trước khi bắt đầu nhé.", _mauLoi);
            return;
        }

        HienThiTrangThai(_lblTrangThaiChoiNhanh,
            "Đã lưu tên hiển thị. Chuẩn bị vào chơi thôi.", _mauThanhCong);

        // Cập nhật tên người chơi trong giao diện
        CapNhatTenNguoiChoi(quickName);

        // Kết nối server, gửi tên chơi nhanh như nickname
        await ConnectToServer(quickName);

        // Sau khi kết nối thành công thì chuyển sang sảnh chờ
        HienManHinh(tabSanhCho);
        CapNhatTrangThaiDanhSach();
    }

    // Mô phỏng luồng tạo phòng để người dùng nhìn thấy trạng thái rõ ràng hơn.
    private void btnTaoPhong_Click(object? sender, EventArgs e)
    {
        lblThongTinPhong.Text = "Phòng mới của bạn đã sẵn sàng";
        lblLuotChoi.Text = $"Lượt của: {lblTenNguoiChoiSanh.Text}";
        lblTuHienTai.Text = "--";
        lblHuongDanNoiTu.Text = "Hãy mời thêm bạn bè vào để bắt đầu thật vui nhé.";
        lstNguoiChoi.Items.Clear();
        lstNguoiChoi.Items.Add($"{lblTenNguoiChoiSanh.Text} (chủ phòng)");
        lblTrangThaiPhong.Text = "Phòng mới đã được tạo. Bạn có thể chia sẻ mã phòng với bạn bè ngay bây giờ.";
        CapNhatTrangThaiDanhSach();
        HienManHinh(tabPhongChoi);
    }

    private void btnThamGiaPhong_Click(object? sender, EventArgs e)
    {
        if (lstPhongNoiBat.SelectedItem is null)
        {
            lblTrangThaiPhong.Text = "Chọn một phòng bạn thích rồi mình đưa bạn vào ngay.";
            lblTrangThaiPhong.Visible = true;
            return;
        }

        lblThongTinPhong.Text = $"Bạn đang ở {lstPhongNoiBat.SelectedItem}";
        lblHuongDanNoiTu.Text = "Mọi người đang vào phòng. Lượt đầu tiên sẽ bắt đầu rất sớm.";
        CapNhatTrangThaiDanhSach();
        HienManHinh(tabPhongChoi);
    }

    private void btnGuiTu_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtNhapTu.Text))
        {
            lblHuongDanNoiTu.Text = "Nhập một từ thật hợp lý để tiếp tục lượt chơi nhé.";
            return;
        }

        lblTuHienTai.Text = txtNhapTu.Text.Trim();
        lblHuongDanNoiTu.Text = "Từ của bạn đã được gửi rồi. Cùng chờ lượt tiếp theo nào.";
        txtNhapTu.Clear();
    }

    private void btnGuiChat_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtTinNhan.Text))
        {
            lblTrangThaiChat.Text = "Viết một tin nhắn ngắn rồi gửi cho mọi người nhé.";
            lblTrangThaiChat.Visible = true;
            return;
        }

        lstChatTrongPhong.Items.Add($"{lblTenNguoiChoiSanh.Text}: {txtTinNhan.Text.Trim()}");
        lblTrangThaiChat.Text = "Tin nhắn của bạn đã xuất hiện trong cuộc trò chuyện.";
        CapNhatTrangThaiDanhSach();
        lstChatTrongPhong.TopIndex = lstChatTrongPhong.Items.Count - 1;
        txtTinNhan.Clear();
    }

    private void btnRoiPhong_Click(object? sender, EventArgs e)
    {
        lstNguoiChoi.Items.Clear();
        lstChatTrongPhong.Items.Clear();
        lblThongTinPhong.Text = "Bạn chưa vào phòng nào";
        lblTuHienTai.Text = "--";
        lblHuongDanNoiTu.Text = "Khi trò chơi bắt đầu, từ đầu tiên sẽ xuất hiện tại đây.";
        CapNhatTrangThaiDanhSach();
        HienManHinh(tabSanhCho);
    }

    // Bật hoặc ẩn các trạng thái rỗng để giao diện luôn gợi ý được việc cần làm tiếp theo.
    private void CapNhatTrangThaiDanhSach()
    {
        lblTrangThaiPhong.Visible = lstPhongNoiBat.Items.Count == 0;
        lblTrangThaiNguoiChoi.Visible = lstNguoiChoi.Items.Count == 0;
        lblTrangThaiChat.Visible = lstChatTrongPhong.Items.Count == 0;
    }

    // thêm phần quên mật khẩu (tuần 5)
    private void lnkQuenMatKhau_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        HienThiTrangThai(_lblTrangThaiDangNhap, "Tính năng quên mật khẩu chưa được hỗ trợ.", _mauLoi);
    }

    // thêm phần bạn bè (tuần 5)
    private void btnBanBe_Click(object sender, EventArgs e)
    {
        MessageBox.Show("Tính năng bạn bè đang được phát triển.", "Thông báo",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // bảng xếp hạng (tuần 5)
    private void btnBangXepHang_Click(object sender, EventArgs e) 
    {
        MessageBox.Show("Tính năng bảng xếp hạng đang được phát triển.", "Thông báo",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // tính năng cài đặt cho phép xem và chỉnh sửa ip và port

    private void btnCaiDat_Click(object sender, EventArgs e)
    {
        // Kiểm tra panel đã khởi tạo chưa
        if (pnlConnect != null)
        {
            // Đảo trạng thái hiển thị
            pnlConnect.Visible = !pnlConnect.Visible;

            // Cập nhật trạng thái để dễ theo dõi
            lblStatus.Text = pnlConnect.Visible
                ? "⚙️ Đang hiển thị cấu hình Server (IP/Port)"
                : "⚙️ Đã ẩn cấu hình Server";
        }
        else
        {
            // Nếu vì lý do nào đó panel chưa khởi tạo
            MessageBox.Show("Panel cấu hình chưa được khởi tạo!");
        }
    }

    //nút thoát
    private void btnThoat_Click(object sender, EventArgs e)
    {
        var ketQua = MessageBox.Show("Bạn có chắc muốn thoát không?", "Xác nhận thoát",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (ketQua == DialogResult.Yes)
            Application.Exit();
    }

    // Dọn dẹp khi đóng app
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _isConnected = false;
        _writer?.Close();
        _reader?.Close();
        _client?.Close();
        base.OnFormClosing(e);
    }
}
