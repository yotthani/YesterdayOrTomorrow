using SkiaSharp;

namespace StarTrekGame.AssetGenerator.Services;

/// <summary>
/// Generates authentic LCARS (Library Computer Access/Retrieval System) theme PNG assets
/// for the Federation UI theme. Assets are transparent PNGs for use as CSS background-images.
/// Inspired by authentic TNG-era LCARS interface design.
/// </summary>
public class LCARSThemeGenerator
{
    public event Action<string>? OnStatusMessage;

    private static class Colors
    {
        public static readonly SKColor Orange    = new(255, 153,   0);  // #FF9900 - Primary
        public static readonly SKColor Gold      = new(255, 204,   0);  // #FFCC00 - Header accent
        public static readonly SKColor Peach     = new(255, 204, 153);  // #FFCC99 - Light segment
        public static readonly SKColor Tan       = new(204, 153, 102);  // #CC9966 - Mid segment
        public static readonly SKColor Lavender  = new(204, 153, 204);  // #CC99CC - Sidebar accent
        public static readonly SKColor Purple    = new(153, 102, 204);  // #9966CC - Deep sidebar
        public static readonly SKColor Blue      = new(153, 153, 255);  // #9999FF - Data blue
        public static readonly SKColor LightBlue = new(153, 204, 255);  // #99CCFF - Highlight
        public static readonly SKColor Black     = new(0, 0, 0);
        public static readonly SKColor Transparent = SKColors.Transparent;
    }

