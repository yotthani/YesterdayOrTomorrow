using StarTrekGame.Domain.SharedKernel;

namespace StarTrekGame.Domain.Identity;

/// <summary>
/// OAuth authentication providers.
/// </summary>
public enum AuthProvider
{
    Local,
    Google,
    Discord,
    Steam,
    GitHub,
    Microsoft
}

/// <summary>
/// Represents a user in the system.
/// </summary>
public class User : Entity
{
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? DisplayName { get; private set; }
    public string? AvatarUrl { get; private set; }
    public GlobalRole GlobalRole { get; private set; } = GlobalRole.Player;
    public UserStatus Status { get; private set; } = UserStatus.Active;
    public string? BanReason { get; private set; }
    public DateTime? BannedUntil { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastLoginAt { get; private set; }
    public int LoginCount { get; private set; }
    
    private readonly List<LinkedAccount> _linkedAccounts = new();
    public IReadOnlyList<LinkedAccount> LinkedAccounts => _linkedAccounts.AsReadOnly();

    private User() { } // EF Core

    public User(string username, string email, AuthProvider provider, string providerId)
    {
        Username = username;
        Email = email;
        CreatedAt = DateTime.UtcNow;
        LastLoginAt = DateTime.UtcNow;
        LoginCount = 1;
        
        _linkedAccounts.Add(new LinkedAccount(Id, provider, providerId, true));
    }

    public static User Create(string username, string email, AuthProvider provider, string providerId)
    {
        return new User(username, email, provider, providerId);
    }

    public void UpdateProfile(string? displayName, string? avatarUrl)
    {
        DisplayName = displayName;
        AvatarUrl = avatarUrl;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        LoginCount++;
    }

    public void LinkAccount(AuthProvider provider, string providerId)
    {
        if (_linkedAccounts.Any(a => a.Provider == provider))
            throw new InvalidOperationException($"Account already linked to {provider}");
            
        _linkedAccounts.Add(new LinkedAccount(Id, provider, providerId, false));
    }

    public void UnlinkAccount(AuthProvider provider)
    {
        var account = _linkedAccounts.FirstOrDefault(a => a.Provider == provider);
        if (account == null)
            throw new InvalidOperationException($"No linked account for {provider}");
        if (account.IsPrimary)
            throw new InvalidOperationException("Cannot unlink primary account");
            
        _linkedAccounts.Remove(account);
    }

    public bool CanAccess()
    {
        if (Status == UserStatus.Banned)
        {
            if (BannedUntil.HasValue && BannedUntil.Value < DateTime.UtcNow)
            {
                Status = UserStatus.Active;
                BanReason = null;
                BannedUntil = null;
                return true;
            }
            return false;
        }
        return Status == UserStatus.Active;
    }

    public void Ban(string reason, DateTime? until = null)
    {
        Status = UserStatus.Banned;
        BanReason = reason;
        BannedUntil = until;
    }

    public void Unban()
    {
        Status = UserStatus.Active;
        BanReason = null;
        BannedUntil = null;
    }

    public void SetGlobalRole(GlobalRole role)
    {
        GlobalRole = role;
    }
}

public enum UserStatus
{
    Active,
    Inactive,
    Banned,
    Deleted
}

public class LinkedAccount
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public AuthProvider Provider { get; private set; }
    public string ProviderId { get; private set; } = string.Empty;
    public bool IsPrimary { get; private set; }
    public DateTime LinkedAt { get; private set; }

    private LinkedAccount() { } // EF Core

    public LinkedAccount(Guid userId, AuthProvider provider, string providerId, bool isPrimary)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Provider = provider;
        ProviderId = providerId;
        IsPrimary = isPrimary;
        LinkedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Repository interface for User entities.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByProviderIdAsync(AuthProvider provider, string providerId, CancellationToken ct = default);
    Task<User?> GetByExternalIdAsync(AuthProvider provider, string externalId, CancellationToken ct = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
    Task<bool> ExistsAsync(string email, CancellationToken ct = default);
}

/// <summary>
/// IAuthService is defined in StarTrekGame.Infrastructure.Auth
/// </summary>

/// <summary>
/// AuthResult is defined in StarTrekGame.Infrastructure.Auth
/// </summary>

/// <summary>
/// Token service interface is defined in StarTrekGame.Infrastructure.Auth
/// </summary>

/// <summary>
/// OAuth provider interfaces are defined in StarTrekGame.Infrastructure.Auth
/// </summary>

/// <summary>
/// Result of OAuth token exchange.
/// </summary>
public class OAuthResult
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public string? Error { get; set; }
    public int ExpiresIn { get; set; }
}

/// <summary>
/// User info from OAuth provider.
/// </summary>
public class OAuthUserInfo
{
    public string ExternalId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    
    // Alias for compatibility
    public string Id { get => ExternalId; set => ExternalId = value; }
    public string? Username { get => DisplayName; set => DisplayName = value; }
}

/// <summary>
/// Logger interface for infrastructure.
/// </summary>
public interface ILogger<T>
{
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(Exception ex, string message, params object[] args);
    void LogError(string message, params object[] args);
}
