using SkiaSharp;

namespace StarTrekGame.AssetGenerator.Services;

/// <summary>
/// Programmatic generator for LCARS-style UI elements.
/// Creates perfect geometric shapes without AI - guaranteed consistent results.
/// </summary>
public class LCARSGenerator
{
    // LCARS color palette
    public static class Colors
    {
        public static readonly SKColor Orange = new(255, 153, 0);       // #FF9900 - Primary
        public static readonly SKColor Peach = new(255, 204, 153);      // #FFCC99 - Secondary
        public static readonly SKColor Tan = new(204, 166, 128);        // #CCA680 - Beige/Tan
        public static readonly SKColor Blue = new(153, 153, 255);       // #9999FF - Data
        public static readonly SKColor LightBlue = new(153, 204, 255);  // #99CCFF - Highlight
        public static readonly SKColor Purple = new(204, 153, 204);     // #CC99CC - Alert
        public static readonly SKColor Lavender = new(204, 153, 255);   // #CC99FF - Status
        public static readonly SKColor Red = new(255, 102, 102);        // #FF6666 - Warning
        public static readonly SKColor White = new(255, 255, 255);      // #FFFFFF - Text
        public static readonly SKColor Black = new(0, 0, 0);            // #000000 - Background
    }

    public event Action<string>? OnStatusMessage;

    /// <summary>
    /// Whether to use black background with glow (like Gemini output) or transparent background
    /// </summary>
    public bool UseBlackBackgroundWithGlow { get; set; } = true;

    /// <summary>
    /// Generate a LCARS UI element as base64 PNG
    /// </summary>
    public Task<GenerationResult> GenerateAsync(LCARSElementType elementType, SKColor color, int width, int height)
    {
        try
        {
            OnStatusMessage?.Invoke($"Generating LCARS {elementType} ({width}x{height})...");

            using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
            var canvas = surface.Canvas;

            // Use black background with glow for better visual quality (like Gemini output)
            if (UseBlackBackgroundWithGlow)
            {
                canvas.Clear(Colors.Black);
            }
            else
            {
                canvas.Clear(SKColors.Transparent);
            }

            // Create outer glow effect (subtle ambient glow)
            using var outerGlowPaint = new SKPaint
            {
                Color = color.WithAlpha(40),
                IsAntialias = true,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 25)
            };

            // Create inner glow effect (brighter, closer)
            using var glowPaint = new SKPaint
            {
                Color = color.WithAlpha(UseBlackBackgroundWithGlow ? (byte)150 : (byte)100),
                IsAntialias = true,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, UseBlackBackgroundWithGlow ? 15 : 12)
            };

