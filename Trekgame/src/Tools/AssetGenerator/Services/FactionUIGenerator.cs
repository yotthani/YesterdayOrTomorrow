using SkiaSharp;

namespace StarTrekGame.AssetGenerator.Services;

/// <summary>
/// Programmatic generator for faction-specific UI elements.
/// Each faction has distinctive visual language from Star Trek canon:
/// - Federation: LCARS (colorful rounded shapes)
/// - Klingon: Angular, battle-worn metal, red accents
/// - Romulan: Elegant curves, green/bronze, bird motifs
/// - Borg: Hexagonal patterns, green-on-black, technological horror
/// - Cardassian: Geometric efficiency, amber/brown
/// </summary>
public class FactionUIGenerator
{
    public event Action<string>? OnStatusMessage;

    /// <summary>
    /// Whether to use black/dark background with glow
    /// </summary>
    public bool UseBackgroundWithGlow { get; set; } = true;

    #region Color Palettes

    public static class FederationColors
    {
        public static readonly SKColor Orange = new(255, 153, 0);       // #FF9900 - Primary
        public static readonly SKColor Peach = new(255, 204, 153);      // #FFCC99 - Secondary
        public static readonly SKColor Tan = new(204, 166, 128);        // #CCA680 - Beige/Tan
        public static readonly SKColor Blue = new(153, 153, 255);       // #9999FF - Data
        public static readonly SKColor LightBlue = new(153, 204, 255);  // #99CCFF - Highlight
        public static readonly SKColor Purple = new(204, 153, 204);     // #CC99CC - Alert
        public static readonly SKColor Lavender = new(204, 153, 255);   // #CC99FF - Status
        public static readonly SKColor Red = new(255, 102, 102);        // #FF6666 - Warning
        public static readonly SKColor Background = new(0, 0, 0);       // Pure black
    }

    public static class KlingonColors
    {
        public static readonly SKColor BloodRed = new(204, 0, 0);       // #CC0000 - Primary
        public static readonly SKColor BrightRed = new(255, 51, 51);    // #FF3333 - Accent
        public static readonly SKColor Gunmetal = new(74, 74, 74);      // #4A4A4A - Frame
        public static readonly SKColor DarkMetal = new(51, 51, 51);     // #333333 - Dark frame
        public static readonly SKColor Bronze = new(139, 115, 85);      // #8B7355 - Bone/bronze
        public static readonly SKColor BurningOrange = new(255, 102, 0);// #FF6600 - Hot accent
        public static readonly SKColor Background = new(10, 0, 0);      // Near-black with red tint
    }

    public static class RomulanColors
    {
        public static readonly SKColor EmeraldGreen = new(0, 170, 68);  // #00AA44 - Primary
        public static readonly SKColor BrightGreen = new(0, 204, 85);   // #00CC55 - Highlight
        public static readonly SKColor DarkGreen = new(0, 68, 51);      // #004433 - Shadow
        public static readonly SKColor Bronze = new(212, 175, 55);      // #D4AF37 - Gold accent
        public static readonly SKColor DarkBronze = new(184, 134, 11);  // #B8860B - Darker gold
        public static readonly SKColor Teal = new(0, 255, 170);         // #00FFAA - Glow
        public static readonly SKColor Background = new(0, 10, 5);      // Very dark green-black
    }

    public static class BorgColors
    {
        public static readonly SKColor Green = new(0, 255, 0);          // #00FF00 - Primary
        public static readonly SKColor BrightGreen = new(51, 255, 51);  // #33FF33 - Glow
        public static readonly SKColor DarkGreen = new(0, 34, 0);       // #002200 - Shadow
        public static readonly SKColor Gunmetal = new(51, 51, 51);      // #333333 - Metal
        public static readonly SKColor White = new(255, 255, 255);      // #FFFFFF - Data
        public static readonly SKColor Background = new(0, 5, 0);       // Pure black with green tint
    }

    public static class CardassianColors
    {
        public static readonly SKColor AmberOrange = new(255, 136, 0);  // #FF8800 - Primary
        public static readonly SKColor DarkOrange = new(204, 102, 0);   // #CC6600 - Secondary
        public static readonly SKColor DarkBrown = new(74, 53, 32);     // #4A3520 - Frame
        public static readonly SKColor Yellow = new(255, 204, 0);       // #FFCC00 - Highlight
        public static readonly SKColor Cream = new(255, 238, 204);      // #FFEECC - Text
        public static readonly SKColor Background = new(10, 5, 0);      // Dark brown-black
    }

    #endregion

