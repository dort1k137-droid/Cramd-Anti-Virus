namespace ProgramLauncher.Models;

/// <summary>
/// Результаты оценки угрозы (0-100)
/// </summary>
public class ThreatAssessment
{
    public int RiskScore { get; set; } // 0-100
    public RiskLevel RiskLevel { get; set; }
    public string FileName { get; set; } = "";
    public string FilePath { get; set; } = "";
    public long FileSize { get; set; }
    public string? MD5 { get; set; }
    public string? SHA256 { get; set; }
    public string? Publisher { get; set; }
    public bool IsSigned { get; set; }
    public bool IsPacked { get; set; }
    public string? PackerName { get; set; }
    
    // Детекты
    public List<string> SuspiciousStrings { get; set; } = new();
    public List<string> SuspiciousBehaviors { get; set; } = new();
    public List<string> FileChanges { get; set; } = new();
    public List<string> RegistryChanges { get; set; } = new();
    public List<string> NetworkConnections { get; set; } = new();
    public List<string> ProcessCreations { get; set; } = new();
    public List<string> DllLoads { get; set; } = new();
    
    // Оценка по категориям
    public int FileAnalysisScore { get; set; }
    public int BehaviorScore { get; set; }
    public int NetworkScore { get; set; }
    public int SystemScore { get; set; }
    
    public string Summary { get; set; } = "";
    public List<string> Recommendations { get; set; } = new();
}

public enum RiskLevel
{
    Safe = 0,        // 0-20
    Low = 1,         // 21-40
    Medium = 2,      // 41-60
    High = 3,        // 61-80
    Critical = 4     // 81-100
}

public static class RiskLevelExtensions
{
    public static string GetEmoji(this RiskLevel level) => level switch
    {
        RiskLevel.Safe => "🟢",
        RiskLevel.Low => "🟡",
        RiskLevel.Medium => "🟠",
        RiskLevel.High => "🔴",
        RiskLevel.Critical => "☢️",
        _ => "⚪"
    };
    
    public static string GetLabel(this RiskLevel level) => level switch
    {
        RiskLevel.Safe => "Безопасно",
        RiskLevel.Low => "Подозрительно",
        RiskLevel.Medium => "Высокий риск",
        RiskLevel.High => "Опасно",
        RiskLevel.Critical => "Критически опасно",
        _ => "Неизвестно"
    };
    
    public static Color GetColor(this RiskLevel level) => level switch
    {
        RiskLevel.Safe => Color.FromArgb(76, 175, 80),
        RiskLevel.Low => Color.FromArgb(255, 193, 7),
        RiskLevel.Medium => Color.FromArgb(255, 152, 0),
        RiskLevel.High => Color.FromArgb(244, 67, 54),
        RiskLevel.Critical => Color.FromArgb(183, 28, 28),
        _ => Color.Gray
    };
}