            // Create main shape paint
            using var shapePaint = new SKPaint
            {
                Color = color,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            // Draw based on element type - pass outer glow when using black background
            switch (elementType)
            {
                case LCARSElementType.PillButton:
                    DrawPillButton(canvas, width, height, glowPaint, shapePaint, UseBlackBackgroundWithGlow ? outerGlowPaint : null);
                    break;
                case LCARSElementType.RoundedRectangle:
                    DrawRoundedRectangle(canvas, width, height, glowPaint, shapePaint, UseBlackBackgroundWithGlow ? outerGlowPaint : null);
                    break;
                case LCARSElementType.ElbowLeft:
                    DrawElbowSimple(canvas, width, height, glowPaint, shapePaint, isLeft: true, UseBlackBackgroundWithGlow ? outerGlowPaint : null);
                    break;
                case LCARSElementType.ElbowRight:
                    DrawElbowSimple(canvas, width, height, glowPaint, shapePaint, isLeft: false, UseBlackBackgroundWithGlow ? outerGlowPaint : null);
                    break;
                case LCARSElementType.HorizontalBar:
                    DrawHorizontalBar(canvas, width, height, glowPaint, shapePaint, UseBlackBackgroundWithGlow ? outerGlowPaint : null);
                    break;
                case LCARSElementType.VerticalBar:
                    DrawVerticalBar(canvas, width, height, glowPaint, shapePaint, UseBlackBackgroundWithGlow ? outerGlowPaint : null);
                    break;
                case LCARSElementType.Circle:
                    DrawCircle(canvas, width, height, glowPaint, shapePaint, UseBlackBackgroundWithGlow ? outerGlowPaint : null);
                    break;
                case LCARSElementType.ProgressBarEmpty:
                    DrawProgressBarEmpty(canvas, width, height, color);
                    break;
                case LCARSElementType.ProgressBarFilled:
                    DrawHorizontalBar(canvas, width, height, glowPaint, shapePaint, UseBlackBackgroundWithGlow ? outerGlowPaint : null);
                    break;
                case LCARSElementType.CornerBracketTopLeft:
                case LCARSElementType.CornerBracketTopRight:
                case LCARSElementType.CornerBracketBottomLeft:
                case LCARSElementType.CornerBracketBottomRight:
                    DrawCornerBracket(canvas, width, height, glowPaint, shapePaint, elementType, UseBlackBackgroundWithGlow ? outerGlowPaint : null);
                    break;
                case LCARSElementType.HeaderBar:
                    DrawHeaderBar(canvas, width, height, glowPaint, shapePaint, UseBlackBackgroundWithGlow ? outerGlowPaint : null);
                    break;
                case LCARSElementType.FooterBar:
                    DrawFooterBar(canvas, width, height, glowPaint, shapePaint, UseBlackBackgroundWithGlow ? outerGlowPaint : null);
                    break;
                case LCARSElementType.DividerHorizontal:
                    DrawDivider(canvas, width, height, glowPaint, shapePaint, isHorizontal: true, UseBlackBackgroundWithGlow ? outerGlowPaint : null);
                    break;
                case LCARSElementType.DividerVertical:
                    DrawDivider(canvas, width, height, glowPaint, shapePaint, isHorizontal: false, UseBlackBackgroundWithGlow ? outerGlowPaint : null);
                    break;
                case LCARSElementType.CapEndLeft:
                    DrawCapEnd(canvas, width, height, glowPaint, shapePaint, isLeft: true, UseBlackBackgroundWithGlow ? outerGlowPaint : null);
                    break;
                case LCARSElementType.CapEndRight:
                    DrawCapEnd(canvas, width, height, glowPaint, shapePaint, isLeft: false, UseBlackBackgroundWithGlow ? outerGlowPaint : null);
                    break;
                case LCARSElementType.WindowFrame:
                    DrawWindowFrame(canvas, width, height, color, UseBlackBackgroundWithGlow ? outerGlowPaint : null);
                    break;
                default:
                    DrawRoundedRectangle(canvas, width, height, glowPaint, shapePaint, UseBlackBackgroundWithGlow ? outerGlowPaint : null);
                    break;
            }

            // Convert to PNG
            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            var base64 = Convert.ToBase64String(data.ToArray());

            OnStatusMessage?.Invoke($"LCARS {elementType} generated successfully");

            return Task.FromResult(new GenerationResult
            {
                Success = true,
                ImageBase64 = base64,
                PromptUsed = $"LCARS {elementType} - {ColorToName(color)}"
            });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new GenerationResult
            {
                Success = false,
                ErrorMessage = $"LCARS generation failed: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Parse element name to determine type and color
    /// </summary>
    public (LCARSElementType type, SKColor color) ParseElementName(string elementName)
    {
        var lower = elementName.ToLowerInvariant();

        // Determine color - check specific colors first
        SKColor color = Colors.Orange; // Default
        if (lower.Contains("blue") && !lower.Contains("light"))
            color = Colors.Blue;
        else if (lower.Contains("light blue") || lower.Contains("lightblue"))
            color = Colors.LightBlue;
        else if (lower.Contains("purple"))
            color = Colors.Purple;
        else if (lower.Contains("lavender"))
            color = Colors.Lavender;
        else if (lower.Contains("beige") || lower.Contains("tan"))
            color = Colors.Tan;
        else if (lower.Contains("peach"))
            color = Colors.Peach;
        else if (lower.Contains("red"))
            color = Colors.Red;
        // Orange is default, no need to check

        // Determine type - order matters for specificity
        LCARSElementType type;

        // Specific types first
        if (lower.Contains("pill") || lower.Contains("capsule"))
            type = LCARSElementType.PillButton;
        else if (lower.Contains("elbow left") || (lower.Contains("elbow") && lower.Contains("left")))
            type = LCARSElementType.ElbowLeft;
        else if (lower.Contains("elbow right") || (lower.Contains("elbow") && lower.Contains("right")))
            type = LCARSElementType.ElbowRight;
        else if (lower.Contains("elbow"))
            type = LCARSElementType.ElbowLeft; // Default to left if unspecified
        else if (lower.Contains("corner bracket left") || (lower.Contains("corner") && lower.Contains("bracket") && lower.Contains("left")))
            type = LCARSElementType.CornerBracketTopLeft;
        else if (lower.Contains("corner bracket right") || (lower.Contains("corner") && lower.Contains("bracket") && lower.Contains("right")))
            type = LCARSElementType.CornerBracketTopRight;
        else if (lower.Contains("bracket") && lower.Contains("left"))
            type = LCARSElementType.CornerBracketTopLeft;
        else if (lower.Contains("bracket") && lower.Contains("right"))
            type = LCARSElementType.CornerBracketTopRight;
        else if (lower.Contains("header"))
            type = LCARSElementType.HeaderBar;
        else if (lower.Contains("footer"))
            type = LCARSElementType.FooterBar;
        else if (lower.Contains("divider horizontal") || (lower.Contains("divider") && lower.Contains("horizontal")))
            type = LCARSElementType.DividerHorizontal;
        else if (lower.Contains("divider vertical") || (lower.Contains("divider") && lower.Contains("vertical")))
            type = LCARSElementType.DividerVertical;
        else if (lower.Contains("divider"))
            type = LCARSElementType.DividerHorizontal;
        else if (lower.Contains("cap end left") || (lower.Contains("cap") && lower.Contains("left")))
            type = LCARSElementType.CapEndLeft;
        else if (lower.Contains("cap end right") || (lower.Contains("cap") && lower.Contains("right")))
            type = LCARSElementType.CapEndRight;
        else if (lower.Contains("cap"))
            type = LCARSElementType.CapEndLeft;
        else if (lower.Contains("progress") && lower.Contains("empty"))
            type = LCARSElementType.ProgressBarEmpty;
        else if (lower.Contains("progress") || lower.Contains("track") || lower.Contains("meter") || lower.Contains("gauge") || lower.Contains("slider"))
            type = LCARSElementType.ProgressBarEmpty;
        else if (lower.Contains("bar") && lower.Contains("vertical"))
            type = LCARSElementType.VerticalBar;
        else if (lower.Contains("bar"))
            type = LCARSElementType.HorizontalBar;
        else if (lower.Contains("circle") || lower.Contains("dot") || lower.Contains("indicator") || lower.Contains("radio"))
            type = LCARSElementType.Circle;
        else if (lower.Contains("window") || lower.Contains("frame") || lower.Contains("dialog") || lower.Contains("alert") || lower.Contains("tooltip") || lower.Contains("panel") || lower.Contains("info"))
            type = LCARSElementType.WindowFrame;
        else if (lower.Contains("checkbox") || lower.Contains("toggle"))
            type = LCARSElementType.RoundedRectangle;
        else if (lower.Contains("tab") || lower.Contains("button") || lower.Contains("square") || lower.Contains("rounded") || lower.Contains("badge") || lower.Contains("display") || lower.Contains("number"))
            type = LCARSElementType.RoundedRectangle;
        else
            type = LCARSElementType.RoundedRectangle; // Default

        return (type, color);
    }

    #region Drawing Methods

    private void DrawPillButton(SKCanvas canvas, int width, int height, SKPaint glowPaint, SKPaint shapePaint, SKPaint? outerGlowPaint = null)
    {
        float margin = Math.Min(width, height) * 0.12f;  // Slightly more margin for glow room
        float radius = (height - margin * 2) / 2;

        var rect = new SKRect(margin, margin, width - margin, height - margin);

        // Draw outer glow first (if provided)
        if (outerGlowPaint != null)
        {
            canvas.DrawRoundRect(rect, radius, radius, outerGlowPaint);
        }
        canvas.DrawRoundRect(rect, radius, radius, glowPaint);
        canvas.DrawRoundRect(rect, radius, radius, shapePaint);
    }

    private void DrawRoundedRectangle(SKCanvas canvas, int width, int height, SKPaint glowPaint, SKPaint shapePaint, SKPaint? outerGlowPaint = null)
    {
        float margin = Math.Min(width, height) * 0.12f;
        float cornerRadius = Math.Min(width, height) * 0.15f;

        var rect = new SKRect(margin, margin, width - margin, height - margin);

        if (outerGlowPaint != null) canvas.DrawRoundRect(rect, cornerRadius, cornerRadius, outerGlowPaint);
        canvas.DrawRoundRect(rect, cornerRadius, cornerRadius, glowPaint);
        canvas.DrawRoundRect(rect, cornerRadius, cornerRadius, shapePaint);
    }

    private void DrawElbowSimple(SKCanvas canvas, int width, int height, SKPaint glowPaint, SKPaint shapePaint, bool isLeft, SKPaint? outerGlowPaint = null)
    {
        // Simplified LCARS elbow: an L-shaped piece with rounded corners
        float margin = Math.Min(width, height) * 0.08f;
        float thickness = Math.Min(width, height) * 0.35f;
        float cornerRadius = thickness / 2;

        using var path = new SKPath();

        if (isLeft)
        {
            // L-shape: vertical bar at left, horizontal bar at bottom
            float verticalBarRight = margin + thickness;
            float horizontalBarTop = height - margin - thickness;

            path.MoveTo(margin, margin);
            path.LineTo(verticalBarRight, margin);
            path.LineTo(verticalBarRight, horizontalBarTop);
            path.LineTo(width - margin, horizontalBarTop);
            path.LineTo(width - margin, height - margin);
            path.LineTo(margin, height - margin);
            path.Close();
        }
        else
        {
            // Mirror: vertical bar at right, horizontal bar at bottom
            float verticalBarLeft = width - margin - thickness;
            float horizontalBarTop = height - margin - thickness;

            path.MoveTo(width - margin, margin);
            path.LineTo(verticalBarLeft, margin);
            path.LineTo(verticalBarLeft, horizontalBarTop);
            path.LineTo(margin, horizontalBarTop);
            path.LineTo(margin, height - margin);
            path.LineTo(width - margin, height - margin);
            path.Close();
        }

        if (outerGlowPaint != null) canvas.DrawPath(path, outerGlowPaint);
        canvas.DrawPath(path, glowPaint);
        canvas.DrawPath(path, shapePaint);
    }

    private void DrawHorizontalBar(SKCanvas canvas, int width, int height, SKPaint glowPaint, SKPaint shapePaint, SKPaint? outerGlowPaint = null)
    {
        float margin = height * 0.15f;
        float radius = (height - margin * 2) / 2;

        var rect = new SKRect(margin, margin, width - margin, height - margin);

        if (outerGlowPaint != null) canvas.DrawRoundRect(rect, radius, radius, outerGlowPaint);
        canvas.DrawRoundRect(rect, radius, radius, glowPaint);
        canvas.DrawRoundRect(rect, radius, radius, shapePaint);
    }

    private void DrawVerticalBar(SKCanvas canvas, int width, int height, SKPaint glowPaint, SKPaint shapePaint, SKPaint? outerGlowPaint = null)
    {
        float margin = width * 0.15f;
        float radius = (width - margin * 2) / 2;

        var rect = new SKRect(margin, margin, width - margin, height - margin);

        if (outerGlowPaint != null) canvas.DrawRoundRect(rect, radius, radius, outerGlowPaint);
        canvas.DrawRoundRect(rect, radius, radius, glowPaint);
        canvas.DrawRoundRect(rect, radius, radius, shapePaint);
    }

    private void DrawCircle(SKCanvas canvas, int width, int height, SKPaint glowPaint, SKPaint shapePaint, SKPaint? outerGlowPaint = null)
    {
        float centerX = width / 2f;
        float centerY = height / 2f;
        float radius = Math.Min(width, height) * 0.35f;

        if (outerGlowPaint != null) canvas.DrawCircle(centerX, centerY, radius + 10, outerGlowPaint);
        canvas.DrawCircle(centerX, centerY, radius + 5, glowPaint);
        canvas.DrawCircle(centerX, centerY, radius, shapePaint);
    }

    private void DrawProgressBarEmpty(SKCanvas canvas, int width, int height, SKColor color)
    {
        float margin = height * 0.15f;
        float strokeWidth = Math.Max(3, height * 0.1f);
        float radius = (height - margin * 2) / 2;

        var rect = new SKRect(margin + strokeWidth/2, margin + strokeWidth/2,
                              width - margin - strokeWidth/2, height - margin - strokeWidth/2);

        using var strokePaint = new SKPaint
        {
            Color = color,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = strokeWidth
        };

        using var glowPaint = new SKPaint
        {
            Color = color.WithAlpha(60),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = strokeWidth + 6,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 4)
        };

        canvas.DrawRoundRect(rect, radius, radius, glowPaint);
        canvas.DrawRoundRect(rect, radius, radius, strokePaint);
    }

    private void DrawCornerBracket(SKCanvas canvas, int width, int height, SKPaint glowPaint, SKPaint shapePaint, LCARSElementType corner, SKPaint? outerGlowPaint = null)
    {
        float margin = Math.Min(width, height) * 0.08f;
        float thickness = Math.Min(width, height) * 0.25f;

        using var path = new SKPath();

        switch (corner)
        {
            case LCARSElementType.CornerBracketTopLeft:
                path.MoveTo(margin, margin);
                path.LineTo(width - margin, margin);
                path.LineTo(width - margin, margin + thickness);
                path.LineTo(margin + thickness, margin + thickness);
                path.LineTo(margin + thickness, height - margin);
                path.LineTo(margin, height - margin);
                path.Close();
                break;

            case LCARSElementType.CornerBracketTopRight:
                path.MoveTo(margin, margin);
                path.LineTo(width - margin, margin);
                path.LineTo(width - margin, height - margin);
                path.LineTo(width - margin - thickness, height - margin);
                path.LineTo(width - margin - thickness, margin + thickness);
                path.LineTo(margin, margin + thickness);
                path.Close();
                break;

            case LCARSElementType.CornerBracketBottomLeft:
                path.MoveTo(margin, margin);
                path.LineTo(margin + thickness, margin);
                path.LineTo(margin + thickness, height - margin - thickness);
                path.LineTo(width - margin, height - margin - thickness);
                path.LineTo(width - margin, height - margin);
                path.LineTo(margin, height - margin);
                path.Close();
                break;

            case LCARSElementType.CornerBracketBottomRight:
                path.MoveTo(width - margin, margin);
                path.LineTo(width - margin - thickness, margin);
                path.LineTo(width - margin - thickness, height - margin - thickness);
                path.LineTo(margin, height - margin - thickness);
                path.LineTo(margin, height - margin);
                path.LineTo(width - margin, height - margin);
                path.Close();
                break;
        }

        if (outerGlowPaint != null) canvas.DrawPath(path, outerGlowPaint);
        canvas.DrawPath(path, glowPaint);
        canvas.DrawPath(path, shapePaint);
    }

    private void DrawHeaderBar(SKCanvas canvas, int width, int height, SKPaint glowPaint, SKPaint shapePaint, SKPaint? outerGlowPaint = null)
    {
        // Header: pill shape at top
        float margin = Math.Min(width, height) * 0.1f;
        float barHeight = height * 0.6f;
        float radius = barHeight / 2;

        var rect = new SKRect(margin, margin, width - margin, margin + barHeight);

        if (outerGlowPaint != null) canvas.DrawRoundRect(rect, radius, radius, outerGlowPaint);
        canvas.DrawRoundRect(rect, radius, radius, glowPaint);
        canvas.DrawRoundRect(rect, radius, radius, shapePaint);
    }

    private void DrawFooterBar(SKCanvas canvas, int width, int height, SKPaint glowPaint, SKPaint shapePaint, SKPaint? outerGlowPaint = null)
    {
        // Footer: pill shape at bottom
        float margin = Math.Min(width, height) * 0.1f;
        float barHeight = height * 0.6f;
        float radius = barHeight / 2;

        var rect = new SKRect(margin, height - margin - barHeight, width - margin, height - margin);

        if (outerGlowPaint != null) canvas.DrawRoundRect(rect, radius, radius, outerGlowPaint);
        canvas.DrawRoundRect(rect, radius, radius, glowPaint);
        canvas.DrawRoundRect(rect, radius, radius, shapePaint);
    }

    private void DrawDivider(SKCanvas canvas, int width, int height, SKPaint glowPaint, SKPaint shapePaint, bool isHorizontal, SKPaint? outerGlowPaint = null)
    {
        float margin = Math.Min(width, height) * 0.12f;

        if (isHorizontal)
        {
            float barHeight = height * 0.2f;
            float y = (height - barHeight) / 2;
            var rect = new SKRect(margin, y, width - margin, y + barHeight);
            float radius = barHeight / 2;

            if (outerGlowPaint != null) canvas.DrawRoundRect(rect, radius, radius, outerGlowPaint);
            canvas.DrawRoundRect(rect, radius, radius, glowPaint);
            canvas.DrawRoundRect(rect, radius, radius, shapePaint);
        }
        else
        {
            float barWidth = width * 0.2f;
            float x = (width - barWidth) / 2;
            var rect = new SKRect(x, margin, x + barWidth, height - margin);
            float radius = barWidth / 2;

            if (outerGlowPaint != null) canvas.DrawRoundRect(rect, radius, radius, outerGlowPaint);
            canvas.DrawRoundRect(rect, radius, radius, glowPaint);
            canvas.DrawRoundRect(rect, radius, radius, shapePaint);
        }
    }

    private void DrawCapEnd(SKCanvas canvas, int width, int height, SKPaint glowPaint, SKPaint shapePaint, bool isLeft, SKPaint? outerGlowPaint = null)
    {
        // Cap end: semicircle
        float margin = Math.Min(width, height) * 0.1f;
        float radius = (height - margin * 2) / 2;

        if (isLeft)
        {
            var rect = new SKRect(margin, margin, width - margin + radius, height - margin);
            if (outerGlowPaint != null) canvas.DrawRoundRect(rect, radius, radius, outerGlowPaint);
            canvas.DrawRoundRect(rect, radius, radius, glowPaint);
            canvas.DrawRoundRect(rect, radius, radius, shapePaint);
        }
        else
        {
            var rect = new SKRect(margin - radius, margin, width - margin, height - margin);
            if (outerGlowPaint != null) canvas.DrawRoundRect(rect, radius, radius, outerGlowPaint);
            canvas.DrawRoundRect(rect, radius, radius, glowPaint);
            canvas.DrawRoundRect(rect, radius, radius, shapePaint);
        }
    }

    private void DrawWindowFrame(SKCanvas canvas, int width, int height, SKColor color, SKPaint? outerGlowPaint = null)
    {
        // Window frame: outline with LCARS styling and glow
        float margin = Math.Min(width, height) * 0.08f;
        float strokeWidth = Math.Max(4, Math.Min(width, height) * 0.08f);
        float cornerRadius = Math.Min(width, height) * 0.1f;

        var rect = new SKRect(margin + strokeWidth/2, margin + strokeWidth/2,
                              width - margin - strokeWidth/2, height - margin - strokeWidth/2);

        using var strokePaint = new SKPaint
        {
            Color = color,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = strokeWidth
        };

        using var glowPaint = new SKPaint
        {
            Color = color.WithAlpha(UseBlackBackgroundWithGlow ? (byte)120 : (byte)80),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = strokeWidth + 10,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 8)
        };

        // Outer ambient glow (very subtle)
        if (outerGlowPaint != null)
        {
            using var outerStrokeGlow = new SKPaint
            {
                Color = color.WithAlpha(30),
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = strokeWidth + 20,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 20)
            };
            canvas.DrawRoundRect(rect, cornerRadius, cornerRadius, outerStrokeGlow);
        }

        canvas.DrawRoundRect(rect, cornerRadius, cornerRadius, glowPaint);
        canvas.DrawRoundRect(rect, cornerRadius, cornerRadius, strokePaint);
    }

    #endregion

    private static string ColorToName(SKColor color)
    {
        if (color == Colors.Orange) return "Orange";
        if (color == Colors.Blue) return "Blue";
        if (color == Colors.Purple) return "Purple";
        if (color == Colors.Tan) return "Tan";
        if (color == Colors.Peach) return "Peach";
        if (color == Colors.Red) return "Red";
        if (color == Colors.LightBlue) return "Light Blue";
        if (color == Colors.Lavender) return "Lavender";
        return "Custom";
    }
}

public enum LCARSElementType
{
    PillButton,
    RoundedRectangle,
    ElbowLeft,
    ElbowRight,
    HorizontalBar,
    VerticalBar,
    Circle,
    ProgressBarEmpty,
    ProgressBarFilled,
    CornerBracketTopLeft,
    CornerBracketTopRight,
    CornerBracketBottomLeft,
    CornerBracketBottomRight,
    HeaderBar,
    FooterBar,
    DividerHorizontal,
    DividerVertical,
    CapEndLeft,
    CapEndRight,
    WindowFrame
}
