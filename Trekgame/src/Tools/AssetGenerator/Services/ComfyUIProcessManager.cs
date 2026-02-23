using System.Diagnostics;
using Microsoft.Extensions.Hosting;

namespace StarTrekGame.AssetGenerator.Services;

/// <summary>
/// Manages the ComfyUI process lifecycle - starts and stops ComfyUI automatically
/// Implements IHostedService to ensure proper cleanup when the application stops
/// </summary>
public class ComfyUIProcessManager : IHostedService, IDisposable
{
    private Process? _comfyUIProcess;
    private bool _isStarting;
    private bool _disposed;

    // Default ComfyUI installation path
    public string ComfyUIPath { get; set; } = @"D:\AI\ComfyUI";
    public string PythonExecutable { get; set; } = "python";
    public int Port { get; set; } = 8188;
    // GPU mode enabled - Python 3.12 with CUDA PyTorch installed
    public bool UseGPU { get; set; } = true;

    public event Action<string>? OnLogMessage;
    public event Action<bool>? OnStatusChanged;

    public bool IsRunning => _comfyUIProcess != null && !_comfyUIProcess.HasExited;

    /// <summary>
    /// Start ComfyUI if not already running
    /// </summary>
    public async Task<(bool success, string message)> StartAsync()
    {
        if (IsRunning)
        {
            return (true, "ComfyUI is already running");
        }

        if (_isStarting)
        {
            return (false, "ComfyUI is already starting...");
        }

        _isStarting = true;

        try
        {
            // Check if ComfyUI path exists
            var mainPy = Path.Combine(ComfyUIPath, "main.py");
            if (!File.Exists(mainPy))
            {
                return (false, $"ComfyUI not found at {ComfyUIPath}");
            }

            // Check if something is already running on the port
            if (await IsPortInUseAsync(Port))
            {
                // Try to connect - maybe ComfyUI is already running externally
                OnLogMessage?.Invoke($"Port {Port} is in use - checking if ComfyUI is responding...");
                return (true, "ComfyUI appears to be running externally");
            }

            // Find Python in the ComfyUI environment
            var pythonPath = FindPythonExecutable();
            if (string.IsNullOrEmpty(pythonPath))
            {
                return (false, "Could not find Python executable for ComfyUI");
            }

            OnLogMessage?.Invoke($"Starting ComfyUI from {ComfyUIPath}...");
            OnLogMessage?.Invoke($"Using Python: {pythonPath}");

            // Build command line arguments
            // --disable-auto-launch: don't open browser
            // --disable-metadata: faster startup
            var args = $"\"{mainPy}\" --listen 127.0.0.1 --port {Port} --disable-auto-launch";
            if (UseGPU)
            {
                // Try GPU mode - will fall back to CPU if CUDA not available
                args += " --cuda-device 0";
            }
            else
            {
                // Force CPU mode
                args += " --cpu";
            }

            // Start ComfyUI using cmd.exe to get a proper console with working stdout/stderr
            // This avoids tqdm crash issues when streams are redirected
            var cmdArgs = $"/c \"\"{pythonPath}\" {args}\"";

            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = cmdArgs,
                WorkingDirectory = ComfyUIPath,
                UseShellExecute = true,  // Use shell to get proper console handles
                CreateNoWindow = false   // Show console window for ComfyUI output
            };

            // Set environment variables via cmd /c with set commands
            // For GPU mode, we'll pass env vars through the command
            if (UseGPU)
            {
                cmdArgs = $"/c \"set CUDA_VISIBLE_DEVICES=0 && \"{pythonPath}\" {args}\"";
                startInfo.Arguments = cmdArgs;
            }

            _comfyUIProcess = new Process { StartInfo = startInfo };

            _comfyUIProcess.EnableRaisingEvents = true;
            _comfyUIProcess.Exited += (s, e) =>
            {
                OnLogMessage?.Invoke("ComfyUI process exited");
                OnStatusChanged?.Invoke(false);
            };

            if (!_comfyUIProcess.Start())
            {
                return (false, "Failed to start ComfyUI process");
            }

            // No output redirection when using UseShellExecute = true
            OnLogMessage?.Invoke("ComfyUI console window opened - check it for output");

            OnLogMessage?.Invoke($"ComfyUI process started (PID: {_comfyUIProcess.Id})");

            // Wait for ComfyUI to be ready (check HTTP endpoint)
            var ready = await WaitForReadyAsync(timeoutSeconds: 60);

            if (ready)
            {
                OnStatusChanged?.Invoke(true);
                return (true, $"ComfyUI started successfully on port {Port}");
            }
            else
            {
                return (false, "ComfyUI started but is not responding - check logs");
            }
        }
        catch (Exception ex)
        {
            OnLogMessage?.Invoke($"Error starting ComfyUI: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
        finally
        {
            _isStarting = false;
        }
    }

    /// <summary>
    /// Stop ComfyUI gracefully
    /// </summary>
    public async Task<(bool success, string message)> StopAsync()
    {
        if (!IsRunning)
        {
            return (true, "ComfyUI is not running");
        }

        try
        {
            OnLogMessage?.Invoke("Stopping ComfyUI...");

            // Try graceful shutdown first
            _comfyUIProcess!.CloseMainWindow();

            // Wait for graceful exit
            var exited = await Task.Run(() => _comfyUIProcess.WaitForExit(5000));

            if (!exited)
            {
                OnLogMessage?.Invoke("Graceful shutdown timed out, forcing kill...");
                _comfyUIProcess.Kill(entireProcessTree: true);
                await Task.Run(() => _comfyUIProcess.WaitForExit(3000));
            }

            _comfyUIProcess.Dispose();
            _comfyUIProcess = null;

            OnStatusChanged?.Invoke(false);
            OnLogMessage?.Invoke("ComfyUI stopped");

            return (true, "ComfyUI stopped successfully");
        }
        catch (Exception ex)
        {
            OnLogMessage?.Invoke($"Error stopping ComfyUI: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Find Python executable in ComfyUI's virtual environment or system
    /// </summary>
    private string? FindPythonExecutable()
    {
        // Check for embedded Python in ComfyUI (common for portable installs)
        var embeddedPaths = new[]
        {
            Path.Combine(ComfyUIPath, "python_embeded", "python.exe"),
            Path.Combine(ComfyUIPath, "python_embedded", "python.exe"),
            Path.Combine(ComfyUIPath, "venv", "Scripts", "python.exe"),
            Path.Combine(ComfyUIPath, ".venv", "Scripts", "python.exe"),
            Path.Combine(ComfyUIPath, "python", "python.exe"),
        };

        foreach (var path in embeddedPaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        // Check parent directory for AILocal structure
        var parentPython = Path.Combine(Path.GetDirectoryName(ComfyUIPath)!, "python", "python.exe");
        if (File.Exists(parentPython))
        {
            return parentPython;
        }

        // Fall back to system Python
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "where",
                Arguments = "python",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 0)
                {
                    return lines[0].Trim();
                }
            }
        }
        catch { }

        return null;
    }

    /// <summary>
    /// Wait for ComfyUI HTTP endpoint to be ready
    /// </summary>
    private async Task<bool> WaitForReadyAsync(int timeoutSeconds = 60)
    {
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(2);

        var endpoint = $"http://127.0.0.1:{Port}/system_stats";
        var startTime = DateTime.UtcNow;

        while ((DateTime.UtcNow - startTime).TotalSeconds < timeoutSeconds)
        {
            try
            {
                var response = await httpClient.GetAsync(endpoint);
                if (response.IsSuccessStatusCode)
                {
                    OnLogMessage?.Invoke("ComfyUI is ready!");
                    return true;
                }
            }
            catch
            {
                // Not ready yet
            }

            // Check if process died
            if (_comfyUIProcess == null || _comfyUIProcess.HasExited)
            {
                OnLogMessage?.Invoke("ComfyUI process died during startup");
                return false;
            }

            await Task.Delay(1000);
            OnLogMessage?.Invoke($"Waiting for ComfyUI to be ready... ({(int)(DateTime.UtcNow - startTime).TotalSeconds}s)");
        }

        return false;
    }

    /// <summary>
    /// Check if a port is in use
    /// </summary>
    private async Task<bool> IsPortInUseAsync(int port)
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(2);
            var response = await httpClient.GetAsync($"http://127.0.0.1:{port}/");
            return true; // Got a response, port is in use
        }
        catch
        {
            return false; // Connection failed, port is free
        }
    }

    #region IHostedService Implementation

    /// <summary>
    /// Called when the application host starts - we don't auto-start ComfyUI here,
    /// let the UI handle that based on user preference
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        OnLogMessage?.Invoke("ComfyUIProcessManager initialized");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when the application host stops - ensures ComfyUI is properly shut down
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        OnLogMessage?.Invoke("Application stopping - shutting down ComfyUI...");
        Console.WriteLine(">>> ComfyUIProcessManager.StopAsync called - stopping ComfyUI");

        if (IsRunning)
        {
            await StopAsync();
        }

        Console.WriteLine(">>> ComfyUI shutdown complete");
    }

    #endregion

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (IsRunning)
        {
            try
            {
                _comfyUIProcess?.Kill(entireProcessTree: true);
                _comfyUIProcess?.Dispose();
            }
            catch { }
        }

        GC.SuppressFinalize(this);
    }
}
