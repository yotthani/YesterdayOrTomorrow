namespace StarTrekGame.Services;

/// <summary>
/// Service for generating dynamic visual effects for stars and planets.
/// Provides CSS-based effects for performance with dynamic calculations.
/// </summary>
public class SpaceEffectsService
{
    #region Star Effects

    /// <summary>
    /// Get CSS classes and styles for a star based on its type
    /// </summary>
    public StarEffectStyle GetStarStyle(StarType type, int size = 64)
    {
        var (coreColor, glowColor, flareColor, intensity) = type switch
        {
            StarType.YellowDwarf => ("#FFF4E0", "#FFD700", "#FFAA00", 1.0),
            StarType.RedDwarf => ("#FFB4A0", "#FF6B4A", "#FF4500", 0.6),
            StarType.OrangeStar => ("#FFD4A0", "#FFA500", "#FF8C00", 0.8),
            StarType.BlueGiant => ("#E0F0FF", "#4A9FFF", "#0066FF", 1.5),
            StarType.RedGiant => ("#FFC4B0", "#FF4500", "#CC0000", 2.0),
            StarType.WhiteDwarf => ("#FFFFFF", "#E0E0FF", "#A0A0FF", 0.4),
            StarType.NeutronStar => ("#E0FFFF", "#00FFFF", "#0088FF", 0.3),
            StarType.BinaryPair => ("#FFF4E0", "#FFD700", "#4A9FFF", 1.2),
            _ => ("#FFF4E0", "#FFD700", "#FFAA00", 1.0)
        };

        var glowSize = (int)(size * intensity * 2);
        var flareSize = (int)(size * intensity * 3);
        var pulseSpeed = 3 + (intensity * 2); // seconds

        return new StarEffectStyle
        {
            CoreStyle = $@"
                width: {size}px;
                height: {size}px;
                background: radial-gradient(circle, {coreColor} 0%, {glowColor} 60%, transparent 100%);
                border-radius: 50%;
                position: relative;
            ",
            GlowStyle = $@"
                position: absolute;
                width: {glowSize}px;
                height: {glowSize}px;
                top: 50%;
                left: 50%;
                transform: translate(-50%, -50%);
                background: radial-gradient(circle, 
                    {glowColor}80 0%, 
                    {glowColor}40 30%, 
                    {glowColor}10 60%, 
                    transparent 100%);
                border-radius: 50%;
                pointer-events: none;
                animation: starPulse {pulseSpeed}s ease-in-out infinite;
            ",
            FlareStyle = $@"
                position: absolute;
                width: {flareSize}px;
                height: {flareSize}px;
                top: 50%;
                left: 50%;
                transform: translate(-50%, -50%);
                background: 
                    linear-gradient(0deg, transparent 45%, {flareColor}40 49%, {flareColor}60 50%, {flareColor}40 51%, transparent 55%),
                    linear-gradient(90deg, transparent 45%, {flareColor}40 49%, {flareColor}60 50%, {flareColor}40 51%, transparent 55%);
                pointer-events: none;
                animation: starFlareRotate 30s linear infinite;
            ",
            CssClass = $"star star-{type.ToString().ToLower()}"
        };
    }

    /// <summary>
    /// Get lens flare effect for bright stars (appears when star is near screen edge)
    /// </summary>
    public string GetLensFlareStyle(double starX, double starY, double screenWidth, double screenHeight, string flareColor = "#FFD700")
    {
        // Calculate distance from center
        var centerX = screenWidth / 2;
        var centerY = screenHeight / 2;
        var dx = starX - centerX;
        var dy = starY - centerY;
        var distance = Math.Sqrt(dx * dx + dy * dy);
        var maxDistance = Math.Sqrt(centerX * centerX + centerY * centerY);
        
        // Flare intensity increases towards edges
        var intensity = Math.Min(1.0, distance / maxDistance);
        if (intensity < 0.3) return "display: none;"; // No flare near center

        // Flare position (opposite side of center from star)
        var flareX = centerX - dx * 0.5;
        var flareY = centerY - dy * 0.5;

        return $@"
            position: absolute;
            left: {flareX}px;
            top: {flareY}px;
            width: {60 * intensity}px;
            height: {60 * intensity}px;
            background: radial-gradient(circle, {flareColor}{(int)(intensity * 60):X2} 0%, transparent 70%);
            border-radius: 50%;
            pointer-events: none;
        ";
    }

    #endregion

    #region Planet Effects

