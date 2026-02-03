using Microsoft.JSInterop;

namespace StarTrekGame.Web.Services;

public interface ISoundService
{
    Task PlayAsync(string soundType, float volume = 1.0f);
    Task SetEnabledAsync(bool enabled);
    Task SetVolumeAsync(float volume);
    
    // Combat sounds
    Task PlayPhaserAsync(float volume = 1.0f);
    Task PlayTorpedoAsync(float volume = 1.0f);
    Task PlayExplosionAsync(float volume = 1.0f);
    Task PlayShieldHitAsync(float volume = 1.0f);
    Task PlayHullHitAsync(float volume = 1.0f);
    Task PlayCriticalAsync(float volume = 1.0f);
    
    // UI sounds
    Task PlayClickAsync(float volume = 1.0f);
    Task PlaySelectAsync(float volume = 1.0f);
    Task PlayErrorAsync(float volume = 1.0f);
    Task PlaySuccessAsync(float volume = 1.0f);
    Task PlayNotificationAsync(float volume = 1.0f);
    Task PlayTurnEndAsync(float volume = 1.0f);
    
    // Ambient sounds
    Task PlayWarpAsync(float volume = 1.0f);
    Task PlayScanAsync(float volume = 1.0f);
    Task PlayCommAsync(float volume = 1.0f);
    Task PlayAlertAsync(float volume = 1.0f);
    Task PlayBuildCompleteAsync(float volume = 1.0f);
}

public class SoundService : ISoundService
{
    private readonly IJSRuntime _js;
    private bool _initialized;

    public SoundService(IJSRuntime js)
    {
        _js = js;
    }

    private async Task EnsureInitialized()
    {
        if (!_initialized)
        {
            _initialized = true;
            // The sound system is initialized via the script in MainLayout
        }
    }

    public async Task PlayAsync(string soundType, float volume = 1.0f)
    {
        try
        {
            await EnsureInitialized();
            await _js.InvokeVoidAsync("GameSounds.play", soundType, volume);
        }
        catch
        {
            // Silently ignore sound errors - don't break the app
        }
    }

    public async Task SetEnabledAsync(bool enabled)
    {
        try
        {
            await _js.InvokeVoidAsync("GameSounds.setEnabled", enabled);
        }
        catch { }
    }

    public async Task SetVolumeAsync(float volume)
    {
        try
        {
            await _js.InvokeVoidAsync("GameSounds.setVolume", volume);
        }
        catch { }
    }

    // Combat sounds
    public Task PlayPhaserAsync(float volume = 1.0f) => PlayAsync("phaser", volume);
    public Task PlayTorpedoAsync(float volume = 1.0f) => PlayAsync("torpedo", volume);
    public Task PlayExplosionAsync(float volume = 1.0f) => PlayAsync("explosion", volume);
    public Task PlayShieldHitAsync(float volume = 1.0f) => PlayAsync("shield_hit", volume);
    public Task PlayHullHitAsync(float volume = 1.0f) => PlayAsync("hull_hit", volume);
    public Task PlayCriticalAsync(float volume = 1.0f) => PlayAsync("critical", volume);

    // UI sounds
    public Task PlayClickAsync(float volume = 1.0f) => PlayAsync("click", volume);
    public Task PlaySelectAsync(float volume = 1.0f) => PlayAsync("select", volume);
    public Task PlayErrorAsync(float volume = 1.0f) => PlayAsync("error", volume);
    public Task PlaySuccessAsync(float volume = 1.0f) => PlayAsync("success", volume);
    public Task PlayNotificationAsync(float volume = 1.0f) => PlayAsync("notification", volume);
    public Task PlayTurnEndAsync(float volume = 1.0f) => PlayAsync("turn_end", volume);

    // Ambient sounds
    public Task PlayWarpAsync(float volume = 1.0f) => PlayAsync("warp", volume);
    public Task PlayScanAsync(float volume = 1.0f) => PlayAsync("scan", volume);
    public Task PlayCommAsync(float volume = 1.0f) => PlayAsync("comm", volume);
    public Task PlayAlertAsync(float volume = 1.0f) => PlayAsync("alert", volume);
    public Task PlayBuildCompleteAsync(float volume = 1.0f) => PlayAsync("build_complete", volume);
}