    /// <summary>
    /// Generate a UI element for the specified faction
    /// </summary>
    public Task<GenerationResult> GenerateAsync(string faction, UIElementType elementType, int width, int height)
    {
        return faction.ToLowerInvariant() switch
        {
            "federation" => GenerateFederationAsync(elementType, width, height),
            "klingon" => GenerateKlingonAsync(elementType, width, height),
            "romulan" => GenerateRomulanAsync(elementType, width, height),
            "borg" => GenerateBorgAsync(elementType, width, height),
            "cardassian" => GenerateCardassianAsync(elementType, width, height),
            _ => GenerateFederationAsync(elementType, width, height) // Default to Federation
        };
    }

    #region Federation (LCARS) Generation

    private Task<GenerationResult> GenerateFederationAsync(UIElementType elementType, int width, int height)
    {
        OnStatusMessage?.Invoke($"Generating Federation LCARS {elementType} ({width}x{height})...");

        using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;
        canvas.Clear(UseBackgroundWithGlow ? FederationColors.Background : SKColors.Transparent);

        var color = FederationColors.Orange;

        switch (elementType)
        {
            case UIElementType.Button:
                DrawLCARSPill(canvas, width, height, color);
                break;
            case UIElementType.PanelFrame:
                DrawLCARSFrame(canvas, width, height);
                break;
            case UIElementType.ProgressBar:
                DrawLCARSProgressBar(canvas, width, height, color);
                break;
            case UIElementType.HeaderBar:
                DrawLCARSPill(canvas, width, height, color);
                break;
            case UIElementType.CornerAccent:
                DrawLCARSElbow(canvas, width, height, color);
                break;
            default:
                DrawLCARSPill(canvas, width, height, color);
                break;
        }

        return EncodeResult(surface, $"Federation LCARS {elementType}");
    }

    private void DrawLCARSPill(SKCanvas canvas, int width, int height, SKColor color)
    {
        float margin = Math.Min(width, height) * 0.12f;
        float radius = (height - margin * 2) / 2;
        var rect = new SKRect(margin, margin, width - margin, height - margin);

        // Outer glow
        using var outerGlow = new SKPaint
        {
            Color = color.WithAlpha(40),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 25)
        };
        canvas.DrawRoundRect(rect, radius, radius, outerGlow);

