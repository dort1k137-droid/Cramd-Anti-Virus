using System.Drawing;
using System.Windows.Forms;
using ProgramLauncher.Models;
using ProgramLauncher.Services;

namespace ProgramLauncher;

public class SafeAnalysisForm : Form
{
    private bool _isDarkTheme = true;
    private readonly SandboxAnalyzer _analyzer;

    // Цвета темы
    private Color _bgColor;
    private Color _textColor;
    private Color _panelColor;
    private Color _cardColor;
    private Color _dimTextColor;
    private Color _accentColor;

    // UI
    private Panel _panelHeader;
    private Label _lblFileName;
    private Label _lblRiskScore;
    private Panel _panelRiskIndicator;
    private Button _btnStartAnalysis;
    private Button _btnExport;
    private Button _btnClose;
    private ProgressBar _progressAnalysis;
    private Label _lblStatus;

    // Шкала угроз
    private Panel _panelThreatScale;

    // Вкладки
    private TabControl _tabControl;
    private TabPage _tabOverview;
    private TabPage _tabTimeline;
    private TabPage _tabProcesses;
    private TabPage _tabFiles;
    private TabPage _tabNetwork;
    private TabPage _tabSimple;

    private FlowLayoutPanel _panelStats;
    private RichTextBox _txtTimeline;
    private TreeView _treeProcesses;
    private ListView _listFiles;
    private ListView _listNetwork;
    private FlowLayoutPanel _panelSimpleExplanation;

    private string _filePath = "";
    private ThreatAssessment? _currentAssessment;
    private readonly List<MonitorEvent> _events = new();

    public SafeAnalysisForm()
    {
        _analyzer = new SandboxAnalyzer();
        _analyzer.OnEvent += OnAnalysisEvent;
        _analyzer.OnAssessmentComplete += OnAssessmentComplete;

        InitColors();
        InitializeComponents();
        ApplyTheme();
    }

    private void InitColors()
    {
        UpdateColors();
    }

    private void UpdateColors()
    {
        if (_isDarkTheme)
        {
            _bgColor = Color.FromArgb(25, 25, 35);
            _textColor = Color.White;
            _panelColor = Color.FromArgb(35, 35, 48);
            _cardColor = Color.FromArgb(45, 45, 58);
            _dimTextColor = Color.FromArgb(170, 170, 185);
            _accentColor = Color.FromArgb(0, 180, 255);
        }
        else
        {
            _bgColor = Color.FromArgb(245, 245, 250);
            _textColor = Color.FromArgb(30, 30, 40);
            _panelColor = Color.White;
            _cardColor = Color.FromArgb(240, 240, 245);
            _dimTextColor = Color.DimGray;
            _accentColor = Color.FromArgb(0, 120, 215);
        }
    }

    private void InitializeComponents()
    {
        Text = "🛡️ Безопасный анализ — Cramd Anti-Virus";
        Size = new Size(960, 760);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;

        // ===== Header (Dock=Top, высота 70) =====
        _panelHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 70,
            Padding = new Padding(0)
        };

        _lblFileName = new Label
        {
            Text = "Выберите файл для анализа",
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            Location = new Point(15, 8),
            Size = new Size(500, 25),
            AutoEllipsis = true
        };
        _panelHeader.Controls.Add(_lblFileName);

        _lblStatus = new Label
        {
            Text = "Готов к анализу",
            Location = new Point(15, 38),
            Size = new Size(400, 20),
            Font = new Font("Segoe UI", 9F)
        };
        _panelHeader.Controls.Add(_lblStatus);

        // Индикатор риска
        _panelRiskIndicator = new Panel
        {
            Location = new Point(530, 10),
            Size = new Size(160, 50),
            BackColor = Color.Gray
        };

        _lblRiskScore = new Label
        {
            Text = "0/100",
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            Dock = DockStyle.Fill,
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter
        };
        _panelRiskIndicator.Controls.Add(_lblRiskScore);
        _panelHeader.Controls.Add(_panelRiskIndicator);