    /// <summary>
    /// Calculate shadow gradient for a planet based on star position
    /// </summary>
    public string GetPlanetShadowGradient(
        double planetX, double planetY, 
        double starX, double starY,
        double shadowIntensity = 0.95)
    {
        // Calculate direction from planet to star
        var dx = starX - planetX;
        var dy = starY - planetY;
        
        // Angle in degrees (shadow is opposite to light source)
        var lightAngle = Math.Atan2(dy, dx) * (180 / Math.PI);
        var shadowAngle = lightAngle + 180;

        // Normalize angle to 0-360
        while (shadowAngle < 0) shadowAngle += 360;
        while (shadowAngle >= 360) shadowAngle -= 360;

        return $@"linear-gradient(
            {shadowAngle:F1}deg,
            transparent 0%,
            transparent 35%,
            rgba(0,0,0,0.1) 42%,
            rgba(0,0,0,0.3) 48%,
            rgba(0,0,0,0.6) 52%,
            rgba(0,0,0,{shadowIntensity:F2}) 60%,
            rgba(0,0,0,{shadowIntensity:F2}) 100%
        )";
    }

    /// <summary>
    /// Get complete planet style with shadow overlay
    /// </summary>
    public PlanetEffectStyle GetPlanetStyle(
        double planetX, double planetY,
        double starX, double starY,
        int size = 64,
        bool hasAtmosphere = false,
        string atmosphereColor = "100,150,255")
    {
        var shadowGradient = GetPlanetShadowGradient(planetX, planetY, starX, starY);
        
        // Calculate light direction for atmosphere glow
        var dx = starX - planetX;
        var dy = starY - planetY;
        var lightAngle = Math.Atan2(dy, dx);
        var lightX = 30 + (int)(Math.Cos(lightAngle) * 30); // 0-60% range
        var lightY = 30 + (int)(Math.Sin(lightAngle) * 30);

        return new PlanetEffectStyle
        {
            ContainerStyle = $@"
                width: {size}px;
                height: {size}px;
                position: relative;
                border-radius: 50%;
                overflow: hidden;
            ",
            ShadowStyle = $@"
                position: absolute;
                inset: 0;
                border-radius: 50%;
                background: {shadowGradient};
                pointer-events: none;
                z-index: 2;
            ",
            AtmosphereStyle = hasAtmosphere ? $@"
                position: absolute;
                inset: -8%;
                border-radius: 50%;
                background: radial-gradient(
                    circle at {lightX}% {lightY}%,
                    rgba({atmosphereColor},0.4) 0%,
                    rgba({atmosphereColor},0.1) 30%,
                    transparent 60%
                );
                pointer-events: none;
                z-index: 1;
            " : "display: none;",
            TerminatorStyle = $@"
                position: absolute;
                inset: 0;
                border-radius: 50%;
                box-shadow: inset 0 0 {size / 8}px rgba(0,0,0,0.3);
                pointer-events: none;
                z-index: 3;
            "
        };
    }

    /// <summary>
    /// Get atmosphere color based on planet type
    /// </summary>
    public string GetAtmosphereColor(PlanetType type) => type switch
    {
        PlanetType.Terran => "100,150,255",      // Blue Earth-like
        PlanetType.Ocean => "80,140,255",        // Deep blue
        PlanetType.Jungle => "100,200,150",      // Green tint
        PlanetType.Desert => "255,200,150",      // Orange/tan
        PlanetType.Ice => "200,220,255",         // Pale blue
        PlanetType.Volcanic => "255,100,50",     // Red/orange
        PlanetType.Toxic => "200,255,100",       // Sickly green
        PlanetType.GasGiant => "255,200,150",    // Jupiter-like
        _ => "150,180,220"                       // Default bluish
    };

    #endregion

    #region Animation Keyframes CSS

    /// <summary>
    /// Get CSS keyframes for star animations (include once in page)
    /// </summary>
    public string GetAnimationKeyframesCSS() => @"
        @keyframes starPulse {
            0%, 100% { 
                transform: translate(-50%, -50%) scale(1); 
                opacity: 0.8; 
            }
            50% { 
                transform: translate(-50%, -50%) scale(1.15); 
                opacity: 1; 
            }
        }

        @keyframes starFlareRotate {
            0% { transform: translate(-50%, -50%) rotate(0deg); }
            100% { transform: translate(-50%, -50%) rotate(360deg); }
        }

        @keyframes planetRotate {
            0% { background-position: 0% 50%; }
            100% { background-position: 100% 50%; }
        }

        @keyframes atmosphereShimmer {
            0%, 100% { opacity: 0.7; }
            50% { opacity: 1; }
        }
    ";

    #endregion
}

#region Data Models

public enum StarType
{
    YellowDwarf,    // Our Sun, G-type
    RedDwarf,       // M-type, common
    OrangeStar,     // K-type
    BlueGiant,      // O/B-type, hot
    RedGiant,       // Dying star, expanded
    WhiteDwarf,     // Stellar remnant
    NeutronStar,    // Pulsar
    BinaryPair      // Two stars
}

public enum PlanetType
{
    Terran,         // Earth-like, Class M
    Ocean,          // Water world
    Jungle,         // Tropical
    Desert,         // Arid
    Ice,            // Frozen
    Volcanic,       // Lava world
    Toxic,          // Acidic atmosphere
    GasGiant,       // Jupiter-like
    Barren,         // Dead, rocky
    Crystalline     // Exotic
}

public class StarEffectStyle
{
    public string CoreStyle { get; set; } = "";
    public string GlowStyle { get; set; } = "";
    public string FlareStyle { get; set; } = "";
    public string CssClass { get; set; } = "";
}

public class PlanetEffectStyle
{
    public string ContainerStyle { get; set; } = "";
    public string ShadowStyle { get; set; } = "";
    public string AtmosphereStyle { get; set; } = "";
    public string TerminatorStyle { get; set; } = "";
}

#endregion
