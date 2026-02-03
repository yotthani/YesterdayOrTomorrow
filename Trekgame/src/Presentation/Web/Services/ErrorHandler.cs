using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace StarTrekGame.Web.Services;

/// <summary>
/// Centralized error handling service for UI operations
/// </summary>
public interface IErrorHandler
{
    Task<T?> ExecuteAsync<T>(Func<Task<T>> operation, string operationName, T? fallback = default);
    Task<bool> ExecuteAsync(Func<Task> operation, string operationName);
    void HandleError(Exception ex, string context);
    void ShowError(string message);
    void ShowWarning(string message);
    void ShowSuccess(string message);
    void ShowInfo(string message);
}

public class ErrorHandler : IErrorHandler
{
    private readonly ISnackbar _snackbar;
    private readonly ILogger<ErrorHandler> _logger;

    public ErrorHandler(ISnackbar snackbar, ILogger<ErrorHandler> logger)
    {
        _snackbar = snackbar;
        _logger = logger;
    }

    /// <summary>
    /// Execute an async operation with automatic error handling and optional fallback
    /// </summary>
    public async Task<T?> ExecuteAsync<T>(Func<Task<T>> operation, string operationName, T? fallback = default)
    {
        try
        {
            return await operation();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Network error during {Operation}", operationName);
            ShowError($"Network error: Unable to {operationName.ToLower()}. Please check your connection.");
            return fallback;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Timeout during {Operation}", operationName);
            ShowWarning($"Request timed out while trying to {operationName.ToLower()}. Please try again.");
            return fallback;
        }
        catch (Exception ex)
        {
            HandleError(ex, operationName);
            return fallback;
        }
    }

    /// <summary>
    /// Execute an async operation with automatic error handling
    /// </summary>
    public async Task<bool> ExecuteAsync(Func<Task> operation, string operationName)
    {
        try
        {
            await operation();
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Network error during {Operation}", operationName);
            ShowError($"Network error: Unable to {operationName.ToLower()}. Please check your connection.");
            return false;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Timeout during {Operation}", operationName);
            ShowWarning($"Request timed out while trying to {operationName.ToLower()}. Please try again.");
            return false;
        }
        catch (Exception ex)
        {
            HandleError(ex, operationName);
            return false;
        }
    }

    /// <summary>
    /// Handle and log an exception
    /// </summary>
    public void HandleError(Exception ex, string context)
    {
        _logger.LogError(ex, "Error in {Context}: {Message}", context, ex.Message);
        
        var message = ex switch
        {
            UnauthorizedAccessException => "You don't have permission to perform this action.",
            InvalidOperationException => $"Invalid operation: {ex.Message}",
            ArgumentException => $"Invalid input: {ex.Message}",
            _ => $"An error occurred: {ex.Message}"
        };
        
        ShowError(message);
    }

    public void ShowError(string message)
    {
        _snackbar.Add(message, Severity.Error, config =>
        {
            config.Icon = Icons.Material.Filled.Error;
            config.VisibleStateDuration = 7000;
        });
    }

    public void ShowWarning(string message)
    {
        _snackbar.Add(message, Severity.Warning, config =>
        {
            config.Icon = Icons.Material.Filled.Warning;
            config.VisibleStateDuration = 5000;
        });
    }

    public void ShowSuccess(string message)
    {
        _snackbar.Add(message, Severity.Success, config =>
        {
            config.Icon = Icons.Material.Filled.CheckCircle;
            config.VisibleStateDuration = 3000;
        });
    }

    public void ShowInfo(string message)
    {
        _snackbar.Add(message, Severity.Info, config =>
        {
            config.Icon = Icons.Material.Filled.Info;
            config.VisibleStateDuration = 4000;
        });
    }
}

/// <summary>
/// Extension methods for cleaner error handling in components
/// </summary>
public static class ErrorHandlerExtensions
{
    /// <summary>
    /// Wrap a component's initialization with error handling
    /// </summary>
    public static async Task SafeInitializeAsync(
        this ComponentBase component, 
        IErrorHandler errorHandler,
        Func<Task> initialization,
        string componentName)
    {
        await errorHandler.ExecuteAsync(initialization, $"initialize {componentName}");
    }
}