        // Кнопки в header — выстроены в ряд без наложений
        _btnExport = new Button
        {
            Text = "📄 Экспорт",
            Location = new Point(700, 10),
            Size = new Size(110, 24),
            Font = new Font("Segoe UI", 9F),
            BackColor = Color.FromArgb(76, 175, 80),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _btnExport.FlatAppearance.BorderSize = 0;
        _btnExport.Click += BtnExport_Click;
        _panelHeader.Controls.Add(_btnExport);

        _btnStartAnalysis = new Button
        {
            Text = "▶ Начать анализ",
            Location = new Point(700, 38),
            Size = new Size(110, 24),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _btnStartAnalysis.FlatAppearance.BorderSize = 0;
        _btnStartAnalysis.Click += BtnStartAnalysis_Click;
        _panelHeader.Controls.Add(_btnStartAnalysis);

        _btnClose = new Button
        {
            Text = "✕ Закрыть",
            Location = new Point(820, 38),
            Size = new Size(110, 24),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            BackColor = Color.FromArgb(220, 50, 50),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _btnClose.FlatAppearance.BorderSize = 0;
        _btnClose.Click += (s, e) => Close();
        _panelHeader.Controls.Add(_btnClose);

        // Прогресс-бар (под header)
        _progressAnalysis = new ProgressBar
        {
            Dock = DockStyle.Top,
            Height = 4,
            Style = ProgressBarStyle.Marquee,
            Visible = false
        };

        // ===== Шкала угроз (Dock=Top, высота 130) =====
        _panelThreatScale = new Panel
        {
            Dock = DockStyle.Top,
            Height = 130,
            Padding = new Padding(15, 5, 15, 5)
        };

        var lblScaleTitle = new Label
        {
            Text = "Шкала угроз",
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Location = new Point(15, 5),
            Size = new Size(200, 20)
        };
        _panelThreatScale.Controls.Add(lblScaleTitle);

        // Четыре строки шкалы
        var scaleItems = new[]
        {
            ("🟢", "0–20", "Запуск программы", Color.FromArgb(76, 175, 80)),
            ("🟡", "21–40", "Создан новый процесс", Color.FromArgb(255, 193, 7)),
            ("🟠", "41–60", "Подключение к интернету", Color.FromArgb(255, 152, 0)),
            ("🔴", "61–100", "Изменение автозагрузки", Color.FromArgb(244, 67, 54))
        };

        for (int i = 0; i < scaleItems.Length; i++)
        {
            var (emoji, range, desc, color) = scaleItems[i];
            int y = 28 + i * 24;

            var lblDot = new Label
            {
                Text = emoji,
                Location = new Point(15, y),
                Size = new Size(25, 20),
                Font = new Font("Segoe UI", 10F),
                TextAlign = ContentAlignment.MiddleLeft
            };
            _panelThreatScale.Controls.Add(lblDot);

            var lblRange = new Label
            {
                Text = range,
                Location = new Point(45, y),
                Size = new Size(70, 20),
                Font = new Font("Consolas", 9F, FontStyle.Bold),
                ForeColor = color,
                TextAlign = ContentAlignment.MiddleLeft
            };
            _panelThreatScale.Controls.Add(lblRange);

            var lblArrow = new Label
            {
                Text = "→",
                Location = new Point(120, y),
                Size = new Size(20, 20),
                Font = new Font("Segoe UI", 9F),
                TextAlign = ContentAlignment.MiddleLeft
            };
            _panelThreatScale.Controls.Add(lblArrow);

            var lblDesc = new Label
            {
                Text = desc,
                Location = new Point(145, y),
                Size = new Size(300, 20),
                Font = new Font("Segoe UI", 9F),
                TextAlign = ContentAlignment.MiddleLeft
            };
            _panelThreatScale.Controls.Add(lblDesc);
        }

        // ===== Tabs (Dock=Fill) =====
        _tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F),
            Padding = new Point(12, 8)
        };

        // Обзор
        _tabOverview = new TabPage("📊 Обзор");
        _tabOverview.Padding = new Padding(15);

        _panelStats = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true
        };
        _tabOverview.Controls.Add(_panelStats);

