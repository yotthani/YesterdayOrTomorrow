namespace StarTrekGame.Web.Services;

/// <summary>
/// Template service for Main Menu UI (outside of game).
/// Provides a consistent, faction-neutral design for menu screens.
/// Separate from FactionTemplateService which handles in-game UI.
/// </summary>
public class MainMenuTemplateService
{
    private readonly Dictionary<string, MenuTemplate> _templates;

    public MainMenuTemplateService()
    {
        _templates = InitializeTemplates();
    }

    /// <summary>
    /// Main menu nutzt aktuell nur lcars – Fraktions-Styles sind noch nicht fertig und werden ignoriert.
    /// </summary>
    public MenuTemplate GetTemplate(string styleId = "lcars")
    {
        const string defaultStyle = "lcars";
        return _templates[defaultStyle];
    }

    /// <summary>
    /// Liefert nur das aktive Theme – Fraktions-Styles werden ausgeblendet, da noch nicht fertig.
    /// </summary>
    public IEnumerable<string> GetAllStyleIds() => new[] { "lcars" };

    private Dictionary<string, MenuTemplate> InitializeTemplates()
    {
        return new Dictionary<string, MenuTemplate>
        {
            // Default LCARS style - Federation-inspired but neutral
            ["lcars"] = new MenuTemplate
            {
                Id = "lcars",
                Name = "LCARS Interface",
                Description = "Classic Federation computer interface",

                Colors = new MenuColorPalette
                {
                    Primary = "#FF9900",      // LCARS orange
                    Secondary = "#CC9966",    // LCARS tan
                    Tertiary = "#FFCC99",     // LCARS peach
                    Accent = "#CC99FF",       // LCARS lavender
                    Background = "#000011",
                    Text = "#FFCC99",
                    TextMuted = "#9999CC"
                },

                Header = new MenuHeaderTemplate
                {
                    Style = "lcars-bar",
                    ShowLogo = true,
                    LogoStyle = "trek-italic",
                    Structure = new[] { "lcars-cap-left", "lcars-bar-segments", "lcars-cap-right", "title-block" }
                },

                Sidebar = new MenuSidebarTemplate
                {
                    Style = "lcars-sidebar",
                    Position = "left",
                    Width = "220px",
                    Structure = new[] { "cap-header", "nav-items", "spacer", "nav-items-secondary", "cap-footer" }
                },

                Panel = new MenuPanelTemplate
                {
                    Style = "lcars-panel",
                    BorderRadius = "0 30px 30px 0",
                    Structure = new[] { "color-strip-left", "content-area" }
                },

                Button = new MenuButtonTemplate
                {
                    Style = "lcars-rounded",
                    BorderRadius = "20px"
                },

                Footer = new MenuFooterTemplate
                {
                    Style = "lcars-footer",
                    ShowVersion = true,
                    Structure = new[] { "segment-left", "text-center", "segment-right" }
                }
            },

            // Cinematic style - dramatic, movie-like
            ["cinematic"] = new MenuTemplate
            {
                Id = "cinematic",
                Name = "Cinematic",
                Description = "Dramatic movie-style interface",

                Colors = new MenuColorPalette
                {
                    Primary = "#FFDD88",
                    Secondary = "#88AACC",
                    Tertiary = "#334455",
                    Accent = "#FFB366",
                    Background = "#000000",
                    Text = "#DDEEFF",
                    TextMuted = "#7799BB"
                },

                Header = new MenuHeaderTemplate
                {
                    Style = "cinematic-fade",
                    ShowLogo = true,
                    LogoStyle = "glow-shimmer",
                    Structure = new[] { "glow-backdrop", "title-centered", "subtitle-fade" }
                },

                Sidebar = new MenuSidebarTemplate
                {
                    Style = "cinematic-transparent",
                    Position = "left",
                    Width = "200px",
                    Structure = new[] { "nav-items-vertical", "fade-bottom" }
                },

                Panel = new MenuPanelTemplate
                {
                    Style = "cinematic-glass",
                    BorderRadius = "16px",
                    Structure = new[] { "glass-backdrop", "content-area", "glow-border" }
                },

                Button = new MenuButtonTemplate
                {
                    Style = "cinematic-glow",
                    BorderRadius = "8px"
                },

                Footer = new MenuFooterTemplate
                {
                    Style = "cinematic-minimal",
                    ShowVersion = true,
                    Structure = new[] { "disclaimer", "version-small" }
                }
            },

            // Minimal/Modern style
            ["minimal"] = new MenuTemplate
            {
                Id = "minimal",
                Name = "Minimal",
                Description = "Clean, modern interface",

                Colors = new MenuColorPalette
                {
                    Primary = "#4A9EFF",
                    Secondary = "#00D4FF",
                    Tertiary = "#1A2530",
                    Accent = "#FFC844",
                    Background = "#0A0E14",
                    Text = "#E0E8F0",
                    TextMuted = "#708898"
                },

                Header = new MenuHeaderTemplate
                {
                    Style = "minimal-clean",
                    ShowLogo = true,
                    LogoStyle = "simple-text",
                    Structure = new[] { "logo-left", "nav-center", "actions-right" }
                },

                Sidebar = new MenuSidebarTemplate
                {
                    Style = "minimal-hidden",
                    Position = "none",
                    Width = "0",
                    Structure = Array.Empty<string>()
                },

                Panel = new MenuPanelTemplate
                {
                    Style = "minimal-card",
                    BorderRadius = "12px",
                    Structure = new[] { "content-area" }
                },

                Button = new MenuButtonTemplate
                {
                    Style = "minimal-flat",
                    BorderRadius = "6px"
                },

                Footer = new MenuFooterTemplate
                {
                    Style = "minimal-footer",
                    ShowVersion = true,
                    Structure = new[] { "version-center" }
                }
            },

            // Starfleet Academy style - training/tutorial feel
            ["academy"] = new MenuTemplate
            {
                Id = "academy",
                Name = "Starfleet Academy",
                Description = "Educational interface style",

                Colors = new MenuColorPalette
                {
                    Primary = "#4488FF",
                    Secondary = "#88CCFF",
                    Tertiary = "#223344",
                    Accent = "#FFCC00",
                    Background = "#001122",
                    Text = "#AACCEE",
                    TextMuted = "#6688AA"
                },

                Header = new MenuHeaderTemplate
                {
                    Style = "academy-banner",
                    ShowLogo = true,
                    LogoStyle = "academy-crest",
                    Structure = new[] { "academy-crest", "title-block", "stardate-display" }
                },

                Sidebar = new MenuSidebarTemplate
                {
                    Style = "academy-nav",
                    Position = "left",
                    Width = "180px",
                    Structure = new[] { "nav-header", "nav-items", "progress-indicator" }
                },

                Panel = new MenuPanelTemplate
                {
                    Style = "academy-panel",
                    BorderRadius = "8px",
                    Structure = new[] { "header-bar", "content-area", "footer-bar" }
                },

                Button = new MenuButtonTemplate
                {
                    Style = "academy-button",
                    BorderRadius = "4px"
                },

                Footer = new MenuFooterTemplate
                {
                    Style = "academy-footer",
                    ShowVersion = true,
                    Structure = new[] { "academy-motto", "version" }
                }
            },

            // =================================================================
            // FACTION-SPECIFIC MENU STYLES
            // =================================================================

            // Klingon - Angular blade aesthetic
            ["klingon"] = new MenuTemplate
            {
                Id = "klingon",
                Name = "Klingon Honor",
                Description = "Warrior's battle interface",

                Colors = new MenuColorPalette
                {
                    Primary = "#CC0000",
                    Secondary = "#FF4444",
                    Tertiary = "#880000",
                    Accent = "#FFAA00",
                    Background = "#0A0505",
                    Text = "#FFCCAA",
                    TextMuted = "#996644"
                },

                Header = new MenuHeaderTemplate
                {
                    Style = "klingon-blade",
                    ShowLogo = true,
                    LogoStyle = "trefoil-emblem",
                    Structure = new[] { "blade-left", "emblem", "title-block", "blade-right" }
                },

                Sidebar = new MenuSidebarTemplate
                {
                    Style = "klingon-sidebar",
                    Position = "left",
                    Width = "200px",
                    Structure = new[] { "spike-top", "nav-blades", "spacer", "spike-bottom" }
                },

                Panel = new MenuPanelTemplate
                {
                    Style = "klingon-panel",
                    BorderRadius = "0",
                    Structure = new[] { "corner-tl", "corner-tr", "content-area", "corner-bl", "corner-br" }
                },

                Button = new MenuButtonTemplate
                {
                    Style = "klingon-angular",
                    BorderRadius = "0"
                },

                Footer = new MenuFooterTemplate
                {
                    Style = "klingon-footer",
                    ShowVersion = true,
                    Structure = new[] { "blade-divider", "version" }
                }
            },

            // Romulan - Elegant, curved, bronze accents
            ["romulan"] = new MenuTemplate
            {
                Id = "romulan",
                Name = "Romulan Elegance",
                Description = "Sophisticated spy interface",

                Colors = new MenuColorPalette
                {
                    Primary = "#00AA44",
                    Secondary = "#44FF88",
                    Tertiary = "#006633",
                    Accent = "#D4AF37",
                    Background = "#020A05",
                    Text = "#CCFFDD",
                    TextMuted = "#669977"
                },

                Header = new MenuHeaderTemplate
                {
                    Style = "romulan-elegant",
                    ShowLogo = true,
                    LogoStyle = "bird-emblem",
                    Structure = new[] { "wing-left", "emblem", "title-block", "wing-right", "v-accent" }
                },

                Sidebar = new MenuSidebarTemplate
                {
                    Style = "romulan-sidebar",
                    Position = "left",
                    Width = "180px",
                    Structure = new[] { "bronze-header", "nav-lenses", "bronze-footer" }
                },

                Panel = new MenuPanelTemplate
                {
                    Style = "romulan-panel",
                    BorderRadius = "2px",
                    Structure = new[] { "bronze-bar", "content-area", "accent-line" }
                },

                Button = new MenuButtonTemplate
                {
                    Style = "romulan-lens",
                    BorderRadius = "4px 20px 20px 4px"
                },

                Footer = new MenuFooterTemplate
                {
                    Style = "romulan-footer",
                    ShowVersion = true,
                    Structure = new[] { "v-divider", "version" }
                }
            },

            // Borg - Hexagonal grid, green glow
            ["borg"] = new MenuTemplate
            {
                Id = "borg",
                Name = "Borg Collective",
                Description = "Assimilation interface",

                Colors = new MenuColorPalette
                {
                    Primary = "#33CC33",
                    Secondary = "#00FF00",
                    Tertiary = "#006600",
                    Accent = "#CCCC33",
                    Background = "#000500",
                    Text = "#00FF00",
                    TextMuted = "#338833"
                },

                Header = new MenuHeaderTemplate
                {
                    Style = "borg-grid",
                    ShowLogo = false,
                    LogoStyle = "designation",
                    Structure = new[] { "hex-left", "designation-text", "hex-right", "node-indicator" }
                },

                Sidebar = new MenuSidebarTemplate
                {
                    Style = "borg-alcove",
                    Position = "left",
                    Width = "100px",
                    Structure = new[] { "targeting-display", "nav-hexes", "data-conduit" }
                },

                Panel = new MenuPanelTemplate
                {
                    Style = "borg-panel",
                    BorderRadius = "0",
                    Structure = new[] { "hex-corners", "scanlines", "content-area", "data-stream" }
                },

                Button = new MenuButtonTemplate
                {
                    Style = "borg-hex",
                    BorderRadius = "0"
                },

                Footer = new MenuFooterTemplate
                {
                    Style = "borg-footer",
                    ShowVersion = true,
                    Structure = new[] { "node-status", "collective-text" }
                }
            },

            // Cardassian - Notched corners, surveillance aesthetic
            ["cardassian"] = new MenuTemplate
            {
                Id = "cardassian",
                Name = "Cardassian Order",
                Description = "Central Command interface",

                Colors = new MenuColorPalette
                {
                    Primary = "#008888",
                    Secondary = "#00AAAA",
                    Tertiary = "#006666",
                    Accent = "#CC6633",
                    Background = "#050808",
                    Text = "#AADDDD",
                    TextMuted = "#668888"
                },

                Header = new MenuHeaderTemplate
                {
                    Style = "cardassian-command",
                    ShowLogo = true,
                    LogoStyle = "union-emblem",
                    Structure = new[] { "status-light", "emblem", "title-block", "hierarchy-indicator" }
                },

                Sidebar = new MenuSidebarTemplate
                {
                    Style = "cardassian-sidebar",
                    Position = "left",
                    Width = "180px",
                    Structure = new[] { "emblem-header", "nav-blocks", "scan-line" }
                },

                Panel = new MenuPanelTemplate
                {
                    Style = "cardassian-panel",
                    BorderRadius = "0",
                    Structure = new[] { "notch-tl", "notch-tr", "content-area", "notch-bl", "notch-br" }
                },

                Button = new MenuButtonTemplate
                {
                    Style = "cardassian-notched",
                    BorderRadius = "0"
                },

                Footer = new MenuFooterTemplate
                {
                    Style = "cardassian-footer",
                    ShowVersion = true,
                    Structure = new[] { "surveillance-line", "version" }
                }
            },

            // Ferengi - Hexagons, pie charts, business aesthetic
            ["ferengi"] = new MenuTemplate
            {
                Id = "ferengi",
                Name = "Ferengi Commerce",
                Description = "Profit-focused interface",

                Colors = new MenuColorPalette
                {
                    Primary = "#00CCFF",
                    Secondary = "#33DDFF",
                    Tertiary = "#0099BB",
                    Accent = "#FF66CC",
                    Background = "#0a0812",
                    Text = "#CCFFFF",
                    TextMuted = "#669999"
                },

                Header = new MenuHeaderTemplate
                {
                    Style = "ferengi-terminal",
                    ShowLogo = true,
                    LogoStyle = "profit-display",
                    Structure = new[] { "circular-display", "title-block", "profit-indicator" }
                },

                Sidebar = new MenuSidebarTemplate
                {
                    Style = "ferengi-sidebar",
                    Position = "left",
                    Width = "160px",
                    Structure = new[] { "pie-chart", "nav-hexes", "latinum-counter" }
                },

                Panel = new MenuPanelTemplate
                {
                    Style = "ferengi-panel",
                    BorderRadius = "4px",
                    Structure = new[] { "gradient-top", "content-area", "hexagon-footer" }
                },

                Button = new MenuButtonTemplate
                {
                    Style = "ferengi-business",
                    BorderRadius = "4px"
                },

                Footer = new MenuFooterTemplate
                {
                    Style = "ferengi-footer",
                    ShowVersion = true,
                    Structure = new[] { "transaction-bar", "version" }
                }
            },

            // Dominion - Imperial purple, diamond shapes
            ["dominion"] = new MenuTemplate
            {
                Id = "dominion",
                Name = "Dominion Imperial",
                Description = "Founders' divine interface",

                Colors = new MenuColorPalette
                {
                    Primary = "#8844CC",
                    Secondary = "#AA66EE",
                    Tertiary = "#663399",
                    Accent = "#FFCC00",
                    Background = "#0a0510",
                    Text = "#DDCCFF",
                    TextMuted = "#8866AA"
                },

                Header = new MenuHeaderTemplate
                {
                    Style = "dominion-imperial",
                    ShowLogo = true,
                    LogoStyle = "diamond-emblem",
                    Structure = new[] { "diamond-left", "founder-symbol", "title-block", "diamond-right" }
                },

                Sidebar = new MenuSidebarTemplate
                {
                    Style = "dominion-sidebar",
                    Position = "left",
                    Width = "200px",
                    Structure = new[] { "diamond-header", "nav-imperial", "shimmer-footer" }
                },

                Panel = new MenuPanelTemplate
                {
                    Style = "dominion-panel",
                    BorderRadius = "8px",
                    Structure = new[] { "diamond-top", "content-area", "shimmer-overlay" }
                },

                Button = new MenuButtonTemplate
                {
                    Style = "dominion-imperial",
                    BorderRadius = "8px"
                },

                Footer = new MenuFooterTemplate
                {
                    Style = "dominion-footer",
                    ShowVersion = true,
                    Structure = new[] { "gold-line", "version" }
                }
            },

            // Bajoran - Circular orbs, spiritual aesthetic
            ["bajoran"] = new MenuTemplate
            {
                Id = "bajoran",
                Name = "Bajoran Spiritual",
                Description = "Temple interface style",

                Colors = new MenuColorPalette
                {
                    Primary = "#00BBCC",
                    Secondary = "#33DDEE",
                    Tertiary = "#008899",
                    Accent = "#FF8833",
                    Background = "#040808",
                    Text = "#DDEEFF",
                    TextMuted = "#778899"
                },

                Header = new MenuHeaderTemplate
                {
                    Style = "bajoran-temple",
                    ShowLogo = true,
                    LogoStyle = "orb-display",
                    Structure = new[] { "wing-left", "orb-emblem", "title-block", "wing-right" }
                },

                Sidebar = new MenuSidebarTemplate
                {
                    Style = "bajoran-sidebar",
                    Position = "left",
                    Width = "180px",
                    Structure = new[] { "orb-header", "nav-circular", "wing-footer" }
                },

                Panel = new MenuPanelTemplate
                {
                    Style = "bajoran-panel",
                    BorderRadius = "12px",
                    Structure = new[] { "arch-top", "content-area", "warm-glow" }
                },

                Button = new MenuButtonTemplate
                {
                    Style = "bajoran-rounded",
                    BorderRadius = "20px"
                },

                Footer = new MenuFooterTemplate
                {
                    Style = "bajoran-footer",
                    ShowVersion = true,
                    Structure = new[] { "faith-divider", "version" }
                }
            },

            // =================================================================
            // ADDITIONAL FACTIONS - Unique UI Styles
            // =================================================================

            // Tholian Assembly - Crystalline, hexagonal, amber/orange
            ["tholian"] = new MenuTemplate
            {
                Id = "tholian",
                Name = "Tholian Assembly",
                Description = "Crystalline web interface",

                Colors = new MenuColorPalette
                {
                    Primary = "#FF8800",
                    Secondary = "#FFAA33",
                    Tertiary = "#CC6600",
                    Accent = "#FFDD00",
                    Background = "#0a0500",
                    Text = "#FFDDAA",
                    TextMuted = "#AA8855"
                },

                Header = new MenuHeaderTemplate
                {
                    Style = "tholian-web",
                    ShowLogo = true,
                    LogoStyle = "crystal-emblem",
                    Structure = new[] { "crystal-left", "web-pattern", "title-block", "crystal-right" }
                },

                Sidebar = new MenuSidebarTemplate
                {
                    Style = "tholian-sidebar",
                    Position = "left",
                    Width = "140px",
                    Structure = new[] { "hex-grid", "nav-crystals", "web-footer" }
                },

                Panel = new MenuPanelTemplate
                {
                    Style = "tholian-panel",
                    BorderRadius = "0",
                    Structure = new[] { "hex-frame", "web-overlay", "content-area" }
                },

                Button = new MenuButtonTemplate
                {
                    Style = "tholian-crystal",
                    BorderRadius = "0"
                },

                Footer = new MenuFooterTemplate
                {
                    Style = "tholian-footer",
                    ShowVersion = true,
                    Structure = new[] { "web-line", "version" }
                }
            },

            // Gorn Hegemony - Reptilian predator hive (SNW style)
            // Aggressive dinosaur-like creatures with bio-organic hive tech
            // Orange/amber holographics, green shields, organic grown ships
            ["gorn"] = new MenuTemplate
            {
                Id = "gorn",
                Name = "Gorn Hegemony",
                Description = "Reptilian predator hive interface",

                Colors = new MenuColorPalette
                {
                    Primary = "#FF6622",      // Orange holographic
                    Secondary = "#FFAA33",    // Amber
                    Tertiary = "#CC4400",     // Dark orange
                    Accent = "#44DD44",       // Green (shields)
                    Background = "#050807",   // Dark organic
                    Text = "#DDCCAA",
                    TextMuted = "#887755"
                },

                Header = new MenuHeaderTemplate
                {
                    Style = "gorn-hive",
                    ShowLogo = true,
                    LogoStyle = "predator-eye",
                    Structure = new[] { "rib-left", "emblem", "title-block", "rib-right", "holo-line" }
                },

                Sidebar = new MenuSidebarTemplate
                {
                    Style = "gorn-sidebar",
                    Position = "left",
                    Width = "180px",
                    Structure = new[] { "egg-indicators", "nav-hunters", "hive-status" }
                },

                Panel = new MenuPanelTemplate
                {
                    Style = "gorn-panel",
                    BorderRadius = "12px 12px 8px 8px",
                    Structure = new[] { "organic-ribs", "content-area", "holo-strip", "egg-glow" }
                },

                Button = new MenuButtonTemplate
                {
                    Style = "gorn-organic",
                    BorderRadius = "20px"
                },

                Footer = new MenuFooterTemplate
                {
                    Style = "gorn-footer",
                    ShowVersion = true,
                    Structure = new[] { "hive-divider", "version" }
                }
            },

            // Breen Confederacy - Discovery ship interior style (Gold/Amber + Cyan)
            ["breen"] = new MenuTemplate
            {
                Id = "breen",
                Name = "Breen Confederacy",
                Description = "Industrial enigma with gold light pillars",

                Colors = new MenuColorPalette
                {
                    Primary = "#FFAA33",      // Gold/Amber
                    Secondary = "#FFCC66",    // Light amber
                    Tertiary = "#CC8822",     // Dark gold
                    Accent = "#00CCCC",       // Cyan/Teal
                    Background = "#08080a",   // Near black
                    Text = "#FFEEDD",
                    TextMuted = "#AA9977"
                },

                Header = new MenuHeaderTemplate
                {
                    Style = "breen-industrial",
                    ShowLogo = true,
                    LogoStyle = "helmet-emblem",
                    Structure = new[] { "pillar-left", "emblem", "title-block", "pillar-right", "cyan-line" }
                },

                Sidebar = new MenuSidebarTemplate
                {
                    Style = "breen-sidebar",
                    Position = "left",
                    Width = "160px",
                    Structure = new[] { "gold-header", "nav-angular", "cyan-footer" }
                },

                Panel = new MenuPanelTemplate
                {
                    Style = "breen-panel",
                    BorderRadius = "0",
                    Structure = new[] { "angular-border", "content-area", "gold-pillars", "cyan-strip" }
                },

                Button = new MenuButtonTemplate
                {
                    Style = "breen-angular",
                    BorderRadius = "0"
                },

                Footer = new MenuFooterTemplate
                {
                    Style = "breen-footer",
                    ShowVersion = true,
                    Structure = new[] { "cyan-line", "version" }
                }
            },

            // Orion Syndicate - Criminal, green, seductive
            ["orion"] = new MenuTemplate
            {
                Id = "orion",
                Name = "Orion Syndicate",
                Description = "Criminal enterprise interface",

                Colors = new MenuColorPalette
                {
                    Primary = "#33AA66",
                    Secondary = "#55CC88",
                    Tertiary = "#228844",
                    Accent = "#FFD700",
                    Background = "#050a08",
                    Text = "#CCFFDD",
                    TextMuted = "#669977"
                },

                Header = new MenuHeaderTemplate
                {
                    Style = "orion-syndicate",
                    ShowLogo = true,
                    LogoStyle = "syndicate-emblem",
                    Structure = new[] { "chain-left", "emblem", "title-block", "chain-right" }
                },

                Sidebar = new MenuSidebarTemplate
                {
                    Style = "orion-sidebar",
                    Position = "left",
                    Width = "180px",
                    Structure = new[] { "credits-header", "nav-deals", "credits-footer" }
                },

                Panel = new MenuPanelTemplate
                {
                    Style = "orion-panel",
                    BorderRadius = "10px",
                    Structure = new[] { "gold-border", "content-area", "shadow-overlay" }
                },

                Button = new MenuButtonTemplate
                {
                    Style = "orion-deal",
                    BorderRadius = "8px"
                },

                Footer = new MenuFooterTemplate
                {
                    Style = "orion-footer",
                    ShowVersion = true,
                    Structure = new[] { "chain-divider", "version" }
                }
            },

            // =================================================================
            // DELTA QUADRANT FACTIONS
            // =================================================================

            // Kazon - Tribal, aggressive, orange/brown
            ["kazon"] = new MenuTemplate
            {
                Id = "kazon",
                Name = "Kazon Collective",
                Description = "Tribal warrior interface",

                Colors = new MenuColorPalette
                {
                    Primary = "#CC6633",
                    Secondary = "#DD8844",
                    Tertiary = "#994422",
                    Accent = "#FFAA55",
                    Background = "#0a0505",
                    Text = "#FFDDCC",
                    TextMuted = "#AA8866"
                },

                Header = new MenuHeaderTemplate
                {
                    Style = "kazon-tribal",
                    ShowLogo = true,
                    LogoStyle = "sect-emblem",
                    Structure = new[] { "spike-left", "emblem", "title-block", "spike-right" }
                },

                Sidebar = new MenuSidebarTemplate
                {
                    Style = "kazon-sidebar",
                    Position = "left",
                    Width = "160px",
                    Structure = new[] { "sect-header", "nav-tribal", "spike-footer" }
                },

                Panel = new MenuPanelTemplate
                {
                    Style = "kazon-panel",
                    BorderRadius = "0",
                    Structure = new[] { "tribal-border", "content-area", "spike-corners" }
                },

                Button = new MenuButtonTemplate
                {
                    Style = "kazon-rough",
                    BorderRadius = "0"
                },

                Footer = new MenuFooterTemplate
                {
                    Style = "kazon-footer",
                    ShowVersion = true,
                    Structure = new[] { "tribal-divider", "version" }
                }
            },

            // Hirogen - Hunter, predatory, dark green/black
            ["hirogen"] = new MenuTemplate
            {
                Id = "hirogen",
                Name = "Hirogen Hunters",
                Description = "Predator tracking interface",

                Colors = new MenuColorPalette
                {
                    Primary = "#557744",
                    Secondary = "#779966",
                    Tertiary = "#334422",
                    Accent = "#AACC88",
                    Background = "#030503",
                    Text = "#BBDDAA",
                    TextMuted = "#668855"
                },

                Header = new MenuHeaderTemplate
                {
                    Style = "hirogen-hunt",
                    ShowLogo = true,
                    LogoStyle = "trophy-emblem",
                    Structure = new[] { "sensor-left", "emblem", "title-block", "sensor-right" }
                },

                Sidebar = new MenuSidebarTemplate
                {
                    Style = "hirogen-sidebar",
                    Position = "left",
                    Width = "140px",
                    Structure = new[] { "tracking-header", "nav-hunt", "trophy-footer" }
                },

                Panel = new MenuPanelTemplate
                {
                    Style = "hirogen-panel",
                    BorderRadius = "2px",
                    Structure = new[] { "sensor-border", "content-area", "tracking-overlay" }
                },

                Button = new MenuButtonTemplate
                {
                    Style = "hirogen-stealth",
                    BorderRadius = "2px"
                },

                Footer = new MenuFooterTemplate
                {
                    Style = "hirogen-footer",
                    ShowVersion = true,
                    Structure = new[] { "sensor-line", "version" }
                }
            },

            // Talaxian - Friendly, warm, yellow/orange
            ["talaxian"] = new MenuTemplate
            {
                Id = "talaxian",
                Name = "Talaxian Trade",
                Description = "Friendly trader interface",

                Colors = new MenuColorPalette
                {
                    Primary = "#DDAA33",
                    Secondary = "#FFCC55",
                    Tertiary = "#AA8822",
                    Accent = "#FFEE88",
                    Background = "#0a0805",
                    Text = "#FFEEDD",
                    TextMuted = "#AA9966"
                },

                Header = new MenuHeaderTemplate
                {
                    Style = "talaxian-friendly",
                    ShowLogo = true,
                    LogoStyle = "whisker-emblem",
                    Structure = new[] { "curve-left", "emblem", "title-block", "curve-right" }
                },

                Sidebar = new MenuSidebarTemplate
                {
                    Style = "talaxian-sidebar",
                    Position = "left",
                    Width = "180px",
                    Structure = new[] { "warm-header", "nav-friendly", "warm-footer" }
                },

                Panel = new MenuPanelTemplate
                {
                    Style = "talaxian-panel",
                    BorderRadius = "16px",
                    Structure = new[] { "warm-border", "content-area", "friendly-glow" }
                },

                Button = new MenuButtonTemplate
                {
                    Style = "talaxian-warm",
                    BorderRadius = "16px"
                },

                Footer = new MenuFooterTemplate
                {
                    Style = "talaxian-footer",
                    ShowVersion = true,
                    Structure = new[] { "warm-divider", "version" }
                }
            },

            // Vidiian - Medical, disease, sickly green/yellow
            ["vidiian"] = new MenuTemplate
            {
                Id = "vidiian",
                Name = "Vidiian Sodality",
                Description = "Medical science interface",

                Colors = new MenuColorPalette
                {
                    Primary = "#88AA66",
                    Secondary = "#AACC88",
                    Tertiary = "#668844",
                    Accent = "#DDEE99",
                    Background = "#050805",
                    Text = "#DDEEBB",
                    TextMuted = "#889966"
                },

                Header = new MenuHeaderTemplate
                {
                    Style = "vidiian-medical",
                    ShowLogo = true,
                    LogoStyle = "medical-emblem",
                    Structure = new[] { "scan-left", "emblem", "title-block", "scan-right" }
                },

                Sidebar = new MenuSidebarTemplate
                {
                    Style = "vidiian-sidebar",
                    Position = "left",
                    Width = "160px",
                    Structure = new[] { "bio-header", "nav-medical", "bio-footer" }
                },

                Panel = new MenuPanelTemplate
                {
                    Style = "vidiian-panel",
                    BorderRadius = "6px",
                    Structure = new[] { "bio-border", "content-area", "scan-overlay" }
                },

                Button = new MenuButtonTemplate
                {
                    Style = "vidiian-clinical",
                    BorderRadius = "6px"
                },

                Footer = new MenuFooterTemplate
                {
                    Style = "vidiian-footer",
                    ShowVersion = true,
                    Structure = new[] { "bio-line", "version" }
                }
            },

            // Nausicaan - Pirate, brutal, dark red
            ["nausicaan"] = new MenuTemplate
            {
                Id = "nausicaan",
                Name = "Nausicaan Raiders",
                Description = "Pirate raider interface",

                Colors = new MenuColorPalette
                {
                    Primary = "#993333",
                    Secondary = "#BB5555",
                    Tertiary = "#662222",
                    Accent = "#DD7777",
                    Background = "#080505",
                    Text = "#DDBBBB",
                    TextMuted = "#886666"
                },

                Header = new MenuHeaderTemplate
                {
                    Style = "nausicaan-raider",
                    ShowLogo = true,
                    LogoStyle = "tusk-emblem",
                    Structure = new[] { "tusk-left", "emblem", "title-block", "tusk-right" }
                },

                Sidebar = new MenuSidebarTemplate
                {
                    Style = "nausicaan-sidebar",
                    Position = "left",
                    Width = "160px",
                    Structure = new[] { "raid-header", "nav-brutal", "raid-footer" }
                },

                Panel = new MenuPanelTemplate
                {
                    Style = "nausicaan-panel",
                    BorderRadius = "0",
                    Structure = new[] { "brutal-border", "content-area", "tusk-corners" }
                },

                Button = new MenuButtonTemplate
                {
                    Style = "nausicaan-brutal",
                    BorderRadius = "0"
                },

                Footer = new MenuFooterTemplate
                {
                    Style = "nausicaan-footer",
                    ShowVersion = true,
                    Structure = new[] { "tusk-divider", "version" }
                }
            }
        };
    }
}

