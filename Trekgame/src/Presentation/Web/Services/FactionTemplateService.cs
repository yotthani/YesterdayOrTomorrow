using System.Text.Json;

namespace StarTrekGame.Web.Services;

/// <summary>
/// Service that provides faction UI templates - data-driven component structure definitions.
/// Similar to XAML DataTemplates but for web.
/// </summary>
public class FactionTemplateService
{
    private readonly Dictionary<string, FactionTemplate> _templates;
    private readonly FactionTemplate _defaultTemplate;

    public FactionTemplateService()
    {
        _templates = LoadTemplates();
        _defaultTemplate = _templates["default"];
    }

    public FactionTemplate GetTemplate(string factionId)
    {
        return _templates.TryGetValue(factionId.ToLowerInvariant(), out var template)
            ? template
            : _defaultTemplate;
    }

    public IEnumerable<string> GetAllFactionIds() => _templates.Keys;

    private Dictionary<string, FactionTemplate> LoadTemplates()
    {
        // In production, load from JSON file. For now, define in code.
        return new Dictionary<string, FactionTemplate>
        {
            ["default"] = new FactionTemplate
            {
                Id = "default",
                Name = "Stellaris Default",

                // Button template
                Button = new ButtonTemplate
                {
                    Shape = "rounded",
                    Structure = new[] { "content" },
                    CssClass = "default-btn",
                    BorderRadius = "6px"
                },

                // Panel template
                Panel = new PanelTemplate
                {
                    Shape = "rounded",
                    Structure = new[] { "header", "body" },
                    CssClass = "default-panel",
                    BorderRadius = "8px",
                    Decorations = Array.Empty<DecorationElement>()
                },

                // Sidebar template
                Sidebar = new SidebarTemplate
                {
                    Layout = "vertical-stack",
                    Structure = new[] { "nav-items" },
                    CssClass = "default-sidebar",
                    NavItemTemplate = new NavItemTemplate
                    {
                        Structure = new[] { "icon", "label" },
                        Shape = "rectangle",
                        CssClass = "default-nav-item"
                    }
                },

                // Header template
                Header = new HeaderTemplate
                {
                    Layout = "horizontal",
                    Structure = new[] { "title-block", "resources", "actions" },
                    CssClass = "default-header",
                    Decorations = Array.Empty<DecorationElement>()
                },

                // Colors (fallback)
                Colors = new ColorPalette
                {
                    Primary = "#4a9eff",
                    Secondary = "#00d4ff",
                    Accent = "#ffc844",
                    Background = "#040608",
                    Text = "#d0e4f8"
                },

                // Default layout - header top, sidebar left, context right
                Layout = new LayoutTemplate
                {
                    GridAreas = "\"header header header\" \"sidebar main context\"",
                    GridColumns = "140px 1fr 300px",
                    GridRows = "70px 1fr",
                    Areas = new[] { "header", "sidebar", "main", "context" }
                }
            },

            ["federation"] = new FactionTemplate
            {
                Id = "federation",
                Name = "United Federation of Planets",

                Button = new ButtonTemplate
                {
                    Shape = "rounded",
                    Structure = new[] { "content" },
                    CssClass = "snw-btn",
                    BorderRadius = "8px"
                },

                Panel = new PanelTemplate
                {
                    Shape = "snw-frame",
                    Structure = new[] { "content-area" },
                    CssClass = "snw-panel",
                    BorderRadius = "8px",
                    Decorations = new[]
                    {
                        new DecorationElement { Type = "cyan-border", Position = "all" }
                    }
                },

                Sidebar = new SidebarTemplate
                {
                    Layout = "snw-sidebar",
                    Structure = new[] { "nav-items" },
                    CssClass = "snw-sidebar",
                    NavItemTemplate = new NavItemTemplate
                    {
                        Structure = new[] { "icon", "label" },
                        Shape = "rounded",
                        CssClass = "snw-nav-item"
                    }
                },

                Header = new HeaderTemplate
                {
                    Layout = "snw-header",
                    Structure = new[] { "emblem", "title", "resources", "stardate", "actions" },
                    CssClass = "snw-header",
                    Decorations = new[]
                    {
                        new DecorationElement { Type = "cyan-underline", Position = "bottom" }
                    }
                },

                Colors = new ColorPalette
                {
                    Primary = "#00c8e8",
                    Secondary = "#1265d4",
                    Accent = "#00e070",
                    Background = "#020814",
                    Text = "#e0f4ff"
                },

                // SNW/Prodigy LCARS: Modern flat panels, thin borders, data-grid readouts
                Layout = new LayoutTemplate
                {
                    GridAreas = "\"header header header\" \"sidebar main context\"",
                    GridColumns = "80px 1fr 260px",
                    GridRows = "56px 1fr",
                    Areas = new[] { "header", "sidebar", "main", "context" },
                    Features = new[] { "snw-lcars", "data-readouts" }
                }
            },

            ["klingon"] = new FactionTemplate
            {
                Id = "klingon",
                Name = "Klingon Empire",

                Button = new ButtonTemplate
                {
                    Shape = "angular",
                    Structure = new[] { "corner-accent-tl", "content", "corner-accent-br" },
                    CssClass = "klingon-btn",
                    ClipPath = "polygon(8px 0, 100% 0, calc(100% - 8px) 100%, 0 100%)"
                },

                Panel = new PanelTemplate
                {
                    Shape = "beveled",
                    Structure = new[] { "corner-accents", "rivets", "header", "body" },
                    CssClass = "klingon-panel",
                    ClipPath = "polygon(15px 0, calc(100% - 15px) 0, 100% 15px, 100% calc(100% - 15px), calc(100% - 15px) 100%, 15px 100%, 0 calc(100% - 15px), 0 15px)",
                    Decorations = new[]
                    {
                        new DecorationElement { Type = "triangle", Position = "corner-tl", Color = "primary" },
                        new DecorationElement { Type = "triangle", Position = "corner-br", Color = "primary" },
                        new DecorationElement { Type = "rivet", Position = "corners" },
                        new DecorationElement { Type = "spike", Position = "header-sides" }
                    }
                },

                Sidebar = new SidebarTemplate
                {
                    Layout = "angular-stack",
                    Structure = new[] { "emblem", "metal-frame", "nav-blades", "spike-bottom" },
                    CssClass = "klingon-sidebar",
                    NavItemTemplate = new NavItemTemplate
                    {
                        Structure = new[] { "blade-icon", "blade-text", "blade-edge" },
                        Shape = "blade",
                        CssClass = "klingon-nav-blade"
                    }
                },

                Header = new HeaderTemplate
                {
                    Layout = "warrior-bar",
                    Structure = new[] { "blade-left", "trefoil", "title", "resources", "actions", "blade-right", "spike" },
                    CssClass = "klingon-header",
                    Decorations = new[]
                    {
                        new DecorationElement { Type = "blade", Position = "sides" },
                        new DecorationElement { Type = "trefoil-emblem", Position = "left" },
                        new DecorationElement { Type = "spike", Position = "bottom-center" }
                    }
                },

                Colors = new ColorPalette
                {
                    Primary = "#CC0000",
                    Secondary = "#FF4444",
                    Accent = "#FFAA00",
                    Background = "#0A0505",
                    Text = "#FFCCAA"
                },

                // Klingon: Edge-hugging nav with triangular buttons along screen edges
                Layout = new LayoutTemplate
                {
                    GridAreas = "\"main\"",
                    GridColumns = "1fr",
                    GridRows = "1fr",
                    Areas = new[] { "main" },
                    Features = new[] { "edge-nav", "edge-status", "edge-teeth", "corner-accents", "top-title", "central-emblem" }
                }
            },

            ["romulan"] = new FactionTemplate
            {
                Id = "romulan",
                Name = "Romulan Star Empire",

                Button = new ButtonTemplate
                {
                    Shape = "lens",
                    Structure = new[] { "wing-left", "content", "wing-right" },
                    CssClass = "romulan-btn",
                    BorderRadius = "4px 20px 20px 4px"
                },

                Panel = new PanelTemplate
                {
                    Shape = "elegant",
                    Structure = new[] { "wing-top", "bronze-header", "body", "curve-bottom" },
                    CssClass = "romulan-panel",
                    BorderRadius = "2px",
                    Decorations = new[]
                    {
                        new DecorationElement { Type = "v-motif", Position = "top-center" },
                        new DecorationElement { Type = "bronze-bar", Position = "header" },
                        new DecorationElement { Type = "accent-line", Position = "left", Color = "primary" }
                    }
                },

                Sidebar = new SidebarTemplate
                {
                    Layout = "curved-elegant",
                    Structure = new[] { "bird-emblem", "curved-frame", "nav-lenses", "bronze-bottom" },
                    CssClass = "romulan-sidebar",
                    NavItemTemplate = new NavItemTemplate
                    {
                        Structure = new[] { "lens-glow", "icon", "label" },
                        Shape = "lens",
                        CssClass = "romulan-nav-lens"
                    }
                },

                Header = new HeaderTemplate
                {
                    Layout = "elegant-bar",
                    Structure = new[] { "wing-left", "bird-icon", "title", "divider", "resources", "actions", "wing-right", "v-accent" },
                    CssClass = "romulan-header",
                    Decorations = new[]
                    {
                        new DecorationElement { Type = "wing", Position = "sides" },
                        new DecorationElement { Type = "v-motif", Position = "bottom-center" },
                        new DecorationElement { Type = "bronze-divider", Position = "center" }
                    }
                },

                Colors = new ColorPalette
                {
                    Primary = "#00AA44",
                    Secondary = "#44FF88",
                    Accent = "#D4AF37",
                    Background = "#020A05",
                    Text = "#CCFFDD"
                },

                // Romulan: Elegant V-layout, context on right only (no left sidebar)
                Layout = new LayoutTemplate
                {
                    GridAreas = "\"header header\" \"main context\"",
                    GridColumns = "1fr 320px",
                    GridRows = "65px 1fr",
                    Areas = new[] { "header", "main", "context" },
                    Features = new[] { "v-shaped-accents", "elegant-curves", "bronze-accents" }
                }
            },

            ["borg"] = new FactionTemplate
            {
                Id = "borg",
                Name = "Borg Collective",

                Button = new ButtonTemplate
                {
                    Shape = "hexagonal",
                    Structure = new[] { "hex-frame", "content" },
                    CssClass = "borg-btn",
                    ClipPath = "polygon(15% 0%, 85% 0%, 100% 50%, 85% 100%, 15% 100%, 0% 50%)"
                },

                Panel = new PanelTemplate
                {
                    Shape = "grid",
                    Structure = new[] { "hex-corners", "designation-header", "scanlines", "body", "data-stream" },
                    CssClass = "borg-panel",
                    BorderRadius = "0",
                    Decorations = new[]
                    {
                        new DecorationElement { Type = "hex", Position = "corners" },
                        new DecorationElement { Type = "scanlines", Position = "overlay" },
                        new DecorationElement { Type = "grid-pattern", Position = "background" },
                        new DecorationElement { Type = "data-stream", Position = "bottom" },
                        new DecorationElement { Type = "node-indicator", Position = "header-right" }
                    }
                },

                Sidebar = new SidebarTemplate
                {
                    Layout = "hex-grid",
                    Structure = new[] { "collective-node", "hex-grid", "nav-hexes", "data-conduit" },
                    CssClass = "borg-sidebar",
                    NavItemTemplate = new NavItemTemplate
                    {
                        Structure = new[] { "hex-outer", "hex-inner", "designation", "label", "pulse" },
                        Shape = "hexagon",
                        CssClass = "borg-nav-hex"
                    }
                },

                Header = new HeaderTemplate
                {
                    Layout = "collective-bar",
                    Structure = new[] { "grid-overlay", "designation", "subroutine-text", "data-readout", "action-node", "status-nodes" },
                    CssClass = "borg-header",
                    Decorations = new[]
                    {
                        new DecorationElement { Type = "grid-pattern", Position = "background" },
                        new DecorationElement { Type = "node", Position = "corners" },
                        new DecorationElement { Type = "glow-line", Position = "bottom" }
                    }
                },

                Colors = new ColorPalette
                {
                    Primary = "#00FF00",
                    Secondary = "#004400",
                    Accent = "#00FF00",
                    Background = "#000500",
                    Text = "#00FF00"
                },

                // Borg: Grid-based layout with panels everywhere, data flows
                Layout = new LayoutTemplate
                {
                    GridAreas = "\"header header header\" \"sidebar-l main sidebar-r\" \"footer footer footer\"",
                    GridColumns = "100px 1fr 100px",
                    GridRows = "55px 1fr 40px",
                    Areas = new[] { "header", "sidebar-l", "main", "sidebar-r", "footer" },
                    Features = new[] { "hex-grid-overlay", "data-streams", "scanlines", "symmetric" }
                }
            },

            ["cardassian"] = new FactionTemplate
            {
                Id = "cardassian",
                Name = "Cardassian Union",

                Button = new ButtonTemplate
                {
                    Shape = "notched",
                    Structure = new[] { "notch-tl", "content", "notch-br" },
                    CssClass = "cardassian-btn",
                    ClipPath = "polygon(6px 0, calc(100% - 6px) 0, 100% 6px, 100% calc(100% - 6px), calc(100% - 6px) 100%, 6px 100%, 0 calc(100% - 6px), 0 6px)"
                },

                Panel = new PanelTemplate
                {
                    Shape = "geometric",
                    Structure = new[] { "notch-corners", "status-header", "body", "scan-line" },
                    CssClass = "cardassian-panel",
                    ClipPath = "polygon(8px 0, calc(100% - 8px) 0, 100% 8px, 100% calc(100% - 8px), calc(100% - 8px) 100%, 8px 100%, 0 calc(100% - 8px), 0 8px)",
                    Decorations = new[]
                    {
                        new DecorationElement { Type = "notch", Position = "corners" },
                        new DecorationElement { Type = "status-light", Position = "header-left" },
                        new DecorationElement { Type = "hierarchy-badge", Position = "header-right" },
                        new DecorationElement { Type = "scan-line", Position = "overlay" }
                    }
                },

                Sidebar = new SidebarTemplate
                {
                    Layout = "regimented",
                    Structure = new[] { "emblem", "status-bar", "nav-blocks", "scan-line" },
                    CssClass = "cardassian-sidebar",
                    NavItemTemplate = new NavItemTemplate
                    {
                        Structure = new[] { "indicator", "icon", "label", "notch" },
                        Shape = "notched-block",
                        CssClass = "cardassian-nav-block"
                    }
                },

                Header = new HeaderTemplate
                {
                    Layout = "command-bar",
                    Structure = new[] { "status-light", "emblem", "title", "resources", "actions", "hierarchy-indicator", "surveillance-line" },
                    CssClass = "cardassian-header",
                    Decorations = new[]
                    {
                        new DecorationElement { Type = "status-light", Position = "left" },
                        new DecorationElement { Type = "surveillance-line", Position = "bottom" },
                        new DecorationElement { Type = "hierarchy-indicator", Position = "right" }
                    }
                },

                Colors = new ColorPalette
                {
                    Primary = "#FF8800",
                    Secondary = "#4A3520",
                    Accent = "#FFCC00",
                    Background = "#0A0500",
                    Text = "#FFDDAA"
                },

                // Cardassian: Surveillance style, info bars on both sides
                Layout = new LayoutTemplate
                {
                    GridAreas = "\"header header header\" \"info-l main info-r\" \"status status status\"",
                    GridColumns = "180px 1fr 180px",
                    GridRows = "60px 1fr 35px",
                    Areas = new[] { "header", "info-l", "main", "info-r", "status" },
                    Features = new[] { "status-indicators", "surveillance-lines", "hierarchy-display" }
                }
            },

            ["dominion"] = new FactionTemplate
            {
                Id = "dominion",
                Name = "The Dominion",

                Button = new ButtonTemplate
                {
                    Shape = "imperial",
                    Structure = new[] { "diamond-accent", "content" },
                    CssClass = "dominion-btn",
                    BorderRadius = "8px"
                },

                Panel = new PanelTemplate
                {
                    Shape = "imperial",
                    Structure = new[] { "diamond-top", "founder-header", "body", "shimmer-overlay" },
                    CssClass = "dominion-panel",
                    BorderRadius = "8px",
                    Decorations = new[]
                    {
                        new DecorationElement { Type = "diamond", Position = "top-center" },
                        new DecorationElement { Type = "founder-symbol", Position = "header-left" },
                        new DecorationElement { Type = "shimmer", Position = "overlay" },
                        new DecorationElement { Type = "gold-line", Position = "bottom" }
                    }
                },

                Sidebar = new SidebarTemplate
                {
                    Layout = "imperial-column",
                    Structure = new[] { "founder-emblem", "diamond-header", "nav-gems", "ketracel-indicator" },
                    CssClass = "dominion-sidebar",
                    NavItemTemplate = new NavItemTemplate
                    {
                        Structure = new[] { "gem-facet", "icon", "label", "shimmer" },
                        Shape = "gem",
                        CssClass = "dominion-nav-gem"
                    }
                },

                Header = new HeaderTemplate
                {
                    Layout = "divine-bar",
                    Structure = new[] { "diamond-left", "founder-icon", "title", "resources", "actions", "diamond-right", "golden-line" },
                    CssClass = "dominion-header",
                    Decorations = new[]
                    {
                        new DecorationElement { Type = "diamond", Position = "sides" },
                        new DecorationElement { Type = "golden-line", Position = "bottom" },
                        new DecorationElement { Type = "shimmer", Position = "overlay" }
                    }
                },

                Colors = new ColorPalette
                {
                    Primary = "#9933FF",
                    Secondary = "#2A1A4A",
                    Accent = "#FFD700",
                    Background = "#050010",
                    Text = "#EEDDFF"
                },

                // Dominion: Hierarchical top-down layout, imperial command style
                Layout = new LayoutTemplate
                {
                    GridAreas = "\"header\" \"command\" \"main\" \"subjects\"",
                    GridColumns = "1fr",
                    GridRows = "70px 50px 1fr 45px",
                    Areas = new[] { "header", "command", "main", "subjects" },
                    Features = new[] { "hierarchical", "golden-accents", "divine-shimmer", "centered-content" }
                }
            },

            ["ferengi"] = new FactionTemplate
            {
                Id = "ferengi",
                Name = "Ferengi Alliance",

                Button = new ButtonTemplate
                {
                    Shape = "hexagonal",
                    Structure = new[] { "hex-frame", "content", "profit-indicator" },
                    CssClass = "ferengi-btn",
                    ClipPath = "polygon(20% 0%, 80% 0%, 100% 50%, 80% 100%, 20% 100%, 0% 50%)"
                },

                Panel = new PanelTemplate
                {
                    Shape = "business",
                    Structure = new[] { "profit-header", "hexagon-corners", "body", "transaction-bar" },
                    CssClass = "ferengi-panel",
                    BorderRadius = "4px",
                    Decorations = new[]
                    {
                        new DecorationElement { Type = "hexagon", Position = "corners" },
                        new DecorationElement { Type = "pie-chart", Position = "header-right" },
                        new DecorationElement { Type = "profit-line", Position = "bottom" },
                        new DecorationElement { Type = "latinum-glow", Position = "center" }
                    }
                },

                Sidebar = new SidebarTemplate
                {
                    Layout = "data-column",
                    Structure = new[] { "profit-display", "circular-frame", "nav-hexes", "transaction-log" },
                    CssClass = "ferengi-sidebar",
                    NavItemTemplate = new NavItemTemplate
                    {
                        Structure = new[] { "hex-icon", "label", "profit-value" },
                        Shape = "hexagon",
                        CssClass = "ferengi-nav-hex"
                    }
                },

                Header = new HeaderTemplate
                {
                    Layout = "business-bar",
                    Structure = new[] { "circular-indicator", "title", "profit-display", "resources", "actions", "pie-chart" },
                    CssClass = "ferengi-header",
                    Decorations = new[]
                    {
                        new DecorationElement { Type = "circular-frame", Position = "left" },
                        new DecorationElement { Type = "pie-chart", Position = "right" },
                        new DecorationElement { Type = "cyan-pink-bar", Position = "bottom" }
                    }
                },

                Colors = new ColorPalette
                {
                    Primary = "#00CCFF",
                    Secondary = "#FF66CC",
                    Accent = "#FFD700",
                    Background = "#0a0812",
                    Text = "#CCFFFF"
                },

                // Ferengi: Business terminal style with data displays
                Layout = new LayoutTemplate
                {
                    GridAreas = "\"header header header\" \"sidebar main context\" \"footer footer footer\"",
                    GridColumns = "140px 1fr 200px",
                    GridRows = "65px 1fr 40px",
                    Areas = new[] { "header", "sidebar", "main", "context", "footer" },
                    Features = new[] { "hex-elements", "circular-displays", "profit-indicators", "data-grid" }
                }
            },

            ["bajoran"] = new FactionTemplate
            {
                Id = "bajoran",
                Name = "Bajoran Republic",

                Button = new ButtonTemplate
                {
                    Shape = "rounded",
                    Structure = new[] { "orb-accent", "content" },
                    CssClass = "bajoran-btn",
                    BorderRadius = "4px"
                },

                Panel = new PanelTemplate
                {
                    Shape = "spiritual",
                    Structure = new[] { "orb-header", "wing-corners", "body", "connection-bar" },
                    CssClass = "bajoran-panel",
                    BorderRadius = "4px",
                    Decorations = new[]
                    {
                        new DecorationElement { Type = "orb", Position = "header-center" },
                        new DecorationElement { Type = "wing", Position = "corners" },
                        new DecorationElement { Type = "ring-display", Position = "sidebar" },
                        new DecorationElement { Type = "sacred-glow", Position = "overlay" }
                    }
                },

                Sidebar = new SidebarTemplate
                {
                    Layout = "temple-column",
                    Structure = new[] { "orb-display", "arch-frame", "nav-rings", "wing-accent" },
                    CssClass = "bajoran-sidebar",
                    NavItemTemplate = new NavItemTemplate
                    {
                        Structure = new[] { "ring-icon", "label", "status-orb" },
                        Shape = "rounded",
                        CssClass = "bajoran-nav-ring"
                    }
                },

                Header = new HeaderTemplate
                {
                    Layout = "temple-bar",
                    Structure = new[] { "wing-left", "orb-emblem", "title", "resources", "actions", "wing-right" },
                    CssClass = "bajoran-header",
                    Decorations = new[]
                    {
                        new DecorationElement { Type = "orb", Position = "center" },
                        new DecorationElement { Type = "wing", Position = "sides" },
                        new DecorationElement { Type = "cyan-orange-bar", Position = "bottom" }
                    }
                },

                Colors = new ColorPalette
                {
                    Primary = "#00BBCC",
                    Secondary = "#FF8833",
                    Accent = "#DDAA33",
                    Background = "#040808",
                    Text = "#CCEEEE"
                },

                // Bajoran: Spiritual temple style with circular orb displays
                Layout = new LayoutTemplate
                {
                    GridAreas = "\"header header header\" \"sidebar main context\" \"footer footer footer\"",
                    GridColumns = "130px 1fr 180px",
                    GridRows = "60px 1fr 35px",
                    Areas = new[] { "header", "sidebar", "main", "context", "footer" },
                    Features = new[] { "orb-displays", "circular-rings", "wing-accents", "sacred-geometry" }
                }
            },

            // ================================================================
            // ADDITIONAL FACTIONS - Unique UI Structures
            // ================================================================

            ["tholian"] = new FactionTemplate
            {
                Id = "tholian",
                Name = "Tholian Assembly",

                Button = new ButtonTemplate
                {
                    Shape = "crystalline",
                    Structure = new[] { "crystal-facet", "content" },
                    CssClass = "tholian-btn",
                    ClipPath = "polygon(15% 0%, 85% 0%, 100% 30%, 100% 70%, 85% 100%, 15% 100%, 0% 70%, 0% 30%)"
                },

                Panel = new PanelTemplate
                {
                    Shape = "web-frame",
                    Structure = new[] { "crystal-corners", "web-pattern", "body", "heat-indicator" },
                    CssClass = "tholian-panel",
                    ClipPath = "polygon(10px 0, calc(100% - 10px) 0, 100% 10px, 100% calc(100% - 10px), calc(100% - 10px) 100%, 10px 100%, 0 calc(100% - 10px), 0 10px)",
                    Decorations = new[]
                    {
                        new DecorationElement { Type = "crystal", Position = "corners" },
                        new DecorationElement { Type = "web-lines", Position = "overlay" },
                        new DecorationElement { Type = "heat-glow", Position = "background" },
                        new DecorationElement { Type = "facet-shine", Position = "header" }
                    }
                },

                Sidebar = new SidebarTemplate
                {
                    Layout = "crystalline-column",
                    Structure = new[] { "crystal-emblem", "web-frame", "nav-crystals", "heat-bar" },
                    CssClass = "tholian-sidebar",
                    NavItemTemplate = new NavItemTemplate
                    {
                        Structure = new[] { "crystal-icon", "label", "heat-level" },
                        Shape = "crystal",
                        CssClass = "tholian-nav-crystal"
                    }
                },

                Header = new HeaderTemplate
                {
                    Layout = "web-bar",
                    Structure = new[] { "crystal-left", "web-pattern", "title", "resources", "actions", "crystal-right", "heat-line" },
                    CssClass = "tholian-header",
                    Decorations = new[]
                    {
                        new DecorationElement { Type = "crystal", Position = "sides" },
                        new DecorationElement { Type = "web-lines", Position = "background" },
                        new DecorationElement { Type = "heat-bar", Position = "bottom" }
                    }
                },

                Colors = new ColorPalette
                {
                    Primary = "#FF8800",
                    Secondary = "#FFAA33",
                    Accent = "#FFDD00",
                    Background = "#0a0500",
                    Text = "#FFDDAA"
                },

                // Tholian: Crystalline web pattern, heat-based indicators
                Layout = new LayoutTemplate
                {
                    GridAreas = "\"header header header\" \"web-l main web-r\" \"heat heat heat\"",
                    GridColumns = "120px 1fr 120px",
                    GridRows = "55px 1fr 30px",
                    Areas = new[] { "header", "web-l", "main", "web-r", "heat" },
                    Features = new[] { "crystalline-shapes", "web-patterns", "heat-indicators", "faceted-panels" }
                }
            },

            // ================================================================
            // GORN HEGEMONY - Reptilian Predator Hive (Star Trek: SNW)
            // ================================================================
            // CONCEPT: "What if Velociraptors survived and formed a civilization?"
            // The Gorn are intelligent evolved dinosaurs - apex REPTILIAN predators
            // with bio-organic hive technology.
            // Ships are GROWN not built. Hunters: Bronze star-shaped craft.
            // Mothership: Massive organic bio-mechanical carrier.
            // UI Colors: Orange/Amber (holographics, targeting, egg chambers)
            //            Green (shields, energy fields)
            // Breeding: Xenomorph-style parasitic reproduction cycle
            // ================================================================
            ["gorn"] = new FactionTemplate
            {
                Id = "gorn",
                Name = "Gorn Hegemony",

                Button = new ButtonTemplate
                {
                    Shape = "organic-rounded",
                    Structure = new[] { "bio-glow", "content", "targeting-pip" },
                    CssClass = "gorn-btn",
                    BorderRadius = "20px"
                },

                Panel = new PanelTemplate
                {
                    Shape = "bio-organic",
                    Structure = new[] { "rib-pattern", "holo-strip", "body", "egg-indicator", "hive-bar" },
                    CssClass = "gorn-panel",
                    BorderRadius = "12px 12px 8px 8px",
                    Decorations = new[]
                    {
                        new DecorationElement { Type = "rib-structure", Position = "sides" },
                        new DecorationElement { Type = "orange-holographic", Position = "top" },
                        new DecorationElement { Type = "egg-chamber-glow", Position = "header-right" },
                        new DecorationElement { Type = "targeting-circle", Position = "overlay" },
                        new DecorationElement { Type = "organic-veins", Position = "background" }
                    }
                },

                Sidebar = new SidebarTemplate
                {
                    Layout = "hive-column",
                    Structure = new[] { "hegemony-emblem", "organic-frame", "nav-hunters", "breeding-status", "shield-bar" },
                    CssClass = "gorn-sidebar",
                    NavItemTemplate = new NavItemTemplate
                    {
                        Structure = new[] { "targeting-icon", "label", "hive-signal" },
                        Shape = "organic-rounded",
                        CssClass = "gorn-nav-hunter"
                    }
                },

                Header = new HeaderTemplate
                {
                    Layout = "hive-command-bar",
                    Structure = new[] { "rib-left", "predator-eye", "title", "resources", "actions", "rib-right", "holo-line" },
                    CssClass = "gorn-header",
                    Decorations = new[]
                    {
                        new DecorationElement { Type = "organic-ribs", Position = "sides" },
                        new DecorationElement { Type = "orange-holographic", Position = "bottom" },
                        new DecorationElement { Type = "egg-indicators", Position = "corners" }
                    }
                },

                Colors = new ColorPalette
                {
                    Primary = "#FF6622",      // Orange holographic (primary UI)
                    Secondary = "#FFAA33",    // Amber accent
                    Accent = "#44DD44",       // Green shields/energy
                    Background = "#050807",   // Dark organic teal
                    Text = "#DDCCAA"          // Warm text
                },

                // Gorn: Bio-organic hive interface with hunting/breeding displays
                Layout = new LayoutTemplate
                {
                    GridAreas = "\"header header header\" \"hive main hunt\" \"breeding breeding breeding\"",
                    GridColumns = "160px 1fr 160px",
                    GridRows = "60px 1fr 40px",
                    Areas = new[] { "header", "hive", "main", "hunt", "breeding" },
                    Features = new[] { "bio-organic-panels", "orange-holographics", "green-shields", "egg-chambers", "targeting-circles", "rib-structures" }
                }
            },

            // ================================================================
            // BREEN CONFEDERACY - Discovery Ship Interior Style
            // ================================================================
            // Gold/Amber dominant with tall light pillars
            // Cyan/Teal accent strips on floor
            // Angular industrial design, mysterious
            // ================================================================
            ["breen"] = new FactionTemplate
            {
                Id = "breen",
                Name = "Breen Confederacy",

                Button = new ButtonTemplate
                {
                    Shape = "angular-industrial",
                    Structure = new[] { "gold-edge", "content", "cyan-accent" },
                    CssClass = "breen-btn",
                    BorderRadius = "0",
                    ClipPath = "polygon(8px 0, 100% 0, 100% calc(100% - 8px), calc(100% - 8px) 100%, 0 100%, 0 8px)"
                },

                Panel = new PanelTemplate
                {
                    Shape = "industrial-angular",
                    Structure = new[] { "light-pillar-left", "gold-header", "body", "cyan-floor-strip", "light-pillar-right" },
                    CssClass = "breen-panel",
                    BorderRadius = "0",
                    ClipPath = "polygon(12px 0, calc(100% - 12px) 0, 100% 12px, 100% calc(100% - 12px), calc(100% - 12px) 100%, 12px 100%, 0 calc(100% - 12px), 0 12px)",
                    Decorations = new[]
                    {
                        new DecorationElement { Type = "gold-light-pillar", Position = "sides" },
                        new DecorationElement { Type = "amber-glow", Position = "top" },
                        new DecorationElement { Type = "cyan-floor-strip", Position = "bottom" },
                        new DecorationElement { Type = "industrial-corner", Position = "corners" }
                    }
                },

                Sidebar = new SidebarTemplate
                {
                    Layout = "industrial-column",
                    Structure = new[] { "helmet-emblem", "gold-frame", "nav-angular", "cyan-accent-bar" },
                    CssClass = "breen-sidebar",
                    NavItemTemplate = new NavItemTemplate
                    {
                        Structure = new[] { "angular-icon", "label", "status-pip" },
                        Shape = "angular",
                        CssClass = "breen-nav-angular"
                    }
                },

                Header = new HeaderTemplate
                {
                    Layout = "industrial-bar",
                    Structure = new[] { "pillar-left", "helmet-icon", "title", "resources", "actions", "pillar-right", "cyan-line" },
                    CssClass = "breen-header",
                    Decorations = new[]
                    {
                        new DecorationElement { Type = "gold-pillar", Position = "sides" },
                        new DecorationElement { Type = "amber-glow", Position = "background" },
                        new DecorationElement { Type = "cyan-strip", Position = "bottom" }
                    }
                },

                Colors = new ColorPalette
                {
                    Primary = "#FFAA33",      // Gold/Amber (light pillars)
                    Secondary = "#FFCC66",    // Lighter amber
                    Accent = "#00CCCC",       // Cyan/Teal (floor strips)
                    Background = "#08080a",   // Near black
                    Text = "#FFEEDD"          // Warm white
                },

                // Breen: Industrial angular style with gold pillars and cyan accents
                Layout = new LayoutTemplate
                {
                    GridAreas = "\"header header\" \"main context\"",
                    GridColumns = "1fr 220px",
                    GridRows = "55px 1fr",
                    Areas = new[] { "header", "main", "context" },
                    Features = new[] { "gold-light-pillars", "cyan-floor-strips", "angular-corners", "industrial-aesthetic", "enigmatic-display" }
                }
            },

            ["orion"] = new FactionTemplate
            {
                Id = "orion",
                Name = "Orion Syndicate",

                Button = new ButtonTemplate
                {
                    Shape = "deal",
                    Structure = new[] { "gold-trim", "content", "credit-badge" },
                    CssClass = "orion-btn",
                    BorderRadius = "8px"
                },

                Panel = new PanelTemplate
                {
                    Shape = "syndicate",
                    Structure = new[] { "gold-border", "shadow-corners", "body", "deal-bar" },
                    CssClass = "orion-panel",
                    BorderRadius = "10px",
                    Decorations = new[]
                    {
                        new DecorationElement { Type = "gold-trim", Position = "border" },
                        new DecorationElement { Type = "shadow", Position = "overlay" },
                        new DecorationElement { Type = "credit-counter", Position = "header-right" },
                        new DecorationElement { Type = "chain-link", Position = "corners" }
                    }
                },

                Sidebar = new SidebarTemplate
                {
                    Layout = "syndicate-column",
                    Structure = new[] { "syndicate-emblem", "gold-frame", "nav-deals", "profit-ticker" },
                    CssClass = "orion-sidebar",
                    NavItemTemplate = new NavItemTemplate
                    {
                        Structure = new[] { "deal-icon", "label", "credit-value" },
                        Shape = "deal",
                        CssClass = "orion-nav-deal"
                    }
                },

                Header = new HeaderTemplate
                {
                    Layout = "syndicate-bar",
                    Structure = new[] { "chain-left", "skull-emblem", "title", "resources", "actions", "chain-right", "deal-ticker" },
                    CssClass = "orion-header",
                    Decorations = new[]
                    {
                        new DecorationElement { Type = "chain-link", Position = "sides" },
                        new DecorationElement { Type = "gold-line", Position = "bottom" },
                        new DecorationElement { Type = "shadow-overlay", Position = "background" }
                    }
                },

                Colors = new ColorPalette
                {
                    Primary = "#33AA66",
                    Secondary = "#55CC88",
                    Accent = "#FFD700",
                    Background = "#050a08",
                    Text = "#CCFFDD"
                },

                // Orion: Criminal syndicate style with gold accents and shadows
                Layout = new LayoutTemplate
                {
                    GridAreas = "\"header header header\" \"deals main contacts\" \"ticker ticker ticker\"",
                    GridColumns = "160px 1fr 160px",
                    GridRows = "60px 1fr 30px",
                    Areas = new[] { "header", "deals", "main", "contacts", "ticker" },
                    Features = new[] { "gold-accents", "shadow-effects", "criminal-aesthetic", "credit-displays" }
                }
            },

            // Delta Quadrant Factions

            ["kazon"] = new FactionTemplate
            {
                Id = "kazon",
                Name = "Kazon Sects",

                Button = new ButtonTemplate
                {
                    Shape = "tribal",
                    Structure = new[] { "spike-left", "content", "spike-right" },
                    CssClass = "kazon-btn",
                    ClipPath = "polygon(5px 0, calc(100% - 5px) 0, 100% 50%, calc(100% - 5px) 100%, 5px 100%, 0 50%)"
                },

                Panel = new PanelTemplate
                {
                    Shape = "tribal",
                    Structure = new[] { "spike-border", "sect-header", "body", "territory-bar" },
                    CssClass = "kazon-panel",
                    BorderRadius = "0",
                    Decorations = new[]
                    {
                        new DecorationElement { Type = "spike", Position = "corners" },
                        new DecorationElement { Type = "tribal-pattern", Position = "border" },
                        new DecorationElement { Type = "sect-badge", Position = "header-left" },
                        new DecorationElement { Type = "territory-glow", Position = "background" }
                    }
                },

                Sidebar = new SidebarTemplate
                {
                    Layout = "tribal-column",
                    Structure = new[] { "sect-emblem", "spike-frame", "nav-tribal", "resource-bar" },
                    CssClass = "kazon-sidebar",
                    NavItemTemplate = new NavItemTemplate
                    {
                        Structure = new[] { "tribal-icon", "label", "sect-mark" },
                        Shape = "spiked",
                        CssClass = "kazon-nav-tribal"
                    }
                },

                Header = new HeaderTemplate
                {
                    Layout = "warlord-bar",
                    Structure = new[] { "spike-left", "sect-icon", "title", "resources", "actions", "spike-right", "territory-line" },
                    CssClass = "kazon-header",
                    Decorations = new[]
                    {
                        new DecorationElement { Type = "spike", Position = "sides" },
                        new DecorationElement { Type = "tribal-pattern", Position = "background" },
                        new DecorationElement { Type = "territory-bar", Position = "bottom" }
                    }
                },

                Colors = new ColorPalette
                {
                    Primary = "#CC6633",
                    Secondary = "#DD8844",
                    Accent = "#FFAA55",
                    Background = "#0a0505",
                    Text = "#FFDDCC"
                },

                // Kazon: Tribal aggressive style with spikes
                Layout = new LayoutTemplate
                {
                    GridAreas = "\"header header\" \"sect main\"",
                    GridColumns = "140px 1fr",
                    GridRows = "65px 1fr",
                    Areas = new[] { "header", "sect", "main" },
                    Features = new[] { "spike-elements", "tribal-patterns", "territorial-displays", "aggressive-aesthetic" }
                }
            },

            ["hirogen"] = new FactionTemplate
            {
                Id = "hirogen",
                Name = "Hirogen Hunters",

                Button = new ButtonTemplate
                {
                    Shape = "hunter",
                    Structure = new[] { "sensor-pip", "content" },
                    CssClass = "hirogen-btn",
                    BorderRadius = "2px"
                },

                Panel = new PanelTemplate
                {
                    Shape = "tracking",
                    Structure = new[] { "sensor-border", "tracking-header", "body", "trophy-bar" },
                    CssClass = "hirogen-panel",
                    BorderRadius = "2px",
                    Decorations = new[]
                    {
                        new DecorationElement { Type = "sensor-pip", Position = "corners" },
                        new DecorationElement { Type = "tracking-grid", Position = "overlay" },
                        new DecorationElement { Type = "trophy-count", Position = "header-right" },
                        new DecorationElement { Type = "hunt-status", Position = "sidebar" }
                    }
                },

                Sidebar = new SidebarTemplate
                {
                    Layout = "hunting-column",
                    Structure = new[] { "alpha-emblem", "sensor-frame", "nav-tracking", "trophy-display" },
                    CssClass = "hirogen-sidebar",
                    NavItemTemplate = new NavItemTemplate
                    {
                        Structure = new[] { "sensor-icon", "label", "hunt-status" },
                        Shape = "tracking",
                        CssClass = "hirogen-nav-track"
                    }
                },

                Header = new HeaderTemplate
                {
                    Layout = "alpha-bar",
                    Structure = new[] { "sensor-left", "alpha-icon", "title", "resources", "actions", "sensor-right", "tracking-line" },
                    CssClass = "hirogen-header",
                    Decorations = new[]
                    {
                        new DecorationElement { Type = "sensor-pip", Position = "sides" },
                        new DecorationElement { Type = "tracking-grid", Position = "background" },
                        new DecorationElement { Type = "hunt-bar", Position = "bottom" }
                    }
                },

                Colors = new ColorPalette
                {
                    Primary = "#557744",
                    Secondary = "#779966",
                    Accent = "#AACC88",
                    Background = "#030503",
                    Text = "#BBDDAA"
                },

                // Hirogen: Hunter tracking interface, trophy-focused
                Layout = new LayoutTemplate
                {
                    GridAreas = "\"header header header\" \"tracking main trophies\"",
                    GridColumns = "100px 1fr 140px",
                    GridRows = "50px 1fr",
                    Areas = new[] { "header", "tracking", "main", "trophies" },
                    Features = new[] { "tracking-displays", "sensor-patterns", "trophy-system", "hunting-aesthetic" }
                }
            }
        };
    }
}