        // Таймлайн
        _tabTimeline = new TabPage("⏱️ Таймлайн");
        _tabTimeline.Padding = new Padding(10);

        _txtTimeline = new RichTextBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 9F),
            ReadOnly = true,
            BorderStyle = BorderStyle.None
        };
        _tabTimeline.Controls.Add(_txtTimeline);

        // Процессы
        _tabProcesses = new TabPage("🚀 Процессы");
        _tabProcesses.Padding = new Padding(10);

        _treeProcesses = new TreeView
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9F)
        };
        _tabProcesses.Controls.Add(_treeProcesses);

        // Файлы
        _tabFiles = new TabPage("📁 Файлы");
        _tabFiles.Padding = new Padding(10);

        _listFiles = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Font = new Font("Segoe UI", 9F)
        };
        _listFiles.Columns.Add("Действие", 80);
        _listFiles.Columns.Add("Путь", 450);
        _listFiles.Columns.Add("Время", 120);
        _tabFiles.Controls.Add(_listFiles);

        // Сеть
        _tabNetwork = new TabPage("🌐 Сеть");
        _tabNetwork.Padding = new Padding(10);

        _listNetwork = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Font = new Font("Segoe UI", 9F)
        };
        _listNetwork.Columns.Add("Тип", 80);
        _listNetwork.Columns.Add("Адрес/Порт", 250);
        _listNetwork.Columns.Add("Время", 120);
        _tabNetwork.Controls.Add(_listNetwork);

        // Что делает программа
        _tabSimple = new TabPage("❓ Что делает программа?");
        _tabSimple.Padding = new Padding(15);

        _panelSimpleExplanation = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true
        };
        _tabSimple.Controls.Add(_panelSimpleExplanation);

        // Порядок добавления: Fill сначала, потом Top-панели снизу-вверх
        Controls.Add(_tabControl);
        Controls.Add(_panelThreatScale);
        Controls.Add(_progressAnalysis);
        Controls.Add(_panelHeader);
    }

    private async void BtnStartAnalysis_Click(object? sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog
        {
            Filter = "Исполняемые файлы (*.exe)|*.exe|Все файлы (*.*)|*.*",
            Title = "Выберите файл для анализа"
        };

        if (dlg.ShowDialog() != DialogResult.OK) return;

        _filePath = dlg.FileName;
        _lblFileName.Text = $"Анализ: {Path.GetFileName(_filePath)}";
        _btnStartAnalysis.Enabled = false;
        _progressAnalysis.Visible = true;
        _lblStatus.Text = "Идёт анализ...";

        _txtTimeline.Clear();
        _panelStats.Controls.Clear();
        _panelSimpleExplanation.Controls.Clear();
        _treeProcesses.Nodes.Clear();
        _listFiles.Items.Clear();
        _listNetwork.Items.Clear();
        _events.Clear();

        _currentAssessment = await _analyzer.AnalyzeAsync(_filePath);
    }

    private void OnAnalysisEvent(MonitorEvent evt)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => OnAnalysisEvent(evt)));
            return;
        }

        _events.Add(evt);

        _txtTimeline.AppendText($"[{evt.Timestamp:HH:mm:ss}] {evt.Type.GetEmoji()} {evt.Description}\n");

        if (evt.Type is EventType.FileCreate or EventType.FileModify or EventType.FileDelete)
        {
            var item = new ListViewItem(evt.Type.GetEmoji().ToString());
            item.SubItems.Add(evt.Description);
            item.SubItems.Add(evt.Timestamp.ToString("HH:mm:ss"));
            _listFiles.Items.Add(item);
        }

        if (evt.Type is EventType.NetworkConnect or EventType.NetworkListen)
        {
            var item = new ListViewItem(evt.Type.GetEmoji().ToString());
            item.SubItems.Add(evt.Description);
            item.SubItems.Add(evt.Timestamp.ToString("HH:mm:ss"));
            _listNetwork.Items.Add(item);
        }

        if (evt.Type == EventType.ProcessStart && !string.IsNullOrEmpty(evt.ProcessName))
        {
            _treeProcesses.Nodes.Add(new TreeNode($"{evt.Type.GetEmoji()} {evt.ProcessName} (PID: {evt.PID})"));
        }

        _lblStatus.Text = $"Анализ... событий: {_events.Count}";
    }

    private void OnAssessmentComplete(ThreatAssessment assessment)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => OnAssessmentComplete(assessment)));
            return;
        }

        _progressAnalysis.Visible = false;
        _btnStartAnalysis.Enabled = true;
        _lblStatus.Text = "Анализ завершён";

        UpdateRiskIndicator(assessment);
        ShowStatistics(assessment);
        ShowSimpleExplanations();
    }

    private void UpdateRiskIndicator(ThreatAssessment assessment)
    {
        _lblRiskScore.Text = $"{assessment.RiskScore}/100";
        _panelRiskIndicator.BackColor = assessment.RiskLevel.GetColor();
    }

    // ==========================================
    //  ОБЗОР — карточки статистики
    // ==========================================
    private void ShowStatistics(ThreatAssessment assessment)
    {
        _panelStats.Controls.Clear();

        AddStatCard("📁 Анализ файла", $"{assessment.FileAnalysisScore}/25",
            assessment.IsSigned ? "✅ Подписан" : "⚠️ Без подписи");
        AddStatCard("🚀 Поведение", $"{assessment.BehaviorScore}/25",
            $"{assessment.ProcessCreations.Count} процессов");
        AddStatCard("🌐 Сеть", $"{assessment.NetworkScore}/25",
            $"{assessment.NetworkConnections.Count} подключений");
        AddStatCard("⚙️ Система", $"{assessment.SystemScore}/25",
            $"{assessment.RegistryChanges.Count} изм. реестра");

        AddSection("Рекомендации", assessment.Recommendations);
        AddSection("Подозрительные строки", assessment.SuspiciousStrings.Select(s => $"⚠️ {s}").ToList());
    }

    private void AddStatCard(string title, string value, string subtitle)
    {
        var panel = new TableLayoutPanel
        {
            Size = new Size(820, 55),
            Margin = new Padding(0, 0, 0, 8),
            BackColor = _cardColor,
            ColumnCount = 3,
            RowCount = 1,
            Padding = new Padding(12, 8, 12, 8)
        };

        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220F));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        var lblTitle = new Label
        {
            Text = title,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F),
            ForeColor = _dimTextColor,
            TextAlign = ContentAlignment.MiddleLeft
        };
        panel.Controls.Add(lblTitle, 0, 0);

        var lblValue = new Label
        {
            Text = value,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            ForeColor = _textColor,
            TextAlign = ContentAlignment.MiddleLeft
        };
        panel.Controls.Add(lblValue, 1, 0);

        var lblSubtitle = new Label
        {
            Text = subtitle,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9F),
            ForeColor = _dimTextColor,
            TextAlign = ContentAlignment.MiddleLeft
        };
        panel.Controls.Add(lblSubtitle, 2, 0);

        _panelStats.Controls.Add(panel);
    }

    private void AddSection(string title, List<string> items)
    {
        if (items.Count == 0) return;

        var lblTitle = new Label
        {
            Text = title + ":",
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = _accentColor,
            AutoSize = true,
            Margin = new Padding(0, 15, 0, 8),
            MaximumSize = new Size(820, 0)
        };
        _panelStats.Controls.Add(lblTitle);

        foreach (var item in items)
        {
            var lbl = new Label
            {
                Text = $"•  {item}",
                Font = new Font("Segoe UI", 9F),
                ForeColor = _dimTextColor,
                AutoSize = false,
                Size = new Size(820, 22),
                AutoEllipsis = true,
                Margin = new Padding(10, 2, 0, 2),
                Padding = new Padding(5, 0, 5, 0),
                TextAlign = ContentAlignment.MiddleLeft
            };
            _panelStats.Controls.Add(lbl);
        }
    }

    // ==========================================
    //  ПРОСТЫЕ ОБЪЯСНЕНИЯ
    // ==========================================
    private void ShowSimpleExplanations()
    {
        _panelSimpleExplanation.Controls.Clear();

        var explanations = new List<(string emoji, string title, string desc)>();

        if (_events.Any(e => e.Type == EventType.NetworkConnect))
            explanations.Add(("🌐", "Подключалась к интернету", "Программа отправляла или получала данные из сети"));

        if (_events.Any(e => e.Type is EventType.FileCreate or EventType.FileModify))
            explanations.Add(("📄", "Создавала или изменяла файлы", "Программа работала с файлами на диске"));

        if (_events.Any(e => e.Type is EventType.RegistryCreate or EventType.RegistryModify))
            explanations.Add(("🔧", "Изменяла настройки Windows", "Программа вносила изменения в системный реестр"));

        if (_events.Any(e => e.Type == EventType.AutoStart))
            explanations.Add(("⚡", "Добавила себя в автозагрузку", "Программа будет запускаться автоматически при старте Windows"));

        if (_events.Any(e => e.Type == EventType.ProcessStart))
            explanations.Add(("🚀", "Запускала другие программы", "Программа создавала дочерние процессы"));

        if (explanations.Count == 0)
            explanations.Add(("✅", "Ничего подозрительного не обнаружено", "Программа вела себя нормально"));

        foreach (var (emoji, title, desc) in explanations)
        {
            var card = new Panel
            {
                Size = new Size(820, 60),
                Margin = new Padding(0, 0, 0, 10),
                BackColor = _cardColor,
                Padding = new Padding(15, 8, 15, 8)
            };

            var lblEmoji = new Label
            {
                Text = emoji,
                Location = new Point(10, 8),
                Size = new Size(35, 44),
                Font = new Font("Segoe UI Emoji", 18F),
                TextAlign = ContentAlignment.MiddleCenter
            };
            card.Controls.Add(lblEmoji);

            var lblTitle = new Label
            {
                Text = title,
                Location = new Point(55, 5),
                Size = new Size(750, 24),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = _textColor,
                TextAlign = ContentAlignment.MiddleLeft
            };
            card.Controls.Add(lblTitle);

            var lblDesc = new Label
            {
                Text = desc,
                Location = new Point(55, 30),
                Size = new Size(750, 22),
                Font = new Font("Segoe UI", 9F),
                ForeColor = _dimTextColor,
                TextAlign = ContentAlignment.MiddleLeft
            };
            card.Controls.Add(lblDesc);

            _panelSimpleExplanation.Controls.Add(card);
        }
    }

    // ==========================================
    //  ЭКСПОРТ ОТЧЁТА
    // ==========================================
    private void BtnExport_Click(object? sender, EventArgs e)
    {
        if (_currentAssessment == null)
        {
            MessageBox.Show("Сначала проведите анализ", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dlg = new SaveFileDialog
        {
            Filter = "Текстовый файл (*.txt)|*.txt",
            FileName = $"Cramd_Report_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
        };

        if (dlg.ShowDialog() != DialogResult.OK) return;

        var report = GenerateReport();
        File.WriteAllText(dlg.FileName, report);
        MessageBox.Show($"Отчёт сохранён:\n{dlg.FileName}", "Экспорт", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private string GenerateReport()
    {
        if (_currentAssessment == null) return "Нет данных для экспорта";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("═══════════════════════════════════════════════════════");
        sb.AppendLine("  Cramd Anti-Virus — Отчёт о безопасном анализе");
        sb.AppendLine("═══════════════════════════════════════════════════════\n");
        sb.AppendLine($"Дата: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
        sb.AppendLine($"Файл: {_currentAssessment.FilePath}");
        sb.AppendLine($"Размер: {_currentAssessment.FileSize} байт");
        sb.AppendLine($"MD5: {_currentAssessment.MD5}");
        sb.AppendLine($"SHA256: {_currentAssessment.SHA256}\n");
        sb.AppendLine($"═══════════════════════════════════════════════════════");
        sb.AppendLine($"  ОЦЕНКА РИСКА: {_currentAssessment.RiskScore}/100 ({_currentAssessment.RiskLevel.GetEmoji()} {_currentAssessment.RiskLevel.GetLabel()})");
        sb.AppendLine($"═══════════════════════════════════════════════════════\n");
        sb.AppendLine($"Подписан: {(_currentAssessment.IsSigned ? _currentAssessment.Publisher : "Нет")}");
        sb.AppendLine($"Упаковщик: {(_currentAssessment.IsPacked ? _currentAssessment.PackerName : "Нет")}\n");
        sb.AppendLine("═══════════════════════════════════════════════════════");
        sb.AppendLine("  СОБЫТИЯ");
        sb.AppendLine("═══════════════════════════════════════════════════════\n");

        foreach (var evt in _events)
        {
            sb.AppendLine($"[{evt.Timestamp:HH:mm:ss}] {evt.Type.GetEmoji()} {evt.Severity} - {evt.Description}");
        }

        sb.AppendLine("\n═══════════════════════════════════════════════════════");
        sb.AppendLine("  РЕКОМЕНДАЦИИ");
        sb.AppendLine("═══════════════════════════════════════════════════════\n");

        foreach (var rec in _currentAssessment.Recommendations)
        {
            sb.AppendLine($"• {rec}");
        }

        return sb.ToString();
    }

    // ==========================================
    //  ТЕМА
    // ==========================================
    public void SetTheme(bool isDark)
    {
        _isDarkTheme = isDark;
        UpdateColors();
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        BackColor = _bgColor;
        ForeColor = _textColor;

        _panelHeader.BackColor = _panelColor;
        _lblFileName.ForeColor = _textColor;
        _lblStatus.ForeColor = _dimTextColor;

        _panelThreatScale.BackColor = _panelColor;
        foreach (Control c in _panelThreatScale.Controls)
        {
            if (c is Label lbl)
            {
                if (lbl.Font.Bold)
                    lbl.ForeColor = _textColor;
                else
                    lbl.ForeColor = _dimTextColor;
            }
        }

        _tabControl.BackColor = _panelColor;
        _tabControl.ForeColor = _textColor;
        foreach (TabPage tab in _tabControl.Controls)
        {
            tab.BackColor = _bgColor;
            tab.ForeColor = _textColor;
        }

        _txtTimeline.BackColor = _isDarkTheme ? Color.FromArgb(20, 20, 30) : Color.White;
        _txtTimeline.ForeColor = _isDarkTheme ? Color.FromArgb(180, 255, 180) : Color.FromArgb(30, 80, 30);

        _treeProcesses.BackColor = _panelColor;
        _treeProcesses.ForeColor = _textColor;

        _listFiles.BackColor = _panelColor;
        _listFiles.ForeColor = _textColor;
        _listNetwork.BackColor = _panelColor;
        _listNetwork.ForeColor = _textColor;

        _panelStats.BackColor = _bgColor;
        _panelSimpleExplanation.BackColor = _bgColor;
    }
}
