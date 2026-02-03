using StarTrekGame.Domain.Identity;
using StarTrekGame.Domain.SharedKernel;
using System.Security.Claims;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace StarTrekGame.Infrastructure.Auth;

#region Auth Service Implementation

/// <summary>
/// Handles authentication via multiple OAuth providers.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IOAuthProviderFactory _providerFactory;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IOAuthProviderFactory providerFactory,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _providerFactory = providerFactory;
        _logger = logger;
    }

    public async Task<AuthResult> AuthenticateAsync(
        AuthProvider provider, 
        string code, 
        string? redirectUri = null)
    {
        try
        {
            // Get the appropriate OAuth provider
            var oauthProvider = _providerFactory.GetProvider(provider);
            
            // Exchange code for tokens
            var oauthResult = await oauthProvider.ExchangeCodeAsync(code, redirectUri);
            if (!oauthResult.Success)
            {
                return AuthResult.Failed(oauthResult.Error ?? "OAuth exchange failed");
            }
            
            // Get user info from provider
            var userInfo = await oauthProvider.GetUserInfoAsync(oauthResult.AccessToken!);
            if (userInfo == null)
            {
                return AuthResult.Failed("Could not retrieve user information");
            }
            
            // Find or create user
            var user = await _userRepository.GetByExternalIdAsync(provider, userInfo.ExternalId);
            
            if (user == null)
            {
                // Create new user
                user = User.Create(
                    userInfo.DisplayName,
                    userInfo.Email,
                    provider,
                    userInfo.ExternalId);
                    
                if (!string.IsNullOrEmpty(userInfo.AvatarUrl))
                {
                    user.UpdateProfile(userInfo.DisplayName, userInfo.AvatarUrl);
                }
                
                await _userRepository.AddAsync(user);
                _logger.LogInformation("New user created: {UserId} via {Provider}", user.Id, provider);
            }
            else
            {
                // Update last login
                user.RecordLogin();
                await _userRepository.UpdateAsync(user);
            }
            
            // Check if user can access (not banned, etc.)
            if (!user.CanAccess())
            {
                return AuthResult.Failed($"Account is banned: {user.BanReason}");
            }
            
            // Generate our own tokens
            var tokens = _tokenService.GenerateTokens(user);
            
            return AuthResult.Succeeded(user, tokens);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication failed for provider {Provider}", provider);
            return AuthResult.Failed("Authentication failed: " + ex.Message);
        }
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var principal = _tokenService.ValidateRefreshToken(refreshToken);
            if (principal == null)
            {
                return AuthResult.Failed("Invalid refresh token");
            }
            
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return AuthResult.Failed("Invalid token claims");
            }
            
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return AuthResult.Failed("User not found");
            }
            
            if (!user.CanAccess())
            {
                return AuthResult.Failed($"Account is banned: {user.BanReason}");
            }
            
            var tokens = _tokenService.GenerateTokens(user);
            
            return AuthResult.Succeeded(user, tokens);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return AuthResult.Failed("Token refresh failed");
        }
    }

    public async Task<bool> ValidateTokenAsync(string accessToken)
    {
        return await Task.FromResult(_tokenService.ValidateAccessToken(accessToken) != null);
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
        await _tokenService.RevokeRefreshTokenAsync(refreshToken);
    }

    public async Task<Result> LinkAccountAsync(
        Guid userId, 
        AuthProvider provider, 
        string code, 
        string? redirectUri = null)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return Result.Failure("User not found");
        }
        
        var oauthProvider = _providerFactory.GetProvider(provider);
        var oauthResult = await oauthProvider.ExchangeCodeAsync(code, redirectUri);
        
        if (!oauthResult.Success)
        {
            return Result.Failure(oauthResult.Error ?? "OAuth exchange failed");
        }
        
        var userInfo = await oauthProvider.GetUserInfoAsync(oauthResult.AccessToken!);
        if (userInfo == null)
        {
            return Result.Failure("Could not retrieve account information");
        }
        
        // Check if this external account is already linked to another user
        var existingUser = await _userRepository.GetByExternalIdAsync(provider, userInfo.ExternalId);
        if (existingUser != null && existingUser.Id != userId)
        {
            return Result.Failure("This account is already linked to another user");
        }
        
        try
        {
            user.LinkAccount(provider, userInfo.ExternalId);
            await _userRepository.UpdateAsync(user);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}

#endregion

#region Auth Results

public class AuthResult
{
    public bool Success { get; private set; }
    public string? Error { get; private set; }
    public User? User { get; private set; }
    public TokenPair? Tokens { get; private set; }

