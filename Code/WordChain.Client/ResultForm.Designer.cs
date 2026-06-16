using System.Drawing;
using System.Windows.Forms;

namespace WordChain.Client;

partial class ResultForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel tlpRoot;
    private Label lblTitle;
    private Label lblWinner;
    private ListView lstFinalScores;
    private ColumnHeader colTenNguoiChoi;
    private ColumnHeader colDiem;
    private Button btnBackToLobby;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components is not null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        tlpRoot = new TableLayoutPanel();
        lblTitle = new Label();
        lblWinner = new Label();
        lstFinalScores = new ListView();
        colTenNguoiChoi = new ColumnHeader();
        colDiem = new ColumnHeader();
        btnBackToLobby = new Button();
        tlpRoot.SuspendLayout();
        SuspendLayout();
        //
        // tlpRoot
        //
        tlpRoot.ColumnCount = 1;
        tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpRoot.Controls.Add(lblTitle, 0, 0);
        tlpRoot.Controls.Add(lblWinner, 0, 1);
        tlpRoot.Controls.Add(lstFinalScores, 0, 2);
        tlpRoot.Controls.Add(btnBackToLobby, 0, 3);
        tlpRoot.Dock = DockStyle.Fill;
        tlpRoot.Location = new Point(0, 0);
        tlpRoot.Name = "tlpRoot";
        tlpRoot.Padding = new Padding(24);
        tlpRoot.RowCount = 4;
        tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
        tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 62F));
        tlpRoot.Size = new Size(720, 520);
        tlpRoot.TabIndex = 0;
        //
        // lblTitle
        //
        lblTitle.Dock = DockStyle.Fill;
        lblTitle.Font = new Font("Bahnschrift SemiBold", 22F, FontStyle.Bold);
        lblTitle.ForeColor = Color.FromArgb(48, 59, 115);
        lblTitle.Location = new Point(27, 24);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(666, 48);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "Kết quả trận đấu";
        //
        // lblWinner
        //
        lblWinner.Dock = DockStyle.Fill;
        lblWinner.Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold);
        lblWinner.ForeColor = Color.FromArgb(87, 100, 167);
        lblWinner.Location = new Point(27, 72);
        lblWinner.Name = "lblWinner";
        lblWinner.Size = new Size(666, 58);
        lblWinner.TabIndex = 1;
        lblWinner.Text = "🏆 Người thắng: ...";
        lblWinner.TextAlign = ContentAlignment.MiddleLeft;
        //
        // lstFinalScores
        //
        lstFinalScores.BackColor = Color.White;
        lstFinalScores.Columns.AddRange(new ColumnHeader[] { colTenNguoiChoi, colDiem });
        lstFinalScores.Dock = DockStyle.Fill;
        lstFinalScores.Font = new Font("Segoe UI", 11F);
        lstFinalScores.FullRowSelect = true;
        lstFinalScores.GridLines = true;
        lstFinalScores.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        lstFinalScores.Location = new Point(27, 133);
        lstFinalScores.MultiSelect = false;
        lstFinalScores.Name = "lstFinalScores";
        lstFinalScores.Size = new Size(666, 298);
        lstFinalScores.TabIndex = 2;
        lstFinalScores.UseCompatibleStateImageBehavior = false;
        lstFinalScores.View = View.Details;
        //
        // colTenNguoiChoi
        //
        colTenNguoiChoi.Text = "Tên người chơi";
        colTenNguoiChoi.Width = 470;
        //
        // colDiem
        //
        colDiem.Text = "Điểm";
        colDiem.TextAlign = HorizontalAlignment.Center;
        colDiem.Width = 150;
        //
        // btnBackToLobby
        //
        btnBackToLobby.Anchor = AnchorStyles.Right;
        btnBackToLobby.BackColor = Color.FromArgb(110, 124, 212);
        btnBackToLobby.FlatAppearance.BorderSize = 0;
        btnBackToLobby.FlatStyle = FlatStyle.Flat;
        btnBackToLobby.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
        btnBackToLobby.ForeColor = Color.White;
        btnBackToLobby.Location = new Point(529, 448);
        btnBackToLobby.Name = "btnBackToLobby";
        btnBackToLobby.Size = new Size(164, 36);
        btnBackToLobby.TabIndex = 3;
        btnBackToLobby.Text = "Về phòng chờ";
        btnBackToLobby.UseVisualStyleBackColor = false;
        btnBackToLobby.Click += btnBackToLobby_Click;
        //
        // ResultForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(245, 247, 253);
        ClientSize = new Size(720, 520);
        Controls.Add(tlpRoot);
        MinimumSize = new Size(620, 460);
        Name = "ResultForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Kết quả trận đấu";
        tlpRoot.ResumeLayout(false);
        ResumeLayout(false);
    }
}
