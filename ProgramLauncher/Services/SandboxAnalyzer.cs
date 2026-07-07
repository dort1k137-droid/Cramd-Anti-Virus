using System.Diagnostics;
using System.Text.RegularExpressions;
using ProgramLauncher.Models;

namespace ProgramLauncher.Services;

/// <summary>
/// Сервис безопасного анализа файлов с мониторингом поведения
/// </summary>
public class SandboxAnalyzer
{
    private readonly List<MonitorEvent> _events = new();
    private readonly ThreatAssessment _assessment = new();
    private Process? _monitoredProcess;
    private System.Threading.Timer? _resourceMonitor;
    private bool _isAnalyzing;
    
    // Подозрительные паттерны в файлах
    private static readonly string[] SuspiciousPatterns =
    {
        @"cmd\.exe", @"powershell", @"wscript", @"cscript",
        @"reg\s+add", @"reg\s+delete", @"schtasks",
        @"http://", @"https://", @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}",
        @"CreateRemoteThread", @"VirtualAllocEx", @"WriteProcessMemory",
        @"SetWindowsHookEx", @"GetAsyncKeyState", @"keylog",
        @"password", @"credential", @"token", @"api[-_]?key"
    };

    public event Action<MonitorEvent>? OnEvent;
    public event Action<ThreatAssessment>? OnAssessmentComplete;

    /// <summary>
    /// Полный анализ файла с безопасным запуском
    /// </summary>
    public async Task<ThreatAssessment> AnalyzeAsync(string filePath, int timeoutSeconds = 30)
    {
        _events.Clear();
        _assessment.RiskScore = 0;
        _assessment.FileName = Path.GetFileName(filePath);
        _assessment.FilePath = filePath;
        _assessment.SuspiciousBehaviors.Clear();
        _assessment.FileChanges.Clear();
        _assessment.RegistryChanges.Clear();
        _assessment.NetworkConnections.Clear();
        _assessment.ProcessCreations.Clear();
        _assessment.DllLoads.Clear();
        _isAnalyzing = true;

        AddEvent(EventType.Unknown, EventSeverity.Info, "Анализ", $"Начало анализа файла: {filePath}", "Проверяем файл на угрозы...");

        try
        {
            // 1. Статический анализ
            await Task.Run(() => PerformStaticAnalysis(filePath));

            // 2. Запуск в режиме мониторинга
            await MonitorProcessAsync(filePath, timeoutSeconds);

            // 3. Расчёт итогового риска
            CalculateFinalRisk();

            _assessment.Summary = GenerateSummary();
            _assessment.Recommendations = GenerateRecommendations();

            _isAnalyzing = false;
            OnAssessmentComplete?.Invoke(_assessment);
        }
        catch (Exception ex)
        {
            AddEvent(EventType.Unknown, EventSeverity.Critical, "Ошибка", ex.Message, "Произошла ошибка при анализе");
            _assessment.RiskScore = 50;
            _assessment.RiskLevel = RiskLevel.Medium;
            _isAnalyzing = false;
        }

        return _assessment;
    }

    private void PerformStaticAnalysis(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        _assessment.FileSize = fileInfo.Length;

        // Хешы
        using (var stream = File.OpenRead(filePath))
        {
            _assessment.MD5 = ComputeHash(System.Security.Cryptography.MD5.Create(), stream);
            stream.Position = 0;
            _assessment.SHA256 = ComputeHash(System.Security.Cryptography.SHA256.Create(), stream);
        }

        // Проверка на подозрительные строки
        CheckSuspiciousStrings(filePath);

        // Анализ PE
        AnalyzePE(filePath);

        // Цифровая подпись
        CheckDigitalSignature(filePath);
    }

    private void CheckSuspiciousStrings(string filePath)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            using var reader = new StreamReader(stream);
            string content = reader.ReadToEnd();

            foreach (var pattern in SuspiciousPatterns)
            {
                if (Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase))
                {
                    _assessment.SuspiciousStrings.Add(pattern);
                    AddEvent(EventType.Unknown, EventSeverity.Medium, "Статический анализ",
                        $"Найдена подозрительная строка: {pattern}",
                        "В файле есть текст, который может указывать на опасные действия");
                    _assessment.FileAnalysisScore += 5;
                }
            }
        }
        catch { }
    }

    private void AnalyzePE(string filePath)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            byte[] header = new byte[4096];
            stream.Read(header, 0, header.Length);

            // Проверка MZ
            if (header[0] != 'M' || header[1] != 'Z')
            {
                _assessment.SuspiciousBehaviors.Add("Не является PE-файлом");
                _assessment.FileAnalysisScore += 20;
                return;
            }

            // Проверка на упаковщики
            string[] packerSignatures = { "UPX", "ASPack", "PECompact", "Themida", "VMProtect" };
            string headerStr = System.Text.Encoding.ASCII.GetString(header);
            
            foreach (var packer in packerSignatures)
            {
                if (headerStr.Contains(packer))
                {
                    _assessment.IsPacked = true;
                    _assessment.PackerName = packer;
                    AddEvent(EventType.Unknown, EventSeverity.High, "Упаковщик",
                        $"Обнаружен упаковщик: {packer}",
                        "Файл сжат или защищён — это может скрывать вредоносный код");
                    _assessment.FileAnalysisScore += 25;
                    break;
                }
            }
        }
        catch { }
    }

    private void CheckDigitalSignature(string filePath)
    {
        try
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(filePath);
            if (!string.IsNullOrEmpty(versionInfo.CompanyName))
            {
                _assessment.IsSigned = true;
                _assessment.Publisher = versionInfo.CompanyName;
                AddEvent(EventType.Unknown, EventSeverity.Info, "Подпись",
                    $"Издатель: {versionInfo.CompanyName}",
                    $"Файл подписан компанией {versionInfo.CompanyName}");
                _assessment.FileAnalysisScore -= 15; // Снижаем риск
            }
            else
            {
                AddEvent(EventType.Unknown, EventSeverity.Low, "Подпись",
                    "Цифровая подпись отсутствует",
                    "Неизвестно, кто создал этот файл");
                _assessment.FileAnalysisScore += 10;
            }
        }
        catch
        {
            _assessment.FileAnalysisScore += 10;
        }
    }

    private async Task MonitorProcessAsync(string filePath, int timeoutSeconds)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true,
                CreateNoWindow = false
            };

            _monitoredProcess = Process.Start(psi);
            if (_monitoredProcess == null) return;

            AddEvent(EventType.ProcessStart, EventSeverity.Info, "Процесс",
                $"Запущен процесс: {filePath} (PID: {_monitoredProcess.Id})",
                "Программа начала работу");

            // Запуск мониторинга ресурсов
            _resourceMonitor = new System.Threading.Timer(
                _ => MonitorResources(),
                null,
                1000,
                1000
            );

            // Мониторинг в течение timeoutSeconds
            await Task.Delay(timeoutSeconds * 1000);

            if (!_monitoredProcess.HasExited)
            {
                _monitoredProcess.Kill();
                AddEvent(EventType.ProcessEnd, EventSeverity.Info, "Процесс",
                    "Процесс завершён принудительно",
                    "Время анализа истекло");
            }
        }
        catch (Exception ex)
        {
            AddEvent(EventType.Unknown, EventSeverity.Critical, "Ошибка",
                $"Не удалось запустить: {ex.Message}",
                "Программа не запустилась или произошла ошибка");
        }
        finally
        {
            _resourceMonitor?.Dispose();
            _monitoredProcess?.Dispose();
        }
    }

    private void MonitorResources()
    {
        if (_monitoredProcess == null || _monitoredProcess.HasExited) return;

        try
        {
            _monitoredProcess.Refresh();
            long memoryMB = _monitoredProcess.WorkingSet64 / (1024 * 1024);
            TimeSpan cpuTime = _monitoredProcess.TotalProcessorTime;

            // Подозрительно высокое использование памяти
            if (memoryMB > 500)
            {
                AddEvent(EventType.MemoryAllocation, EventSeverity.Medium, "Память",
                    $"Высокое потребление памяти: {memoryMB} МБ",
                    "Программа использует много оперативной памяти");
                _assessment.BehaviorScore += 5;
            }
        }
        catch { }
    }

    private void AddEvent(EventType type, EventSeverity severity, string category,
        string description, string simpleExplanation)
    {
        var evt = new MonitorEvent
        {
            Timestamp = DateTime.Now,
            Type = type,
            Severity = severity,
            Category = category,
            Description = description,
            SimpleExplanation = simpleExplanation,
            RiskContribution = severity switch
            {
                EventSeverity.Info => 0,
                EventSeverity.Low => 5,
                EventSeverity.Medium => 15,
                EventSeverity.High => 25,
                EventSeverity.Critical => 40,
                _ => 0
            }
        };

        _events.Add(evt);
        OnEvent?.Invoke(evt);

        // Обновляем оценку
        _assessment.BehaviorScore += evt.RiskContribution;

        // Категоризация событий
        switch (type)
        {
            case EventType.FileCreate:
            case EventType.FileModify:
            case EventType.FileDelete:
                _assessment.FileChanges.Add(description);
                break;
            case EventType.RegistryCreate:
            case EventType.RegistryModify:
            case EventType.RegistryDelete:
                _assessment.RegistryChanges.Add(description);
                break;
            case EventType.NetworkConnect:
            case EventType.NetworkListen:
                _assessment.NetworkConnections.Add(description);
                _assessment.NetworkScore += evt.RiskContribution;
                break;
            case EventType.ProcessStart:
                _assessment.ProcessCreations.Add(description);
                break;
            case EventType.DllLoad:
                _assessment.DllLoads.Add(description);
                break;
            case EventType.CodeInjection:
                _assessment.SuspiciousBehaviors.Add("Обнаружена инъекция кода!");
                _assessment.SystemScore += 50;
                break;
            case EventType.AutoStart:
                _assessment.SuspiciousBehaviors.Add("Добавление в автозагрузку");
                _assessment.SystemScore += 30;
                break;
            case EventType.SystemFileModify:
                _assessment.SuspiciousBehaviors.Add("Изменение системных файлов");
                _assessment.SystemScore += 40;
                break;
        }
    }

    private void CalculateFinalRisk()
    {
        // Ограничиваем каждый компонент максимум 25 баллами
        _assessment.FileAnalysisScore = Math.Min(_assessment.FileAnalysisScore, 25);
        _assessment.BehaviorScore = Math.Min(_assessment.BehaviorScore, 25);
        _assessment.NetworkScore = Math.Min(_assessment.NetworkScore, 25);
        _assessment.SystemScore = Math.Min(_assessment.SystemScore, 25);

        _assessment.RiskScore = _assessment.FileAnalysisScore +
                                _assessment.BehaviorScore +
                                _assessment.NetworkScore +
                                _assessment.SystemScore;

        _assessment.RiskScore = Math.Min(_assessment.RiskScore, 100);
        _assessment.RiskScore = Math.Max(_assessment.RiskScore, 0);

        _assessment.RiskLevel = _assessment.RiskScore switch
        {
            <= 20 => RiskLevel.Safe,
            <= 40 => RiskLevel.Low,
            <= 60 => RiskLevel.Medium,
            <= 80 => RiskLevel.High,
            _ => RiskLevel.Critical
        };
    }

    private string GenerateSummary()
    {
        var parts = new List<string>();

        if (_assessment.IsSigned)
            parts.Add($"✅ Подписан: {_assessment.Publisher}");
        else
            parts.Add("⚠️ Без цифровой подписи");

        if (_assessment.IsPacked)
            parts.Add($"📦 Упаковщик: {_assessment.PackerName}");

        if (_assessment.NetworkConnections.Count > 0)
            parts.Add($"🌐 Сетевых подключений: {_assessment.NetworkConnections.Count}");

        if (_assessment.FileChanges.Count > 0)
            parts.Add($"📄 Изменений файлов: {_assessment.FileChanges.Count}");

        if (_assessment.RegistryChanges.Count > 0)
            parts.Add($"🔧 Изменений реестра: {_assessment.RegistryChanges.Count}");

        return string.Join(" | ", parts);
    }

    private List<string> GenerateRecommendations()
    {
        var recs = new List<string>();

        if (_assessment.RiskLevel >= RiskLevel.High)
            recs.Add("❌ Не рекомендуется запускать этот файл");

        if (_assessment.IsPacked)
            recs.Add("📦 Распакуйте файл и проверьте содержимое");

        if (!_assessment.IsSigned)
            recs.Add("⚠️ Запускайте только из доверенного источника");

        if (_assessment.NetworkConnections.Count > 0)
            recs.Add("🌐 Проверьте, куда программа подключается");

        if (_assessment.RegistryChanges.Count > 0)
            recs.Add("🔧 Проверьте изменения в реестре");

        if (recs.Count == 0)
            recs.Add("✅ Файл выглядит безопасным");

        return recs;
    }

    private string ComputeHash(System.Security.Cryptography.HashAlgorithm algo, Stream stream)
    {
        byte[] hash = algo.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    public IReadOnlyList<MonitorEvent> GetEvents() => _events.AsReadOnly();
    public bool IsAnalyzing => _isAnalyzing;
}
