using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using ProgramLauncher.Models;
using ProgramLauncher.Services;

namespace ProgramLauncher;

public partial class Form1 : Form
{
    private Process? _embeddedProcess;
    private IntPtr _embeddedHandle = IntPtr.Zero;
    private IntPtr _originalParent = IntPtr.Zero;
    private bool _isDarkTheme = true;
    private System.Windows.Forms.Timer? _processMonitorTimer;

    // Настройки мониторинга
    private bool _monitorProcesses = true;
    private bool _monitorFiles = true;
    private bool _monitorRegistry = true;
    private bool _monitorNetwork = true;
    private bool _monitorDlls = true;
    private bool _monitorMemory = true;

    // --- База "чёрных" хешей (примеры) ---
    private static readonly HashSet<string> BlacklistedHashes = new(StringComparer.OrdinalIgnoreCase)
    {
        // Примеры — добавляй реальные хеши вредоносных файлов
        // "d41d8cd98f00b204e9800998ecf8427e", // пример MD5
    };

    // --- Подозрительные расширения ---
    private static readonly HashSet<string> SuspiciousExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".bat", ".cmd", ".vbs", ".js", ".ps1", ".wsf", ".msi",
        ".scr", ".pif", ".com", ".dll", ".inf", ".reg"
    };

    // --- Подозрительные имена ---
    private static readonly string[] SuspiciousNames =
    {
        "keylog", "inject", "hook", "steal", "phish", "malware",
        "trojan", "virus", "worm", "backdoor", "rootkit", "exploit",
        "ransomware", "cryptominer", "botnet", "rat_", "c2_", "beacon"
    };

    // --- Win32 API для встраивания окна ---
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool IsWow64Process(IntPtr hProcess, [Out] out bool wow64Process);

    private const int GWL_STYLE = -16;
    private const int WS_CAPTION = 0x00C00000;
    private const int WS_THICKFRAME = 0x00040000;
    private const int WS_MINIMIZEBOX = 0x00020000;
    private const int WS_MAXIMIZEBOX = 0x00010000;
    private const int WS_SYSMENU = 0x00080000;
    private const int SW_SHOWMAXIMIZED = 3;

    // --- Результаты сканирования ---
    private class ScanResult
    {
        public bool IsClean { get; set; }
        public string ThreatLevel { get; set; } = "UNKNOWN"; // CLEAN, LOW, MEDIUM, HIGH, CRITICAL
        public List<string> Warnings { get; set; } = new();
        public List<string> Blocks { get; set; } = new();
        public string? Md5 { get; set; }
        public string? Sha256 { get; set; }
        public long FileSize { get; set; }
        public string? Publisher { get; set; }
        public bool IsAdmin { get; set; }
        public Dictionary<string, object> Details { get; set; } = new();
    }

    public Form1()
    {
        InitializeComponent();
        _processMonitorTimer = new System.Windows.Forms.Timer();
        _processMonitorTimer.Interval = 1000;
        _processMonitorTimer.Tick += ProcessMonitorTimer_Tick;
        
        ApplyTheme();
        Log("🛡️  Cramd Anti-Virus инициализирован", Color.Cyan);
        Log("Мониторинг процессов активен", Color.Gray);
        Log("────────────────────────────────────────", Color.DarkGray);
    }

    private void Form1_Load(object? sender, EventArgs e)
    {
        _processMonitorTimer?.Start();
        UpdateIllustration("idle");
    }

    private void UpdateIllustration(string state)
    {
        if (pbIllustration == null) return;
        
        pbIllustration.Image = state switch
        {
            "running" => CreateRunningIllustration(),
            "scanning" => CreateScanningIllustration(),
            "threat" => CreateThreatIllustration(),
            _ => CreatePlaceholderIllustration()
        };

        lblIllustration.Text = state switch
        {
            "running" => $"Запущено: {_embeddedProcess?.ProcessName ?? "Процесс"}",
            "scanning" => "Идёт сканирование...",
            "threat" => "⚠️ Обнаружена угроза!",
            _ => "Запустите программу для отображения процесса"
        };
    }

    private Image CreateRunningIllustration()
    {
        var bmp = new Bitmap(200, 200);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.FromArgb(245, 245, 250));
        
        using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
            new Point(0, 0), new Point(200, 200),
            Color.FromArgb(0, 120, 215),
            Color.FromArgb(0, 180, 255)
        );
        
        g.FillEllipse(brush, new Rectangle(30, 30, 140, 140));
        g.DrawString("🖥️", new Font("Segoe UI Emoji", 48), Brushes.White, new Point(65, 65));
        
        return bmp;
    }

    private Image CreateScanningIllustration()
    {
        var bmp = new Bitmap(200, 200);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.FromArgb(245, 245, 250));
        
        using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
            new Point(0, 0), new Point(200, 200),
            Color.FromArgb(0, 150, 100),
            Color.FromArgb(0, 200, 150)
        );
        
        g.FillEllipse(brush, new Rectangle(30, 30, 140, 140));
        g.DrawString("🔍", new Font("Segoe UI Emoji", 48), Brushes.White, new Point(65, 65));
        
        return bmp;
    }

    private Image CreateThreatIllustration()
    {
        var bmp = new Bitmap(200, 200);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.FromArgb(245, 245, 250));
        
        using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
            new Point(0, 0), new Point(200, 200),
            Color.FromArgb(220, 50, 50),
            Color.FromArgb(255, 100, 100)
        );
        
        g.FillEllipse(brush, new Rectangle(30, 30, 140, 140));
        g.DrawString("⚠️", new Font("Segoe UI Emoji", 48), Brushes.White, new Point(65, 65));
        
        return bmp;
    }

    private void ProcessMonitorTimer_Tick(object? sender, EventArgs e)
    {
        if (_embeddedProcess != null && !_embeddedProcess.HasExited)
        {
            try
            {
                _embeddedProcess.Refresh();
                int mem = (int)(_embeddedProcess.WorkingSet64 / 1024);
                int cpu = 0;
                try
                {
                    cpu = (int)_embeddedProcess.TotalProcessorTime.TotalMilliseconds;
                }
                catch { }

                string status = $"📊 Процесс: {_embeddedProcess.ProcessName} (PID: {_embeddedProcess.Id}) | Память: {mem} КБ | Статус: Работает";
                LogProcess(status);
            }
            catch
            {
                // процесс завершился
            }
        }
    }

    // ==========================================
    //  ЛОГИРОВАНИЕ
    // ==========================================
    private void Log(string message, Color color)
    {
        if (txtLogs.InvokeRequired)
        {
            txtLogs.Invoke(new Action(() => Log(message, color)));
            return;
        }

        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        string line = $"[{timestamp}] {message}";

        txtLogs.AppendText(line + Environment.NewLine);

        // Красим последнюю добавленную строку
        int start = txtLogs.Text.Length - line.Length - Environment.NewLine.Length;
        if (start < 0) start = 0;
        int len = line.Length;
        if (len <= 0) len = 1;

        txtLogs.Select(start, len);
        txtLogs.SelectionColor = color;
        txtLogs.SelectionStart = txtLogs.Text.Length;
        txtLogs.SelectionLength = 0;
        txtLogs.SelectionColor = Color.Lime;

        txtLogs.ScrollToCaret();
    }

    private void LogInfo(string msg) => Log(msg, Color.LightGreen);
    private void LogWarn(string msg) => Log(msg, Color.Yellow);
    private void LogError(string msg) => Log(msg, Color.Red);
    private void LogProcess(string msg) => Log(msg, Color.Cyan);
    private void LogDetail(string msg) => Log(msg, Color.Gray);

    // ==========================================
    //  СКАНИРОВАНИЕ ФАЙЛА (АНТИВИРУС)
    // ==========================================
    private ScanResult ScanFile(string filePath)
    {
        var result = new ScanResult { IsClean = true };

        try
        {
            // --- 1. Проверка существования ---
            if (!File.Exists(filePath))
            {
                result.Blocks.Add("Файл не существует");
                result.IsClean = false;
                result.ThreatLevel = "CRITICAL";
                return result;
            }

            var fileInfo = new FileInfo(filePath);
            result.FileSize = fileInfo.Length;

            // --- 2. Проверка размера ---
            if (fileInfo.Length == 0)
            {
                result.Blocks.Add("Файл пустой (0 байт)");
                result.ThreatLevel = result.ThreatLevel == "UNKNOWN" ? "HIGH" : result.ThreatLevel;
                result.IsClean = false;
            }
            else if (fileInfo.Length < 1024)
            {
                result.Warnings.Add($"Подозрительно маленький размер: {FormatSize(fileInfo.Length)}");
                result.ThreatLevel = "MEDIUM";
            }

            if (fileInfo.Length > 500 * 1024 * 1024)
            {
                result.Warnings.Add($"Очень большой размер: {FormatSize(fileInfo.Length)}");
            }

            // --- 3. Проверка расширения ---
            string ext = fileInfo.Extension;
            if (SuspiciousExtensions.Contains(ext))
            {
                result.Warnings.Add($"Расширение '{ext}' относится к скриптовым/системным файлам");
                if (result.ThreatLevel == "UNKNOWN") result.ThreatLevel = "MEDIUM";
            }

            // --- 4. Проверка имени файла ---
            string nameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
            foreach (var suspicious in SuspiciousNames)
            {
                if (nameWithoutExt.Contains(suspicious, StringComparison.OrdinalIgnoreCase))
                {
                    result.Blocks.Add($"Подозрительное имя: содержит '{suspicious}'");
                    result.ThreatLevel = "HIGH";
                    result.IsClean = false;
                    break;
                }
            }

            // --- 5. Проверка пути ---
            string pathLower = filePath.ToLowerInvariant();
            if (pathLower.StartsWith(@"\\") || pathLower.StartsWith("http://") || pathLower.StartsWith("https://"))
            {
                result.Warnings.Add("Файл находится на сетевом/удалённом ресурсе");
                if (result.ThreatLevel == "UNKNOWN") result.ThreatLevel = "MEDIUM";
            }

            if (pathLower.Contains("temp") || pathLower.Contains("appdata") || pathLower.Contains("local"))
            {
                result.Warnings.Add("Файл находится во временной директории");
            }

            // --- 6. Вычисление хешей ---
            LogProcess("⏳ Вычисление хешей...");
            try
            {
                using var stream = File.OpenRead(filePath);
                result.Md5 = ComputeMD5(stream);
                stream.Position = 0;
                result.Sha256 = ComputeSHA256(stream);
                LogDetail($"MD5:    {result.Md5}");
                LogDetail($"SHA256: {result.Sha256}");
            }
            catch (Exception ex)
            {
                result.Warnings.Add($"Не удалось вычислить хеши: {ex.Message}");
            }

            // --- 7. Проверка по чёрному списку хешей ---
            if (!string.IsNullOrEmpty(result.Md5) && BlacklistedHashes.Contains(result.Md5))
            {
                result.Blocks.Add("Файл найден в чёрном списке (MD5 совпадение)");
                result.ThreatLevel = "CRITICAL";
                result.IsClean = false;
            }

            if (!string.IsNullOrEmpty(result.Sha256) && BlacklistedHashes.Contains(result.Sha256))
            {
                result.Blocks.Add("Файл найден в чёрном списке (SHA256 совпадение)");
                result.ThreatLevel = "CRITICAL";
                result.IsClean = false;
            }

            // --- 8. Проверка PE-заголовка ---
            LogProcess("⏳ Анализ PE-заголовка...");
            try
            {
                using var stream = File.OpenRead(filePath);
                var peInfo = AnalyzePE(stream, fileInfo.Length);
                result.Details["PE"] = peInfo;

                if (peInfo.ContainsKey("suspicious_sections"))
                {
                    var sections = (List<string>)peInfo["suspicious_sections"]!;
                    foreach (var sec in sections)
                    {
                        result.Warnings.Add($"Подозрительная секция: {sec}");
                    }
                    if (result.ThreatLevel == "UNKNOWN" || result.ThreatLevel == "LOW")
                        result.ThreatLevel = "MEDIUM";
                }

                if (peInfo.ContainsKey("is_dll") && (bool)peInfo["is_dll"]!)
                {
                    result.Warnings.Add("Файл является DLL-библиотекой");
                }

                if (peInfo.ContainsKey("is_64bit") && !(bool)peInfo["is_64bit"]!)
                {
                    result.Warnings.Add("32-битное приложение (на 64-бит системе)");
                }
            }
            catch (Exception ex)
            {
                LogDetail($"PE-анализ: {ex.Message}");
            }

            // --- 9. Проверка подписи ---
            LogProcess("⏳ Проверка цифровой подписи...");
            try
            {
                var signature = GetFileSignature(filePath);
                if (!string.IsNullOrEmpty(signature))
                {
                    result.Publisher = signature;
                    LogInfo($"✅ Подписан: {signature}");
                    if (result.ThreatLevel == "UNKNOWN")
                    {
                        result.ThreatLevel = "LOW";
                        result.IsClean = true;
                    }
                }
                else
                {
                    result.Warnings.Add("Файл не имеет цифровой подписи");
                    if (result.ThreatLevel == "UNKNOWN")
                        result.ThreatLevel = "MEDIUM";
                }
            }
            catch
            {
                // игнорируем ошибки подписи
            }

            // --- 10. Проверка прав администратора ---
            result.IsAdmin = IsRunningAsAdmin();
            if (result.IsAdmin)
            {
                LogWarn("⚠️ Приложение запущено от имени администратора");
            }

            // --- Итоговая оценка ---
            if (result.Blocks.Count > 0)
            {
                result.IsClean = false;
                result.ThreatLevel = result.Blocks.Any(b => b.Contains("чёрн") || b.Contains("совпад"))
                    ? "CRITICAL"
                    : "HIGH";
            }
            else if (result.ThreatLevel == "UNKNOWN")
            {
                result.ThreatLevel = "LOW";
                result.IsClean = true;
            }
        }
        catch (Exception ex)
        {
            result.Blocks.Add($"Ошибка сканирования: {ex.Message}");
            result.IsClean = false;
            result.ThreatLevel = "CRITICAL";
        }

        return result;
    }

    private string ComputeMD5(Stream stream)
    {
        using var md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    private string ComputeSHA256(Stream stream)
    {
        using var sha = SHA256.Create();
        byte[] hash = sha.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    private Dictionary<string, object> AnalyzePE(Stream stream, long fileSize)
    {
        var info = new Dictionary<string, object>();

        try
        {
            byte[] header = new byte[Math.Min(4096, (int)fileSize)];
            int bytesRead = stream.Read(header, 0, header.Length);
            if (bytesRead < header.Length)
            {
                Array.Resize(ref header, bytesRead);
            }

            // Проверка MZ-заголовка
            if (header.Length < 2 || header[0] != 'M' || header[1] != 'Z')
            {
                info["error"] = "Не является PE-файлом (нет MZ-заголовка)";
                return info;
            }

            // PE-смещение (DWORD по адресу 0x3C)
            if (header.Length < 0x40)
            {
                info["error"] = "Слишком короткий файл для PE";
                return info;
            }

            uint peOffset = BitConverter.ToUInt32(header, 0x3C);
            if (peOffset + 4 > header.Length)
            {
                // Читаем дальше
                Array.Resize(ref header, (int)Math.Max(header.Length * 2, peOffset + 256));
                stream.Position = 0;
                stream.Read(header, 0, header.Length);
            }

            if (header.Length < peOffset + 4)
            {
                info["error"] = "Невозможно прочитать PE-заголовок";
                return info;
            }

            uint peSig = BitConverter.ToUInt32(header, (int)peOffset);
            if (peSig != 0x00004550) // "PE\0\0"
            {
                info["error"] = "Неверный PE-сигнатур";
                return info;
            }

            // COFF Header (сразу после PE-сигнатуры)
            int coffOffset = (int)peOffset + 4;
            if (coffOffset + 20 > header.Length)
            {
                info["error"] = "Невозможно прочитать COFF-заголовок";
                return info;
            }

            ushort machine = BitConverter.ToUInt16(header, coffOffset);
            ushort numSections = BitConverter.ToUInt16(header, coffOffset + 2);
            ushort characteristics = BitConverter.ToUInt16(header, coffOffset + 18);

            info["machine"] = machine == 0x14c ? "x86" : machine == 0x8664 ? "x64" : machine.ToString("X4");
            info["is_64bit"] = machine == 0x8664;
            info["is_dll"] = (characteristics & 0x2000) != 0;
            info["sections_count"] = numSections;

            // Анализ секций
            int sectionHeaderSize = 40;
            int sectionOffset = coffOffset + 20;
            var suspiciousSections = new List<string>();

            if (sectionOffset + numSections * sectionHeaderSize <= header.Length)
            {
                for (int i = 0; i < numSections && (sectionOffset + sectionHeaderSize) <= header.Length; i++)
                {
                    string sectionName = Encoding.ASCII.GetString(header, sectionOffset, 8)
                        .Replace("\0", "").Trim();

                    uint virtualSize = BitConverter.ToUInt32(header, sectionOffset + 8);
                    uint sizeOfData = BitConverter.ToUInt32(header, sectionOffset + 16);

                    // Подозрительные имена секций
                    if (!string.IsNullOrEmpty(sectionName) &&
                        (sectionName.StartsWith(".") ||
                         sectionName.Contains("UPX") ||
                         sectionName.Contains("packed") ||
                         sectionName.Contains("crypto") ||
                         sectionName.Contains("shell")))
                    {
                        suspiciousSections.Add($"{sectionName} (VSize={virtualSize}, Size={sizeOfData})");
                    }

                    // Подозрительное соотношение размеров
                    if (sizeOfData > 0 && virtualSize > sizeOfData * 3)
                    {
                        suspiciousSections.Add($"{(string.IsNullOrEmpty(sectionName) ? "(unnamed)" : sectionName)} — большой VSize vs Size");
                    }

                    sectionOffset += sectionHeaderSize;
                }
            }

            if (suspiciousSections.Count > 0)
                info["suspicious_sections"] = suspiciousSections;
        }
        catch (Exception ex)
        {
            info["error"] = ex.Message;
        }

        return info;
    }

    private string? GetFileSignature(string filePath)
    {
        try
        {
            var assembly = System.Reflection.Assembly.LoadFrom(filePath);
            var attributes = assembly.GetCustomAttributes(false);

            foreach (var attr in attributes)
            {
                if (attr is System.Reflection.AssemblyCompanyAttribute comp)
                    return comp.Company;
                if (attr is System.Reflection.AssemblyProductAttribute prod)
                    return prod.Product;
            }
        }
        catch
        {
            // Не все .exe — .NET assembly
        }

        // Попытка через FileVersionInfo
        try
        {
            var fvi = FileVersionInfo.GetVersionInfo(filePath);
            if (!string.IsNullOrEmpty(fvi.CompanyName))
                return fvi.CompanyName;
        }
        catch { }

        return null;
    }

    private bool IsRunningAsAdmin()
    {
        using var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
        var principal = new System.Security.Principal.WindowsPrincipal(identity);
        return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
    }

    private string FormatSize(long bytes)
    {
        string[] sizes = { "Б", "КБ", "МБ", "ГБ" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    // ==========================================
    //  UI ОБРАБОТЧИКИ
    // ==========================================

    // --- Выбор файла для анализа/запуска ---
    private void BtnSelectFile_Click(object? sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog
        {
            Filter = "Исполняемые файлы (*.exe)|*.exe|Все файлы (*.*)|*.*",
            Title = "Выберите программу для анализа или запуска"
        };

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            lblSelectedFile.Text = dlg.FileName;
            lblSelectedFile.ForeColor = Color.FromArgb(0, 120, 215);
        }
    }

    // --- Запуск и встраивание ---
    private void BtnLaunch_Click(object? sender, EventArgs e)
    {
        string exePath = lblSelectedFile.Text.Trim();

        if (string.IsNullOrEmpty(exePath) || exePath == "Файл не выбран")
        {
            MessageBox.Show("Сначала выберите программу.", "Внимание",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!File.Exists(exePath))
        {
            MessageBox.Show("Файл не найден:\n" + exePath, "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // === АНТИВИРУС: Сканирование ===
        LogProcess("╔══════════════════════════════════════╗");
        LogProcess("║  🔍 ЗАПУСК СКАНИРОВАНИЯ ФАЙЛА       ║");
        LogProcess("╚══════════════════════════════════════╝");
        LogInfo($"📁 Файл: {exePath}");

        ScanResult scan = ScanFile(exePath);

        // Вывод результатов сканирования
        LogDetail($"   ├─ Размер: {FormatSize(scan.FileSize)}");
        LogDetail($"   ├─ Издатель: {(!string.IsNullOrEmpty(scan.Publisher) ? scan.Publisher : "Не подписан")}");

        foreach (var warn in scan.Warnings)
            LogWarn("⚠️  " + warn);

        foreach (var block in scan.Blocks)
            LogError("🚫 " + block);

        // Обновление статуса
        lblScanStatus.Text = $"Статус: {scan.ThreatLevel} | Чисто: {scan.IsClean}";

        if (scan.ThreatLevel == "CRITICAL")
        {
            lblScanStatus.ForeColor = Color.Red;
            MessageBox.Show(
                $"Файл заблокирован!\n\n" +
                $"Уровень угрозы: {scan.ThreatLevel}\n" +
                $"Причины:\n" + string.Join("\n", scan.Blocks),
                "🛡️  ОБНАРУЖЕНА УГРОЗА",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            LogError("❌ ЗАПУСК ЗАБЛОКИРОВАН — обнаружена угроза!");
            return;
        }
        else if (scan.ThreatLevel == "HIGH")
        {
            lblScanStatus.ForeColor = Color.OrangeRed;
            var res = MessageBox.Show(
                $"Обнаружены подозрительные признаки:\n\n" +
                string.Join("\n", scan.Warnings) +
                "\n\nЗапустить всё равно?",
                "⚠️  ПОДОЗРИТЕЛЬНЫЙ ФАЙЛ",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (res != DialogResult.Yes)
            {
                LogWarn("❌ Запуск отменён пользователем");
                return;
            }
        }
        else if (scan.ThreatLevel == "MEDIUM")
        {
            lblScanStatus.ForeColor = Color.Orange;
            LogWarn("⚠️  Файл имеет средние подозрительные признаки");
        }
        else
        {
            lblScanStatus.ForeColor = Color.Lime;
            LogInfo("✅ Файл проверен — признаков угрозы не обнаружено");
        }

        LogProcess("╔══════════════════════════════════════╗");
        LogProcess("║  🚀 ПОДГОТОВКА К ЗАПУСКУ ПРОЦЕССА   ║");
        LogProcess("╚══════════════════════════════════════╝");

        // --- Запуск ---
        CloseEmbedded();

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = true
            };

            LogProcess("╔══════════════════════════════════════╗");
            LogProcess("║  🚀 ЗАПУСК ПРОЦЕССА В СИСТЕМЕ       ║");
            LogProcess("╚══════════════════════════════════════╝");
            _embeddedProcess = Process.Start(psi);

            if (_embeddedProcess == null)
            {
                LogError("❌ Не удалось запустить процесс");
                MessageBox.Show("Не удалось запустить процесс.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LogInfo($"✅ Процесс запущен (PID: {_embeddedProcess.Id})");
            LogDetail($"   ├─ Имя: {_embeddedProcess.ProcessName}");
            LogDetail($"   ├─ Файл: {exePath}");

            // Ждём окно
            LogProcess("⏳ Ожидание появления окна программы...");
            if (!_embeddedProcess.WaitForInputIdle(10000))
            {
                LogWarn("⏳ WaitForInputIdle не удался, пробуем вручную...");
            }

            Thread.Sleep(500);

            if (_embeddedProcess.HasExited)
            {
                LogError("❌ Процесс завершился сразу после запуска");
                MessageBox.Show("Процесс завершился сразу после запуска.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _embeddedProcess = null;
                return;
            }

            IntPtr hWnd = _embeddedProcess.MainWindowHandle;

            int retries = 20;
            while (hWnd == IntPtr.Zero && retries-- > 0 && !_embeddedProcess.HasExited)
            {
                Thread.Sleep(250);
                _embeddedProcess.Refresh();
                hWnd = _embeddedProcess.MainWindowHandle;
            }

            if (hWnd == IntPtr.Zero)
            {
                LogWarn("⚠️ Не удалось найти окно программы (запущено без GUI)");
                return;
            }

            LogProcess("╔══════════════════════════════════════╗");
            LogProcess("║  🎯 ВСТРАИВАНИЕ ОКНА В ПРИЛОЖЕНИЕ   ║");
            LogProcess("╚══════════════════════════════════════╝");
            LogInfo($"   ├─ Окно найдено (HWND: {hWnd})");
            LogDetail("   ├─ Удаление рамки и заголовка окна...");
            EmbedWindow(hWnd);
            LogInfo("✅ Окно встроено успешно!");
            UpdateIllustration("running");
        }
        catch (Exception ex)
        {
            LogError($"❌ Ошибка запуска: {ex.Message}");
            MessageBox.Show("Ошибка запуска:\n" + ex.Message, "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void EmbedWindow(IntPtr hWnd)
    {
        _embeddedHandle = hWnd;
        _originalParent = IntPtr.Zero;

        int style = GetWindowLong(hWnd, GWL_STYLE);
        style &= ~(WS_CAPTION | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_SYSMENU);
        SetWindowLong(hWnd, GWL_STYLE, style);

        SetParent(hWnd, panelSafeHost.Handle);
        ShowWindow(hWnd, SW_SHOWMAXIMIZED);
        FitToPanel();
    }

    private void FitToPanel()
    {
        if (_embeddedHandle == IntPtr.Zero) return;
        MoveWindow(_embeddedHandle, 0, 0, panelSafeHost.ClientSize.Width, panelSafeHost.ClientSize.Height, true);
    }

    private void PanelHost_Resize(object? sender, EventArgs e)
    {
        FitToPanel();
    }

    // --- Закрытие встроенного окна ---
    private void BtnClose_Click(object? sender, EventArgs e)
    {
        CloseEmbedded();
    }

    private void CloseEmbedded()
    {
        if (_embeddedProcess != null)
        {
            LogProcess($"╔══════════════════════════════════════╗");
            LogProcess($"║  🛑 ЗАВЕРШЕНИЕ ПРОЦЕССА (PID: {_embeddedProcess.Id}) ║");
            LogProcess($"╚══════════════════════════════════════╝");
            try
            {
                if (!_embeddedProcess.HasExited)
                {
                    LogInfo("   ├─ Отправка команды закрытия...");
                    _embeddedProcess.CloseMainWindow();
                    if (!_embeddedProcess.WaitForExit(3000))
                    {
                        LogWarn("   ├─ Процесс не закрылся — принудительное завершение...");
                        _embeddedProcess.Kill();
                    }
                    else
                    {
                        LogInfo("   └─ Процесс закрыт корректно");
                    }
                }
            }
            catch
            {
                // игнорируем
            }
            _embeddedProcess.Dispose();
        }

        _embeddedProcess = null;
        _embeddedHandle = IntPtr.Zero;
    }

    private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
    {
        LogProcess("🛑 Закрытие приложения...");
        _processMonitorTimer?.Stop();
        CloseEmbedded();
    }

    // ==========================================
    //  НАСТРОЙКИ И ТЕМЫ
    // ==========================================
    private void PbSettings_Click(object? sender, EventArgs e)
    {
        using var settings = new SettingsForm(_isDarkTheme);
        if (settings.ShowDialog(this) == DialogResult.OK)
        {
            _isDarkTheme = settings.IsDarkTheme;
            _monitorProcesses = settings.MonitorProcesses;
            _monitorFiles = settings.MonitorFiles;
            _monitorRegistry = settings.MonitorRegistry;
            _monitorNetwork = settings.MonitorNetwork;
            _monitorDlls = settings.MonitorDlls;
            _monitorMemory = settings.MonitorMemory;
            ApplyTheme();
            LogInfo(_isDarkTheme ? "🌙 Тёмная тема применена" : "☀️ Светлая тема применена");
            LogDetail($"Мониторинг: P={_monitorProcesses} F={_monitorFiles} R={_monitorRegistry} N={_monitorNetwork} D={_monitorDlls} M={_monitorMemory}");
        }
    }

    private void ApplyTheme()
    {
        Color bgMain, bgPanel, bgInput, textMain, textDim, accent, logBg, logFg, bottomBar;
        
        if (_isDarkTheme)
        {
            bgMain = Color.FromArgb(25, 25, 35);
            bgPanel = Color.FromArgb(30, 30, 40);
            bgInput = Color.FromArgb(50, 50, 60);
            textMain = Color.White;
            textDim = Color.LightGray;
            accent = Color.FromArgb(0, 180, 255);
            logBg = Color.FromArgb(15, 15, 25);
            logFg = Color.Lime;
            bottomBar = Color.FromArgb(20, 20, 30);
        }
        else
        {
            bgMain = Color.FromArgb(245, 245, 250);
            bgPanel = Color.White;
            bgInput = Color.White;
            textMain = Color.FromArgb(30, 30, 40);
            textDim = Color.DimGray;
            accent = Color.FromArgb(0, 120, 215);
            logBg = Color.FromArgb(30, 30, 40);
            logFg = Color.FromArgb(0, 200, 150);
            bottomBar = Color.FromArgb(240, 240, 245);
        }
        
        // Основная форма
        BackColor = bgMain;
        ForeColor = textMain;
        
        // Вкладки
        tabControl.BackColor = bgPanel;
        tabControl.ForeColor = textMain;
        foreach (TabPage tab in tabControl.Controls)
        {
            tab.BackColor = bgPanel;
            tab.ForeColor = textMain;
        }
        
        // Сканер
        lblScanStatus.ForeColor = accent;
        lblScanResult.ForeColor = textMain;
        
        // Безопасный анализ
        panelSafeHost.BackColor = bgMain;
        lblIllustration.ForeColor = textDim;
        lblSelectedFile.ForeColor = textDim;
        lblSafeAnalysisDesc.ForeColor = textDim;
        if (_isDarkTheme)
        {
            btnSelectFile.BackColor = Color.FromArgb(0, 120, 215);
            btnSelectFile.ForeColor = Color.White;
            btnStartSafeAnalysis.BackColor = Color.FromArgb(76, 175, 80);
            btnStartSafeAnalysis.ForeColor = Color.White;
            btnLaunch.BackColor = Color.FromArgb(0, 120, 215);
            btnLaunch.ForeColor = Color.White;
            btnClose.BackColor = Color.FromArgb(220, 50, 50);
            btnClose.ForeColor = Color.White;
            btnScanFolder.BackColor = Color.FromArgb(0, 120, 215);
            btnScanDisk.BackColor = Color.FromArgb(0, 150, 100);
        }
        
        // Логи
        panelLogs.BackColor = logBg;
        txtLogs.BackColor = logBg;
        txtLogs.ForeColor = logFg;
        
        // Нижняя панель
        panelBottomBar.BackColor = bottomBar;
        pbSettings.Image = _isDarkTheme ? CreateSettingsIconDark() : CreateSettingsIcon();
        
        LogDetail(_isDarkTheme ? "🎨 Тёмная тема применена" : "🎨 Светлая тема применена");
    }

    private Image CreateSettingsIconDark()
    {
        var bmp = new Bitmap(26, 26);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.Transparent);
        g.DrawString("⚙", new Font(new FontFamily("Segoe UI"), 16), Brushes.White, new Point(2, 2));
        return bmp;
    }

    // ==========================================
    //  СКАНЕР УГРОЗ (ПАПКИ/ДИСКИ)
    // ==========================================
    private async void BtnScanFolder_Click(object? sender, EventArgs e)
    {
        if (folderBrowser.ShowDialog() == DialogResult.OK)
        {
            await ScanDirectory(folderBrowser.SelectedPath);
        }
    }

    private async void BtnScanDisk_Click(object? sender, EventArgs e)
    {
        if (cmbDrives.SelectedIndex >= 0)
        {
            // Извлекаем букву диска (C:\) из строки "C:\ (10ГБ/100ГБ)"
            string selectedItem = cmbDrives.SelectedItem.ToString()!;
            string drive = selectedItem.Substring(0, 3); // "C:\"
            LogInfo($"Начало сканирования диска: {drive}");
            await ScanDirectory(drive);
        }
    }

    private async Task ScanDirectory(string path)
    {
        UpdateIllustration("scanning");
        lblScanResult.Text = $"Сканирование: {path}\n";
        progressScan.Value = 0;
        progressScan.Style = ProgressBarStyle.Marquee;
        
        int totalFiles = 0;
        int threatCount = 0;
        int cleanCount = 0;
        int errorCount = 0;
        
        try
        {
            // Сначала считаем файлы (с игнорированием ошибок доступа)
            LogProcess($"⏳ Подсчёт файлов на диске {path}...");
            var files = GetFilesSafe(path, "*.exe");
            totalFiles = files.Count;
            progressScan.Style = ProgressBarStyle.Continuous;
            progressScan.Maximum = totalFiles > 0 ? totalFiles : 1;

            LogInfo($"Найдено {totalFiles} EXE файлов для проверки");

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                try
                {
                    var result = ScanFile(file);
                    
                    if (!result.IsClean)
                    {
                        threatCount++;
                        lblScanResult.Text = $"🚫 УГРОЗА: {file} ({result.ThreatLevel})\n" + lblScanResult.Text;
                    }
                    else
                    {
                        cleanCount++;
                    }
                }
                catch (Exception ex)
                {
                    errorCount++;
                    LogDetail($"⚠️ Ошибка проверки {file}: {ex.Message}");
                }
                
                progressScan.Value = Math.Min(i + 1, progressScan.Maximum);
                
                // Обновляем UI каждые 10 файлов
                if (i % 10 == 0)
                {
                    await Task.Delay(1);
                    lblScanResult.Text = $"Сканирование: {path}\n" +
                        $"Обработано: {i + 1}/{totalFiles} | " +
                        $"✅ Чистых: {cleanCount} | " +
                        $"🚫 Угроз: {threatCount} | " +
                        $"⚠️ Ошибок: {errorCount}\n\n" +
                        $"Последние угрозы:\n" +
                        lblScanResult.Text.Split('\n').Take(8).Aggregate((a, b) => a + "\n" + b);
                }
            }

            lblScanResult.Text = $"═══════════════════════════════════════\n";
            lblScanResult.Text += $"📊 РЕЗУЛЬТАТЫ СКАНИРОВАНИЯ\n";
            lblScanResult.Text += $"═══════════════════════════════════════\n";
            lblScanResult.Text += $"Путь: {path}\n";
            lblScanResult.Text += $"Всего файлов: {totalFiles}\n";
            lblScanResult.Text += $"✅ Чистых: {cleanCount}\n";
            lblScanResult.Text += $"🚫 Угроз: {threatCount}\n";
            lblScanResult.Text += $"⚠️ Ошибок доступа: {errorCount}\n";
            lblScanResult.Text += $"═══════════════════════════════════════\n";
            
            if (threatCount > 0)
            {
                UpdateIllustration("threat");
                LogError($"Сканирование завершено: обнаружено {threatCount} угроз!");
            }
            else
            {
                UpdateIllustration("scanning");
                LogInfo($"Сканирование завершено: угроз не обнаружено");
            }
        }
        catch (Exception ex)
        {
            lblScanResult.Text += $"\n❌ Критическая ошибка: {ex.Message}";
            LogError($"Ошибка сканирования: {ex.Message}");
        }
        
        UpdateIllustration("idle");
    }

    /// <summary>
    /// Безопасное получение списка файлов с игнорированием ошибок доступа
    /// </summary>
    private List<string> GetFilesSafe(string path, string pattern)
    {
        var files = new List<string>();
        
        try
        {
            // Пробуем получить файлы из текущей папки
            try
            {
                files.AddRange(Directory.GetFiles(path, pattern));
            }
            catch (UnauthorizedAccessException) { }
            catch (IOException) { }
            
            // Рекурсивно обходим подпапки
            string[] directories;
            try
            {
                directories = Directory.GetDirectories(path);
            }
            catch (UnauthorizedAccessException)
            {
                return files;
            }
            catch (IOException)
            {
                return files;
            }

            foreach (var dir in directories)
            {
                try
                {
                    files.AddRange(GetFilesSafe(dir, pattern));
                }
                catch (UnauthorizedAccessException) { }
                catch (IOException) { }
            }
        }
        catch { }
        
        return files;
    }

    // ==========================================
    //  БЕЗОПАСНЫЙ АНАЛИЗ
    // ==========================================
    private void BtnStartSafeAnalysis_Click(object? sender, EventArgs e)
    {
        var analysisForm = new SafeAnalysisForm();
        analysisForm.SetTheme(_isDarkTheme);
        analysisForm.ShowDialog(this);
    }
}
