using System.Drawing;
using System.Windows.Forms;

namespace WordChain.Client;

partial class LobbyForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel tlpRoot;
    private Label lblLobbyTitle;
    private Label lblRoomCode;
    private Label lblLobbyStatus;
    private ListBox lstRoomPlayers;
    private Label lblPlayersTitle;
    private Label lblHostHint;
    private Button btnStartGame;

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
        lblLobbyTitle = new Label();
        lblRoomCode = new Label();
        lblLobbyStatus = new Label();
        lblPlayersTitle = new Label();
        lstRoomPlayers = new ListBox();
        lblHostHint = new Label();
        btnStartGame = new Button();
        tlpRoot.SuspendLayout();
        SuspendLayout();
        // 
        // tlpRoot
        // 
        tlpRoot.ColumnCount = 1;
        tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpRoot.Controls.Add(lblLobbyTitle, 0, 0);
        tlpRoot.Controls.Add(lblRoomCode, 0, 1);
        tlpRoot.Controls.Add(lblLobbyStatus, 0, 2);
        tlpRoot.Controls.Add(lblPlayersTitle, 0, 3);
        tlpRoot.Controls.Add(lstRoomPlayers, 0, 4);
        tlpRoot.Controls.Add(lblHostHint, 0, 5);
        tlpRoot.Controls.Add(btnStartGame, 0, 6);
        tlpRoot.Dock = DockStyle.Fill;
        tlpRoot.Location = new Point(0, 0);
        tlpRoot.Name = "tlpRoot";
        tlpRoot.Padding = new Padding(24);
        tlpRoot.RowCount = 7;
        tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
        tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
        tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
        tlpRoot.Size = new Size(760, 560);
        tlpRoot.TabIndex = 0;
        // 
        // lblLobbyTitle
        // 
        lblLobbyTitle.Dock = DockStyle.Fill;
        lblLobbyTitle.Font = new Font("Bahnschrift SemiBold", 24F, FontStyle.Bold);
        lblLobbyTitle.ForeColor = Color.FromArgb(51, 60, 103);
        lblLobbyTitle.Location = new Point(27, 24);
        lblLobbyTitle.Name = "lblLobbyTitle";
        lblLobbyTitle.Size = new Size(706, 52);
        lblLobbyTitle.TabIndex = 0;
        lblLobbyTitle.Text = "Phòng chờ";
        // 
        // lblRoomCode
        // 
        lblRoomCode.Dock = DockStyle.Fill;
        lblRoomCode.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
        lblRoomCode.ForeColor = Color.FromArgb(87, 100, 167);
        lblRoomCode.Location = new Point(27, 76);
        lblRoomCode.Name = "lblRoomCode";
        lblRoomCode.Size = new Size(706, 34);
        lblRoomCode.TabIndex = 1;
        lblRoomCode.Text = "Mã phòng: ---";
        // 
        // lblLobbyStatus
        // 
        lblLobbyStatus.Dock = DockStyle.Fill;
        lblLobbyStatus.Font = new Font("Segoe UI", 11F);
        lblLobbyStatus.ForeColor = Color.FromArgb(108, 112, 143);
        lblLobbyStatus.Location = new Point(27, 110);
        lblLobbyStatus.Name = "lblLobbyStatus";
        lblLobbyStatus.Size = new Size(706, 48);
        lblLobbyStatus.TabIndex = 2;
        lblLobbyStatus.Text = "Trạng thái phòng";
        // 
        // lblPlayersTitle
        // 
        lblPlayersTitle.Dock = DockStyle.Fill;
        lblPlayersTitle.Font = new Font("Bahnschrift SemiBold", 16F, FontStyle.Bold);
        lblPlayersTitle.ForeColor = Color.FromArgb(51, 60, 103);
        lblPlayersTitle.Location = new Point(27, 158);
        lblPlayersTitle.Name = "lblPlayersTitle";
        lblPlayersTitle.Size = new Size(706, 36);
        lblPlayersTitle.TabIndex = 3;
        lblPlayersTitle.Text = "Người chơi trong phòng";
        // 
        // lstRoomPlayers
        // 
        lstRoomPlayers.BackColor = Color.White;
        lstRoomPlayers.BorderStyle = BorderStyle.None;
        lstRoomPlayers.Dock = DockStyle.Fill;
        lstRoomPlayers.Font = new Font("Segoe UI", 11F);
        lstRoomPlayers.FormattingEnabled = true;
        lstRoomPlayers.ItemHeight = 20;
        lstRoomPlayers.Location = new Point(27, 197);
        lstRoomPlayers.Name = "lstRoomPlayers";
        lstRoomPlayers.Size = new Size(706, 222);
        lstRoomPlayers.TabIndex = 4;
        // 
        // lblHostHint
        // 
        lblHostHint.Dock = DockStyle.Fill;
        lblHostHint.Font = new Font("Segoe UI", 10.5F);
        lblHostHint.ForeColor = Color.FromArgb(108, 112, 143);
        lblHostHint.Location = new Point(27, 422);
        lblHostHint.Name = "lblHostHint";
        lblHostHint.Size = new Size(706, 54);
        lblHostHint.TabIndex = 5;
        lblHostHint.Text = "Gợi ý";
        lblHostHint.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // btnStartGame
        // 
        btnStartGame.Anchor = AnchorStyles.Right;
        btnStartGame.BackColor = Color.FromArgb(110, 124, 212);
        btnStartGame.FlatAppearance.BorderSize = 0;
        btnStartGame.FlatStyle = FlatStyle.Flat;
        btnStartGame.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
        btnStartGame.ForeColor = Color.White;
        btnStartGame.Location = new Point(585, 488);
        btnStartGame.Name = "btnStartGame";
        btnStartGame.Size = new Size(148, 36);
        btnStartGame.TabIndex = 6;
        btnStartGame.Text = "Bắt đầu";
        btnStartGame.UseVisualStyleBackColor = false;
        btnStartGame.Click += btnStartGame_Click;
        // 
        // LobbyForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(245, 247, 253);
        ClientSize = new Size(760, 560);
        Controls.Add(tlpRoot);
        MinimumSize = new Size(680, 520);
        Name = "LobbyForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Phòng chờ";
        tlpRoot.ResumeLayout(false);
        ResumeLayout(false);
    }
}