        // Inner glow
        using var innerGlow = new SKPaint
        {
            Color = color.WithAlpha(150),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 15)
        };
        canvas.DrawRoundRect(rect, radius, radius, innerGlow);

        // Main shape
        using var fill = new SKPaint { Color = color, IsAntialias = true };
        canvas.DrawRoundRect(rect, radius, radius, fill);
    }

    private void DrawLCARSFrame(SKCanvas canvas, int width, int height)
    {
        // LCARS frame: thick bar on left, thin bar on top, elbow corner
        float barWidth = width * 0.15f;
        float barHeight = height * 0.08f;

        // Left bar (orange)
        DrawLCARSBar(canvas, 0, barHeight, barWidth, height - barHeight, FederationColors.Orange);

        // Top bar (tan/peach)
        DrawLCARSBar(canvas, barWidth, 0, width - barWidth, barHeight, FederationColors.Peach);

        // Elbow corner (purple)
        DrawLCARSElbowCorner(canvas, 0, 0, barWidth, barHeight, FederationColors.Lavender);

        // Bottom accent bar
        DrawLCARSBar(canvas, barWidth, height - barHeight * 0.5f, width - barWidth, barHeight * 0.5f, FederationColors.Blue);
    }

    private void DrawLCARSBar(SKCanvas canvas, float x, float y, float w, float h, SKColor color)
    {
        float radius = Math.Min(w, h) * 0.3f;
        var rect = new SKRect(x + 2, y + 2, x + w - 2, y + h - 2);

        using var glow = new SKPaint
        {
            Color = color.WithAlpha(100),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 8)
        };
        canvas.DrawRoundRect(rect, radius, radius, glow);

        using var fill = new SKPaint { Color = color, IsAntialias = true };
        canvas.DrawRoundRect(rect, radius, radius, fill);
    }

    private void DrawLCARSElbowCorner(SKCanvas canvas, float x, float y, float w, float h, SKColor color)
    {
        using var path = new SKPath();
        path.MoveTo(x, y);
        path.LineTo(x + w, y);
        path.LineTo(x + w, y + h);
        path.LineTo(x + w * 0.3f, y + h);
        path.ArcTo(new SKRect(x, y + h * 0.3f, x + w * 0.6f, y + h), 90, 90, false);
        path.LineTo(x, y);
        path.Close();

        using var glow = new SKPaint
        {
            Color = color.WithAlpha(100),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 8)
        };
        canvas.DrawPath(path, glow);

        using var fill = new SKPaint { Color = color, IsAntialias = true };
        canvas.DrawPath(path, fill);
    }

    private void DrawLCARSElbow(SKCanvas canvas, int width, int height, SKColor color)
    {
        float margin = Math.Min(width, height) * 0.08f;
        float thickness = Math.Min(width, height) * 0.35f;

        using var path = new SKPath();
        path.MoveTo(margin, margin);
        path.LineTo(margin + thickness, margin);
        path.LineTo(margin + thickness, height - margin - thickness);
        path.LineTo(width - margin, height - margin - thickness);
        path.LineTo(width - margin, height - margin);
        path.LineTo(margin, height - margin);
        path.Close();

        using var outerGlow = new SKPaint
        {
            Color = color.WithAlpha(40),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 25)
        };
        canvas.DrawPath(path, outerGlow);

        using var innerGlow = new SKPaint
        {
            Color = color.WithAlpha(150),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 15)
        };
        canvas.DrawPath(path, innerGlow);

        using var fill = new SKPaint { Color = color, IsAntialias = true };
        canvas.DrawPath(path, fill);
    }

    private void DrawLCARSProgressBar(SKCanvas canvas, int width, int height, SKColor color)
    {
        float margin = height * 0.15f;
        float strokeWidth = Math.Max(3, height * 0.1f);
        float radius = (height - margin * 2) / 2;

        var rect = new SKRect(margin + strokeWidth / 2, margin + strokeWidth / 2,
                              width - margin - strokeWidth / 2, height - margin - strokeWidth / 2);

        using var glow = new SKPaint
        {
            Color = color.WithAlpha(60),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = strokeWidth + 6,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 4)
        };
        canvas.DrawRoundRect(rect, radius, radius, glow);

        using var stroke = new SKPaint
        {
            Color = color,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = strokeWidth
        };
        canvas.DrawRoundRect(rect, radius, radius, stroke);
    }

    #endregion

    #region Klingon Generation

    private Task<GenerationResult> GenerateKlingonAsync(UIElementType elementType, int width, int height)
    {
        OnStatusMessage?.Invoke($"Generating Klingon {elementType} ({width}x{height})...");

        using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;
        canvas.Clear(UseBackgroundWithGlow ? KlingonColors.Background : SKColors.Transparent);

        switch (elementType)
        {
            case UIElementType.Button:
                DrawKlingonButton(canvas, width, height);
                break;
            case UIElementType.PanelFrame:
                DrawKlingonFrame(canvas, width, height);
                break;
            case UIElementType.ProgressBar:
                DrawKlingonProgressBar(canvas, width, height);
                break;
            case UIElementType.HeaderBar:
                DrawKlingonHeader(canvas, width, height);
                break;
            case UIElementType.CornerAccent:
                DrawKlingonCorner(canvas, width, height);
                break;
            default:
                DrawKlingonButton(canvas, width, height);
                break;
        }

        return EncodeResult(surface, $"Klingon {elementType}");
    }

    private void DrawKlingonButton(SKCanvas canvas, int width, int height)
    {
        // Klingon: angular, aggressive, metal with red glow
        float margin = Math.Min(width, height) * 0.1f;
        float bevel = Math.Min(width, height) * 0.15f;

        using var path = new SKPath();
        // Angular hexagon-ish shape with aggressive cuts
        path.MoveTo(margin + bevel, margin);
        path.LineTo(width - margin - bevel, margin);
        path.LineTo(width - margin, margin + bevel);
        path.LineTo(width - margin, height - margin - bevel);
        path.LineTo(width - margin - bevel, height - margin);
        path.LineTo(margin + bevel, height - margin);
        path.LineTo(margin, height - margin - bevel);
        path.LineTo(margin, margin + bevel);
        path.Close();

        // Red glow
        using var glow = new SKPaint
        {
            Color = KlingonColors.BloodRed.WithAlpha(100),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 15)
        };
        canvas.DrawPath(path, glow);

        // Metal fill with gradient
        using var fill = new SKPaint
        {
            IsAntialias = true,
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, margin),
                new SKPoint(0, height - margin),
                new[] { KlingonColors.Gunmetal, KlingonColors.DarkMetal },
                SKShaderTileMode.Clamp)
        };
        canvas.DrawPath(path, fill);

        // Red accent stripe
        float stripeY = height * 0.4f;
        using var stripe = new SKPaint
        {
            Color = KlingonColors.BloodRed,
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 3)
        };
        canvas.DrawRect(margin + bevel, stripeY, width - 2 * margin - 2 * bevel, height * 0.08f, stripe);
    }

    private void DrawKlingonFrame(SKCanvas canvas, int width, int height)
    {
        float border = Math.Min(width, height) * 0.06f;

        // Outer frame - angular with spikes
        DrawKlingonAngularFrame(canvas, 0, 0, width, height, border, KlingonColors.DarkMetal);

        // Inner red glow line
        float inset = border + 3;
        using var glowLine = new SKPaint
        {
            Color = KlingonColors.BloodRed.WithAlpha(150),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 5)
        };
        canvas.DrawRect(inset, inset, width - 2 * inset, height - 2 * inset, glowLine);

        // Corner spikes
        DrawKlingonSpike(canvas, 0, 0, border * 2, true, true);
        DrawKlingonSpike(canvas, width, 0, border * 2, false, true);
        DrawKlingonSpike(canvas, 0, height, border * 2, true, false);
        DrawKlingonSpike(canvas, width, height, border * 2, false, false);
    }

    private void DrawKlingonAngularFrame(SKCanvas canvas, float x, float y, float w, float h, float thickness, SKColor color)
    {
        using var framePaint = new SKPaint
        {
            Color = color,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = thickness
        };

        // Red glow behind frame
        using var glow = new SKPaint
        {
            Color = KlingonColors.BloodRed.WithAlpha(60),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = thickness + 10,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 10)
        };

        var rect = new SKRect(x + thickness / 2, y + thickness / 2, x + w - thickness / 2, y + h - thickness / 2);
        canvas.DrawRect(rect, glow);
        canvas.DrawRect(rect, framePaint);
    }

    private void DrawKlingonSpike(SKCanvas canvas, float x, float y, float size, bool leftSide, bool topSide)
    {
        using var path = new SKPath();
        float dx = leftSide ? 1 : -1;
        float dy = topSide ? 1 : -1;

        path.MoveTo(x, y);
        path.LineTo(x + dx * size, y);
        path.LineTo(x + dx * size * 0.3f, y + dy * size * 0.7f);
        path.LineTo(x, y + dy * size);
        path.Close();

        using var fill = new SKPaint
        {
            Color = KlingonColors.Bronze,
            IsAntialias = true
        };
        canvas.DrawPath(path, fill);

        // Red glow on spike
        using var glow = new SKPaint
        {
            Color = KlingonColors.BloodRed.WithAlpha(100),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 5)
        };
        canvas.DrawPath(path, glow);
    }

    private void DrawKlingonProgressBar(SKCanvas canvas, int width, int height)
    {
        float margin = height * 0.2f;
        float bevel = height * 0.1f;

        using var path = new SKPath();
        path.MoveTo(margin + bevel, margin);
        path.LineTo(width - margin - bevel, margin);
        path.LineTo(width - margin, margin + bevel);
        path.LineTo(width - margin, height - margin - bevel);
        path.LineTo(width - margin - bevel, height - margin);
        path.LineTo(margin + bevel, height - margin);
        path.LineTo(margin, height - margin - bevel);
        path.LineTo(margin, margin + bevel);
        path.Close();

        using var glow = new SKPaint
        {
            Color = KlingonColors.BloodRed.WithAlpha(80),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 4,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 6)
        };
        canvas.DrawPath(path, glow);

        using var stroke = new SKPaint
        {
            Color = KlingonColors.Gunmetal,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3
        };
        canvas.DrawPath(path, stroke);
    }

    private void DrawKlingonHeader(SKCanvas canvas, int width, int height)
    {
        DrawKlingonButton(canvas, width, height);

        // Add trefoil emblem suggestion (simplified)
        float cx = width * 0.1f;
        float cy = height * 0.5f;
        float r = Math.Min(width, height) * 0.15f;

        using var emblem = new SKPaint
        {
            Color = KlingonColors.BloodRed,
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 3)
        };
        canvas.DrawCircle(cx, cy, r, emblem);
    }

    private void DrawKlingonCorner(SKCanvas canvas, int width, int height)
    {
        // Aggressive angular corner with spike
        float thickness = Math.Min(width, height) * 0.3f;
        float spikeLength = Math.Min(width, height) * 0.4f;

        using var path = new SKPath();
        path.MoveTo(0, 0);
        path.LineTo(width, 0);
        path.LineTo(width, thickness);
        path.LineTo(thickness + spikeLength * 0.3f, thickness);
        path.LineTo(thickness, thickness + spikeLength);
        path.LineTo(thickness, height);
        path.LineTo(0, height);
        path.Close();

        using var glow = new SKPaint
        {
            Color = KlingonColors.BloodRed.WithAlpha(100),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 15)
        };
        canvas.DrawPath(path, glow);

        using var fill = new SKPaint
        {
            IsAntialias = true,
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(width, height),
                new[] { KlingonColors.Gunmetal, KlingonColors.DarkMetal },
                SKShaderTileMode.Clamp)
        };
        canvas.DrawPath(path, fill);
    }

    #endregion

    #region Romulan Generation

    private Task<GenerationResult> GenerateRomulanAsync(UIElementType elementType, int width, int height)
    {
        OnStatusMessage?.Invoke($"Generating Romulan {elementType} ({width}x{height})...");

        using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;
        canvas.Clear(UseBackgroundWithGlow ? RomulanColors.Background : SKColors.Transparent);

        switch (elementType)
        {
            case UIElementType.Button:
                DrawRomulanButton(canvas, width, height);
                break;
            case UIElementType.PanelFrame:
                DrawRomulanFrame(canvas, width, height);
                break;
            case UIElementType.ProgressBar:
                DrawRomulanProgressBar(canvas, width, height);
                break;
            case UIElementType.HeaderBar:
                DrawRomulanHeader(canvas, width, height);
                break;
            case UIElementType.CornerAccent:
                DrawRomulanCorner(canvas, width, height);
                break;
            default:
                DrawRomulanButton(canvas, width, height);
                break;
        }

        return EncodeResult(surface, $"Romulan {elementType}");
    }

    private void DrawRomulanButton(SKCanvas canvas, int width, int height)
    {
        // Romulan: elegant sweeping curves, like bird wings
        float margin = Math.Min(width, height) * 0.1f;

        using var path = new SKPath();
        // Elegant wing-like shape
        path.MoveTo(margin, height * 0.5f);
        path.QuadTo(margin, margin, width * 0.3f, margin);
        path.LineTo(width * 0.7f, margin);
        path.QuadTo(width - margin, margin, width - margin, height * 0.5f);
        path.QuadTo(width - margin, height - margin, width * 0.7f, height - margin);
        path.LineTo(width * 0.3f, height - margin);
        path.QuadTo(margin, height - margin, margin, height * 0.5f);
        path.Close();

        // Green glow
        using var outerGlow = new SKPaint
        {
            Color = RomulanColors.EmeraldGreen.WithAlpha(50),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 20)
        };
        canvas.DrawPath(path, outerGlow);

        // Inner glow
        using var innerGlow = new SKPaint
        {
            Color = RomulanColors.BrightGreen.WithAlpha(100),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 10)
        };
        canvas.DrawPath(path, innerGlow);

        // Fill with gradient (dark to light green)
        using var fill = new SKPaint
        {
            IsAntialias = true,
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, margin),
                new SKPoint(0, height - margin),
                new[] { RomulanColors.DarkGreen, RomulanColors.EmeraldGreen },
                SKShaderTileMode.Clamp)
        };
        canvas.DrawPath(path, fill);

        // Bronze accent line
        using var accent = new SKPaint
        {
            Color = RomulanColors.Bronze,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2
        };
        canvas.DrawPath(path, accent);
    }

    private void DrawRomulanFrame(SKCanvas canvas, int width, int height)
    {
        float border = Math.Min(width, height) * 0.05f;

        // Elegant frame with curved corners
        var rect = new SKRect(border, border, width - border, height - border);
        float cornerRadius = Math.Min(width, height) * 0.08f;

        // Outer green glow
        using var outerGlow = new SKPaint
        {
            Color = RomulanColors.EmeraldGreen.WithAlpha(40),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = border + 15,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 15)
        };
        canvas.DrawRoundRect(rect, cornerRadius, cornerRadius, outerGlow);

        // Bronze frame
        using var frame = new SKPaint
        {
            Color = RomulanColors.Bronze,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = border
        };
        canvas.DrawRoundRect(rect, cornerRadius, cornerRadius, frame);

        // Inner green line
        var innerRect = new SKRect(border * 2, border * 2, width - border * 2, height - border * 2);
        using var innerLine = new SKPaint
        {
            Color = RomulanColors.EmeraldGreen.WithAlpha(150),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 3)
        };
        canvas.DrawRoundRect(innerRect, cornerRadius * 0.8f, cornerRadius * 0.8f, innerLine);

        // Bird wing accents in corners
        DrawRomulanWingAccent(canvas, border, border, cornerRadius, true, true);
        DrawRomulanWingAccent(canvas, width - border, border, cornerRadius, false, true);
    }

    private void DrawRomulanWingAccent(SKCanvas canvas, float x, float y, float size, bool leftSide, bool topSide)
    {
        using var path = new SKPath();
        float dx = leftSide ? 1 : -1;
        float dy = topSide ? 1 : -1;

        // Simplified bird wing curve
        path.MoveTo(x, y);
        path.QuadTo(x + dx * size * 0.5f, y + dy * size * 0.2f, x + dx * size, y + dy * size * 0.5f);
        path.QuadTo(x + dx * size * 0.7f, y + dy * size * 0.7f, x + dx * size * 0.3f, y + dy * size);

        using var stroke = new SKPaint
        {
            Color = RomulanColors.Bronze,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3
        };
        canvas.DrawPath(path, stroke);
    }

    private void DrawRomulanProgressBar(SKCanvas canvas, int width, int height)
    {
        float margin = height * 0.2f;
        float cornerRadius = height * 0.3f;

        var rect = new SKRect(margin, margin, width - margin, height - margin);

        using var glow = new SKPaint
        {
            Color = RomulanColors.EmeraldGreen.WithAlpha(60),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 5,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 5)
        };
        canvas.DrawRoundRect(rect, cornerRadius, cornerRadius, glow);

        using var stroke = new SKPaint
        {
            Color = RomulanColors.Bronze,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2
        };
        canvas.DrawRoundRect(rect, cornerRadius, cornerRadius, stroke);
    }

    private void DrawRomulanHeader(SKCanvas canvas, int width, int height)
    {
        DrawRomulanButton(canvas, width, height);
    }

    private void DrawRomulanCorner(SKCanvas canvas, int width, int height)
    {
        // Elegant sweeping curve corner - Art Deco inspired
        using var path = new SKPath();
        path.MoveTo(0, 0);
        path.LineTo(width, 0);
        path.QuadTo(width * 0.6f, height * 0.2f, width * 0.3f, height * 0.3f);
        path.QuadTo(width * 0.2f, height * 0.6f, 0, height);
        path.Close();

        using var glow = new SKPaint
        {
            Color = RomulanColors.EmeraldGreen.WithAlpha(80),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 15)
        };
        canvas.DrawPath(path, glow);

        using var fill = new SKPaint
        {
            IsAntialias = true,
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(width, height),
                new[] { RomulanColors.Bronze, RomulanColors.DarkBronze },
                SKShaderTileMode.Clamp)
        };
        canvas.DrawPath(path, fill);

        // Green accent line
        using var accent = new SKPaint
        {
            Color = RomulanColors.EmeraldGreen,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 3)
        };
        canvas.DrawPath(path, accent);
    }

    #endregion

    #region Borg Generation

    private Task<GenerationResult> GenerateBorgAsync(UIElementType elementType, int width, int height)
    {
        OnStatusMessage?.Invoke($"Generating Borg {elementType} ({width}x{height})...");

        using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;
        canvas.Clear(UseBackgroundWithGlow ? BorgColors.Background : SKColors.Transparent);

        switch (elementType)
        {
            case UIElementType.Button:
                DrawBorgButton(canvas, width, height);
                break;
            case UIElementType.PanelFrame:
                DrawBorgFrame(canvas, width, height);
                break;
            case UIElementType.ProgressBar:
                DrawBorgProgressBar(canvas, width, height);
                break;
            case UIElementType.HeaderBar:
                DrawBorgHeader(canvas, width, height);
                break;
            case UIElementType.CornerAccent:
                DrawBorgCorner(canvas, width, height);
                break;
            default:
                DrawBorgButton(canvas, width, height);
                break;
        }

        return EncodeResult(surface, $"Borg {elementType}");
    }

    private void DrawBorgButton(SKCanvas canvas, int width, int height)
    {
        // Borg: hexagonal, cold, technological
        float margin = Math.Min(width, height) * 0.1f;
        float hexInset = height * 0.25f;

        using var path = new SKPath();
        // Hexagonal shape
        path.MoveTo(margin + hexInset, margin);
        path.LineTo(width - margin - hexInset, margin);
        path.LineTo(width - margin, height * 0.5f);
        path.LineTo(width - margin - hexInset, height - margin);
        path.LineTo(margin + hexInset, height - margin);
        path.LineTo(margin, height * 0.5f);
        path.Close();

        // Green glow
        using var glow = new SKPaint
        {
            Color = BorgColors.Green.WithAlpha(80),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 12)
        };
        canvas.DrawPath(path, glow);

        // Fill with dark green
        using var fill = new SKPaint
        {
            Color = BorgColors.DarkGreen,
            IsAntialias = true
        };
        canvas.DrawPath(path, fill);

        // Green edge
        using var edge = new SKPaint
        {
            Color = BorgColors.Green,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2
        };
        canvas.DrawPath(path, edge);

        // Data stream lines
        DrawBorgDataLines(canvas, margin, margin, width - 2 * margin, height - 2 * margin);
    }

    private void DrawBorgDataLines(SKCanvas canvas, float x, float y, float w, float h)
    {
        using var linePaint = new SKPaint
        {
            Color = BorgColors.Green.WithAlpha(100),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1
        };

        // Horizontal data lines
        for (int i = 1; i < 4; i++)
        {
            float lineY = y + h * i / 4;
            canvas.DrawLine(x + w * 0.1f, lineY, x + w * 0.9f, lineY, linePaint);
        }
    }

    private void DrawBorgFrame(SKCanvas canvas, int width, int height)
    {
        float border = Math.Min(width, height) * 0.04f;

        // Draw hexagonal honeycomb pattern as background
        DrawBorgHoneycomb(canvas, border, border, width - 2 * border, height - 2 * border);

        // Frame
        var rect = new SKRect(border, border, width - border, height - border);

        using var glow = new SKPaint
        {
            Color = BorgColors.Green.WithAlpha(60),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = border + 8,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 10)
        };
        canvas.DrawRect(rect, glow);

        using var frame = new SKPaint
        {
            Color = BorgColors.Green,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = border
        };
        canvas.DrawRect(rect, frame);
    }

    private void DrawBorgHoneycomb(SKCanvas canvas, float x, float y, float w, float h)
    {
        float hexSize = Math.Min(w, h) * 0.08f;
        float hexWidth = hexSize * 2;
        float hexHeight = hexSize * (float)Math.Sqrt(3);

        using var hexPaint = new SKPaint
        {
            Color = BorgColors.Green.WithAlpha(30),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1
        };

        for (float row = y; row < y + h; row += hexHeight * 0.75f)
        {
            float offset = ((int)((row - y) / (hexHeight * 0.75f)) % 2) * hexWidth * 0.5f;
            for (float col = x + offset; col < x + w; col += hexWidth)
            {
                DrawHexagon(canvas, col, row, hexSize, hexPaint);
            }
        }
    }

    private void DrawHexagon(SKCanvas canvas, float cx, float cy, float size, SKPaint paint)
    {
        using var path = new SKPath();
        for (int i = 0; i < 6; i++)
        {
            float angle = (float)(Math.PI / 3 * i - Math.PI / 6);
            float px = cx + size * (float)Math.Cos(angle);
            float py = cy + size * (float)Math.Sin(angle);
            if (i == 0) path.MoveTo(px, py);
            else path.LineTo(px, py);
        }
        path.Close();
        canvas.DrawPath(path, paint);
    }

    private void DrawBorgProgressBar(SKCanvas canvas, int width, int height)
    {
        float margin = height * 0.2f;

        var rect = new SKRect(margin, margin, width - margin, height - margin);

        using var glow = new SKPaint
        {
            Color = BorgColors.Green.WithAlpha(60),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 4,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 5)
        };
        canvas.DrawRect(rect, glow);

        using var stroke = new SKPaint
        {
            Color = BorgColors.Green,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2
        };
        canvas.DrawRect(rect, stroke);

        // Segment lines
        int segments = 10;
        float segWidth = (width - 2 * margin) / segments;
        using var segPaint = new SKPaint
        {
            Color = BorgColors.Green.WithAlpha(50),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1
        };
        for (int i = 1; i < segments; i++)
        {
            float sx = margin + i * segWidth;
            canvas.DrawLine(sx, margin, sx, height - margin, segPaint);
        }
    }

    private void DrawBorgHeader(SKCanvas canvas, int width, int height)
    {
        DrawBorgButton(canvas, width, height);
    }

    private void DrawBorgCorner(SKCanvas canvas, int width, int height)
    {
        // Borg corner: angular with hexagon pattern
        float thickness = Math.Min(width, height) * 0.3f;

        using var path = new SKPath();
        path.MoveTo(0, 0);
        path.LineTo(width, 0);
        path.LineTo(width, thickness);
        path.LineTo(thickness, thickness);
        path.LineTo(thickness, height);
        path.LineTo(0, height);
        path.Close();

        using var glow = new SKPaint
        {
            Color = BorgColors.Green.WithAlpha(80),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 15)
        };
        canvas.DrawPath(path, glow);

        using var fill = new SKPaint
        {
            Color = BorgColors.DarkGreen,
            IsAntialias = true
        };
        canvas.DrawPath(path, fill);

        using var edge = new SKPaint
        {
            Color = BorgColors.Green,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2
        };
        canvas.DrawPath(path, edge);

        // Small hexagons as texture
        DrawBorgHoneycomb(canvas, 2, 2, thickness - 4, thickness - 4);
    }

    #endregion

    #region Cardassian Generation

    private Task<GenerationResult> GenerateCardassianAsync(UIElementType elementType, int width, int height)
    {
        OnStatusMessage?.Invoke($"Generating Cardassian {elementType} ({width}x{height})...");

        using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;
        canvas.Clear(UseBackgroundWithGlow ? CardassianColors.Background : SKColors.Transparent);

        switch (elementType)
        {
            case UIElementType.Button:
                DrawCardassianButton(canvas, width, height);
                break;
            case UIElementType.PanelFrame:
                DrawCardassianFrame(canvas, width, height);
                break;
            case UIElementType.ProgressBar:
                DrawCardassianProgressBar(canvas, width, height);
                break;
            default:
                DrawCardassianButton(canvas, width, height);
                break;
        }

        return EncodeResult(surface, $"Cardassian {elementType}");
    }

    private void DrawCardassianButton(SKCanvas canvas, int width, int height)
    {
        // Cardassian: geometric, efficient, surveillance aesthetic
        float margin = Math.Min(width, height) * 0.1f;
        float notch = Math.Min(width, height) * 0.1f;

        using var path = new SKPath();
        // Geometric shape with small notches
        path.MoveTo(margin + notch, margin);
        path.LineTo(width - margin, margin);
        path.LineTo(width - margin, height - margin - notch);
        path.LineTo(width - margin - notch, height - margin);
        path.LineTo(margin, height - margin);
        path.LineTo(margin, margin + notch);
        path.Close();

        // Amber glow
        using var glow = new SKPaint
        {
            Color = CardassianColors.AmberOrange.WithAlpha(80),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 12)
        };
        canvas.DrawPath(path, glow);

        // Fill with gradient
        using var fill = new SKPaint
        {
            IsAntialias = true,
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(margin, margin),
                new SKPoint(width - margin, height - margin),
                new[] { CardassianColors.DarkBrown, CardassianColors.DarkOrange },
                SKShaderTileMode.Clamp)
        };
        canvas.DrawPath(path, fill);

        // Amber accent line
        using var accent = new SKPaint
        {
            Color = CardassianColors.AmberOrange,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2
        };
        canvas.DrawPath(path, accent);
    }

    private void DrawCardassianFrame(SKCanvas canvas, int width, int height)
    {
        float border = Math.Min(width, height) * 0.05f;

        var rect = new SKRect(border, border, width - border, height - border);

        using var glow = new SKPaint
        {
            Color = CardassianColors.AmberOrange.WithAlpha(50),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = border + 10,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 12)
        };
        canvas.DrawRect(rect, glow);

        using var frame = new SKPaint
        {
            Color = CardassianColors.DarkBrown,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = border
        };
        canvas.DrawRect(rect, frame);

        // Inner amber line
        var innerRect = new SKRect(border * 2, border * 2, width - border * 2, height - border * 2);
        using var innerLine = new SKPaint
        {
            Color = CardassianColors.AmberOrange.WithAlpha(150),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 2)
        };
        canvas.DrawRect(innerRect, innerLine);
    }

    private void DrawCardassianProgressBar(SKCanvas canvas, int width, int height)
    {
        float margin = height * 0.2f;

        var rect = new SKRect(margin, margin, width - margin, height - margin);

        using var glow = new SKPaint
        {
            Color = CardassianColors.AmberOrange.WithAlpha(60),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 4,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 5)
        };
        canvas.DrawRect(rect, glow);

        using var stroke = new SKPaint
        {
            Color = CardassianColors.DarkBrown,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2
        };
        canvas.DrawRect(rect, stroke);
    }

    #endregion

    #region Helpers

    private Task<GenerationResult> EncodeResult(SKSurface surface, string description)
    {
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        var base64 = Convert.ToBase64String(data.ToArray());

        OnStatusMessage?.Invoke($"{description} generated successfully");

        return Task.FromResult(new GenerationResult
        {
            Success = true,
            ImageBase64 = base64,
            PromptUsed = description
        });
    }

    #endregion
}

/// <summary>
/// Generic UI element types that apply across all factions
/// </summary>
public enum UIElementType
{
    Button,
    PanelFrame,
    ProgressBar,
    HeaderBar,
    CornerAccent,
    Sidebar,
    AlertBox,
    Tooltip,
    MinimapFrame,
    Divider
}