#region Template Data Models

public class FactionTemplate
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public ButtonTemplate Button { get; set; } = new();
    public PanelTemplate Panel { get; set; } = new();
    public SidebarTemplate Sidebar { get; set; } = new();
    public HeaderTemplate Header { get; set; } = new();
    public ColorPalette Colors { get; set; } = new();

    /// <summary>
    /// Defines the overall page layout structure.
    /// Header is always at top, but sidebars and content areas can vary.
    /// </summary>
    public LayoutTemplate Layout { get; set; } = new();
}

/// <summary>
/// Defines the overall page layout - where sidebars, content, and panels go.
/// Header always stays at top, but everything else can be rearranged per faction.
/// </summary>
public class LayoutTemplate
{
    /// <summary>
    /// CSS Grid template areas string. Header area is always "header" at top.
    /// Examples:
    /// - "header header header" / "sidebar main context" (default)
    /// - "header header" / "main sidebar" (right sidebar only)
    /// - "header" / "main" / "footer" (no sidebars, bottom bar)
    /// </summary>
    public string GridAreas { get; set; } = "\"header header header\" \"sidebar main context\"";

    /// <summary>
    /// CSS Grid template columns (e.g., "140px 1fr 300px")
    /// </summary>
    public string GridColumns { get; set; } = "140px 1fr 300px";

