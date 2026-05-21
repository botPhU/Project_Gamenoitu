namespace WordChain.Client;

/// <summary>
/// File này do Visual Studio tự động sinh ra khi bạn sử dụng công cụ kéo thả (Designer).
/// Đây là phần "partial class" – tức là nó kết hợp với Form1.cs để tạo thành một lớp hoàn chỉnh.
/// 
/// QUY TẮC QUAN TRỌNG:
///   - KHÔNG tự ý chỉnh sửa file này bằng tay nếu bạn đang dùng chế độ kéo thả (Design View).
///   - Mọi thay đổi trực tiếp vào file này có thể bị Visual Studio ghi đè lại.
///   - Thay vào đó, hãy mở "Form1.cs [Design]" để kéo thả và cấu hình các điều khiển (Controls).
/// </summary>
partial class Form1
{
    /// <summary>
    /// Biến chứa danh sách các thành phần (components) của form.
    /// Visual Studio sử dụng biến này để quản lý vòng đời của các điều khiển trên giao diện.
    /// Thường được dùng với Timer, ContextMenuStrip, ImageList, v.v.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Phương thức giải phóng tài nguyên (Destructor pattern của WinForms).
    /// Được gọi tự động khi form bị đóng hoặc ứng dụng kết thúc.
    /// </summary>
    /// <param name="disposing">
    ///   true  = giải phóng cả tài nguyên được quản lý (managed resources) lẫn không quản lý.
    ///   false = chỉ giải phóng tài nguyên không được quản lý (unmanaged resources).
    /// </param>
    protected override void Dispose(bool disposing)
    {
        // Nếu đang giải phóng và danh sách components tồn tại → giải phóng hết
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        // Gọi lên lớp cha (Form) để đảm bảo tài nguyên nền tảng cũng được giải phóng
        base.Dispose(disposing);
    }

    // ========================================================================================
    // KHU VỰC DO DESIGNER TỰ SINH RA MÃ NGUỒN
    // ========================================================================================
    // Khi bạn kéo thả các Control từ ToolBox vào Design View của Visual Studio,
    // Visual Studio sẽ tự động thêm mã khởi tạo vào bên trong phương thức InitializeComponent().
    // Khu vực này được đánh dấu bằng #region để có thể thu gọn trong editor.
    // ========================================================================================
    #region Windows Form Designer generated code

    /// <summary>
    /// Phương thức khởi tạo giao diện – bắt buộc phải có trong mọi WinForms Form.
    /// Được gọi tự động bởi constructor của Form1 (trong Form1.cs) thông qua: InitializeComponent();
    /// 
    /// Tại đây bạn (hoặc Designer) sẽ:
    ///   - Đặt kích thước cửa sổ (ClientSize).
    ///   - Đặt tiêu đề cửa sổ (Text).
    ///   - Đặt màu nền, font chữ mặc định (BackColor, Font).
    ///   - Thêm và cấu hình từng Control (Button, TextBox, Label, Panel, v.v.).
    ///   - Gắn các sự kiện (Event Handlers) cho các nút bấm, v.v.
    /// </summary>
    private void InitializeComponent()
    {
        // Khởi tạo container quản lý các thành phần không hiển thị (ví dụ: Timer)
        this.components = new System.ComponentModel.Container();

        // ── Thuộc tính AutoScaleMode ──────────────────────────────────────────────
        // Quy định cách form tự động điều chỉnh kích thước khi DPI hoặc font hệ thống thay đổi.
        // AutoScaleMode.Font: tỷ lệ theo kích thước font – phù hợp nhất cho desktop thông thường.
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;

        // ── Kích thước vùng nội dung (Client Area) ───────────────────────────────
        // ClientSize là phần bên trong cửa sổ (không tính thanh tiêu đề và viền).
        // Giá trị mặc định: Rộng 800px × Cao 450px.
        // TODO: Điều chỉnh kích thước phù hợp khi bạn thiết kế giao diện thực tế.
        this.ClientSize = new System.Drawing.Size(800, 450);

        // ── Tiêu đề cửa sổ ───────────────────────────────────────────────────────
        // Hiển thị trên thanh tiêu đề (Title Bar) của ứng dụng.
        this.Text = "Game Nối Từ Online";

        // ── GHI CHÚ VỀ CÁC CONTROLS ──────────────────────────────────────────────
        // Khi bạn kéo thả Controls từ ToolBox vào Design View, Visual Studio sẽ
        // tự động sinh thêm code vào đây. Ví dụ:
        //
        //   this.btnConnect = new System.Windows.Forms.Button();
        //   this.btnConnect.Text = "Kết nối";
        //   this.btnConnect.Location = new System.Drawing.Point(10, 10);
        //   this.btnConnect.Size = new System.Drawing.Size(100, 30);
        //   this.btnConnect.Click += new System.EventHandler(this.BtnConnect_Click);
        //   this.Controls.Add(this.btnConnect);
        //
        // Tham khảo Form1.cs để biết danh sách các TODO cần triển khai cho từng màn hình.
    }

    #endregion
}
