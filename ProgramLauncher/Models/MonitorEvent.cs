namespace ProgramLauncher.Models;

/// <summary>
/// Событие мониторинга поведения программы
/// </summary>
public class MonitorEvent
{
    public DateTime Timestamp { get; set; }
    public EventType Type { get; set; }
    public EventSeverity Severity { get; set; }
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public string Details { get; set; } = "";
    public int RiskContribution { get; set; } // Сколько добавило к общему риску
    public string? ProcessName { get; set; }
    public int? PID { get; set; }
    public string? Path { get; set; }
    public string? SimpleExplanation { get; set; } // Объяснение простым языком
}

public enum EventType
{
    ProcessStart,
    ProcessEnd,
    FileCreate,
    FileModify,
    FileDelete,
    RegistryCreate,
    RegistryModify,
    RegistryDelete,
    NetworkConnect,
    NetworkListen,
    DllLoad,
    MemoryAllocation,
    CodeInjection,
    AutoStart,
    SystemFileModify,
    ClipboardAccess,
    Screenshot,
    Keylog,
    Unknown
}

public enum EventSeverity
{
    Info = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public static class EventTypeExtensions
{
    public static string GetEmoji(this EventType type) => type switch
    {
        EventType.ProcessStart => "🚀",
        EventType.ProcessEnd => "🛑",
        EventType.FileCreate => "📄",
        EventType.FileModify => "✏️",
        EventType.FileDelete => "🗑️",
        EventType.RegistryCreate => "📝",
        EventType.RegistryModify => "🔧",
        EventType.RegistryDelete => "❌",
        EventType.NetworkConnect => "🌐",
        EventType.NetworkListen => "📡",
        EventType.DllLoad => "📦",
        EventType.MemoryAllocation => "💾",
        EventType.CodeInjection => "💉",
        EventType.AutoStart => "⚡",
        EventType.SystemFileModify => "⚠️",
        EventType.ClipboardAccess => "📋",
        EventType.Screenshot => "📸",
        EventType.Keylog => "⌨️",
        _ => "❓"
    };
    
    public static string GetSimpleExplanation(this EventType type) => type switch
    {
        EventType.ProcessStart => "Запустила другую программу",
        EventType.ProcessEnd => "Завершила программу",
        EventType.FileCreate => "Создала новый файл",
        EventType.FileModify => "Изменила существующий файл",
        EventType.FileDelete => "Удалила файл",
        EventType.RegistryCreate => "Добавила запись в реестр",
        EventType.RegistryModify => "Изменила настройку в реестре",
        EventType.RegistryDelete => "Удалила запись из реестра",
        EventType.NetworkConnect => "Подключилась к интернету",
        EventType.NetworkListen => "Открыла сетевой порт для приёма данных",
        EventType.DllLoad => "Загрузила библиотеку (DLL)",
        EventType.MemoryAllocation => "Выделила память",
        EventType.CodeInjection => "Внедрила код в другой процесс",
        EventType.AutoStart => "Добавила себя в автозагрузку",
        EventType.SystemFileModify => "Изменила системный файл Windows",
        EventType.ClipboardAccess => "Получила доступ к буферу обмена",
        EventType.Screenshot => "Сделала снимок экрана",
        EventType.Keylog => "Перехватывает нажатия клавиш",
        _ => "Выполнила действие"
    };
}
