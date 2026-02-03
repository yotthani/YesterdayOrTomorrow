using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StarTrekGame.Web.Services;

public interface INotificationService
{
    event Action<GameNotification>? OnNotification;
    event Action? OnNotificationsChanged;
    
    IReadOnlyList<GameNotification> Notifications { get; }
    int UnreadCount { get; }
    
    void AddNotification(string title, string message, NotificationType type = NotificationType.Info, string? actionUrl = null);
    void MarkAsRead(Guid id);
    void MarkAllAsRead();
    void Dismiss(Guid id);
    void Clear();
}

public class NotificationService : INotificationService
{
    private readonly List<GameNotification> _notifications = new();
    
    public event Action<GameNotification>? OnNotification;
    public event Action? OnNotificationsChanged;
    
    public IReadOnlyList<GameNotification> Notifications => _notifications.AsReadOnly();
    public int UnreadCount => _notifications.Count(n => !n.IsRead);

    public void AddNotification(string title, string message, NotificationType type = NotificationType.Info, string? actionUrl = null)
    {
        var notification = new GameNotification
        {
            Id = Guid.NewGuid(),
            Title = title,
            Message = message,
            Type = type,
            ActionUrl = actionUrl,
            Timestamp = DateTime.UtcNow,
            IsRead = false
        };
        
        _notifications.Insert(0, notification);
        
        // Keep only last 50 notifications
        if (_notifications.Count > 50)
        {
            _notifications.RemoveRange(50, _notifications.Count - 50);
        }
        
        OnNotification?.Invoke(notification);
        OnNotificationsChanged?.Invoke();
    }

    public void MarkAsRead(Guid id)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id == id);
        if (notification != null)
        {
            notification.IsRead = true;
            OnNotificationsChanged?.Invoke();
        }
    }

    public void MarkAllAsRead()
    {
        foreach (var n in _notifications)
        {
            n.IsRead = true;
        }
        OnNotificationsChanged?.Invoke();
    }

    public void Dismiss(Guid id)
    {
        _notifications.RemoveAll(n => n.Id == id);
        OnNotificationsChanged?.Invoke();
    }

    public void Clear()
    {
        _notifications.Clear();
        OnNotificationsChanged?.Invoke();
    }
}

public class GameNotification
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public NotificationType Type { get; set; }
    public string? ActionUrl { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsRead { get; set; }
    
    public string TypeIcon => Type switch
    {
        NotificationType.Success => "✅",
        NotificationType.Warning => "⚠️",
        NotificationType.Error => "❌",
        NotificationType.Combat => "⚔️",
        NotificationType.Diplomacy => "🤝",
        NotificationType.Research => "🔬",
        NotificationType.Colony => "🏠",
        NotificationType.Fleet => "🚀",
        _ => "ℹ️"
    };
    
    public string TypeColor => Type switch
    {
        NotificationType.Success => "#44dd66",
        NotificationType.Warning => "#ffaa00",
        NotificationType.Error => "#ff4455",
        NotificationType.Combat => "#ff6666",
        NotificationType.Diplomacy => "#66aaff",
        NotificationType.Research => "#aa66ff",
        NotificationType.Colony => "#66dd88",
        NotificationType.Fleet => "#4a9eff",
        _ => "#aabbcc"
    };
}

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error,
    Combat,
    Diplomacy,
    Research,
    Colony,
    Fleet
}