    /// <summary>
    /// CSS Grid template rows (e.g., "70px 1fr" or "70px 1fr 60px" for footer)
    /// </summary>
    public string GridRows { get; set; } = "70px 1fr";

    /// <summary>
    /// Which areas are present in this layout
    /// </summary>
    public string[] Areas { get; set; } = new[] { "header", "sidebar", "main", "context" };

    /// <summary>
    /// Special layout features for this faction
    /// </summary>
    public string[] Features { get; set; } = Array.Empty<string>();
}

public class ButtonTemplate
{
    public string Shape { get; set; } = "rounded";
    public string[] Structure { get; set; } = Array.Empty<string>();
    public string CssClass { get; set; } = "";
    public string? BorderRadius { get; set; }
    public string? ClipPath { get; set; }
}

public class PanelTemplate
{
    public string Shape { get; set; } = "rounded";
    public string[] Structure { get; set; } = Array.Empty<string>();
    public string CssClass { get; set; } = "";
    public string? BorderRadius { get; set; }
    public string? ClipPath { get; set; }
    public DecorationElement[] Decorations { get; set; } = Array.Empty<DecorationElement>();
}

public class SidebarTemplate
{
    public string Layout { get; set; } = "vertical-stack";
    public string[] Structure { get; set; } = Array.Empty<string>();
    public string CssClass { get; set; } = "";
    public NavItemTemplate NavItemTemplate { get; set; } = new();
}

public class NavItemTemplate
{
    public string[] Structure { get; set; } = Array.Empty<string>();
    public string Shape { get; set; } = "rectangle";
    public string CssClass { get; set; } = "";
}

public class HeaderTemplate
{
    public string Layout { get; set; } = "horizontal";
    public string[] Structure { get; set; } = Array.Empty<string>();
    public string CssClass { get; set; } = "";
    public DecorationElement[] Decorations { get; set; } = Array.Empty<DecorationElement>();
}

public class DecorationElement
{
    public string Type { get; set; } = "";
    public string Position { get; set; } = "";
    public string? Color { get; set; }
    public string[]? Colors { get; set; }
}

public class ColorPalette
{
    public string Primary { get; set; } = "#4a9eff";
    public string Secondary { get; set; } = "#00d4ff";
    public string Accent { get; set; } = "#ffc844";
    public string Background { get; set; } = "#040608";
    public string Text { get; set; } = "#d0e4f8";
}

#endregion
