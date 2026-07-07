using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ProgramLauncher;

public class SettingsForm : Form
{
    private bool _isDarkTheme = true;

    // Тема
    private RadioButton _rbDark;
    private RadioButton _rbLight;

    // Мониторинг
    private CheckBox _chkProcessMonitor;
    private CheckBox _chkFileMonitor;
    private CheckBox _chkRegistryMonitor;
    private CheckBox _chkNetworkMonitor;
    private CheckBox _chkDllMonitor;
    private CheckBox _chkMemoryMonitor;

    private Button _btnApply;

    public bool IsDarkTheme => _isDarkTheme;

    // Настройки мониторинга
    public bool MonitorProcesses { get; private set; } = true;
    public bool MonitorFiles { get; private set; } = true;
    public bool MonitorRegistry { get; private set; } = true;
    public bool MonitorNetwork { get; private set; } = true;
    public bool MonitorDlls { get; private set; } = true;
    public bool MonitorMemory { get; private set; } = true;

    public SettingsForm(bool initialDark)
    {
        _isDarkTheme = initialDark;
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        Text = "Настройки — Cramd Anti-Virus";
        Size = new Size(440, 560);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(250, 250, 252);

        // ===== Иконка + заголовок =====
        var pbIcon = new PictureBox
        {
            Location = new Point(20, 15),
            Size = new Size(48, 48),
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = CreateIcon()
        };
        Controls.Add(pbIcon);

        var lblTitle = new Label
        {
            Text = "Cramd Anti-Virus",
            Location = new Point(80, 20),
            Size = new Size(300, 30),
            Font = new Font("Segoe UI", 15F, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 30, 40)
        };
        Controls.Add(lblTitle);

        var lblVersion = new Label
        {
            Text = "Версия 1.2.0",
            Location = new Point(80, 48),
            Size = new Size(200, 20),
            Font = new Font("Segoe UI", 8F),
            ForeColor = Color.Gray
        };
        Controls.Add(lblVersion);

        // ===== Панель темы =====
        var panelTheme = new Panel
        {
            Location = new Point(20, 80),
            Size = new Size(390, 80),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        Controls.Add(panelTheme);

        var lblTheme = new Label
        {
            Text = "🎨 Тема оформления",
            Location = new Point(15, 10),
            Size = new Size(300, 22),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(50, 50, 60),
            Parent = panelTheme
        };

        _rbDark = new RadioButton
        {
            Text = "🌙 Тёмная",
            Location = new Point(15, 38),
            Size = new Size(150, 28),
            Font = new Font("Segoe UI", 10F),
            Checked = _isDarkTheme,
            Parent = panelTheme
        };
        _rbDark.CheckedChanged += (s, e) => { if (_rbDark.Checked) _isDarkTheme = true; };

        _rbLight = new RadioButton
        {
            Text = "☀️ Светлая",
            Location = new Point(190, 38),
            Size = new Size(150, 28),
            Font = new Font("Segoe UI", 10F),
            Checked = !_isDarkTheme,
            Parent = panelTheme
        };
        _rbLight.CheckedChanged += (s, e) => { if (_rbLight.Checked) _isDarkTheme = false; };

        // ===== Панель мониторинга =====
        var panelMonitor = new Panel
        {
            Location = new Point(20, 175),
            Size = new Size(390, 290),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        Controls.Add(panelMonitor);

        var lblMonitor = new Label
        {
            Text = "📡 Мониторинг (отключить отдельные виды)",
            Location = new Point(15, 10),
            Size = new Size(360, 22),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(50, 50, 60),
            Parent = panelMonitor
        };

        int chkY = 40;
        int chkSpacing = 38;

        _chkProcessMonitor = CreateMonitorCheckbox("🚀 Мониторинг процессов",
            "Отслеживание создания и завершения процессов", chkY, panelMonitor);
        chkY += chkSpacing;

        _chkFileMonitor = CreateMonitorCheckbox("📁 Мониторинг файлов",
            "Отслеживание создания, изменения, удаления файлов", chkY, panelMonitor);
        chkY += chkSpacing;

        _chkRegistryMonitor = CreateMonitorCheckbox("🔧 Мониторинг реестра",
            "Отслеживание изменений в системном реестре", chkY, panelMonitor);
        chkY += chkSpacing;

        _chkNetworkMonitor = CreateMonitorCheckbox("🌐 Мониторинг сети",
            "Отслеживание сетевых подключений и портов", chkY, panelMonitor);
        chkY += chkSpacing;

        _chkDllMonitor = CreateMonitorCheckbox("📦 Мониторинг DLL",
            "Отслеживание загрузки библиотек (DLL)", chkY, panelMonitor);
        chkY += chkSpacing;

        _chkMemoryMonitor = CreateMonitorCheckbox("💾 Мониторинг памяти",
            "Отслеживание использования CPU и памяти", chkY, panelMonitor);

        // ===== Кнопка применить =====
        _btnApply = new Button
        {
            Text = "✓ Применить",
            Location = new Point(160, 485),
            Size = new Size(120, 38),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(0, 120, 215),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _btnApply.FlatAppearance.BorderSize = 0;
        _btnApply.Click += BtnApply_Click;
        Controls.Add(_btnApply);
    }

    private CheckBox CreateMonitorCheckbox(string title, string desc, int y, Control parent)
    {
        var chk = new CheckBox
        {
            Text = title,
            Location = new Point(15, y),
            Size = new Size(360, 20),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(50, 50, 60),
            Checked = true,
            Parent = parent
        };

        var lblDesc = new Label
        {
            Text = desc,
            Location = new Point(35, y + 18),
            Size = new Size(340, 16),
            Font = new Font("Segoe UI", 8F),
            ForeColor = Color.Gray,
            Parent = parent
        };

        return chk;
    }

    private void BtnApply_Click(object? sender, EventArgs e)
    {
        MonitorProcesses = _chkProcessMonitor.Checked;
        MonitorFiles = _chkFileMonitor.Checked;
        MonitorRegistry = _chkRegistryMonitor.Checked;
        MonitorNetwork = _chkNetworkMonitor.Checked;
        MonitorDlls = _chkDllMonitor.Checked;
        MonitorMemory = _chkMemoryMonitor.Checked;

        DialogResult = DialogResult.OK;
        Close();
    }

    private Image CreateIcon()
    {
        var bmp = new Bitmap(48, 48);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var path = new GraphicsPath();
        path.AddPolygon(new[]
        {
            new Point(24, 3),
            new Point(45, 13),
            new Point(45, 31),
            new Point(24, 45),
            new Point(3, 31),
            new Point(3, 13)
        });

        var brush = new LinearGradientBrush(
            new Point(0, 0), new Point(48, 48),
            Color.FromArgb(0, 120, 215),
            Color.FromArgb(0, 180, 255)
        );
        g.FillPath(brush, path);
        g.DrawString("🛡", new Font("Segoe UI Emoji", 20), Brushes.White, new Point(12, 10));

        return bmp;
    }
}