    public static AuthResult Succeeded(User user, TokenPair tokens) => new()
    {
        Success = true,
        User = user,
        Tokens = tokens
    };

    public static AuthResult Failed(string error) => new()
    {
        Success = false,
        Error = error
    };
}

public class TokenPair
{
    public string AccessToken { get; init; } = "";
    public string RefreshToken { get; init; } = "";
    public DateTime AccessTokenExpiry { get; init; }
    public DateTime RefreshTokenExpiry { get; init; }
}

#endregion

#region OAuth Provider Interface

public interface IOAuthProvider
{
    AuthProvider ProviderType { get; }
    string GetAuthorizationUrl(string state, string? redirectUri = null);
    Task<OAuthTokenResult> ExchangeCodeAsync(string code, string? redirectUri = null);
    Task<OAuthUserInfo?> GetUserInfoAsync(string accessToken);
}

public class OAuthTokenResult
{
    public bool Success { get; init; }
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public string? Error { get; init; }
}

// OAuthUserInfo is defined in StarTrekGame.Domain.Identity

public interface IOAuthProviderFactory
{
    IOAuthProvider GetProvider(AuthProvider provider);
}

#endregion

#region OAuth Provider Implementations

public class GoogleOAuthProvider : IOAuthProvider
{
    private readonly GoogleOAuthConfig _config;
    private readonly HttpClient _httpClient;

    public GoogleOAuthProvider(GoogleOAuthConfig config, HttpClient httpClient)
    {
        _config = config;
        _httpClient = httpClient;
    }

    public AuthProvider ProviderType => AuthProvider.Google;

    public string GetAuthorizationUrl(string state, string? redirectUri = null)
    {
        var redirect = redirectUri ?? _config.DefaultRedirectUri;
        return $"https://accounts.google.com/o/oauth2/v2/auth" +
               $"?client_id={_config.ClientId}" +
               $"&redirect_uri={Uri.EscapeDataString(redirect)}" +
               $"&response_type=code" +
               $"&scope={Uri.EscapeDataString("openid email profile")}" +
               $"&state={state}";
    }

    public async Task<OAuthTokenResult> ExchangeCodeAsync(string code, string? redirectUri = null)
    {
        var redirect = redirectUri ?? _config.DefaultRedirectUri;
        
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _config.ClientId,
            ["client_secret"] = _config.ClientSecret,
            ["code"] = code,
            ["grant_type"] = "authorization_code",
            ["redirect_uri"] = redirect
        });

        var response = await _httpClient.PostAsync(
            "https://oauth2.googleapis.com/token", content);
            
        if (!response.IsSuccessStatusCode)
        {
            return new OAuthTokenResult { Success = false, Error = "Token exchange failed" };
        }

        var json = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>();
        
        return new OAuthTokenResult
        {
            Success = true,
            AccessToken = json?.access_token,
            RefreshToken = json?.refresh_token
        };
    }

    public async Task<OAuthUserInfo?> GetUserInfoAsync(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            
        var response = await _httpClient.GetAsync(
            "https://www.googleapis.com/oauth2/v2/userinfo");
            
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadFromJsonAsync<GoogleUserInfo>();
        
        return json == null ? null : new OAuthUserInfo
        {
            ExternalId = json.id,
            Email = json.email,
            DisplayName = json.name,
            AvatarUrl = json.picture
        };
    }
    
    private class GoogleTokenResponse
    {
        public string? access_token { get; set; }
        public string? refresh_token { get; set; }
    }
    
    private class GoogleUserInfo
    {
        public string id { get; set; } = "";
        public string email { get; set; } = "";
        public string name { get; set; } = "";
        public string? picture { get; set; }
    }
}

public class MicrosoftOAuthProvider : IOAuthProvider
{
    private readonly MicrosoftOAuthConfig _config;
    private readonly HttpClient _httpClient;

    public MicrosoftOAuthProvider(MicrosoftOAuthConfig config, HttpClient httpClient)
    {
        _config = config;
        _httpClient = httpClient;
    }

    public AuthProvider ProviderType => AuthProvider.Microsoft;

    public string GetAuthorizationUrl(string state, string? redirectUri = null)
    {
        var redirect = redirectUri ?? _config.DefaultRedirectUri;
        return $"https://login.microsoftonline.com/common/oauth2/v2.0/authorize" +
               $"?client_id={_config.ClientId}" +
               $"&redirect_uri={Uri.EscapeDataString(redirect)}" +
               $"&response_type=code" +
               $"&scope={Uri.EscapeDataString("openid email profile")}" +
               $"&state={state}";
    }

