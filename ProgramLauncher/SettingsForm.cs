using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ProgramLauncher;

public class SettingsForm : Form
{
    private bool _isDarkTheme = true;
    private Panel _panelTheme;
    private RadioButton _rbDark;
    private RadioButton _rbLight;
    private Button _btnApply;
    private Label _lblTitle;
    private PictureBox _pbIcon;

    public bool IsDarkTheme => _isDarkTheme;

    public SettingsForm(bool initialDark)
    {
        _isDarkTheme = initialDark;
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // Form
        Text = "Настройки";
        Size = new Size(400, 320);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(250, 250, 252);
        
        // Icon
        _pbIcon = new PictureBox
        {
            Location = new Point(25, 20),
            Size = new Size(56, 56),
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = CreateIcon()
        };
        Controls.Add(_pbIcon);

        // Title
        _lblTitle = new Label
        {
            Text = "Cramd Anti-Virus",
            Location = new Point(95, 25),
            Size = new Size(280, 30),
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 30, 40)
        };
        Controls.Add(_lblTitle);

        // Theme panel
        _panelTheme = new Panel
        {
            Location = new Point(25, 95),
            Size = new Size(330, 130),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        Controls.Add(_panelTheme);

        // Theme label
        var lblTheme = new Label
        {
            Text = "Тема оформления",
            Location = new Point(20, 15),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(50, 50, 60),
            Parent = _panelTheme
        };

        // Dark theme radio
        _rbDark = new RadioButton
        {
            Text = "🌙 Тёмная тема",
            Location = new Point(20, 45),
            Size = new Size(140, 30),
            Font = new Font("Segoe UI", 10F),
            Checked = _isDarkTheme,
            Parent = _panelTheme
        };
        _rbDark.CheckedChanged += (s, e) => { if (_rbDark.Checked) _isDarkTheme = true; };

        // Light theme radio
        _rbLight = new RadioButton
        {
            Text = "☀️ Светлая тема",
            Location = new Point(170, 45),
            Size = new Size(140, 30),
            Font = new Font("Segoe UI", 10F),
            Checked = !_isDarkTheme,
            Parent = _panelTheme
        };
        _rbLight.CheckedChanged += (s, e) => { if (_rbLight.Checked) _isDarkTheme = false; };

        // Apply button
        _btnApply = new Button
        {
            Text = "✓ Применить",
            Location = new Point(140, 245),
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

        // Version label
        var lblVersion = new Label
        {
            Text = "Версия 1.0.0",
            Location = new Point(25, 285),
            Font = new Font("Segoe UI", 8F),
            ForeColor = Color.Gray
        };
        Controls.Add(lblVersion);
    }

    private void BtnApply_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }

    private Image CreateIcon()
    {
        var bmp = new Bitmap(56, 56);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        
        var path = new GraphicsPath();
        path.AddPolygon(new[]
        {
            new Point(28, 4),
            new Point(52, 16),
            new Point(52, 36),
            new Point(28, 52),
            new Point(4, 36),
            new Point(4, 16)
        });
        
        var brush = new LinearGradientBrush(
            new Point(0, 0), new Point(56, 56),
            Color.FromArgb(0, 120, 215),
            Color.FromArgb(0, 180, 255)
        );
        g.FillPath(brush, path);
        g.DrawString("🛡", new Font("Segoe UI Emoji", 24), Brushes.White, new Point(14, 12));
        
        return bmp;
    }
}
