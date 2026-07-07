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

    private System.Windows.Forms.Panel panelLogs;
    private System.Windows.Forms.RichTextBox txtLogs;
    private System.Windows.Forms.PictureBox pbSettings;
    private System.Windows.Forms.Panel panelBottomBar;
    private System.Windows.Forms.TabControl tabControl;
    private System.Windows.Forms.TabPage tabScanner;
    private System.Windows.Forms.TabPage tabSafeAnalysis;
    private System.Windows.Forms.Button btnScanFolder;
    private System.Windows.Forms.Button btnScanDisk;
    private System.Windows.Forms.ComboBox cmbDrives;
    private System.Windows.Forms.ProgressBar progressScan;
    private System.Windows.Forms.Label lblScanResult;
    private System.Windows.Forms.FolderBrowserDialog folderBrowser;
    private System.Windows.Forms.Button btnSelectFile;
    private System.Windows.Forms.Button btnStartSafeAnalysis;
    private System.Windows.Forms.Label lblSafeAnalysisDesc;
    private System.Windows.Forms.Label lblSelectedFile;
    private System.Windows.Forms.Button btnLaunch;
    private System.Windows.Forms.Button btnClose;
    private System.Windows.Forms.Panel panelSafeHost;
    private System.Windows.Forms.PictureBox pbIllustration;
    private System.Windows.Forms.Label lblIllustration;
    private System.Windows.Forms.Label lblScanStatus;

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        panelLogs = new Panel();
        txtLogs = new RichTextBox();
        pbSettings = new PictureBox();
        panelBottomBar = new Panel();
        tabControl = new TabControl();
        tabScanner = new TabPage();
        tabSafeAnalysis = new TabPage();
        btnScanFolder = new Button();
        btnScanDisk = new Button();
        cmbDrives = new ComboBox();
        progressScan = new ProgressBar();
        lblScanResult = new Label();
        folderBrowser = new FolderBrowserDialog();
        btnSelectFile = new Button();
        btnStartSafeAnalysis = new Button();
        lblSafeAnalysisDesc = new Label();
        lblSelectedFile = new Label();
        btnLaunch = new Button();
        btnClose = new Button();
        panelSafeHost = new Panel();
        pbIllustration = new PictureBox();
        lblIllustration = new Label();
        lblScanStatus = new Label();

        SuspendLayout();

        // ===== tabControl =====
        tabControl.Dock = DockStyle.Fill;
        tabControl.Font = new Font(new FontFamily("Segoe UI"), 10F);
        tabControl.Controls.Add(tabScanner);
        tabControl.Controls.Add(tabSafeAnalysis);

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

        // ===== tabSafeAnalysis =====
        tabSafeAnalysis.Text = "🛡️ Безопасный анализ";
        tabSafeAnalysis.Padding = new Padding(15);
        tabSafeAnalysis.BackColor = Color.FromArgb(250, 250, 252);

        // Панель хоста (центр) — Dock=Fill, добавляется первой
        panelSafeHost.Dock = DockStyle.Fill;
        panelSafeHost.BackColor = Color.FromArgb(245, 245, 250);
        panelSafeHost.Padding = new Padding(10);
        panelSafeHost.Controls.Add(pbIllustration);
        panelSafeHost.Controls.Add(lblIllustration);
        tabSafeAnalysis.Controls.Add(panelSafeHost);

        // pbIllustration — по центру панели
        pbIllustration.Location = new Point(0, 0);
        pbIllustration.Size = new Size(200, 200);
        pbIllustration.SizeMode = PictureBoxSizeMode.Zoom;
        pbIllustration.BackColor = Color.Transparent;
        pbIllustration.Image = CreatePlaceholderIllustration();
        pbIllustration.Anchor = AnchorStyles.None;

        // lblIllustration
        lblIllustration.Text = "Выберите файл и запустите анализ";
        lblIllustration.Location = new Point(0, 210);
        lblIllustration.Size = new Size(500, 30);
        lblIllustration.Font = new Font(new FontFamily("Segoe UI"), 10F);
        lblIllustration.ForeColor = Color.Gray;
        lblIllustration.TextAlign = ContentAlignment.TopCenter;
        lblIllustration.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        // Панель кнопок — Dock=Bottom
        var panelButtons = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 55,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 5, 0, 5)
        };

        btnStartSafeAnalysis.Text = "🛡️ Начать анализ";
        btnStartSafeAnalysis.Location = new Point(0, 5);
        btnStartSafeAnalysis.Size = new Size(170, 45);
        btnStartSafeAnalysis.Font = new Font(new FontFamily("Segoe UI"), 10F, FontStyle.Bold);
        btnStartSafeAnalysis.BackColor = Color.FromArgb(76, 175, 80);
        btnStartSafeAnalysis.ForeColor = Color.White;
        btnStartSafeAnalysis.FlatStyle = FlatStyle.Flat;
        btnStartSafeAnalysis.FlatAppearance.BorderSize = 0;
        btnStartSafeAnalysis.Cursor = Cursors.Hand;
        btnStartSafeAnalysis.Click += BtnStartSafeAnalysis_Click;
        panelButtons.Controls.Add(btnStartSafeAnalysis);

        btnLaunch.Text = "▶ Запустить";
        btnLaunch.Location = new Point(180, 5);
        btnLaunch.Size = new Size(120, 45);
        btnLaunch.Font = new Font(new FontFamily("Segoe UI"), 10F, FontStyle.Bold);
        btnLaunch.BackColor = Color.FromArgb(0, 120, 215);
        btnLaunch.ForeColor = Color.White;
        btnLaunch.FlatStyle = FlatStyle.Flat;
        btnLaunch.FlatAppearance.BorderSize = 0;
        btnLaunch.Cursor = Cursors.Hand;
        btnLaunch.Click += BtnLaunch_Click;
        panelButtons.Controls.Add(btnLaunch);

        btnClose.Text = "✕ Закрыть";
        btnClose.Location = new Point(310, 5);
        btnClose.Size = new Size(120, 45);
        btnClose.Font = new Font(new FontFamily("Segoe UI"), 10F, FontStyle.Bold);
        btnClose.BackColor = Color.FromArgb(220, 50, 50);
        btnClose.ForeColor = Color.White;
        btnClose.FlatStyle = FlatStyle.Flat;
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.Cursor = Cursors.Hand;
        btnClose.Click += BtnClose_Click;
        panelButtons.Controls.Add(btnClose);
        tabSafeAnalysis.Controls.Add(panelButtons);

        // Панель выбора файла — Dock=Top
        var panelSelect = new Panel
        {
            Dock = DockStyle.Top,
            Height = 85,
            BackColor = Color.White,
            Padding = new Padding(0)
        };

        var lblTitle = new Label
        {
            Text = "Полный анализ безопасности",
            Location = new Point(0, 5),
            Size = new Size(500, 25),
            Font = new Font(new FontFamily("Segoe UI"), 13F, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 30, 40)
        };
        panelSelect.Controls.Add(lblTitle);

        lblSafeAnalysisDesc.Text = "Проверка хешей • Подписи • Упаковщиков • Мониторинг процессов, файлов, реестра, сети • Оценка риска 0–100";
        lblSafeAnalysisDesc.Location = new Point(0, 32);
        lblSafeAnalysisDesc.Size = new Size(500, 20);
        lblSafeAnalysisDesc.Font = new Font(new FontFamily("Segoe UI"), 8.5F);
        lblSafeAnalysisDesc.ForeColor = Color.FromArgb(100, 100, 110);
        panelSelect.Controls.Add(lblSafeAnalysisDesc);

        // lblScanStatus — в panelSelect
        lblScanStatus.Text = "Готово к работе";
        lblScanStatus.Location = new Point(0, 55);
        lblScanStatus.Size = new Size(500, 22);
        lblScanStatus.Font = new Font(new FontFamily("Segoe UI"), 10F, FontStyle.Bold);
        lblScanStatus.ForeColor = Color.FromArgb(0, 120, 215);
        panelSelect.Controls.Add(lblScanStatus);

        btnSelectFile.Text = "📂 Выбрать файл";
        btnSelectFile.Location = new Point(520, 10);
        btnSelectFile.Size = new Size(150, 35);
        btnSelectFile.Font = new Font(new FontFamily("Segoe UI"), 10F, FontStyle.Bold);
        btnSelectFile.BackColor = Color.FromArgb(0, 120, 215);
        btnSelectFile.ForeColor = Color.White;
        btnSelectFile.FlatStyle = FlatStyle.Flat;
        btnSelectFile.FlatAppearance.BorderSize = 0;
        btnSelectFile.Cursor = Cursors.Hand;
        btnSelectFile.Click += BtnSelectFile_Click;
        panelSelect.Controls.Add(btnSelectFile);

        lblSelectedFile.Text = "Файл не выбран";
        lblSelectedFile.Location = new Point(520, 50);
        lblSelectedFile.Size = new Size(270, 22);
        lblSelectedFile.Font = new Font(new FontFamily("Segoe UI"), 9F);
        lblSelectedFile.ForeColor = Color.FromArgb(100, 100, 110);
        lblSelectedFile.AutoEllipsis = true;
        panelSelect.Controls.Add(lblSelectedFile);

        tabSafeAnalysis.Controls.Add(panelSelect);

        // ===== panelLogs =====
        panelLogs.Dock = DockStyle.Bottom;
        panelLogs.Height = 200;
        panelLogs.BackColor = Color.FromArgb(25, 25, 35);
        panelLogs.Controls.Add(txtLogs);

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
        Text = "Cramd Anti-Virus v1.2.0";
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
                long used = drive.TotalSize - drive.TotalFreeSpace;
                cmbDrives.Items.Add($"{drive.Name} ({used / (1024*1024*1024)}ГБ / {drive.TotalSize / (1024*1024*1024)}ГБ)");
            }
        }
        if (cmbDrives.Items.Count > 0) cmbDrives.SelectedIndex = 0;
    }
}