    public async Task<OAuthTokenResult> ExchangeCodeAsync(string code, string? redirectUri = null)
    {
        var redirect = redirectUri ?? _config.DefaultRedirectUri;
        
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _config.ClientId,
            ["client_secret"] = _config.ClientSecret,
            ["code"] = code,
            ["grant_type"] = "authorization_code",
            ["redirect_uri"] = redirect,
            ["scope"] = "openid email profile"
        });

        var response = await _httpClient.PostAsync(
            "https://login.microsoftonline.com/common/oauth2/v2.0/token", content);
            
        if (!response.IsSuccessStatusCode)
        {
            return new OAuthTokenResult { Success = false, Error = "Token exchange failed" };
        }

        var json = await response.Content.ReadFromJsonAsync<MicrosoftTokenResponse>();
        
        return new OAuthTokenResult
        {
            Success = true,
            AccessToken = json?.access_token,
            RefreshToken = json?.refresh_token
        };
    }

    public async Task<OAuthUserInfo?> GetUserInfoAsync(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            
        var response = await _httpClient.GetAsync("https://graph.microsoft.com/v1.0/me");
            
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadFromJsonAsync<MicrosoftUserInfo>();
        
        return json == null ? null : new OAuthUserInfo
        {
            ExternalId = json.id,
            Email = json.mail ?? json.userPrincipalName,
            DisplayName = json.displayName
        };
    }
    
    private class MicrosoftTokenResponse
    {
        public string? access_token { get; set; }
        public string? refresh_token { get; set; }
    }
    
    private class MicrosoftUserInfo
    {
        public string id { get; set; } = "";
        public string? mail { get; set; }
        public string userPrincipalName { get; set; } = "";
        public string displayName { get; set; } = "";
    }
}

public class DiscordOAuthProvider : IOAuthProvider
{
    private readonly DiscordOAuthConfig _config;
    private readonly HttpClient _httpClient;

    public DiscordOAuthProvider(DiscordOAuthConfig config, HttpClient httpClient)
    {
        _config = config;
        _httpClient = httpClient;
    }

    public AuthProvider ProviderType => AuthProvider.Discord;

    public string GetAuthorizationUrl(string state, string? redirectUri = null)
    {
        var redirect = redirectUri ?? _config.DefaultRedirectUri;
        return $"https://discord.com/api/oauth2/authorize" +
               $"?client_id={_config.ClientId}" +
               $"&redirect_uri={Uri.EscapeDataString(redirect)}" +
               $"&response_type=code" +
               $"&scope={Uri.EscapeDataString("identify email")}" +
               $"&state={state}";
    }

    public async Task<OAuthTokenResult> ExchangeCodeAsync(string code, string? redirectUri = null)
    {
        var redirect = redirectUri ?? _config.DefaultRedirectUri;
        
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _config.ClientId,
            ["client_secret"] = _config.ClientSecret,
            ["code"] = code,
            ["grant_type"] = "authorization_code",
            ["redirect_uri"] = redirect
        });

        var response = await _httpClient.PostAsync(
            "https://discord.com/api/oauth2/token", content);
            
        if (!response.IsSuccessStatusCode)
        {
            return new OAuthTokenResult { Success = false, Error = "Token exchange failed" };
        }

        var json = await response.Content.ReadFromJsonAsync<DiscordTokenResponse>();
        
        return new OAuthTokenResult
        {
            Success = true,
            AccessToken = json?.access_token,
            RefreshToken = json?.refresh_token
        };
    }

    public async Task<OAuthUserInfo?> GetUserInfoAsync(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            
        var response = await _httpClient.GetAsync("https://discord.com/api/users/@me");
            
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadFromJsonAsync<DiscordUserInfo>();
        if (json == null) return null;
        
        var avatarUrl = !string.IsNullOrEmpty(json.avatar) 
            ? $"https://cdn.discordapp.com/avatars/{json.id}/{json.avatar}.png"
            : null;
        
        return new OAuthUserInfo
        {
            ExternalId = json.id,
            Email = json.email ?? "",
            DisplayName = json.global_name ?? json.username,
            AvatarUrl = avatarUrl
        };
    }
    
    private class DiscordTokenResponse
    {
        public string? access_token { get; set; }
        public string? refresh_token { get; set; }
    }
    
    private class DiscordUserInfo
    {
        public string id { get; set; } = "";
        public string username { get; set; } = "";
        public string? global_name { get; set; }
        public string? email { get; set; }
        public string? avatar { get; set; }
    }
}

#endregion

#region OAuth Configuration

public class GoogleOAuthConfig
{
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string DefaultRedirectUri { get; set; } = "";
}