#region Menu Template Data Models

public class MenuTemplate
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public MenuColorPalette Colors { get; set; } = new();
    public MenuHeaderTemplate Header { get; set; } = new();
    public MenuSidebarTemplate Sidebar { get; set; } = new();
    public MenuPanelTemplate Panel { get; set; } = new();
    public MenuButtonTemplate Button { get; set; } = new();
    public MenuFooterTemplate Footer { get; set; } = new();
}

public class MenuColorPalette
{
    public string Primary { get; set; } = "#FF9900";
    public string Secondary { get; set; } = "#CC9966";
    public string Tertiary { get; set; } = "#FFCC99";
    public string Accent { get; set; } = "#CC99FF";
    public string Background { get; set; } = "#000011";
    public string Text { get; set; } = "#FFCC99";
    public string TextMuted { get; set; } = "#9999CC";
}

public class MenuHeaderTemplate
{
    public string Style { get; set; } = "lcars-bar";
    public bool ShowLogo { get; set; } = true;
    public string LogoStyle { get; set; } = "trek-italic";
    public string[] Structure { get; set; } = Array.Empty<string>();
}

public class MenuSidebarTemplate
{
    public string Style { get; set; } = "lcars-sidebar";
    public string Position { get; set; } = "left";  // "left", "right", "none"
    public string Width { get; set; } = "220px";
    public string[] Structure { get; set; } = Array.Empty<string>();
}

public class MenuPanelTemplate
{
    public string Style { get; set; } = "lcars-panel";
    public string BorderRadius { get; set; } = "0 30px 30px 0";
    public string[] Structure { get; set; } = Array.Empty<string>();
}

public class MenuButtonTemplate
{
    public string Style { get; set; } = "lcars-rounded";
    public string BorderRadius { get; set; } = "20px";
}

public class MenuFooterTemplate
{
    public string Style { get; set; } = "lcars-footer";
    public bool ShowVersion { get; set; } = true;
    public string[] Structure { get; set; } = Array.Empty<string>();
}

#endregion