    /// <summary>Generate all LCARS theme assets and save to the given directory.</summary>
    public async Task GenerateAllAsync(string outputDir)
    {
        Directory.CreateDirectory(outputDir);

        var tasks = new List<(string name, Func<byte[]> gen)>
        {
            ("elbow-top-left.png",       GenElbowTopLeft),
            ("elbow-bottom-left.png",    GenElbowBottomLeft),
            ("bar-horizontal-orange.png",() => GenHorizontalBar(Colors.Orange, Colors.Gold)),
            ("bar-horizontal-tan.png",   () => GenHorizontalBar(Colors.Tan,    Colors.Peach)),
            ("bar-horizontal-blue.png",  () => GenHorizontalBar(Colors.Blue,   Colors.LightBlue)),
            ("bar-vertical-orange.png",  () => GenVerticalBar(Colors.Orange,   Colors.Gold)),
            ("bar-vertical-purple.png",  () => GenVerticalBar(Colors.Lavender, Colors.Purple)),
            ("bar-vertical-blue.png",    () => GenVerticalBar(Colors.Blue,     Colors.LightBlue)),
            ("cap-right.png",            GenCapRight),
            ("cap-bottom.png",           GenCapBottom),
            ("button-pill-orange.png",   () => GenButtonPill(Colors.Orange, Colors.Gold)),
            ("button-pill-purple.png",   () => GenButtonPill(Colors.Lavender, Colors.Purple)),
            ("button-pill-blue.png",     () => GenButtonPill(Colors.Blue, Colors.LightBlue)),
            ("panel-frame.png",          GenPanelFrame),
            ("sidebar-full.png",         GenSidebarFull),
            ("header-full.png",          GenHeaderFull),
        };

        foreach (var (name, gen) in tasks)
        {
            OnStatusMessage?.Invoke($"Generating {name}...");
            var bytes = await Task.Run(gen);
            var path  = Path.Combine(outputDir, name);
            await File.WriteAllBytesAsync(path, bytes);
            OnStatusMessage?.Invoke($"Saved {path}");
        }

        OnStatusMessage?.Invoke("All LCARS theme assets generated.");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Elbow: top-left L-shaped connector
    // 300×150 transparent PNG
    // ──────────────────────────────────────────────────────────────────────────
    private byte[] GenElbowTopLeft()
    {
        const int W = 300, H = 150;
        using var surface = SKSurface.Create(new SKImageInfo(W, H, SKColorType.Rgba8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        float barV = 80f;   // vertical bar width
        float barH = 50f;   // horizontal bar height
        float r    = 48f;   // outer radius
        float ri   = 22f;   // inner radius

        // ── main elbow path ──────────────────────────────────────────────────
        using var path = new SKPath();
        // Start bottom-left of vertical bar
        path.MoveTo(0, H);
        // Left side up
        path.LineTo(0, r);
        // Outer arc top-left corner (outer)
        path.ArcTo(new SKRect(0, 0, r * 2, r * 2), 180, 90, false);
        // Top edge to right
        path.LineTo(W, 0);
        // Right side of horizontal bar down
        path.LineTo(W, barH);
        // Back to inner corner area
        path.LineTo(barV + ri, barH);
        // Inner arc (concave corner)
        path.ArcTo(new SKRect(barV - ri * 0, barH - ri * 0, barV + ri * 2, barH + ri * 2), 270, -90, false);
        // Down left side of inner cutout
        path.LineTo(barV, H);
        path.Close();

        // Gradient: left=orange, right=gold, adds depth
        using var grad = SKShader.CreateLinearGradient(
            new SKPoint(0, 0), new SKPoint(W * 0.6f, H),
            new[] { Colors.Orange, Colors.Gold, Colors.Peach },
            new[] { 0f, 0.5f, 1f },
            SKShaderTileMode.Clamp);

        using var fillPaint = new SKPaint { IsAntialias = true, Shader = grad };
        canvas.DrawPath(path, fillPaint);

        // Sheen: top-center highlight
        using var sheenGrad = SKShader.CreateLinearGradient(
            new SKPoint(W * 0.2f, 0), new SKPoint(W * 0.2f, barH),
            new[] { SKColors.White.WithAlpha(70), SKColors.Transparent },
            SKShaderTileMode.Clamp);
        using var sheenPaint = new SKPaint { IsAntialias = true, Shader = sheenGrad };
        canvas.DrawPath(path, sheenPaint);

        // Edge highlight line (thin bright line on outer edge for 3D feel)
        using var edgePaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2f,
            Color = Colors.Peach.WithAlpha(200)
        };
        canvas.DrawPath(path, edgePaint);

        return EncodePng(surface);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Elbow: bottom-left connector (closes the sidebar at the bottom)
    // 300×150 transparent PNG
    // ──────────────────────────────────────────────────────────────────────────
    private byte[] GenElbowBottomLeft()
    {
        const int W = 300, H = 150;
        using var surface = SKSurface.Create(new SKImageInfo(W, H, SKColorType.Rgba8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        float barV = 80f;
        float barH = 50f;
        float r    = 48f;
        float ri   = 22f;

        using var path = new SKPath();
        path.MoveTo(0, 0);
        path.LineTo(barV, 0);
        path.LineTo(barV, H - barH - ri);
        path.ArcTo(new SKRect(barV, H - barH - ri * 2, barV + ri * 2, H - barH), 180, 90, false);
        path.LineTo(W, H - barH);
        path.LineTo(W, H);
        path.LineTo(r, H);
        path.ArcTo(new SKRect(0, H - r * 2, r * 2, H), 90, 90, false);
        path.Close();

        using var grad = SKShader.CreateLinearGradient(
            new SKPoint(0, H), new SKPoint(W * 0.6f, 0),
            new[] { Colors.Purple, Colors.Lavender, Colors.Tan },
            new[] { 0f, 0.5f, 1f },
            SKShaderTileMode.Clamp);

        using var fillPaint = new SKPaint { IsAntialias = true, Shader = grad };
        canvas.DrawPath(path, fillPaint);

        using var sheenGrad = SKShader.CreateLinearGradient(
            new SKPoint(W * 0.2f, H), new SKPoint(W * 0.2f, H - barH),
            new[] { SKColors.White.WithAlpha(60), SKColors.Transparent },
            SKShaderTileMode.Clamp);
        using var sheenPaint = new SKPaint { IsAntialias = true, Shader = sheenGrad };
        canvas.DrawPath(path, sheenPaint);

        using var edgePaint = new SKPaint
        {
            IsAntialias = true, Style = SKPaintStyle.Stroke,
            StrokeWidth = 2f, Color = Colors.Lavender.WithAlpha(200)
        };
        canvas.DrawPath(path, edgePaint);

        return EncodePng(surface);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Horizontal bar tile - 100×40 transparent PNG (repeat-x safe)
    // ──────────────────────────────────────────────────────────────────────────
    private byte[] GenHorizontalBar(SKColor colorA, SKColor colorB)
    {
        const int W = 100, H = 40;
        using var surface = SKSurface.Create(new SKImageInfo(W, H, SKColorType.Rgba8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        using var grad = SKShader.CreateLinearGradient(
            new SKPoint(0, 0), new SKPoint(0, H),
            new[] { colorB.WithAlpha(220), colorA, colorA.WithAlpha(210) },
            new[] { 0f, 0.4f, 1f },
            SKShaderTileMode.Clamp);

        var rect = new SKRect(0, 0, W, H);
        using var fillPaint = new SKPaint { IsAntialias = true, Shader = grad };
        canvas.DrawRect(rect, fillPaint);

        // Top sheen
        using var sheenGrad = SKShader.CreateLinearGradient(
            new SKPoint(0, 0), new SKPoint(0, H * 0.4f),
            new[] { SKColors.White.WithAlpha(80), SKColors.Transparent },
            SKShaderTileMode.Clamp);
        using var sheenPaint = new SKPaint { IsAntialias = true, Shader = sheenGrad };
        canvas.DrawRect(rect, sheenPaint);

        return EncodePng(surface);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Vertical bar tile - 80×100 transparent PNG (repeat-y safe)
    // ──────────────────────────────────────────────────────────────────────────
    private byte[] GenVerticalBar(SKColor colorA, SKColor colorB)
    {
        const int W = 80, H = 100;
        using var surface = SKSurface.Create(new SKImageInfo(W, H, SKColorType.Rgba8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        using var grad = SKShader.CreateLinearGradient(
            new SKPoint(0, 0), new SKPoint(W, 0),
            new[] { colorB.WithAlpha(210), colorA, colorA.WithAlpha(200) },
            new[] { 0f, 0.4f, 1f },
            SKShaderTileMode.Clamp);

        var rect = new SKRect(0, 0, W, H);
        using var fillPaint = new SKPaint { IsAntialias = true, Shader = grad };
        canvas.DrawRect(rect, fillPaint);

        // Left-edge sheen (vertical highlight)
        using var sheenGrad = SKShader.CreateLinearGradient(
            new SKPoint(0, 0), new SKPoint(W * 0.35f, 0),
            new[] { SKColors.White.WithAlpha(70), SKColors.Transparent },
            SKShaderTileMode.Clamp);
        using var sheenPaint = new SKPaint { IsAntialias = true, Shader = sheenGrad };
        canvas.DrawRect(rect, sheenPaint);

        return EncodePng(surface);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Right cap: pill end-cap for horizontal bars - 40×40 transparent PNG
    // ──────────────────────────────────────────────────────────────────────────
    private byte[] GenCapRight()
    {
        const int S = 40;
        using var surface = SKSurface.Create(new SKImageInfo(S, S, SKColorType.Rgba8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        using var path = new SKPath();
        path.MoveTo(0, 0);
        path.ArcTo(new SKRect(0, 0, S, S), 270, 180, false);
        path.LineTo(0, S);
        path.Close();

        using var grad = SKShader.CreateLinearGradient(
            new SKPoint(0, 0), new SKPoint(0, S),
            new[] { Colors.Gold.WithAlpha(220), Colors.Orange, Colors.Orange.WithAlpha(210) },
            new[] { 0f, 0.4f, 1f },
            SKShaderTileMode.Clamp);
        using var fillPaint = new SKPaint { IsAntialias = true, Shader = grad };
        canvas.DrawPath(path, fillPaint);

        using var sheenGrad = SKShader.CreateLinearGradient(
            new SKPoint(0, 0), new SKPoint(0, S * 0.4f),
            new[] { SKColors.White.WithAlpha(80), SKColors.Transparent },
            SKShaderTileMode.Clamp);
        using var sheenPaint = new SKPaint { IsAntialias = true, Shader = sheenGrad };
        canvas.DrawPath(path, sheenPaint);

        return EncodePng(surface);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Bottom cap: pill end-cap for vertical bars - 80×40 transparent PNG
    // ──────────────────────────────────────────────────────────────────────────
    private byte[] GenCapBottom()
    {
        const int W = 80, H = 40;
        using var surface = SKSurface.Create(new SKImageInfo(W, H, SKColorType.Rgba8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        using var path = new SKPath();
        path.MoveTo(0, 0);
        path.LineTo(W, 0);
        path.ArcTo(new SKRect(0, 0, W, H * 2), 0, 180, false);
        path.Close();

        using var grad = SKShader.CreateLinearGradient(
            new SKPoint(0, 0), new SKPoint(W, 0),
            new[] { Colors.Purple.WithAlpha(210), Colors.Lavender, Colors.Purple.WithAlpha(200) },
            new[] { 0f, 0.5f, 1f },
            SKShaderTileMode.Clamp);
        using var fillPaint = new SKPaint { IsAntialias = true, Shader = grad };
        canvas.DrawPath(path, fillPaint);

        using var sheenGrad = SKShader.CreateLinearGradient(
            new SKPoint(0, 0), new SKPoint(W * 0.35f, 0),
            new[] { SKColors.White.WithAlpha(70), SKColors.Transparent },
            SKShaderTileMode.Clamp);
        using var sheenPaint = new SKPaint { IsAntialias = true, Shader = sheenGrad };
        canvas.DrawPath(path, sheenPaint);

        return EncodePng(surface);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Pill button - 240×56 transparent PNG
    // ──────────────────────────────────────────────────────────────────────────
    private byte[] GenButtonPill(SKColor colorA, SKColor colorB)
    {
        const int W = 240, H = 56;
        float r = H / 2f;
        using var surface = SKSurface.Create(new SKImageInfo(W, H, SKColorType.Rgba8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        var rect = new SKRoundRect(new SKRect(0, 0, W, H), r, r);

        // Outer glow
        using var glowPaint = new SKPaint
        {
            IsAntialias = true,
            Color = colorA.WithAlpha(60),
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Outer, 8)
        };
        canvas.DrawRoundRect(rect, glowPaint);

        // Main fill gradient (top-to-bottom sheen)
        using var mainGrad = SKShader.CreateLinearGradient(
            new SKPoint(0, 0), new SKPoint(0, H),
            new[] { colorB.WithAlpha(240), colorA, new SKColor(
                (byte)(colorA.Red * 0.7f), (byte)(colorA.Green * 0.7f), (byte)(colorA.Blue * 0.7f)) },
            new[] { 0f, 0.45f, 1f },
            SKShaderTileMode.Clamp);
        using var mainPaint = new SKPaint { IsAntialias = true, Shader = mainGrad };
        canvas.DrawRoundRect(rect, mainPaint);

        // Top sheen
        var sheenRect = new SKRoundRect(new SKRect(2, 2, W - 2, H * 0.48f), r - 2, r - 2);
        using var sheenGrad = SKShader.CreateLinearGradient(
            new SKPoint(0, 2), new SKPoint(0, H * 0.48f),
            new[] { SKColors.White.WithAlpha(100), SKColors.Transparent },
            SKShaderTileMode.Clamp);
        using var sheenPaint = new SKPaint { IsAntialias = true, Shader = sheenGrad };
        canvas.DrawRoundRect(sheenRect, sheenPaint);

        return EncodePng(surface);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Panel frame - 600×400 transparent PNG (used as border image)
    // ──────────────────────────────────────────────────────────────────────────
    private byte[] GenPanelFrame()
    {
        const int W = 600, H = 400;
        float barV = 80f;
        float barH = 50f;
        float r  = 48f;
        float ri = 22f;

        using var surface = SKSurface.Create(new SKImageInfo(W, H, SKColorType.Rgba8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        // ── Outer frame path (the LCARS "window" frame) ───────────────────────
        using var outerPath = new SKPath();
        // Top bar
        outerPath.MoveTo(r, 0);
        outerPath.ArcTo(new SKRect(0, 0, r * 2, r * 2), 180, 90, false);
        outerPath.LineTo(W - r, 0);
        outerPath.ArcTo(new SKRect(W - r * 2, 0, W, r * 2), 270, 90, false);
        outerPath.LineTo(W, H - r);
        outerPath.ArcTo(new SKRect(W - r * 2, H - r * 2, W, H), 0, 90, false);
        outerPath.LineTo(r, H);
        outerPath.ArcTo(new SKRect(0, H - r * 2, r * 2, H), 90, 90, false);
        outerPath.Close();

        using var innerPath = new SKPath();
        innerPath.MoveTo(barV + ri, barH);
        innerPath.ArcTo(new SKRect(barV, barH, barV + ri * 2, barH + ri * 2), 270, -90, false);
        innerPath.LineTo(barV, H);
        innerPath.LineTo(W, H);
        innerPath.LineTo(W, barH);
        innerPath.Close();

        // Cut out inner area from outer
        outerPath.AddPath(innerPath, SKPathAddMode.Append);

        // Draw outer frame (orange gradient)
        using var frameGrad = SKShader.CreateLinearGradient(
            new SKPoint(0, 0), new SKPoint(barV + 40, H),
            new[] { Colors.Orange, Colors.Tan, Colors.Lavender, Colors.Purple },
            new[] { 0f, 0.35f, 0.7f, 1f },
            SKShaderTileMode.Clamp);
        using var framePaint = new SKPaint
        {
            IsAntialias = true,
            Shader = frameGrad,
            BlendMode = SKBlendMode.SrcOver
        };

        // Draw the L-frame (left+top bars)
        using var lFramePath = new SKPath();
        lFramePath.MoveTo(0, H);
        lFramePath.LineTo(0, r);
        lFramePath.ArcTo(new SKRect(0, 0, r * 2, r * 2), 180, 90, false);
        lFramePath.LineTo(W, 0);
        lFramePath.LineTo(W, barH);
        lFramePath.LineTo(barV + ri, barH);
        lFramePath.ArcTo(new SKRect(barV, barH - ri * 0, barV + ri * 2, barH + ri * 2), 270, -90, false);
        lFramePath.LineTo(barV, H);
        lFramePath.Close();

        canvas.DrawPath(lFramePath, framePaint);

        // Sheen on top horizontal bar
        using var topSheenGrad = SKShader.CreateLinearGradient(
            new SKPoint(0, 0), new SKPoint(0, barH),
            new[] { SKColors.White.WithAlpha(90), SKColors.Transparent },
            SKShaderTileMode.Clamp);
        using var topSheenPaint = new SKPaint { IsAntialias = true, Shader = topSheenGrad };
        canvas.DrawPath(lFramePath, topSheenPaint);

        return EncodePng(surface);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Sidebar full - 80×800 PNG (the complete sidebar background)
    // Segmented color zones: orange top → tan → purple → blue → purple bottom
    // ──────────────────────────────────────────────────────────────────────────
    private byte[] GenSidebarFull()
    {
        const int W = 80, H = 800;
        using var surface = SKSurface.Create(new SKImageInfo(W, H, SKColorType.Rgba8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        // Segment layout: list of (heightFraction, colorA, colorB)
        var segments = new[]
        {
            (0.18f, Colors.Orange,   Colors.Gold),
            (0.04f, Colors.Black,    Colors.Black),    // gap
            (0.12f, Colors.Tan,      Colors.Peach),
            (0.04f, Colors.Black,    Colors.Black),
            (0.22f, Colors.Lavender, Colors.Purple),
            (0.04f, Colors.Black,    Colors.Black),
            (0.18f, Colors.Blue,     Colors.LightBlue),
            (0.04f, Colors.Black,    Colors.Black),
            (0.14f, Colors.Lavender, Colors.Purple),
        };

        float yOff = 0;
        foreach (var (frac, cA, cB) in segments)
        {
            float segH = H * frac;
            var rect = new SKRect(0, yOff, W, yOff + segH);

            if (cA == Colors.Black)
            {
                // black gap - keep transparent
            }
            else
            {
                DrawBarSegment(canvas, rect, cA, cB, horizontal: false);
            }
            yOff += segH;
        }

        return EncodePng(surface);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Header full - 1200×50 PNG (the complete header bar background)
    // Segmented horizontal: orange left → tan → peach → blue → peach right
    // ──────────────────────────────────────────────────────────────────────────
    private byte[] GenHeaderFull()
    {
        const int W = 1200, H = 50;
        using var surface = SKSurface.Create(new SKImageInfo(W, H, SKColorType.Rgba8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        var segments = new[]
        {
            (0.08f, Colors.Orange,   Colors.Gold),
            (0.02f, Colors.Black,    Colors.Black),
            (0.20f, Colors.Tan,      Colors.Peach),
            (0.02f, Colors.Black,    Colors.Black),
            (0.32f, Colors.Peach,    Colors.Gold),
            (0.02f, Colors.Black,    Colors.Black),
            (0.18f, Colors.Blue,     Colors.LightBlue),
            (0.02f, Colors.Black,    Colors.Black),
            (0.14f, Colors.Lavender, Colors.Purple),
        };

        float xOff = 0;
        foreach (var (frac, cA, cB) in segments)
        {
            float segW = W * frac;
            var rect = new SKRect(xOff, 0, xOff + segW, H);

            if (cA != Colors.Black)
                DrawBarSegment(canvas, rect, cA, cB, horizontal: true);

            xOff += segW;
        }

        return EncodePng(surface);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────────────────
    private static void DrawBarSegment(SKCanvas canvas, SKRect rect, SKColor cA, SKColor cB, bool horizontal)
    {
        SKPoint p0, p1;
        if (horizontal)
        {
            p0 = new SKPoint(rect.Left,  rect.MidY);
            p1 = new SKPoint(rect.Right, rect.MidY);
        }
        else
        {
            p0 = new SKPoint(rect.MidX, rect.Top);
            p1 = new SKPoint(rect.MidX, rect.Bottom);
        }

        // Main gradient
        using var mainGrad = SKShader.CreateLinearGradient(
            horizontal ? new SKPoint(rect.Left, rect.Top) : new SKPoint(rect.Left, rect.Top),
            horizontal ? new SKPoint(rect.Left, rect.Bottom) : new SKPoint(rect.Right, rect.Top),
            new[] { cB.WithAlpha(230), cA, new SKColor(
                (byte)(cA.Red * 0.75f), (byte)(cA.Green * 0.75f), (byte)(cA.Blue * 0.75f), 230) },
            new[] { 0f, 0.42f, 1f },
            SKShaderTileMode.Clamp);

        using var mainPaint = new SKPaint { IsAntialias = true, Shader = mainGrad };
        canvas.DrawRect(rect, mainPaint);

        // Sheen
        SKPoint s0 = horizontal ? new SKPoint(rect.Left, rect.Top) : new SKPoint(rect.Left, rect.Top);
        SKPoint s1 = horizontal ? new SKPoint(rect.Left, rect.Top + (rect.Height * 0.45f))
                                : new SKPoint(rect.Left + (rect.Width * 0.4f), rect.Top);
        using var sheenGrad = SKShader.CreateLinearGradient(s0, s1,
            new[] { SKColors.White.WithAlpha(75), SKColors.Transparent },
            SKShaderTileMode.Clamp);
        using var sheenPaint = new SKPaint { IsAntialias = true, Shader = sheenGrad };
        canvas.DrawRect(rect, sheenPaint);
    }

    private static byte[] EncodePng(SKSurface surface)
    {
        using var img    = surface.Snapshot();
        using var data   = img.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}