public class MicrosoftOAuthConfig
{
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string DefaultRedirectUri { get; set; } = "";
}

public class DiscordOAuthConfig
{
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string DefaultRedirectUri { get; set; } = "";
}

public class OAuthProviderFactory : IOAuthProviderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public OAuthProviderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IOAuthProvider GetProvider(AuthProvider provider) => provider switch
    {
        AuthProvider.Google => _serviceProvider.GetRequiredService<GoogleOAuthProvider>(),
        AuthProvider.Microsoft => _serviceProvider.GetRequiredService<MicrosoftOAuthProvider>(),
        AuthProvider.Discord => _serviceProvider.GetRequiredService<DiscordOAuthProvider>(),
        _ => throw new NotSupportedException($"Provider {provider} is not supported")
    };
}

#endregion

#region Token Service

public interface ITokenService
{
    TokenPair GenerateTokens(User user);
    ClaimsPrincipal? ValidateAccessToken(string token);
    ClaimsPrincipal? ValidateRefreshToken(string token);
    Task RevokeRefreshTokenAsync(string token);
}

public class JwtTokenService : ITokenService
{
    private readonly JwtConfig _config;
    private readonly IRefreshTokenStore _refreshTokenStore;

    public JwtTokenService(JwtConfig config, IRefreshTokenStore refreshTokenStore)
    {
        _config = config;
        _refreshTokenStore = refreshTokenStore;
    }

    public TokenPair GenerateTokens(User user)
    {
        var accessTokenExpiry = DateTime.UtcNow.AddMinutes(_config.AccessTokenExpiryMinutes);
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(_config.RefreshTokenExpiryDays);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.DisplayName),
            new("role", user.GlobalRole.ToString())
        };

        var accessToken = GenerateJwtToken(claims, accessTokenExpiry);
        var refreshToken = GenerateJwtToken(claims, refreshTokenExpiry, isRefreshToken: true);
        
        // Store refresh token
        _refreshTokenStore.Store(refreshToken, user.Id, refreshTokenExpiry);

        return new TokenPair
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiry = accessTokenExpiry,
            RefreshTokenExpiry = refreshTokenExpiry
        };
    }

    private string GenerateJwtToken(List<Claim> claims, DateTime expiry, bool isRefreshToken = false)
    {
        var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(_config.SecretKey));
        var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
            key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

        if (isRefreshToken)
        {
            claims.Add(new Claim("token_type", "refresh"));
        }

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: _config.Issuer,
            audience: _config.Audience,
            claims: claims,
            expires: expiry,
            signingCredentials: credentials);

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        return ValidateToken(token, validateRefresh: false);
    }

    public ClaimsPrincipal? ValidateRefreshToken(string token)
    {
        if (!_refreshTokenStore.IsValid(token))
            return null;
            
        return ValidateToken(token, validateRefresh: true);
    }

    private ClaimsPrincipal? ValidateToken(string token, bool validateRefresh)
    {
        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var key = System.Text.Encoding.UTF8.GetBytes(_config.SecretKey);

        try
        {
            var principal = tokenHandler.ValidateToken(token, new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _config.Issuer,
                ValidateAudience = true,
                ValidAudience = _config.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            if (validateRefresh)
            {
                var tokenTypeClaim = principal.FindFirst("token_type");
                if (tokenTypeClaim?.Value != "refresh")
                    return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public async Task RevokeRefreshTokenAsync(string token)
    {
        await _refreshTokenStore.RevokeAsync(token);
    }
}

public class JwtConfig
{
    public string SecretKey { get; set; } = "";
    public string Issuer { get; set; } = "GalacticStrategy";
    public string Audience { get; set; } = "GalacticStrategyClient";
    public int AccessTokenExpiryMinutes { get; set; } = 15;
    public int RefreshTokenExpiryDays { get; set; } = 7;
}

public interface IRefreshTokenStore
{
    void Store(string token, Guid userId, DateTime expiry);
    bool IsValid(string token);
    Task RevokeAsync(string token);
}

#endregion

#region Interfaces for DI

public interface IAuthService
{
    Task<AuthResult> AuthenticateAsync(AuthProvider provider, string code, string? redirectUri = null);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
    Task<bool> ValidateTokenAsync(string accessToken);
    Task RevokeTokenAsync(string refreshToken);
    Task<Result> LinkAccountAsync(Guid userId, AuthProvider provider, string code, string? redirectUri = null);
}

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByExternalIdAsync(AuthProvider provider, string externalId);
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
}

// ILogger<T> is defined in StarTrekGame.Domain.Identity.User

#endregion
