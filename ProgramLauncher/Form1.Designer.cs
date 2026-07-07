namespace ProgramLauncher;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private System.Windows.Forms.TextBox txtPath;
    private System.Windows.Forms.Button btnBrowse;
    private System.Windows.Forms.Button btnLaunch;
    private System.Windows.Forms.Button btnClose;
    private System.Windows.Forms.Panel panelTop;
    private System.Windows.Forms.Panel panelHost;
    private System.Windows.Forms.Panel panelLogs;
    private System.Windows.Forms.RichTextBox txtLogs;
    private System.Windows.Forms.Label lblScanStatus;
    private System.Windows.Forms.PictureBox pbSettings;
    private System.Windows.Forms.Panel panelBottomBar;
    private System.Windows.Forms.TabControl tabControl;
    private System.Windows.Forms.TabPage tabLauncher;
    private System.Windows.Forms.TabPage tabScanner;
    private System.Windows.Forms.PictureBox pbIllustration;
    private System.Windows.Forms.Label lblIllustration;
    private System.Windows.Forms.Button btnScanFolder;
    private System.Windows.Forms.Button btnScanDisk;
    private System.Windows.Forms.ComboBox cmbDrives;
    private System.Windows.Forms.ProgressBar progressScan;
    private System.Windows.Forms.Label lblScanResult;
    private System.Windows.Forms.FolderBrowserDialog folderBrowser;

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        panelTop = new Panel();
        panelHost = new Panel();
        panelLogs = new Panel();
        txtLogs = new RichTextBox();
        lblScanStatus = new Label();
        pbSettings = new PictureBox();
        panelBottomBar = new Panel();
        tabControl = new TabControl();
        tabLauncher = new TabPage();
        tabScanner = new TabPage();
        txtPath = new TextBox();
        btnBrowse = new Button();
        btnLaunch = new Button();
        btnClose = new Button();
        pbIllustration = new PictureBox();
        lblIllustration = new Label();
        btnScanFolder = new Button();
        btnScanDisk = new Button();
        cmbDrives = new ComboBox();
        progressScan = new ProgressBar();
        lblScanResult = new Label();
        folderBrowser = new FolderBrowserDialog();

        SuspendLayout();

        // ===== tabControl =====
        tabControl.Dock = DockStyle.Fill;
        tabControl.Font = new Font(new FontFamily("Segoe UI"), 10F);
        tabControl.Controls.Add(tabLauncher);
        tabControl.Controls.Add(tabScanner);

        // ===== tabLauncher =====
        tabLauncher.Text = "🚀 Запуск программ";
        tabLauncher.Padding = new Padding(0);
        tabLauncher.BackColor = Color.FromArgb(250, 250, 252);

        // panelTop
        panelTop.Dock = DockStyle.Top;
        panelTop.Height = 90;
        panelTop.BackColor = Color.White;
        panelTop.Padding = new Padding(15);
        panelTop.Controls.Add(lblScanStatus);
        panelTop.Controls.Add(txtPath);
        panelTop.Controls.Add(btnBrowse);
        panelTop.Controls.Add(btnLaunch);
        panelTop.Controls.Add(btnClose);

        // lblScanStatus
        lblScanStatus.Text = "Готово к работе";
        lblScanStatus.Location = new Point(15, 12);
        lblScanStatus.Size = new Size(600, 22);
        lblScanStatus.Font = new Font(new FontFamily("Segoe UI"), 11F, FontStyle.Bold);
        lblScanStatus.ForeColor = Color.FromArgb(0, 120, 215);

        // txtPath
        txtPath.Location = new Point(15, 42);
        txtPath.Size = new Size(450, 25);
        txtPath.Font = new Font(new FontFamily("Segoe UI"), 9F);
        txtPath.PlaceholderText = "Выберите .exe файл для запуска…";
        txtPath.BorderStyle = BorderStyle.FixedSingle;

        // btnBrowse
        btnBrowse.Text = "📂 Обзор";
        btnBrowse.Location = new Point(475, 40);
        btnBrowse.Size = new Size(95, 30);
        btnBrowse.Font = new Font(new FontFamily("Segoe UI"), 9F, FontStyle.Bold);
        btnBrowse.BackColor = Color.FromArgb(240, 240, 245);
        btnBrowse.ForeColor = Color.FromArgb(50, 50, 60);
        btnBrowse.FlatStyle = FlatStyle.Flat;
        btnBrowse.FlatAppearance.BorderSize = 0;
        btnBrowse.Cursor = Cursors.Hand;
        btnBrowse.Click += BtnBrowse_Click;

        // btnLaunch
        btnLaunch.Text = "▶ Запустить";
        btnLaunch.Location = new Point(580, 40);
        btnLaunch.Size = new Size(110, 30);
        btnLaunch.Font = new Font(new FontFamily("Segoe UI"), 9F, FontStyle.Bold);
        btnLaunch.BackColor = Color.FromArgb(0, 120, 215);
        btnLaunch.ForeColor = Color.White;
        btnLaunch.FlatStyle = FlatStyle.Flat;
        btnLaunch.FlatAppearance.BorderSize = 0;
        btnLaunch.Cursor = Cursors.Hand;
        btnLaunch.Click += BtnLaunch_Click;

        // btnClose
        btnClose.Text = "✕ Закрыть";
        btnClose.Location = new Point(700, 40);
        btnClose.Size = new Size(95, 30);
        btnClose.Font = new Font(new FontFamily("Segoe UI"), 9F, FontStyle.Bold);
        btnClose.BackColor = Color.FromArgb(220, 50, 50);
        btnClose.ForeColor = Color.White;
        btnClose.FlatStyle = FlatStyle.Flat;
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.Cursor = Cursors.Hand;
        btnClose.Click += BtnClose_Click;

        // panelHost (центр с иллюстрацией)
        panelHost.Dock = DockStyle.Fill;
        panelHost.BackColor = Color.FromArgb(245, 245, 250);
        panelHost.Padding = new Padding(10);
        panelHost.Controls.Add(pbIllustration);
        panelHost.Controls.Add(lblIllustration);

        // pbIllustration
        pbIllustration.Location = new Point(0, 0);
        pbIllustration.Size = new Size(200, 200);
        pbIllustration.SizeMode = PictureBoxSizeMode.Zoom;
        pbIllustration.BackColor = Color.Transparent;
        pbIllustration.Image = CreatePlaceholderIllustration();
        pbIllustration.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        // lblIllustration
        lblIllustration.Text = "Запустите программу для отображения процесса";
        lblIllustration.Location = new Point(0, 210);
        lblIllustration.Size = new Size(700, 30);
        lblIllustration.Font = new Font(new FontFamily("Segoe UI"), 10F);
        lblIllustration.ForeColor = Color.Gray;
        lblIllustration.TextAlign = ContentAlignment.TopCenter;
        lblIllustration.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        // ===== tabScanner =====
        tabScanner.Text = "🔍 Сканер угроз";
        tabScanner.Padding = new Padding(15);
        tabScanner.BackColor = Color.FromArgb(250, 250, 252);

        var lblScannerTitle = new Label
        {
            Text = "Сканирование на угрозы",
            Location = new Point(0, 10),
            Font = new Font(new FontFamily("Segoe UI"), 14F, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 30, 40)
        };
        tabScanner.Controls.Add(lblScannerTitle);

        // btnScanFolder
        btnScanFolder.Text = "📁 Выбрать папку";
        btnScanFolder.Location = new Point(0, 50);
        btnScanFolder.Size = new Size(150, 40);
        btnScanFolder.Font = new Font(new FontFamily("Segoe UI"), 10F, FontStyle.Bold);
        btnScanFolder.BackColor = Color.FromArgb(0, 120, 215);
        btnScanFolder.ForeColor = Color.White;
        btnScanFolder.FlatStyle = FlatStyle.Flat;
        btnScanFolder.FlatAppearance.BorderSize = 0;
        btnScanFolder.Cursor = Cursors.Hand;
        btnScanFolder.Click += BtnScanFolder_Click;
        tabScanner.Controls.Add(btnScanFolder);

        // btnScanDisk
        btnScanDisk.Text = "💿 Сканировать диск";
        btnScanDisk.Location = new Point(160, 50);
        btnScanDisk.Size = new Size(160, 40);
        btnScanDisk.Font = new Font(new FontFamily("Segoe UI"), 10F, FontStyle.Bold);
        btnScanDisk.BackColor = Color.FromArgb(0, 150, 100);
        btnScanDisk.ForeColor = Color.White;
        btnScanDisk.FlatStyle = FlatStyle.Flat;
        btnScanDisk.FlatAppearance.BorderSize = 0;
        btnScanDisk.Cursor = Cursors.Hand;
        btnScanDisk.Click += BtnScanDisk_Click;
        tabScanner.Controls.Add(btnScanDisk);

        // cmbDrives
        cmbDrives.Location = new Point(330, 55);
        cmbDrives.Size = new Size(200, 25);
        cmbDrives.Font = new Font(new FontFamily("Segoe UI"), 9F);
        cmbDrives.DropDownStyle = ComboBoxStyle.DropDownList;
        tabScanner.Controls.Add(cmbDrives);
        PopulateDrives();

        // progressScan
        progressScan.Location = new Point(0, 110);
        progressScan.Size = new Size(700, 25);
        progressScan.Style = ProgressBarStyle.Continuous;
        tabScanner.Controls.Add(progressScan);

        // lblScanResult
        lblScanResult.Location = new Point(0, 145);
        lblScanResult.Size = new Size(700, 200);
        lblScanResult.Font = new Font("Consolas", 9F);
        lblScanResult.ForeColor = Color.FromArgb(50, 50, 60);
        lblScanResult.Text = "Ожидание сканирования…";
        tabScanner.Controls.Add(lblScanResult);

        // ===== panelLogs =====
        panelLogs.Dock = DockStyle.Bottom;
        panelLogs.Height = 200;
        panelLogs.BackColor = Color.FromArgb(25, 25, 35);

        // txtLogs
        txtLogs.Dock = DockStyle.Fill;
        txtLogs.BackColor = Color.FromArgb(25, 25, 35);
        txtLogs.ForeColor = Color.FromArgb(0, 220, 180);
        txtLogs.Font = new Font("Consolas", 9F);
        txtLogs.ReadOnly = true;
        txtLogs.ScrollBars = RichTextBoxScrollBars.Vertical;
        txtLogs.BorderStyle = BorderStyle.None;
        txtLogs.Padding = new Padding(10);
        txtLogs.TextChanged += (s, e) => { txtLogs.SelectionStart = txtLogs.Text.Length; txtLogs.ScrollToCaret(); };
        panelLogs.Controls.Add(txtLogs);

        // ===== panelBottomBar =====
        panelBottomBar.Dock = DockStyle.Bottom;
        panelBottomBar.Height = 40;
        panelBottomBar.BackColor = Color.FromArgb(240, 240, 245);
        panelBottomBar.Controls.Add(pbSettings);

        // pbSettings
        pbSettings.Location = new Point(12, 7);
        pbSettings.Size = new Size(26, 26);
        pbSettings.SizeMode = PictureBoxSizeMode.Zoom;
        pbSettings.Image = CreateSettingsIcon();
        pbSettings.Cursor = Cursors.Hand;
        pbSettings.Click += PbSettings_Click;

        // ===== Form1 =====
        ClientSize = new Size(820, 720);
        Controls.Add(tabControl);
        Controls.Add(panelLogs);
        Controls.Add(panelBottomBar);
        Text = "Cramd Anti-Virus v1.0.0";
        MinimumSize = new Size(700, 550);
        StartPosition = FormStartPosition.CenterScreen;
        FormClosing += Form1_FormClosing;
        Load += Form1_Load;

        ResumeLayout(false);
    }

    #endregion

    private Image CreateSettingsIcon()
    {
        var bmp = new Bitmap(26, 26);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.DrawString("⚙", new Font(new FontFamily("Segoe UI"), 16), Brushes.Gray, new Point(2, 2));
        return bmp;
    }

    private Image CreatePlaceholderIllustration()
    {
        var bmp = new Bitmap(200, 200);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.Transparent);
        
        using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
            new Point(0, 0), new Point(200, 200),
            Color.FromArgb(200, 200, 210),
            Color.FromArgb(230, 230, 240)
        );
        
        var rect = new Rectangle(20, 20, 160, 160);
        g.FillEllipse(brush, rect);
        g.DrawString("🖥️", new Font("Segoe UI Emoji", 48), Brushes.Gray, new Point(60, 65));
        
        return bmp;
    }

    private void PopulateDrives()
    {
        cmbDrives.Items.Clear();
        foreach (var drive in DriveInfo.GetDrives())
        {
            if (drive.DriveType == DriveType.Fixed)
            {
                cmbDrives.Items.Add($"{drive.Name} (Занято/Всего)");
            }
        }
        if (cmbDrives.Items.Count > 0) cmbDrives.SelectedIndex = 0;
    }
}

