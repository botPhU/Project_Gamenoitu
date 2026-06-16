using System.Drawing;
using System.Windows.Forms;

namespace WordChain.Client;

partial class GameForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel tlpRoot;
    private Panel pnlHeader;
    private Label lblRoomTitle;
    private TableLayoutPanel tlpContent;
    private Panel pnlGameArea;
    private Label lblTurn;
    private Label lblTimer;
    private Label lblCurrentWord;
    private TextBox txtWordInput;
    private Button btnSubmitWord;
    private Label lblMessage;
    private Label lblWordInputTitle;
    private TableLayoutPanel tlpSidebar;
    private Panel pnlPlayers;
    private Label lblPlayersTitle;
    private ListBox lstPlayers;
    private Panel pnlHistory;
    private Label lblHistoryTitle;
    private ListBox lstHistory;
    private Panel pnlChat;
    private Label lblChatTitle;
    private ListBox lstChat;
    private Panel pnlChatInput;
    private TextBox txtChatInput;
    private Button btnSendChat;

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
        pnlHeader = new Panel();
        lblRoomTitle = new Label();
        tlpContent = new TableLayoutPanel();
        pnlGameArea = new Panel();
        lblMessage = new Label();
        btnSubmitWord = new Button();
        txtWordInput = new TextBox();
        lblWordInputTitle = new Label();
        lblCurrentWord = new Label();
        lblTimer = new Label();
        lblTurn = new Label();
        tlpSidebar = new TableLayoutPanel();
        pnlPlayers = new Panel();
        lstPlayers = new ListBox();
        lblPlayersTitle = new Label();
        pnlHistory = new Panel();
        lstHistory = new ListBox();
        lblHistoryTitle = new Label();
        pnlChat = new Panel();
        lstChat = new ListBox();
        pnlChatInput = new Panel();
        txtChatInput = new TextBox();
        btnSendChat = new Button();
        lblChatTitle = new Label();
        tlpRoot.SuspendLayout();
        pnlHeader.SuspendLayout();
        tlpContent.SuspendLayout();
        pnlGameArea.SuspendLayout();
        tlpSidebar.SuspendLayout();
        pnlPlayers.SuspendLayout();
        pnlHistory.SuspendLayout();
        pnlChat.SuspendLayout();
        pnlChatInput.SuspendLayout();
        SuspendLayout();
        // 
        // tlpRoot
        // 
        tlpRoot.ColumnCount = 1;
        tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpRoot.Controls.Add(pnlHeader, 0, 0);
        tlpRoot.Controls.Add(tlpContent, 0, 1);
        tlpRoot.Dock = DockStyle.Fill;
        tlpRoot.Location = new Point(0, 0);
        tlpRoot.Name = "tlpRoot";
        tlpRoot.Padding = new Padding(20);
        tlpRoot.RowCount = 2;
        tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 82F));
        tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpRoot.Size = new Size(1260, 760);
        tlpRoot.TabIndex = 0;
        // 
        // pnlHeader
        // 
        pnlHeader.BackColor = Color.FromArgb(248, 245, 255);
        pnlHeader.Controls.Add(lblRoomTitle);
        pnlHeader.Dock = DockStyle.Fill;
        pnlHeader.Location = new Point(23, 23);
        pnlHeader.Name = "pnlHeader";
        pnlHeader.Padding = new Padding(24, 16, 24, 16);
        pnlHeader.Size = new Size(1214, 76);
        pnlHeader.TabIndex = 0;
        // 
        // lblRoomTitle
        // 
        lblRoomTitle.Dock = DockStyle.Fill;
        lblRoomTitle.Font = new Font("Bahnschrift SemiBold", 24F, FontStyle.Bold);
        lblRoomTitle.ForeColor = Color.FromArgb(51, 60, 103);
        lblRoomTitle.Location = new Point(24, 16);
        lblRoomTitle.Name = "lblRoomTitle";
        lblRoomTitle.Size = new Size(1166, 44);
        lblRoomTitle.TabIndex = 0;
        lblRoomTitle.Text = "Game nối từ";
        // 
        // tlpContent
        // 
        tlpContent.ColumnCount = 2;
        tlpContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 59F));
        tlpContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 41F));
        tlpContent.Controls.Add(pnlGameArea, 0, 0);
        tlpContent.Controls.Add(tlpSidebar, 1, 0);
        tlpContent.Dock = DockStyle.Fill;
        tlpContent.Location = new Point(23, 105);
        tlpContent.Name = "tlpContent";
        tlpContent.RowCount = 1;
        tlpContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpContent.Size = new Size(1214, 632);
        tlpContent.TabIndex = 1;
        // 
        // pnlGameArea
        // 
        pnlGameArea.BackColor = Color.White;
        pnlGameArea.Controls.Add(lblMessage);
        pnlGameArea.Controls.Add(btnSubmitWord);
        pnlGameArea.Controls.Add(txtWordInput);
        pnlGameArea.Controls.Add(lblWordInputTitle);
        pnlGameArea.Controls.Add(lblCurrentWord);
        pnlGameArea.Controls.Add(lblTimer);
        pnlGameArea.Controls.Add(lblTurn);
        pnlGameArea.Dock = DockStyle.Fill;
        pnlGameArea.Location = new Point(0, 0);
        pnlGameArea.Margin = new Padding(0, 0, 14, 0);
        pnlGameArea.Name = "pnlGameArea";
        pnlGameArea.Padding = new Padding(28);
        pnlGameArea.Size = new Size(702, 632);
        pnlGameArea.TabIndex = 0;
        // 
        // lblMessage
        // 
        lblMessage.Font = new Font("Segoe UI", 11F);
        lblMessage.ForeColor = Color.FromArgb(194, 96, 122);
        lblMessage.Location = new Point(32, 358);
        lblMessage.Name = "lblMessage";
        lblMessage.Size = new Size(634, 54);
        lblMessage.TabIndex = 6;
        lblMessage.Text = "Thông báo sẽ hiển thị ở đây.";
        lblMessage.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // btnSubmitWord
        // 
        btnSubmitWord.BackColor = Color.FromArgb(110, 124, 212);
        btnSubmitWord.FlatAppearance.BorderSize = 0;
        btnSubmitWord.FlatStyle = FlatStyle.Flat;
        btnSubmitWord.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
        btnSubmitWord.ForeColor = Color.White;
        btnSubmitWord.Location = new Point(518, 300);
        btnSubmitWord.Name = "btnSubmitWord";
        btnSubmitWord.Size = new Size(148, 42);
        btnSubmitWord.TabIndex = 5;
        btnSubmitWord.Text = "Gửi";
        btnSubmitWord.UseVisualStyleBackColor = false;
        btnSubmitWord.Click += btnSubmitWord_Click;
        // 
        // txtWordInput
        // 
        txtWordInput.Font = new Font("Segoe UI", 11F);
        txtWordInput.Location = new Point(32, 304);
        txtWordInput.Name = "txtWordInput";
        txtWordInput.PlaceholderText = "Nhập từ nối của bạn";
        txtWordInput.Size = new Size(468, 27);
        txtWordInput.TabIndex = 4;
        // 
        // lblWordInputTitle
        // 
        lblWordInputTitle.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
        lblWordInputTitle.ForeColor = Color.FromArgb(108, 112, 143);
        lblWordInputTitle.Location = new Point(32, 262);
        lblWordInputTitle.Name = "lblWordInputTitle";
        lblWordInputTitle.Size = new Size(288, 28);
        lblWordInputTitle.TabIndex = 3;
        lblWordInputTitle.Text = "Nhập từ mới";
        // 
        // lblCurrentWord
        // 
        lblCurrentWord.Font = new Font("Bahnschrift SemiBold", 30F, FontStyle.Bold);
        lblCurrentWord.ForeColor = Color.FromArgb(51, 60, 103);
        lblCurrentWord.Location = new Point(32, 120);
        lblCurrentWord.Name = "lblCurrentWord";
        lblCurrentWord.Size = new Size(634, 100);
        lblCurrentWord.TabIndex = 2;
        lblCurrentWord.Text = "Từ hiện tại: ...";
        lblCurrentWord.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // lblTimer
        // 
        lblTimer.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblTimer.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
        lblTimer.ForeColor = Color.FromArgb(89, 136, 102);
        lblTimer.Location = new Point(470, 32);
        lblTimer.Name = "lblTimer";
        lblTimer.Size = new Size(196, 28);
        lblTimer.TabIndex = 1;
        lblTimer.Text = "Thời gian: 15s";
        lblTimer.TextAlign = ContentAlignment.MiddleRight;
        // 
        // lblTurn
        // 
        lblTurn.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
        lblTurn.ForeColor = Color.FromArgb(108, 112, 143);
        lblTurn.Location = new Point(32, 32);
        lblTurn.Name = "lblTurn";
        lblTurn.Size = new Size(320, 28);
        lblTurn.TabIndex = 0;
        lblTurn.Text = "Đến lượt: ...";
        // 
        // tlpSidebar
        // 
        tlpSidebar.ColumnCount = 1;
        tlpSidebar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpSidebar.Controls.Add(pnlPlayers, 0, 0);
        tlpSidebar.Controls.Add(pnlHistory, 0, 1);
        tlpSidebar.Controls.Add(pnlChat, 0, 2);
        tlpSidebar.Dock = DockStyle.Fill;
        tlpSidebar.Location = new Point(716, 0);
        tlpSidebar.Margin = new Padding(0);
        tlpSidebar.Name = "tlpSidebar";
        tlpSidebar.RowCount = 3;
        tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
        tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
        tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
        tlpSidebar.Size = new Size(498, 632);
        tlpSidebar.TabIndex = 1;
        // 
        // pnlPlayers
        // 
        pnlPlayers.BackColor = Color.White;
        pnlPlayers.Controls.Add(lstPlayers);
        pnlPlayers.Controls.Add(lblPlayersTitle);
        pnlPlayers.Dock = DockStyle.Fill;
        pnlPlayers.Location = new Point(0, 0);
        pnlPlayers.Margin = new Padding(0, 0, 0, 10);
        pnlPlayers.Name = "pnlPlayers";
        pnlPlayers.Padding = new Padding(18);
        pnlPlayers.Size = new Size(498, 179);
        pnlPlayers.TabIndex = 0;
        // 
        // lstPlayers
        // 
        lstPlayers.BackColor = Color.FromArgb(247, 249, 255);
        lstPlayers.BorderStyle = BorderStyle.None;
        lstPlayers.Dock = DockStyle.Fill;
        lstPlayers.Font = new Font("Segoe UI", 10.5F);
        lstPlayers.FormattingEnabled = true;
        lstPlayers.ItemHeight = 19;
        lstPlayers.Location = new Point(18, 50);
        lstPlayers.Name = "lstPlayers";
        lstPlayers.Size = new Size(462, 111);
        lstPlayers.TabIndex = 1;
        // 
        // lblPlayersTitle
        // 
        lblPlayersTitle.Dock = DockStyle.Top;
        lblPlayersTitle.Font = new Font("Bahnschrift SemiBold", 16F, FontStyle.Bold);
        lblPlayersTitle.ForeColor = Color.FromArgb(51, 60, 103);
        lblPlayersTitle.Location = new Point(18, 18);
        lblPlayersTitle.Name = "lblPlayersTitle";
        lblPlayersTitle.Size = new Size(462, 32);
        lblPlayersTitle.TabIndex = 0;
        lblPlayersTitle.Text = "Người chơi";
        // 
        // pnlHistory
        // 
        pnlHistory.BackColor = Color.White;
        pnlHistory.Controls.Add(lstHistory);
        pnlHistory.Controls.Add(lblHistoryTitle);
        pnlHistory.Dock = DockStyle.Fill;
        pnlHistory.Location = new Point(0, 199);
        pnlHistory.Margin = new Padding(0, 10, 0, 10);
        pnlHistory.Name = "pnlHistory";
        pnlHistory.Padding = new Padding(18);
        pnlHistory.Size = new Size(498, 169);
        pnlHistory.TabIndex = 1;
        // 
        // lstHistory
        // 
        lstHistory.BackColor = Color.FromArgb(255, 250, 252);
        lstHistory.BorderStyle = BorderStyle.None;
        lstHistory.Dock = DockStyle.Fill;
        lstHistory.Font = new Font("Segoe UI", 10.5F);
        lstHistory.FormattingEnabled = true;
        lstHistory.ItemHeight = 19;
        lstHistory.Location = new Point(18, 50);
        lstHistory.Name = "lstHistory";
        lstHistory.Size = new Size(462, 101);
        lstHistory.TabIndex = 1;
        // 
        // lblHistoryTitle
        // 
        lblHistoryTitle.Dock = DockStyle.Top;
        lblHistoryTitle.Font = new Font("Bahnschrift SemiBold", 16F, FontStyle.Bold);
        lblHistoryTitle.ForeColor = Color.FromArgb(51, 60, 103);
        lblHistoryTitle.Location = new Point(18, 18);
        lblHistoryTitle.Name = "lblHistoryTitle";
        lblHistoryTitle.Size = new Size(462, 32);
        lblHistoryTitle.TabIndex = 0;
        lblHistoryTitle.Text = "Lịch sử từ";
        // 
        // pnlChat
        // 
        pnlChat.BackColor = Color.White;
        pnlChat.Controls.Add(lstChat);
        pnlChat.Controls.Add(pnlChatInput);
        pnlChat.Controls.Add(lblChatTitle);
        pnlChat.Dock = DockStyle.Fill;
        pnlChat.Location = new Point(0, 388);
        pnlChat.Margin = new Padding(0, 10, 0, 0);
        pnlChat.Name = "pnlChat";
        pnlChat.Padding = new Padding(18);
        pnlChat.Size = new Size(498, 244);
        pnlChat.TabIndex = 2;
        // 
        // lstChat
        // 
        lstChat.BackColor = Color.FromArgb(244, 249, 255);
        lstChat.BorderStyle = BorderStyle.None;
        lstChat.Dock = DockStyle.Fill;
        lstChat.Font = new Font("Segoe UI", 10.5F);
        lstChat.FormattingEnabled = true;
        lstChat.ItemHeight = 19;
        lstChat.Location = new Point(18, 50);
        lstChat.Name = "lstChat";
        lstChat.Size = new Size(462, 138);
        lstChat.TabIndex = 1;
        // 
        // pnlChatInput
        // 
        pnlChatInput.Controls.Add(txtChatInput);
        pnlChatInput.Controls.Add(btnSendChat);
        pnlChatInput.Dock = DockStyle.Bottom;
        pnlChatInput.Location = new Point(18, 188);
        pnlChatInput.Name = "pnlChatInput";
        pnlChatInput.Size = new Size(462, 38);
        pnlChatInput.TabIndex = 2;
        // 
        // txtChatInput
        // 
        txtChatInput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtChatInput.Font = new Font("Segoe UI", 10.5F);
        txtChatInput.Location = new Point(0, 5);
        txtChatInput.Name = "txtChatInput";
        txtChatInput.PlaceholderText = "Nhập tin chat";
        txtChatInput.Size = new Size(346, 26);
        txtChatInput.TabIndex = 0;
        // 
        // btnSendChat
        // 
        btnSendChat.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnSendChat.BackColor = Color.FromArgb(110, 124, 212);
        btnSendChat.FlatAppearance.BorderSize = 0;
        btnSendChat.FlatStyle = FlatStyle.Flat;
        btnSendChat.Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold);
        btnSendChat.ForeColor = Color.White;
        btnSendChat.Location = new Point(356, 3);
        btnSendChat.Name = "btnSendChat";
        btnSendChat.Size = new Size(106, 30);
        btnSendChat.TabIndex = 1;
        btnSendChat.Text = "Gửi chat";
        btnSendChat.UseVisualStyleBackColor = false;
        btnSendChat.Click += btnSendChat_Click;
        // 
        // lblChatTitle
        // 
        lblChatTitle.Dock = DockStyle.Top;
        lblChatTitle.Font = new Font("Bahnschrift SemiBold", 16F, FontStyle.Bold);
        lblChatTitle.ForeColor = Color.FromArgb(51, 60, 103);
        lblChatTitle.Location = new Point(18, 18);
        lblChatTitle.Name = "lblChatTitle";
        lblChatTitle.Size = new Size(462, 32);
        lblChatTitle.TabIndex = 0;
        lblChatTitle.Text = "Chat";
        // 
        // GameForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(244, 246, 252);
        ClientSize = new Size(1260, 760);
        Controls.Add(tlpRoot);
        MinimumSize = new Size(1080, 700);
        Name = "GameForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Phòng chơi";
        tlpRoot.ResumeLayout(false);
        pnlHeader.ResumeLayout(false);
        tlpContent.ResumeLayout(false);
        pnlGameArea.ResumeLayout(false);
        pnlGameArea.PerformLayout();
        tlpSidebar.ResumeLayout(false);
        pnlPlayers.ResumeLayout(false);
        pnlHistory.ResumeLayout(false);
        pnlChat.ResumeLayout(false);
        pnlChatInput.ResumeLayout(false);
        pnlChatInput.PerformLayout();
        ResumeLayout(false);
    }
}
