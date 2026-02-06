using System.Net.Http;
using StarTrekGame.AssetGenerator.Models;
using AssetGenerator.Services;

namespace StarTrekGame.AssetGenerator.Services;

public class PromptBuilderService
{
    private readonly Dictionary<Faction, FactionProfile> _factionProfiles;
    private readonly PromptDataService _promptData;
    private readonly BuildingManifestService _buildingManifestService;
    private bool _jsonDataLoaded = false;

    public PromptBuilderService(HttpClient httpClient)
    {
        _factionProfiles = InitializeFactionProfiles();
        _promptData = new PromptDataService(httpClient);
        _buildingManifestService = new BuildingManifestService();
    }

    /// <summary>
    /// Get the building manifest service for external use
    /// </summary>
    public BuildingManifestService BuildingManifestService => _buildingManifestService;

    /// <summary>
    /// Initialize JSON data loading (call once at startup)
    /// </summary>
    public async Task InitializeJsonDataAsync()
    {
        if (!_jsonDataLoaded)
        {
            await _promptData.LoadAsync();
            await _buildingManifestService.LoadAsync();
            _jsonDataLoaded = true;
        }
    }
    
    // NOTE: Ship geometry data is now loaded from Ships.json via PromptDataService
    // The old hardcoded InitializeIconicShipGeometry() dictionary has been removed.
    // If you get errors about missing ship data, check that Ships.json has the required faction entries.

    public FactionProfile GetFactionProfile(Faction faction) => _factionProfiles[faction];
    
    public List<string> GetAssetList(Faction faction, AssetCategory category)
    {
        var profile = _factionProfiles[faction];
        return category switch
        {
            AssetCategory.MilitaryShips => profile.MilitaryShips,
            AssetCategory.CivilianShips => profile.CivilianShips,
            AssetCategory.MilitaryStructures => profile.MilitaryStructures,
            AssetCategory.CivilianStructures => profile.CivilianStructures,
            AssetCategory.Buildings => profile.Buildings,
            AssetCategory.Troops => profile.Troops,
            AssetCategory.Vehicles => GetVehiclesList(faction),
            AssetCategory.Portraits => profile.PortraitVariants,
            AssetCategory.HouseSymbols => profile.HouseSymbols,
            AssetCategory.EventCharacters => profile.EventCharacters,
            AssetCategory.FactionLeaders => GetFactionLeadersList(),
            // New faction-specific categories
            AssetCategory.UIElements => GetUIElementsList(faction),
            AssetCategory.UIIcons => GetUIIconsList(faction),
            // Universal categories (faction ignored)
            AssetCategory.Planets => GetPlanetsList(),
            AssetCategory.Stars => GetStarsList(),
            AssetCategory.Anomalies => GetAnomaliesList(),
            AssetCategory.GalaxyTiles => GetGalaxyTilesList(),
            AssetCategory.SystemElements => GetSystemElementsList(),
            AssetCategory.Effects => GetEffectsList(faction),
            // All factions in one grid
            AssetCategory.FactionSymbols => GetFactionSymbolsList(),
            AssetCategory.SpecialCharacters => GetSpecialCharactersList(),
            _ => new List<string>()
        };
    }
    
    public GridSpec GetGridSpec(AssetCategory category)
    {
        return category switch
        {
            AssetCategory.MilitaryShips => new GridSpec { Columns = 6, Rows = 6 },
            AssetCategory.CivilianShips => new GridSpec { Columns = 6, Rows = 6 },
            AssetCategory.MilitaryStructures => new GridSpec { Columns = 6, Rows = 6 },
            AssetCategory.CivilianStructures => new GridSpec { Columns = 6, Rows = 6 },
            AssetCategory.Buildings => new GridSpec { Columns = 8, Rows = 6 },
            AssetCategory.Troops => new GridSpec { Columns = 6, Rows = 6 },
            AssetCategory.Vehicles => new GridSpec { Columns = 4, Rows = 4 },  // 16 vehicles
            AssetCategory.Portraits => new GridSpec { Columns = 6, Rows = 6 },
            AssetCategory.HouseSymbols => new GridSpec { Columns = 8, Rows = 6 },
            AssetCategory.EventCharacters => new GridSpec { Columns = 6, Rows = 6 },
            AssetCategory.FactionLeaders => new GridSpec { Columns = 4, Rows = 5 },  // 20 faction leaders (17 defined + 3 spare)
            // New categories
            AssetCategory.UIElements => new GridSpec { Columns = 6, Rows = 6 },
            AssetCategory.UIIcons => new GridSpec { Columns = 8, Rows = 6 },
            AssetCategory.Planets => new GridSpec { Columns = 6, Rows = 6 },
            AssetCategory.Stars => new GridSpec { Columns = 4, Rows = 4 },
            AssetCategory.Anomalies => new GridSpec { Columns = 6, Rows = 6 },
            AssetCategory.GalaxyTiles => new GridSpec { Columns = 6, Rows = 6 },
            AssetCategory.SystemElements => new GridSpec { Columns = 6, Rows = 6 },
            AssetCategory.Effects => new GridSpec { Columns = 6, Rows = 6 },
            AssetCategory.FactionSymbols => new GridSpec { Columns = 5, Rows = 5 },  // 21+ factions (25 slots)
            AssetCategory.SpecialCharacters => new GridSpec { Columns = 4, Rows = 4 },  // 16 special chars
            _ => new GridSpec { Columns = 6, Rows = 6 }
        };
    }
    
    /// <summary>
    /// Check if a faction supports a given category
    /// </summary>
    public bool FactionSupportsCategory(Faction faction, AssetCategory category)
    {
        var profile = _factionProfiles[faction];
        
        // Universal/faction-independent categories - ONLY available under "Special" faction
        var universalCategories = new[] 
        { 
            AssetCategory.Planets, 
            AssetCategory.Stars, 
            AssetCategory.Anomalies,
            AssetCategory.GalaxyTiles,
            AssetCategory.SystemElements,
            AssetCategory.FactionSymbols,
            AssetCategory.FactionLeaders,
            AssetCategory.SpecialCharacters,  // Q, Data, Khan etc.
            AssetCategory.EventCharacters     // Random aliens for events (faction-independent)
        };
        
        if (universalCategories.Contains(category))
        {
            // Only show universal categories under "Special" faction
            return faction == Faction.Special;
        }
        
        // Special faction ONLY supports universal categories (handled above)
        if (faction == Faction.Special)
        {
            return false;  // All other categories not available
        }
        
        // AncientRaces only supports EventCharacters
        if (faction == Faction.AncientRaces)
        {
            return category == AssetCategory.EventCharacters;
        }
        
        // Portrait-only factions
        if (profile.IsPortraitOnly)
        {
            return category == AssetCategory.Portraits || category == AssetCategory.EventCharacters;
        }
        
        // Check if faction has assets for this category
        return category switch
        {
            AssetCategory.MilitaryShips => profile.HasShips && profile.MilitaryShips.Any(),
            AssetCategory.CivilianShips => profile.HasShips && profile.CivilianShips.Any(),
            AssetCategory.MilitaryStructures => profile.MilitaryStructures.Any(),
            AssetCategory.CivilianStructures => profile.CivilianStructures.Any(),
            AssetCategory.Buildings => profile.HasBuildings && profile.Buildings.Any(),
            AssetCategory.Troops => profile.HasTroops && profile.Troops.Any(),
            AssetCategory.Vehicles => true, // All factions have vehicles
            AssetCategory.Portraits => profile.PortraitVariants.Any(),
            AssetCategory.HouseSymbols => profile.HouseSymbols.Any(),
            AssetCategory.EventCharacters => profile.EventCharacters.Any(),
            // Faction-specific UI
            AssetCategory.UIElements => true,
            AssetCategory.UIIcons => true,
            AssetCategory.Effects => true,
            _ => false
        };
    }
    
    /// <summary>
    /// Get available categories for a faction
    /// </summary>
    public List<AssetCategory> GetAvailableCategories(Faction faction)
    {
        return Enum.GetValues<AssetCategory>()
            .Where(c => FactionSupportsCategory(faction, c))
            .ToList();
    }
    
    public string BuildPrompt(Faction faction, AssetCategory category, string assetName)
    {
        var profile = _factionProfiles[faction];
        
        // Try JSON data first for supported categories
        if (_jsonDataLoaded)
        {
            var jsonPrompt = TryBuildFromJson(category, assetName, faction);
            if (!string.IsNullOrEmpty(jsonPrompt))
                return jsonPrompt;
        }
        
        // Fall back to hardcoded prompts
        return category switch
        {
            AssetCategory.MilitaryShips => BuildShipPrompt(profile, assetName, isMilitary: true),
            AssetCategory.CivilianShips => BuildShipPrompt(profile, assetName, isMilitary: false),
            AssetCategory.MilitaryStructures => BuildStructurePrompt(profile, assetName, isMilitary: true),
            AssetCategory.CivilianStructures => BuildStructurePrompt(profile, assetName, isMilitary: false),
            AssetCategory.Buildings => BuildBuildingPrompt(profile, assetName),
            AssetCategory.Troops => BuildTroopPrompt(profile, assetName),
            AssetCategory.Vehicles => BuildVehiclePrompt(profile, assetName),
            AssetCategory.Portraits => BuildPortraitPrompt(profile, assetName),
            AssetCategory.HouseSymbols => BuildHouseSymbolPrompt(profile, assetName),
            AssetCategory.EventCharacters => BuildEventCharacterPrompt(profile, assetName),
            AssetCategory.FactionLeaders => BuildFactionLeaderPrompt(assetName),
            AssetCategory.SpecialCharacters => BuildSpecialCharacterPrompt(assetName),
            // New categories
            AssetCategory.UIElements => BuildUIElementPrompt(profile, assetName),
            AssetCategory.UIIcons => BuildUIIconPrompt(profile, assetName),
            AssetCategory.Planets => BuildPlanetPrompt(assetName),
            AssetCategory.Stars => BuildStarPrompt(assetName),
            AssetCategory.Anomalies => BuildAnomalyPrompt(assetName),
            AssetCategory.GalaxyTiles => BuildGalaxyTilePrompt(assetName),
            AssetCategory.SystemElements => BuildSystemElementPrompt(assetName),
            AssetCategory.Effects => BuildEffectPrompt(profile, assetName),
            AssetCategory.FactionSymbols => BuildFactionSymbolPrompt(assetName),
            _ => string.Empty
        };
    }
    
    /// <summary>
    /// Try to build prompt from JSON data
    /// </summary>
    private string TryBuildFromJson(AssetCategory category, string assetName, Faction faction)
    {
        try
        {
            return category switch
            {
                AssetCategory.FactionLeaders when _promptData.HasCategory("factionleaders") 
                    => BuildFactionLeaderFromJson(assetName),
                AssetCategory.SpecialCharacters when _promptData.HasCategory("specialcharacters") 
                    => BuildSpecialCharacterFromJson(assetName),
                AssetCategory.EventCharacters when _promptData.HasCategory("eventcharacters") 
                    => BuildEventCharacterFromJson(assetName),
                AssetCategory.FactionSymbols when _promptData.HasCategory("factionsymbols") 
                    => BuildFactionSymbolFromJson(assetName),
                AssetCategory.Planets when _promptData.HasCategory("planets") 
                    => BuildPlanetFromJson(assetName),
                AssetCategory.Stars when _promptData.HasCategory("stars") 
                    => BuildStarFromJson(assetName),
                AssetCategory.Anomalies when _promptData.HasCategory("anomalies") 
                    => BuildAnomalyFromJson(assetName),
                _ => ""
            };
        }
        catch
        {
            return "";
        }
    }
    
    private string BuildFactionLeaderFromJson(string leaderName)
    {
        var leaderPrompt = _promptData.BuildFactionLeaderPrompt(leaderName);
        var styleGuide = _promptData.GetStyleGuide("factionleaders");
        
        return $@"FACTION LEADER PORTRAIT - OFFICIAL HEAD OF STATE

{leaderPrompt}

{styleGuide}

**medium shot faction leader portrait, claymation 3D style, commanding presence, game character portrait**
--no funny, thumbprints, exaggerated features, young appearance, casual clothing, action poses, text, labels";
    }
    
    private string BuildSpecialCharacterFromJson(string characterName)
    {
        var charPrompt = _promptData.BuildSpecialCharacterPrompt(characterName);
        var styleGuide = _promptData.GetStyleGuide("specialcharacters");
        
        return $@"ICONIC STAR TREK CHARACTER

{charPrompt}

{styleGuide}

**character portrait, claymation 3D style, recognizable iconic character, game asset**
--no funny, wrong details, generic appearance, text, labels";
    }
    
    private string BuildEventCharacterFromJson(string characterName)
    {
        var charPrompt = _promptData.BuildEventCharacterPrompt(characterName);
        var styleGuide = _promptData.GetStyleGuide("eventcharacters");
        
        return $@"GENERIC NPC CHARACTER FOR GAME EVENTS

{charPrompt}

{styleGuide}

**NPC portrait, claymation 3D style, generic character type, game asset**
--no specific real characters, no Star Trek main cast, text, labels";
    }
    
    private string BuildFactionSymbolFromJson(string symbolName)
    {
        var symbolPrompt = _promptData.BuildFactionSymbolPrompt(symbolName);
        var styleGuide = _promptData.GetStyleGuide("factionsymbols");
        
        return $@"FLAT FACTION SYMBOL / LOGO

{symbolPrompt}

{styleGuide}

**flat 2D faction logo, clean vector emblem, bold graphic symbol, official insignia, game asset**
--no 3D, no metallic, no depth, no shadows, no gradients, no reflections, no perspective, no text, no labels";
    }
    
    private string BuildPlanetFromJson(string planetName)
    {
        var planetPrompt = _promptData.BuildPlanetPrompt(planetName);
        var styleGuide = _promptData.GetStyleGuide("planets");
        
        return $@"PLANET IN SPACE

{planetPrompt}

{styleGuide}

**planet from space, orbital view, realistic space scene, game asset**
--no text, no labels, no UI elements";
    }
    
    private string BuildStarFromJson(string starName)
    {
        var starPrompt = _promptData.BuildStarPrompt(starName);
        var styleGuide = _promptData.GetStyleGuide("stars");
        
        return $@"STAR IN SPACE

{starPrompt}

{styleGuide}

**star in space, glowing stellar object, realistic space scene, game asset**
--no text, no labels, no UI elements";
    }
    
    private string BuildAnomalyFromJson(string anomalyName)
    {
        var anomalyPrompt = _promptData.BuildAnomalyPrompt(anomalyName);
        var styleGuide = _promptData.GetStyleGuide("anomalies");
        
        return $@"SPACE ANOMALY / PHENOMENON

{anomalyPrompt}

{styleGuide}

**space anomaly, cosmic phenomenon, dramatic space scene, game asset**
--no text, no labels, no UI elements";
    }
    
    private string BuildShipPrompt(FactionProfile profile, string shipName, bool isMilitary)
    {
        // Civilian ships need completely different treatment
        if (!isMilitary)
        {
            return BuildCivilianShipPrompt(profile, shipName);
        }

        var shipType = "military warship";
        var weaponNote = "weapon arrays visible";

        // Get ship description from JSON data (Ships.json) - this is the ONLY source
        // Throw error if JSON data is missing so we notice immediately
        var geometryParts = new List<string>();

        // Get faction-wide design language from JSON (designLanguage, colors, features, important)
        var jsonFactionStyle = _promptData.GetFactionStyle("ships", profile.Faction.ToString(), isMilitary: true);
        if (string.IsNullOrEmpty(jsonFactionStyle))
        {
            throw new InvalidOperationException(
                $"MISSING JSON DATA: Ships.json has no 'factionStyles.{profile.Faction.ToString().ToLower()}.military' entry. " +
                $"Please add faction style definition for {profile.Faction} in Data/Prompts/Ships.json");
        }
        geometryParts.Add(jsonFactionStyle);

        // Get ship class variant from JSON (more specific than faction default) - optional but logged
        var jsonClassVariant = _promptData.GetShipClassVariant(profile.Faction.ToString(), shipName);
        if (!string.IsNullOrEmpty(jsonClassVariant))
        {
            geometryParts.Add(jsonClassVariant);
        }
        else
        {
            // Log info - not every ship needs a specific class variant, faction default is used
            Console.WriteLine($"[INFO] No specific classVariant for '{shipName}' in {profile.Faction} - using faction default");
        }

        var geometryDescription = string.Join("\n\n", geometryParts);
        var geometrySection = $"\n\n{geometryDescription}\n";
        
        // Add faction-specific tactical notes
        var tacticalNote = GetFactionTacticalNote(profile.Faction);

        // Faction-specific warnings and negative prompts
        var factionWarning = GetFactionShipWarning(profile.Faction);
        var factionNegativePrompt = GetFactionNegativePrompt(profile.Faction);

        // Adjust camera description based on faction (not all have saucers!)
        var cameraFrontDescription = profile.Faction == Faction.Federation
            ? "Saucer/bridge appears in lower-left quadrant"
            : "Ship's bow/front appears in lower-left quadrant";

        return $@"MANDATORY STYLE GUIDE:
Material & Texture: The spaceship must look like a handmade physical model made of plasticine clay with a subtle comic-like shader. Realistic metallic hull texture with a non-glossy, soft clay finish. High-end stop-motion animation quality (Laika/Aardman style). Very detailed panel lines, windows, and technical details.
Proportions (CRUCIAL - FOLLOW EXACTLY): Accurate to the ship class design. Maintain correct proportions and EXACT number of components as specified.
Lighting: Soft, cinematic studio lighting that emphasizes the clay texture and model quality. No harsh digital shine. Subtle glow from windows and engines.
Camera/Perspective (CRITICAL - MUST FOLLOW EXACTLY):
- DIRECTION: Ship flies toward LOWER-LEFT, engines point UPPER-RIGHT
- Camera at TOP-RIGHT corner looking DOWN at ship
- Ship's FRONT/NOSE/BOW = BOTTOM-LEFT of image
- Ship's BACK/ENGINES/STERN = TOP-RIGHT of image
- Visible: TOP surface + LEFT (port) side of ship
- Nacelles/engines appear in upper-right quadrant
- {cameraFrontDescription}
- Like viewing from 2 o'clock position above the ship
- Angle: 30-45 degrees above horizontal
- DO NOT flip or mirror - nose MUST be lower-left!
Background: Solid black background (#000000).
Details: Glowing windows, engine glow, subtle weathering, {profile.Name} faction markings, {weaponNote}.
{geometrySection}
{tacticalNote}
{factionWarning}
Subject: Single {profile.Name} {shipType}, {shipName} class.
Design Language: {profile.DesignLanguage}
Color Scheme: {profile.ColorScheme}

CRITICAL RULES:
1. Follow the EXACT GEOMETRY specified above
2. Do NOT add extra nacelles or components
3. Count components carefully before generating
4. Ship MUST face BOTTOM-LEFT, engines toward TOP-RIGHT

**single spacecraft miniature model, rendered in high-quality stylized claymation 3D style, game asset, isometric view, ship facing bottom-left**
--no funny, thumbprints, exaggerated features, multiple ships, fleet, grid, shiny plastic, CGI look, action figure, base, stand, table surface, tilt-shift, blurry background, frame, border, text, label, pedestal, extra engines, wrong number of nacelles, stacked hulls, ship facing right, ship facing up, frontal view, rear view{factionNegativePrompt}";
    }
    
    /// <summary>
    /// Build prompt specifically for civilian ships - NO military design language
    /// </summary>
    private string BuildCivilianShipPrompt(FactionProfile profile, string shipName)
    {
        // Determine ship type and size based on name
        var (shipCategory, sizeClass, description) = ClassifyCivilianShip(shipName);
        
        // Get civilian-specific geometry if available
        var geometryDescription = GetCivilianShipGeometry(shipName, profile.Faction);
        var geometrySection = !string.IsNullOrEmpty(geometryDescription) 
            ? $"\n\n{geometryDescription}\n" 
            : "";
        
        // Civilian ships should NOT have military features
        var civilianRules = GetCivilianShipRules(shipCategory);
        
        // Faction-specific civilian aesthetic
        var civilianAesthetic = GetFactionCivilianAesthetic(profile.Faction);
        
        return $@"MANDATORY STYLE GUIDE:
Material & Texture: The spacecraft must look like a handmade physical model made of plasticine clay with a subtle comic-like shader. Realistic hull texture with a non-glossy, soft clay finish. High-end stop-motion animation quality (Laika/Aardman style).
Proportions (CRUCIAL): {sizeClass} - this is a {shipCategory}, NOT a military warship. {description}
Lighting: Soft, cinematic studio lighting that emphasizes the clay texture and model quality. Subtle glow from windows and engines.
Camera/Perspective (CRITICAL - MUST FOLLOW EXACTLY):
- DIRECTION: Ship flies toward LOWER-LEFT, engines point UPPER-RIGHT
- Camera at TOP-RIGHT corner looking DOWN at ship
- Ship's FRONT/NOSE/BOW = BOTTOM-LEFT of image
- Ship's BACK/ENGINES/STERN = TOP-RIGHT of image
- Visible: TOP surface + LEFT (port) side of ship
- Like viewing from 2 o'clock position above the ship
- DO NOT flip or mirror - nose MUST be lower-left!
Background: Solid black background (#000000).
{geometrySection}
{civilianRules}
{civilianAesthetic}

Subject: Single {profile.Name} civilian {shipCategory}, {shipName}. 
Design Language: Civilian/commercial variant - {profile.CivilianDesignLanguage}
Color Scheme: {profile.CivilianColorScheme}

CRITICAL CIVILIAN SHIP RULES:
1. NO SAUCER SECTIONS unless it's specifically a passenger liner
2. NO VISIBLE WEAPONS - this is a civilian vessel
3. Size should match function: shuttles are SMALL, freighters are BULKY, transports are MEDIUM
4. Utilitarian, practical design - function over form
5. Cargo bays, fuel tanks, passenger areas should be visible where appropriate
6. {shipCategory} should look like a {shipCategory}, NOT like a warship

**single spacecraft miniature model, {sizeClass} civilian vessel, rendered in high-quality stylized claymation 3D style, game asset**
--no weapons, no military, no warship, no saucer section, no nacelles on pylons, no battleship, funny, thumbprints, exaggerated features, multiple ships, fleet, grid, shiny plastic, CGI look, action figure, base, stand, table surface, tilt-shift, blurry background, frame, border, text, label, pedestal";
    }
    
    /// <summary>
    /// Classify civilian ship by type and determine appropriate size
    /// </summary>
    private (string category, string sizeClass, string description) ClassifyCivilianShip(string shipName)
    {
        var lowerName = shipName.ToLower();
        
        // Shuttles - SMALL
        if (lowerName.Contains("shuttle") || lowerName.Contains("pod") || lowerName.Contains("work bee") || 
            lowerName.Contains("runabout") || lowerName.Contains("flyer"))
        {
            return ("shuttle/small craft", "SMALL (fits in a hangar bay)", 
                "Compact vessel for short-range transport. No larger than a bus. Simple, boxy or aerodynamic shape.");
        }
        
        // Freighters/Cargo - LARGE and BULKY
        if (lowerName.Contains("freighter") || lowerName.Contains("cargo") || lowerName.Contains("hauler") || 
            lowerName.Contains("tanker") || lowerName.Contains("carrier") || lowerName.Contains("ore"))
        {
            return ("freighter/cargo ship", "LARGE and BULKY", 
                "Massive cargo holds dominate the design. Industrial, utilitarian. Container modules, fuel pods, or bulk storage visible. Wide and heavy-set.");
        }
        
        // Mining ships - INDUSTRIAL
        if (lowerName.Contains("mining") || lowerName.Contains("salvage") || lowerName.Contains("tug") || 
            lowerName.Contains("construction") || lowerName.Contains("work"))
        {
            return ("industrial/mining vessel", "INDUSTRIAL with equipment", 
                "Heavy equipment visible: mining lasers, grappling arms, processing units, collection gear. Rugged, weathered, utilitarian.");
        }
        
        // Transport/Passenger - MEDIUM with windows
        if (lowerName.Contains("transport") || lowerName.Contains("passenger") || lowerName.Contains("liner") || 
            lowerName.Contains("ferry") || lowerName.Contains("yacht") || lowerName.Contains("personnel"))
        {
            return ("transport/passenger ship", "MEDIUM with many windows", 
                "Passenger decks with rows of windows. Comfortable rather than industrial. Civilian amenities visible.");
        }
        
        // Colony ships - VERY LARGE
        if (lowerName.Contains("colony") || lowerName.Contains("settler") || lowerName.Contains("ark") || 
            lowerName.Contains("generation"))
        {
            return ("colony ship", "VERY LARGE multi-deck vessel", 
                "Massive vessel with habitation domes, agricultural sections, industrial areas. Self-contained city in space.");
        }
        
        // Research/Science - SPECIALIZED
        if (lowerName.Contains("research") || lowerName.Contains("science") || lowerName.Contains("survey") || 
            lowerName.Contains("probe") || lowerName.Contains("explorer"))
        {
            return ("research vessel", "MEDIUM with sensor arrays", 
                "Scientific equipment: sensor pods, dish antennas, probe launchers, laboratories. Academic rather than military.");
        }
        
        // Medical - WHITE with crosses
        if (lowerName.Contains("medical") || lowerName.Contains("hospital") || lowerName.Contains("rescue") || 
            lowerName.Contains("ambulance"))
        {
            return ("medical ship", "MEDIUM with red cross markings", 
                "Hospital ship with medical bays visible. White hull with red cross markings. Docking ports for patient transfer.");
        }
        
        // Default civilian
        return ("civilian vessel", "MEDIUM civilian craft", 
            "Non-military design. Practical, functional, civilian purpose.");
    }
    
    /// <summary>
    /// Get rules specific to civilian ship categories
    /// </summary>
    private string GetCivilianShipRules(string shipCategory)
    {
        if (shipCategory.Contains("shuttle"))
        {
            return @"SHUTTLE DESIGN RULES:
- SMALL SIZE: No bigger than a large bus/van
- Simple shape: boxy, aerodynamic, or rounded
- Cockpit windows visible at front
- Small thrusters, NOT large engines
- NO warp nacelles on long pylons - integrated or minimal
- Reference: Star Trek shuttlecraft are compact, simple vessels";
        }
        
        if (shipCategory.Contains("freighter") || shipCategory.Contains("cargo"))
        {
            return @"FREIGHTER DESIGN RULES:
- BULKY, INDUSTRIAL shape - function over form
- Cargo containers, pods, or hold sections visible
- Heavy-duty engines for hauling mass
- Crane arms or cargo doors may be visible
- Weathered, working vessel appearance
- NO sleek military lines - this hauls cargo";
        }
        
        if (shipCategory.Contains("mining") || shipCategory.Contains("industrial"))
        {
            return @"INDUSTRIAL SHIP RULES:
- RUGGED, HEAVY-DUTY construction
- Mining/processing equipment visible
- Grappling arms, drills, or collection systems
- Ore holds or processing tanks
- Very utilitarian - built to work hard
- Weathered and well-used appearance";
        }
        
        if (shipCategory.Contains("transport") || shipCategory.Contains("passenger"))
        {
            return @"TRANSPORT DESIGN RULES:
- Passenger windows in rows along hull
- Comfortable, civilian aesthetic
- Boarding ramps or airlocks visible
- May have small observation deck
- Cruise ship or airline aesthetic
- NOT military, NOT industrial";
        }
        
        if (shipCategory.Contains("colony"))
        {
            return @"COLONY SHIP RULES:
- MASSIVE scale - a city in space
- Habitat domes or rings visible
- Agricultural/hydroponics sections
- Self-contained with all facilities
- Modular construction possible
- Generation ship aesthetic";
        }
        
        return "CIVILIAN RULES: No weapons, practical design, function-appropriate size.";
    }
    
    /// <summary>
    /// Get faction-specific civilian ship aesthetic
    /// </summary>
    private string GetFactionCivilianAesthetic(Faction faction)
    {
        return faction switch
        {
            Faction.Federation => @"FEDERATION CIVILIAN AESTHETIC:
- Clean, optimistic design but NOT military
- Earth-tones or white hull (not grey military)
- Starfleet-adjacent but clearly civilian
- Reference: Federation shuttles, freighters from DS9/TNG background ships
- NO SAUCER SECTIONS on non-passenger ships",

            Faction.Klingon => @"KLINGON CIVILIAN AESTHETIC:
- Still angular but less aggressive than warships
- Working vessels, transport ships, not Birds of Prey
- Dark colors: browns, rust, worn metal (not military green)
- Klingon house markings for ownership
- Practical, sturdy, built to last - Klingons don't waste resources
- NO wing positions, NO disruptor cannons visible",

            Faction.Romulan => @"ROMULAN CIVILIAN AESTHETIC:
- Still bird-like but less aggressive silhouette
- Green hull but lighter shade than military
- Merchant and transport vessels hide military capability
- Secret compartments implied in design
- Elegant but practical, secretive people even in commerce
- NO weapons visible, NO military Warbird features",

            Faction.Ferengi => @"FERENGI CIVILIAN AESTHETIC:
- Profit-oriented, cargo space maximized
- Orange/copper/gold coloring - shows wealth
- Advertisements and company logos on hull
- Trade vessel aesthetic, very commercial
- Gaudy, ostentatious - Ferengi flaunt success
- Ear-like design elements on larger vessels",

            Faction.Cardassian => @"CARDASSIAN CIVILIAN AESTHETIC:
- Industrial, utilitarian, no frills
- Mining and resource extraction focused
- Tan/brown/ochre industrial colors
- Occupation-era work vessel look - built to extract resources
- Still has dorsal spine motif but smaller
- NO weapons, purely functional",

            Faction.Borg => @"BORG CIVILIAN AESTHETIC:
- NOTE: Borg don't have 'civilian' ships - all serve the Collective
- Smaller geometric shapes (cubes, spheres, diamonds)
- Still covered in machinery and conduits
- Probe, scout, or harvester function
- Green glow, dark grey-green metal
- Terrifying even when not warships",

            Faction.Dominion => @"DOMINION CIVILIAN AESTHETIC:
- Organic, beetle-like shapes but smaller scale
- Purple/violet coloring maintained
- Vorta administrative vessels are sleeker
- Transport and supply focus
- Still alien and unsettling
- Grown/biological appearance even for cargo",

            Faction.Bajoran => @"BAJORAN CIVILIAN AESTHETIC:
- Mix of old and new technology
- SOLAR SAIL heritage visible on traditional ships
- Post-occupation reconstruction - repurposed military
- Spiritual elements: Prophets symbolism, temple-like
- Earth-tones, rust, orange, gold accents
- Lightship aesthetic for traditional vessels",

            Faction.Vulcan => @"VULCAN CIVILIAN AESTHETIC:
- Logical, efficient design - no wasted space
- Ring-shaped elements (Vulcan warp technology)
- Bronze, copper, rust-red desert tones
- Ancient yet advanced appearance
- Meditation ship aesthetics possible
- IDIC symbol incorporated subtly",

            Faction.Andorian => @"ANDORIAN CIVILIAN AESTHETIC:
- Ice-crystal inspired even on civilian ships
- Blue and white coloring, silver accents
- Less aggressive than military but still proud
- Antenna-like sensor arrays
- Cold environment adapted
- Trading vessels between empires",

            Faction.Trill => @"TRILL CIVILIAN AESTHETIC:
- Organic flowing curves - symbiont-inspired
- Teal, blue, soft purple coloring
- Scientific/academic focus
- Federation-adjacent design
- Elegant but practical
- Pool/water motifs possible",

            Faction.Gorn => @"GORN CIVILIAN AESTHETIC:
- Massive scale even for civilian ships
- Heavy construction, armored even when civilian
- Dark green/brown coloring
- Reptilian heat vents visible
- Slow, powerful, durable
- Rock-like or shell-like textures",

            Faction.Breen => @"BREEN CIVILIAN AESTHETIC:
- Still mysterious and asymmetric
- Refrigeration systems always present
- Teal/ice-blue crystalline elements
- Completely enclosed - no windows
- Alien and unknowable even as merchants
- Automated appearance",

            Faction.Tholian => @"THOLIAN CIVILIAN AESTHETIC:
- Crystalline construction
- Amber/orange/gold faceted surfaces
- Geometric angular shapes
- Extreme heat (450K+) adapted
- Web anchor points possible
- Gem-like appearance",

            Faction.Orion => @"ORION CIVILIAN AESTHETIC:
- Pirate/smuggler aesthetic even when 'civilian'
- Salvaged and modified appearance
- Green hull accents, dark colors
- Hidden compartments implied
- Fast and maneuverable design
- Criminal enterprise aesthetic",

            _ => "CIVILIAN AESTHETIC: Non-military, practical, functional design appropriate to the faction's culture. No weapons visible, utilitarian purpose."
        };
    }
    
    /// <summary>
    /// Get geometry for civilian ships
    /// </summary>
    private string GetCivilianShipGeometry(string shipName, Faction faction)
    {
        var lowerName = shipName.ToLower();
        
        // Federation shuttles
        if (faction == Faction.Federation)
        {
            if (lowerName.Contains("type 6") || lowerName.Contains("type-6"))
                return "Star Trek TYPE 6 SHUTTLECRAFT: Boxy rectangular shuttle, flat top, angled windows at front. Small, fits in shuttle bay. Accurate to TNG design.";
            if (lowerName.Contains("type 7") || lowerName.Contains("type-7"))
                return "Star Trek TYPE 7 SHUTTLECRAFT: Slightly larger than Type 6, more rounded. Personnel shuttle. Accurate to TNG design.";
            if (lowerName.Contains("type 8") || lowerName.Contains("type-8"))
                return "Star Trek TYPE 8 SHUTTLECRAFT: Sleeker design, curved hull. Accurate to Voyager/DS9 design.";
            if (lowerName.Contains("type 9") || lowerName.Contains("type-9"))
                return "Star Trek TYPE 9 SHUTTLECRAFT: Aerodynamic, can land on planets. Voyager-era. Accurate to original design.";
            if (lowerName.Contains("type 10") || lowerName.Contains("type-10"))
                return "Star Trek TYPE 10 SHUTTLECRAFT: Heavy shuttle, larger. Can be armed. Accurate to TNG/DS9 design.";
            if (lowerName.Contains("type 11") || lowerName.Contains("type-11"))
                return "Star Trek TYPE 11 SHUTTLECRAFT: Captain's shuttle, sleeker personal transport. Accurate to Insurrection design.";
            if (lowerName.Contains("type 15") || lowerName.Contains("type-15") || lowerName.Contains("shuttlepod"))
                return "Star Trek TYPE 15 SHUTTLEPOD: Tiny two-person pod, very compact, minimal warp capability. Accurate to TNG design.";
            if (lowerName.Contains("runabout") || lowerName.Contains("danube"))
                return "Star Trek DANUBE CLASS RUNABOUT: Larger shuttle with modular design, boxy with cockpit at front. Roll bar on top. Reference: DS9 runabouts.";
            if (lowerName.Contains("delta flyer"))
                return "Star Trek DELTA FLYER: Sleek hot-rod shuttle with swept wings, pointed nose. Tom Paris design from Voyager.";
            if (lowerName.Contains("work bee"))
                return "Star Trek WORK BEE: Tiny yellow maintenance pod, one person, mechanical arms. Very small, no warp. Reference: TMP/TNG.";
            if (lowerName.Contains("travel pod"))
                return "Star Trek TRAVEL POD: Small orbital personnel pod, boxy with window. Short range only. Reference: TMP.";
            if (lowerName.Contains("argo"))
                return "Star Trek ARGO SHUTTLE: Heavy shuttle with vehicle bay, can carry ground vehicles. From Nemesis.";
            if (lowerName.Contains("yacht") || lowerName.Contains("captain"))
                return "Star Trek CAPTAIN'S YACHT: Sleek personal vessel that docks with a starship, usually under the saucer. Aerodynamic.";
        }
        
        // Klingon civilian
        if (faction == Faction.Klingon)
        {
            if (lowerName.Contains("toron") || lowerName.Contains("shuttle"))
                return "KLINGON SHUTTLE (Toron type): Angular but small, compact personnel transport. Dark metal, boxy. NOT a Bird of Prey - much smaller and simpler.";
            if (lowerName.Contains("transport") || lowerName.Contains("ferry"))
                return "KLINGON TRANSPORT: Boxy cargo/personnel vessel. Angular Klingon aesthetic but utilitarian. No wing configuration.";
            if (lowerName.Contains("freighter") || lowerName.Contains("hauler") || lowerName.Contains("cargo"))
                return "KLINGON FREIGHTER: Heavy cargo vessel, industrial. Angular but not predatory. Brown/rust coloring. Cargo pods visible.";
            if (lowerName.Contains("tanker") || lowerName.Contains("bloodwine"))
                return "KLINGON TANKER: Cylindrical or spherical tanks for liquid transport. Bloodwine or fuel. Industrial Klingon design.";
        }
        
        // Romulan civilian
        if (faction == Faction.Romulan)
        {
            if (lowerName.Contains("shuttle"))
                return "ROMULAN SHUTTLE: Small, sleek, bird-like but compact. Green hull. NOT a Warbird - much smaller civilian transport.";
            if (lowerName.Contains("transport") || lowerName.Contains("freighter"))
                return "ROMULAN TRANSPORT: Merchant vessel, still elegant bird-like lines but smaller. Green hull, no central void. Cargo space visible.";
            if (lowerName.Contains("science") || lowerName.Contains("research"))
                return "ROMULAN SCIENCE VESSEL: Sleek with sensor arrays. Green hull, elegant. Scientific equipment visible.";
        }
        
        // Cardassian civilian
        if (faction == Faction.Cardassian)
        {
            if (lowerName.Contains("shuttle"))
                return "CARDASSIAN SHUTTLE: Small, angular, spade-shaped but miniature. Tan/brown. Compact transport.";
            if (lowerName.Contains("freighter") || lowerName.Contains("groumall"))
                return "CARDASSIAN FREIGHTER (Groumall type): Cargo vessel with extended holds. Tan/brown, industrial. Reference: DS9 Groumall.";
            if (lowerName.Contains("ore") || lowerName.Contains("mining"))
                return "CARDASSIAN MINING SHIP: Industrial vessel for ore extraction. Processing equipment visible. Terok Nor supply ship aesthetic.";
        }
        
        // Ferengi civilian
        if (faction == Faction.Ferengi)
        {
            if (lowerName.Contains("shuttle") || lowerName.Contains("pod"))
                return "FERENGI SHUTTLE: Small, still crescent-influenced but compact. Orange/copper. Business transport.";
            if (lowerName.Contains("cargo") || lowerName.Contains("freighter"))
                return "FERENGI CARGO SHIP: Large cargo vessel, maximized hold space. Orange hull with trade company markings.";
            if (lowerName.Contains("marauder") || lowerName.Contains("d'kora"))
                return "This is a MILITARY ship - use military prompts instead.";
        }
        
        // Bajoran civilian
        if (faction == Faction.Bajoran)
        {
            if (lowerName.Contains("lightship") || lowerName.Contains("solar sail"))
                return "BAJORAN LIGHTSHIP: Ancient design with SOLAR SAILS. Golden/bronze sails catch tachyon eddies. Traditional wooden-ship-in-space aesthetic. Reference: DS9 'Explorers'.";
            if (lowerName.Contains("shuttle"))
                return "BAJORAN SHUTTLE: Small transport, post-occupation design. Earth-tones, practical. Resistance-era utilitarian.";
            if (lowerName.Contains("transport") || lowerName.Contains("freighter"))
                return "BAJORAN TRANSPORT: Cargo/passenger vessel. Mix of old Bajoran and salvaged Cardassian tech. Rust, orange, earth-tones.";
            if (lowerName.Contains("raider") || lowerName.Contains("interceptor"))
                return "This appears to be a MILITARY ship - use military prompts instead.";
        }
        
        // Vulcan civilian  
        if (faction == Faction.Vulcan)
        {
            if (lowerName.Contains("shuttle"))
                return "VULCAN SHUTTLE: Logical, efficient design. Ring-shaped warp element even on small craft. Bronze/copper coloring.";
            if (lowerName.Contains("transport") || lowerName.Contains("survey"))
                return "VULCAN SURVEY/TRANSPORT: Ring-nacelle configuration. Bronze hull. Scientific and efficient. Reference: Enterprise-era Vulcan ships.";
        }
        
        // Borg - no real civilian, but smaller vessels
        if (faction == Faction.Borg)
        {
            if (lowerName.Contains("probe") || lowerName.Contains("scout"))
                return "BORG PROBE: Small geometric shape (sphere or cube), still covered in machinery. Green glow. Scouts for assimilation targets.";
            if (lowerName.Contains("sphere"))
                return "BORG SPHERE: Perfect sphere covered in Borg machinery. Green glow elements. Can be small or large.";
        }
        
        // Dominion civilian
        if (faction == Faction.Dominion)
        {
            if (lowerName.Contains("transport") || lowerName.Contains("supply"))
                return "DOMINION TRANSPORT: Organic beetle-like shape but cargo-focused. Purple/violet. Grown appearance.";
            if (lowerName.Contains("fighter") || lowerName.Contains("attack"))
                return "This is a MILITARY ship - use military prompts instead.";
        }
        
        // Generic based on ship type name (any faction)
        if (lowerName.Contains("shuttle"))
            return "SHUTTLE: Small personnel transport, fits in shuttle bay. Compact, simple design. Few crew capacity.";
        if (lowerName.Contains("freighter") || lowerName.Contains("cargo"))
            return "FREIGHTER: Large cargo vessel with container modules or bulk holds. Boxy, industrial. NO military features.";
        if (lowerName.Contains("transport"))
            return "TRANSPORT: Passenger or cargo vessel, rows of windows if passenger type. Medium size, practical design.";
        if (lowerName.Contains("tanker"))
            return "TANKER: Large spherical or cylindrical fuel/liquid tanks connected by framework. Industrial.";
        if (lowerName.Contains("mining"))
            return "MINING SHIP: Heavy industrial vessel with mining equipment, collection arms, processing units. Rugged.";
        if (lowerName.Contains("colony"))
            return "COLONY SHIP: Massive vessel with habitat sections, agricultural domes, full life support for colonists.";
        if (lowerName.Contains("yacht") || lowerName.Contains("luxury"))
            return "YACHT: Sleek personal vessel, elegant, VIP transport. Luxury features visible.";
        if (lowerName.Contains("tug") || lowerName.Contains("salvage"))
            return "TUG/SALVAGE: Heavy-duty work vessel with grappling equipment, tow connections. Industrial.";
        
        return "";
    }
    
    /// <summary>
    /// Get faction-specific tactical/design notes
    /// </summary>
    private string GetFactionTacticalNote(Faction faction)
    {
        return faction switch
        {
            Faction.Federation => "FEDERATION DESIGN: Exploration-focused, well-rounded capabilities. Defensive shields strong, offensive moderate.",
            Faction.Klingon => "KLINGON TACTICAL: Klingon ships are armed FRONT AND REAR - they attack in all directions, never retreat. Show weapon emplacements both fore and aft.",
            Faction.Romulan => "ROMULAN DESIGN: Uses quantum singularity core (green glow), NOT traditional nacelles. Cloaking device aesthetic. Deceptive, ambush predator.",
            Faction.Borg => "BORG AESTHETIC: Pure function, no aesthetic consideration. Every surface covered in exposed machinery and conduits. Adaptive technology.",
            Faction.Cardassian => "CARDASSIAN DESIGN: No external nacelles. Forward-facing primary weapon. Military efficiency over aesthetics. Occupation-oriented.",
            Faction.Dominion => "DOMINION DESIGN: Organic/biological aesthetic. Purple/violet coloring. Grown as much as built. Jem'Hadar ships are fearless attack craft.",
            Faction.Ferengi => "FERENGI DESIGN: Commerce-focused. More cargo space than weapons. Shield-heavy for protecting goods. Not designed for war.",
            Faction.Breen => "BREEN DESIGN: Refrigeration systems integral. Energy dampening weapons. Completely mysterious technology. Asymmetric, alien design.",
            Faction.Gorn => "GORN DESIGN: Massive, heavily armored. Slow but extremely powerful. Built for their large crew. Reptilian heat requirements.",
            Faction.Vulcan => "VULCAN DESIGN: Ring/circular warp drive configuration. Logical, efficient, no wasted space. Bronze/copper coloring. Ancient yet advanced.",
            Faction.Andorian => "ANDORIAN DESIGN: NO SAUCER SECTIONS! Ships have ARROW/SPEAR-SHAPED hulls with SWEPT-BACK WING PYLONS like raptors. Blue-gray color. Imperial Guard military tradition.",
            Faction.Trill => "TRILL DESIGN: Federation-adjacent but unique. Symbiont-inspired organic curves. Scientific focus. Elegant exploration vessels.",
            Faction.Bajoran => "BAJORAN DESIGN: Spiritual aesthetic with solar-sail and lightship heritage. Resistance-era ships are utilitarian, post-occupation more elegant. Prophets symbolism.",
            Faction.Tholian => "THOLIAN DESIGN: Crystalline geometric shapes. Web-spinning capability. Extreme heat environments. Faceted surfaces like gems. Amber/orange coloring.",
            Faction.Orion => "ORION DESIGN: Pirate/raider aesthetic. Salvaged and modified ships. Green hull accents. Fast and maneuverable. Built for hit-and-run and smuggling.",
            _ => ""
        };
    }

    /// <summary>
    /// Get faction-specific warning text to prevent wrong designs
    /// </summary>
    private string GetFactionShipWarning(Faction faction)
    {
        return faction switch
        {
            Faction.Federation => "\n\nCRITICAL FEDERATION RULE: Federation ships have ONLY ONE SAUCER - never two saucers stacked or side by side.\n",
            Faction.Andorian => "\n\nCRITICAL ANDORIAN RULE: Andorian ships have ABSOLUTELY NO SAUCER SECTIONS! They are ARROW/SPEAR shaped with SWEPT-BACK WING PYLONS like birds of prey. If you are drawing a saucer or disc shape, STOP - that is WRONG for Andorian!\n",
            Faction.Klingon => "\n\nCRITICAL KLINGON RULE: Klingon ships are NOT all Bird of Prey! D7/K't'inga have HAMMERHEAD shape, Vor'cha is DAGGER shaped, Negh'Var is MASSIVE and WIDE.\n",
            Faction.Romulan => "\n\nCRITICAL ROMULAN RULE: Romulan warbirds have NO visible warp nacelles - propulsion is internal. D'deridex has DOUBLE HULL with negative space.\n",
            Faction.Vulcan => "\n\nCRITICAL VULCAN RULE: Vulcan ships have RING-SHAPED WARP DRIVES encircling the hull - NOT nacelles on pylons! Bronze/copper color.\n",
            Faction.Borg => "\n\nCRITICAL BORG RULE: Borg ships are PERFECT GEOMETRIC shapes - cubes, spheres, octahedrons. NO organic curves, NO windows.\n",
            Faction.Cardassian => "\n\nCRITICAL CARDASSIAN RULE: Cardassian ships are INSECTOID/COBRA shaped - pointed head, segmented body. Yellow-brown/ochre hull.\n",
            Faction.Dominion => "\n\nCRITICAL DOMINION RULE: Dominion ships look ORGANIC and INSECTOID - like alien beetles. Purple/violet hull. NOT mechanical.\n",
            Faction.Orion => "\n\nCRITICAL ORION RULE: Orion ships are CRESCENT/SICKLE shaped - curved like elegant blades. Dark hull with GREEN accent lighting.\n",
            _ => ""
        };
    }

    /// <summary>
    /// Get faction-specific negative prompt additions
    /// </summary>
    private string GetFactionNegativePrompt(Faction faction)
    {
        return faction switch
        {
            Faction.Federation => ", double saucer, two saucers",
            Faction.Andorian => ", saucer, saucer section, disc shape, circular hull, round primary hull, Federation design",
            Faction.Klingon => ", Federation saucer, round hull",
            Faction.Romulan => ", visible nacelles, Federation saucer",
            Faction.Vulcan => ", Federation saucer, nacelles on pylons",
            Faction.Borg => ", organic curves, windows, saucer",
            Faction.Cardassian => ", Federation saucer, round hull",
            Faction.Dominion => ", mechanical look, Federation saucer, angular design",
            Faction.Orion => ", Federation saucer, angular blocky design",
            Faction.Breen => ", symmetrical design, Federation saucer",
            Faction.Gorn => ", sleek design, Federation saucer",
            Faction.Tholian => ", organic curves, Federation saucer",
            _ => ""
        };
    }

    private string BuildStructurePrompt(FactionProfile profile, string structureName, bool isMilitary)
    {
        var structureType = isMilitary ? "military defense structure" : "civilian space structure";
        var designNote = isMilitary ? "defensive weapon emplacements, shield arrays, sensor dishes" : "docking ports, habitat modules, communication arrays";
        
        // Get faction-specific structure style
        var factionStyle = GetFactionStructureStyle(profile.Faction, isMilitary);
        
        // Get specific structure geometry if known
        var structureGeometry = GetStructureGeometry(structureName, profile.Faction, isMilitary);
        var geometrySection = !string.IsNullOrEmpty(structureGeometry) 
            ? $"\n\nSTRUCTURE TYPE: {structureGeometry}\n" 
            : "";
        
        return $@"MANDATORY STYLE GUIDE:
Material & Texture: The space structure must look like a handmade physical model made of plasticine clay with a subtle comic-like shader. Realistic metallic texture with a non-glossy, soft clay finish. High-end stop-motion animation quality (Laika/Aardman style). Very detailed technical elements.
Proportions (Crucial): Clearly STATIONARY structure - no engines, no movement capability. NOT A SHIP!
Lighting: Soft, cinematic studio lighting that emphasizes the clay texture and model quality. No harsh digital shine. Subtle glow from windows and power systems.
Camera/Perspective (CRITICAL - MUST FOLLOW EXACTLY):
- DIRECTION: Main entrance/front faces LOWER-LEFT
- Camera at TOP-RIGHT corner looking DOWN at structure
- Structure's FRONT = BOTTOM-LEFT of image
- Structure's BACK = TOP-RIGHT of image
- Visible: TOP surface + LEFT side of structure
- Like viewing from 2 o'clock position above
- Angle: 30-45 degrees above horizontal
- Structure centered in frame
Background: Solid black background (#000000).
Details: {designNote}, {profile.Name} faction markings, operational lighting.

{factionStyle}
{geometrySection}

Subject: Single {profile.Name} {structureType}, {structureName}.
Design Language: {profile.DesignLanguage} applied to stationary installation.
Color Scheme: {profile.ColorScheme}

CRITICAL RULES FOR SPACE STRUCTURES:
1. This is a STATIONARY STRUCTURE - NOT a ship!
2. NO warp nacelles, NO impulse engines, NO propulsion of any kind
3. NO saucer sections (unless specifically a ring-station)
4. Structures have DOCKING PORTS for ships to attach
5. May have solar panels, radiators, sensor arrays
6. Should look like it's built to STAY in one place

**single space station miniature model, rendered in high-quality stylized claymation 3D style, game asset, isometric view**
--no spaceship, no starship, no vessel, no nacelles, no engines, no warp drive, no saucer, funny, thumbprints, exaggerated features, moving vessel, multiple structures, grid, shiny plastic, CGI look, base, stand, table surface, tilt-shift, blurry background, frame, border, text, label, side view, front view";
    }
    
    /// <summary>
    /// Get specific geometry description for known structure types
    /// </summary>
    private string GetStructureGeometry(string structureName, Faction faction, bool isMilitary)
    {
        var lowerName = structureName.ToLower();
        
        // Federation structures
        if (faction == Faction.Federation)
        {
            if (lowerName.Contains("spacedock"))
                return "SPACEDOCK: Massive mushroom-shaped station. Large internal bay can hold multiple starships. Reference: Star Trek III Earth Spacedock. Doors open on the mushroom cap.";
            if (lowerName.Contains("starbase") && lowerName.Contains("1"))
                return "STARBASE ONE: Administrative starbase, mushroom-style with extended arms for docking. Multiple docking ports around the rim.";
            if (lowerName.Contains("deep space") || lowerName.Contains("ds9"))
                return "DEEP SPACE STATION: Ring-and-tower design like DS9 (originally Cardassian Terok Nor). Central core with docking pylons extending outward. Promenade ring.";
            if (lowerName.Contains("defense platform") || lowerName.Contains("weapon"))
                return "ORBITAL DEFENSE PLATFORM: Small, angular platform with visible phaser arrays and torpedo launchers. Solar panels for power. Minimal crew.";
            if (lowerName.Contains("drydock"))
                return "DRYDOCK: Open framework structure for ship construction/repair. Skeletal lattice with work arms and equipment. Ship would sit inside the frame.";
            if (lowerName.Contains("relay") || lowerName.Contains("communication"))
                return "SUBSPACE RELAY: Tall antenna-like structure with dish arrays. Automated, minimal habitation. Long-range communication boosting.";
            if (lowerName.Contains("research") || lowerName.Contains("science"))
                return "RESEARCH STATION: Modular cylindrical or spherical habitat. Multiple sensor arrays and antennas. Lab modules visible. Small crew.";
            if (lowerName.Contains("cargo") || lowerName.Contains("depot"))
                return "CARGO DEPOT: Container storage facility. Modular cargo pods attached to central spine. Crane arms for loading. Industrial appearance.";
        }
        
        // Klingon structures
        if (faction == Faction.Klingon)
        {
            if (lowerName.Contains("fortress") || lowerName.Contains("defense"))
                return "KLINGON ORBITAL FORTRESS: Aggressive angular design with multiple weapon arrays. Dark metal, red lights. Disruptor cannons and torpedo bays visible.";
            if (lowerName.Contains("shipyard"))
                return "KLINGON SHIPYARD: Heavy industrial construction yard. Ships being built in open frameworks. Brutal, functional design.";
        }
        
        // Cardassian structures  
        if (faction == Faction.Cardassian)
        {
            if (lowerName.Contains("terok") || lowerName.Contains("nor") || lowerName.Contains("deep space"))
                return "TEROK NOR STYLE: Ring station with central core and extending docking pylons. Ore processing aesthetic. Brown/tan coloring. Reference: DS9.";
            if (lowerName.Contains("mining") || lowerName.Contains("ore"))
                return "ORE PROCESSING STATION: Industrial facility for refining materials. Conveyor systems, storage tanks, processing units visible. Utilitarian.";
        }
        
        // Borg structures
        if (faction == Faction.Borg)
        {
            if (lowerName.Contains("transwarp"))
                return "TRANSWARP HUB: Massive geometric structure with multiple transwarp aperture openings. Covered in Borg machinery. Green glowing conduits.";
            if (lowerName.Contains("unicomplex"))
                return "UNICOMPLEX: Enormous Borg structure - city-sized. Multiple geometric shapes connected. Queen's central chamber. Assimilation processing.";
            return "BORG STRUCTURE: Geometric shape (cube, octahedron, or irregular) covered entirely in exposed machinery. Green glow. No windows. Terrifying.";
        }
        
        // Generic based on name
        if (lowerName.Contains("platform"))
            return "ORBITAL PLATFORM: Flat platform structure with equipment on top. Solar panels, sensor arrays, or weapons depending on purpose.";
        if (lowerName.Contains("station"))
            return "SPACE STATION: Habitable structure with docking facilities. Windows for crew areas. Power generation and life support visible.";
        if (lowerName.Contains("relay") || lowerName.Contains("array"))
            return "SENSOR/COMM ARRAY: Primarily antennas and dishes. Minimal habitation. Automated or small crew. Information gathering/transmission.";
        if (lowerName.Contains("depot") || lowerName.Contains("warehouse"))
            return "STORAGE DEPOT: Container modules and cargo holds. Loading/unloading equipment. Industrial, functional design.";
        if (lowerName.Contains("shipyard") || lowerName.Contains("drydock"))
            return "SHIPYARD/DRYDOCK: Open framework for ship construction. Crane arms, equipment platforms, raw materials storage.";
            
        return "";
    }
    
    private string GetFactionStructureStyle(Faction faction, bool isMilitary)
    {
        return faction switch
        {
            Faction.Federation => isMilitary 
                ? "FEDERATION MILITARY STRUCTURE: Space station designs - NOT ships! Options: Mushroom-shaped starbase (Spacedock), modular platform with docking arms, ring-shaped station, or orbital weapons platform. White/grey hull with blue accent lighting. Phaser arrays and shield emitters visible. NO warp nacelles, NO saucer sections - these are stationary installations."
                : "FEDERATION CIVILIAN STRUCTURE: Space station designs - NOT ships! Options: Ring-station with docking ports, cylinder habitat (O'Neill style), sphere with observation decks, or modular construction platform. Large windows, docking rings for visitors. Welcoming, optimistic architecture. Parks visible through windows. NO warp nacelles - stationary only.",
            
            Faction.Klingon => isMilitary
                ? "KLINGON MILITARY STRUCTURE: Fortress-like with sharp angular spires. Heavy disruptor cannons and torpedo launchers visible. Dark metal, red warning lights. Intimidating, brutal, built for war. Multiple weapon coverage angles."
                : "KLINGON CIVILIAN STRUCTURE: Still angular and imposing. Training facilities, arenas. Dark metals with torch-like lighting. Great Hall aesthetic in space. Honor symbols prominently displayed.",
            
            Faction.Romulan => isMilitary
                ? "ROMULAN MILITARY STRUCTURE: Bird-wing aesthetic even in stations. Green hull coloring, cloaking device emitters. Elegant but deadly. Hidden weapon systems. Singularity power core visible as green glow."
                : "ROMULAN CIVILIAN STRUCTURE: Secretive design, few windows. Green accents. Intelligence and surveillance aesthetic. Tal Shiar observation posts. Elegant but paranoid architecture.",
            
            Faction.Cardassian => isMilitary
                ? "CARDASSIAN MILITARY STRUCTURE: Blocky, utilitarian, oppressive. Dorsal spines even on stations. Brown/tan coloring. Interrogation facilities, prison sections. Terok Nor style. Ore processing industrial look."
                : "CARDASSIAN CIVILIAN STRUCTURE: Still utilitarian and drab. Military efficiency in civilian spaces. Resource extraction facilities. Labor camp aesthetic. Minimal comfort considerations.",
            
            Faction.Borg => 
                "BORG STRUCTURE: Geometric shape (cube, octahedron, sphere segment). Entirely covered in exposed conduits, machinery, pipes. Green glowing elements. Assimilation chambers. No windows. Transwarp hub aesthetic. Technological nightmare.",
            
            Faction.Ferengi =>
                "FERENGI STRUCTURE: Designed for commerce. Docking for many ships. Advertisement displays. Vault/treasury sections. Gold and orange accents. Holosuites, gambling, commerce. Tower of Commerce style.",
            
            Faction.Dominion => isMilitary
                ? "DOMINION MILITARY STRUCTURE: Organic, grown appearance. Purple/violet coloring. Ketracel-white production facilities. Jem'Hadar barracks. Bioorganic technology. Hive-like internal structure."
                : "DOMINION CIVILIAN STRUCTURE: Vorta administrative aesthetic. Still organic-looking. Founder worship temples. Communication relays. Purple bioluminescence. Grown rather than built appearance.",
            
            Faction.Vulcan => isMilitary
                ? "VULCAN MILITARY STRUCTURE: Logical, efficient design. Ring-shaped elements. Bronze/copper coloring. Defense platforms with minimal aggression aesthetic. Logic dictates preparedness."
                : "VULCAN CIVILIAN STRUCTURE: Temple-like meditation aesthetic. Desert-adapted with heat radiators. Mount Seleya inspiration. Surak teachings inscribed. IDIC symbols. Ancient wisdom meets advanced technology.",
            
            Faction.Andorian => isMilitary
                ? "ANDORIAN MILITARY STRUCTURE: Imperial Guard aesthetic. Ice-crystal inspired architecture. Aggressive weapon platforms. Blue/white coloring. Antenna-array sensor systems. Warrior culture pride."
                : "ANDORIAN CIVILIAN STRUCTURE: Crystalline ice architecture. Cold environment adapted. Underground elements. Clan gathering halls in space. Blue lighting. Trading posts between Federation and Empire.",
            
            Faction.Trill => isMilitary
                ? "TRILL MILITARY STRUCTURE: Federation-style but organic curves. Symbiont-inspired flowing design. Science-focused even in military. Teal/blue coloring. Joined/Unjoined crew facilities."
                : "TRILL CIVILIAN STRUCTURE: Symbiosis Commission aesthetic. Pool-like symbiont habitats visible. Academic/scientific focus. Elegant curves. Records of past hosts. Zhian'tara chambers.",
            
            Faction.Bajoran => isMilitary
                ? "BAJORAN MILITARY STRUCTURE: Resistance-era utilitarian mixed with Prophets aesthetic. Orange/gold coloring. Orb chambers for religious significance. Post-occupation rebuilding visible. DS9-influenced."
                : "BAJORAN CIVILIAN STRUCTURE: Temple of the Prophets aesthetic. Orb arks and shrines. Earring-decoration motifs. Vedek Assembly halls. Agricultural spirituality. Kai residence elegance.",
            
            Faction.Breen => isMilitary
                ? "BREEN MILITARY STRUCTURE: Refrigeration-based architecture. Completely enclosed. Teal crystalline elements. Energy dampening systems visible. Mysterious, no windows. Asymmetric alien design."
                : "BREEN CIVILIAN STRUCTURE: Still mysterious and enclosed. Cold storage facilities. Crystalline ice-like elements. No visible inhabitants ever. Automated systems. Unknown internal layout.",
            
            Faction.Gorn => isMilitary
                ? "GORN MILITARY STRUCTURE: Massive scale for large reptilian crew. Cave-like interiors implied. Heavy armor plating. Dark green coloring. Heating elements visible. Brutalist powerful construction."
                : "GORN CIVILIAN STRUCTURE: Still massive scale. Rock-integrated design. Basking platforms. Egg-chamber aesthetic for hatcheries. Natural materials integrated. Ancient reptilian culture.",
            
            Faction.Tholian => 
                "THOLIAN STRUCTURE: Crystalline geometric construction. Faceted surfaces like cut gems. Amber/orange/gold coloring. Extreme heat environment (450K+). Web anchor points. No traditional windows - crystalline walls. Hexagonal patterns.",
            
            Faction.Orion =>isMilitary
                ? "ORION MILITARY STRUCTURE: Pirate haven aesthetic. Hidden weapon emplacements. Syndicate base styling. Dark greens and golds. Defensive but also escape routes. Black market hidden docking."
                : "ORION CIVILIAN STRUCTURE: Pleasure palace aesthetic. Casino/entertainment styling. Slave market areas (historical). Green and gold opulence. Trade hub. Decadent criminal enterprise look.",
            
            _ => ""
        };
    }
    
    private string BuildBuildingPrompt(FactionProfile profile, string buildingName)
    {
        var factionArchitecture = GetFactionBuildingStyle(profile.Faction, buildingName);

        // Try to get detailed building info from JSON manifest
        var buildingInfo = _buildingManifestService.GetBuilding(profile.Faction, buildingName);
        var buildingDescription = buildingInfo?.Description ?? string.Empty;
        var buildingCategory = buildingInfo?.Category ?? string.Empty;

        // Build the building-specific details section
        var buildingDetails = string.Empty;
        if (!string.IsNullOrEmpty(buildingDescription))
        {
            buildingDetails = $@"
BUILDING PURPOSE:
- Category: {buildingCategory}
- Function: {buildingDescription}
- Design the building to clearly reflect this purpose in its architecture.";
        }

        return $@"MANDATORY STYLE GUIDE:
Material & Texture: The building must look like a handmade physical model made of plasticine clay with a subtle comic-like shader. Realistic architectural texture with a non-glossy, soft clay finish. High-end stop-motion animation quality (Laika/Aardman style). Very detailed windows, doors, and structural elements.
Proportions (Crucial): Proper architectural proportions for the building type.
Lighting: Soft, cinematic studio lighting that emphasizes the clay texture. No harsh digital shine. Subtle glow from windows.
Camera/Perspective (CRITICAL - MUST FOLLOW EXACTLY):
- DIRECTION: Building entrance faces LOWER-LEFT
- Camera at TOP-RIGHT corner looking DOWN at building
- Building's FRONT ENTRANCE = BOTTOM-LEFT of image
- Building's BACK = TOP-RIGHT of image
- Visible: ROOF/TOP + FRONT facade + LEFT side
- Like viewing from 2 o'clock position above
- Angle: 30-45 degrees above horizontal
- Building centered in frame
Background: Solid black background (#000000).
Details: Faction-appropriate architectural details, glowing windows, entry points, subtle faction insignia.

{factionArchitecture}
{buildingDetails}

Subject: Single {profile.Name} planetary building, {buildingName}.
Architecture Style: {profile.Architecture}
Color Scheme: {profile.ColorScheme}

CRITICAL: Building must clearly belong to {profile.Name} faction. Architecture should be immediately recognizable.

**single building miniature model, rendered in high-quality stylized claymation 3D style, tabletop game piece, isometric view**
--no funny, thumbprints, exaggerated features, multiple buildings, city, landscape, ground texture, grass, terrain, grid, shiny plastic, CGI look, base, stand, tilt-shift, blurry background, frame, border, text, label, birds eye view, top-down only, side view only";
    }
    
    private string GetFactionBuildingStyle(Faction faction, string buildingName)
    {
        var buildingLower = buildingName.ToLower();
        var isMilitary = buildingLower.Contains("barracks") || buildingLower.Contains("weapons") || 
                         buildingLower.Contains("defense") || buildingLower.Contains("military") ||
                         buildingLower.Contains("fortress") || buildingLower.Contains("arsenal");
        
        return faction switch
        {
            Faction.Federation => $@"FEDERATION ARCHITECTURE: 
- Utopian, optimistic futurism with curved glass domes and organic flowing lines
- White/beige walls with large transparent aluminum windows
- Lush interior gardens visible through windows
- Clean energy (no smokestacks), solar panels, fusion reactors integrated elegantly
- Starfleet delta emblems on official buildings
- San Francisco / Vulcan temple influenced design
- GROUND-BASED building - no spaceship elements, no nacelles, no saucer sections!
- {(isMilitary ? "Military buildings still elegant but reinforced, phaser turrets disguised as architecture" : "Welcoming, open, park-like surroundings implied")}",

            Faction.Klingon => $@"KLINGON ARCHITECTURE:
- Fortress-like brutal construction, sharp angular spires reaching upward
- Dark stone and metal, bronze/copper accents
- Torches and braziers for lighting (even if decorative)
- Bat'leth and warrior motifs in architecture
- Great Hall style - vaulted ceilings, combat arenas
- Statues of Kahless and legendary warriors
- {(isMilitary ? "Heavy fortification, weapon racks visible, training grounds" : "Still imposing - even homes look like small fortresses")}
- Targ pens and hunting trophy displays",

            Faction.Romulan => $@"ROMULAN ARCHITECTURE:
- Elegant but secretive, few windows, surveillance aesthetic
- Green marble and dark metals, bird-wing motifs
- Roman/Byzantine influenced but more angular
- Hidden rooms and passages implied in design
- Tal Shiar observation towers, sensor arrays disguised as decoration
- Raptor/bird statues and emblems
- {(isMilitary ? "Cloaked weapon emplacements, prison/interrogation aesthetic" : "Paranoid design - defensible even civilian buildings")}",

            Faction.Cardassian => $@"CARDASSIAN ARCHITECTURE:
- Utilitarian, industrial, oppressive atmosphere
- Brown/tan/grey coloring, minimal decoration
- Oval windows, ribbed exterior walls
- Terok Nor / Deep Space 9 aesthetic
- Ore processing and resource extraction facilities
- Central Archon prominence, Obsidian Order surveillance
- {(isMilitary ? "Prison camp aesthetic, interrogation chambers, labor facilities" : "Still drab and functional, comfort is secondary to efficiency")}
- Dorsal ridge motif repeated in building spines",

            Faction.Borg => $@"BORG ARCHITECTURE:
- NOT traditional buildings - ASSIMILATION COMPLEXES
- Geometric base shapes covered entirely in machinery, conduits, pipes
- Green glowing elements, regeneration alcoves visible
- No windows, no aesthetic consideration
- Looks like circuit boards and server rooms made architectural
- Maturation chambers, vinculum nodes
- Dark grey-green metal, completely utilitarian
- Technological horror - buildings feel alive and threatening",

            Faction.Ferengi => $@"FERENGI ARCHITECTURE:
- Commerce-focused, ostentatious displays of wealth
- Gold, orange, copper accents everywhere
- Tower of Commerce style - tall, attention-grabbing
- Vault doors, secure storage, counting houses
- Advertisement displays, product showcases
- Ear-shaped architectural motifs (subtle)
- {(isMilitary ? "Mercenary compounds, weapons dealers, security vaults" : "Shops, holosuites, gambling dens, auction houses")}
- Grand Nagus portraits and Rules of Acquisition displayed",

            Faction.Dominion => $@"DOMINION ARCHITECTURE:
- Organic, bioengineered appearance - GROWN not built
- Purple/violet coloring with bioluminescent accents
- Curved, flowing forms like shells or carapaces
- Ketracel-white dispensaries for Jem'Hadar facilities
- Founder worship shrines (shapeshifter pools)
- Vorta administrative elegance mixed with Jem'Hadar brutality
- {(isMilitary ? "Hatcheries, cloning facilities, warrior barracks" : "Still alien and unsettling, organic technology")}",

            Faction.Vulcan => $@"VULCAN ARCHITECTURE:
- Desert-temple inspired, Mount Seleya aesthetic
- Bronze, copper, rust-red coloring (Vulcan's Forge desert tones)
- Logical geometric forms, no wasted space
- Meditation chambers, kolinahr monasteries
- IDIC symbol incorporated into design
- Ancient stone combined with advanced technology
- {(isMilitary ? "Logic dictates defense - minimal but effective fortification" : "Academies, libraries, meditation retreats")}
- Surak statues and teachings inscribed",

            Faction.Andorian => $@"ANDORIAN ARCHITECTURE:
- Ice-crystal inspired, cold environment adapted
- Blue and white coloring, silver accents
- Antenna-like spires reaching upward
- Underground components (Andorian cities are underground)
- Imperial Guard militarism evident even in civilian buildings
- {(isMilitary ? "Aggressive fortifications, training grounds, dueling arenas" : "Trading posts, science facilities, diplomatic halls")}
- Passionate warrior culture aesthetic",

            Faction.Trill => $@"TRILL ARCHITECTURE:
- Symbiont-inspired organic curves and flowing design
- Teal, blue, soft purple coloring
- Pool-like elements (symbiont pools visible)
- Symbiosis Commission institutional aesthetic
- Records halls for past host memories
- Scientific/academic focus
- {(isMilitary ? "Defense integrated subtly, not aggressive" : "Universities, hospitals, joining facilities")}",

            Faction.Bajoran => $@"BAJORAN ARCHITECTURE:
- Temple of the Prophets aesthetic
- Orange, gold, brown earth tones
- Orb arks and shrine chambers
- Earring-decoration motifs in architecture
- Post-occupation rebuilding - spirituality meets hope
- {(isMilitary ? "Resistance-era bunkers, utilitarian but proud" : "Monasteries, vedek assembly, kai residence")}
- Celestial Temple imagery, Prophets symbolism",

            Faction.Breen => @"BREEN ARCHITECTURE:
- Refrigeration aesthetic - ice-crystal inspired, cold materials
- Teal and ice-blue coloring
- Mysterious, enclosed, no exposed inhabitants
- Cooling systems and cryogenic elements visible
- Asymmetrical, alien design unlike Alpha Quadrant species",

            Faction.Gorn => @"GORN ARCHITECTURE:
- Massive, heavy construction for large reptilian species
- Cave-like interiors, natural rock integrated
- Dark green/brown coloring
- Brutalist, powerful, intimidating
- Heating elements (cold-blooded species)",

            Faction.Tholian => @"THOLIAN ARCHITECTURE:
- Crystalline construction - buildings look like cut gems
- Faceted surfaces, geometric precision
- Amber, orange, gold coloring, translucent elements
- Extreme heat environment (interior is 450+ Kelvin)
- Hexagonal and triangular patterns dominate
- Web-like connecting structures between buildings
- No windows in traditional sense - crystalline walls
- Completely alien, non-humanoid design aesthetic",

            Faction.Orion => $@"ORION ARCHITECTURE:
- Decadent pleasure palace aesthetic
- Green-skinned culture reflected in green/gold colors
- Syndicate headquarters style - hidden rooms, escape routes
- Slave market areas (historical/criminal enterprise)
- Casino, entertainment hall, dance floor aesthetics
- Vault and counting house for wealth storage
- {(isMilitary ? "Pirate haven, hidden weapons, defensible" : "Opulent, gaudy displays of wealth")}
- Black market aesthetic - secret dealings
- Mixed styles from stolen/salvaged goods",

            _ => ""
        };
    }
    
    private string BuildTroopPrompt(FactionProfile profile, string unitName)
    {
        var factionTroopStyle = GetFactionTroopStyle(profile.Faction, unitName);
        
        return $@"MANDATORY STYLE GUIDE:
Material & Texture: The unit must look like a handmade physical model made of plasticine clay with a subtle comic-like shader. Realistic texture with a non-glossy, soft clay finish. High-end stop-motion animation quality (Laika/Aardman style). Detailed equipment and armor.
Proportions (Crucial): Stylized but near-realistic proportions. NOT chibi or oversized head.
Lighting: Soft, cinematic studio lighting that emphasizes the clay texture. No harsh digital shine.
Camera/Perspective (CRITICAL - MUST FOLLOW EXACTLY):
- DIRECTION: Unit faces LOWER-LEFT, looking toward camera
- Camera at TOP-RIGHT corner looking DOWN at unit
- Unit's FRONT/FACE = toward BOTTOM-LEFT of image
- Unit's BACK = toward TOP-RIGHT of image
- Visible: FRONT + LEFT side of unit
- Full body visible from head to feet
- Like viewing from 2 o'clock position above
- Angle: 30-45 degrees above horizontal
- Unit centered in frame
Background: Solid black background (#000000).
Details: Faction-appropriate military equipment, weapons, armor design.

{factionTroopStyle}

Subject: Single {profile.Name} ground unit, {unitName}.
Species Appearance: {profile.RaceFeatures}
Design Language: {profile.DesignLanguage} military ground forces aesthetic.
Color Scheme: {profile.ColorScheme} military variant.

CRITICAL: Troop must be clearly {profile.Name} - species features and faction equipment must be correct.

**single military unit miniature model, rendered in high-quality stylized claymation 3D style, wargame piece, isometric view**
--no funny, thumbprints, exaggerated features, oversized head, multiple units, squad, grid, shiny plastic, CGI look, base, stand, table surface, tilt-shift, blurry background, frame, border, text, label, top-down view, birds eye, back view";
    }
    
    private string GetFactionTroopStyle(Faction faction, string unitName)
    {
        var unitLower = unitName.ToLower();
        var isHeavy = unitLower.Contains("heavy") || unitLower.Contains("elite") || unitLower.Contains("tank");
        var isOfficer = unitLower.Contains("officer") || unitLower.Contains("commander") || unitLower.Contains("leader") || unitLower.Contains("chief");
        
        return faction switch
        {
            Faction.Federation => $@"FEDERATION GROUND FORCES:
- Starfleet uniform or MACO tactical armor
- Black base with department color (gold for security/tactical)
- Phaser rifle (Type-3) or hand phaser, NOT ballistic weapons
- Tricorder on belt, communicator badge
- Protective vest/armor for combat units, but NOT full medieval plate
- Clean, professional military appearance
- {(isOfficer ? "Command insignia visible, fewer weapons, more tricorder" : "")}
- {(isHeavy ? "Hazard Team style heavy armor, helmet with visor" : "")}
- Humans, Vulcans, Andorians, Bolians etc. in Starfleet service",

            Faction.Klingon => $@"KLINGON WARRIORS:
- Klingon with prominent forehead ridges, fierce expression
- Dark leather and metal armor, NOT Starfleet style
- Bat'leth (curved sword) AND disruptor pistol - Klingons use BOTH melee and ranged
- Baldric/sash across chest with House insignia
- War paint or battle scars
- Long dark hair, often braided
- {(isOfficer ? "More ornate armor, cape, command sash" : "")}
- {(isHeavy ? "Full battle armor, larger bat'leth, heavier disruptor" : "")}
- Aggressive stance, ready for combat - Klingons don't stand at parade rest",

            Faction.Romulan => $@"ROMULAN MILITARY:
- Romulan with V-shaped forehead ridge, pointed ears, bowl-cut hair
- Grey/green padded uniform with shoulder armor
- Disruptor rifle, NOT phaser
- Tal Shiar operatives in darker, more sinister uniforms
- Calculating expression, not emotional
- Bird-of-prey insignia on uniform
- {(isOfficer ? "Centurion or Commander rank insignia, cape" : "")}
- {(isHeavy ? "Full body armor, heavier weapons, shock trooper" : "")}
- Disciplined military posture but paranoid awareness",

            Faction.Cardassian => $@"CARDASSIAN SOLDIERS:
- Cardassian with grey scaly skin, neck ridges, spoon-shaped forehead
- Grey/brown armor with ribbed texture
- Phaser rifle Cardassian design (more angular)
- Utilitarian, efficient, professional but ruthless
- Obsidian Order operatives in darker civilian clothes
- {(isOfficer ? "Gul or Legate rank armor, more ornate" : "")}
- {(isHeavy ? "Occupation force heavy armor, interrogation equipment" : "")}
- Cold, calculating expression - they enjoy their work",

            Faction.Borg => $@"BORG DRONES:
- Assimilated humanoid with visible cybernetic implants
- Pale grey skin, one eye replaced with laser/sensor
- Mechanical arm with assimilation tubules
- Exoskeleton components, tubes connecting to body
- NO individual expression - blank, emotionless
- Black bodysuit with mechanical components attached
- Collective identification numbers, NOT names
- {(isHeavy ? "Tactical drone with more weapons, heavier armor plating" : "")}
- Walks with mechanical precision, inhuman movement implied",

            Faction.Ferengi => $@"FERENGI COMBATANTS:
- Ferengi with large ears, sharp teeth, orange-brown skin
- NOT natural warriors - prefer mercenaries or technology
- Energy whip or small phaser pistol
- Profit-motivated - carrying latinum or trade goods
- Gaudy, ostentatious even in combat gear
- {(isOfficer ? "Daimon rank, more gold accessories" : "")}
- Cowardly but cunning expression
- Would rather negotiate than fight",

            Faction.Dominion => $@"JEM'HADAR SOLDIERS:
- Jem'Hadar with grey reptilian skin, bony facial ridges, scaled texture
- Purple/grey battle armor, organic-looking
- Ketracel-white tube visible (they're addicted, need it to live)
- Kar'takin (polearm blade) AND plasma rifle
- Bred for combat, utterly fearless
- {(isOfficer ? "First, Second rank markings, more ornate armor" : "")}
- {(isHeavy ? "Jem'Hadar Honored Elder, heavier armor, veteran" : "")}
- Fierce, focused, 'Victory is life' mentality
- OR Vorta administrator - pale, elegant, manipulative expression",

            Faction.Breen => @"BREEN SOLDIERS:
- FULLY ENCLOSED refrigeration suit - NO visible face/body
- Snout-like helmet with vocoder
- Metallic green-grey suit
- Energy dampening weapon (unique to Breen)
- Completely mysterious - never seen outside suit
- Mechanical, alien body language",

            Faction.Gorn => @"GORN WARRIORS:
- Large reptilian humanoid, green scaly skin
- Powerful build, much larger than human
- Slow but incredibly strong
- Minimal armor - tough hide
- Heavy weapons to match their strength
- Cold-blooded predator expression, multifaceted eyes",

            Faction.Vulcan => $@"VULCAN SECURITY/MILITARY:
- Vulcan with pointed ears, upswept eyebrows
- Completely emotionless, logical expression
- Pale to olive skin, slight green tint
- V'Shar (Vulcan Security) uniform or Starfleet
- Phaser or traditional lirpa/ahn-woon for ceremonial
- {(isOfficer ? "Elder robes, more formal" : "")}
- Nerve pinch hand position implied
- Pacifist but capable - logic permits self-defense",

            Faction.Andorian => $@"ANDORIAN IMPERIAL GUARD:
- Blue skin, white hair, TWO ANTENNAE on forehead
- Aggressive, passionate warrior expression
- Antennae position shows emotion (forward = aggressive)
- Imperial Guard blue/white armor
- Ushaan-tor (ice-pick weapon) or phaser rifle
- {(isOfficer ? "Commander insignia, more ornate armor" : "")}
- {(isHeavy ? "Heavy combat armor, larger weapons" : "")}
- Fierce warrior culture - honorable but hot-tempered",

            Faction.Trill => $@"TRILL PERSONNEL:
- Humanoid with LEOPARD-SPOT pattern along temples/neck
- Spots run from forehead down sides of face to body
- Can be Joined (with symbiont) or Unjoined
- Trill Defense Force uniform or Starfleet variant
- Scientific equipment as likely as weapons
- {(isOfficer ? "Multiple lifetimes of experience implied" : "")}
- Thoughtful, wise expression
- Symbiont pouch visible on Joined",

            Faction.Bajoran => $@"BAJORAN MILITARY/MILITIA:
- Humanoid with distinctive NOSE RIDGES (wrinkled nose bridge)
- Bajoran earring on right ear (d'ja pagh)
- Resistance-era: Rugged, guerrilla fighter aesthetic
- Post-occupation: Militia uniform, orange/brown
- Phaser rifle or resistance-era projectile weapons
- {(isOfficer ? "Militia commander, vedek if religious" : "")}
- Spiritual yet determined expression
- Pagh (life force) mentioned in gestures",

            _ => ""
        };
    }
    
    private string BuildPortraitPrompt(FactionProfile profile, string variantDescription)
    {
        var factionPortraitStyle = GetFactionPortraitStyle(profile.Faction, variantDescription);
        var uniformDescription = GetFactionUniformDescription(profile.Faction, variantDescription);
        
        return $@"MANDATORY STYLE GUIDE:
Material & Texture: The character must look like a handmade physical model made of plasticine clay with a subtle comic-like shader. Realistic skin texture with a non-glossy, soft clay finish. High-end stop-motion animation quality (Laika/Aardman style).
Proportions (Crucial): Stylized but near-realistic proportions. Do NOT use excessive caricature or oversized head.
Lighting: Soft, cinematic studio lighting that emphasizes the clay texture (subsurface scattering). No harsh digital shine.
Expression: Serious, stoic, characteristic of the species.
Composition: Frontal view or 3/4 view, looking at camera.
Background: Solid black background (#000000).

{factionPortraitStyle}

UNIFORM/CLOTHING DETAILS:
{uniformDescription}

Subject: {profile.Name} character portrait, {variantDescription}.
Species Features: {profile.RaceFeatures}

CRITICAL: Character must be clearly recognizable as {profile.Name}. Species features, uniform details, and faction styling must be accurate to Star Trek canon.

**medium shot character portrait, rendered in high-quality stylized claymation 3D style, game character portrait**
--no funny, thumbprints, exaggerated features, oversized head, disproportionate, shiny skin, wet skin, CGI plastic look, action figure joints, miniature base, table surface, tilt-shift, blurry background, frame, border, text, card layout";
    }
    
    private string GetFactionUniformDescription(Faction faction, string variantDescription)
    {
        var variantLower = variantDescription.ToLower();
        var isMilitary = variantLower.Contains("officer") || variantLower.Contains("soldier") || 
                         variantLower.Contains("warrior") || variantLower.Contains("guard") ||
                         variantLower.Contains("tactical") || variantLower.Contains("security");
        var isAdmiral = variantLower.Contains("admiral") || variantLower.Contains("general") || 
                        variantLower.Contains("commander") || variantLower.Contains("captain");
        var isCivilian = variantLower.Contains("civilian") || variantLower.Contains("merchant") || 
                         variantLower.Contains("trader");
        var isScience = variantLower.Contains("scientist") || variantLower.Contains("doctor") ||
                        variantLower.Contains("medical") || variantLower.Contains("science");
        var isEngineer = variantLower.Contains("engineer") || variantLower.Contains("technician");
        
        return faction switch
        {
            Faction.Federation => GetStarfleetUniform(variantLower, isAdmiral, isScience, isEngineer, isCivilian),
            Faction.Klingon => GetKlingonArmor(variantLower, isAdmiral, isCivilian),
            Faction.Romulan => GetRomulanUniform(variantLower, isAdmiral, isCivilian),
            Faction.Cardassian => GetCardassianUniform(variantLower, isAdmiral, isCivilian),
            Faction.Borg => GetBorgAppearance(variantLower),
            Faction.Ferengi => GetFerengiAttire(variantLower, isAdmiral),
            Faction.Dominion => GetDominionUniform(variantLower),
            Faction.Breen => GetBreenSuit(),
            Faction.Gorn => GetGornAttire(variantLower),
            Faction.Vulcan => GetVulcanAttire(variantLower, isMilitary, isCivilian),
            Faction.Andorian => GetAndorianAttire(variantLower, isMilitary),
            Faction.Trill => GetTrillAttire(variantLower, isMilitary),
            Faction.Bajoran => GetBajoranAttire(variantLower, isMilitary),
            Faction.Orion => GetOrionAttire(variantLower),
            _ => "Faction-appropriate clothing and equipment."
        };
    }
    
    private string GetStarfleetUniform(string variant, bool isAdmiral, bool isScience, bool isEngineer, bool isCivilian)
    {
        if (isCivilian)
        {
            return @"FEDERATION CIVILIAN CLOTHING:
- Comfortable, casual 24th century fashion
- Soft fabrics, muted colors (beige, grey, soft blue)
- No uniform elements, no combadge
- Simple tunic or sweater style top
- Practical but comfortable";
        }
        
        var departmentColor = isScience ? "BLUE (Sciences/Medical)" : 
                              isEngineer ? "GOLD/YELLOW (Operations/Engineering)" : 
                              "RED (Command/Tactical)";
        
        if (isAdmiral)
        {
            return $@"STARFLEET ADMIRAL UNIFORM (TNG/DS9 ERA):
- Black jumpsuit base with quilted texture on shoulders
- {departmentColor} department color on shoulders and upper chest
- FOUR gold rank pips on right collar (Admiral)
- Combadge: Gold delta shield on silver oval backing, LEFT chest
- Grey undershirt collar visible at neck
- Admiral variant may have additional gold piping
- Polished, well-maintained, authoritative appearance
- May include dress uniform jacket for formal occasions";
        }
        
        return $@"STARFLEET DUTY UNIFORM (TNG/DS9 ERA):
- Black jumpsuit base with quilted/ribbed texture on shoulders
- {departmentColor} department color on shoulders wrapping to upper chest
- Rank pips: Small gold/black dots on RIGHT collar (1-4 pips for Ensign to Captain)
- Combadge: Gold delta/chevron shield on silver oval backing, positioned on LEFT chest
- Grey ribbed undershirt collar visible at neckline
- Uniform fits snugly, professional military appearance
- Black boots, no visible belt
- Fabric has subtle texture, not shiny
- Phaser may be holstered at hip for security personnel";
    }
    
    private string GetKlingonArmor(string variant, bool isGeneral, bool isCivilian)
    {
        if (isCivilian)
        {
            return @"KLINGON CIVILIAN ATTIRE:
- Dark leather tunic, still warrior-influenced
- House colors subtly displayed
- Simpler than warrior armor but still imposing
- May include ceremonial elements for noble houses
- Baldric sash still common for house affiliation";
        }
        
        if (isGeneral || variant.Contains("chancellor"))
        {
            return @"KLINGON HIGH COMMAND ARMOR:
- Ornate ceremonial battle armor, dark metal plates
- Heavy pauldrons (shoulder armor) with House emblems
- Chancellor's cloak or General's cape, dark red or black
- Multiple blade weapons visible: Bat'leth on back, d'k tahg at belt
- Elaborate baldric sash with medals and House insignia
- Gauntlets with blade attachments
- Ceremonial helmet optional
- Battle-worn but polished for status
- Trefoil Empire emblem prominent";
        }
        
        return @"KLINGON WARRIOR ARMOR (STANDARD):
- Layered dark leather and metal plate construction
- Chest plate: Overlapping metal segments, dark gunmetal/bronze
- Shoulder pauldrons: Curved metal plates, often asymmetrical
- Baldric sash: Leather strap diagonal across chest, House insignia attached
- Belt: Wide leather with d'k tahg dagger sheath
- Arm guards: Leather and metal vambraces
- Boots: Heavy, mid-calf, metal toe caps
- Colors: Dark grey, bronze, copper, blood red accents
- Battle damage and wear marks show experience
- Klingon trefoil symbol on belt buckle or chest
- Disruptor pistol holster on hip";
    }
    
    private string GetRomulanUniform(string variant, bool isCommander, bool isCivilian)
    {
        if (isCivilian)
        {
            return @"ROMULAN CIVILIAN ATTIRE:
- Elegant robes in muted greens and greys
- High collars, asymmetrical cuts
- Bird-of-prey motifs subtly woven into fabric
- Sophisticated but guarded appearance
- May include family house pins or jewelry";
        }
        
        if (variant.Contains("tal shiar") || variant.Contains("intelligence"))
        {
            return @"TAL SHIAR UNIFORM:
- All black leather uniform, more intimidating than standard
- Minimal insignia - secrecy is paramount
- Long black coat over fitted uniform
- Hidden weapons assumed
- No visible rank - they outrank everyone
- Cold, calculating, predatory appearance
- May have subtle Tal Shiar emblem (bird eye symbol)";
        }
        
        if (isCommander || variant.Contains("senator") || variant.Contains("praetor"))
        {
            return @"ROMULAN COMMAND/SENATE UNIFORM:
- Formal grey-green tunic with silver geometric trim
- High structured collar, padded shoulders with rank insignia
- Ceremonial sash for senators, metallic thread
- Bird-of-prey clasp at collar
- Elaborate shoulder boards for high ranks
- Commander's cape for formal occasions, deep green
- Polished, imperious, authoritative";
        }
        
        return @"ROMULAN MILITARY UNIFORM (STANDARD):
- Grey-green padded tunic, quilted texture
- High collar standing up, V-neck opening
- Shoulder pads: Pronounced, angular, with rank chevrons
- Belt: Wide, metallic buckle with bird emblem
- Pants: Same grey-green, tucked into boots
- Boots: Black, knee-high, polished
- Colors: Forest green, grey, silver trim
- Disruptor holster on hip
- Minimal decoration - efficiency over ostentation
- Subtle bird-of-prey emblem on chest or shoulder";
    }
    
    private string GetCardassianUniform(string variant, bool isGul, bool isCivilian)
    {
        if (isCivilian)
        {
            return @"CARDASSIAN CIVILIAN ATTIRE:
- Still military-influenced (Cardassian society is militarized)
- Grey/brown tunics with subtle ribbed texture
- High collars, structured shoulders
- Less armor but same aesthetic
- Obsidian Order civilians dress more plainly, blend in";
        }
        
        if (variant.Contains("obsidian") || variant.Contains("intelligence"))
        {
            return @"OBSIDIAN ORDER ATTIRE:
- Plain civilian clothing to blend in
- Or: Black variant of military uniform
- No visible rank or insignia
- Hidden weapons and surveillance equipment assumed
- Could be anyone, anywhere - that's the point
- Cold, observant expression";
        }
        
        if (isGul || variant.Contains("legate"))
        {
            return @"CARDASSIAN GUL/LEGATE ARMOR:
- Heavy grey chest armor with prominent neck guard
- Elaborate ribbed/scaled texture across entire uniform
- Rank insignia on collar and chest: larger geometric patterns
- Legate's ceremonial additions: gold/copper trim
- Cape or cloak for formal occasions
- Multiple weapon holsters
- Command authority evident in bearing";
        }
        
        return @"CARDASSIAN MILITARY UNIFORM:
- Grey/brown segmented armor, reptilian scale texture
- High neck guard rising behind head (distinctive!)
- Chest plate: Segmented, ribbed horizontal bands
- Shoulder armor: Curved plates over pronounced padding
- Spoon-shaped forehead echoed in armor contours
- Belt with disruptor holster
- Boots: Heavy, military, integrated into leg armor
- Colors: Tan, brown, grey, gunmetal
- Texture: Like reptile scales or insect carapace
- Cardassian Union emblem on chest plate";
    }
    
    private string GetBorgAppearance(string variant)
    {
        if (variant.Contains("queen"))
        {
            return @"BORG QUEEN APPEARANCE:
- Humanoid female torso, pale grey skin
- Elaborate mechanical head/spine assembly
- Skull cap with tubes and wires
- One eye organic, one with red laser implant
- Mechanical spine connects to body cradle
- Black bodysuit under mechanical components
- Seductive yet horrifying
- Individual personality unlike drones
- Dark lips, intense gaze";
        }
        
        return @"BORG DRONE APPEARANCE:
- Assimilated humanoid, original species sometimes visible
- Pale grey skin, unhealthy pallor
- Cybernetic implants covering 30-50% of face/body:
  * Eye replaced with red laser targeting implant
  * Skull implants, tubes entering head
  * Mechanical arm with assimilation tubules
  * Exoskeleton components on torso and limbs
- Black bodysuit, form-fitting
- Tubes and cables connecting implants
- NO individual uniform - just components
- Designation number, not name
- Completely blank expression";
    }
    
    private string GetFerengiAttire(string variant, bool isNagus)
    {
        if (isNagus || variant.Contains("nagus") || variant.Contains("daimon"))
        {
            return @"FERENGI GRAND NAGUS/DAIMON ATTIRE:
- Extremely ornate gold and orange robes
- Headdress with large decorative ears (emphasizing their own)
- Staff of the Grand Nagus (if Nagus)
- Multiple gold chains, rings, jewelry
- Latinum bars or coins visible
- Gaudy, ostentatious display of wealth
- Finest fabrics, gold thread
- Rules of Acquisition symbols on clothing";
        }
        
        return @"FERENGI STANDARD ATTIRE:
- Orange/copper colored jacket or tunic
- Exaggerated shoulder padding
- Gold accents and buttons
- Multiple pockets (for commerce)
- Headdress or skullcap common
- Gold jewelry: rings, earrings, chains
- Latinum purse or container visible
- Belt with trading tools
- Colors: Orange, gold, copper, rust
- Slightly gaudy but 'business professional' by Ferengi standards";
    }
    
    private string GetDominionUniform(string variant)
    {
        if (variant.Contains("vorta"))
        {
            return @"VORTA ADMINISTRATOR ATTIRE:
- Elegant purple/violet robes, flowing cut
- High collar, sophisticated styling
- No visible weapons (Vorta don't fight)
- Smug, diplomatic bearing
- Subtle Dominion insignia
- Colors: Purple, violet, lavender
- Fine fabrics, graceful movement
- Communication device on wrist";
        }
        
        if (variant.Contains("founder") || variant.Contains("changeling"))
        {
            return @"FOUNDER/CHANGELING APPEARANCE:
- Simple brown/tan robes when in humanoid form
- Or: No clothing (they ARE their form)
- Smooth, featureless when relaxed
- Orange-brown skin color
- May show partial morphing effect
- Serene, god-like expression
- No adornment needed - they are gods to Dominion";
        }
        
        // Default: Jem'Hadar
        return @"JEM'HADAR SOLDIER ARMOR:
- Purple/grey battle armor, organic-looking
- Chest plate: Curved, beetle-like carapace
- Shoulder armor: Segmented, insectoid
- Ketracel-white tube: Visible on neck/chest (CRITICAL - they need it to live)
- Helmet: Optional, ridged, skull-like
- Kar'takin polearm weapon or plasma rifle
- Colors: Purple, grey, dark violet
- Organic texture - grown not manufactured
- Battle-ready stance, fearless expression
- No rank insignia - all serve the Founders equally";
    }
    
    private string GetBreenSuit()
    {
        return @"BREEN REFRIGERATION SUIT:
- Full body environmental suit, NEVER removed
- Helmet: Snout-like protrusion with single visor slit
- Vocoder speaker for unintelligible language
- Metallic green-grey coloring
- Cooling systems visible as tubes and vents
- Body shape obscured - no one knows what's inside
- Weapon: Energy dampening rifle
- NO skin, face, or body ever visible
- Completely mysterious and alien";
    }
    
    private string GetGornAttire(string variant)
    {
        return @"GORN ATTIRE:
- Minimal clothing - tough reptilian hide provides protection
- Metallic harness or baldric across chest
- Belt with weapon holsters
- Arm bands showing rank or clan
- Optional: Light armor plates on shoulders
- Colors: Bronze, copper, dark green
- Emphasizes massive muscular build
- Weapon: Heavy disruptor or melee weapon
- Cold-blooded predator needs no protection from cold";
    }
    
    private string GetVulcanAttire(string variant, bool isMilitary, bool isCivilian)
    {
        if (variant.Contains("priest") || variant.Contains("kolinahr") || variant.Contains("elder"))
        {
            return @"VULCAN PRIEST/ELDER ROBES:
- Long flowing robes in bronze, rust-red, or sand colors
- IDIC symbol (triangle with circle) on medallion or brooch
- High collar, asymmetrical draping
- Hood optional for outdoor ceremonies
- Sandals or simple boots
- Meditation beads or prayer items
- Ancient, dignified styling
- Mt. Seleya temple aesthetic";
        }
        
        if (variant.Contains("starfleet") || isMilitary)
        {
            return @"VULCAN IN STARFLEET:
- Standard Starfleet uniform (see Federation description)
- Blue department color common (Sciences)
- IDIC pin sometimes worn alongside combadge
- Same calm, logical expression as civilian Vulcans
- Perfectly maintained uniform, precise appearance";
        }
        
        return @"VULCAN CIVILIAN ATTIRE:
- Simple, elegant robes in earth tones
- Bronze, rust, tan, grey colors
- Asymmetrical cut, one shoulder often draped
- IDIC symbol worn as jewelry or brooch
- High collars, clean lines
- Practical but dignified
- No ostentation - logic dictates simplicity
- Desert-appropriate fabrics";
    }
    
    private string GetAndorianAttire(string variant, bool isMilitary)
    {
        if (isMilitary || variant.Contains("guard") || variant.Contains("imperial"))
        {
            return @"ANDORIAN IMPERIAL GUARD ARMOR:
- Blue-tinted metal armor, ice-crystal aesthetic
- Chest plate with Andorian Empire symbol
- Shoulder pauldrons, sharp angular design
- Antenna-inspired helmet optional
- Ice-blue and silver coloring
- Ushaan (ice-pick weapon) at belt for dueling
- Phase pistol holster
- Proud, aggressive warrior stance";
        }
        
        return @"ANDORIAN CIVILIAN ATTIRE:
- Warm clothing (ice world natives)
- Blue, white, silver colors matching skin
- High collars, insulated fabrics
- Clan or family pins
- Antenna freely mobile (important for expression)
- Practical but elegant styling
- May include ceremonial elements for important families";
    }
    
    private string GetTrillAttire(string variant, bool isMilitary)
    {
        if (variant.Contains("commission") || variant.Contains("guardian"))
        {
            return @"TRILL SYMBIOSIS COMMISSION ROBES:
- Formal robes in teal and purple
- Symbiont pool imagery woven into fabric
- High status, scientific/medical authority
- Guardian medallion showing centuries of service
- Simple but prestigious styling";
        }
        
        if (variant.Contains("starfleet") || isMilitary)
        {
            return @"TRILL IN STARFLEET:
- Standard Starfleet uniform
- Spots visible on temples, neck (species identifier)
- Science blue common for Joined Trill
- Multiple lifetimes of experience in bearing";
        }
        
        return @"TRILL CIVILIAN ATTIRE:
- Elegant, flowing clothes in soft colors
- Teal, purple, soft blue preferred
- Spots on temples and neck visible
- Jewelry may incorporate symbiont motifs
- Sophisticated but not ostentatious
- Joined Trill carry themselves with ancient wisdom";
    }
    
    private string GetBajoranAttire(string variant, bool isMilitary)
    {
        if (variant.Contains("vedek") || variant.Contains("kai") || variant.Contains("priest"))
        {
            return @"BAJORAN RELIGIOUS ATTIRE:
- Flowing orange/saffron robes
- Elaborate d'ja pagh earring on RIGHT ear (CRITICAL)
- Religious symbols: Tear drop (Orb), Celestial Temple
- Kai wears most elaborate version
- Vedek Assembly sash or stole
- Simple sandals, prayer beads
- Serene, spiritual expression
- Orb-shaped jewelry or clasps";
        }
        
        if (isMilitary || variant.Contains("militia") || variant.Contains("major"))
        {
            return @"BAJORAN MILITIA UNIFORM:
- Tan/khaki military uniform, practical cut
- Rust-red trim and accents
- D'ja pagh earring on RIGHT ear (always present)
- Rank insignia on collar
- Phaser holster at hip
- Resistance-era influenced design
- Bajoran insignia (planet symbol) on chest
- Practical, battle-tested appearance";
        }
        
        return @"BAJORAN CIVILIAN ATTIRE:
- Comfortable earth-toned clothing
- Orange, brown, rust colors common
- D'ja pagh earring ALWAYS on RIGHT ear
- Religious elements often incorporated
- Post-occupation practical style
- May include resistance-era elements with pride
- Simple but dignified";
    }
    
    private string GetOrionAttire(string variant)
    {
        if (variant.Contains("syndicate") || variant.Contains("boss"))
        {
            return @"ORION SYNDICATE BOSS ATTIRE:
- Lavish, ostentatious clothing
- Gold, green, copper, black colors
- Revealing cuts showing green skin (cultural)
- Multiple weapons, visible wealth
- Syndicate insignia (usually hidden)
- Intimidating but wealthy appearance";
        }
        
        if (variant.Contains("dancer") || variant.Contains("slave"))
        {
            return @"ORION DANCER/SLAVE ATTIRE:
- Minimal, revealing clothing (cultural/historical)
- Green skin prominently displayed
- Gold jewelry, chains, bangles
- Exotic, seductive styling
- May show liberation/freedom elements for freed individuals";
        }
        
        return @"ORION STANDARD ATTIRE:
- Practical clothing for trade or piracy
- Green skin visible (species identifier)
- Mix of styles from various cultures (traders/pirates)
- Weapons visible, utilitarian
- Gold accents showing wealth
- Syndicate or independent markings
- Tough, streetwise appearance";
    }
    
    private string GetFactionPortraitStyle(Faction faction, string variantDescription)
    {
        var variantLower = variantDescription.ToLower();
        var isMilitary = variantLower.Contains("officer") || variantLower.Contains("soldier") || 
                         variantLower.Contains("warrior") || variantLower.Contains("guard");
        var isLeader = variantLower.Contains("admiral") || variantLower.Contains("general") || 
                       variantLower.Contains("chancellor") || variantLower.Contains("praetor");
        var isCivilian = variantLower.Contains("civilian") || variantLower.Contains("merchant") || 
                         variantLower.Contains("scientist") || variantLower.Contains("doctor");
        
        return faction switch
        {
            Faction.Federation => $@"FEDERATION SPECIES & APPEARANCE:
- Could be Human (various ethnicities), Vulcan (pointed ears, upswept eyebrows), Andorian (blue skin, white hair, antennae), Tellarite (porcine features), Bolian (blue skin, bifurcated ridge), Trill (leopard spots on temples), Betazoid (black irises), Bajoran (nose ridges)
- Starfleet uniform: Black with colored shoulder (Red=Command, Gold=Operations/Security, Blue=Science/Medical)
- Combadge on left chest, rank pips on collar
- {(isLeader ? "Admiral uniform with extra pips, more formal" : "")}
- {(isCivilian ? "Casual Federation civilian clothes - comfortable, practical, no uniform" : "")}
- Expression: Confident, professional, hopeful. Federation officers are optimistic.",

            Faction.Klingon => $@"KLINGON SPECIES & APPEARANCE:
- MUST have prominent forehead ridges (multiple bony ridges across forehead)
- Dark skin tone, fierce expression with teeth visible
- Long dark hair, often braided or wild
- Goatee or beard common on males
- Females also have ridges, can be warriors
- Leather and metal armor with House baldric (sash)
- {(isLeader ? "Chancellor/General - ornate armor, cape, more gold" : "")}
- {(isCivilian ? "Even civilians look warrior-like - simpler clothes but still tough" : "")}
- Expression: FIERCE, PROUD, READY FOR BATTLE. Klingons don't smile politely - they bare teeth.",

            Faction.Romulan => $@"ROMULAN SPECIES & APPEARANCE:
- Pointed ears like Vulcans but V-shaped subtle forehead ridge
- Traditional bowl-cut hairstyle (straight across forehead)
- Pale to olive skin, often darker than Vulcans
- Calculating, suspicious expression
- Grey/green military uniform with padded shoulders
- Tal Shiar (secret police) wear darker, more sinister version
- {(isLeader ? "Praetor/Senator - more ornate robes, bird emblems" : "")}
- {(isCivilian ? "Still guarded expression - Romulans trust no one" : "")}
- Expression: GUARDED, CALCULATING, SUPERIOR. Romulans never reveal their true intentions.",

            Faction.Cardassian => $@"CARDASSIAN SPECIES & APPEARANCE:
- Grey scaly skin texture, neck ridges extending to shoulders
- Spoon-shaped ridge in center of forehead (distinctive!)
- Slicked-back dark hair
- Blue eyes common
- Grey/brown military armor with ribbed texture
- Obsidian Order (secret police) in darker clothes
- {(isLeader ? "Gul/Legate rank - more ornate armor" : "")}
- {(isCivilian ? "Still authoritarian appearance - civilian Cardassians still look military" : "")}
- Expression: COLD, CALCULATING, RUTHLESS. Slight smirk of superiority.",

            Faction.Borg => $@"BORG DRONE APPEARANCE:
- Assimilated humanoid - could be any species originally
- PALE GREY SKIN - no healthy color
- One eye replaced with red laser implant
- Cybernetic implants covering part of face/skull
- Tubes and wires connected to skin
- Mechanical arm with assimilation tubules
- Black bodysuit with exoskeleton components
- NO expression - completely blank, emotionless
- Designation numbers, NOT a name
- The individual is gone - only the Collective remains",

            Faction.Ferengi => $@"FERENGI SPECIES & APPEARANCE:
- LARGE EARS (very large, wrinkled, highly sensitive)
- Sharp teeth, wide mouth
- Orange-brown skin
- Bald with bumpy skull ridges
- Shorter stature than humans
- Gaudy clothing with gold/orange, showing wealth
- Headpiece/hat common on males
- {(isLeader ? "Grand Nagus or DaiMon - most ornate gold accessories" : "")}
- {(isCivilian ? "Merchant attire with many pockets for latinum" : "")}
- Expression: GREEDY, CALCULATING, CUNNING. Always looking for profit angle.",

            Faction.Dominion => $@"DOMINION SPECIES:
VORTA:
- Pale lavender/grey skin, violet eyes
- Small ear ridges
- Elegant, diplomatic appearance
- Flowing robes, purple coloring
- Worships the Founders (Changelings)
- Smug, manipulative expression

JEM'HADAR:
- Grey reptilian skin with scales
- Bony ridges on face, especially forehead
- Sharp teeth, predator appearance
- Battle armor, purple/grey
- Ketracel-white tube visible
- Completely focused, no fear, no hesitation
- 'Victory is life' mentality",

            Faction.Breen => @"BREEN APPEARANCE:
- NEVER seen outside refrigeration suit
- Snout-like helmet with single visor
- Vocoder for speech (no one knows their language)
- Full body environmental suit, metallic green-grey
- Completely mysterious - their true appearance is unknown
- Helmet always on - cannot show face",

            Faction.Gorn => @"GORN APPEARANCE:
- Large reptilian humanoid
- Green scaly skin, pronounced snout
- Multifaceted compound eyes (insect-like but reptilian)
- Sharp teeth, powerful jaw
- Slow, deliberate movement
- Much larger than humans
- Minimal clothing - tough hide
- Cold, predatory expression",

            Faction.Vulcan => @"VULCAN APPEARANCE:
- Pointed ears, upswept eyebrows (distinctive!)
- Pale to olive skin
- Dark hair, often in formal style
- IDIC symbol worn
- Flowing robes or Starfleet uniform
- COMPLETELY emotionless expression
- Logical, controlled, no visible emotion
- Slightly green blood tint",

            Faction.Andorian => @"ANDORIAN APPEARANCE:
- BLUE SKIN (light to medium blue)
- WHITE HAIR
- TWO ANTENNAE on forehead (they move to show emotion!)
- Slightly larger ears
- Often aggressive, militaristic
- Imperial Guard armor or Starfleet uniform
- Passionate, emotional (opposite of Vulcans)",

            Faction.Trill => @"TRILL APPEARANCE:
- LEOPARD-SPOT pattern along temples and sides of neck
- Spots run from hairline, down temples, sides of face, down neck to body
- Humanoid, can be any skin tone
- Joined Trill have symbiont in abdominal pouch
- Thoughtful, wise expression (especially if Joined - multiple lifetimes)
- Trill uniform or Starfleet variant
- Scientific/academic personality common
- Symbiosis Commission robes for officials",

            Faction.Bajoran => @"BAJORAN APPEARANCE:
- Distinctive NOSE RIDGES (wrinkled, ridged nose bridge)
- Bajoran EARRING on RIGHT ear (d'ja pagh - required)
- Humanoid, various skin tones
- Spiritual yet determined expression
- Vedek/Kai robes for religious figures
- Militia uniform (orange/brown) for military
- Resistance-era fighters: rugged, guerrilla look
- Faith in the Prophets evident in bearing",

            _ => ""
        };
    }
    
    private string BuildHouseSymbolPrompt(FactionProfile profile, string symbolVariant)
    {
        var factionHeraldry = GetFactionHeraldryStyle(profile.Faction, symbolVariant);
        
        return $@"FLAT HERALDIC SYMBOL / EMBLEM

{factionHeraldry}

Subject: {profile.Name} house/family symbol, {symbolVariant}.
Style: {profile.HeraldicStyle}

MANDATORY STYLE REQUIREMENTS:
- FLAT 2D graphic symbol/logo - NO 3D, NO depth, NO metallic
- Clean vector-style art with solid colors
- Simple, bold, iconic design that reads well at any size
- Think: Flag emblems, coat of arms, corporate logos
- Solid color fills with clean outlines
- Suitable for flags, banners, building signs, UI icons

COLOR APPROACH:
- Use faction-appropriate colors as solid fills
- High contrast between symbol and background
- Maximum 3-4 colors per symbol
- Clean color separations (no gradients, no shading)

COMPOSITION:
- Symbol perfectly centered
- Fills most of the frame (80-90%)
- Solid black background (#000000)
- No perspective, no shadows, no reflections

CRITICAL:
- This is a FLAT GRAPHIC SYMBOL, not a 3D object
- Must work as a flag emblem or building sign
- Simple enough to be recognized at small sizes
- Iconic and memorable design

**flat 2D heraldic symbol, clean vector logo, bold graphic emblem, flag design, game asset**
--no 3D, no metallic, no depth, no shadows, no gradients, no reflections, no perspective, no text";
    }
    
    private string GetFactionHeraldryStyle(Faction faction, string symbolVariant)
    {
        return faction switch
        {
            Faction.Federation => @"FEDERATION HERALDRY:
- United Federation of Planets seal: Oval with stars representing member worlds
- Starfleet delta/arrowhead symbol
- Laurel wreaths for achievement/honor
- Clean geometric designs
- Stars, comets, celestial imagery
- Blue, white, gold colors
- Optimistic, unified symbolism
- Individual member world seals (Vulcan IDIC, Andorian snowflake, etc.)",

            Faction.Klingon => @"KLINGON HERALDRY:
- Klingon Empire trefoil (three-pointed symbol)
- Individual Great House symbols - each House has unique emblem
- Bat'leth (sword) motifs
- Targ (beast) imagery
- Sharp, angular designs - NO soft curves
- Red, black, bronze/gold colors
- Warrior imagery: blades, claws, fangs
- Honor symbols, battle commemoration
- Blood/combat themed (within reason)",

            Faction.Romulan => @"ROMULAN HERALDRY:
- Romulan Star Empire symbol: bird of prey with two planets
- Raptor/eagle bird imagery
- Sharp talons, predatory birds
- Green, silver, black colors
- Subtle, layered designs (secrets within secrets)
- Tal Shiar has distinct darker variants
- Senate/political house symbols
- Dual nature imagery (Romulus and Remus)",

            Faction.Cardassian => @"CARDASSIAN HERALDRY:
- Cardassian Union symbol: oval/arch shape
- Military efficiency in design
- Brown, tan, grey colors
- Geometric but not ornate
- Central Command symbols
- Obsidian Order has hidden/subtle variants
- Occupation/authority imagery
- Functional, not decorative",

            Faction.Borg => @"BORG HERALDRY:
- Borg do NOT have traditional heraldry
- If needed: Circuit patterns, geometric shapes
- Green on black
- Cube/sphere motifs
- Technical diagrams as 'art'
- Assimilation process imagery
- Collective unity symbols
- No individuality represented",

            Faction.Ferengi => @"FERENGI HERALDRY:
- Ferengi Alliance symbol: stylized ear shape
- Profit/wealth imagery: latinum bars, coins
- Gold, orange, copper colors
- Commercial/trade symbols
- Individual business/family wealth markers
- Grand Nagus seal
- Rules of Acquisition illustrated
- Gaudy, ostentatious, showing wealth",

            Faction.Dominion => @"DOMINION HERALDRY:
- Dominion symbol: organic, flowing design
- Purple, violet, lavender colors
- Founder worship imagery
- Vorta administrative seals
- Jem'Hadar unit/battalion markers
- Organic, biological aesthetic even in symbols
- Obedience and order themes
- Gamma Quadrant celestial references",

            Faction.Vulcan => @"VULCAN HERALDRY:
- IDIC symbol (Infinite Diversity in Infinite Combinations) - triangle with circle
- Logical geometric patterns, precise mathematical designs
- Bronze, copper, rust-red colors
- Mount Seleya imagery
- Surak teaching symbols
- Flame/logic motifs
- Kolinahr achievement markers
- Vulcan Science Academy emblems",

            Faction.Andorian => @"ANDORIAN HERALDRY:
- Andorian Empire/Imperial Guard symbol
- Antenna motifs
- Ice crystal patterns
- Blue and white/silver colors
- Clan/family emblems
- Warrior achievement markers
- Ushaan (duel) honor symbols
- Andoria planetary imagery",

            Faction.Trill => @"TRILL HERALDRY:
- Symbiosis Commission seal
- Symbiont-inspired flowing organic designs
- Teal, blue, soft purple colors
- Joined status markers
- Host family lineage symbols
- Scientific achievement emblems
- Caves of Mak'ala imagery
- Memory/continuity themes",

            Faction.Bajoran => @"BAJORAN HERALDRY:
- Bajoran religious symbols - Prophets imagery
- Celestial Temple / wormhole representations
- Orb symbols (Tears of the Prophets)
- Orange, gold, brown earth tones
- Resistance era emblems
- Vedek Assembly seals
- D'jarra (caste) historical markers
- Earring/d'ja pagh stylized designs",

            Faction.Breen => @"BREEN HERALDRY:
- Breen Confederacy symbol
- Crystalline ice patterns
- Teal and ice-blue colors
- Mysterious encoded designs
- Refrigeration suit helmet motif
- Unknown meaning to outsiders
- Angular, alien geometric patterns",

            Faction.Gorn => @"GORN HERALDRY:
- Gorn Hegemony symbol
- Reptilian claw and fang motifs
- Dark green, brown, bronze colors
- Scaled texture patterns
- Strength and power imagery
- Ancient reptilian civilization markers
- Territorial conquest symbols",

            _ => $"Standard heraldic emblem in {faction} style."
        };
    }
    
    private string BuildEventCharacterPrompt(FactionProfile profile, string characterName)
    {
        var characterStyle = GetEventCharacterStyle(characterName);
        
        return $@"MANDATORY STYLE GUIDE:
Material & Texture: The character must look like a handmade physical model made of plasticine clay with a subtle comic-like shader. Realistic skin/surface texture with a non-glossy, soft clay finish. High-end stop-motion animation quality (Laika/Aardman style).
Proportions (Crucial): Stylized but near-realistic proportions. Do NOT use excessive caricature or oversized head.
Lighting: Soft, cinematic studio lighting that emphasizes the clay texture (subsurface scattering). No harsh digital shine.
Expression: Characteristic of this unique being - mysterious, powerful, or otherworldly as appropriate.
Composition: Frontal view or 3/4 view, looking at camera.
Background: Solid black background (#000000).

{characterStyle}

Subject: {characterName} - special event character portrait.

CRITICAL: This is a UNIQUE character type, not a standard faction member. Follow the specific description above carefully.

**medium shot character portrait, rendered in high-quality stylized claymation 3D style, game character portrait**
--no funny, thumbprints, exaggerated features, oversized head, disproportionate, shiny skin, wet skin, CGI plastic look, action figure joints, miniature base, table surface, tilt-shift, blurry background, frame, border, text, card layout";
    }
    
    private string GetEventCharacterStyle(string characterName)
    {
        var nameLower = characterName.ToLower();
        
        // === SPECIFIC NAMED CHARACTERS ===
        
        // Q - THE Q (John de Lancie)
        if (nameLower.Contains("john de lancie") || (nameLower.Contains("q ") && nameLower.Contains("classic")))
        {
            return @"Q (THE Q - John de Lancie):
- Tall human male, middle-aged, distinguished
- Brown hair, slightly receding
- SMUG, MISCHIEVOUS smirk (signature expression!)
- Starfleet captain uniform OR judge robes OR casual
- Raised eyebrow, about to say something clever
- Snapping fingers pose optional
- Air of omnipotent superiority
- 'Mon capitaine!' energy
- Reference: John de Lancie as Q";
        }
        
        // Data (Brent Spiner)
        if (nameLower.Contains("data yellow") || nameLower.Contains("data emotion") || 
            (nameLower.Contains("data") && !nameLower.Contains("star")))
        {
            return @"DATA (Brent Spiner):
- Android with PALE YELLOWISH skin
- Slicked back black hair
- YELLOW/GOLD EYES (distinctive!)
- Starfleet uniform (gold/operations)
- Head tilt, curious expression
- Trying to understand humans
- Perfectly symmetrical features
- Neutral but CURIOUS expression (not blank)
- Reference: Brent Spiner as Data from TNG";
        }
        
        // Lore (Data's evil twin)
        if (nameLower.Contains("lore"))
        {
            return @"LORE (Data's Evil Twin):
- Identical to Data but with EMOTION
- Pale yellowish skin, yellow eyes
- CRUEL SMILE, malevolent expression
- More expressive than Data
- Confident, arrogant posture
- Same Soong-type android but evil
- Reference: Brent Spiner as Lore";
        }
        
        // Khan Noonien Singh (Classic - Ricardo Montalban)
        if (nameLower.Contains("khan") && nameLower.Contains("classic"))
        {
            return @"KHAN NOONIEN SINGH (Classic):
- Genetically enhanced SUPERHUMAN
- South Asian features, long grey hair
- Muscular, powerful build
- Open chest showing physique
- VENGEFUL, intense expression
- 20th century Earth dictator vibe
- 'I shall avenge!' intensity
- Reference: Ricardo Montalban as Khan (Wrath of Khan)";
        }
        
        // Khan (Into Darkness version)
        if (nameLower.Contains("khan") && (nameLower.Contains("into darkness") || nameLower.Contains("young")))
        {
            return @"KHAN (Into Darkness):
- Genetically enhanced SUPERHUMAN
- Pale skin, dark slicked hair
- Sharp features, intense eyes
- Starfleet or dark civilian clothes
- Cold, calculating expression
- Physically imposing
- Reference: Benedict Cumberbatch as Khan";
        }
        
        // Guinan (Whoopi Goldberg)
        if (nameLower.Contains("guinan"))
        {
            return @"GUINAN (El-Aurian Bartender):
- Dark-skinned El-Aurian female
- Elaborate HAT/HEADPIECE (signature!)
- Wise, knowing smile
- Ten Forward bartender attire
- All-knowing expression
- Mysterious, ageless quality
- 'Let me tell you something...' energy
- Reference: Whoopi Goldberg as Guinan";
        }
        
        // Odo (René Auberjonois)
        if (nameLower.Contains("odo") || (nameLower.Contains("changeling") && nameLower.Contains("constable")))
        {
            return @"ODO (Changeling Constable):
- Smooth, undefined features
- Slicked back hair (part of form)
- ORANGE-BROWN skin, waxy texture
- Security uniform (DS9 style)
- Grumpy, suspicious expression
- Hands behind back pose
- 'I don't trust anyone' energy
- Reference: René Auberjonois as Odo";
        }
        
        // Mirror Spock
        if (nameLower.Contains("mirror") && nameLower.Contains("spock"))
        {
            return @"MIRROR SPOCK (Evil Spock):
- Vulcan male with POINTED EARS
- GOATEE BEARD (signature!)
- Raised eyebrow
- Terran Empire uniform with gold sash
- Calculating but not entirely evil
- 'It is logical' but darker
- Reference: Leonard Nimoy as Mirror Spock";
        }
        
        // === GENERIC CHARACTER TYPES ===
        
        // Q Continuum entities (generic)
        if (nameLower.Contains("q ") || nameLower == "q" || nameLower.Contains("q continuum"))
        {
            return @"Q CONTINUUM ENTITY:
- Human-like appearance but with subtle OTHERWORLDLY quality
- Can appear as any species but favors distinguished human male
- Mischievous, superior, amused expression
- Slightly glowing or ethereal edge to form
- Wearing anything from Starfleet uniform to judge robes to casual
- SMUG, all-knowing expression - Q finds mortals amusing
- Could be snapping fingers or dramatic gesture
- Subtle reality-warping effect around edges";
        }
        
        // Androids / Synthetic life (generic)
        if (nameLower.Contains("android") || nameLower.Contains("soong") || nameLower.Contains("synthetic") || 
            nameLower.Contains("artificial") || nameLower.Contains("b4"))
        {
            return @"SOONG-TYPE ANDROID:
- Perfect human appearance but with subtle artificial quality
- Pale, slightly yellowish skin tone
- Yellow/gold eyes (distinctive!)
- Perfectly symmetrical features
- NO expression or ATTEMPTING expression (learning emotions)
- Starfleet uniform or civilian clothes
- Slight metallic sheen to skin if damaged/revealed
- Could show partial internal circuitry for variants
- Curious, analytical expression";
        }
        
        // Augment / Super Soldiers (generic)
        if (nameLower.Contains("augment") || nameLower.Contains("super soldier") || nameLower.Contains("enhanced"))
        {
            return @"AUGMENT (Genetically Enhanced Human):
- Human but PHYSICALLY SUPERIOR
- Perfect physique, intense expression
- Superior intelligence in eyes
- Could be any ethnicity
- Arrogant, superior body language
- 20th century or 22nd century origins
- Genetically engineered for perfection
- Aggressive, ambitious personality implied";
        }
        
        // Changelings / Founders
        if (nameLower.Contains("changeling") || nameLower.Contains("founder") || nameLower.Contains("shapeshifter"))
        {
            return @"CHANGELING / FOUNDER:
- In humanoid form: smooth, undefined features
- Orange-brown skin color, waxy texture
- Slicked-back hair appearance (part of their form)
- Could show partial liquid/morphing effect
- Serene, ancient, god-like expression
- Simple robes
- OR: partially morphed between forms
- Great Link golden liquid around edges";
        }
        
        // El-Aurians (Guinan's species)
        if (nameLower.Contains("el-aurian") || nameLower.Contains("listener"))
        {
            return @"EL-AURIAN (Listener):
- Human-like but with ancient, wise quality
- Dark skin common, elaborate hat/headpiece
- All-knowing, mystical expression
- Civilian clothes, often bartender attire
- Subtle 'seen everything' wisdom in eyes
- Refugee from Borg destruction
- Could be any age (very long-lived species)";
        }
        
        // Prophets / Wormhole Aliens
        if (nameLower.Contains("prophet") || nameLower.Contains("wormhole alien") || nameLower.Contains("pah-wraith"))
        {
            var isPahWraith = nameLower.Contains("pah-wraith");
            return isPahWraith 
                ? @"PAH-WRAITH:
- Appearing as possessed humanoid (often Bajoran)
- RED GLOWING EYES (distinctive!)
- Malevolent, angry expression
- Fire/flame visual elements
- Dark, corrupted version of normal appearance
- Sinister, evil aura"
                : @"PROPHET / WORMHOLE ALIEN:
- Non-corporeal beings - appearing AS someone the viewer knows
- Could look like any species (they borrow forms)
- Ethereal, glowing quality
- Peaceful, cryptic expression
- Non-linear time perception
- Blue/white glow effect
- Speaking in riddles implied by expression";
        }
        
        // Borg Queen
        if (nameLower.Contains("borg queen"))
        {
            return @"BORG QUEEN:
- Humanoid female with extensive cybernetic implants
- Pale grey skin, bald or minimal dark hair
- One eye cybernetic, one organic
- Mechanical spine and shoulder assembly
- Seductive yet terrifying expression
- Both individual AND collective
- Queen's distinctive head/spine harness
- Dark lips, intense gaze
- 'I am the Borg' confidence";
        }
        
        // Traveler
        if (nameLower.Contains("traveler") || nameLower.Contains("tau alpha"))
        {
            return @"THE TRAVELER:
- Alien humanoid with grey skin
- Elongated features, high forehead
- Simple grey robes
- Wise, knowing expression  
- Reality-warping abilities implied
- Mysterious, benevolent presence
- Wesley's mentor figure
- Transcendent being";
        }
        
        // Douwd / Kevin Uxbridge type
        if (nameLower.Contains("douwd") || nameLower.Contains("immortal entity"))
        {
            return @"IMMORTAL ENTITY (Douwd-type):
- Appears as ordinary elderly human
- But with IMMENSE hidden power implied
- Gentle, sorrowful expression
- Simple civilian clothes
- Weight of eternity in eyes
- Pacifist but capable of genocide
- Ordinary facade hiding cosmic being";
        }
        
        // Temporal Agent
        if (nameLower.Contains("temporal agent") || nameLower.Contains("time traveler") || nameLower.Contains("29th century"))
        {
            return @"TEMPORAL AGENT:
- Humanoid in sleek future uniform
- 29th century minimalist aesthetic
- Temporal badge/device visible
- Knowing expression (seen future)
- Could be any species
- Time displacement equipment
- Serious, mission-focused";
        }
        
        // Species 8472
        if (nameLower.Contains("8472") || nameLower.Contains("fluidic"))
        {
            return @"SPECIES 8472:
- Tall, tripedal alien from fluidic space
- Grey-green mottled skin
- Three legs, multiple limbs
- Elongated skull, no visible eyes (or sunken)
- Incredibly hostile, aggressive stance
- Organic bioship technology implied
- Borg's only true threat
- Terrifying, alien appearance";
        }
        
        // Holographic characters
        if (nameLower.Contains("hologram") || nameLower.Contains("holographic") || nameLower.Contains("emh") ||
            nameLower.Contains("photonic"))
        {
            return @"HOLOGRAPHIC BEING:
- Appears perfectly human/humanoid
- Mobile emitter visible on arm (if independent)
- Doctor EMH: balding, professional
- Slight scan-line or flicker effect optional
- 'Please state the nature of medical emergency' expression
- Or: other holographic character types
- Self-aware, rights-seeking personality";
        }
        
        // Trelane / Squire of Gothos type
        if (nameLower.Contains("trelane") || nameLower.Contains("squire") || nameLower.Contains("godlike entity"))
        {
            return @"GODLIKE ENTITY (Trelane-type):
- Human appearance in period costume
- 18th-19th century Earth attire
- Childish, petulant expression
- Reality-warping effects around
- Spoiled 'cosmic child' vibe
- All-powerful but immature
- Q-like but less sophisticated";
        }
        
        // Generic cosmic/ascended being
        if (nameLower.Contains("ascended") || nameLower.Contains("cosmic") || nameLower.Contains("energy being"))
        {
            return @"ASCENDED / ENERGY BEING:
- Glowing humanoid form
- Made of light/energy
- Features visible but ethereal
- Transcendent expression
- Beyond physical form
- White/blue/golden glow
- Ancient and wise";
        }
        
        // Mirror Universe variant
        if (nameLower.Contains("mirror") || nameLower.Contains("terran empire"))
        {
            return @"MIRROR UNIVERSE CHARACTER:
- Same species as prime universe
- But DARKER, more aggressive expression
- Terran Empire insignia (sword through Earth)
- More revealing/aggressive clothing
- Goatee for males (classic!)
- Scar or eye patch optional
- Cruel, ambitious expression
- Gold sash for officers";
        }
        
        // Iconians
        if (nameLower.Contains("iconian"))
        {
            return @"ICONIAN (Ancient Race):
- Tall humanoid with elegant, ancient appearance
- Blue/purple skin tones, luminous eyes
- Highly advanced but ancient aesthetic
- Ornate robes with technological integration
- Gateway technology visual hints
- Proud, superior expression
- Extinct but powerful race
- NOT a wizard - advanced ALIEN species
- Clean, elegant design - no staff or wand";
        }
        
        // Preservers
        if (nameLower.Contains("preserver"))
        {
            return @"PRESERVER (Ancient Race):
- Ancient humanoid seeder race
- Simple, understated appearance
- Monk-like robes but ALIEN not medieval
- Caretaker of primitive species
- Benevolent, parental expression
- Mysterious technology implied
- NOT a wizard - ALIEN scientist/caretaker";
        }
        
        // T'Kon Empire
        if (nameLower.Contains("t'kon") || nameLower.Contains("tkon"))
        {
            return @"T'KON EMPIRE (Ancient Race):
- Powerful ancient warrior civilization
- Humanoid with distinct alien features
- Ornate armor or official robes
- Empire that could move stars
- Proud, commanding presence
- Guardian/warrior aesthetic
- NOT medieval - ADVANCED alien empire";
        }
        
        // Metrons
        if (nameLower.Contains("metron"))
        {
            return @"METRON (Powerful Alien):
- Highly evolved humanoid
- Shimmering, ethereal appearance
- Silver/white coloring
- Greek god-like aesthetic but ALIEN
- Above petty conflicts
- Observer/judge demeanor
- Simple elegant garment
- NOT a wizard - evolved ALIEN species";
        }
        
        // Organians
        if (nameLower.Contains("organian"))
        {
            return @"ORGANIAN (Energy Being):
- Appears as simple humanoid (disguise)
- Or glowing energy form (true form)
- Humble robed appearance when physical
- Brilliant light being when revealed
- Peaceful, non-violent expression
- Incredibly powerful but pacifist
- NOT a wizard - NON-CORPOREAL being";
        }
        
        // Excalbians
        if (nameLower.Contains("excalbian"))
        {
            return @"EXCALBIAN:
- Rock-like alien being
- Molten/volcanic appearance
- Craggy, geological features
- Glowing fissures in body
- Studies concepts of good/evil
- Alien and inhuman
- NOT humanoid in traditional sense";
        }
        
        // First Federation
        if (nameLower.Contains("first federation") || nameLower.Contains("balok"))
        {
            return @"FIRST FEDERATION:
- Small humanoid alien
- Childlike appearance (Balok's true form)
- Large head, friendly expression
- Or: terrifying puppet dummy face
- Tranya beverage association
- Welcoming but mysterious
- NOT threatening despite first contact";
        }
        
        // Sheliak
        if (nameLower.Contains("sheliak"))
        {
            return @"SHELIAK:
- Non-humanoid alien
- Crystalline/chitinous body structure
- Classification obsessed
- Arrogant, superior demeanor
- Considers humans inferior
- Unusual body shape
- Corporate entity member";
        }
        
        // Sphere Builders
        if (nameLower.Contains("sphere builder"))
        {
            return @"SPHERE BUILDER:
- Transdimensional being
- Ethereal, ghost-like appearance
- Pale, almost translucent
- Future manipulators
- Temporal Cold War faction
- Otherworldly, unsettling";
        }
        
        // Hur'q
        if (nameLower.Contains("hur'q"))
        {
            return @"HUR'Q:
- Insectoid alien species
- Beetle-like appearance
- Warrior/swarm mentality
- Ancient enemy of Klingons
- Chitin armor natural
- Hive-like society
- Aggressive, alien features";
        }
        
        // Progenitors
        if (nameLower.Contains("progenitor"))
        {
            return @"PROGENITOR (Original Humanoid):
- Ancient humanoid - first of all humanoid species
- Holographic/ancient recording appearance
- Simple elegant features
- Seeded the galaxy with humanoid DNA
- Message from billions of years ago
- Bittersweet, hopeful expression
- Origin of ALL humanoid races";
        }
        
        // Crystalline Entity
        if (nameLower.Contains("crystalline entity"))
        {
            return @"CRYSTALLINE ENTITY AVATAR:
- If appearing as humanoid: geometric, prismatic features
- Crystal/glass-like skin
- Refracts light into rainbows
- Beautiful but deadly
- Consumes life force
- Alien, non-emotional presence";
        }
        
        // V'Ger
        if (nameLower.Contains("v'ger"))
        {
            return @"V'GER ENTITY:
- Machine entity seeking creator
- Humanoid form if appearing as one
- Mechanical/organic hybrid appearance
- Probe-like elements integrated
- Seeking meaning, purpose
- Originally Voyager 6 spacecraft
- Both ancient and childlike";
        }

        // Default for unknown special characters
        return @"UNIQUE STAR TREK CHARACTER:
- Alien or unusual humanoid appearance
- NOT a wizard, NOT medieval, NOT fantasy
- Science fiction aesthetic only
- Advanced alien species or entity
- Distinctive alien features (skin color, forehead ridges, unusual eyes)
- Futuristic or ancient-advanced clothing
- NO staffs, NO wands, NO robes with mystical symbols
- Think: Star Trek alien species, not fantasy wizard";
    }
    
    private Dictionary<Faction, FactionProfile> InitializeFactionProfiles()
    {
        return new Dictionary<Faction, FactionProfile>
        {
            [Faction.Federation] = new FactionProfile
            {
                Faction = Faction.Federation,
                Name = "Federation",
                DesignLanguage = "Elegant curved forms, saucer sections, paired nacelles, smooth flowing lines, optimistic and exploratory aesthetic",
                ColorScheme = "Pearl white and light grey hull, blue deflector dish glow, red Bussard collectors, subtle blue accent lighting",
                CivilianDesignLanguage = "Simple practical shapes, boxy shuttles, cylindrical freighters, modular cargo containers, utilitarian but clean",
                CivilianColorScheme = "White or cream hull, blue accents, civilian markings, no military grey",
                Architecture = "Utopian futuristic, curved glass domes, organic flowing structures, bright and welcoming",
                RaceFeatures = "Human appearance with various ethnicities, or Vulcan pointed ears, or Andorian blue skin and antennae",
                ClothingDetails = "Starfleet uniform - black with colored shoulder section (red command, gold operations, blue science), delta badge",
                HeraldicStyle = "Clean geometric UFP seal variations, laurel wreaths, stars",
                MilitaryShips = GenerateShipList("Constitution", "Galaxy", "Sovereign", "Defiant", "Intrepid", "Excelsior", "Nebula", "Akira", "Prometheus", "Nova", "Ambassador", "Miranda", "Oberth", "Steamrunner", "Saber", "Norway", "Cheyenne", "Springfield", "New Orleans", "Centaur", "Curry Type", "Olympic", "Niagara", "Freedom", "Challenger", "Luna", "Vesta", "Odyssey", "Inquiry", "Reliant Type", "Constellation", "Rhode Island", "Raven", "Yeager Type", "Armstrong", "Gagarin"),
                CivilianShips = GenerateShipList("Type 6 Shuttle", "Type 7 Shuttle", "Type 8 Shuttle", "Type 9 Shuttle", "Type 10 Shuttle", "Type 11 Shuttle", "Type 15 Shuttlepod", "Danube Runabout", "Delta Flyer", "Argo Transport", "Captain's Yacht", "Work Bee", "Travel Pod", "Cargo Shuttle", "Medical Shuttle", "Executive Transport", "Cargo Freighter", "Colony Transport", "Passenger Liner", "Medical Ship", "Research Vessel", "Survey Ship", "Tug", "Tanker", "Mining Ship", "Constructor Ship", "Diplomatic Courier", "Academy Trainer", "Hopper", "Personnel Carrier", "Long Range Shuttle", "Warp Sled", "Sphinx Workpod", "Yellowstone Runabout", "Aeroshuttle", "Waverider"),
                MilitaryStructures = GenerateStructureList("Spacedock", "Starbase 1 Type", "Deep Space Station", "Orbital Defense Platform", "Phaser Array Platform", "Torpedo Platform", "Shield Generator Station", "Sensor Array Large", "Sensor Array Small", "Early Warning Station", "Tachyon Detection Grid", "Subspace Relay", "Command Station", "Fleet Coordination Center", "Weapons Depot", "Fighter Bay Platform", "Repair Dock", "Medical Station", "Quarantine Platform", "Listening Post", "Intelligence Hub", "Training Station", "Defense Satellite", "Mine Layer Platform", "Tractor Beam Station", "Interdiction Platform", "Heavy Weapons Platform", "Point Defense Cluster", "Anti-Fighter Platform", "Orbital Fortress", "Defense Ring Segment", "Perimeter Station", "Border Outpost", "Checkpoint Station", "Patrol Base", "Rapid Response Hub"),
                CivilianStructures = GenerateStructureList("Trading Station", "Commercial Hub", "Merchant Waystation", "Cargo Depot", "Passenger Terminal", "Transit Hub", "Drydock Small", "Drydock Large", "Shipyard Module", "Construction Platform", "Salvage Station", "Recycling Center", "Research Station", "Science Outpost", "Observatory", "Stellar Cartography", "Communications Relay", "Subspace Booster", "News Broadcast Station", "Entertainment Hub", "Holosuite Station", "Recreation Platform", "Hospital Station", "Rehabilitation Center", "Agricultural Station", "Hydroponics Bay", "Mining Headquarters", "Refinery Platform", "Gas Collection Station", "Power Generation Station", "Solar Array", "Diplomatic Station", "Cultural Exchange Hub", "University Station", "Archive Station", "Museum Platform"),
                Buildings = GenerateBuildingList("Starfleet Headquarters", "Starfleet Academy", "Federation Council", "President's Office", "Diplomatic Corps", "Science Institute", "Medical Research", "Daystrom Institute", "Warp Core Factory", "Phaser Assembly", "Torpedo Manufacturing", "Ground Shipyard", "Power Generator", "Fusion Plant", "Solar Collector", "Geothermal Station", "Habitat Dome", "Residential Complex", "Recreation Center", "Holosuite Arcade", "Hospital", "Medical Academy", "Counseling Center", "Agricultural Dome", "Hydroponics", "Food Processing", "Water Treatment", "Transporter Hub", "Shuttle Bay", "Landing Pad", "Cargo Warehouse", "Communications Tower", "Weather Control", "Terraforming Hub", "Museum", "Memorial Park", "Arboretum", "Security Office", "Detention Center", "Training Ground", "Weapons Range", "Observatory", "Library", "School", "University", "Embassy", "Trade Center"),
                Troops = GenerateTroopList(
                    // Basic Security (6)
                    "Security Officer Male", "Security Officer Female", "Security Chief", "Security Guard", "Brig Officer", "Patrol Officer",
                    // MACO/Special Forces (6)
                    "MACO Soldier", "MACO Heavy Weapons", "MACO Sniper", "MACO Demolitions", "MACO Squad Leader", "MACO Commander",
                    // Hazard Team (4)
                    "Hazard Team Member", "Hazard Team Leader", "Hazard Team Specialist", "Hazard Team Medic",
                    // Combat Specialists (6)
                    "Tactical Officer", "Combat Medic", "Combat Engineer", "Sniper", "Demolitions Expert", "Communications Officer",
                    // Vehicles & Equipment (6)
                    "Phaser Turret", "Photon Mortar", "Ground APC", "Ground Tank", "Hover Bike", "Argo Buggy",
                    // Heavy/Elite (6)
                    "Exosuit Infantry", "Heavy Weapons Trooper", "Assault Trooper", "Shield Bearer", "Point Man", "Rear Guard",
                    // Support (2)
                    "Field Medic", "Engineering Support"),
                PortraitVariants = GeneratePortraitList("Human Male Young Officer", "Human Male Admiral", "Human Male Civilian", "Human Male Scientist", "Human Female Young Officer", "Human Female Admiral", "Human Female Civilian", "Human Female Doctor", "Vulcan Male Young", "Vulcan Male Elder", "Vulcan Female Young", "Vulcan Female Elder", "Andorian Male", "Andorian Female", "Tellarite Male", "Tellarite Female", "Bolian Male", "Bolian Female", "Trill Male Joined", "Trill Female Joined", "Betazoid Male", "Betazoid Female", "Bajoran Male", "Bajoran Female", "Human Male Engineer", "Human Female Engineer", "Human Male Tactical", "Human Female Tactical", "Caitian Male", "Caitian Female", "Benzite Male", "Denobulan Male", "Saurian Male", "Efrosian Male", "Rigelian Female", "Ktarian Female"),
                HouseSymbols = GenerateSymbolList("UFP Seal Standard", "UFP Seal Ornate", "Starfleet Command", "Starfleet Academy", "Starfleet Medical", "Starfleet Science", "Starfleet Engineering", "Starfleet Security", "Starfleet Intelligence", "Starfleet Diplomatic", "MACO Insignia", "Hazard Team", "Earth Seal", "Vulcan IDIC", "Andorian Imperial", "Tellar Seal", "Alpha Centauri", "Betazed Seal", "Trill Seal", "Bajor Member", "First Fleet", "Second Fleet", "Third Fleet", "Fourth Fleet", "Fifth Fleet", "Sixth Fleet", "Seventh Fleet", "Eighth Fleet", "Exploration Division", "Defense Division", "Research Division", "Medical Division", "Engineering Corps", "Judge Advocate", "Temporal Division", "Section 31 Subtle", "Starfleet Marines", "Border Patrol", "Academy Graduate", "Command School", "War College", "Diplomatic Institute", "Science Academy", "Medical Academy", "Engineering School", "Admiral Rank", "Captain Rank", "Commodore Rank")
            },
            
            [Faction.Klingon] = new FactionProfile
            {
                Faction = Faction.Klingon,
                Name = "Klingon",
                DesignLanguage = "Aggressive angular shapes, forward-swept wings, predatory bird-like silhouette, brutal and intimidating, exposed hull plating",
                ColorScheme = "Dark military green, gunmetal grey, bronze/copper accents, red warning lights, battle damage weathering",
                CivilianDesignLanguage = "Still angular but industrial, heavy-duty work vessels, less aggressive, functional brutalist",
                CivilianColorScheme = "Rust browns, dark greys, industrial weathering, House markings",
                Architecture = "Fortress-like brutal structures, sharp spires, heavy fortification, dark metals, torch lighting aesthetic",
                RaceFeatures = "Prominent forehead ridges, dark skin, long dark hair often braided, fierce expression, sharp teeth",
                ClothingDetails = "Dark leather and metal armor, baldric sash, House insignia on chest",
                HeraldicStyle = "Trefoil Empire symbol variations, sharp angular designs, blade motifs",
                MilitaryShips = GenerateShipList("Bird of Prey B'rel", "Bird of Prey K'vort", "Vor'cha Attack Cruiser", "Negh'Var Warship", "K't'inga Battlecruiser", "D7 Battlecruiser", "Raptor Scout", "D5 Cruiser", "IKS Rotarran Type", "Bortas Class", "Qugh Destroyer", "Hegh'ta Heavy Bird", "Ch'Tang Type", "Martok's Flagship", "Ning'tao Escort", "Somraw Raptor", "M'Char Fighter", "Kivra Shuttle", "Fek'lhr Dreadnought", "Sarcophagus Ship", "Cleave Ship", "D4 Cruiser", "Chargh Type", "Pach Strike Wing", "Qa'vak Patrol", "HoH'SuS Bird", "Qaw'Dun Bird", "Mat'Ha Raptor", "Kurak Science", "DuQwI' Fighter", "Mogh Battlecruiser", "Ty'Gokor Type", "Qu'Daj Fighter", "Koloth Memorial", "Kor Memorial", "Kang Memorial"),
                CivilianShips = GenerateShipList("Klingon Transport", "Toron Shuttle", "Cargo Hauler", "Prison Barge", "Colony Ship", "Mining Vessel", "Slave Transport", "Merchant Runner", "Dilithium Hauler", "Ore Carrier", "Targ Transport", "Warrior Ferry", "Honor Guard Yacht", "High Council Transport", "Ambassador Vessel", "Trade Negotiator", "Bloodwine Tanker", "Supply Runner", "Medical Transport", "Wounded Warrior Ship", "Recruitment Vessel", "Academy Transport", "House Noble Yacht", "Civilian Shuttle", "Agricultural Ship", "Livestock Transport", "Construction Barge", "Salvage Vessel", "Scout Courier", "Message Carrier", "Diplomatic Runner", "Chancellor's Yacht", "Opera House Ship", "Festival Barge", "Hunting Vessel", "Safari Transport"),
                MilitaryStructures = GenerateStructureList("Defense Outpost", "Weapon Platform", "Cloaking Detection Array", "Shield Generator", "Torpedo Station", "Disruptor Battery", "Orbital Fortress", "Listening Post", "Sensor Station", "Mine Field Control", "Fleet Staging Base", "Repair Dock Military", "Munitions Depot", "Training Station", "Prison Satellite", "Interrogation Platform", "Intelligence Hub", "Communications Array", "Command Station", "Emperor's Eye Station", "House Defense Station", "Border Post", "Checkpoint Platform", "Patrol Base", "Quick Response Hub", "Heavy Cannon Platform", "Siege Station", "Bombardment Platform", "Ground Support Station", "Troop Deployment Hub", "Fighter Carrier Station", "Honor Guard Station", "Elite Training Platform", "Veterans Station", "War Memorial Station", "Kahless Shrine Station"),
                CivilianStructures = GenerateStructureList("Trading Post", "Dilithium Processing", "Shipyard", "Communication Relay", "Mining Station", "Ore Processing", "Cargo Hub", "Merchant Exchange", "Bloodwine Brewery Station", "Food Processing", "Recreation Station", "Arena Viewing Platform", "Opera House Station", "Cultural Center", "Museum of Honor", "History Archive", "Academy Annex", "Medical Station", "Rehabilitation Center", "Agricultural Station", "Livestock Hub", "Construction Yard", "Salvage Depot", "Civilian Dock", "Transit Hub", "Passenger Terminal", "Hotel Station", "Entertainment Complex", "Gambling Den", "Warrior's Rest", "Veterans Home", "Shrine to Kahless", "Temple Station", "Monastery Platform", "Pilgrimage Stop", "Tournament Station"),
                Buildings = GenerateBuildingList("Great Hall", "Warrior Academy", "Weapons Factory", "Barracks", "Prison Complex", "Arena", "Temple of Kahless", "Forge", "Disruptor Factory", "Bat'leth Smithy", "Targ Kennel", "Bloodwine Hall", "Feast Hall", "Council Chamber", "High Council Building", "House Stronghold", "Defense Tower", "Guard Post", "Training Ground", "Combat Arena", "Veterans Hall", "Memorial Obelisk", "Statue of Kahless", "Opera House", "Museum of Conquest", "History Hall", "Library of Honor", "Medical Facility", "Field Hospital", "Rehabilitation Center", "Agricultural Dome", "Targ Farm", "Gagh Breeding", "Mining Complex", "Ore Refinery", "Power Plant", "Shield Generator", "Sensor Tower", "Communications Hub", "Shuttle Bay", "Landing Fortress", "Cargo Warehouse", "Trade Hall", "Merchant Quarter", "Embassy Fortress", "Diplomatic Hall", "Chancellor's Residence", "Emperor's Palace"),
                Troops = GenerateTroopList(
                    // Warrior Classes (8)
                    "Klingon Warrior Male", "Klingon Warrior Female", "Young Warrior", "Veteran Warrior", "Elite Warrior", "Berserker", "Dahar Master", "Sword of Kahless",
                    // Melee Specialists (6)
                    "Bat'leth Master", "Mek'leth Fighter", "D'k tahg Assassin", "Pain Stick Guard", "Dual Blade Warrior", "Champion Duelist",
                    // Ranged/Heavy (6)
                    "Disruptor Rifleman", "Disruptor Pistol Officer", "Heavy Weapons Trooper", "Siege Gunner", "Sniper", "Grenadier",
                    // Special Units (6)
                    "Honor Guard", "House Guard", "Imperial Guard", "Yan-Isleth Brotherhood", "Intelligence Operative", "Dahar Master Guard",
                    // Support (4)
                    "Combat Medic", "Combat Engineer", "Scout", "Communications Officer",
                    // Beasts & Vehicles (6)
                    "Targ Warbeast", "Targ Handler", "Siege Disruptor", "Klingon Tank", "Assault Speeder", "Drop Pod Warrior"),
                PortraitVariants = GeneratePortraitList("Male Warrior Young", "Male Warrior Veteran", "Male Warrior Elder", "Male General", "Male Chancellor Type", "Male House Lord", "Female Warrior Young", "Female Warrior Veteran", "Female House Lady", "Female Scientist", "Male Priest of Kahless", "Female Priestess", "Male Dahar Master", "Female Dahar Master", "Male Civilian", "Female Civilian", "Male Merchant", "Female Merchant", "Male Youth", "Female Youth", "Male Child", "Female Child", "Male Dishonored", "Male Outcast", "Augment Klingon Male", "Augment Klingon Female", "House Minor Noble Male", "House Minor Noble Female", "Male Engineer", "Female Engineer", "Male Doctor", "Female Doctor", "Male Ambassador", "Female Ambassador", "Male Judge", "Female Judge"),
                HouseSymbols = GenerateSymbolList("Empire Trefoil Standard", "Empire Trefoil Ornate", "House of Martok", "House of Mogh", "House of Duras", "House of Kor", "House of Kang", "House of Koloth", "House of Gowron", "House of Torg", "House of Kozak", "House of K'mpec", "House of Gorkon", "House of Azetbur", "High Council Seal", "Chancellor's Seal", "Defense Force", "Imperial Intelligence", "Warrior Caste", "Priest Caste", "Merchant Caste", "Scientist Caste", "Order of the Bat'leth", "Order of Kahless", "Dahar Master Rank", "General Rank", "Captain Rank", "Commander Rank", "First City", "Qo'noS Seal", "Boreth Monastery", "Ty'Gokor", "Narendra III Memorial", "Khitomer Memorial", "Praxis Memorial", "Blood Oath Seal", "Victory Seal", "Honor Seal", "Conquest Seal", "Battle Standard", "War Banner", "Raid Banner", "Siege Banner", "Death Banner", "Glory Banner", "Legend Banner", "Immortal Banner", "Ancestor Banner")
            },
            
            [Faction.Romulan] = CreateRomulanProfile(),
            [Faction.Ferengi] = CreateFerengiProfile(),
            [Faction.Cardassian] = CreateCardassianProfile(),
            [Faction.Borg] = CreateBorgProfile(),
            [Faction.Dominion] = CreateDominionProfile(),
            [Faction.Breen] = CreateBreenProfile(),
            [Faction.Gorn] = CreateGornProfile(),
            [Faction.Andorian] = CreateAndorianProfile(),
            [Faction.Vulcan] = CreateVulcanProfile(),
            [Faction.Trill] = CreateTrillProfile(),
            [Faction.Bajoran] = CreateBajoranProfile(),
            [Faction.Tholian] = CreateTholianProfile(),
            [Faction.Orion] = CreateOrionProfile(),
            [Faction.Hirogen] = CreateHirogenProfile(),
            [Faction.Betazoid] = CreateBetazoidProfile(),
            [Faction.Maquis] = CreateMaquisProfile(),
            [Faction.Pirates] = CreatePiratesProfile(),
            [Faction.Nausicaan] = CreateNausicaanProfile(),
            [Faction.Species8472] = CreateSpecies8472Profile(),
            [Faction.Special] = CreateSpecialProfile(),
            [Faction.AncientRaces] = CreateAncientRacesProfile()
        };
    }
    
    // Helper methods to generate lists with exact counts
    private List<string> GenerateShipList(params string[] ships) => ships.Take(36).ToList();
    private List<string> GenerateStructureList(params string[] structures) => structures.Take(36).ToList();
    private List<string> GenerateBuildingList(params string[] buildings) => buildings.Take(48).ToList();
    private List<string> GenerateTroopList(params string[] troops) => troops.Take(16).ToList();
    private List<string> GeneratePortraitList(params string[] portraits) => portraits.Take(36).ToList();
    private List<string> GenerateSymbolList(params string[] symbols) => symbols.Take(48).ToList();
    
    // Stub methods for other factions - these would be fully implemented
    private FactionProfile CreateRomulanProfile() => new FactionProfile
    {
        Faction = Faction.Romulan,
        Name = "Romulan",
        DesignLanguage = "Bird-like swept wings, elegant yet menacing, hidden power, sleek predatory lines",
        ColorScheme = "Dark forest green, grey, silver trim, green plasma glow",
        CivilianDesignLanguage = "Still bird-like but less aggressive, merchant and diplomatic vessels, hidden compartments",
        CivilianColorScheme = "Lighter green, grey, civilian markings, no military insignia",
        Architecture = "Imperial Roman inspired, grand columns, hidden surveillance, elegant but oppressive",
        RaceFeatures = "Pointed ears, V-shaped forehead ridges, pale greenish complexion, calculating expression",
        ClothingDetails = "Grey-green military uniform, padded shoulders, Star Empire insignia",
        HeraldicStyle = "Bird of prey symbol variations, double-headed eagle motifs",
        MilitaryShips = GenerateShipList("D'deridex Warbird", "Valdore Warbird", "Mogai Warbird", "D7 Romulan", "Bird of Prey Romulan", "Scimitar Dreadnought", "Norexan Warbird", "D'ridthau Warbird", "Dhael Warbird", "Khenn Cruiser", "Lanora Scout", "Tiercel Fighter", "T'varo Warbird", "Ha'apax Warbird", "Falchion Dreadnought", "Dhelan Warbird", "Ar'kif Tactical", "Ha'feh Assault", "Aelahl Battlecruiser", "Khopesh Dreadnought", "Tal Shiar Adapted", "Faeht Intel", "Malem Light", "Hathos Warbird", "Kara Advanced", "Deleth Advanced", "Vastam Command", "Tebok Command", "Valkis Dreadnought", "Thrai Dreadnought", "Aves Dyson", "Harpia Dyson", "Fleet Warbird Alpha", "Fleet Warbird Beta", "Fleet Warbird Gamma", "Patrol Warbird"),
        CivilianShips = GenerateShipList("Romulan Shuttle", "Romulan Transport", "Romulan Freighter", "Tal Shiar Scout", "Science Vessel", "Medical Transport", "Colony Ship", "Diplomatic Yacht", "Senator's Barge", "Merchant Runner", "Cargo Hauler", "Mining Vessel", "Ore Processor", "Agricultural Ship", "Passenger Liner", "Research Vessel", "Survey Ship", "Communications Ship", "Refugee Transport", "Supply Runner", "Construction Vessel", "Salvage Ship", "Fuel Tanker", "Personnel Ferry", "VIP Transport", "Intel Courier", "Message Runner", "Trade Ship", "Luxury Yacht", "Exploration Vessel", "Long Range Scout", "Deep Space Probe", "Civilian Shuttle", "Work Pod", "Repair Tender", "Tug"),
        MilitaryStructures = GenerateStructureList("Romulan Starbase", "Defense Platform", "Cloaked Minefield Control", "Sensor Array", "Weapons Platform", "Shield Generator", "Fleet Command", "Intelligence Station", "Tal Shiar Outpost", "Border Station", "Listening Post", "Early Warning", "Torpedo Platform", "Disruptor Battery", "Repair Dock Military", "Staging Area", "Fighter Base", "Patrol Hub", "Orbital Fortress", "Defense Satellite", "Mine Layer", "Interdiction Platform", "Singularity Station", "Research Military", "Weapons Testing", "Prison Station", "Interrogation Platform", "Training Base", "Fleet Yard", "Munitions Depot", "Communications Hub", "Relay Station Military", "Checkpoint", "Quarantine Platform", "Defensive Ring", "Command Nexus"),
        CivilianStructures = GenerateStructureList("Trading Station", "Commercial Hub", "Civilian Starbase", "Shipyard", "Drydock", "Research Station", "Science Outpost", "Communications Relay", "Passenger Terminal", "Cargo Depot", "Mining Headquarters", "Ore Processing", "Refinery", "Agricultural Station", "Medical Station", "Hospital Platform", "University Station", "Cultural Center", "Diplomatic Station", "Embassy Platform", "Recreation Hub", "Entertainment Center", "Transit Station", "Hotel Station", "Museum Platform", "Archive Station", "Observatory", "Sensor Array Civilian", "Weather Station", "Construction Yard", "Salvage Depot", "Fuel Depot", "Power Station", "Solar Array", "Habitat Ring", "Colony Support"),
        Buildings = GenerateBuildingList("Senate Building", "Praetor's Palace", "Tal Shiar Headquarters", "Military Academy", "Fleet Command Ground", "Weapons Factory", "Disruptor Assembly", "Power Plant", "Shield Generator Ground", "Barracks", "Training Ground", "Intelligence Hub", "Surveillance Center", "Prison Complex", "Interrogation Center", "Research Institute", "Science Academy", "University", "Library Archive", "Museum of Conquest", "Temple of State", "Cultural Center", "Hospital", "Medical Research", "Agricultural Dome", "Food Processing", "Residential Complex", "Government Housing", "Noble Estate", "Common Housing", "Commercial District", "Trade Center", "Merchant Guild", "Banking Center", "Diplomatic Hall", "Embassy", "Spaceport", "Landing Facility", "Cargo Warehouse", "Industrial Complex", "Mining Headquarters", "Refinery", "Construction Yard", "Communications Tower", "Sensor Station", "Defense Tower", "Civilian Defense", "Memorial"),
        Troops = GenerateTroopList(
            // Regular Military (8)
            "Uhlan Soldier Male", "Uhlan Soldier Female", "Centurion", "Subcenturion", "Commander", "Subcommander", "Admiral Guard", "Praetor Guard",
            // Tal Shiar (6)
            "Tal Shiar Agent", "Tal Shiar Operative", "Tal Shiar Assassin", "Tal Shiar Interrogator", "Tal Shiar Commander", "Tal Shiar Observer",
            // Reman Forces (6)
            "Reman Warrior", "Reman Shock Trooper", "Reman Vanguard", "Reman Berserker", "Reman Shadow Guard", "Reman Scorpion Pilot",
            // Elite Guards (4)
            "Senate Guard", "Honor Guard", "Praetorian Guard", "Imperial Guard",
            // Combat Specialists (6)
            "Romulan Sniper", "Heavy Disruptor Trooper", "Demolitions Expert", "Combat Engineer", "Field Medic", "Communications Officer",
            // Vehicles & Equipment (6)
            "Disruptor Turret", "Plasma Mortar", "Romulan APC", "Scorpion Fighter Pilot", "Scout Speeder", "Cloaked Infiltrator"),
        PortraitVariants = GeneratePortraitList("Male Commander", "Male Centurion", "Male Subcommander", "Male Admiral", "Male Senator", "Male Praetor", "Male Tal Shiar", "Male Civilian", "Male Scientist", "Male Merchant", "Female Commander", "Female Centurion", "Female Subcommander", "Female Admiral", "Female Senator", "Female Tal Shiar", "Female Civilian", "Female Scientist", "Reman Male Warrior", "Reman Male Leader", "Reman Female", "Male Elder", "Female Elder", "Male Youth", "Female Youth", "Male Diplomat", "Female Diplomat", "Male Engineer", "Female Engineer", "Male Doctor", "Female Doctor", "Male Refugee", "Female Refugee", "Romulan-Vulcan Hybrid", "Unification Supporter", "Defector"),
        HouseSymbols = GenerateSymbolList("Star Empire Standard", "Star Empire Ornate", "Tal Shiar", "Senate Seal", "Praetor Seal", "Fleet Command", "Imperial Navy", "Intelligence Division", "Science Division", "Diplomatic Corps", "House Major 1", "House Major 2", "House Major 3", "House Major 4", "House Major 5", "House Minor 1", "House Minor 2", "House Minor 3", "Reman Symbol", "Unification", "Old Republic", "Raptor Standard", "Eagle Standard", "Bird of Prey Symbol", "Commander Rank", "Admiral Rank", "Centurion Rank", "Subcommander Rank", "Uhlan Rank", "Veteran Badge", "Combat Honor", "Intelligence Honor", "Service Medal", "Senate Honor", "Praetor's Guard", "Elite Forces", "Border Defense", "Exploration Corps", "Science Academy", "Medical Corps", "Engineering Corps", "Colonial Authority", "Trade Guild", "Mining Guild", "Agricultural Guild", "Cultural Ministry", "State Religion", "Historical Society")
    };
    
    private FactionProfile CreateFerengiProfile() => new FactionProfile
    {
        Faction = Faction.Ferengi,
        Name = "Ferengi",
        DesignLanguage = "D'Kora warships have TWO FORWARD PRONGS like tuning fork with command pod at rear. Civilian ships are BOXY cargo or SLEEK yachts. NOT circular.",
        ColorScheme = "Orange, gold, bronze, copper, warm metallic tones",
        CivilianDesignLanguage = "Commerce-focused, BOXY cargo haulers OR sleek luxury yachts, NOT fork-shaped",
        CivilianColorScheme = "Orange-gold, product branding, trade company logos",
        Architecture = "Opulent commercial design, gold trimmings, vault-like structures",
        RaceFeatures = "Large lobed ears, sharp teeth, orange-brown wrinkled skin, bald, cunning expression",
        ClothingDetails = "Colorful merchant robes, gold jewelry, rank-indicating headdress",
        HeraldicStyle = "Ferengi Alliance symbol, latinum bar motifs, commercial emblems",
        MilitaryShips = GenerateShipList("D'Kora Marauder", "Nagus Marauder", "Quark Marauder", "Ferengi Raider", "Ferengi Patrol", "Ferengi Escort", "Ferengi Interceptor", "Ferengi Gunship", "Ferengi Frigate", "Ferengi Destroyer", "Ferengi Cruiser", "Ferengi Battlecruiser", "Ferengi Dreadnought", "Acquisition Cruiser", "Profit Cruiser", "Latinum Cruiser", "Commerce Raider", "Privateer", "Mercenary Ship", "Security Vessel", "Defense Ship", "Patrol Craft", "Border Guard", "Customs Vessel", "Enforcement Ship", "Grand Nagus Yacht", "FCA Enforcer", "Liquidator Ship", "Collector Ship", "Retrieval Vessel", "Bounty Hunter", "Repo Ship", "Asset Recovery", "Ferengi Fighter", "Attack Shuttle", "Armed Trader"),
        CivilianShips = GenerateShipList("Ferengi Shuttle", "Cargo Vessel", "Bulk Freighter", "Container Ship", "Tanker", "Passenger Liner", "Luxury Cruise", "Casino Ship", "Entertainment Vessel", "Holosuite Barge", "Trade Ship", "Merchant Vessel", "Commerce Runner", "Supply Ship", "Delivery Vessel", "Mining Ship", "Ore Hauler", "Salvage Vessel", "Auction Ship", "Display Vessel", "Sales Yacht", "Negotiation Ship", "Banking Vessel", "Vault Ship", "Transport", "Personnel Ferry", "Colony Ship", "Construction Vessel", "Repair Tender", "Fuel Tanker", "Food Transport", "Medical Ship", "Gambling Ship", "Tourist Liner", "Exploration Vessel", "Survey Ship"),
        MilitaryStructures = GenerateStructureList("Defense Platform", "Weapons Satellite", "Shield Station", "Patrol Base", "Security Hub", "Enforcement Station", "Customs Station", "Border Post", "Guard Station", "Mercenary Base", "Privateer Haven", "Bounty Station", "Collection Point", "Holding Facility", "Prison Platform", "Interrogation Station", "Security Vault", "Armory Station", "Training Facility", "Fleet Base", "Repair Dock Military", "Munitions Depot", "Sensor Array", "Early Warning", "Communications Military", "Command Station", "Tactical Hub", "Fighter Base", "Intercept Station", "Blockade Station", "Embargo Platform", "Sanction Station", "Seizure Facility", "Confiscation Hub", "Asset Station", "Liquidation Platform"),
        CivilianStructures = GenerateStructureList("Trading Post", "Commerce Station", "Merchant Hub", "Tower of Commerce Style", "Cargo Depot", "Warehouse Station", "Distribution Center", "Auction House", "Sales Platform", "Market Station", "Banking Station", "Vault Station", "Latinum Depository", "Investment Hub", "Stock Exchange", "Casino Station", "Entertainment Hub", "Holosuite Complex", "Resort Station", "Hotel Platform", "Restaurant Station", "Shopping Station", "Mall Platform", "Display Center", "Showroom Station", "Mining Headquarters", "Refinery", "Processing Station", "Salvage Depot", "Repair Station", "Shipyard", "Drydock", "Construction Yard", "Research Station", "Development Hub", "Patent Office"),
        Buildings = GenerateBuildingList("Tower of Commerce", "Grand Nagus Palace", "FCA Headquarters", "Banking Tower", "Latinum Vault", "Trade Center", "Merchant Guild", "Auction House", "Sales Floor", "Negotiation Hall", "Contract Center", "Deal Room", "Casino Complex", "Entertainment Center", "Holosuite Arcade", "Resort Complex", "Hotel Tower", "Restaurant Row", "Shopping Center", "Mall Complex", "Warehouse", "Cargo Hub", "Distribution Center", "Mining Office", "Refinery", "Processing Plant", "Salvage Yard", "Repair Shop", "Shipyard Ground", "Construction Office", "School of Business", "Academy of Acquisition", "Library of Profit", "Museum of Wealth", "Temple of Commerce", "Shrine to Latinum", "Residential Tower", "Noble Estate", "Common Housing", "Worker Housing", "Government Building", "FCA Office", "Liquidator Office", "Enforcer Station", "Customs House", "Spaceport", "Landing Pad", "Defense Tower"),
        Troops = GenerateTroopList(
            // Standard Security (6)
            "Ferengi Guard Male", "Ferengi Guard Female", "Security Officer", "Patrol Guard", "Checkpoint Guard", "Perimeter Guard",
            // FCA/Enforcement (6)
            "FCA Enforcer", "Liquidator", "Debt Collector", "Asset Seizure Agent", "Customs Officer", "Trade Inspector",
            // Mercenaries (6)
            "Ferengi Mercenary", "Hired Gun", "Privateer", "Bounty Hunter", "Contract Soldier", "Security Consultant",
            // Elite/VIP (6)
            "Nagus Guard", "DaiMon Bodyguard", "Vault Guard", "Executive Protection", "Elite Enforcer", "Personal Security",
            // Combat Types (6)
            "Armed Merchant", "Combat Specialist", "Demolitions Expert", "Heavy Weapons", "Sniper", "Combat Medic",
            // Vehicles & Equipment (6)
            "Energy Whip Trooper", "Sonic Disruptor", "Defense Turret", "Security Drone", "Armored Transport", "Pursuit Vehicle"),
        PortraitVariants = GeneratePortraitList("Male DaiMon", "Male Merchant", "Male Trader", "Male Banker", "Male Grand Nagus Type", "Male FCA Agent", "Male Liquidator", "Male Scientist", "Male Engineer", "Male Civilian", "Male Youth", "Male Elder", "Male Worker", "Male Servant", "Female Traditional", "Female Modern", "Female Merchant", "Female Scientist", "Female Leader", "Female Civilian", "Male Rom Type", "Male Quark Type", "Male Nog Type", "Male Zek Type", "Male Brunt Type", "Male Gint Type", "Wealthy Merchant", "Poor Trader", "Criminal", "Smuggler", "Honest Dealer", "Con Artist", "Bartender", "Waiter", "Cook", "Entertainer"),
        HouseSymbols = GenerateSymbolList("Ferengi Alliance", "Grand Nagus Seal", "FCA Symbol", "Liquidator Badge", "DaiMon Rank", "Merchant Guild", "Trade Association", "Banking Cartel", "Mining Consortium", "Shipping Guild", "Entertainment Corp", "Casino Group", "Salvage Guild", "Construction Guild", "Rules of Acquisition", "First Rule", "Tenth Rule", "Acquisition Medal", "Profit Medal", "Commerce Award", "Trade Honor", "Deal Badge", "Contract Seal", "Negotiation Star", "Sales Record", "Best Deal", "Biggest Profit", "Latinum Bar Symbol", "Gold Press Latinum", "Bar of Latinum", "Slip of Latinum", "Strip of Latinum", "Brick of Latinum", "Tower of Commerce Symbol", "Sacred Marketplace", "Divine Treasury", "Vault of Eternal Destitution", "Blessed Exchequer", "Family Seal 1", "Family Seal 2", "Family Seal 3", "Family Seal 4", "Family Seal 5", "Business Logo 1", "Business Logo 2", "Business Logo 3", "Business Logo 4")
    };
    
    private FactionProfile CreateCardassianProfile() => new FactionProfile
    {
        Faction = Faction.Cardassian,
        Name = "Cardassian",
        DesignLanguage = "Spade/arrowhead shape with pointed bow, distinctive dorsal spine ridge, angular armor plating, no saucers",
        ColorScheme = "Brown, tan, beige, yellow accents, ochre tones",
        CivilianDesignLanguage = "Still utilitarian but less military, ore processing aesthetic, functional design",
        CivilianColorScheme = "Tan, brown, industrial grey, mining company markings",
        Architecture = "Brutal authoritarian, angular intimidating structures, surveillance everywhere",
        RaceFeatures = "Grey scaly skin, neck ridges, spoon-shaped forehead marking, cold expression",
        ClothingDetails = "Brown-grey military armor, scaled texture, Cardassian Union insignia",
        HeraldicStyle = "Cardassian Union symbol, angular geometric patterns, surveillance eye motifs",
        MilitaryShips = GenerateShipList("Galor Cruiser", "Keldon Cruiser", "Hideki Patrol", "Legate Warship", "Gul Cruiser", "Hutet Battlecruiser", "Rasilak Destroyer", "Groumall Freighter Armed", "Damar Cruiser", "Dukat Flagship", "Cardassian Dreadnought", "Cardassian Frigate", "Cardassian Escort", "Cardassian Scout", "Cardassian Fighter", "Cardassian Interceptor", "Border Patrol", "Customs Enforcer", "Occupation Cruiser", "Orbital Patrol", "System Defense", "Strike Cruiser", "Assault Ship", "Troop Transport Armed", "Command Cruiser", "Fleet Carrier", "Heavy Cruiser", "Light Cruiser", "Fast Attack", "Missile Cruiser", "Torpedo Ship", "Gunship", "Corvette", "Defense Ship", "Obsidian Order Ship", "Intelligence Vessel"),
        CivilianShips = GenerateShipList("Cardassian Shuttle", "Cardassian Transport", "Cargo Freighter", "Bulk Hauler", "Ore Carrier", "Mining Ship", "Processing Vessel", "Refinery Ship", "Colony Ship", "Settler Transport", "Worker Transport", "Slave Transport", "Passenger Ship", "Diplomatic Vessel", "Medical Ship", "Hospital Ship", "Supply Freighter", "Food Transport", "Agricultural Ship", "Construction Vessel", "Engineering Ship", "Repair Tender", "Salvage Ship", "Tanker", "Fuel Ship", "Research Vessel", "Science Ship", "Survey Ship", "Exploration Vessel", "Communications Ship", "Relay Vessel", "Courier", "VIP Transport", "Gul's Yacht", "Legate's Barge", "Civilian Shuttle"),
        MilitaryStructures = GenerateStructureList("Cardassian Starbase", "Orbital Defense", "Weapons Platform", "Shield Station", "Terok Nor Type Station", "Command Station", "Fleet Base", "Repair Dock", "Construction Yard", "Fighter Base", "Patrol Station", "Border Fortress", "Listening Post", "Sensor Array", "Early Warning", "Minefield Control", "Torpedo Station", "Disruptor Platform", "Siege Station", "Bombardment Platform", "Troop Station", "Prison Station", "Labor Camp Orbital", "Interrogation Station", "Obsidian Order Base", "Intelligence Hub", "Communications Array", "Relay Station Military", "Staging Area", "Supply Depot", "Munitions Station", "Training Station", "Academy Orbital", "Medical Military", "Quarantine Platform", "Checkpoint Station"),
        CivilianStructures = GenerateStructureList("Trading Station", "Commercial Hub", "Ore Processing Station", "Mining Station", "Refinery Station", "Cargo Depot", "Warehouse Station", "Distribution Hub", "Civilian Starbase", "Drydock", "Shipyard", "Construction Platform", "Research Station", "Science Outpost", "Medical Station", "Hospital Platform", "Communications Relay", "Subspace Booster", "Passenger Terminal", "Transit Hub", "Hotel Station", "Recreation Platform", "Entertainment Hub", "Agricultural Station", "Food Processing", "Water Processing", "Power Station", "Solar Array", "Habitat Station", "Colony Support", "Worker Housing Orbital", "Administrative Station", "Government Hub", "Cultural Center", "Archive Station", "Museum Platform"),
        Buildings = GenerateBuildingList("Central Command", "Obsidian Order HQ", "Gul's Residence", "Legate's Palace", "Military Academy", "Fleet Command", "Weapons Factory", "Disruptor Assembly", "Shipyard Ground", "Barracks", "Training Ground", "Prison Complex", "Labor Camp", "Interrogation Center", "Detention Facility", "Ore Processing", "Mining Complex", "Refinery", "Industrial Center", "Power Plant", "Shield Generator", "Communications Tower", "Surveillance Center", "Sensor Station", "Government Building", "Administrative Center", "Colonial Office", "Occupation HQ", "Diplomatic Hall", "Trade Center", "Commercial District", "Merchant Quarter", "Residential Block", "Worker Housing", "Elite Housing", "Hospital", "Medical Center", "Research Institute", "Science Academy", "University", "Library Archive", "Cultural Center", "Temple of State", "Memorial Hall", "Museum", "Agricultural Dome", "Food Distribution", "Spaceport"),
        Troops = GenerateTroopList(
            // Regular Military Ranks (8)
            "Garresh Soldier", "Gil Junior Officer", "Glinn Officer", "Dalin Senior", "Gul Commander", "Legate Guard", "Military Recruit", "Veteran Soldier",
            // Obsidian Order (6)
            "Obsidian Order Agent", "Obsidian Order Operative", "Obsidian Order Interrogator", "Obsidian Order Assassin", "Obsidian Order Commander", "Order Field Agent",
            // Occupation Forces (6)
            "Occupation Soldier", "Prison Guard", "Labor Camp Overseer", "Population Control", "Resistance Hunter", "Security Patrol",
            // Combat Specialists (6)
            "Cardassian Sniper", "Heavy Weapons Trooper", "Demolitions Expert", "Combat Engineer", "Field Medic", "Communications Officer",
            // Special Units (4)
            "Gul Personal Guard", "Central Command Guard", "Elite Strike Team", "First Order Soldier",
            // Vehicles & Equipment (6)
            "Disruptor Turret", "Orbital Strike Beacon", "Ground APC", "Cardassian Tank", "Patrol Speeder", "Siege Platform"),
        PortraitVariants = GeneratePortraitList("Male Gul", "Male Legate", "Male Glinn", "Male Gil", "Male Soldier", "Male Obsidian Order", "Male Scientist", "Male Engineer", "Male Doctor", "Male Civilian", "Male Worker", "Male Merchant", "Female Military Officer", "Female Scientist", "Female Doctor", "Female Civilian", "Female Agent", "Female Engineer", "Male Elder", "Female Elder", "Male Youth", "Female Youth", "Garak Type", "Dukat Type", "Damar Type", "Tain Type", "Ziyal Type Half", "Resistance Member", "Dissident", "Loyalist", "Veteran", "Recruit", "Intelligence Officer", "Diplomatic Attache", "Colonial Administrator", "Mining Foreman"),
        HouseSymbols = GenerateSymbolList("Cardassian Union", "Central Command", "Obsidian Order", "Military Command", "Fleet Command", "Ground Forces", "Intelligence Division", "Science Division", "Colonial Authority", "Mining Authority", "Trade Bureau", "Gul Rank", "Legate Rank", "Glinn Rank", "Gil Rank", "Dalin Rank", "Garresh Rank", "Order of Merit", "Combat Medal", "Service Medal", "Occupation Badge", "Victory Badge", "Intelligence Star", "Science Award", "Family Crest 1", "Family Crest 2", "Family Crest 3", "Family Crest 4", "Family Crest 5", "House Symbol 1", "House Symbol 2", "House Symbol 3", "Colonial Badge", "Mining Guild", "Merchant Guild", "Engineering Corps", "Medical Corps", "Judicial Authority", "Religious Symbol", "State Symbol", "Historical Badge", "Cultural Award", "Educational Seal", "Diplomatic Corps", "Loyalty Badge", "Veteran Badge", "Recruit Badge")
    };
    
    private FactionProfile CreateBorgProfile() => new FactionProfile
    {
        Faction = Faction.Borg,
        Name = "Borg",
        DesignLanguage = "Cubic and spherical geometric shapes, chaotic mechanical surfaces, tubes and conduits",
        ColorScheme = "Dark grey, black, sickly green glow, metallic with organic patches",
        CivilianDesignLanguage = "No civilian - all serves the Collective. Smaller geometric harvester/probe shapes",
        CivilianColorScheme = "Same as military - dark grey, black, green glow",
        Architecture = "Assimilated chaos, purely functional, exposed machinery, hive-like",
        RaceFeatures = "Pale grey skin, cybernetic implants, eye piece, tubes in skin, emotionless",
        ClothingDetails = "Black bodysuit covered in cybernetic components, exoskeleton pieces",
        HeraldicStyle = "Geometric Borg patterns, circuitry designs, cube motifs",
        MilitaryShips = GenerateShipList("Borg Cube", "Borg Tactical Cube", "Borg Sphere", "Borg Diamond", "Borg Probe", "Borg Scout", "Borg Interceptor", "Borg Detector", "Borg Assimilator", "Borg Harvester", "Borg Collector", "Borg Regen Ship", "Borg Queen Ship", "Borg Command Ship", "Borg Fusion Cube", "Borg Octahedron", "Borg Pyramid", "Borg Cylinder", "Borg Wedge", "Borg Obelisk", "Borg Assembler", "Borg Constructor", "Borg Repair Drone", "Borg Carrier", "Borg Invasion Cube", "Borg Transwarp Ship", "Borg Juggernaut", "Borg Dreadnought", "Borg Apex", "Borg Unimatrix Ship", "Borg Super Cube", "Borg Mobile Factory", "Borg Assimilation Ship", "Borg Adaptation Vessel", "Borg Advanced Probe", "Borg Queen Diamond"),
        CivilianShips = GenerateShipList("Borg Probe Small", "Borg Scout Drone", "Borg Harvester", "Borg Collector", "Borg Resource Gatherer", "Borg Mining Drone", "Borg Processing Unit", "Borg Transport Drone", "Borg Cargo Module", "Borg Supply Sphere", "Borg Repair Drone", "Borg Construction Drone", "Borg Salvage Unit", "Borg Recovery Vessel", "Borg Beacon Drone", "Borg Relay Drone", "Borg Sensor Probe", "Borg Scanner", "Borg Data Collector", "Borg Communication Node", "Borg Regeneration Pod", "Borg Maturation Vessel", "Borg Vinculum Ship", "Borg Medical Drone", "Borg Research Probe", "Borg Analysis Unit", "Borg Sample Collector", "Borg Specimen Transport", "Borg Adaptation Drone", "Borg Technology Probe", "Borg Nanoprobe Spreader", "Borg Assimilation Shuttle", "Borg Mini Sphere", "Borg Mini Cube", "Borg Worker", "Borg Utility"),
        MilitaryStructures = GenerateStructureList("Borg Unicomplex", "Borg Transwarp Hub", "Borg Transwarp Gate", "Borg Defense Node", "Borg Tactical Node", "Borg Weapons Array", "Borg Shield Matrix", "Borg Sensor Grid", "Borg Command Nexus", "Borg Queen Chamber", "Borg Central Plexus", "Borg Vinculum Station", "Borg Subspace Beacon", "Borg Relay Station", "Borg Interlink Node", "Borg Assembly Hub", "Borg Shipyard Cube", "Borg Construction Matrix", "Borg Repair Bay", "Borg Regeneration Hub", "Borg Adaptation Center", "Borg Research Node", "Borg Assimilation Center", "Borg Processing Hub", "Borg Maturation Chamber", "Borg Drone Storage", "Borg Holding Area", "Borg Distribution Node", "Borg Power Station", "Borg Energy Matrix", "Borg Defense Platform", "Borg Mine Layer", "Borg Interdiction Node", "Borg Early Warning", "Borg Border Station", "Borg Forward Base"),
        CivilianStructures = GenerateStructureList("Borg Mining Station", "Borg Ore Processor", "Borg Resource Hub", "Borg Harvesting Node", "Borg Collection Point", "Borg Storage Cube", "Borg Cargo Bay", "Borg Distribution Center", "Borg Transport Hub", "Borg Relay Point", "Borg Communication Array", "Borg Subspace Node", "Borg Data Center", "Borg Analysis Hub", "Borg Research Station", "Borg Technology Center", "Borg Adaptation Lab", "Borg Nanite Factory", "Borg Component Plant", "Borg Assembly Line", "Borg Drone Factory", "Borg Maturation Facility", "Borg Medical Bay", "Borg Regeneration Station", "Borg Maintenance Hub", "Borg Repair Station", "Borg Recycling Node", "Borg Salvage Station", "Borg Energy Collector", "Borg Power Node", "Borg Solar Harvester", "Borg Fusion Plant", "Borg Singularity Tap", "Borg Subspace Tap", "Borg Resource Probe", "Borg Survey Node"),
        Buildings = GenerateBuildingList("Borg Assimilation Center", "Borg Processing Hub", "Borg Maturation Chamber", "Borg Regeneration Alcove Complex", "Borg Vinculum Chamber", "Borg Central Node", "Borg Queen Chamber", "Borg Drone Storage", "Borg Holding Chamber", "Borg Data Core", "Borg Memory Hub", "Borg Analysis Center", "Borg Adaptation Lab", "Borg Technology Center", "Borg Research Complex", "Borg Nanite Factory", "Borg Component Assembly", "Borg Drone Factory", "Borg Weapons Plant", "Borg Shield Generator", "Borg Power Core", "Borg Energy Plant", "Borg Resource Processor", "Borg Mining Hub", "Borg Ore Refinery", "Borg Metal Works", "Borg Organic Processing", "Borg Bio Lab", "Borg Medical Center", "Borg Repair Facility", "Borg Maintenance Hub", "Borg Communication Tower", "Borg Subspace Array", "Borg Relay Hub", "Borg Sensor Station", "Borg Defense Tower", "Borg Shield Node", "Borg Weapon Emplacement", "Borg Landing Zone", "Borg Transport Hub", "Borg Distribution Center", "Borg Storage Vault", "Borg Archive", "Borg History Core", "Borg Collective Memory", "Borg Unimatrix Building", "Borg Fusion Hub", "Borg Transwarp Node"),
        Troops = GenerateTroopList(
            // Drone Types by Origin (12)
            "Borg Drone Human", "Borg Drone Klingon", "Borg Drone Romulan", "Borg Drone Vulcan", "Borg Drone Cardassian", "Borg Drone Ferengi",
            "Borg Drone Andorian", "Borg Drone Bajoran", "Borg Drone Species 8472 Adapted", "Borg Drone Hirogen", "Borg Drone Kazon", "Borg Drone Unknown Species",
            // Specialized Drones (12)
            "Borg Tactical Drone", "Borg Heavy Drone", "Borg Medical Drone", "Borg Engineering Drone", "Borg Science Drone", "Borg Worker Drone",
            "Borg Combat Drone Elite", "Borg Assault Drone", "Borg Guardian Drone", "Borg Interlink Drone", "Borg Command Drone", "Borg Queen Guard",
            // Equipment & Special (12)
            "Borg Assimilation Turret", "Borg Shield Generator Mobile", "Borg Regeneration Unit Field", "Borg Adaptation Node Mobile", "Borg Cutting Beam Turret", "Borg Tractor Node",
            "Borg Scout Mini", "Borg Probe Ground", "Borg Nanoprobe Cloud", "Borg Vinculum Mobile", "Borg Queen Avatar", "Borg Collective Nexus Mobile"),
        PortraitVariants = GeneratePortraitList("Borg Drone Human Male", "Borg Drone Human Female", "Borg Drone Klingon", "Borg Drone Romulan", "Borg Drone Vulcan", "Borg Drone Cardassian", "Borg Drone Ferengi", "Borg Drone Andorian", "Borg Drone Bajoran", "Borg Drone Betazoid", "Borg Drone Bolian", "Borg Drone Trill", "Borg Queen Human Type", "Borg Queen Species Unknown", "Borg Queen Young", "Borg Queen Ancient", "Seven of Nine Type", "Hugh Type Liberated", "Locutus Type", "Tactical Drone Close", "Medical Drone Close", "Engineering Drone Close", "Science Drone Close", "Worker Drone Close", "Heavy Drone Close", "Scout Drone Close", "Child Drone", "Elder Drone", "Fresh Assimilated", "Fully Integrated", "Malfunctioning Drone", "Disconnected Drone", "Resisting Assimilation", "Hybrid Drone", "Enhanced Drone", "Prototype Drone"),
        HouseSymbols = GenerateSymbolList("Borg Collective Symbol", "Borg Cube Glyph", "Borg Sphere Glyph", "Borg Diamond Glyph", "Borg Queen Mark", "Borg Unimatrix 01", "Borg Unimatrix 02", "Borg Unimatrix 03", "Borg Unimatrix Zero", "Borg Primary", "Borg Secondary", "Borg Tertiary", "Borg Grid Pattern", "Borg Circuit Pattern", "Borg Hexagon Grid", "Borg Square Grid", "Borg Conduit Pattern", "Borg Neural Pattern", "Borg Adaptation Glyph", "Borg Assimilation Mark", "Borg Perfection Symbol", "Borg Unity Mark", "Borg Collective Mind", "Borg Hive Symbol", "Borg Designation 1 of 1", "Borg Tactical Glyph", "Borg Medical Glyph", "Borg Engineering Glyph", "Borg Science Glyph", "Borg Worker Glyph", "Borg Scout Glyph", "Borg Command Glyph", "Borg Transwarp Symbol", "Borg Vinculum Pattern", "Borg Regeneration Symbol", "Borg Species Catalog", "Borg Technology Index", "Borg Resistance Futile", "Borg We Are Borg", "Borg Assimilate Glyph", "Borg Adapt Glyph", "Borg Overcome Glyph", "Borg Collective Voice", "Borg Green Glow Pattern", "Borg Power Symbol", "Borg Data Stream", "Borg Memory Pattern", "Borg History Archive")
    };
    
    private FactionProfile CreateDominionProfile() => new FactionProfile
    {
        Faction = Faction.Dominion,
        Name = "Dominion",
        DesignLanguage = "Organic beetle-like forms, compact powerful silhouettes, bioluminescent accents",
        ColorScheme = "Purple, violet, silver, bioluminescent lavender glow",
        CivilianDesignLanguage = "Organic grown shapes, transport-focused, Vorta administrative elegance",
        CivilianColorScheme = "Lighter purple, violet, silver, subtle glow",
        Architecture = "Organic yet militaristic, curved bio-mechanical structures",
        RaceFeatures = "Vorta: pale skin, violet eyes. Jem'Hadar: grey reptilian, bony ridges",
        ClothingDetails = "Vorta: elegant robes. Jem'Hadar: grey-purple battle armor",
        HeraldicStyle = "Dominion symbol variations, organic flowing patterns",
        MilitaryShips = GenerateShipList("Jem'Hadar Fighter", "Jem'Hadar Attack Ship", "Jem'Hadar Battle Cruiser", "Jem'Hadar Battleship", "Jem'Hadar Dreadnought", "Jem'Hadar Super Carrier", "Jem'Hadar Strike Ship", "Jem'Hadar Heavy Escort", "Jem'Hadar Vanguard", "Jem'Hadar Recon Ship", "Dominion Battleship", "Dominion Dreadnought", "Dominion Command Ship", "Dominion Flagship", "Breen Warship Ally", "Cardassian Galor Ally", "Founder Vessel", "Vorta Command Ship", "Dominion Carrier", "Dominion Heavy Cruiser", "Dominion Strike Cruiser", "Dominion Patrol Ship", "Dominion Border Ship", "Dominion Escort", "Dominion Frigate", "Dominion Destroyer", "Dominion Interceptor", "Dominion Scout", "Dominion Probe", "Dominion Fighter Wing", "Dominion Bomber", "Dominion Assault Ship", "Dominion Troop Transport", "Dominion Siege Ship", "Dominion Mine Layer", "Dominion Advanced Fighter"),
        CivilianShips = GenerateShipList("Dominion Shuttle", "Vorta Transport", "Dominion Freighter", "Dominion Cargo Ship", "Dominion Supply Ship", "Ketracel White Transport", "Dominion Colony Ship", "Dominion Settler Ark", "Vorta Diplomatic Yacht", "Founder Transport", "Great Link Vessel", "Dominion Medical Ship", "Dominion Hospital Vessel", "Jem'Hadar Wounded Transport", "Dominion Science Vessel", "Dominion Research Ship", "Dominion Survey Craft", "Dominion Mining Ship", "Dominion Ore Processor", "Dominion Construction Ship", "Dominion Repair Vessel", "Dominion Salvage Ship", "Dominion Tanker", "Dominion Fuel Ship", "Dominion Agricultural Ship", "Dominion Bio Transport", "Dominion Cloning Ship", "Dominion Hatchery Vessel", "Dominion Communications Ship", "Dominion Relay Vessel", "Dominion Courier", "Dominion VIP Transport", "Dominion Luxury Vessel", "Dominion Civilian Shuttle", "Dominion Work Pod", "Dominion Tug"),
        MilitaryStructures = GenerateStructureList("Dominion Starbase", "Dominion Orbital Fortress", "Jem'Hadar Base", "Dominion Defense Platform", "Dominion Weapons Array", "Dominion Shield Station", "Dominion Sensor Grid", "Dominion Command Station", "Dominion Fleet Hub", "Dominion Staging Area", "Dominion Repair Dock", "Dominion Fighter Bay", "Dominion Munitions Depot", "Dominion Training Center", "Ketracel White Facility Orbital", "Cloning Facility Orbital", "Jem'Hadar Barracks Station", "Vorta Command Post", "Founder Sanctuary Orbital", "Dominion Intelligence Hub", "Dominion Communications Array", "Dominion Early Warning", "Dominion Border Station", "Dominion Checkpoint", "Dominion Patrol Base", "Dominion Listening Post", "Dominion Mine Field Control", "Dominion Interdiction Platform", "Dominion Siege Platform", "Dominion Bombardment Station", "Dominion Troop Station", "Dominion Prison Platform", "Dominion Holding Facility", "Dominion Quarantine Station", "Dominion Research Military", "Dominion Weapons Testing"),
        CivilianStructures = GenerateStructureList("Dominion Trading Post", "Dominion Commerce Hub", "Dominion Civilian Station", "Dominion Shipyard", "Dominion Drydock", "Dominion Construction Yard", "Dominion Mining Station", "Dominion Ore Processing", "Dominion Refinery", "Dominion Cargo Depot", "Dominion Distribution Hub", "Dominion Passenger Terminal", "Dominion Transit Station", "Great Link Sanctuary", "Founder Temple Station", "Vorta Administrative Hub", "Dominion Research Station", "Dominion Science Platform", "Dominion Medical Station", "Dominion Hospital Platform", "Dominion Cloning Center", "Jem'Hadar Hatchery Station", "Ketracel White Factory", "Dominion Agricultural Station", "Dominion Bio Farm", "Dominion Food Processing", "Dominion Communications Relay", "Dominion Entertainment Hub", "Dominion Cultural Center", "Dominion Education Station", "Dominion Archive", "Dominion Power Station", "Dominion Energy Hub", "Dominion Solar Collector", "Dominion Habitat Ring", "Dominion Colony Support"),
        Buildings = GenerateBuildingList("Dominion Command Center", "Founder Palace", "Great Link Pool", "Vorta Administrative Center", "Jem'Hadar Barracks", "Jem'Hadar Training Ground", "Ketracel White Facility", "Cloning Center", "Hatchery Complex", "Dominion Military Academy", "Dominion Weapons Factory", "Dominion Ship Assembly", "Dominion Power Plant", "Dominion Shield Generator", "Dominion Communications Tower", "Dominion Sensor Station", "Dominion Defense Tower", "Dominion Prison Complex", "Dominion Holding Center", "Dominion Interrogation", "Vorta Residence", "Founder Sanctuary", "Great Link Chamber", "Dominion Temple", "Dominion Worship Center", "Dominion Research Institute", "Dominion Science Academy", "Dominion Medical Center", "Dominion Hospital", "Dominion Agricultural Dome", "Dominion Bio Farm", "Dominion Food Processing", "Dominion Mining Complex", "Dominion Ore Refinery", "Dominion Industrial Hub", "Dominion Construction Yard", "Dominion Cargo Warehouse", "Dominion Trade Center", "Dominion Market", "Dominion Residential Complex", "Vorta Housing", "Jem'Hadar Housing", "Worker Housing", "Dominion Spaceport", "Dominion Landing Zone", "Dominion Transport Hub", "Dominion Memorial", "Dominion Victory Monument"),
        Troops = GenerateTroopList(
            // Jem'Hadar Warriors (12)
            "Jem'Hadar Soldier", "Jem'Hadar Veteran", "Jem'Hadar First", "Jem'Hadar Second", "Jem'Hadar Third", "Jem'Hadar Elder",
            "Jem'Hadar Shock Trooper", "Jem'Hadar Assault Trooper", "Jem'Hadar Berserker", "Jem'Hadar Honor Guard", "Jem'Hadar Alpha", "Jem'Hadar Honored Elder",
            // Combat Specialists (8)
            "Jem'Hadar Sniper", "Jem'Hadar Heavy Weapons", "Jem'Hadar Demolitions", "Jem'Hadar Combat Engineer", "Jem'Hadar Medic", "Jem'Hadar Scout",
            "Jem'Hadar Shrouded Infiltrator", "Jem'Hadar Communications",
            // Vorta (6)
            "Vorta Commander", "Vorta Field Supervisor", "Vorta Administrator", "Vorta Interrogator", "Vorta Scientist", "Vorta Diplomat",
            // Support & Equipment (10)
            "Founder Shapeshifter Combat", "Founder Infiltrator", "Dominion Turret", "Polaron Cannon", "Dominion APC", "Dominion Tank",
            "Jem'Hadar Fighter Pilot", "Ketracel Dispenser", "Dominion Probe Ground", "Dominion Shield Generator Mobile"),
        PortraitVariants = GeneratePortraitList("Jem'Hadar Soldier Young", "Jem'Hadar Veteran Scarred", "Jem'Hadar First Commander", "Jem'Hadar Elder", "Jem'Hadar Honored Elder", "Vorta Male Weyoun Type", "Vorta Male Different Clone", "Vorta Female", "Vorta Male Elder", "Vorta Female Elder", "Founder Male Form", "Founder Female Form", "Founder Elder Form", "Founder Odo Type", "Founder Unknown Form", "Jem'Hadar Alpha", "Jem'Hadar Shrouded", "Vorta Clone Fresh", "Vorta Clone Degraded", "Jem'Hadar Newborn", "Jem'Hadar Mature", "Vorta Administrator", "Vorta Scientist", "Vorta Diplomat", "Vorta Commander Field", "Jem'Hadar Berserker", "Jem'Hadar Calm", "Dominion Prisoner", "Liberated Jem'Hadar", "Defector Vorta", "Founder in Link", "Vorta at Work", "Jem'Hadar Training", "Victory Celebration", "Before Battle", "After Battle"),
        HouseSymbols = GenerateSymbolList("Dominion Symbol Standard", "Dominion Symbol Ornate", "Founder Sigil", "Great Link Symbol", "Vorta Administrative", "Jem'Hadar Military", "Jem'Hadar First Rank", "Jem'Hadar Second Rank", "Jem'Hadar Third Rank", "Jem'Hadar Fourth Rank", "Jem'Hadar Fifth Rank", "Jem'Hadar Sixth Rank", "Jem'Hadar Seventh Rank", "Jem'Hadar Honored Elder", "Ketracel White Symbol", "Victory is Life", "Obedience Brings Victory", "Founders Will", "Dominion Fleet", "Dominion Ground Forces", "Dominion Intelligence", "Dominion Science", "Dominion Medical", "Dominion Engineering", "Dominion Colonial", "Dominion Trade", "Dominion Mining", "Alpha Quadrant Conquest", "Beta Quadrant Symbol", "Gamma Quadrant Home", "Wormhole Symbol", "DS9 Conquest Mark", "Cardassia Alliance", "Breen Alliance", "Son'a Alliance", "Species Catalog Symbol", "Cloning Facility Mark", "Hatchery Symbol", "Training Complete", "Battle Honor", "Founder Blessing", "Vorta Service Medal", "Jem'Hadar Valor", "Dominion Victory", "Campaign Badge", "Sector Control", "Fleet Commander", "Ground Commander")
    };
    
    private FactionProfile CreateBreenProfile() => new FactionProfile
    {
        Faction = Faction.Breen,
        Name = "Breen",
        DesignLanguage = "Asymmetric unusual alien shapes, refrigeration aesthetic, mysterious enclosed designs",
        ColorScheme = "Teal, green, metallic ice-blue, cold crystalline accents",
        CivilianDesignLanguage = "Cold utilitarian designs, refrigeration integrated, mysterious enclosed forms",
        CivilianColorScheme = "Ice blue, silver, pale teal, frost white accents",
        Architecture = "Alien and cold, crystalline structures, enclosed mysterious facilities",
        RaceFeatures = "Fully enclosed in refrigeration suit, snout-like helmet, visor, mysterious",
        ClothingDetails = "Full body refrigeration suit, metallic green-grey, helmet with vocoder",
        HeraldicStyle = "Breen Confederacy symbol, crystalline angular patterns, ice motifs",
        MilitaryShips = GenerateShipList(
            "Chel Grett Cruiser", "Plesh Brek Raider", "Sar Theln Carrier", "Rezreth Dreadnought",
            "Bleth Choas Heavy Cruiser", "Chel Boalg Warship", "Breen Frigate", "Breen Destroyer",
            "Breen Light Cruiser", "Breen Battlecruiser", "Breen Patrol Ship", "Breen Scout",
            "Breen Interceptor", "Breen Fighter", "Breen Assault Ship", "Breen Strike Cruiser",
            "Breen Torpedo Ship", "Breen Siege Cruiser", "Breen Command Ship", "Breen Flagship",
            "Energy Dampener Cruiser", "Breen Fast Attack", "Breen Border Patrol", "Breen Escort",
            "Breen Corvette", "Breen Gunship", "Thot Command Cruiser", "Breen Mine Layer",
            "Breen Blockade Runner", "Breen Stealth Ship", "Breen Intelligence Vessel", "Breen Troop Transport",
            "Cryo Assault Ship", "Breen Defense Ship", "Breen Battle Carrier", "Breen Super Dreadnought"),
        CivilianShips = GenerateShipList(
            "Breen Shuttle", "Breen Cargo Ship", "Breen Freighter", "Breen Transport",
            "Breen Passenger Liner", "Breen Colony Ship", "Breen Mining Vessel", "Breen Ore Hauler",
            "Breen Tanker", "Breen Construction Ship", "Breen Repair Vessel", "Breen Medical Ship",
            "Breen Science Vessel", "Breen Survey Craft", "Breen Diplomatic Transport", "Breen Courier",
            "Breen VIP Transport", "Breen Cargo Hauler", "Breen Ice Harvester", "Breen Refrigeration Ship",
            "Breen Supply Ship", "Breen Salvage Ship", "Breen Communication Ship", "Breen Fuel Transport",
            "Breen Agricultural Ship", "Breen Hospital Ship", "Breen Settler Ark", "Breen Work Pod",
            "Breen Tug", "Breen Luxury Yacht", "Breen Trade Ship", "Breen Industrial Hauler",
            "Breen Cryo Transport", "Breen Research Ship", "Breen Probe Ship", "Breen Civilian Shuttle"),
        MilitaryStructures = GenerateStructureList(
            "Breen Starbase", "Breen Orbital Fortress", "Breen Defense Platform", "Breen Weapons Array",
            "Breen Shield Station", "Breen Sensor Grid", "Breen Command Station", "Breen Fleet Depot",
            "Breen Repair Dock", "Breen Fighter Bay", "Breen Munitions Depot", "Breen Training Center",
            "Breen Intelligence Hub", "Breen Communication Array", "Breen Early Warning Post", "Breen Border Station",
            "Breen Checkpoint", "Breen Patrol Base", "Breen Listening Post", "Breen Mine Field Control",
            "Breen Interdiction Platform", "Breen Siege Platform", "Breen Prison Platform", "Breen Holding Facility",
            "Breen Weapons Testing", "Energy Dampener Station", "Breen Cryo Weapons Lab", "Breen Staging Area",
            "Breen Torpedo Platform", "Breen Guard Station", "Breen Armory Station", "Breen Medical Outpost",
            "Breen Quarantine Station", "Breen Operations Base", "Breen Logistics Hub", "Breen Strategic Command"),
        CivilianStructures = GenerateStructureList(
            "Breen Trading Post", "Breen Commerce Hub", "Breen Civilian Station", "Breen Shipyard",
            "Breen Drydock", "Breen Construction Yard", "Breen Mining Station", "Breen Ore Processing",
            "Breen Refinery", "Breen Cargo Depot", "Breen Distribution Hub", "Breen Passenger Terminal",
            "Breen Transit Station", "Breen Research Station", "Breen Science Platform", "Breen Medical Station",
            "Breen Hospital Platform", "Breen Power Station", "Breen Energy Hub", "Breen Habitat Module",
            "Breen Colony Support", "Breen Cryo Research Lab", "Breen Ice Mining Station", "Breen Refrigeration Hub",
            "Breen Agricultural Station", "Breen Food Processing", "Breen Communication Relay", "Breen Entertainment Complex",
            "Breen Cultural Center", "Breen Education Station", "Breen Archive", "Breen Solar Collector",
            "Breen Geothermal Station", "Breen Recreation Module", "Breen Market Station", "Breen Diplomatic Station"),
        Buildings = GenerateBuildingList(
            "Breen Confederacy Command", "Thot's Citadel", "Cryo Barracks", "Breen Military Academy",
            "Breen Weapons Factory", "Breen Ship Assembly", "Breen Power Plant", "Breen Shield Generator",
            "Breen Communication Tower", "Breen Sensor Station", "Breen Defense Tower", "Breen Prison Complex",
            "Breen Interrogation Center", "Energy Dampener Factory", "Cryo Weapons Lab", "Breen Intelligence HQ",
            "Breen Training Ground", "Breen Armory", "Breen Munitions Store", "Breen Guard Post",
            "Breen War Memorial", "Breen Confederacy Hall", "Breen Administrative Center", "Breen Residential Complex",
            "Breen Housing Block", "Breen Ice Palace", "Breen Refrigeration Plant", "Cryo Medical Center",
            "Breen Hospital", "Breen Research Institute", "Breen Science Academy", "Breen Mining Complex",
            "Breen Ore Refinery", "Breen Industrial Hub", "Breen Cargo Warehouse", "Breen Trade Center",
            "Breen Market Hall", "Breen Spaceport", "Breen Landing Zone", "Breen Transport Hub",
            "Breen Food Processing", "Breen Agricultural Dome", "Breen Water Reclamation", "Breen Geothermal Plant",
            "Breen Archive Building", "Breen Cultural Hall", "Breen Entertainment Hub", "Monument of Cold"),
        Troops = GenerateTroopList(
            "Breen Soldier", "Breen Heavy Trooper", "Breen Shock Trooper", "Breen Cryo Operative",
            "Breen Energy Dampener Tech", "Breen Sniper", "Breen Demolitions Expert", "Breen Officer",
            "Breen Elite Guard", "Breen Commander", "Breen Scout", "Breen Infiltrator",
            "Breen Combat Engineer", "Breen Medic", "Breen Thot Guard", "Breen Special Forces"),
        PortraitVariants = GeneratePortraitList(
            "Breen Thot Commander", "Breen Military Officer", "Breen Junior Officer", "Breen Elite Warrior",
            "Breen Veteran Soldier", "Breen Scout Operative", "Breen Intelligence Agent", "Breen Diplomat",
            "Breen Ambassador", "Breen Trade Envoy", "Breen Merchant", "Breen Civilian Worker",
            "Breen Engineer", "Breen Scientist", "Breen Medic", "Breen Ship Captain",
            "Breen Fleet Admiral", "Breen Ground Commander", "Breen Special Ops", "Breen Pilot",
            "Breen Technician", "Breen Mining Foreman", "Breen Construction Lead", "Breen Guard Sergeant",
            "Breen Prison Warden", "Breen Communications Officer", "Breen Weapons Expert", "Breen Navigator",
            "Breen Supply Officer", "Breen Confederacy Elder", "Breen Council Member", "Breen Provincial Governor",
            "Breen District Chief", "Breen Youth Cadet", "Breen Cultural Attaché", "Breen Refugee"),
        HouseSymbols = GenerateSymbolList(
            "Breen Confederacy Seal", "Breen Confederacy Ornate", "Breen Military Command", "Breen Fleet Insignia",
            "Breen Intelligence Division", "Breen Ground Forces", "Breen Special Operations", "Energy Dampener Corps",
            "Breen Cryo Division", "Thot's Seal", "Provincial Seal Alpha", "Provincial Seal Beta",
            "Provincial Seal Gamma", "Provincial Seal Delta", "Frost Blade Emblem", "Ice Shard Insignia",
            "Crystal Spire Symbol", "Frozen Star Crest", "Glacial Shield", "Arctic Wind Mark",
            "Permafrost Seal", "Tundra Guard", "Blizzard Corps", "Hailstorm Division",
            "Cryo Fist", "Ice Crown", "Frozen Helm", "Winter Forge",
            "Cold Star Order", "Frost Serpent", "Ice Hawk", "Frozen Wolf",
            "Crystal Dagger", "Rime Shield", "Avalanche Division", "Polar Guard",
            "Snow Viper", "Ice Wraith", "Frozen Thunder", "Cryo Knight Order",
            "Breen Trade Guild", "Breen Mining Guild", "Breen Science Division", "Breen Medical Corps",
            "Breen Engineering Corps", "Breen Diplomatic Service", "Breen Youth Corps", "Breen Victory Mark")
    };
    
    private FactionProfile CreateGornProfile() => new FactionProfile
    {
        Faction = Faction.Gorn,
        Name = "Gorn",
        DesignLanguage = "Reptilian massive forms, heavy armor plating, brutal powerful silhouettes",
        ColorScheme = "Dark green, brown, bronze, natural reptilian tones",
        CivilianDesignLanguage = "Heavy utilitarian forms, natural rock textures, functional brutalist design",
        CivilianColorScheme = "Dark green, brown, bronze, earth tones",
        Architecture = "Massive cave-like structures, natural rock integrated, brutalist reptilian",
        RaceFeatures = "Green scaly reptilian skin, multifaceted eyes, sharp teeth, muscular",
        ClothingDetails = "Minimal scaled armor pieces, metallic harness, showing physique",
        HeraldicStyle = "Gorn Hegemony symbol, reptilian claw/tooth motifs, scaled patterns",
        MilitaryShips = GenerateShipList(
            "Vishap Cruiser", "Tuatara Cruiser", "Draguas Destroyer", "Zilant Battleship",
            "Varanus Support Ship", "Balaur Dreadnought", "Gorn Frigate", "Gorn Light Cruiser",
            "Gorn Heavy Cruiser", "Gorn Battlecruiser", "Gorn Patrol Ship", "Gorn Scout",
            "Gorn Interceptor", "Gorn Fighter", "Gorn Assault Ship", "Gorn Strike Cruiser",
            "Gorn Torpedo Ship", "Gorn Siege Cruiser", "Gorn Command Ship", "Gorn Flagship",
            "Gorn Escort", "Gorn Fast Attack", "Gorn Border Patrol", "Gorn Corvette",
            "Gorn Gunship", "Gorn Carrier", "Gorn Mine Layer", "Gorn Blockade Ship",
            "Gorn Troop Transport", "Gorn Invasion Ship", "Gorn Defense Ship", "Gorn Battle Carrier",
            "Gorn Plasma Cruiser", "Gorn Raider", "Gorn Super Dreadnought", "Hegemony Flagship"),
        CivilianShips = GenerateShipList(
            "Gorn Shuttle", "Gorn Cargo Ship", "Gorn Freighter", "Gorn Transport",
            "Gorn Passenger Liner", "Gorn Colony Ship", "Gorn Mining Vessel", "Gorn Ore Hauler",
            "Gorn Tanker", "Gorn Construction Ship", "Gorn Repair Vessel", "Gorn Medical Ship",
            "Gorn Science Vessel", "Gorn Survey Craft", "Gorn Diplomatic Transport", "Gorn Courier",
            "Gorn VIP Transport", "Gorn Bulk Hauler", "Gorn Excavation Ship", "Gorn Salvage Vessel",
            "Gorn Supply Ship", "Gorn Communication Ship", "Gorn Fuel Transport", "Gorn Agricultural Ship",
            "Gorn Hospital Ship", "Gorn Settler Ark", "Gorn Work Pod", "Gorn Tug",
            "Gorn Livestock Transport", "Gorn Trade Ship", "Gorn Industrial Hauler", "Gorn Egg Transport",
            "Gorn Research Ship", "Gorn Probe Ship", "Gorn Civilian Shuttle", "Gorn Luxury Barge"),
        MilitaryStructures = GenerateStructureList(
            "Gorn Starbase", "Gorn Orbital Fortress", "Gorn Defense Platform", "Gorn Weapons Array",
            "Gorn Shield Station", "Gorn Sensor Grid", "Gorn Command Station", "Gorn Fleet Depot",
            "Gorn Repair Dock", "Gorn Fighter Bay", "Gorn Munitions Depot", "Gorn Training Center",
            "Gorn Intelligence Hub", "Gorn Communication Array", "Gorn Early Warning Post", "Gorn Border Station",
            "Gorn Checkpoint", "Gorn Patrol Base", "Gorn Listening Post", "Gorn Mine Field Control",
            "Gorn Interdiction Platform", "Gorn Siege Platform", "Gorn Prison Platform", "Gorn Holding Facility",
            "Gorn Weapons Testing", "Gorn Plasma Weapons Lab", "Gorn Breeding Station", "Gorn Staging Area",
            "Gorn Torpedo Platform", "Gorn Guard Station", "Gorn Armory Station", "Gorn Medical Outpost",
            "Gorn Quarantine Station", "Gorn Operations Base", "Gorn Logistics Hub", "Gorn Strategic Command"),
        CivilianStructures = GenerateStructureList(
            "Gorn Trading Post", "Gorn Commerce Hub", "Gorn Civilian Station", "Gorn Shipyard",
            "Gorn Drydock", "Gorn Construction Yard", "Gorn Mining Station", "Gorn Ore Processing",
            "Gorn Refinery", "Gorn Cargo Depot", "Gorn Distribution Hub", "Gorn Passenger Terminal",
            "Gorn Transit Station", "Gorn Research Station", "Gorn Science Platform", "Gorn Medical Station",
            "Gorn Hospital Platform", "Gorn Power Station", "Gorn Energy Hub", "Gorn Habitat Module",
            "Gorn Colony Support", "Gorn Hatchery Station", "Gorn Breeding Research", "Gorn Heat Generation Hub",
            "Gorn Agricultural Station", "Gorn Food Processing", "Gorn Communication Relay", "Gorn Arena Station",
            "Gorn Cultural Center", "Gorn Education Station", "Gorn Archive", "Gorn Solar Collector",
            "Gorn Geothermal Station", "Gorn Recreation Module", "Gorn Market Station", "Gorn Diplomatic Station"),
        Buildings = GenerateBuildingList(
            "Gorn Hegemony Command", "King's Throne Hall", "Warrior Barracks", "Gorn Military Academy",
            "Gorn Weapons Forge", "Gorn Ship Assembly", "Gorn Power Plant", "Gorn Shield Generator",
            "Gorn Communication Tower", "Gorn Sensor Station", "Gorn Defense Tower", "Gorn Prison Complex",
            "Gorn Interrogation Pit", "Gorn Arena Complex", "Gorn Combat Arena", "Gorn Gladiator Hall",
            "Gorn Training Ground", "Gorn Armory", "Gorn Munitions Store", "Gorn Guard Post",
            "Gorn War Memorial", "Gorn Hegemony Hall", "Gorn Administrative Center", "Gorn Residential Complex",
            "Gorn Nesting Quarter", "Gorn Hatchery", "Gorn Egg Incubator", "Gorn Medical Center",
            "Gorn Hospital", "Gorn Research Institute", "Gorn Science Academy", "Gorn Mining Complex",
            "Gorn Ore Refinery", "Gorn Industrial Hub", "Gorn Cargo Warehouse", "Gorn Trade Center",
            "Gorn Market Pit", "Gorn Spaceport", "Gorn Landing Zone", "Gorn Transport Hub",
            "Gorn Food Processing", "Gorn Hunting Ground", "Gorn Water Reservoir", "Gorn Heat Generator",
            "Gorn Archive Cave", "Gorn Temple of Strength", "Gorn Entertainment Pit", "Gorn Victory Monument"),
        Troops = GenerateTroopList(
            "Gorn Warrior", "Gorn Heavy Warrior", "Gorn Berserker", "Gorn Saurian Guard",
            "Gorn Slasher", "Gorn Siege Breaker", "Gorn Sniper", "Gorn Officer",
            "Gorn Elite Guard", "Gorn Commander", "Gorn Scout", "Gorn Infiltrator",
            "Gorn Combat Engineer", "Gorn Medic", "Gorn King's Guard", "Gorn Shock Trooper"),
        PortraitVariants = GeneratePortraitList(
            "Gorn King", "Gorn General", "Gorn Admiral", "Gorn Captain",
            "Gorn Officer", "Gorn Veteran Warrior", "Gorn Young Warrior", "Gorn Elite Guard",
            "Gorn Arena Champion", "Gorn Scout", "Gorn Intelligence Agent", "Gorn Diplomat",
            "Gorn Ambassador", "Gorn Trade Envoy", "Gorn Merchant", "Gorn Civilian Worker",
            "Gorn Engineer", "Gorn Scientist", "Gorn Healer", "Gorn Ship Captain",
            "Gorn Fleet Commander", "Gorn Ground Commander", "Gorn Special Ops", "Gorn Pilot",
            "Gorn Technician", "Gorn Mining Overseer", "Gorn Hatchery Keeper", "Gorn Elder",
            "Gorn Council Member", "Gorn Provincial Ruler", "Gorn Clan Chief", "Gorn Youth",
            "Gorn Gladiator", "Gorn Hunter", "Gorn Female Matriarch", "Gorn Shaman"),
        HouseSymbols = GenerateSymbolList(
            "Gorn Hegemony Seal", "Gorn Hegemony Ornate", "Gorn Military Command", "Gorn Fleet Insignia",
            "Gorn Ground Forces", "Gorn Arena Champion Mark", "Gorn King's Crest", "Gorn Royal Seal",
            "Clan Claw Mark", "Clan Fang Emblem", "Clan Scale Shield", "Clan Tooth Crest",
            "Clan Tail Strike", "Saurian Guard Insignia", "Stone Fist Order", "Iron Scale Division",
            "Thunder Claw Regiment", "Blood Fang Battalion", "Razor Spine Unit", "Granite Shield",
            "Volcanic Forge", "Desert Storm Crest", "Rock Crusher Mark", "Obsidian Blade",
            "Emerald Serpent", "Bronze Dragon", "Stone Pillar", "Mountain King",
            "Cave Bear", "Iron Jaw", "Bone Crusher", "Magma Heart",
            "Stone Eye", "Crystal Fang", "Jade Claw", "Amber Scale",
            "Gorn Trade Guild", "Gorn Mining Guild", "Gorn Science Caste", "Gorn Medical Caste",
            "Gorn Engineering Caste", "Gorn Diplomatic Caste", "Gorn Hunter's Lodge", "Gorn Arena Guild",
            "Gorn Hatchery Caste", "Gorn Elder Council", "Gorn Victory Crest", "Gorn Dominance Mark")
    };
    
    private FactionProfile CreateAndorianProfile() => new FactionProfile
    {
        Faction = Faction.Andorian,
        Name = "Andorian",
        DesignLanguage = "RAPTOR/BIRD-OF-PREY silhouette - ELONGATED POINTED HULL with MULTIPLE SWEPT-BACK WING PYLONS like spread raptor wings. Aggressive predator aesthetic. NOT Federation saucer design!",
        ColorScheme = "BLUE-GRAY to TEAL hull, CYAN/TURQUOISE glowing accents, DARK BLUE panels, SILVER metallic trim",
        CivilianDesignLanguage = "Crystalline elegant forms, ice-inspired curves, underground city aesthetics",
        CivilianColorScheme = "Light blue, white, silver, cool pastels",
        Architecture = "Crystalline ice-inspired, elegant military, antenna-like spires",
        RaceFeatures = "Blue skin, white hair, two antennae on forehead, intense determined expression",
        ClothingDetails = "Imperial Guard armor or elegant cold-weather clothing, blue and white tones",
        HeraldicStyle = "Andorian Empire symbol, antenna motifs, ice crystal patterns",
        MilitaryShips = GenerateShipList(
            "Kumari Escort", "Charal Escort", "Khyzon Escort", "Andorian Cruiser",
            "Andorian Battlecruiser", "Andorian Frigate", "Andorian Destroyer", "Andorian Heavy Cruiser",
            "Andorian Light Cruiser", "Andorian Battleship", "Andorian Dreadnought", "Andorian Carrier",
            "Andorian Patrol Ship", "Andorian Scout", "Andorian Interceptor", "Andorian Fighter",
            "Andorian Assault Ship", "Andorian Strike Cruiser", "Andorian Torpedo Ship", "Andorian Command Ship",
            "Andorian Flagship", "Andorian Fast Escort", "Andorian Fast Attack", "Andorian Border Patrol",
            "Andorian Corvette", "Andorian Gunship", "Andorian Mine Layer", "Andorian Blockade Runner",
            "Andorian Troop Transport", "Andorian Defense Ship", "Andorian Battle Carrier", "Andorian Raider",
            "Andorian Stealth Ship", "Andorian Intelligence Vessel", "Imperial Guard Warship", "Andorian Super Dreadnought"),
        CivilianShips = GenerateShipList(
            "Andorian Shuttle", "Andorian Cargo Ship", "Andorian Freighter", "Andorian Transport",
            "Andorian Passenger Liner", "Andorian Colony Ship", "Andorian Mining Vessel", "Andorian Ore Hauler",
            "Andorian Tanker", "Andorian Construction Ship", "Andorian Repair Vessel", "Andorian Medical Ship",
            "Andorian Science Vessel", "Andorian Survey Craft", "Andorian Diplomatic Transport", "Andorian Courier",
            "Andorian VIP Transport", "Andorian Ice Harvester", "Andorian Research Ship", "Andorian Salvage Vessel",
            "Andorian Supply Ship", "Andorian Communication Ship", "Andorian Fuel Transport", "Andorian Agricultural Ship",
            "Andorian Hospital Ship", "Andorian Settler Ark", "Andorian Work Pod", "Andorian Tug",
            "Andorian Luxury Yacht", "Andorian Trade Ship", "Andorian Industrial Hauler", "Andorian Cultural Ship",
            "Andorian Probe Ship", "Andorian Civilian Shuttle", "Andorian Academy Ship", "Andorian Exploration Vessel"),
        MilitaryStructures = GenerateStructureList(
            "Andorian Starbase", "Andorian Orbital Fortress", "Andorian Defense Platform", "Andorian Weapons Array",
            "Andorian Shield Station", "Andorian Sensor Grid", "Andorian Command Station", "Andorian Fleet Depot",
            "Andorian Repair Dock", "Andorian Fighter Bay", "Andorian Munitions Depot", "Imperial Guard Academy",
            "Andorian Intelligence Hub", "Andorian Communication Array", "Andorian Early Warning Post", "Andorian Border Station",
            "Andorian Checkpoint", "Andorian Patrol Base", "Andorian Listening Post", "Andorian Mine Field Control",
            "Andorian Interdiction Platform", "Andorian Siege Platform", "Andorian Prison Platform", "Andorian Holding Facility",
            "Andorian Weapons Testing", "Andorian Cryo Weapons Lab", "Andorian War Room Station", "Andorian Staging Area",
            "Andorian Torpedo Platform", "Andorian Guard Station", "Andorian Armory Station", "Andorian Medical Outpost",
            "Andorian Quarantine Station", "Andorian Operations Base", "Andorian Logistics Hub", "Imperial Guard Command"),
        CivilianStructures = GenerateStructureList(
            "Andorian Trading Post", "Andorian Commerce Hub", "Andorian Civilian Station", "Andorian Shipyard",
            "Andorian Drydock", "Andorian Construction Yard", "Andorian Mining Station", "Andorian Ore Processing",
            "Andorian Refinery", "Andorian Cargo Depot", "Andorian Distribution Hub", "Andorian Passenger Terminal",
            "Andorian Transit Station", "Andorian Research Station", "Andorian Science Platform", "Andorian Medical Station",
            "Andorian Hospital Platform", "Andorian Power Station", "Andorian Energy Hub", "Andorian Habitat Module",
            "Andorian Colony Support", "Andorian Ice Research Lab", "Andorian Geothermal Station", "Andorian Underground Hub",
            "Andorian Agricultural Station", "Andorian Food Processing", "Andorian Communication Relay", "Andorian Entertainment Complex",
            "Andorian Cultural Center", "Andorian Education Station", "Andorian Archive", "Andorian Solar Collector",
            "Andorian Weather Station", "Andorian Recreation Module", "Andorian Market Station", "Andorian Diplomatic Station"),
        Buildings = GenerateBuildingList(
            "Andorian Imperial Palace", "Imperial Guard Headquarters", "Guard Barracks", "Andorian Military Academy",
            "Andorian Weapons Factory", "Andorian Ship Assembly", "Andorian Power Plant", "Andorian Shield Generator",
            "Andorian Communication Tower", "Andorian Sensor Station", "Andorian Defense Tower", "Andorian Prison Complex",
            "Andorian Interrogation Center", "Andorian Intelligence HQ", "Andorian Training Ground", "Andorian Armory",
            "Andorian Munitions Store", "Andorian Guard Post", "Andorian War Memorial", "Ushaan Arena",
            "Andorian Council Chamber", "Andorian Administrative Center", "Andorian Underground City Gate", "Andorian Residential Complex",
            "Andorian Ice Dwelling", "Andorian Clan Hall", "Andorian Medical Center", "Andorian Hospital",
            "Andorian Research Institute", "Andorian Science Academy", "Andorian Mining Complex", "Andorian Ore Refinery",
            "Andorian Industrial Hub", "Andorian Cargo Warehouse", "Andorian Trade Center", "Andorian Market Ice Hall",
            "Andorian Spaceport", "Andorian Landing Zone", "Andorian Transport Hub", "Andorian Food Processing",
            "Andorian Hydroponic Dome", "Andorian Water Reclamation", "Andorian Geothermal Plant", "Andorian Archive Building",
            "Andorian Museum of History", "Andorian Cultural Hall", "Andorian Entertainment Dome", "Andorian Heroes Monument"),
        Troops = GenerateTroopList(
            "Imperial Guard Soldier", "Imperial Guard Veteran", "Imperial Guard Heavy", "Andorian Commando",
            "Andorian Sniper", "Andorian Demolitions", "Andorian Officer", "Andorian Elite Guard",
            "Andorian Commander", "Andorian Scout", "Andorian Infiltrator", "Andorian Combat Engineer",
            "Andorian Medic", "Andorian Ushaan Duelist", "Andorian Special Forces", "Imperial Guard Captain"),
        PortraitVariants = GeneratePortraitList(
            "Andorian Male Chancellor", "Andorian Male General", "Andorian Male Admiral", "Andorian Male Captain",
            "Andorian Male Officer", "Andorian Male Guard Veteran", "Andorian Male Young Guard", "Andorian Male Civilian",
            "Andorian Male Scientist", "Andorian Male Engineer", "Andorian Male Merchant", "Andorian Male Diplomat",
            "Andorian Male Elder", "Andorian Male Artist", "Andorian Male Mining Worker", "Andorian Male Doctor",
            "Andorian Male Pilot", "Andorian Male Intelligence", "Andorian Female Chancellor", "Andorian Female General",
            "Andorian Female Admiral", "Andorian Female Captain", "Andorian Female Officer", "Andorian Female Guard Veteran",
            "Andorian Female Young Guard", "Andorian Female Civilian", "Andorian Female Scientist", "Andorian Female Engineer",
            "Andorian Female Merchant", "Andorian Female Diplomat", "Andorian Female Elder", "Andorian Female Artist",
            "Andorian Female Doctor", "Andorian Female Pilot", "Aenar Male", "Aenar Female"),
        HouseSymbols = GenerateSymbolList(
            "Andorian Empire Seal", "Andorian Empire Ornate", "Imperial Guard Insignia", "Imperial Guard Elite",
            "Andorian Fleet Command", "Andorian Ground Forces", "Andorian Intelligence", "Andorian Special Operations",
            "Clan Shran", "Clan Thy'lek", "Clan Tarah", "Clan Talla",
            "Clan Thelin", "Clan Keval", "Ice Blade Order", "Frost Star Crest",
            "Crystal Spire Emblem", "Aurora Division", "Glacier Shield", "Icefall Regiment",
            "Snowstorm Battalion", "Northern Wind Mark", "Blue Star Order", "Ushaan Combat Badge",
            "Frozen Honor", "Ice Sentinel", "Crystal Antenna", "Snow Hawk",
            "Frost Wolf", "Ice Bear", "Glacier Serpent", "Polar Star",
            "Diamond Shard", "Ice Crystal Ward", "Blizzard Force", "Tundra Guard",
            "Andorian Trade Guild", "Andorian Mining Guild", "Andorian Science Division", "Andorian Medical Corps",
            "Andorian Engineering Corps", "Andorian Diplomatic Service", "Andorian Academy Badge", "Andorian Arena Badge",
            "Aenar Peace Symbol", "Aenar Telepathy Mark", "Andorian Victory Crest", "Andorian Heritage Mark")
    };
    
    private FactionProfile CreateVulcanProfile() => new FactionProfile
    {
        Faction = Faction.Vulcan,
        Name = "Vulcan",
        DesignLanguage = "Ring-shaped warp nacelles, circular forms, logical geometric layouts, ancient yet advanced, elegant efficiency",
        ColorScheme = "Bronze, rust red, warm desert tones, orange-brown, copper accents, red-orange glow",
        Architecture = "Desert-temple inspired, Mt. Seleya style, logical geometric forms, meditation spaces, IDIC symbolism",
        RaceFeatures = "Pointed ears, upswept eyebrows, pale to olive skin, completely emotionless expression, green blood tint",
        ClothingDetails = "Flowing robes or form-fitting uniform, earth tones, IDIC symbol on chest or clasp",
        HeraldicStyle = "IDIC symbol variations, logical geometric patterns, flame/logic motifs, Vulcan script",
        MilitaryShips = GenerateShipList("D'Kyr Combat Cruiser", "Surak Class", "Sh'Raan Cruiser", "Suurok Class", "T'Plana Hath Type", "Vahklas Transport", "Apollo Class", "Vulcan Cruiser", "Combat Shuttle", "Ring Ship Prototype", "Defense Frigate", "Patrol Cruiser", "Science Vessel", "Survey Ship", "Diplomatic Cruiser", "Defense Platform Ship", "Rapid Response Vessel", "Border Patrol", "Escort Frigate", "Interceptor", "Heavy Cruiser", "Command Ship", "Assault Transport", "Carrier", "Dreadnought", "Scout", "Destroyer", "Light Cruiser", "Battlecruiser", "Flagship", "Monitor", "Corvette", "Gunship", "Torpedo Boat", "Missile Cruiser", "Stealth Ship"),
        CivilianShips = GenerateShipList("Vulcan Shuttle", "Long Range Shuttle", "Science Probe Ship", "Meditation Transport", "Pilgrim Vessel", "Academy Ship", "Cargo Transport", "Passenger Liner", "Medical Ship", "Research Vessel", "Diplomatic Yacht", "Trade Ship", "Mining Vessel", "Colony Transport", "Refugee Ark", "Survey Craft", "Communication Ship", "Construction Vessel", "Salvage Ship", "Tanker", "Freighter", "Courier", "VIP Transport", "Science Station Mobile", "Hospital Ship", "Agricultural Ship", "Water Carrier", "Ore Hauler", "Fuel Transport", "Luxury Yacht", "Tour Ship", "Museum Ship", "School Ship", "Temple Ship", "Archive Ship", "Cultural Exchange"),
        MilitaryStructures = GenerateStructureList("Vulcan Defense Outpost", "Orbital Weapons Platform", "Shield Station", "Sensor Array", "Communication Hub", "Command Station", "Fleet Depot", "Repair Facility", "Training Station", "Research Base", "Intelligence Post", "Border Station", "Patrol Base", "Emergency Response Hub", "Defense Grid Node", "Torpedo Platform", "Phaser Array", "Early Warning", "Listening Post", "Guard Station", "Checkpoint", "Garrison", "Armory Station", "Medical Outpost", "Quarantine Platform", "Security Hub", "Prison Station", "Tactical Center", "Operations Base", "Logistics Hub", "Supply Depot", "Munitions Store", "Fighter Bay", "Shuttle Base", "Reserve Fleet Dock", "Strategic Command"),
        CivilianStructures = GenerateStructureList("Vulcan Science Academy Station", "Meditation Retreat", "Logic Temple Orbital", "IDIC Center", "Cultural Archive", "Research Station", "Observatory", "Communication Relay", "Trade Hub", "Passenger Terminal", "Shipyard", "Drydock", "Construction Platform", "Mining Station", "Refinery", "Power Station", "Agricultural Platform", "Hydroponics Bay", "Medical Center", "Hospital Station", "University Module", "Library Archive", "Museum Platform", "Art Gallery", "Music Academy", "Philosophy Center", "Diplomatic Station", "Embassy Platform", "Conference Center", "Transit Hub", "Cargo Depot", "Warehouse Station", "Market Platform", "Recreation Module", "Spa Retreat", "Thermal Springs"),
        Buildings = GenerateBuildingList("Vulcan Science Academy", "Temple of Logic", "Mt. Seleya Monastery", "IDIC Center", "Meditation Garden", "Logic Temple", "Surak Memorial", "Ancient Library", "High Command", "Defense Ministry", "Diplomatic Center", "Embassy Complex", "Kolinahr Retreat", "Healing Temple", "Hospital", "Medical Research", "Physics Institute", "Space Academy", "Agricultural Dome", "Hydroponic Center", "Forge Works", "Lirpa Smithy", "Mining Facility", "Refinery", "Power Plant", "Geothermal Station", "Water Reclamation", "Air Processing", "Residence Complex", "Family Estate", "Market Square", "Trade Hall", "Transport Hub", "Shuttle Port", "Communication Tower", "Sensor Array", "Security Office", "Guard Post", "Archive Building", "Museum", "Art Center", "Music Hall", "School", "Academy Annex", "Research Lab", "Observatory", "Weather Station", "Seismic Monitor"),
        Troops = GenerateTroopList("Vulcan Security Officer", "High Command Guard", "Vulcan Commando", "Logic Extremist Radical", "Kolinahr Master", "Security Chief", "Tactical Officer", "Science Officer Combat", "Medical Combat Team", "Lirpa Warrior", "Nerve Pinch Specialist", "Mind Meld Interrogator", "Desert Ranger", "Temple Guard", "V'Shar Agent", "Defense Force Soldier"),
        PortraitVariants = GeneratePortraitList("Vulcan Male Elder Priest", "Vulcan Male Young Adept", "Vulcan Male Science Officer", "Vulcan Male Ambassador", "Vulcan Male Merchant", "Vulcan Male Healer", "Vulcan Female Elder Priestess", "Vulcan Female Young Adept", "Vulcan Female Science Officer", "Vulcan Female Ambassador", "Vulcan Female Healer", "Vulcan Female Teacher", "Vulcan Male Kolinahr Master", "Vulcan Female Kolinahr Master", "Vulcan Male Security", "Vulcan Female Security", "Vulcan Male Civilian", "Vulcan Female Civilian", "Vulcan Male Child", "Vulcan Female Child", "Vulcan Male Forge Survivor", "Vulcan Male Mind Melder", "Vulcan Female Logic Extremist", "Vulcan Male Artist", "Vulcan Female Musician", "Vulcan Male Historian", "Vulcan Female Archivist", "Vulcan Male Engineer", "Vulcan Female Pilot", "Vulcan Male Pon Farr", "Vulcan Female Matriarch", "Vulcan Male Patriarch", "Vulcan Male Syrrannite", "Vulcan Female Syrrannite", "Vulcan Romulan Hybrid", "Vulcan Human Hybrid"),
        HouseSymbols = GenerateSymbolList("IDIC Standard", "IDIC Ornate", "IDIC Ancient", "Surak Seal", "High Command", "Science Academy", "Kolinahr Flame", "Mt. Seleya", "ShiKahr City", "Vulcan Script Logic", "Vulcan Script Peace", "Vulcan Script Wisdom", "Flame of Knowledge", "Stone of Gol", "Kir'Shara", "Mind Meld Symbol", "Katric Ark", "Family Katra", "House T'Pau", "House Sarek", "House Surak", "House Sybok", "House Soval", "Clan Symbol Ancient", "Forge Survivors", "Syrrannite Movement", "V'Shar Intelligence", "Medical Corps", "Science Directorate", "Space Council", "Logic Temple", "Desert Wanderers", "Meditation Seal", "Logic Master Rank", "Adept Rank", "Initiate Rank", "Elder Rank", "Ambassador Seal", "Diplomatic Corps", "Trade Council", "Mining Guild", "Artisan Guild", "Music Academy", "Philosophy School", "History Archive", "Cultural Preservation", "Off-World Vulcans", "Reunification Movement")
    };
    
    private FactionProfile CreateTrillProfile() => new FactionProfile
    {
        Faction = Faction.Trill,
        Name = "Trill",
        DesignLanguage = "Elegant organic curves, symbiotic design motifs, dual-nature aesthetic, joined harmony",
        ColorScheme = "Soft purples, teals, organic greens, iridescent accents suggesting symbiont",
        Architecture = "Organic flowing forms, caves for symbionts, pools and water features, dual-level design",
        RaceFeatures = "Leopard-like spots running from temples down sides of face and body, humanoid appearance otherwise",
        ClothingDetails = "Elegant robes or Starfleet uniform, often with symbiont-honoring patterns or clasps",
        HeraldicStyle = "Symbiont pool motifs, joined/intertwined designs, cave/water symbols",
        MilitaryShips = GenerateShipList("Trill Defense Ship", "Symbiont Transport Combat", "Patrol Cruiser", "Frigate", "Destroyer", "Cruiser", "Battleship", "Carrier", "Escort", "Scout", "Interceptor", "Heavy Cruiser", "Light Cruiser", "Corvette", "Gunship", "Defense Platform", "Assault Ship", "Command Vessel", "Flagship", "Dreadnought", "Monitor", "Torpedo Ship", "Missile Cruiser", "Fighter Carrier", "Assault Carrier", "Stealth Ship", "Electronic Warfare", "Medical Frigate", "Repair Ship", "Supply Ship", "Troop Transport", "Marine Assault", "Boarding Craft", "Mine Layer", "Mine Sweeper", "Blockade Runner"),
        CivilianShips = GenerateShipList("Trill Shuttle", "Symbiont Transport", "Medical Ship", "Science Vessel", "Research Ship", "Diplomatic Yacht", "Passenger Liner", "Cargo Freighter", "Colony Ship", "Mining Vessel", "Construction Ship", "Repair Vessel", "Tanker", "Agricultural Ship", "Water Carrier", "Luxury Transport", "Tour Ship", "Academy Vessel", "Training Ship", "Hospital Ship", "Refugee Ark", "Cultural Ship", "Archive Transport", "Museum Ship", "VIP Shuttle", "Courier", "Mail Ship", "News Vessel", "Entertainment Ship", "Pleasure Yacht", "Sports Ship", "Race Ship", "Exploration Vessel", "Survey Craft", "Probe Ship", "Salvage Vessel"),
        MilitaryStructures = GenerateStructureList("Trill Defense Station", "Orbital Platform", "Shield Generator", "Weapons Array", "Sensor Grid", "Command Post", "Fleet Base", "Repair Dock", "Training Center", "Academy Station", "Intelligence Hub", "Communication Array", "Early Warning", "Patrol Base", "Border Station", "Checkpoint", "Garrison", "Armory", "Medical Station", "Quarantine", "Prison Platform", "Security Hub", "Tactical Command", "Operations Center", "Logistics Base", "Supply Depot", "Fighter Bay", "Shuttle Base", "Marine Barracks", "Ground Support", "Orbital Bombardment", "Siege Platform", "Defense Grid", "Emergency Response", "Reserve Fleet", "Strategic Reserve"),
        CivilianStructures = GenerateStructureList("Trill Symbiosis Commission Station", "Symbiont Pool Orbital", "Medical Center", "Science Station", "Research Platform", "University Module", "Academy Annex", "Library Archive", "Museum Platform", "Cultural Center", "Art Gallery", "Music Hall", "Diplomatic Station", "Embassy Platform", "Conference Center", "Trade Hub", "Market Station", "Cargo Depot", "Passenger Terminal", "Transit Hub", "Shipyard", "Drydock", "Construction Platform", "Mining Station", "Refinery", "Power Plant", "Agricultural Station", "Hydroponics", "Recreation Center", "Spa Resort", "Hotel Station", "Entertainment Complex", "News Station", "Communication Relay", "Weather Monitor", "Observatory"),
        Buildings = GenerateBuildingList("Symbiosis Commission", "Symbiont Caves", "Joining Center", "Medical Institute", "Science Academy", "Trill University", "Government Complex", "Senate Building", "Diplomatic Center", "Embassy Row", "Cultural Museum", "History Archive", "Music Academy", "Art Institute", "Hospital", "Clinic", "Research Lab", "Observatory", "Agricultural Dome", "Hydroponics", "Mining Facility", "Refinery", "Power Plant", "Water Treatment", "Residential Complex", "Family Estate", "Market Square", "Trade Center", "Transport Hub", "Shuttle Port", "Security Office", "Guard Post", "Communication Tower", "Sensor Array", "School", "Academy", "Library", "Museum", "Theater", "Concert Hall", "Sports Arena", "Park", "Garden", "Memorial", "Temple", "Monastery", "Retreat Center"),
        Troops = GenerateTroopList("Trill Security Officer", "Joined Warrior", "Symbiosis Guardian", "Defense Force", "Tactical Officer", "Science Combat", "Medical Combat", "Special Operations", "Commando", "Sniper", "Heavy Weapons", "Demolitions", "Scout", "Infiltrator", "Intelligence Agent", "Diplomatic Guard"),
        PortraitVariants = GeneratePortraitList("Trill Male Joined Young", "Trill Male Joined Elder", "Trill Male Unjoined", "Trill Male Initiate", "Trill Male Guardian", "Trill Male Scientist", "Trill Female Joined Young", "Trill Female Joined Elder", "Trill Female Unjoined", "Trill Female Initiate", "Trill Female Guardian", "Trill Female Scientist", "Trill Male Doctor", "Trill Female Doctor", "Trill Male Ambassador", "Trill Female Ambassador", "Trill Male Military", "Trill Female Military", "Trill Male Civilian", "Trill Female Civilian", "Trill Male Child", "Trill Female Child", "Trill Male Symbiont Host Dax Type", "Trill Female Symbiont Host Dax Type", "Trill Male Artist", "Trill Female Musician", "Trill Male Historian", "Trill Female Pilot", "Trill Male Engineer", "Trill Female Captain", "Trill Reassociation Exile", "Trill Commission Member", "Trill Guardian Elder", "Trill Pool Tender", "Trill Male Merchant", "Trill Female Trader"),
        HouseSymbols = GenerateSymbolList("Trill Symbiosis Commission", "Symbiont Pool", "Joining Ceremony", "Trill Government", "Defense Force", "Science Academy", "Medical Institute", "House Dax", "House Odan", "House Joran", "House Curzon", "House Jadzia", "House Ezri", "House Lela", "House Tobin", "House Emony", "House Audrid", "House Torias", "Ancient Symbiont", "Guardian Order", "Initiate Seal", "Joined Seal", "Unjoined Seal", "Zhian'tara", "Trill Script", "Cave Entrance", "Pool Ripple", "Dual Soul", "Intertwined Lives", "Memory Flow", "Past Lives", "Future Joining", "Commission Seal", "Medical Symbol", "Science Seal", "Cultural Seal", "Diplomatic Seal", "Trade Guild", "Mining Guild", "Agricultural Guild", "Transport Guild", "Artist Guild", "Musician Seal", "Writer Seal", "Historian Seal", "Explorer Seal", "Pioneer Seal", "Colony Seal")
    };
    
    private FactionProfile CreateBajoranProfile() => new FactionProfile
    {
        Faction = Faction.Bajoran,
        Name = "Bajoran",
        DesignLanguage = "Spiritual aesthetic, curved solar-sail designs, earring-inspired forms, resilient and hopeful",
        ColorScheme = "Earth tones, terracotta, warm orange, gold temple accents, rust and copper",
        Architecture = "Temple-inspired with solar motifs, Prophets iconography, post-occupation rebuilding aesthetic",
        RaceFeatures = "Distinctive ridged nose bridge, d'ja pagh earring on right ear, humanoid appearance",
        ClothingDetails = "Flowing robes for religious, militia uniform for military, earring always present",
        HeraldicStyle = "Bajoran religious symbols, Prophets imagery, Celestial Temple motifs, ancient script",
        MilitaryShips = GenerateShipList("Bajoran Interceptor", "Bajoran Assault Ship", "Bajoran Freighter Armed", "Bajoran Raider", "Resistance Fighter", "Militia Patrol", "Defense Ship", "Frigate", "Destroyer", "Cruiser", "Heavy Cruiser", "Battleship", "Carrier", "Escort", "Scout", "Corvette", "Gunship", "Torpedo Ship", "Missile Boat", "Fighter", "Bomber", "Transport Armed", "Command Ship", "Flagship", "Dreadnought", "Monitor", "Assault Carrier", "Troop Ship", "Marine Transport", "Boarding Craft", "Mine Layer", "Blockade Runner", "Stealth Ship", "Electronic Warfare", "Medical Combat", "Repair Combat"),
        CivilianShips = GenerateShipList("Bajoran Lightship Solar Sail", "Bajoran Shuttle", "Transport", "Cargo Ship", "Passenger Liner", "Colony Ship", "Medical Ship", "Hospital Ship", "Science Vessel", "Research Ship", "Exploration Craft", "Survey Ship", "Mining Vessel", "Construction Ship", "Repair Ship", "Agricultural Ship", "Water Tanker", "Fuel Ship", "Refugee Ark", "Pilgrim Ship", "Temple Ship", "Monastery Vessel", "Diplomatic Yacht", "VIP Transport", "Courier", "Mail Ship", "News Vessel", "Tour Ship", "Luxury Yacht", "Fishing Vessel", "Aquaculture Ship", "Salvage Vessel", "Tug", "Construction Barge", "Ore Carrier", "Tanker"),
        MilitaryStructures = GenerateStructureList("Bajoran Defense Station", "Orbital Weapon Platform", "Shield Station", "Sensor Array", "Militia Base", "Resistance Hideout", "Command Post", "Fleet Depot", "Repair Dock", "Training Center", "Intelligence Hub", "Communication Array", "Early Warning", "Patrol Base", "Border Station", "Checkpoint", "Garrison", "Armory", "Medical Station", "Prison Platform", "Security Hub", "Tactical Command", "Operations Center", "Logistics Base", "Supply Depot", "Fighter Bay", "Shuttle Base", "Marine Barracks", "Ground Support", "Emergency Response", "Reserve Base", "Strategic Reserve", "Militia Academy", "Veterans Station", "Memorial Platform", "Resistance Museum"),
        CivilianStructures = GenerateStructureList("Bajoran Temple Station", "Monastery Orbital", "Vedek Assembly Platform", "Kai Residence", "Medical Center", "Hospital Station", "Science Station", "Research Platform", "University Module", "Academy Annex", "Library Archive", "Museum Platform", "Cultural Center", "Art Gallery", "Music Hall", "Diplomatic Station", "Embassy Platform", "Trade Hub", "Market Station", "Cargo Depot", "Passenger Terminal", "Transit Hub", "Shipyard", "Drydock", "Mining Station", "Refinery", "Power Plant", "Agricultural Station", "Hydroponics", "Recreation Center", "Hotel Station", "Refugee Center", "Orphanage Station", "Elder Care", "Communication Relay", "Celestial Temple Observation"),
        Buildings = GenerateBuildingList("Bajoran Temple", "Monastery", "Vedek Assembly", "Kai Residence", "Government Center", "First Minister Office", "Chamber of Ministers", "Militia Headquarters", "Security Office", "Hospital", "Medical Institute", "Science Academy", "University", "School", "Orphanage", "Refugee Center", "Agricultural Center", "Hydroponics Dome", "Mining Facility", "Refinery", "Power Plant", "Water Treatment", "Residential Complex", "Family Home", "Market Square", "Trade Center", "Art Gallery", "Museum", "Music Hall", "Theater", "Sports Arena", "Park", "Garden", "Memorial Plaza", "Resistance Memorial", "Occupation Museum", "Communications", "Transport Hub", "Shuttle Port", "Security Post", "Guard Tower", "Shrine", "Orb Repository", "Prophets Temple", "Celestial Temple Viewing", "Library", "Archive", "Embassy"),
        Troops = GenerateTroopList("Bajoran Militia Officer", "Bajoran Resistance Fighter", "Vedek Guard", "Temple Guardian", "Security Officer", "Tactical Officer", "Scout", "Sniper", "Heavy Weapons", "Demolitions Expert", "Medic", "Engineer", "Commando", "Special Forces", "Intelligence Agent", "Diplomatic Guard"),
        PortraitVariants = GeneratePortraitList("Bajoran Male Vedek", "Bajoran Male Prylar", "Bajoran Male Kai", "Bajoran Male Militia Officer", "Bajoran Male Resistance Fighter", "Bajoran Male Civilian", "Bajoran Male Merchant", "Bajoran Male Farmer", "Bajoran Female Vedek", "Bajoran Female Prylar", "Bajoran Female Kai", "Bajoran Female Militia Officer", "Bajoran Female Resistance Fighter", "Bajoran Female Civilian", "Bajoran Female Merchant", "Bajoran Female Artist", "Bajoran Male Child", "Bajoran Female Child", "Bajoran Male Elder", "Bajoran Female Elder", "Bajoran Male Scientist", "Bajoran Female Doctor", "Bajoran Male Engineer", "Bajoran Female Pilot", "Bajoran Male Refugee", "Bajoran Female Comfort Woman Survivor", "Bajoran Male Ranjen", "Bajoran Female Ranjen", "Bajoran Male First Minister", "Bajoran Female Ambassador", "Bajoran Male Occupation Survivor", "Bajoran Female Freedom Fighter", "Bajoran Male Collaborator Redeemed", "Bajoran Male Artist", "Bajoran Female Musician", "Bajoran Cardassian Hybrid"),
        HouseSymbols = GenerateSymbolList("Bajoran Religious Symbol", "Prophets Seal", "Celestial Temple", "Orb of Prophecy", "Orb of Change", "Orb of Time", "Orb of Wisdom", "Orb of the Emissary", "Kai Seal", "Vedek Assembly", "Prylar Order", "Ranjen Symbol", "Temple of Iponu", "D'jarra Caste Artist", "D'jarra Caste Soldier", "D'jarra Caste Farmer", "D'jarra Caste Merchant", "Bajoran Militia", "Resistance Cell", "Liberation Force", "First Minister Seal", "Chamber of Ministers", "Provincial Government", "Dahkur Province", "Rakantha Province", "Musilla Province", "Hedrikspool Province", "Family Kira", "Family Bareil", "Family Winn", "Family Opaka", "Family Shakaar", "Occupation Memorial", "Liberation Day", "Gratitude Festival", "Peldor Festival", "Ancient Bajor", "B'hala Discovery", "Emissary Blessing", "Kosst Amojan Ward", "Pah-wraith Warning", "Reckoning Symbol", "Unity Symbol", "Rebuilding Hope", "New Bajor Colony", "Deep Space Nine", "Terok Nor Reclaimed")
    };
    
    private FactionProfile CreateTholianProfile() => new FactionProfile
    {
        Faction = Faction.Tholian,
        Name = "Tholian",
        DesignLanguage = "Crystalline geometric forms, faceted surfaces, web-spinner motifs, extreme heat aesthetic",
        ColorScheme = "Amber, orange, gold, crystalline translucent, volcanic red glow",
        Architecture = "Crystal lattice structures, high-temperature environments, geometric precision",
        RaceFeatures = "Crystalline silicon-based life form, faceted gem-like appearance, no humanoid features, glowing internal heat",
        ClothingDetails = "No clothing - body is crystalline carapace, environmental suit when outside hot environment",
        HeraldicStyle = "Geometric crystalline patterns, web designs, faceted gem shapes",
        MilitaryShips = GenerateShipList("Tholian Web Spinner", "Tholian Cruiser", "Tholian Battleship", "Tholian Carrier", "Tholian Frigate", "Tholian Destroyer", "Tholian Interceptor", "Tholian Scout", "Tholian Dreadnought", "Tholian Command Ship", "Tholian Heavy Cruiser", "Tholian Light Cruiser", "Tholian Corvette", "Tholian Gunship", "Tholian Torpedo Ship", "Web Generator Ship", "Web Tender", "Web Anchor", "Tholian Fighter", "Tholian Bomber", "Tholian Assault Ship", "Tholian Troop Transport", "Tholian Boarding Craft", "Tholian Mine Layer", "Tholian Patrol Ship", "Tholian Border Guard", "Tholian Sensor Ship", "Tholian Communication Ship", "Tholian Flagship", "Tholian Super Dreadnought", "Tholian Monitor", "Tholian Station Ship", "Tholian Siege Ship", "Tholian Blockade Ship", "Tholian Stealth Vessel", "Tholian Web Caster"),
        CivilianShips = GenerateShipList("Tholian Transport", "Tholian Cargo Ship", "Tholian Colony Ship", "Tholian Mining Vessel", "Tholian Construction Ship", "Tholian Research Vessel", "Tholian Science Ship", "Tholian Survey Craft", "Tholian Probe Ship", "Tholian Diplomatic Vessel", "Tholian Trade Ship", "Tholian Tanker", "Tholian Ore Carrier", "Tholian Crystal Harvester", "Tholian Heat Collector", "Tholian Energy Ship", "Tholian Repair Vessel", "Tholian Salvage Ship", "Tholian Tug", "Tholian Shuttle", "Tholian Personal Craft", "Tholian VIP Transport", "Tholian Courier", "Tholian Medical Ship", "Tholian Hospital Ship", "Tholian Agricultural Ship", "Tholian Livestock Ship", "Tholian Passenger Ship", "Tholian Tour Vessel", "Tholian Cultural Ship", "Tholian Archive Ship", "Tholian Museum Vessel", "Tholian School Ship", "Tholian Academy Ship", "Tholian Refugee Vessel", "Tholian Emergency Ship"),
        MilitaryStructures = GenerateStructureList("Tholian Web Station", "Tholian Defense Platform", "Tholian Fortress", "Tholian Weapon Platform", "Tholian Shield Generator", "Tholian Sensor Array", "Tholian Command Station", "Tholian Fleet Base", "Tholian Repair Dock", "Tholian Training Center", "Tholian Intelligence Hub", "Tholian Communication Array", "Tholian Early Warning", "Tholian Patrol Base", "Tholian Border Station", "Tholian Checkpoint", "Tholian Garrison", "Tholian Armory", "Tholian Prison Crystal", "Tholian Security Hub", "Tholian Tactical Center", "Tholian Operations Base", "Tholian Logistics Hub", "Tholian Supply Depot", "Tholian Fighter Bay", "Tholian Web Anchor Station", "Tholian Siege Platform", "Tholian Bombardment Station", "Tholian Marine Base", "Tholian Ground Support", "Tholian Emergency Station", "Tholian Reserve Base", "Tholian Strategic Command", "Tholian Admiral Station", "Tholian War Council", "Tholian Assembly Defense"),
        CivilianStructures = GenerateStructureList("Tholian Crystal City Station", "Tholian Hive Center", "Tholian Assembly Platform", "Tholian Government Station", "Tholian Trade Hub", "Tholian Market Station", "Tholian Cargo Depot", "Tholian Passenger Terminal", "Tholian Shipyard", "Tholian Drydock", "Tholian Construction Platform", "Tholian Mining Station", "Tholian Refinery", "Tholian Power Station", "Tholian Heat Collector Station", "Tholian Crystal Farm", "Tholian Research Station", "Tholian Science Platform", "Tholian University", "Tholian Academy", "Tholian Library", "Tholian Museum", "Tholian Cultural Center", "Tholian Diplomatic Station", "Tholian Embassy", "Tholian Conference Center", "Tholian Medical Station", "Tholian Hospital", "Tholian Recreation Center", "Tholian Communication Relay", "Tholian Transit Hub", "Tholian Observatory", "Tholian Weather Station", "Tholian Emergency Center", "Tholian Archive Station", "Tholian Monument Platform"),
        Buildings = GenerateBuildingList("Tholian Crystal Spire", "Tholian Hive Center", "Tholian Assembly Hall", "Tholian Government Complex", "Tholian Defense Center", "Tholian Military Academy", "Tholian Science Institute", "Tholian Research Lab", "Tholian University", "Tholian Library Crystal", "Tholian Museum", "Tholian Cultural Center", "Tholian Art Spire", "Tholian Music Crystal", "Tholian Hospital", "Tholian Medical Center", "Tholian Residence Cluster", "Tholian Family Crystal", "Tholian Market Square", "Tholian Trade Center", "Tholian Mining Complex", "Tholian Refinery", "Tholian Power Plant", "Tholian Heat Generator", "Tholian Agricultural Dome", "Tholian Crystal Garden", "Tholian Transport Hub", "Tholian Shuttle Port", "Tholian Security Post", "Tholian Guard Tower", "Tholian Communication Tower", "Tholian Sensor Spire", "Tholian Embassy", "Tholian Diplomatic Center", "Tholian Prison Crystal", "Tholian Detention Center", "Tholian Memorial Crystal", "Tholian Monument", "Tholian Archive", "Tholian School", "Tholian Academy Annex", "Tholian Observation Tower", "Tholian Weather Control", "Tholian Emergency Center", "Tholian Web Spinner Ground", "Tholian Defense Tower", "Tholian Shield Generator", "Tholian Weapon Battery"),
        Troops = GenerateTroopList("Tholian Warrior", "Tholian Commander", "Tholian Guard", "Tholian Heavy Warrior", "Tholian Web Spinner Ground", "Tholian Crystal Cannon", "Tholian Heat Projector", "Tholian Siege Crystal", "Tholian Scout", "Tholian Infiltrator", "Tholian Sniper Crystal", "Tholian Demolitions", "Tholian Medic Crystal", "Tholian Engineer", "Tholian Special Forces", "Tholian Elite Guard"),
        PortraitVariants = GeneratePortraitList("Tholian Commander", "Tholian Warrior", "Tholian Worker", "Tholian Science Caste", "Tholian Diplomat", "Tholian Leader", "Tholian Elder", "Tholian Young", "Tholian Guard", "Tholian Engineer", "Tholian Pilot", "Tholian Medical", "Tholian Merchant", "Tholian Artist", "Tholian Web Master", "Tholian Assembly Member", "Tholian Admiral", "Tholian General", "Tholian Ambassador", "Tholian Spy", "Tholian Prisoner", "Tholian Refugee", "Tholian Colonist", "Tholian Miner", "Tholian Scientist", "Tholian Teacher", "Tholian Student", "Tholian Priest", "Tholian Oracle", "Tholian Judge", "Tholian Executioner", "Tholian Healer", "Tholian Builder", "Tholian Farmer", "Tholian Noble", "Tholian Royal"),
        HouseSymbols = GenerateSymbolList("Tholian Assembly Seal", "Tholian Web Pattern", "Tholian Crystal Lattice", "Tholian Heat Symbol", "Tholian Faceted Gem", "Tholian Geometric Star", "Tholian Hexagon", "Tholian Pentagon", "Tholian Triangle", "Tholian Diamond", "Tholian Spiral", "Tholian Web Anchor", "Tholian Caste Worker", "Tholian Caste Warrior", "Tholian Caste Leader", "Tholian Caste Science", "Tholian Caste Builder", "Tholian Caste Pilot", "Tholian Fleet Command", "Tholian Ground Command", "Tholian Defense Force", "Tholian Intelligence", "Tholian Diplomatic Corps", "Tholian Trade Guild", "Tholian Mining Guild", "Tholian Science Academy", "Tholian Military Academy", "Tholian Cultural Center", "Tholian Medical Corps", "Tholian Engineering Corps", "Tholian Communication Guild", "Tholian Transport Guild", "Tholian Honor Symbol", "Tholian Victory Symbol", "Tholian Unity Symbol", "Tholian Home World", "Tholian Colony One", "Tholian Colony Two", "Tholian Border Marker", "Tholian Warning Symbol", "Tholian Welcome Symbol", "Tholian Sacred Crystal", "Tholian Ancient Symbol", "Tholian Modern Symbol", "Tholian Future Symbol", "Tholian Alliance Seal", "Tholian War Seal", "Tholian Peace Seal")
    };
    
    private FactionProfile CreateOrionProfile() => new FactionProfile
    {
        Faction = Faction.Orion,
        Name = "Orion",
        DesignLanguage = "Pirate aesthetic, salvaged and modified ships, sensual curves, criminal enterprise style",
        ColorScheme = "Green skin tones reflected in ship colors, gold and bronze accents, dark greens and blacks",
        Architecture = "Decadent pleasure palaces, hidden syndicate bases, slave markets, black market aesthetic",
        RaceFeatures = "Green skin (ranging from light to dark), black hair common, attractive humanoid appearance",
        ClothingDetails = "Revealing attire for females (cultural), practical pirate gear, syndicate uniforms, dancer costumes",
        HeraldicStyle = "Syndicate symbols, dagger and coin motifs, serpentine designs",
        MilitaryShips = GenerateShipList("Orion Interceptor", "Orion Raider", "Orion Slaver", "Orion Marauder", "Orion Corsair", "Orion Pirate Ship", "Orion Blackguard", "Orion Brigantine", "Orion Cutlass", "Orion Scimitar", "Orion Privateer", "Orion Blockade Runner", "Orion Smuggler Fast", "Orion Syndicate Cruiser", "Orion Battle Cruiser", "Orion Dreadnought", "Orion Carrier", "Orion Fighter", "Orion Bomber", "Orion Assault Ship", "Orion Boarding Craft", "Orion Slave Ship", "Orion Prison Ship", "Orion Patrol Craft", "Orion Scout", "Orion Stealth Ship", "Orion Electronic Warfare", "Orion Mine Layer", "Orion Torpedo Boat", "Orion Gunship", "Orion Corvette", "Orion Frigate", "Orion Destroyer", "Orion Command Ship", "Orion Flagship", "Orion Super Raider"),
        CivilianShips = GenerateShipList("Orion Transport", "Orion Cargo Ship", "Orion Trade Vessel", "Orion Smuggler", "Orion Luxury Yacht", "Orion Pleasure Barge", "Orion Casino Ship", "Orion Slave Transport", "Orion Colony Ship", "Orion Mining Vessel", "Orion Construction Ship", "Orion Salvage Vessel", "Orion Repair Ship", "Orion Tanker", "Orion Freighter", "Orion Courier", "Orion VIP Transport", "Orion Diplomatic Vessel", "Orion Medical Ship", "Orion Hospital Ship", "Orion Research Vessel", "Orion Survey Ship", "Orion Exploration Craft", "Orion Tour Ship", "Orion Entertainment Vessel", "Orion Arena Ship", "Orion Auction Ship", "Orion Market Vessel", "Orion Bank Ship", "Orion Vault Transport", "Orion Shuttle", "Orion Personal Craft", "Orion Racing Ship", "Orion Sports Vessel", "Orion News Ship", "Orion Communication Vessel"),
        MilitaryStructures = GenerateStructureList("Orion Syndicate Base", "Orion Pirate Haven", "Orion Defense Platform", "Orion Weapon Station", "Orion Shield Generator", "Orion Sensor Array", "Orion Command Post", "Orion Fleet Base", "Orion Repair Dock", "Orion Training Center", "Orion Intelligence Hub", "Orion Communication Array", "Orion Early Warning", "Orion Patrol Base", "Orion Border Station", "Orion Checkpoint", "Orion Garrison", "Orion Armory", "Orion Prison Station", "Orion Interrogation Platform", "Orion Security Hub", "Orion Tactical Center", "Orion Operations Base", "Orion Logistics Hub", "Orion Supply Depot", "Orion Fighter Bay", "Orion Raider Base", "Orion Smuggler Haven", "Orion Black Market Station", "Orion Slave Pen", "Orion Arena Platform", "Orion Bounty Station", "Orion Contract Station", "Orion Mercenary Base", "Orion Assassin Guild", "Orion Thief Guild"),
        CivilianStructures = GenerateStructureList("Orion Trade Station", "Orion Market Hub", "Orion Pleasure Palace", "Orion Casino Station", "Orion Entertainment Hub", "Orion Arena Station", "Orion Auction House", "Orion Slave Market", "Orion Banking Station", "Orion Vault Station", "Orion Cargo Depot", "Orion Passenger Terminal", "Orion Shipyard", "Orion Drydock", "Orion Mining Station", "Orion Refinery", "Orion Power Station", "Orion Agricultural Station", "Orion Food Processing", "Orion Medical Station", "Orion Hospital", "Orion Research Station", "Orion Science Platform", "Orion University", "Orion Academy", "Orion Library", "Orion Museum", "Orion Cultural Center", "Orion Diplomatic Station", "Orion Embassy", "Orion Communication Relay", "Orion Transit Hub", "Orion Hotel Station", "Orion Resort Platform", "Orion Spa Station", "Orion Recreation Center"),
        Buildings = GenerateBuildingList("Orion Syndicate Headquarters", "Orion Crime Boss Palace", "Orion Slave Market", "Orion Auction House", "Orion Pleasure Palace", "Orion Casino", "Orion Arena", "Orion Fight Pit", "Orion Dance Hall", "Orion Brothel", "Orion Bank Vault", "Orion Counting House", "Orion Trade Center", "Orion Market Square", "Orion Black Market", "Orion Smuggler Den", "Orion Pirate Haven", "Orion Thief Guild", "Orion Assassin Guild", "Orion Bounty Office", "Orion Contract Office", "Orion Prison", "Orion Interrogation Center", "Orion Slave Pen", "Orion Training Camp", "Orion Barracks", "Orion Armory", "Orion Shipyard", "Orion Repair Bay", "Orion Mining Complex", "Orion Refinery", "Orion Power Plant", "Orion Residence Mansion", "Orion Apartment Block", "Orion Hospital", "Orion Medical Clinic", "Orion School", "Orion Academy", "Orion Library", "Orion Museum", "Orion Art Gallery", "Orion Music Hall", "Orion Theater", "Orion Restaurant", "Orion Bar", "Orion Hotel", "Orion Resort", "Orion Spa"),
        Troops = GenerateTroopList("Orion Syndicate Soldier", "Orion Pirate", "Orion Slaver", "Orion Guard", "Orion Mercenary", "Orion Assassin", "Orion Thief", "Orion Bounty Hunter", "Orion Heavy Weapons", "Orion Sniper", "Orion Demolitions", "Orion Scout", "Orion Infiltrator", "Orion Seductress Spy", "Orion Crime Boss Guard", "Orion Elite Mercenary"),
        PortraitVariants = GeneratePortraitList("Orion Male Syndicate Boss", "Orion Male Pirate Captain", "Orion Male Slaver", "Orion Male Merchant", "Orion Male Warrior", "Orion Male Guard", "Orion Female Syndicate Leader", "Orion Female Pirate Captain", "Orion Female Dancer", "Orion Female Slave Girl", "Orion Female Merchant", "Orion Female Warrior", "Orion Male Assassin", "Orion Female Assassin", "Orion Male Thief", "Orion Female Thief", "Orion Male Bounty Hunter", "Orion Female Bounty Hunter", "Orion Male Civilian", "Orion Female Civilian", "Orion Male Child", "Orion Female Child", "Orion Male Elder", "Orion Female Elder", "Orion Male Scientist", "Orion Female Doctor", "Orion Male Engineer", "Orion Female Pilot", "Orion Male Freed Slave", "Orion Female Freed Slave", "Orion Male Noble", "Orion Female Noble", "Orion Male Gladiator", "Orion Female Gladiator", "Orion Male Bartender", "Orion Female Entertainer"),
        HouseSymbols = GenerateSymbolList("Orion Syndicate Symbol", "Orion Pirate Flag", "Orion Slaver Mark", "Orion Trade Guild", "Orion Assassin Guild", "Orion Thief Guild", "Orion Bounty Guild", "Orion Mercenary Guild", "Orion Banking Clan", "Orion Mining Clan", "Orion Shipping Clan", "Orion Entertainment Guild", "Orion Arena League", "Orion Fight Circuit", "Orion Dance Troupe", "Orion Music Guild", "Orion Crime Family One", "Orion Crime Family Two", "Orion Crime Family Three", "Orion Crime Family Four", "Orion Crime Family Five", "Orion Boss Seal", "Orion Underboss Seal", "Orion Captain Seal", "Orion Lieutenant Seal", "Orion Soldier Seal", "Orion Associate Seal", "Orion Contract Seal", "Orion Bounty Seal", "Orion Hit Seal", "Orion Theft Seal", "Orion Smuggling Seal", "Orion Slavery Seal", "Orion Freedom Seal", "Orion Neutral Seal", "Orion Alliance Seal", "Orion War Seal", "Orion Peace Seal", "Orion Honor Seal", "Orion Betrayal Warning", "Orion Death Mark", "Orion Protection Seal", "Orion Territory Marker", "Orion Border Marker", "Orion Home World", "Orion Colony Marker", "Orion Outpost Marker", "Orion Haven Marker")
    };

    private FactionProfile CreateHirogenProfile() => new FactionProfile
    {
        Faction = Faction.Hirogen,
        Name = "Hirogen",
        DesignLanguage = "Massive hunter ships, heavily armored, trophy displays, predator aesthetic, organic-tech hybrid",
        ColorScheme = "Bronze, olive green, dark brown, bone white trophy accents, amber lighting",
        CivilianDesignLanguage = "Functional hunting vessels, trophy transport, nomadic fleet design",
        CivilianColorScheme = "Dark olive, bronze trim, weathered hunting vessel aesthetic",
        Architecture = "Nomadic hunting lodges, trophy halls, training arenas, sparse functional structures",
        // CRITICAL HIROGEN PHYSICAL FEATURES:
        // - DUAL-LOBED CRANIUM: Two distinct bulging lobes on top of skull (SIGNATURE FEATURE)
        // - FLAT NOSE: No nasal bridge, nostrils flush with face like reptile/Saurian
        // - NO LIPS: Lipless reptilian mouth slit
        // - GILA MONSTER SKIN: Pebbled/scaled texture, tan/beige/olive with mottled patches
        // - MASSIVE BUILD: Tower over humans, oversized head
        // - ARMOR: Silver-blue metallic when suited; bronze/olive when unarmored
        RaceFeatures = "DUAL-LOBED CRANIUM (two bulges on skull), FLAT NOSE (flush with face), NO LIPS (reptilian mouth), PEBBLED GILA MONSTER SKIN (tan/olive mottled), massive build, completely bald",
        ClothingDetails = "ARMORED: Silver-blue metallic full armor with helmet/respirator mask, narrow visor. UNARMORED: Bronze/olive hunting armor. Trophy decorations (bones, teeth), sensor equipment",
        HeraldicStyle = "Predator symbols, hunting trophies, prey species marks, alpha status indicators",
        MilitaryShips = GenerateShipList("Hirogen Hunting Vessel", "Hirogen Attack Ship", "Hirogen Warship", "Hirogen Venatic Class", "Hirogen Pursuit Craft", "Hirogen Heavy Hunter", "Hirogen Alpha Ship", "Hirogen Pack Leader", "Hirogen Ambush Craft", "Hirogen Stealth Hunter", "Hirogen Trophy Ship", "Hirogen Arena Ship", "Hirogen Training Vessel", "Hirogen Scout Ship", "Hirogen Interceptor", "Hirogen Raider", "Hirogen Assault Ship", "Hirogen Boarding Craft", "Hirogen Fighter", "Hirogen Bomber", "Hirogen Carrier", "Hirogen Command Ship", "Hirogen Flagship", "Hirogen Battle Cruiser", "Hirogen Dreadnought", "Hirogen Destroyer", "Hirogen Frigate", "Hirogen Corvette", "Hirogen Patrol Craft", "Hirogen Gunship", "Hirogen Torpedo Boat", "Hirogen Mine Layer", "Hirogen Electronic Warfare", "Hirogen Communications Ship", "Hirogen Supply Ship", "Hirogen Repair Vessel"),
        CivilianShips = GenerateShipList("Hirogen Transport", "Hirogen Cargo Ship", "Hirogen Colony Ship", "Hirogen Mining Vessel", "Hirogen Construction Ship", "Hirogen Salvage Vessel", "Hirogen Repair Ship", "Hirogen Tanker", "Hirogen Freighter", "Hirogen Courier", "Hirogen Medical Ship", "Hirogen Research Vessel", "Hirogen Survey Ship", "Hirogen Exploration Craft", "Hirogen Shuttle", "Hirogen Personal Craft", "Hirogen Trophy Transport", "Hirogen Habitat Ship", "Hirogen Generation Ship", "Hirogen Nomad Vessel", "Hirogen Fleet Tender", "Hirogen Supply Vessel", "Hirogen Food Processing", "Hirogen Water Harvester", "Hirogen Fuel Collector", "Hirogen Ore Processor", "Hirogen Manufacturing Ship", "Hirogen Training Ship", "Hirogen Youth Vessel", "Hirogen Elder Ship", "Hirogen Diplomatic Vessel", "Hirogen Trade Ship", "Hirogen Barter Vessel", "Hirogen Exchange Ship", "Hirogen Communication Relay", "Hirogen Beacon Ship"),
        MilitaryStructures = GenerateStructureList("Hirogen Hunting Station", "Hirogen Trophy Hall Station", "Hirogen Training Arena", "Hirogen Defense Platform", "Hirogen Weapon Station", "Hirogen Shield Generator", "Hirogen Sensor Array", "Hirogen Command Post", "Hirogen Fleet Base", "Hirogen Repair Dock", "Hirogen Intelligence Hub", "Hirogen Communication Array", "Hirogen Early Warning", "Hirogen Patrol Base", "Hirogen Border Station", "Hirogen Checkpoint", "Hirogen Garrison", "Hirogen Armory", "Hirogen Prison Station", "Hirogen Interrogation Platform", "Hirogen Security Hub", "Hirogen Tactical Center", "Hirogen Operations Base", "Hirogen Logistics Hub", "Hirogen Supply Depot", "Hirogen Fighter Bay", "Hirogen Hunter Bay", "Hirogen Ambush Point", "Hirogen Relay Station", "Hirogen Prey Tracker", "Hirogen Hunt Coordinator", "Hirogen Alpha Station", "Hirogen Pack Station", "Hirogen Territory Marker", "Hirogen Boundary Station", "Hirogen Outpost"),
        CivilianStructures = GenerateStructureList("Hirogen Trade Station", "Hirogen Market Hub", "Hirogen Trophy Exchange", "Hirogen Hunting Bazaar", "Hirogen Cargo Depot", "Hirogen Shipyard", "Hirogen Drydock", "Hirogen Mining Station", "Hirogen Refinery", "Hirogen Power Station", "Hirogen Agricultural Station", "Hirogen Food Processing", "Hirogen Medical Station", "Hirogen Hospital", "Hirogen Research Station", "Hirogen Science Platform", "Hirogen Training Academy", "Hirogen Youth Center", "Hirogen Elder Sanctuary", "Hirogen Cultural Center", "Hirogen Museum of Hunts", "Hirogen Trophy Gallery", "Hirogen Communication Relay", "Hirogen Transit Hub", "Hirogen Habitat Station", "Hirogen Recreation Center", "Hirogen Arena Station", "Hirogen Combat Training", "Hirogen Simulation Center", "Hirogen Holodeck Station", "Hirogen Prey Reserve", "Hirogen Wildlife Station", "Hirogen Preservation Center", "Hirogen Diplomatic Station", "Hirogen Embassy", "Hirogen Neutral Zone"),
        Buildings = GenerateBuildingList("Hirogen Alpha Lodge", "Hirogen Hunt Command", "Hirogen Trophy Hall", "Hirogen Great Hunt Memorial", "Hirogen Barracks", "Hirogen Training Arena", "Hirogen Combat Pit", "Hirogen Weapon Forge", "Hirogen Armor Works", "Hirogen Sensor Workshop", "Hirogen Tracking Center", "Hirogen Prey Database", "Hirogen Hunt Planning", "Hirogen Strategy Hall", "Hirogen Pack Quarters", "Hirogen Alpha Residence", "Hirogen Beta Quarters", "Hirogen Youth Training", "Hirogen Elder Council", "Hirogen Medical Bay", "Hirogen Healing Center", "Hirogen Gene Lab", "Hirogen Enhancement Facility", "Hirogen Food Hall", "Hirogen Meat Processing", "Hirogen Preservation Facility", "Hirogen Supply Depot", "Hirogen Equipment Storage", "Hirogen Vehicle Bay", "Hirogen Shuttle Port", "Hirogen Communication Tower", "Hirogen Relay Station", "Hirogen Power Generator", "Hirogen Shield Generator", "Hirogen Defense Tower", "Hirogen Watchtower", "Hirogen Observation Post", "Hirogen Sensor Tower", "Hirogen Holographic Arena", "Hirogen Simulation Chamber", "Hirogen Virtual Hunt", "Hirogen Recreation Hall", "Hirogen Trophy Display", "Hirogen Museum", "Hirogen Archive", "Hirogen Library", "Hirogen Diplomatic Hall", "Hirogen Trade Post"),
        Troops = GenerateTroopList("Hirogen Hunter", "Hirogen Alpha Hunter", "Hirogen Beta Hunter", "Hirogen Pack Leader", "Hirogen Tracker", "Hirogen Stalker", "Hirogen Ambusher", "Hirogen Sniper", "Hirogen Heavy Hunter", "Hirogen Arena Fighter", "Hirogen Combat Trainer", "Hirogen Youth Hunter", "Hirogen Elder Hunter", "Hirogen Trophy Keeper", "Hirogen Prey Handler", "Hirogen Hunt Master"),
        PortraitVariants = GeneratePortraitList("Hirogen Alpha Male", "Hirogen Beta Male", "Hirogen Hunter Male", "Hirogen Tracker Male", "Hirogen Elder Male", "Hirogen Youth Male", "Hirogen Alpha Female", "Hirogen Hunter Female", "Hirogen Tracker Female", "Hirogen Elder Female", "Hirogen Arena Champion", "Hirogen Hunt Master", "Hirogen Pack Leader", "Hirogen Lone Wolf", "Hirogen Veteran Hunter", "Hirogen Trophy Collector", "Hirogen Prey Specialist", "Hirogen Technology Expert", "Hirogen Medic", "Hirogen Engineer", "Hirogen Pilot", "Hirogen Navigator", "Hirogen Communications", "Hirogen Weapons Master", "Hirogen Armor Smith", "Hirogen Trainer", "Hirogen Youth Mentor", "Hirogen Elder Sage", "Hirogen Diplomat", "Hirogen Trader", "Hirogen Storyteller", "Hirogen Historian", "Hirogen Scout", "Hirogen Infiltrator", "Hirogen Heavy Weapons", "Hirogen Commander"),
        HouseSymbols = GenerateSymbolList("Hirogen Alpha Symbol", "Hirogen Pack Mark", "Hirogen Hunt Seal", "Hirogen Trophy Mark", "Hirogen Prey Symbol", "Hirogen Kill Count", "Hirogen Arena Champion", "Hirogen Grand Hunt", "Hirogen First Kill", "Hirogen Alpha Pack", "Hirogen Beta Pack", "Hirogen Gamma Pack", "Hirogen Delta Pack", "Hirogen Hunting Ground", "Hirogen Territory Mark", "Hirogen Border Sign", "Hirogen Warning Sign", "Hirogen Challenge Mark", "Hirogen Honor Symbol", "Hirogen Veteran Mark", "Hirogen Elder Status", "Hirogen Youth Mark", "Hirogen Training Symbol", "Hirogen Combat Ready", "Hirogen Hunt Active", "Hirogen Rest Period", "Hirogen Migration Mark", "Hirogen Fleet Symbol", "Hirogen Ship Mark", "Hirogen Weapon Symbol", "Hirogen Armor Mark", "Hirogen Tech Symbol", "Hirogen Medical Mark", "Hirogen Trade Symbol", "Hirogen Neutral Mark", "Hirogen Alliance Symbol", "Hirogen Enemy Mark", "Hirogen Prey Species One", "Hirogen Prey Species Two", "Hirogen Prey Species Three", "Hirogen Trophy Rank One", "Hirogen Trophy Rank Two", "Hirogen Trophy Rank Three", "Hirogen Trophy Rank Four", "Hirogen Trophy Rank Five", "Hirogen Grand Master", "Hirogen Legend Status", "Hirogen Eternal Hunt")
    };

    // =================================================================
    // NEW FACTIONS - Betazoid, Maquis, Pirates, Species 8472
    // =================================================================

    private FactionProfile CreateBetazoidProfile() => new FactionProfile
    {
        Faction = Faction.Betazoid,
        Name = "Betazoid",
        DesignLanguage = "Elegant, organic, flowing designs - peaceful telepathic culture, Federation member",
        ColorScheme = "Soft purples, lavenders, silver, white - calming, sophisticated palette",
        CivilianDesignLanguage = "Graceful architecture, gardens, meditation spaces, open designs",
        CivilianColorScheme = "Light purple, cream, silver accents, natural wood tones",
        Architecture = "Organic flowing structures, large windows, integration with nature, peaceful aesthetic",
        RaceFeatures = "Humanoid appearance nearly identical to humans, SOLID BLACK EYES (no visible iris/pupil - key feature), telepathic/empathic abilities",
        ClothingDetails = "Flowing elegant robes in purples and silvers, or Starfleet uniforms if serving in fleet",
        HeraldicStyle = "Abstract mind/telepathy symbols, flowing organic designs, peaceful imagery",
        // Betazoids primarily use Federation ships - limited own military
        IsPortraitOnly = true,
        HasShips = false,
        HasBuildings = true,
        HasTroops = false,
        Buildings = GenerateBuildingList("Betazoid Embassy", "Betazoid Meditation Center", "Betazoid University of Psychology", "Betazoid Telepathy Institute", "Betazoid Cultural Center", "Betazoid Garden Palace", "Betazoid Healing Center", "Betazoid Counseling Hall", "Betazoid Wedding Chapel", "Betazoid Art Gallery", "Betazoid Music Hall", "Betazoid Library", "Betazoid Botanical Garden", "Betazoid Residence Manor", "Betazoid Government Hall", "Betazoid Diplomatic Center"),
        PortraitVariants = GeneratePortraitList("Betazoid Female Counselor", "Betazoid Male Diplomat", "Betazoid Female Ambassador", "Betazoid Male Telepath", "Betazoid Female Elder", "Betazoid Male Youth", "Betazoid Female Starfleet Officer", "Betazoid Male Civilian", "Betazoid Female Noble", "Betazoid Male Healer", "Betazoid Female Student", "Betazoid Male Professor", "Betazoid Female Bride", "Betazoid Male Ceremonial", "Betazoid Female Artist", "Betazoid Male Musician"),
        HouseSymbols = GenerateSymbolList("Betazoid Mind Symbol", "Betazoid Telepathy Mark", "Betazoid House Troi", "Betazoid Noble House", "Betazoid Meditation Symbol", "Betazoid Peace Sign", "Betazoid Unity Mark", "Betazoid Wedding Symbol", "Betazoid Government Seal", "Betazoid University Crest", "Betazoid Healing Symbol", "Betazoid Art Guild", "Betazoid Music Guild", "Betazoid Diplomatic Seal", "Betazoid Cultural Mark", "Betazoid Federation Member")
    };

    private FactionProfile CreateMaquisProfile() => new FactionProfile
    {
        Faction = Faction.Maquis,
        Name = "Maquis",
        DesignLanguage = "Rugged, improvised, guerrilla aesthetic - converted civilian ships, hidden bases",
        ColorScheme = "Earth tones, browns, greens, rust - camouflage and utilitarian",
        CivilianDesignLanguage = "Hidden settlements, underground bunkers, camouflaged outposts",
        CivilianColorScheme = "Natural camouflage colors, weathered materials",
        Architecture = "Hidden bunkers, cave bases, camouflaged settlements, repurposed structures",
        RaceFeatures = "Mixed species - primarily Human, Bajoran, some Vulcan and other Federation species who joined the resistance",
        ClothingDetails = "Practical civilian clothing, leather jackets, utilitarian gear, no uniforms - resistance fighter look",
        HeraldicStyle = "Resistance symbols, fist/freedom imagery, anti-Cardassian marks",
        MilitaryShips = GenerateShipList("Maquis Raider", "Maquis Fighter", "Maquis Attack Ship", "Maquis Interceptor", "Maquis Converted Freighter", "Maquis Armed Transport", "Maquis Blockade Runner", "Maquis Scout", "Maquis Patrol Craft", "Maquis Bomber", "Maquis Strike Ship", "Maquis Gunship", "Maquis Fast Attack", "Maquis Infiltrator", "Maquis Stealth Ship", "Maquis Command Ship", "Maquis Carrier", "Maquis Heavy Raider", "Maquis Torpedo Boat", "Maquis Mine Layer", "Maquis Electronic Warfare", "Maquis Communications Ship", "Maquis Supply Runner", "Maquis Medical Ship", "Maquis Repair Vessel", "Maquis Salvage Ship", "Maquis Tug", "Maquis Shuttle", "Maquis Courier", "Maquis Escape Pod Carrier", "Maquis Decoy Ship", "Maquis Q-Ship", "Maquis Converted Yacht", "Maquis Armed Shuttle", "Maquis Fighter Carrier", "Maquis Flagship"),
        CivilianShips = GenerateShipList("Maquis Transport", "Maquis Cargo Ship", "Maquis Freighter", "Maquis Refugee Ship", "Maquis Colony Ship", "Maquis Supply Vessel", "Maquis Medical Transport", "Maquis Passenger Ship", "Maquis Shuttle", "Maquis Courier", "Maquis Personal Craft", "Maquis Family Ship", "Maquis Farm Ship", "Maquis Mining Vessel", "Maquis Construction Ship", "Maquis Repair Ship"),
        MilitaryStructures = GenerateStructureList("Maquis Hidden Base", "Maquis Asteroid Outpost", "Maquis Defense Platform", "Maquis Sensor Array", "Maquis Communications Relay", "Maquis Weapons Cache", "Maquis Fighter Bay", "Maquis Command Bunker", "Maquis Shield Generator", "Maquis Ambush Point", "Maquis Early Warning", "Maquis Patrol Station", "Maquis Border Outpost", "Maquis Prison", "Maquis Interrogation Post", "Maquis Training Camp"),
        CivilianStructures = GenerateStructureList("Maquis Colony", "Maquis Settlement", "Maquis Underground City", "Maquis Cave Base", "Maquis Hidden Farm", "Maquis Medical Station", "Maquis School", "Maquis Market", "Maquis Refugee Camp", "Maquis Supply Depot", "Maquis Shipyard", "Maquis Repair Facility", "Maquis Communication Hub", "Maquis Meeting Hall", "Maquis Memorial", "Maquis Cemetery"),
        Buildings = GenerateBuildingList("Maquis Command Center", "Maquis Barracks", "Maquis Armory", "Maquis Training Hall", "Maquis Medical Bay", "Maquis Communications", "Maquis Strategy Room", "Maquis Mess Hall", "Maquis Bunker", "Maquis Underground Hangar", "Maquis Weapons Workshop", "Maquis Supply Cache", "Maquis Intelligence Hub", "Maquis Meeting Room", "Maquis Leader Quarters", "Maquis Cell Block", "Maquis Interrogation Room", "Maquis Escape Tunnel", "Maquis Hidden Entrance", "Maquis Watchtower", "Maquis Sniper Post", "Maquis Ambush Point", "Maquis Safe House", "Maquis Dead Drop", "Maquis Coded Message Center", "Maquis Recruitment Office", "Maquis Memorial Wall", "Maquis Trophy Room", "Maquis Planning Room", "Maquis Radio Station", "Maquis Power Generator", "Maquis Water Recycler", "Maquis Food Storage", "Maquis Family Quarters", "Maquis Children's Area", "Maquis Recreation Room", "Maquis Library", "Maquis Holodeck", "Maquis Transporter Room", "Maquis Shuttle Bay", "Maquis Docking Port", "Maquis Cargo Bay", "Maquis Engineering", "Maquis Reactor Room", "Maquis Life Support", "Maquis Environmental Control", "Maquis Security Station", "Maquis Checkpoint"),
        Troops = GenerateTroopList("Maquis Fighter", "Maquis Cell Leader", "Maquis Saboteur", "Maquis Sniper", "Maquis Infiltrator", "Maquis Demolitions Expert", "Maquis Medic", "Maquis Scout", "Maquis Communications", "Maquis Heavy Weapons", "Maquis Pilot", "Maquis Engineer", "Maquis Intelligence", "Maquis Recruiter", "Maquis Veteran", "Maquis Commander"),
        PortraitVariants = GeneratePortraitList("Maquis Human Male Fighter", "Maquis Human Female Fighter", "Maquis Bajoran Male", "Maquis Bajoran Female", "Maquis Vulcan Male", "Maquis Human Male Leader", "Maquis Human Female Leader", "Maquis Veteran Male", "Maquis Veteran Female", "Maquis Youth Male", "Maquis Youth Female", "Maquis Medic Female", "Maquis Engineer Male", "Maquis Pilot Female", "Maquis Sniper Male", "Maquis Saboteur Female"),
        HouseSymbols = GenerateSymbolList("Maquis Resistance Symbol", "Maquis Fist Symbol", "Maquis Freedom Mark", "Maquis Cell Alpha", "Maquis Cell Beta", "Maquis Cell Gamma", "Maquis Strike Team", "Maquis Intelligence", "Maquis Medical", "Maquis Engineering", "Maquis Pilot Wings", "Maquis Veteran Badge", "Maquis Leadership", "Maquis Memorial", "Maquis Victory Mark", "Maquis Anti-Cardassian")
    };

    private FactionProfile CreatePiratesProfile() => new FactionProfile
    {
        Faction = Faction.Pirates,
        Name = "Pirates",
        DesignLanguage = "Mismatched, cobbled together, intimidating - stolen and modified ships from various factions",
        ColorScheme = "Dark colors, black, rust, blood red accents - skull and crossbones imagery",
        CivilianDesignLanguage = "Hidden asteroid bases, lawless stations, black markets",
        CivilianColorScheme = "Grimy, weathered, neon signs, dark and dangerous",
        Architecture = "Asteroid hideouts, derelict station conversions, lawless ports",
        // NOTE: Do NOT use Nausicaans or Orions - they have their own factions!
        // Use diverse species: Humans, Bolians, Tellarites, Lurians, Yridians, Pakleds, etc.
        RaceFeatures = "Mixed species (NOT Nausicaan/Orion - they have own factions). Humans, Bolians (blue, ridge), Tellarites (porcine), Lurians (large head), Yridians (wrinkled gray), Pakleds, other aliens. Rough, scarred, dangerous",
        ClothingDetails = "Leader: Dark military uniform with medals. Crew: Mismatched armor, leather, stolen uniforms, weapons visible",
        HeraldicStyle = "Skull imagery, crossed weapons, intimidating symbols, crew markings",
        MilitaryShips = GenerateShipList("Pirate Raider", "Pirate Frigate", "Pirate Cruiser", "Pirate Destroyer", "Pirate Battleship", "Pirate Carrier", "Pirate Flagship", "Pirate Interceptor", "Pirate Gunboat", "Pirate Corvette", "Pirate Fast Attack", "Pirate Boarding Ship", "Pirate Stealth Raider", "Pirate Q-Ship", "Pirate Armed Freighter", "Pirate Missile Boat", "Pirate Mine Layer", "Pirate Electronic Warfare", "Pirate Command Ship", "Pirate Dreadnought", "Pirate Assault Ship", "Pirate Landing Craft", "Pirate Fighter", "Pirate Bomber", "Pirate Scout", "Pirate Patrol Craft", "Pirate Blockade Runner", "Pirate Smuggler Ship", "Pirate Slave Ship", "Pirate Prison Ship", "Pirate Torture Ship", "Pirate Trophy Ship", "Pirate Salvage Vessel", "Pirate Tug", "Pirate Shuttle", "Pirate Escape Craft"),
        CivilianShips = GenerateShipList("Pirate Transport", "Pirate Cargo Hauler", "Pirate Freighter", "Pirate Tanker", "Pirate Passenger Ship", "Pirate Luxury Yacht", "Pirate Pleasure Barge", "Pirate Casino Ship", "Pirate Slave Transport", "Pirate Contraband Runner", "Pirate Medical Ship", "Pirate Repair Vessel", "Pirate Supply Ship", "Pirate Mining Ship", "Pirate Salvage Tug", "Pirate Personal Craft"),
        MilitaryStructures = GenerateStructureList("Pirate Fortress", "Pirate Defense Platform", "Pirate Weapons Platform", "Pirate Shield Generator", "Pirate Sensor Array", "Pirate Communications Relay", "Pirate Fighter Bay", "Pirate Command Station", "Pirate Prison Station", "Pirate Interrogation Platform", "Pirate Ambush Point", "Pirate Patrol Base", "Pirate Border Outpost", "Pirate Warning Beacon", "Pirate Mine Field Control", "Pirate Torpedo Platform"),
        CivilianStructures = GenerateStructureList("Pirate Haven", "Pirate Port", "Pirate Black Market", "Pirate Cantina Station", "Pirate Gambling Den", "Pirate Pleasure Station", "Pirate Slave Market", "Pirate Shipyard", "Pirate Drydock", "Pirate Repair Station", "Pirate Supply Depot", "Pirate Cargo Hub", "Pirate Fence Station", "Pirate Hideout", "Pirate Safe House", "Pirate Meeting Point"),
        Buildings = GenerateBuildingList("Pirate Captain's Quarters", "Pirate Crew Barracks", "Pirate Armory", "Pirate Weapons Locker", "Pirate Treasure Vault", "Pirate Brig", "Pirate Torture Chamber", "Pirate Interrogation Room", "Pirate War Room", "Pirate Navigation Center", "Pirate Communications", "Pirate Black Market", "Pirate Cantina", "Pirate Gambling Hall", "Pirate Fighting Pit", "Pirate Slave Pen", "Pirate Medical Bay", "Pirate Mess Hall", "Pirate Kitchen", "Pirate Storage", "Pirate Engineering", "Pirate Reactor Room", "Pirate Hangar", "Pirate Docking Bay", "Pirate Transporter Room", "Pirate Trophy Room", "Pirate Captain's Office", "Pirate Meeting Room", "Pirate Lookout Tower", "Pirate Defense Tower", "Pirate Gun Emplacement", "Pirate Shield Generator", "Pirate Power Plant", "Pirate Water Recycler", "Pirate Life Support", "Pirate Escape Pods", "Pirate Hidden Passage", "Pirate Secret Room", "Pirate Contraband Storage", "Pirate Drug Lab", "Pirate Counterfeiting", "Pirate Hacking Center", "Pirate Intelligence", "Pirate Recruitment", "Pirate Training Pit", "Pirate Execution Ground", "Pirate Graveyard", "Pirate Memorial"),
        Troops = GenerateTroopList("Pirate Captain", "Pirate First Mate", "Pirate Bosun", "Pirate Raider", "Pirate Boarder", "Pirate Gunner", "Pirate Sniper", "Pirate Enforcer", "Pirate Thug", "Pirate Slaver", "Pirate Torturer", "Pirate Scout", "Pirate Pilot", "Pirate Engineer", "Pirate Medic", "Pirate Recruit"),
        // NOTE: Diverse species - NOT Nausicaan or Orion (they have own factions)
        PortraitVariants = GeneratePortraitList("Pirate Human Male Commander", "Pirate Human Female Commander", "Pirate Bolian Male", "Pirate Bolian Female", "Pirate Tellarite Male", "Pirate Lurian Male", "Pirate Yridian Male", "Pirate Pakled Male", "Pirate Human Male Raider", "Pirate Human Female Raider", "Pirate Alien Male", "Pirate Alien Female", "Pirate Scarred Veteran", "Pirate Cyborg", "Pirate Masked Raider", "Pirate Boslic Female"),
        HouseSymbols = GenerateSymbolList("Pirate Skull Crossbones", "Pirate Jolly Roger", "Pirate Captain's Mark", "Pirate Crew Symbol", "Pirate Fleet Mark", "Pirate Boarding Party", "Pirate Kill Count", "Pirate Ship Silhouette", "Pirate Treasure Mark", "Pirate Warning Flag", "Pirate Surrender Demand", "Pirate Alliance Symbol", "Pirate Rivalry Mark", "Pirate Territory Claim", "Pirate Bounty Mark", "Pirate Most Wanted")
    };

    private FactionProfile CreateNausicaanProfile() => new FactionProfile
    {
        Faction = Faction.Nausicaan,
        Name = "Nausicaan",
        DesignLanguage = "Brutal, heavy, intimidating - crude but effective warships, rough industrial aesthetic",
        ColorScheme = "Dark browns, grays, rust, gunmetal - brutal and utilitarian",
        CivilianDesignLanguage = "Rough outposts, mercenary bases, fighting pits",
        CivilianColorScheme = "Same brutal industrial colors",
        Architecture = "Crude fortifications, fighting arenas, mercenary camps",
        // NAUSICAAN PHYSICAL FEATURES (Predator-like, NOT Orc-like!):
        // - VERY TALL (over 2m) and lean/muscular
        // - TWO TUSKS protruding UPWARD from lower jaw (KEY FEATURE!)
        // - ELONGATED SKULL - high domed forehead sloping back
        // - DEEP-SET small eyes under heavy brow
        // - PROMINENT CHEEKBONES - angular face structure
        // - GRAY/TAN leathery skin with slight texture
        // - BALD - no hair
        // - Predator-movie aesthetic, NOT green orc
        RaceFeatures = "TALL lean humanoid (Predator-like, NOT orc!), TWO TUSKS curving UP from lower jaw, elongated high-domed skull sloping back, deep-set small eyes, prominent angular cheekbones, gray/tan leathery skin, completely bald, menacing mercenary appearance",
        ClothingDetails = "Rough leather and metal armor, practical mercenary gear, visible weapons, battle-worn equipment",
        HeraldicStyle = "Crude tribal symbols, tusk imagery, strength symbols, mercenary marks",
        MilitaryShips = GenerateShipList("Nausicaan Raider", "Nausicaan Fighter", "Nausicaan Cruiser", "Nausicaan Destroyer", "Nausicaan Battleship", "Nausicaan Carrier", "Nausicaan Interceptor", "Nausicaan Gunboat", "Nausicaan Frigate", "Nausicaan Corvette", "Nausicaan Fast Attack", "Nausicaan Boarding Ship", "Nausicaan Assault Craft", "Nausicaan Scout", "Nausicaan Patrol Craft", "Nausicaan Command Ship", "Nausicaan Heavy Raider", "Nausicaan Missile Boat", "Nausicaan Mine Layer", "Nausicaan Blockade Runner", "Nausicaan Smuggler", "Nausicaan Transport Attack", "Nausicaan Fighter Carrier", "Nausicaan Flagship", "Nausicaan Dreadnought", "Nausicaan Prison Ship", "Nausicaan Slave Ship", "Nausicaan Arena Ship", "Nausicaan Salvage Vessel", "Nausicaan Tug", "Nausicaan Shuttle", "Nausicaan Drop Ship", "Nausicaan Landing Craft", "Nausicaan Bomber", "Nausicaan Torpedo Boat", "Nausicaan Escort"),
        CivilianShips = GenerateShipList("Nausicaan Transport", "Nausicaan Freighter", "Nausicaan Cargo Ship", "Nausicaan Tanker", "Nausicaan Mining Vessel", "Nausicaan Salvage Ship", "Nausicaan Repair Vessel", "Nausicaan Supply Ship", "Nausicaan Personal Craft", "Nausicaan Shuttle", "Nausicaan Courier", "Nausicaan Passenger Ship", "Nausicaan Medical Ship", "Nausicaan Construction Ship", "Nausicaan Tug", "Nausicaan Merchant Ship"),
        MilitaryStructures = GenerateStructureList("Nausicaan Fortress", "Nausicaan Defense Platform", "Nausicaan Weapons Station", "Nausicaan Shield Generator", "Nausicaan Sensor Array", "Nausicaan Fighter Bay", "Nausicaan Command Post", "Nausicaan Patrol Base", "Nausicaan Border Outpost", "Nausicaan Prison Station", "Nausicaan Interrogation Platform", "Nausicaan Training Arena", "Nausicaan Ambush Point", "Nausicaan Mercenary Base", "Nausicaan Raider Base", "Nausicaan Communications"),
        CivilianStructures = GenerateStructureList("Nausicaan Outpost", "Nausicaan Settlement", "Nausicaan Market", "Nausicaan Cantina Station", "Nausicaan Fighting Pit", "Nausicaan Arena", "Nausicaan Shipyard", "Nausicaan Drydock", "Nausicaan Repair Station", "Nausicaan Supply Depot", "Nausicaan Mining Station", "Nausicaan Refinery", "Nausicaan Trade Post", "Nausicaan Mercenary Hall", "Nausicaan Recruitment Center", "Nausicaan Hideout"),
        Buildings = GenerateBuildingList("Nausicaan Chieftain Hall", "Nausicaan Warrior Barracks", "Nausicaan Armory", "Nausicaan Weapons Forge", "Nausicaan Fighting Pit", "Nausicaan Arena", "Nausicaan Training Ground", "Nausicaan Medical Bay", "Nausicaan Mess Hall", "Nausicaan Cantina", "Nausicaan Gambling Den", "Nausicaan Trophy Hall", "Nausicaan Prison", "Nausicaan Interrogation Room", "Nausicaan Command Center", "Nausicaan Communications", "Nausicaan Power Generator", "Nausicaan Shield Generator", "Nausicaan Defense Tower", "Nausicaan Watchtower", "Nausicaan Hangar", "Nausicaan Docking Bay", "Nausicaan Storage", "Nausicaan Engineering", "Nausicaan Reactor Room", "Nausicaan Life Support", "Nausicaan Mercenary Office", "Nausicaan Contract Hall", "Nausicaan Recruitment", "Nausicaan Slave Pen", "Nausicaan Market", "Nausicaan Black Market", "Nausicaan Fence", "Nausicaan Intelligence", "Nausicaan Ambush Point", "Nausicaan Lookout", "Nausicaan Camp", "Nausicaan Tent", "Nausicaan Fortification", "Nausicaan Wall", "Nausicaan Gate", "Nausicaan Bunker", "Nausicaan Underground", "Nausicaan Escape Route", "Nausicaan Safe House", "Nausicaan Memorial", "Nausicaan Shrine", "Nausicaan Elder Hall"),
        Troops = GenerateTroopList("Nausicaan Chieftain", "Nausicaan Warlord", "Nausicaan Warrior", "Nausicaan Raider", "Nausicaan Brute", "Nausicaan Enforcer", "Nausicaan Thug", "Nausicaan Mercenary", "Nausicaan Bodyguard", "Nausicaan Scout", "Nausicaan Sniper", "Nausicaan Heavy", "Nausicaan Pilot", "Nausicaan Engineer", "Nausicaan Medic", "Nausicaan Recruit"),
        PortraitVariants = GeneratePortraitList("Nausicaan Chieftain Male", "Nausicaan Warlord Male", "Nausicaan Warrior Male", "Nausicaan Warrior Female", "Nausicaan Raider Male", "Nausicaan Mercenary Male", "Nausicaan Mercenary Female", "Nausicaan Brute Male", "Nausicaan Elder Male", "Nausicaan Youth Male", "Nausicaan Pilot Male", "Nausicaan Engineer Male", "Nausicaan Scout Male", "Nausicaan Heavy Male", "Nausicaan Thug Male", "Nausicaan Enforcer Male"),
        HouseSymbols = GenerateSymbolList("Nausicaan Tusk Symbol", "Nausicaan Clan Mark", "Nausicaan Warrior Crest", "Nausicaan Strength Symbol", "Nausicaan Battle Mark", "Nausicaan Mercenary Badge", "Nausicaan Chieftain Seal", "Nausicaan Raider Flag", "Nausicaan Kill Count", "Nausicaan Trophy Mark", "Nausicaan Contract Symbol", "Nausicaan Alliance Mark", "Nausicaan Rivalry Symbol", "Nausicaan Territory Claim", "Nausicaan War Banner", "Nausicaan Honor Mark")
    };

    private FactionProfile CreateSpecies8472Profile() => new FactionProfile
    {
        Faction = Faction.Species8472,
        Name = "Species 8472",
        DesignLanguage = "ORGANIC BIOSHIPS - living vessels, no metal, purple/violet bioluminescence, tripod/tentacle forms",
        ColorScheme = "Deep purple, violet, bioluminescent greens, organic browns - alien and terrifying",
        CivilianDesignLanguage = "Organic structures in fluidic space, living architecture",
        CivilianColorScheme = "Purple, violet, organic greens, bioluminescent accents",
        Architecture = "Organic living structures, bioengineered environments, fluidic space realms",
        RaceFeatures = "TRIPEDAL aliens - three legs, three arms, elongated head with multiple eyes, NO MOUTH visible, communicates telepathically, gray-purple skin, extremely tall and thin",
        ClothingDetails = "No clothing - organic exoskeleton/skin, possibly bio-armor integration",
        HeraldicStyle = "Organic symbols, tripod imagery, fluidic space motifs",
        MilitaryShips = GenerateShipList("Species 8472 Bioship", "Species 8472 Dreadnought", "Species 8472 Battleship", "Species 8472 Cruiser", "Species 8472 Destroyer", "Species 8472 Frigate", "Species 8472 Fighter", "Species 8472 Scout", "Species 8472 Carrier", "Species 8472 Flagship", "Species 8472 Planet Killer", "Species 8472 Assault Ship", "Species 8472 Boarding Pod", "Species 8472 Infiltrator", "Species 8472 Stealth Ship", "Species 8472 Command Ship", "Species 8472 Heavy Bioship", "Species 8472 Light Bioship", "Species 8472 Fast Attack", "Species 8472 Patrol Craft", "Species 8472 Interceptor", "Species 8472 Bomber", "Species 8472 Torpedo Ship", "Species 8472 Mine Layer", "Species 8472 Electronic Warfare", "Species 8472 Communications", "Species 8472 Medical Ship", "Species 8472 Repair Organism", "Species 8472 Supply Ship", "Species 8472 Transport", "Species 8472 Colony Ship", "Species 8472 Invasion Ship", "Species 8472 Terraformer", "Species 8472 Gateway Ship", "Species 8472 Fluidic Rift Opener", "Species 8472 Quantum Singularity Ship"),
        CivilianShips = GenerateShipList("Species 8472 Transport Pod", "Species 8472 Cargo Organism", "Species 8472 Passenger Ship", "Species 8472 Colony Pod", "Species 8472 Medical Organism", "Species 8472 Research Vessel", "Species 8472 Survey Ship", "Species 8472 Communication Relay", "Species 8472 Shuttle Organism", "Species 8472 Personal Pod", "Species 8472 Family Unit", "Species 8472 Elder Transport", "Species 8472 Youth Pod", "Species 8472 Agricultural Ship", "Species 8472 Food Processor", "Species 8472 Water Harvester"),
        MilitaryStructures = GenerateStructureList("Species 8472 Bio-Station", "Species 8472 Defense Organism", "Species 8472 Weapons Platform", "Species 8472 Shield Generator", "Species 8472 Sensor Array", "Species 8472 Communications Hub", "Species 8472 Fighter Bay", "Species 8472 Command Center", "Species 8472 Fleet Base", "Species 8472 Repair Facility", "Species 8472 Shipyard Organism", "Species 8472 Drydock", "Species 8472 Ambush Point", "Species 8472 Patrol Station", "Species 8472 Border Outpost", "Species 8472 Gateway Station"),
        CivilianStructures = GenerateStructureList("Species 8472 Habitat", "Species 8472 Colony", "Species 8472 City Organism", "Species 8472 Research Station", "Species 8472 Medical Facility", "Species 8472 Education Center", "Species 8472 Cultural Hub", "Species 8472 Recreation Organism", "Species 8472 Agricultural Station", "Species 8472 Food Processing", "Species 8472 Trade Hub", "Species 8472 Communication Relay", "Species 8472 Transit Hub", "Species 8472 Elder Sanctuary", "Species 8472 Youth Center", "Species 8472 Fluidic Gateway"),
        Buildings = GenerateBuildingList("Species 8472 Organic Tower", "Species 8472 Living Quarters", "Species 8472 Command Organism", "Species 8472 Medical Bay", "Species 8472 Research Lab", "Species 8472 Weapons Growth", "Species 8472 Shield Organism", "Species 8472 Power Generator", "Species 8472 Communication Node", "Species 8472 Storage Organism", "Species 8472 Hangar Growth", "Species 8472 Docking Tentacle", "Species 8472 Transporter Organism", "Species 8472 Replication Chamber", "Species 8472 Training Ground", "Species 8472 Meeting Chamber", "Species 8472 Council Hall", "Species 8472 Elder Chamber", "Species 8472 Youth Nursery", "Species 8472 Recreation Pod", "Species 8472 Food Chamber", "Species 8472 Water Recycler", "Species 8472 Life Support Organism", "Species 8472 Environmental Control", "Species 8472 Security Node", "Species 8472 Defense Tower", "Species 8472 Observation Post", "Species 8472 Sensor Node", "Species 8472 Early Warning", "Species 8472 Ambush Chamber", "Species 8472 Prison Organism", "Species 8472 Interrogation Chamber", "Species 8472 Intelligence Hub", "Species 8472 Infiltration Center", "Species 8472 Genetic Lab", "Species 8472 Evolution Chamber", "Species 8472 Bioweapon Lab", "Species 8472 Virus Chamber", "Species 8472 Telepathy Amplifier", "Species 8472 Mind Link Hub", "Species 8472 Gateway Generator", "Species 8472 Fluidic Rift", "Species 8472 Quantum Lab", "Species 8472 Dimensional Research", "Species 8472 Memorial Growth", "Species 8472 Sacred Grove", "Species 8472 Ancient Organism", "Species 8472 Progenitor Chamber"),
        Troops = GenerateTroopList("Species 8472 Warrior", "Species 8472 Elite", "Species 8472 Commander", "Species 8472 Infiltrator", "Species 8472 Scout", "Species 8472 Heavy Warrior", "Species 8472 Psionic", "Species 8472 Medic", "Species 8472 Engineer", "Species 8472 Pilot", "Species 8472 Scientist", "Species 8472 Elder", "Species 8472 Youth", "Species 8472 Guardian", "Species 8472 Assassin", "Species 8472 Leader"),
        PortraitVariants = GeneratePortraitList("Species 8472 Warrior", "Species 8472 Commander", "Species 8472 Elder", "Species 8472 Scientist", "Species 8472 Pilot", "Species 8472 Infiltrator", "Species 8472 Guardian", "Species 8472 Youth", "Species 8472 Psionic Master", "Species 8472 War Leader", "Species 8472 Ambassador", "Species 8472 Scout", "Species 8472 Heavy Warrior", "Species 8472 Medical", "Species 8472 Engineer", "Species 8472 Ancient One"),
        HouseSymbols = GenerateSymbolList("Species 8472 Tripod Symbol", "Species 8472 Bioship Mark", "Species 8472 Fluidic Space", "Species 8472 Eye Symbol", "Species 8472 Tentacle Mark", "Species 8472 Organic Pattern", "Species 8472 War Mark", "Species 8472 Peace Symbol", "Species 8472 Territory Claim", "Species 8472 Gateway Mark", "Species 8472 Elder Council", "Species 8472 Warrior Caste", "Species 8472 Science Caste", "Species 8472 Pilot Caste", "Species 8472 Guardian Mark", "Species 8472 Ancient Symbol")
    };

    // =================================================================
    // SPECIAL FACTIONS - Portrait/Event only (no ships, buildings, etc.)
    // =================================================================
    
    private FactionProfile CreateSpecialProfile() => new FactionProfile
    {
        Faction = Faction.Special,
        Name = "Universal Assets",
        DesignLanguage = "Various - faction-independent assets",
        ColorScheme = "Varies by asset type",
        Architecture = "N/A",
        RaceFeatures = "N/A",
        ClothingDetails = "N/A",
        HeraldicStyle = "N/A",
        IsPortraitOnly = false,
        HasShips = false,
        HasBuildings = false,
        HasTroops = false,
        
        // EventCharacters - Generic aliens for random events (36 total)
        EventCharacters = new List<string>
        {
            // Traders & Merchants (6)
            "Alien Trader Male Friendly",
            "Alien Trader Female Shrewd",
            "Alien Merchant Exotic Goods",
            "Alien Black Market Dealer Shady",
            "Alien Smuggler Nervous",
            "Alien Auctioneer Wealthy",
            
            // Diplomats & Officials (6)
            "Alien Diplomat Male Formal",
            "Alien Diplomat Female Elegant",
            "Alien Ambassador Regal",
            "Alien Bureaucrat Tired",
            "Alien Negotiator Cunning",
            "Alien Senator Corrupt",
            
            // Scientists & Explorers (6)
            "Alien Scientist Curious",
            "Alien Researcher Obsessed",
            "Alien Explorer Weathered",
            "Alien Archaeologist Excited",
            "Alien Biologist Fascinated",
            "Alien Engineer Practical",
            
            // Refugees & Civilians (6)
            "Alien Refugee Desperate",
            "Alien Refugee Family Leader",
            "Alien Civilian Worker",
            "Alien Farmer Simple",
            "Alien Artist Eccentric",
            "Alien Child Innocent",
            
            // Criminals & Pirates (6)
            "Alien Pirate Captain Scarred",
            "Alien Pirate Crew Rough",
            "Alien Thief Sneaky",
            "Alien Assassin Cold",
            "Alien Bounty Hunter Intimidating",
            "Alien Crime Boss Menacing",
            
            // Mystics & Unusual (6)
            "Alien Mystic Prophet",
            "Alien Healer Gentle",
            "Alien Telepath Intense",
            "Alien Elder Ancient Wise",
            "Alien Beggar Mysterious",
            "Alien Hermit Strange"
        },
        
        // Empty lists for unsupported categories
        MilitaryShips = new List<string>(),
        CivilianShips = new List<string>(),
        MilitaryStructures = new List<string>(),
        CivilianStructures = new List<string>(),
        Buildings = new List<string>(),
        Troops = new List<string>(),
        PortraitVariants = new List<string>(),
        HouseSymbols = new List<string>()
    };
    
    private FactionProfile CreateAncientRacesProfile() => new FactionProfile
    {
        Faction = Faction.AncientRaces,
        Name = "Ancient Races",
        DesignLanguage = "Ancient, powerful, mysterious technology far beyond current civilizations",
        ColorScheme = "Ethereal glows, ancient metals, cosmic colors",
        Architecture = "N/A - portrait only faction",
        RaceFeatures = "Various ancient species - Iconians, Preservers, T'Kon, etc.",
        ClothingDetails = "Ancient robes, mysterious armor, technology-integrated clothing",
        HeraldicStyle = "N/A",
        IsPortraitOnly = true,
        HasShips = false,
        HasBuildings = false,
        HasTroops = false,
        
        EventCharacters = new List<string>
        {
            // Iconians
            "Iconian Male Ancient", "Iconian Female Ancient", "Iconian Holographic Message",
            "Iconian Gateway Keeper", "Iconian Commander",
            
            // Preservers
            "Preserver Ancient Being", "Preserver Hologram Message", "Preserver Caretaker",
            
            // T'Kon Empire
            "T'Kon Empire Guardian", "T'Kon Empire Warrior", "T'Kon Empire Scientist",
            "T'Kon Portal Guardian",
            
            // Progenitors (humanoid seeders)
            "Progenitor Holographic Message", "Progenitor Ancient Form",
            
            // Tkon remnants
            "Portal Guardian Entity",
            
            // Metrons
            "Metron Arbiter", "Metron Observer",
            
            // Organians
            "Organian Council Member", "Organian Energy Form",
            
            // Excalbians
            "Excalbian Shape Shifter", "Excalbian Rock Form",
            
            // Other ancients
            "Hur'q Ancient Warrior", "Hur'q Swarm Leader",
            "Sphere Builder", "Temporal Cold War Agent",
            "First Federation Balok", "First Federation Crew",
            "Sheliak Corporate Entity",
            
            // Mysterious entities
            "Crystalline Entity Avatar", "Dikironium Cloud Creature Form",
            "Space Jellyfish Entity", "Whale Probe Creator",
            "V'Ger Entity Form", "Nomad Probe Creator"
        },
        
        MilitaryShips = new List<string>(),
        CivilianShips = new List<string>(),
        MilitaryStructures = new List<string>(),
        CivilianStructures = new List<string>(),
        Buildings = new List<string>(),
        Troops = new List<string>(),
        PortraitVariants = new List<string>(),
        HouseSymbols = new List<string>()
    };
    
    // ═══════════════════════════════════════════════════════════════════════════
    // VEHICLES ASSET LIST AND PROMPT BUILDER
    // ═══════════════════════════════════════════════════════════════════════════
    
    private List<string> GetVehiclesList(Faction faction)
    {
        return faction switch
        {
            Faction.Federation => new List<string>
            {
                // Combat (4)
                "Argo Buggy", "Federation Tank", "Phaser Turret Mobile", "Shield Generator Mobile",
                // Transport (4)
                "APC Troop Carrier", "Cargo Walker", "Medical Evac Vehicle", "Command Vehicle",
                // Exosuits (4)
                "MACO Exosuit", "Heavy Assault Exosuit", "Engineering Exosuit", "Recon Exosuit",
                // Support (4)
                "Hover Bike Scout", "Workbee Ground", "Tricorder Drone", "Defense Drone"
            },
            Faction.Klingon => new List<string>
            {
                // Combat (4)
                "Klingon Battle Tank", "Disruptor Cannon Mobile", "Siege Platform", "Assault Walker",
                // Transport (4)
                "Warrior Transport", "Targ War Chariot", "Honor Guard Vehicle", "Command Cruiser Ground",
                // Exosuits (4)
                "Warrior Exosuit", "Berserker Armor", "Heavy Assault Exo", "Dahar Master Armor",
                // Support (4)
                "Speeder Bike", "Supply Walker", "Combat Drone", "Targ Handler Vehicle"
            },
            Faction.Romulan => new List<string>
            {
                // Combat (4)
                "Romulan Hover Tank", "Plasma Cannon Mobile", "Cloaked Scout Vehicle", "Assault Walker",
                // Transport (4)
                "Centurion Transport", "Tal Shiar Covert Vehicle", "Senate Guard Transport", "Command Vehicle",
                // Exosuits (4)
                "Centurion Exosuit", "Reman Assault Armor", "Infiltrator Suit", "Heavy Combat Exo",
                // Support (4)
                "Scout Speeder", "Engineering Walker", "Spy Drone", "Shield Drone"
            },
            Faction.Cardassian => new List<string>
            {
                // Combat (4)
                "Cardassian Tank", "Disruptor Platform Mobile", "Occupation Walker", "Siege Vehicle",
                // Transport (4)
                "Troop Carrier", "Prison Transport", "Obsidian Order Vehicle", "Gul Command Vehicle",
                // Exosuits (4)
                "Soldier Exosuit", "Interrogator Armor", "Heavy Assault Exo", "Labor Overseer Mech",
                // Support (4)
                "Patrol Speeder", "Mining Walker", "Surveillance Drone", "Guard Drone"
            },
            Faction.Borg => new List<string>
            {
                // Combat (4)
                "Borg Assault Walker", "Cutting Beam Platform", "Adaptation Node Mobile", "Heavy Assimilator",
                // Transport (4)
                "Drone Transport Cube", "Assimilation Vehicle", "Queen Transport", "Collective Node Mobile",
                // Exosuits (4)
                "Tactical Drone Exo", "Heavy Assault Drone", "Medical Drone Walker", "Engineering Drone Mech",
                // Support (4)
                "Scout Probe Ground", "Repair Drone Mobile", "Shield Node Mobile", "Vinculum Drone"
            },
            Faction.Dominion => new List<string>
            {
                // Combat (4)
                "Jem'Hadar Tank", "Polaron Cannon Mobile", "Assault Beetle", "Siege Walker",
                // Transport (4)
                "Vorta Command Vehicle", "Troop Beetle", "Ketracel Distribution", "Founder Transport",
                // Exosuits (4)
                "Jem'Hadar Exosuit", "First Assault Armor", "Shroud Suit Enhanced", "Heavy Combat Armor",
                // Support (4)
                "Scout Speeder", "Supply Beetle", "Attack Drone", "Shield Drone"
            },
            Faction.Ferengi => new List<string>
            {
                // Combat (4)
                "Ferengi Defense Tank", "Profit Protector", "Vault Guard Mech", "Mercenary Walker",
                // Transport (4)
                "Luxury Transport", "Cargo Hauler", "DaiMon Vehicle", "FCA Enforcement",
                // Exosuits (4)
                "Security Exosuit", "Liquidator Armor", "Bodyguard Mech", "Executive Protection Suit",
                // Support (4)
                "Trade Speeder", "Latinum Drone", "Spy Probe", "Servant Bot"
            },
            _ => new List<string>
            {
                // Generic vehicles for other factions
                "Combat Tank", "Mobile Cannon", "Assault Walker", "Defense Platform Mobile",
                "Troop Transport", "Cargo Vehicle", "Command Vehicle", "Medical Transport",
                "Combat Exosuit", "Heavy Exosuit", "Scout Exosuit", "Support Exosuit",
                "Scout Speeder", "Supply Walker", "Combat Drone", "Support Drone"
            }
        };
    }
    
    private string BuildVehiclePrompt(FactionProfile profile, string vehicleName)
    {
        var vehicleType = GetVehicleType(vehicleName);
        var realWorldReference = GetVehicleRealWorldReference(vehicleName);
        
        return $@"MANDATORY STYLE GUIDE:
Material & Texture: Handmade physical model made of matte plasticine clay. Rich surface details: panel lines, rivets, hatches, vents, armor plates, mechanical joints. Non-glossy soft clay finish. High-end stop-motion quality (Laika/Aardman).
Detail Level: HIGH DETAIL - visible mechanical components, armor segments, weapon mounts, sensor equipment, exhaust ports, running gear. Same detail density as a military vehicle model kit.
Proportions: Chunky, solid, GROUNDED. Low to the ground. Wide stance. Heavy and armored appearance.
Lighting: Dramatic studio lighting from upper left, soft shadows, rim lighting to show surface details.
Composition: Vehicle centered, isometric 3/4 view, facing slightly right.
Background: Solid black background (#000000).

{vehicleType}

{realWorldReference}

CRITICAL - THIS IS A GROUND VEHICLE:
- MUST have visible ground contact: wheels, tracks, legs, or hover pads close to ground
- MUST look like it drives/walks on PLANETARY SURFACE
- NO saucer sections, NO warp nacelles, NO deflector dishes
- NO starship hull shapes - this is NOT a spaceship
- Think: military tank, APC, construction vehicle, walking mech - NOT shuttle or starship
- Reference: Real military vehicles (tanks, APCs, mechs from games) but with {profile.Name} faction styling

FACTION STYLING:
{profile.Name} aesthetic applied to ground vehicle design
Color Scheme: {profile.ColorScheme}

Subject: {profile.Name} {vehicleName}

**single ground combat vehicle, highly detailed clay model, military vehicle aesthetic, game asset, isometric view**
--no spaceship, no starship, no shuttle, no saucer, no nacelles, no deflector, no space, no flying, no wings, smooth surfaces, low detail, funny, thumbprints, blurry, pilot visible";
    }
    
    private string GetVehicleRealWorldReference(string vehicleName)
    {
        var lowerName = vehicleName.ToLower();
        
        if (lowerName.Contains("tank"))
            return "REFERENCE: Like a futuristic M1 Abrams or Leopard 2 tank - tracked, turret with main gun, heavy armor plates, low profile.";
        if (lowerName.Contains("apc") || lowerName.Contains("carrier"))
            return "REFERENCE: Like a futuristic Bradley IFV or BTR - wheeled or tracked, troop compartment, firing ports, ramp door.";
        if (lowerName.Contains("exo") || lowerName.Contains("armor") || lowerName.Contains("suit"))
            return "REFERENCE: Like power armor from Fallout or Space Marine armor - humanoid mech suit, thick plating, integrated weapons.";
        if (lowerName.Contains("walker") || lowerName.Contains("mech"))
            return "REFERENCE: Like an AT-ST or MechWarrior mech - walking legs, cockpit, arm weapons, gyro stabilizers visible.";
        if (lowerName.Contains("bike") || lowerName.Contains("speeder"))
            return "REFERENCE: Like a futuristic military motorcycle or Star Wars speeder bike - sleek, fast, single rider, handlebars or controls.";
        if (lowerName.Contains("turret") || lowerName.Contains("cannon") || lowerName.Contains("platform"))
            return "REFERENCE: Like mobile artillery - weapon platform on wheels/tracks, stabilizer legs, targeting equipment.";
        if (lowerName.Contains("drone"))
            return "REFERENCE: Like a military ground drone - small, tracked or wheeled, sensors, compact, unmanned.";
        if (lowerName.Contains("buggy"))
            return "REFERENCE: Like a military dune buggy or MRAP - open frame, big wheels, roll cage, mounted weapon.";
        if (lowerName.Contains("beetle") || lowerName.Contains("insect"))
            return "REFERENCE: Organic insect-like vehicle - multiple legs, carapace armor, mandibles or organic weapons.";
            
        return "REFERENCE: Futuristic military ground vehicle - armored, practical, combat-ready appearance.";
    }
    
    private string GetVehicleType(string vehicleName)
    {
        var lowerName = vehicleName.ToLower();
        
        if (lowerName.Contains("tank"))
            return "VEHICLE TYPE: Heavy Tank. Armored, tracked or hover, main cannon turret. Slow but powerful. Military combat vehicle.";
        if (lowerName.Contains("apc") || lowerName.Contains("transport") || lowerName.Contains("carrier"))
            return "VEHICLE TYPE: Troop Transport/APC. Armored personnel carrier. Room for soldiers. Protected transport.";
        if (lowerName.Contains("exo") || lowerName.Contains("armor") || lowerName.Contains("suit"))
            return "VEHICLE TYPE: Exosuit/Power Armor. Wearable mech suit. Humanoid shape, enhanced abilities. Pilot inside.";
        if (lowerName.Contains("walker") || lowerName.Contains("mech"))
            return "VEHICLE TYPE: Walker/Mech. Legged vehicle, bipedal or multi-legged. Versatile terrain capability.";
        if (lowerName.Contains("bike") || lowerName.Contains("speeder"))
            return "VEHICLE TYPE: Speeder/Hoverbike. Fast reconnaissance vehicle. Light, agile, single pilot.";
        if (lowerName.Contains("turret") || lowerName.Contains("cannon") || lowerName.Contains("platform"))
            return "VEHICLE TYPE: Mobile Weapons Platform. Moveable heavy weapon emplacement. Artillery or defense.";
        if (lowerName.Contains("drone") || lowerName.Contains("probe"))
            return "VEHICLE TYPE: Drone/Probe. Unmanned ground vehicle. Automated, various functions.";
        if (lowerName.Contains("buggy") || lowerName.Contains("car") || lowerName.Contains("vehicle"))
            return "VEHICLE TYPE: Light Vehicle. Fast, wheeled or hover. Utility or reconnaissance.";
        if (lowerName.Contains("beetle") || lowerName.Contains("insect"))
            return "VEHICLE TYPE: Organic/Bio Vehicle. Grown rather than built. Insectoid or organic appearance.";
        
        return "VEHICLE TYPE: Military ground vehicle. Appropriate design for faction aesthetic.";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // NEW CATEGORY ASSET LISTS
    // ═══════════════════════════════════════════════════════════════════════════
    
    private List<string> GetUIElementsList(Faction faction)
    {
        // Faction-specific UI elements with LCARS-style naming for Federation
        return faction switch
        {
            Faction.Federation => new List<string>
            {
                // LCARS Buttons (6)
                "LCARS Button Pill Orange", "LCARS Button Pill Blue", "LCARS Button Pill Purple", 
                "LCARS Button Rectangle Beige", "LCARS Button Small Square", "LCARS Button Rounded End",
                // LCARS Panels (6)
                "LCARS Panel Frame Horizontal", "LCARS Panel Frame Vertical", "LCARS Corner Bracket Left",
                "LCARS Corner Bracket Right", "LCARS Header Bar", "LCARS Footer Bar",
                // LCARS Progress Bars - EMPTY (6)
                "LCARS Progress Bar Empty Horizontal", "LCARS Progress Bar Empty Vertical", 
                "LCARS Meter Empty Arc", "LCARS Gauge Empty Circle", "LCARS Bar Track Only", "LCARS Slider Track Empty",
                // LCARS Decorative (6)
                "LCARS Elbow Left Orange", "LCARS Elbow Right Blue", "LCARS Divider Horizontal",
                "LCARS Divider Vertical", "LCARS Cap End Left", "LCARS Cap End Right",
                // LCARS Window Elements (6)
                "LCARS Window Frame", "LCARS Dialog Border", "LCARS Tooltip Frame",
                "LCARS Alert Box Frame", "LCARS Info Panel", "LCARS Notification Badge",
                // LCARS Misc (6)
                "LCARS Checkbox Frame Empty", "LCARS Radio Button Frame Empty", "LCARS Toggle Track",
                "LCARS Tab Active", "LCARS Tab Inactive", "LCARS Number Display Frame"
            },
            _ => new List<string>
            {
                // Generic UI elements for other factions
                "Button Primary", "Button Secondary", "Button Accent", "Button Small", "Button Wide", "Button Round",
                "Panel Frame Horizontal", "Panel Frame Vertical", "Corner Bracket Left", "Corner Bracket Right", "Header Bar", "Footer Bar",
                "Progress Bar Empty Horizontal", "Progress Bar Empty Vertical", "Meter Empty", "Gauge Empty", "Bar Track", "Slider Track",
                "Decorative Border Left", "Decorative Border Right", "Divider Horizontal", "Divider Vertical", "End Cap Left", "End Cap Right",
                "Window Frame", "Dialog Border", "Tooltip Frame", "Alert Box", "Info Panel", "Notification Badge",
                "Checkbox Frame Empty", "Radio Frame Empty", "Toggle Track", "Tab Active", "Tab Inactive", "Display Frame"
            }
        };
    }
    
    private string BuildUIElementPrompt(FactionProfile profile, string elementName)
    {
        var factionStyle = GetFactionUIStyle(profile.Faction);
        var elementDetails = GetUIElementDetails(elementName, profile.Faction);
        
        // Special handling for Federation LCARS
        if (profile.Faction == Faction.Federation && elementName.ToLower().Contains("lcars"))
        {
            return BuildLCARSPrompt(elementName);
        }
        
        return $@"MANDATORY STYLE GUIDE:
View: FLAT, 2D, FRONT-FACING view - as if looking at a computer screen straight-on. NO 3D perspective.
Material: Clean, smooth digital render. Polished surfaces like glass or backlit plastic.
Composition: Element centered, perfectly horizontal/vertical alignment.
Background: Solid black background (#000000).

{factionStyle}

{elementDetails}

Subject: {profile.Name} UI element - {elementName}

**single UI element, flat 2D view, front-facing, sharp geometric edges, game interface asset**
--no 3D, no perspective, no tilt, no clay, no soft edges, no text, no numbers";
    }
    
    private string BuildLCARSPrompt(string elementName)
    {
        var lowerName = elementName.ToLower();
        var elementType = GetLCARSElementType(lowerName);
        
        return $@"STAR TREK LCARS INTERFACE ELEMENT

CRITICAL REFERENCE: This must look EXACTLY like the LCARS computer interface from Star Trek: The Next Generation, Deep Space Nine, and Voyager.

LCARS DESIGN RULES (MUST FOLLOW):
1. COLORS: Orange (#FF9900), Periwinkle Blue (#9999FF), Lavender (#CC99CC), Tan/Beige (#FFCC99), Pale Blue (#99CCFF)
2. SHAPES: Simple geometric - rounded rectangles, pill shapes (capsules), quarter-circle elbows
3. STYLE: FLAT solid colors - NO gradients inside shapes, NO 3D shading, NO bevels
4. EDGES: Rounded corners ONLY - no sharp 90-degree corners anywhere
5. BACKGROUND: Black background, elements appear backlit/glowing

{elementType}

VIEW: Perfectly FLAT, 2D, front-on view - like a screenshot from a Star Trek computer screen
LOOK: Clean vector-graphic style, solid flat colors, slight outer glow suggesting backlit display

CRITICAL - WHAT LCARS IS NOT:
- NOT 3D rendered buttons with depth
- NOT glossy/shiny surfaces
- NOT complex multi-layered frames
- NOT realistic metal or plastic
- LCARS is FLAT, SIMPLE, GRAPHIC

**LCARS interface element, flat 2D vector style, solid colors, backlit appearance, Star Trek TNG style**
--no 3D, no depth, no gradients inside shapes, no realistic materials, no complex frames, no bevels, no shadows on element";
    }
    
    private string GetLCARSElementType(string lowerName)
    {
        if (lowerName.Contains("button") && lowerName.Contains("pill"))
            return @"ELEMENT: LCARS PILL BUTTON
- Simple horizontal capsule/pill shape (rectangle with semicircle ends)
- SOLID single color fill (orange, blue, or purple)
- NO border, NO inner details - just the solid colored shape
- Typical size ratio: 4:1 width to height
- Looks like a colored lozenge/pill";

        if (lowerName.Contains("button") && lowerName.Contains("rectangle"))
            return @"ELEMENT: LCARS RECTANGLE BUTTON  
- Rounded rectangle shape
- SOLID single color fill (usually tan/beige)
- All corners rounded (radius about 20% of height)
- NO border, NO inner frame";

        if (lowerName.Contains("elbow"))
        {
            var color = lowerName.Contains("orange") ? "orange (#FF9900)" : "blue (#9999FF)";
            var direction = lowerName.Contains("left") ? "LEFT" : "RIGHT";
            return $@"ELEMENT: LCARS ELBOW/CORNER ({direction})
- L-shaped corner piece
- One end is rounded (semicircle cap)  
- Other end is flat (connects to straight bar)
- SOLID {color} fill
- Creates the characteristic LCARS curved corner transitions
- Like a bent pipe or elbow joint
- The curved part is a quarter circle";
        }

        if (lowerName.Contains("progress") || lowerName.Contains("bar empty") || lowerName.Contains("track"))
            return @"ELEMENT: LCARS BAR TRACK (EMPTY)
- Horizontal pill/capsule shape outline
- EMPTY inside (black/dark interior)
- Thin colored border/frame only
- This is the TRACK that gets filled - show it EMPTY
- Fill will be added dynamically";

        if (lowerName.Contains("header") || lowerName.Contains("footer"))
            return @"ELEMENT: LCARS HEADER/FOOTER BAR
- Long horizontal bar
- Multiple COLOR BLOCKS side by side (orange, blue, purple, tan)
- Each block is a rounded rectangle butting against the next
- Rounded caps on the ends only
- Like a segmented candy bar";

        if (lowerName.Contains("divider"))
            return @"ELEMENT: LCARS DIVIDER
- Simple thin horizontal or vertical bar
- Solid single color
- Rounded ends
- Used to separate content areas";

        if (lowerName.Contains("panel") || lowerName.Contains("frame"))
            return @"ELEMENT: LCARS PANEL FRAME
- NOT a closed rectangle frame!
- Made of separate bars and elbows
- Typically: top bar + left elbow + left bar (open on right/bottom)
- Or configured as needed
- Interior is BLACK for content
- Colored bars frame 1-3 sides only";

        if (lowerName.Contains("cap") || lowerName.Contains("end"))
            return @"ELEMENT: LCARS END CAP
- Semicircle shape
- SOLID single color
- Used to terminate/cap the end of a bar
- Half of a pill shape";

        if (lowerName.Contains("window") || lowerName.Contains("dialog"))
            return @"ELEMENT: LCARS WINDOW/DIALOG
- Asymmetric frame using bars and elbows
- Usually framed on LEFT and TOP only
- Right and bottom are OPEN or minimal
- Large BLACK interior area for content
- Color bars: orange/blue/purple combination";

        if (lowerName.Contains("checkbox") || lowerName.Contains("radio") || lowerName.Contains("toggle"))
            return @"ELEMENT: LCARS CONTROL (EMPTY STATE)
- Small rounded rectangle or circle outline
- Thin colored border
- EMPTY interior (for unchecked state)
- Simple, minimal design";

        if (lowerName.Contains("tab"))
            return @"ELEMENT: LCARS TAB
- Rounded rectangle, taller than wide
- SOLID color fill
- Active: bright color, Inactive: darker/muted
- Rounded corners on all sides";

        return @"ELEMENT: LCARS INTERFACE COMPONENT
- Simple geometric shape
- SOLID flat color (orange, blue, purple, tan, or pale blue)
- Rounded corners
- Clean, minimal, graphic design";
    }
    
    private string GetUIElementDetails(string elementName, Faction faction)
    {
        var lowerName = elementName.ToLower();
        
        // LCARS-specific elements (Federation)
        if (lowerName.Contains("lcars"))
        {
            if (lowerName.Contains("button") && lowerName.Contains("pill"))
                return @"LCARS BUTTON (Pill Shape):
- Horizontal pill/capsule shape with rounded ends
- Solid color fill (orange, blue, purple, or beige depending on variant)
- FLAT 2D rectangle with semicircle caps on left and right ends
- Characteristic LCARS color blocking
- Clean, solid color - no gradients inside
- Subtle outer glow or backlit effect";
            
            if (lowerName.Contains("button") && lowerName.Contains("rectangle"))
                return @"LCARS BUTTON (Rectangle):
- Simple rounded rectangle shape
- Solid beige/tan color typical of LCARS
- Slightly rounded corners
- FLAT 2D view, no perspective";
            
            if (lowerName.Contains("elbow"))
                return @"LCARS ELBOW/CORNER PIECE:
- L-shaped corner piece connecting horizontal and vertical bars
- One end is rounded (pill cap), other end is flat (connects to straight bar)
- Solid single color (orange or blue)
- Creates the characteristic LCARS curved corner transitions
- FLAT 2D, no perspective";
            
            if (lowerName.Contains("progress") || lowerName.Contains("bar") || lowerName.Contains("meter") || lowerName.Contains("gauge") || lowerName.Contains("track"))
                return @"LCARS PROGRESS BAR / METER (EMPTY):
- Show ONLY the track/frame - NO fill, NO progress indicator
- Empty state at 0% - just the outline/container
- Horizontal or vertical bar shape
- Rounded ends typical of LCARS
- Border/frame visible, interior empty or very dark
- Fill will be added dynamically in-game";
            
            if (lowerName.Contains("panel") || lowerName.Contains("frame"))
                return @"LCARS PANEL FRAME:
- Characteristic LCARS bordered panel
- Rounded corners with color-blocked sections
- Multiple color bands (orange, blue, purple, beige)
- FLAT 2D, front-facing view
- Interior area for content (darker or black)";
            
            if (lowerName.Contains("divider"))
                return @"LCARS DIVIDER LINE:
- Simple horizontal or vertical bar
- Solid LCARS color
- Rounded or flat ends
- Used to separate content sections";
            
            if (lowerName.Contains("header") || lowerName.Contains("footer"))
                return @"LCARS HEADER/FOOTER BAR:
- Horizontal bar spanning width
- Multiple color-blocked sections
- Rounded end caps
- Characteristic LCARS stripe pattern";
                
            if (lowerName.Contains("cap") || lowerName.Contains("end"))
                return @"LCARS END CAP:
- Semicircular cap piece for ending a bar
- Half-pill shape
- Solid LCARS color
- Used to terminate horizontal or vertical bars";
                
            if (lowerName.Contains("window") || lowerName.Contains("dialog"))
                return @"LCARS WINDOW FRAME:
- Complete window border with LCARS styling
- Color-blocked corners and edges
- Interior area is dark/black for content
- Rounded corners with pill-shaped accent bars";
        }
        
        // Generic progress bars for any faction
        if (lowerName.Contains("progress") || lowerName.Contains("bar") || lowerName.Contains("meter") || 
            lowerName.Contains("gauge") || lowerName.Contains("track") || lowerName.Contains("slider"))
        {
            return @"PROGRESS BAR / METER (EMPTY STATE):
- Show ONLY the track/frame/outline
- EMPTY - no fill, no progress indicator, 0% state
- Just the container that will hold the progress
- Fill color/amount will be added dynamically in-game
- Clean geometric shape appropriate for faction style";
        }
        
        if (lowerName.Contains("button"))
            return "BUTTON: Clickable button shape appropriate for faction style. Solid color, clear edges, flat 2D view.";
            
        if (lowerName.Contains("panel") || lowerName.Contains("frame") || lowerName.Contains("window"))
            return "PANEL/FRAME: Container frame for content. Clear border, interior area for content, faction-appropriate styling.";
            
        if (lowerName.Contains("checkbox") || lowerName.Contains("radio") || lowerName.Contains("toggle"))
            return "CHECKBOX/RADIO/TOGGLE: Input control frame - show EMPTY/unchecked state. Just the frame, no checkmark or fill.";
            
        if (lowerName.Contains("tab"))
            return "TAB: Tab button for switching views. Rectangular with one rounded edge. Active vs inactive visual difference.";
        
        return "UI ELEMENT: Clean geometric shape appropriate for faction interface style. Flat 2D view, sharp edges.";
    }
    
    private string GetFactionUIStyle(Faction faction)
    {
        return faction switch
        {
            Faction.Federation => @"FEDERATION LCARS INTERFACE STYLE:
Reference: Star Trek TNG/DS9/Voyager computer interfaces

LCARS DESIGN RULES:
- Color palette: Orange (#FF9900), Blue (#99CCFF), Purple (#CC99FF), Beige/Tan (#FFCC99), Black backgrounds
- Shapes: Rounded rectangles, pill shapes (rectangle with semicircle ends), L-shaped elbows
- All corners are ROUNDED - no sharp 90-degree corners
- Color blocking - solid flat colors, no gradients within shapes
- Bars connect via elbow pieces at corners
- Text areas are black/dark with colored borders
- Horizontal and vertical orientation only - no diagonal elements
- Characteristic 'pill-button' shapes with fully rounded ends
- Backlit/glowing appearance - colors seem illuminated from behind",

            Faction.Klingon => @"KLINGON WARRIOR INTERFACE STYLE:
- Angular, aggressive geometric shapes
- Sharp points and blade-like edges
- Colors: Dark red (#8B0000), Bronze/Gold (#CD7F32), Black, Dark grey
- Metal plate texture appearance
- Rivets and panel line details
- Bat'leth curve influences
- Harsh, militaristic design
- NO soft curves - everything angular",

            Faction.Romulan => @"ROMULAN IMPERIAL INTERFACE STYLE:
- Elegant angular shapes with bird-wing motifs
- Colors: Green (#006400), Silver (#C0C0C0), Dark grey, Black
- Sharp but sophisticated edges
- Talon/claw decorative elements
- Roman-inspired geometric patterns
- Layered, secretive design aesthetic
- Subtle gradients allowed",

            Faction.Cardassian => @"CARDASSIAN INTERFACE STYLE:
- Oval and curved rectangular shapes
- Colors: Brown (#8B4513), Tan (#D2B48C), Ochre (#CC7722), Dark grey
- Ribbed/segmented panel textures
- Surveillance-state aesthetic
- Industrial, functional design
- Curved but utilitarian",

            Faction.Borg => @"BORG COLLECTIVE INTERFACE STYLE:
- Geometric shapes - hexagons, angular grids
- Colors: Dark grey (#2F4F4F), Green glow (#00FF00), Black
- Circuit board patterns
- Exposed conduit/wire details
- No aesthetic consideration - pure function
- Harsh, technological, cold
- Grid patterns and data streams",

            Faction.Dominion => @"DOMINION INTERFACE STYLE:
- Organic, flowing curved shapes
- Colors: Purple (#800080), Violet (#EE82EE), Bioluminescent accents
- Beetle/insect carapace influences
- Grown rather than manufactured appearance
- Alien biotech aesthetic
- Smooth organic curves",

            Faction.Ferengi => @"FERENGI COMMERCE INTERFACE STYLE:
- Ornate, decorative shapes
- Colors: Gold (#FFD700), Orange (#FFA500), Copper (#B87333), Bronze
- Ear-lobe inspired curved elements
- Flashy, commercial, ostentatious
- Wealth-displaying decorations
- Excessive ornamentation",

            _ => $@"{faction.ToString().ToUpper()} INTERFACE STYLE:
- Faction-appropriate shapes and colors
- Unique cultural aesthetic elements
- Consistent with faction visual identity
- Clean geometric shapes"
        };
    }
    
    private string BuildUIIconPrompt(FactionProfile profile, string iconName)
    {
        var factionStyle = GetFactionUIStyle(profile.Faction);
        
        return $@"MANDATORY STYLE GUIDE:
View: FLAT, 2D, FRONT-FACING view. NO 3D perspective, NO tilted angle.
Material: Clean, polished digital render. Simple bold shapes.
Finish: Semi-glossy or matte, precise geometry. Modern app icon quality.
Proportions: Simple, bold shapes that read well at small sizes (32x32 to 64x64 pixels).
Lighting: Flat, even lighting. Minimal shadows.
Composition: Icon centered, simple clear silhouette.
Background: Solid black background (#000000).

{factionStyle}

Subject: {profile.Name} faction icon - {iconName}
Style: Clean, modern game icon matching {profile.Name} aesthetic
Color Scheme: {profile.ColorScheme}

CRITICAL: This is a SMALL GAME ICON.
- MUST be flat 2D, front-facing view
- MUST be simple and instantly recognizable
- MUST have clean, sharp edges
- MUST read well at small sizes
- NO 3D perspective, NO clay, NO soft surfaces
- Think: Civilization icons, Stellaris resource icons, modern game UI

**single game icon, flat 2D, front-facing, sharp edges, simple bold shape, readable at small size**
--no 3D, no perspective, no clay, no soft edges, no text, no labels, complex details, blurry";
    }
    
    private List<string> GetUIIconsList(Faction faction)
    {
        return new List<string>
        {
            // Resources (8)
            "Icon Energy", "Icon Dilithium", "Icon Credits", "Icon Food", "Icon Materials", "Icon Research", "Icon Influence", "Icon Population",
            // Actions (8)
            "Icon Attack", "Icon Defend", "Icon Move", "Icon Build", "Icon Repair", "Icon Trade", "Icon Diplomacy", "Icon Espionage",
            // Status (8)
            "Icon Health", "Icon Shield", "Icon Armor", "Icon Speed", "Icon Range", "Icon Damage", "Icon Accuracy", "Icon Evasion",
            // Navigation (8)
            "Icon Home", "Icon Map", "Icon Fleet", "Icon Planet", "Icon Station", "Icon Research Lab", "Icon Shipyard", "Icon Settings",
            // Alerts (8)
            "Icon Warning", "Icon Error", "Icon Success", "Icon Info", "Icon Question", "Icon Lock", "Icon Unlock", "Icon Star",
            // Misc (8)
            "Icon Plus", "Icon Minus", "Icon Close", "Icon Menu", "Icon Arrow Up", "Icon Arrow Down", "Icon Arrow Left", "Icon Arrow Right"
        };
    }
    
    private List<string> GetPlanetsList()
    {
        // Try to load from JSON first
        if (_promptData.HasCategory("planets"))
        {
            var jsonNames = _promptData.GetAssetNames("planets");
            if (jsonNames.Count > 0)
                return jsonNames;
        }

        // Fallback to hardcoded list
        return new List<string>
        {
            // Habitable (6)
            "Planet Class M Earthlike", "Planet Class M Ocean", "Planet Class M Jungle", "Planet Class M Arctic", "Planet Class M Desert", "Planet Class L Marginal",
            // Gas Giants (6)
            "Planet Gas Giant Blue", "Planet Gas Giant Orange", "Planet Gas Giant Banded", "Planet Gas Giant Ringed", "Planet Gas Giant Storm", "Planet Gas Giant Small",
            // Barren/Rocky (6)
            "Planet Class D Barren", "Planet Class H Desert", "Planet Class Y Demon", "Planet Rocky Cratered", "Planet Rocky Volcanic", "Planet Asteroid Large",
            // Ice/Cold (6)
            "Planet Class P Ice", "Planet Frozen Ocean", "Planet Ice Rings", "Planet Snowball", "Planet Cryo Volcanic", "Planet Comet Large",
            // Exotic (6)
            "Planet Class J Super Giant", "Planet Rogue Dark", "Planet Crystalline", "Planet Artificial Dyson", "Planet Hollow", "Planet Ring World Section",
            // Special (6)
            "Planet Borg Assimilated", "Planet Destroyed Half", "Planet Under Attack", "Planet Shielded", "Planet Terraforming", "Planet Ancient Ruins"
        };
    }
    
    private List<string> GetStarsList()
    {
        // Try to load from JSON first
        if (_promptData.HasCategory("stars"))
        {
            var jsonNames = _promptData.GetAssetNames("stars");
            if (jsonNames.Count > 0)
                return jsonNames;
        }

        // Fallback to hardcoded list
        return new List<string>
        {
            // Main Sequence (4)
            "Star Yellow Dwarf", "Star Orange Dwarf", "Star Red Dwarf", "Star Blue Giant",
            // Giants (4)
            "Star Red Giant", "Star Blue Supergiant", "Star White Giant", "Star Orange Giant",
            // Exotic (4)
            "Star Neutron Pulsar", "Star Black Hole", "Star White Dwarf", "Star Brown Dwarf",
            // Binary/Special (4)
            "Star Binary System", "Star Trinary System", "Star Protostar Nebula", "Star Supernova Remnant"
        };
    }
    
    private List<string> GetFactionSymbolsList()
    {
        // Try to load from JSON first
        if (_promptData.HasCategory("factionsymbols"))
        {
            var jsonNames = _promptData.GetAssetNames("factionsymbols");
            if (jsonNames.Count > 0)
                return jsonNames;
        }

        // Fallback to hardcoded list - 5x5 grid (25 slots)
        return new List<string>
        {
            // Row 1 - Major Powers
            "Symbol United Federation of Planets",
            "Symbol Klingon Empire",
            "Symbol Romulan Star Empire",
            "Symbol Cardassian Union",
            "Symbol Ferengi Alliance",
            // Row 2 - Major Powers continued
            "Symbol Dominion",
            "Symbol Borg Collective",
            "Symbol Breen Confederacy",
            "Symbol Gorn Hegemony",
            "Symbol Andorian Empire",
            // Row 3 - Minor Factions
            "Symbol Vulcan High Command",
            "Symbol Trill Symbiosis Commission",
            "Symbol Bajoran Republic",
            "Symbol Tholian Assembly",
            "Symbol Orion Syndicate",
            // Row 4 - New Factions
            "Symbol Hirogen Hunters",
            "Symbol Betazoid",
            "Symbol Maquis Resistance",
            "Symbol Pirates",
            "Symbol Nausicaan",
            // Row 5 - Special
            "Symbol Species 8472",
            "Symbol Terran Empire",
            "Symbol Reserved 1",
            "Symbol Reserved 2",
            "Symbol Reserved 3"
        };
    }
    
    private List<string> GetFactionLeadersList()
    {
        // Try to load from JSON first
        if (_promptData.HasCategory("factionleaders"))
        {
            var jsonNames = _promptData.GetAssetNames("factionleaders");
            if (jsonNames.Count > 0)
                return jsonNames;
        }

        // Fallback to hardcoded list (should not be needed if JSON is loaded)
        return new List<string>
        {
            // Row 1 - Major Faction Leaders (4 columns)
            "Leader Federation President Human Male",
            "Leader Klingon Chancellor Male",
            "Leader Romulan Praetor Male",
            "Leader Cardassian Legate Male",
            // Row 2 - Major Faction Leaders continued
            "Leader Ferengi Grand Nagus Male",
            "Leader Dominion Female Changeling Founder",
            "Leader Borg Queen Female",
            "Leader Breen Thot Modern",
            // Row 3 - Breen variants + Hirogen/Gorn
            "Leader Breen Thot Classic",
            "Leader Breen Thot Revealed Face",
            "Leader Hirogen Alpha Hunter Male",
            "Leader Gorn Hegemony King Male",
            // Row 4 - Minor Faction Leaders
            "Leader Andorian Chancellor Female",
            "Leader Vulcan High Command Admiral Male",
            "Leader Trill Symbiosis President Female",
            "Leader Bajoran Kai Female Religious",
            // Row 5 - Alien Leaders + Mercenaries
            "Leader Tholian Assembly Commander",
            "Leader Orion Syndicate Boss Female",
            "Leader Independent Mercenary Captain",
            "Leader Nausicaan Raider Captain"
        };
    }
    
    private string BuildFactionLeaderPrompt(string leaderName)
    {
        var leaderDetails = GetFactionLeaderDetails(leaderName);
        
        return $@"FACTION LEADER PORTRAIT - OFFICIAL HEAD OF STATE

{leaderDetails}

MANDATORY STYLE GUIDE:
Material & Texture: The character must look like a handmade physical model made of plasticine clay with a subtle comic-like shader. Realistic skin/surface texture with non-glossy, soft clay finish. High-end stop-motion animation quality (Laika/Aardman style).
Proportions: Stylized but near-realistic. NOT caricature or oversized head.
Lighting: Soft, cinematic studio lighting. Subsurface scattering on skin.
Expression: Commanding, authoritative, powerful - this is a LEADER.
Composition: Frontal or 3/4 view, medium shot, looking at camera.
Background: Solid black background (#000000).

CRITICAL - LEADER CHARACTERISTICS:
- Must convey AUTHORITY and POWER
- Wearing official state/military attire for their faction
- Regal bearing, confident posture
- Age: Mature, experienced (not young)
- Distinguished appearance befitting a head of state

**medium shot faction leader portrait, claymation 3D style, commanding presence, game character portrait**
--no funny, thumbprints, exaggerated features, young appearance, casual clothing, action poses, text, labels";
    }
    
    private string GetFactionLeaderDetails(string leaderName)
    {
        var lowerName = leaderName.ToLower();
        
        // IMPORTANT: Check specific faction names FIRST, then fall back to titles
        // This prevents "Trill President" matching on "president" -> Federation
        // and "Andorian Chancellor" matching on "chancellor" -> Klingon
        
        // === SPECIFIC FACTION NAMES FIRST ===
        
        if (lowerName.Contains("andorian"))
        {
            var isFemale = lowerName.Contains("female");
            var genderDesc = isFemale 
                ? "FEMALE Andorian - feminine features, can have elegant/fierce appearance"
                : "MALE Andorian - masculine features, strong jaw";
            var hairStyle = isFemale
                ? "STARK WHITE HAIR - pure white, can be long, braided, or elegantly styled"
                : "STARK WHITE HAIR - pure white, often worn up or back";
            var reference = isFemale
                ? "Think: Andorian women from Enterprise (Talas, Jhamel)"
                : "Think: Commander Shran from Enterprise";
                
            return $@"ANDORIAN LEADER ({(isFemale ? "FEMALE" : "MALE")}):
- ANDORIAN species (NOT Klingon, NOT human)
- {genderDesc}
- ICE-BLUE SKIN - bright cyan/blue color
- {hairStyle}
- TWO ANTENNAE on TOP of forehead (flexible stalks that can move)
- NO forehead ridges (unlike Klingons)
- Humanoid facial structure but distinctly BLUE
- Military uniform: Imperial Guard style, blue/white/silver
- Proud, passionate expression (Andorians are emotional warriors)
- {reference}
- Key: BLUE SKIN + WHITE HAIR + ANTENNAE = Andorian";
        }
        
        if (lowerName.Contains("trill"))
        {
            var isFemale = lowerName.Contains("female");
            return $@"TRILL LEADER ({(isFemale ? "FEMALE" : "MALE")}):
- Trill with distinctive SPOTTED PATTERN running down temples and neck
- {(isFemale ? "FEMALE - elegant, wise feminine features" : "MALE - distinguished masculine features")}
- Humanoid appearance, joined with symbiont (centuries of memories)
- Spots: Brown/tan spots in a line from forehead down sides of face to neck
- Diplomatic formal attire, refined and elegant
- Wise, serene expression (multiple lifetimes of experience)
- {(isFemale ? "Think: Jadzia Dax, Ezri Dax - confident, experienced woman" : "Think: distinguished Trill diplomat")}
- Key: SPOTS ON TEMPLES/NECK = Trill";
        }
        
        if (lowerName.Contains("bajoran"))
            return @"BAJORAN KAI (FEMALE):
- FEMALE Bajoran spiritual leader
- Bajoran with distinctive NOSE RIDGES (horizontal ridges across nose bridge)
- Older, wise woman with serene yet determined expression
- Religious ROBES and ceremonial HEADDRESS
- EARRING (d'ja pagh) on RIGHT ear - important religious symbol
- Orange/red/gold ceremonial robes, ornate
- Think: Kai Winn, Kai Opaka - powerful religious authority
- Key: NOSE RIDGES + EARRING + ROBES = Bajoran Kai";
        
        if (lowerName.Contains("orion"))
            return @"ORION SYNDICATE BOSS (FEMALE):
- FEMALE Orion with vivid GREEN SKIN
- Alluring yet dangerous appearance
- Beautiful but clearly ruthless and calculating
- Luxurious clothing, gold jewelry, wealth on display
- Dark hair, often elaborate styling
- Seductive but intimidating expression
- Think: Orion slave girl who became the master, criminal queen
- Key: GREEN SKIN + FEMALE + DANGEROUS BEAUTY = Orion Boss";
        
        if (lowerName.Contains("vulcan"))
            return @"VULCAN HIGH COMMAND:
- Vulcan with pointed ears, arched eyebrows
- Severe, logical expression - no emotion
- Vulcan robes or High Command uniform
- Dignified, ancient wisdom
- Think: master of logic, emotionless strategist
- Brown/rust/gold robes";
        
        if (lowerName.Contains("hirogen"))
        {
            // Check for helmet/armor variants
            if (lowerName.Contains("helmet") || lowerName.Contains("armored") || lowerName.Contains("suited"))
                return @"HIROGEN ALPHA HUNTER (FULLY ARMORED WITH HELMET):
- MASSIVE towering build - much larger than humans
- FULL-FACE HUNTING HELMET covering entire head
- Helmet is SILVER-BLUE METALLIC with color-shifting sheen
- ANGULAR PREDATORY SHAPE - aggressive hunting aesthetic
- NARROW HORIZONTAL VISOR slit with AMBER/ORANGE glow
- RESPIRATOR SECTION covering mouth/nose - ribbed metal grille
- Tubes and hoses connecting helmet to suit
- SILVER-BLUE METALLIC full body armor matching helmet
- Segmented armor plates, heavy pauldrons
- TROPHY DECORATIONS - bones, teeth from kills
- Sensor equipment integrated into armor
- Battle-worn finish - scratches, dents
- KEY: SILVER-BLUE ARMOR + FULL HELMET + NARROW VISOR + TROPHIES = Armored Hirogen";

            if (lowerName.Contains("mask") || lowerName.Contains("partial"))
                return @"HIROGEN ALPHA HUNTER (PARTIAL HELMET - SIGNATURE LOOK):
- MASSIVE towering reptilian humanoid
- UPPER SKULL VISIBLE showing DUAL-LOBED CRANIUM (two bulges on top of head!)
- Completely BALD - no hair
- Heavy BROW RIDGES over deep-set predatory eyes
- TAN/OLIVE SCALED SKIN visible on forehead (Gila monster texture)
- RESPIRATOR MASK covering LOWER FACE (nose and mouth)
- Mask is SILVER-BLUE METALLIC with ribbed grille
- Tubes connecting mask to armor
- SILVER-BLUE metallic body armor
- Trophy decorations on armor
- KEY: DUAL-LOBED SKULL VISIBLE + SILVER-BLUE RESPIRATOR MASK = Signature Hirogen";

            // Default: unmasked face visible
            return @"HIROGEN ALPHA HUNTER (FACE VISIBLE):
=== CRITICAL CRANIAL STRUCTURE ===
- ENLARGED DUAL-LOBED CRANIUM - TWO DISTINCT BULGING LOBES on top of skull (THIS IS KEY!)
- Head is OVERSIZED compared to humans
- Skull WIDENS at top into two rounded protrusions
- Completely BALD - no hair on head or face

=== CRITICAL FACIAL FEATURES ===
- FLAT NOSE - almost NO nasal bridge, nostrils sit FLUSH with face (like reptile/Saurian)
- NO LIPS - mouth is a LIPLESS reptilian slit
- HEAVY BROW RIDGES protruding over deep-set eyes
- Deep-set PREDATORY EYES with intense hunter's gaze

=== SKIN ===
- SCALED/PEBBLED TEXTURE like GILA MONSTER skin
- TAN/BEIGE to OLIVE coloring with darker MOTTLED patches
- Rough, weathered, bumpy texture - NOT smooth

=== ARMOR (when unmasked) ===
- Heavy BRONZE/OLIVE hunting armor (matches skin tones)
- Trophy decorations - bones, teeth, skulls
- ALPHA STATUS SYMBOLS

- KEY: DUAL-LOBED SKULL + FLAT NOSE + NO LIPS + GILA MONSTER SKIN = Hirogen
- Think: massive reptilian hunter with distinctive two-lobed alien skull";
        }

        if (lowerName.Contains("gorn"))
            return @"GORN HEGEMONY LEADER:
- Reptilian Gorn with green scaly skin
- Large, powerful build
- Saurian features, multi-faceted eyes
- Ceremonial armor or royal regalia
- Fierce, predatory expression
- Think: ancient reptilian king, powerful warrior";

        if (lowerName.Contains("nausicaan"))
            return @"NAUSICAAN RAIDER CAPTAIN:
- TALL, muscular humanoid with rough gray/brown skin
- Prominent TUSKS protruding from lower jaw
- Heavy brow ridges and deep-set eyes
- Bald or minimal coarse hair
- Battle scars and rough, weathered features
- Rough leather and metal armor
- Mismatched pirate/raider aesthetic
- Trophy items from past raids visible
- Dark colors - black, brown, gunmetal
- Aggressive, intimidating, brutish expression
- Think: space pirate, hired muscle, dangerous thug-for-hire
- KEY: TUSKS + TALL + BRUTISH = Nausicaan";

        if (lowerName.Contains("tholian"))
            return @"THOLIAN COMMANDER:
- Crystalline/silicon-based life form
- Geometric, faceted body structure
- Glowing internal light
- Alien, inscrutable
- Environmental suit if visible
- Think: incomprehensible alien intelligence";

        if (lowerName.Contains("betazoid"))
            return @"BETAZOID LEADER/AMBASSADOR:
- Humanoid NEARLY IDENTICAL to human appearance
- SOLID BLACK EYES - completely black, no visible iris or pupil (KEY FEATURE!)
- Elegant, refined features
- Serene, empathic expression showing telepathic awareness
- Often female (matriarchal society) but can be male
- CLOTHING: Flowing elegant robes in PURPLES, LAVENDERS, and SILVERS
- Or Starfleet uniform if serving in fleet
- Sophisticated, calm, diplomatic bearing
- Think: Lwaxana Troi, Deanna Troi - telepathic counselor/diplomat
- KEY: SOLID BLACK EYES + ELEGANT ROBES + SERENE EXPRESSION = Betazoid";

        if (lowerName.Contains("maquis"))
            return @"MAQUIS LEADER/COMMANDER:
- MIXED SPECIES - Human, Bajoran, or occasionally Vulcan
- RESISTANCE FIGHTER aesthetic - NOT military uniform
- Rugged civilian clothing: leather jackets, practical gear
- Battle-worn, determined expression
- Scars or signs of hard living common
- Earth tone colors: browns, greens, blacks
- NO Starfleet uniform (they rejected Starfleet)
- Visible weapons, utilitarian equipment
- Intense, passionate, defiant expression
- Think: Chakotay, Michael Eddington - freedom fighter against Cardassians
- KEY: CIVILIAN CLOTHES + RESISTANCE LOOK + DETERMINED EXPRESSION = Maquis";

        if (lowerName.Contains("pirate") || lowerName.Contains("raider"))
            return @"PIRATE FLEET COMMANDER:
- HUMAN male with commanding, authoritative presence
- Middle-aged to older, experienced and hardened
- Cold, calculating eyes - battle-worn veteran
- DARK MILITARY-STYLE UNIFORM - like a corrupted/twisted admiral's uniform
- BLACK or DARK GRAY base with GOLD/BRASS trim
- High collar, formal military cut
- MEDALS AND DECORATIONS on chest - stolen or self-awarded honors
- Rank insignia (self-proclaimed)
- Optional: dark cape or long coat over uniform
- Cold, authoritative, dangerous expression
- Think: rogue admiral, fallen military commander turned pirate lord
- KEY: DARK MILITARY UNIFORM + MEDALS + COMMANDING PRESENCE = Pirate Commander";

        if (lowerName.Contains("species8472") || lowerName.Contains("8472") || lowerName.Contains("undine"))
            return @"SPECIES 8472 / UNDINE LEADER:
- TRIPEDAL ALIEN - three legs, three arms
- EXTREMELY TALL and thin, elongated body
- ELONGATED HEAD with MULTIPLE EYES (no visible mouth)
- GRAY-PURPLE SKIN with organic texture
- Communicates TELEPATHICALLY - no mouth visible
- NO CLOTHING - organic exoskeleton/bio-armor
- Bioluminescent markings possible
- TERRIFYING, ALIEN appearance - not humanoid
- From FLUIDIC SPACE - completely alien
- Think: ultimate alien threat, extradimensional beings
- KEY: TRIPEDAL + MULTIPLE EYES + NO MOUTH + GRAY-PURPLE = Species 8472";

        if (lowerName.Contains("breen") || lowerName.Contains("thot"))
        {
            // Check for specific Breen variants
            if (lowerName.Contains("classic"))
            {
                return @"BREEN THOT CLASSIC STYLE (DS9 ERA):
- HELMET:
  * Closed helmet made of SAND-COLORED/TAN material
  * Wide HORIZONTAL VISOR glowing NEON GREEN
  * Below visor: ELONGATED SNOUT/BREATHER protruding forward
  * Snout shape: truncated pyramid extending forward and down
  * Vertical ridges/grilles along the respirator
  * Horizontal layered bands on top of helmet
- CLOTHING & ARMOR:
  * SAND/TAN colored LEATHER tunic as base layer
  * CROSSED BANDOLIERS/STRAPS across chest in dark brown
  * Bandoliers have basket-weave/grid texture
  * Massive angular SHOULDER PADS
  * NO capes, NO cloaks
- COLOR PALETTE: Sand, tan, beige, khaki, brown leather, green visor glow
- KEY: Sand-colored leather armor, elongated snout helmet, green visor, NO face visible";
            }

            if (lowerName.Contains("revealed") || lowerName.Contains("face"))
            {
                return @"BREEN THOT WITH VISIBLE FACE:
- HELMET:
  * OPEN-FACE HELMET with dark metal frame
  * GREEN TRANSLUCENT ENERGY FIELD covering the face area
  * Energy field CONTOURS TO THE FACE SHAPE - not flat glass
  * Dark metal frame with BRONZE/GOLD accent trim around faceplate
  * Vertical ribbed tubes on sides of helmet
- FACE:
  * REPTILIAN FACE visible through green energy barrier
  * FLAT NOSE that merges seamlessly into a raised FOREHEAD PLATE
  * Slightly GLOWING EYES with eerie luminescence
  * SCALY TEXTURED SKIN - reptilian/amphibian appearance
  * NOT human - clearly alien reptilian features
- CLOTHING:
  * DARK METALLIC ARMOR - gunmetal/black base
  * BRONZE/GOLD ACCENT STRIPS and trim pieces
  * Heavy industrial aesthetic - tubes, cables, mechanical joints
  * NO capes, NO cloaks, NO fabric
- KEY: Open helmet with CONTOURED green energy field over REPTILIAN face";
            }

            // Default: Modern Breen with rounded helmet
            return @"BREEN THOT MILITARY COMMANDER (MODERN):
- HELMET:
  * ROUNDED DOME HELMET made of dark gunmetal/black metal
  * Helmet shape is SPHERICAL/ROUNDED - like a dome or bubble
  * Narrow HORIZONTAL CYAN/TEAL VISOR - thin glowing slit across eyes
  * RESPIRATOR/BREATH MASK integrated into lower helmet section
  * Respirator has VERTICAL GRILLE LINES/VENTS at the mouth area
  * The respirator section is recessed/inset into the helmet, not smooth
  * Helmet fully CLOSED - NO face visible, NO skin showing
- CLOTHING & ARMOR:
  * DARK METALLIC ARMOR - gunmetal gray/black color
  * Sleek modern design with segmented armor plates
  * High collar integrated with helmet seal
  * Minimal ornamentation - functional military aesthetic
  * NO capes, NO cloaks, NO fabric - only metal armor
- COLOR PALETTE: Dark gunmetal, black metal, cyan/teal visor glow ONLY
- CRITICAL: Helmet is ROUNDED/DOMED, respirator has VERTICAL VENT LINES at mouth";
        }
        
        if (lowerName.Contains("cardassian"))
            return @"CARDASSIAN LEGATE:
- CARDASSIAN species (NOT human, NOT Starfleet)
- GRAY SKIN with blue undertones
- Distinctive NECK RIDGES: raised scales running down sides of neck
- SPOON-SHAPED forehead ridge (like an inverted spoon on forehead)
- Slicked back BLACK hair
- CARDASSIAN MILITARY ARMOR (NOT Starfleet uniform!):
  * Dark brown/gray segmented armor plates
  * High collar with pointed shoulder pieces
  * Chest plate with Cardassian Union symbol
  * NO colored division stripes (that's Starfleet)
- Cold, calculating, reptilian expression
- Think: Gul Dukat, ruthless military dictator
- Color scheme: Brown, tan, gray, dark olive
- Key: NECK RIDGES + SPOON FOREHEAD + BROWN ARMOR = Cardassian";
        
        if (lowerName.Contains("ferengi"))
            return @"FERENGI GRAND NAGUS:
- Ferengi with large ears, prominent lobes
- Ornate gold robes and headdress
- Holds or wears symbols of wealth
- Shrewd, cunning expression
- Think: ultimate capitalist, master dealmaker
- Gold/orange color scheme, ostentatious jewelry
- Staff of the Grand Nagus optional";
        
        if (lowerName.Contains("dominion") || lowerName.Contains("founder") || lowerName.Contains("changeling"))
            return @"DOMINION FOUNDER:
- Female Changeling in humanoid form
- Orange-brown waxy skin, slicked back hair appearance
- Smooth, undefined facial features
- Simple robes
- Serene yet terrifying expression
- Think: ancient god-like being, absolute ruler
- Slightly liquid/morphing quality to edges";
        
        if (lowerName.Contains("borg") || lowerName.Contains("queen"))
            return @"BORG QUEEN:
- Humanoid female with extensive cybernetic implants
- Pale gray skin, bald or minimal dark hair
- One eye cybernetic, one organic
- Mechanical spine and shoulder assembly
- Seductive yet terrifying expression
- Think: hive mind incarnate, perfection personified
- Dark mechanical aesthetic, green accent lights";
        
        if (lowerName.Contains("romulan"))
            return @"ROMULAN PRAETOR:
- Romulan with pointed ears, V-shaped brow ridges
- Severe, calculating expression
- Dark hair, often short or pulled back
- Imperial Romulan robes with high collar
- Think: cunning strategist, master of intrigue
- Green/silver color scheme in attire
- Romulan bird of prey insignia";
        
        if (lowerName.Contains("klingon"))
            return @"KLINGON CHANCELLOR:
- Klingon male with prominent forehead ridges
- Long dark hair, possibly with gray streaks
- Ceremonial Klingon armor or Chancellor's robes
- Fierce but noble expression
- Battle-scarred veteran warrior
- Think: Gowron, Martok - powerful warrior-politician
- Klingon trefoil insignia visible";
        
        // === GENERIC TITLES LAST (fallbacks) ===
        
        if (lowerName.Contains("federation") || lowerName.Contains("president"))
            return @"FEDERATION PRESIDENT:
- Human (or other Federation species) in formal attire
- Distinguished, diplomatic appearance
- Presidential robes or formal Starfleet dress uniform
- Calm, wise, peaceful expression
- Think: seasoned diplomat, experienced statesman
- Gray/white hair common, mature appearance
- Federation insignia visible";
        
        if (lowerName.Contains("chancellor"))
            return @"KLINGON CHANCELLOR:
- Klingon male with prominent forehead ridges
- Long dark hair, possibly with gray streaks
- Ceremonial Klingon armor or Chancellor's robes
- Fierce but noble expression
- Battle-scarred veteran warrior
- Think: Gowron, Martok - powerful warrior-politician
- Klingon trefoil insignia visible";
        
        if (lowerName.Contains("independent") || lowerName.Contains("mercenary"))
            return @"INDEPENDENT MERCENARY CAPTAIN:
- Rugged, experienced appearance
- Battle-worn but capable
- Practical military/tactical gear
- Mixed equipment from various sources
- No specific faction insignia
- Calculating, self-reliant expression
- Think: freelance starship captain, survivor";
        
        // Default for any other leader
        return @"FACTION LEADER:
- Distinguished appearance befitting head of state
- Formal attire appropriate to their culture
- Commanding presence, authority
- Mature, experienced appearance
- Official insignia or symbols of office";
    }
    
    // ===== SPECIAL CHARACTERS (Q, Data, Khan etc.) =====
    
    private List<string> GetSpecialCharactersList()
    {
        // Try to load from JSON first
        if (_promptData.HasCategory("specialcharacters"))
        {
            var jsonNames = _promptData.GetAssetNames("specialcharacters");
            if (jsonNames.Count > 0)
                return jsonNames;
        }

        // Fallback to hardcoded list
        return new List<string>
        {
            // Q (3 variants)
            "Q Classic Smirk Starfleet",
            "Q Judge Robes Tribunal",
            "Q Mariachi Snapping Fingers",

            // Data / Androids (3 variants)
            "Data Yellow Eyes Curious",
            "Data Emotion Chip Smiling",
            "Lore Evil Twin Smirk",

            // Khan / Augments (3 variants)
            "Khan Classic Vengeful",
            "Khan Young Calculating",
            "Augment Soldier Elite",

            // Other iconic (7 variants)
            "Guinan Wise Bartender",
            "Borg Queen Seductive",
            "Odo Grumpy Constable",
            "Mirror Spock Goatee",
            "Holographic Doctor EMH"
        };
    }
    
    private string BuildSpecialCharacterPrompt(string characterName)
    {
        var characterDetails = GetSpecialCharacterDetails(characterName);
        
        return $@"ICONIC STAR TREK CHARACTER PORTRAIT

{characterDetails}

MANDATORY STYLE GUIDE:
Material & Texture: The character must look like a handmade physical model made of plasticine clay. Realistic skin/surface texture with non-glossy, soft clay finish. High-end stop-motion animation quality (Laika/Aardman style).
Proportions: Stylized but recognizable. Must be IDENTIFIABLE as the character.
Lighting: Soft, cinematic studio lighting. Subsurface scattering on skin.
Composition: Medium shot portrait, 3/4 view, looking at camera.
Background: Solid black background (#000000).

CRITICAL - RECOGNIZABILITY:
- This is an ICONIC character - must be recognizable
- Key features that define the character must be present
- Expression and pose should capture their personality

**medium shot character portrait, claymation 3D style, iconic Star Trek character, game asset**
--no funny, no thumbprints, no generic, no wrong features, no text, no labels";
    }
    
    private string GetSpecialCharacterDetails(string characterName)
    {
        var nameLower = characterName.ToLower();
        
        // Q variants
        if (nameLower.Contains("q classic") || nameLower.Contains("q starfleet"))
            return @"Q (THE Q):
- Human male, middle-aged, distinguished
- Brown hair, slightly receding
- SMUG, MISCHIEVOUS SMIRK (signature!)
- Starfleet captain uniform (red/black TNG era)
- Raised eyebrow, about to say something clever
- Air of omnipotent superiority
- 'Mon capitaine!' energy";

        if (nameLower.Contains("q judge"))
            return @"Q AS JUDGE:
- Same Q appearance (middle-aged human male)
- Elaborate RED/BLACK judge robes
- White powdered wig (18th century style)
- Stern, judging expression
- 'Humanity on trial' pose
- Gavel optional";

        if (nameLower.Contains("q mariachi"))
            return @"Q IN MARIACHI OUTFIT:
- Same Q appearance
- Full mariachi costume with sombrero
- Playing or holding guitar
- Amused, playful expression
- Snapping fingers pose
- Classic Q humor moment";

        // Data variants
        if (nameLower.Contains("data yellow") || nameLower.Contains("data curious"))
            return @"DATA:
- Android with PALE YELLOWISH skin
- Slicked back BLACK hair
- YELLOW/GOLD EYES (distinctive!)
- Starfleet uniform (gold/operations, TNG era)
- Head tilted slightly, CURIOUS expression
- Trying to understand humans
- Perfectly symmetrical features";

        if (nameLower.Contains("data emotion"))
            return @"DATA WITH EMOTION CHIP:
- Same Data appearance (pale yellow skin, gold eyes)
- Starfleet uniform
- SMILING genuinely (rare for Data!)
- Experiencing emotions
- Joyful, slightly overwhelmed expression";

        if (nameLower.Contains("lore"))
            return @"LORE (Data's Evil Twin):
- IDENTICAL to Data (pale yellow skin, gold eyes)
- But with CRUEL SMILE, malevolent expression
- More expressive, confident than Data
- Arrogant posture
- Civilian clothes or Starfleet uniform
- Evil intelligence in eyes";

        // Khan variants
        if (nameLower.Contains("khan classic"))
            return @"KHAN NOONIEN SINGH (Classic):
- South Asian features, powerful build
- Long grey/white hair, swept back
- Open chest showing muscular physique
- VENGEFUL, intense expression
- 'FROM HELL'S HEART I STAB AT THEE' energy
- 20th century superman, exiled ruler";

        if (nameLower.Contains("khan young"))
            return @"KHAN (Young/Modern):
- Pale skin, dark slicked hair
- Sharp, angular features
- Intense, cold eyes
- Starfleet or dark civilian clothes
- Calculating, dangerous expression
- Superior intellect visible in gaze";

        if (nameLower.Contains("augment"))
            return @"AUGMENT SUPER SOLDIER:
- Genetically enhanced human
- Perfect physique, intense expression
- Any ethnicity
- Military bearing
- Superior, arrogant body language
- Enhanced human, not quite Khan-level";

        // Other iconic characters
        if (nameLower.Contains("guinan"))
            return @"GUINAN:
- Dark-skinned female, ageless appearance
- Elaborate HAT/HEADPIECE (signature!)
- Wise, knowing smile
- Ten Forward bartender attire (purple/burgundy)
- 'I know things' mysterious expression
- El-Aurian wisdom in eyes";

        if (nameLower.Contains("borg queen"))
            return @"BORG QUEEN:
- Pale grey skin, bald head
- Extensive cybernetic implants
- One eye organic, one cybernetic
- Mechanical spine/shoulder assembly
- Seductive yet terrifying smile
- Dark lips, intense gaze
- 'I am the Borg' confidence";

        if (nameLower.Contains("odo"))
            return @"ODO:
- Smooth, undefined features
- Orange-brown skin, waxy texture
- Slicked back 'hair' (part of form)
- DS9 security uniform (tan/brown)
- GRUMPY, suspicious expression
- Arms behind back (signature pose)
- Constable energy";

        if (nameLower.Contains("mirror spock") || nameLower.Contains("spock goatee"))
            return @"MIRROR SPOCK:
- Vulcan male, pointed ears
- GOATEE BEARD (signature!)
- Raised eyebrow
- Terran Empire uniform with gold sash
- Calculating but not entirely evil
- Sword-through-Earth insignia visible";

        if (nameLower.Contains("doctor") || nameLower.Contains("emh"))
            return @"THE DOCTOR (EMH):
- Balding human male
- Slightly pudgy, middle-aged
- Starfleet medical uniform (blue, Voyager era)
- Mobile emitter on arm
- Exasperated or professional expression
- 'Please state the nature...' energy
- Self-important but caring";

        // Default
        return @"ICONIC STAR TREK CHARACTER:
- Recognizable character from the franchise
- Distinctive features that identify them
- Appropriate costume/uniform
- Expression matching their personality";
    }
    
    private string BuildFactionSymbolPrompt(string symbolName)
    {
        var factionDetails = GetFactionSymbolDetails(symbolName);
        
        return $@"FLAT FACTION SYMBOL / LOGO

{factionDetails}

MANDATORY STYLE REQUIREMENTS:
- FLAT 2D graphic symbol/logo - NO 3D, NO depth, NO metallic
- Clean vector-style art with solid colors
- Official faction emblem as seen in Star Trek
- Simple, bold, iconic design that reads well at any size
- Think: Flag emblems, military insignia graphics, franchise logos
- Solid color fills with clean outlines

COLOR APPROACH:
- Use faction-canonical colors as solid fills
- High contrast between symbol and background  
- Maximum 3-4 colors per symbol
- Clean color separations (no gradients, no shading)

COMPOSITION:
- Symbol perfectly centered
- Fills most of the frame (80-90%)
- Solid black background (#000000)
- No perspective, no shadows, no reflections

CRITICAL:
- This is a FLAT GRAPHIC SYMBOL, not a 3D object
- Must be the OFFICIAL recognizable faction emblem
- Suitable for flags, banners, UI elements, building signs
- Simple enough to be recognized at small sizes

**flat 2D faction logo, clean vector emblem, bold graphic symbol, official insignia, game asset**
--no 3D, no metallic, no depth, no shadows, no gradients, no reflections, no perspective, no text, no labels";
    }
    
    private string GetFactionSymbolDetails(string symbolName)
    {
        var lowerName = symbolName.ToLower();
        
        if (lowerName.Contains("federation"))
            return @"DESIGN: Circle emblem with a STARBURST/SUN in the center with pointed rays radiating outward, surrounded by LAUREL WREATH branches on left and right sides, small STARS scattered around
SHAPE: Circular seal
COLORS: Blue background, white/silver symbol";
        
        if (lowerName.Contains("klingon"))
            return @"DESIGN: TREFOIL shape - THREE POINTED BLADES arranged vertically: one central blade pointing UP, two curved blades on sides pointing DOWN and OUTWARD like a stylized trident
SHAPE: Vertical trefoil/triquetra
COLORS: Red symbol on black, or black on red";
        
        if (lowerName.Contains("romulan"))
            return @"DESIGN: BIRD OF PREY viewed from FRONT with WINGS SPREAD WIDE, the bird is GRIPPING TWO CIRCULAR PLANETS (Romulus and Remus) in its TALONS
SHAPE: Winged bird holding two spheres
COLORS: Green bird on darker background";
        
        if (lowerName.Contains("cardassian"))
            return @"DESIGN: Vertical POINTED OVAL or EYE shape, like an almond, with internal geometric lines forming a stylized pattern inside
SHAPE: Vertical pointed oval/eye
COLORS: Brown/tan symbol, gray accents";
        
        if (lowerName.Contains("ferengi"))
            return @"DESIGN: Stylized FERENGI HEAD in PROFILE (side view) showing the distinctive LARGE EAR prominently, bald head silhouette
SHAPE: Head profile silhouette with big ear
COLORS: Gold/orange symbol";
        
        if (lowerName.Contains("dominion"))
            return @"DESIGN: Abstract ORGANIC SWIRL or SPIRAL shape, flowing curved lines that suggest liquid or morphing, like a stylized comma
SHAPE: Organic spiral/swirl
COLORS: Purple/violet symbol";
        
        if (lowerName.Contains("borg"))
            return @"DESIGN: Geometric OCTAGON or HEXAGON shape with internal CIRCUIT-LIKE PATTERNS, technological grid lines, central circular node
SHAPE: Geometric octagon with circuits
COLORS: Black/dark gray with green accents";
        
        if (lowerName.Contains("breen"))
            return @"DESIGN: Abstract angular shape suggesting their HELMET - vertical elongated form with geometric patterns, cold crystalline aesthetic
SHAPE: Angular helmet-like geometric form
COLORS: Ice blue and white/silver";
        
        if (lowerName.Contains("gorn"))
            return @"DESIGN: DIAMOND or RHOMBUS shape (square rotated 45 degrees), aggressive angular design with reptilian aesthetic
SHAPE: Diamond/rhombus
COLORS: Green and bronze/brown";
        
        if (lowerName.Contains("andorian"))
            return @"DESIGN: Design suggesting TWIN ANTENNAE - two curved lines meeting at top like a stylized V or antenna pair, elegant symmetrical curves
SHAPE: Twin curved antennae/wings spreading
COLORS: Blue and white/silver";
        
        if (lowerName.Contains("vulcan"))
            return @"DESIGN: IDIC symbol: a TRIANGLE pointing DOWN with a CIRCLE or GEM inside it, geometric shapes representing logic
SHAPE: Triangle with circle (IDIC)
COLORS: Bronze/copper and gold";
        
        if (lowerName.Contains("trill"))
            return @"DESIGN: CIRCULAR design with SPOTS pattern - dots arranged within a larger circle, representing the symbiont spots
SHAPE: Circle with spots/dots pattern
COLORS: Blue and teal";
        
        if (lowerName.Contains("bajoran"))
            return @"DESIGN: Flowing SPIRAL or FLAME shape, curved organic lines suggesting celestial nature, like a stylized flame with internal swirls
SHAPE: Spiral/flame curve
COLORS: Gold and orange/bronze";
        
        if (lowerName.Contains("tholian"))
            return @"DESIGN: CRYSTALLINE WEB pattern - interconnected TRIANGLES forming a larger geometric shape, like a faceted crystal
SHAPE: Crystalline triangle web
COLORS: Orange and amber/yellow";
        
        if (lowerName.Contains("orion"))
            return @"DESIGN: Curved WING or CRESCENT shape, like spread bird wings or a curved blade, elegant but threatening curves
SHAPE: Curved wings/crescent
COLORS: Green with gold accents";
        
        if (lowerName.Contains("terran") || lowerName.Contains("mirror"))
            return @"DESIGN: EARTH GLOBE with a SWORD or DAGGER piercing THROUGH it vertically, aggressive imperial imagery
SHAPE: Globe pierced by sword
COLORS: Gold globe, silver sword, red/black accents";
        
        if (lowerName.Contains("maquis"))
            return @"DESIGN: Simple angular RESISTANCE symbol - stylized M or arrow shape, rough guerrilla aesthetic
SHAPE: Angular M or arrow
COLORS: Earth tones - brown, tan, muted";
        
        if (lowerName.Contains("species 8472") || lowerName.Contains("8472"))
            return @"DESIGN: ORGANIC TRIPOD shape - three curved appendages radiating from center like bio-ships
SHAPE: Organic tripod/three appendages
COLORS: Orange and yellow";
        
        // Default
        return @"DESIGN: Simple geometric faction emblem
SHAPE: Bold recognizable shape
COLORS: Faction-appropriate";
    }
    
    private List<string> GetAnomaliesList()
    {
        // Try to load from JSON first
        if (_promptData.HasCategory("anomalies"))
        {
            var jsonNames = _promptData.GetAssetNames("anomalies");
            if (jsonNames.Count > 0)
                return jsonNames;
        }

        // Fallback to hardcoded list
        return new List<string>
        {
            // Nebulae (6)
            "Nebula Blue Emission", "Nebula Red Emission", "Nebula Green Planetary", "Nebula Dark Absorption", "Nebula Reflection Purple", "Nebula Supernova Remnant",
            // Spatial Anomalies (6)
            "Anomaly Wormhole Stable", "Anomaly Wormhole Unstable", "Anomaly Black Hole", "Anomaly White Hole", "Anomaly Quantum Singularity", "Anomaly Subspace Rift",
            // Energy Phenomena (6)
            "Anomaly Ion Storm", "Anomaly Plasma Storm", "Anomaly Gravimetric Shear", "Anomaly Tachyon Field", "Anomaly Chroniton Particles", "Anomaly Metreon Cloud",
            // Debris Fields (6)
            "Field Asteroid Dense", "Field Asteroid Sparse", "Field Debris Battle", "Field Ice Cometary", "Field Radiation Belt", "Field Minefield",
            // Exotic (6)
            "Anomaly Dyson Sphere", "Anomaly Guardian of Forever", "Anomaly Bajoran Wormhole", "Anomaly Nexus Ribbon", "Anomaly Fluidic Rift", "Anomaly Temporal Vortex",
            // Interactive (6)
            "Anomaly Derelict Ship", "Anomaly Space Station Abandoned", "Anomaly Probe Ancient", "Anomaly Beacon Signal", "Anomaly Gateway Iconian", "Anomaly Artifact Floating"
        };
    }
    
    private List<string> GetGalaxyTilesList()
    {
        // Try to load from JSON first
        if (_promptData.HasCategory("galaxytiles"))
        {
            var jsonNames = _promptData.GetAssetNames("galaxytiles");
            if (jsonNames.Count > 0)
                return jsonNames;
        }

        // Fallback to hardcoded list
        return new List<string>
        {
            // Space Types (6)
            "Tile Deep Space Empty", "Tile Deep Space Stars", "Tile Sector Border", "Tile Trade Route", "Tile Patrol Route", "Tile Contested Zone",
            // Territory (6)
            "Tile Federation Space", "Tile Klingon Space", "Tile Romulan Space", "Tile Neutral Zone", "Tile Unexplored", "Tile Forbidden Zone",
            // Features (6)
            "Tile Nebula Overlay", "Tile Asteroid Field Overlay", "Tile Wormhole Entry", "Tile Starbase Icon", "Tile Planet Icon", "Tile Fleet Icon",
            // Strategic (6)
            "Tile Chokepoint", "Tile Resource Rich", "Tile Strategic Value", "Tile Defensive Position", "Tile Supply Line", "Tile Communications Hub",
            // Hazards (6)
            "Tile Radiation Zone", "Tile Gravity Well", "Tile Minefield Zone", "Tile Subspace Distortion", "Tile Ion Storm Zone", "Tile Dead Zone",
            // Connections (6)
            "Tile Hyperlane Straight", "Tile Hyperlane Curve", "Tile Hyperlane Junction", "Tile Hyperlane End", "Tile Border Wall", "Tile Blockade"
        };
    }
    
    private List<string> GetSystemElementsList()
    {
        // Try to load from JSON first
        if (_promptData.HasCategory("systemelements"))
        {
            var jsonNames = _promptData.GetAssetNames("systemelements");
            if (jsonNames.Count > 0)
                return jsonNames;
        }

        // Fallback to hardcoded list
        return new List<string>
        {
            // Asteroids (6)
            "Asteroid Large Rocky", "Asteroid Medium Rocky", "Asteroid Small Rocky", "Asteroid Metallic", "Asteroid Ice", "Asteroid Irregular",
            // Moons (6)
            "Moon Large Barren", "Moon Small Cratered", "Moon Ice", "Moon Volcanic", "Moon Inhabited", "Moon Mining Colony",
            // Stations (6)
            "Station Satellite Comm", "Station Satellite Sensor", "Station Defense Platform", "Station Mining Rig", "Station Research Outpost", "Station Waypoint Beacon",
            // Debris (6)
            "Debris Ship Wreck Small", "Debris Ship Wreck Large", "Debris Station Wreck", "Debris Cargo Containers", "Debris Battle Aftermath", "Debris Ancient Ruins",
            // Natural (6)
            "Comet Active Tail", "Comet Dormant", "Ring Segment Dense", "Ring Segment Sparse", "Solar Flare", "Radiation Burst",
            // Artificial (6)
            "Buoy Navigation", "Buoy Warning", "Mine Space", "Probe Automated", "Drone Repair", "Satellite Spy"
        };
    }
    
    private List<string> GetEffectsList(Faction faction)
    {
        // Some effects are faction-specific (weapon colors)
        return new List<string>
        {
            // Weapons (6)
            "Effect Phaser Beam", "Effect Disruptor Beam", "Effect Plasma Torpedo", "Effect Photon Torpedo", "Effect Quantum Torpedo", "Effect Polaron Beam",
            // Shields (6)
            "Effect Shield Bubble", "Effect Shield Impact", "Effect Shield Failing", "Effect Shield Regenerating", "Effect Shield Down", "Effect Shield Modulating",
            // Explosions (6)
            "Effect Explosion Small", "Effect Explosion Medium", "Effect Explosion Large", "Effect Explosion Ship", "Effect Explosion Station", "Effect Explosion Warp Core",
            // Movement (6)
            "Effect Warp Flash", "Effect Warp Trail", "Effect Impulse Glow", "Effect Thruster Fire", "Effect Cloak Shimmer", "Effect Decloak Flash",
            // Utility (6)
            "Effect Transporter Beam", "Effect Tractor Beam", "Effect Scan Pulse", "Effect Repair Sparkle", "Effect Power Transfer", "Effect Communications Wave",
            // Status (6)
            "Effect Damage Smoke", "Effect Hull Breach", "Effect Fire Onboard", "Effect Disabled Drift", "Effect Boarding Action", "Effect Self Destruct"
        };
    }
    
    private string BuildPlanetPrompt(string planetName)
    {
        var planetDetails = GetPlanetDetails(planetName);
        
        return $@"MANDATORY STYLE GUIDE:
Material & Texture: Handmade spherical model made of plasticine clay. Realistic planetary texture with non-glossy, soft clay finish. High-end stop-motion animation quality (Laika/Aardman style).
Proportions: Spherical planet with appropriate features (atmosphere, clouds, terrain).
Lighting: FLAT, EVEN lighting from front - FULLY LIT planet with NO shadows, NO dark side, NO day/night terminator. The entire planet surface should be visible and evenly illuminated.
Composition: Planet centered, slight 3/4 view showing surface features.
Background: Solid black background (#000000) representing space.

{planetDetails}

Subject: {planetName}
Style: Realistic sci-fi planet with claymation aesthetic

CRITICAL LIGHTING: 
- FULL illumination - entire planet evenly lit
- NO shadow on any part of the planet
- NO day/night terminator line
- NO dark side - we will add shadows dynamically in-game

**single planet model, fully lit, no shadows, even illumination, claymation 3D style, game asset**
--no shadows, no dark side, no terminator, no half-lit, no dramatic lighting, spaceships, stations, text, labels";
    }
    
    private string GetPlanetDetails(string planetName)
    {
        var lowerName = planetName.ToLower();
        
        if (lowerName.Contains("class m") || lowerName.Contains("earthlike"))
            return "PLANET TYPE: Class M (Earth-like). Blue oceans, green/brown continents, white clouds, visible atmosphere. Habitable, temperate climate.";
        if (lowerName.Contains("ocean"))
            return "PLANET TYPE: Ocean World. Almost entirely covered in deep blue water. Small island chains. Heavy cloud cover.";
        if (lowerName.Contains("jungle"))
            return "PLANET TYPE: Jungle World. Dense green vegetation covering most surface. Thick atmosphere, many clouds. Hot and humid.";
        if (lowerName.Contains("arctic") || lowerName.Contains("ice"))
            return "PLANET TYPE: Ice World. White and light blue surface. Frozen oceans, glaciers. Thin wisps of clouds.";
        if (lowerName.Contains("desert"))
            return "PLANET TYPE: Desert World. Tan, orange, brown surface. Minimal water. Sparse clouds. Arid climate.";
        if (lowerName.Contains("gas giant"))
            return "PLANET TYPE: Gas Giant. Massive planet with banded cloud layers. No solid surface. Swirling storms. Jupiter or Saturn-like.";
        if (lowerName.Contains("volcanic"))
            return "PLANET TYPE: Volcanic World. Dark surface with glowing lava rivers and eruptions. Smoke and ash in atmosphere. Hellish.";
        if (lowerName.Contains("barren") || lowerName.Contains("rocky"))
            return "PLANET TYPE: Barren Rocky World. Grey, cratered surface like the Moon. No atmosphere. Dead and lifeless.";
        if (lowerName.Contains("demon") || lowerName.Contains("class y"))
            return "PLANET TYPE: Demon Class. Extremely hostile. Toxic atmosphere, extreme temperatures. Bizarre surface colors. Deadly.";
        if (lowerName.Contains("crystalline"))
            return "PLANET TYPE: Crystalline World. Surface covered in crystal formations. Refracts light in rainbow colors. Alien and beautiful.";
        if (lowerName.Contains("borg"))
            return "PLANET TYPE: Borg Assimilated. Surface covered in geometric Borg structures. Green glow. Technology replacing nature.";
        if (lowerName.Contains("destroyed"))
            return "PLANET TYPE: Destroyed Planet. Half the planet missing, debris field. Glowing core visible. Catastrophic damage.";
            
        return "PLANET TYPE: Standard planetary body with appropriate surface features for its classification.";
    }
    
    private string BuildStarPrompt(string starName)
    {
        var starDetails = GetStarDetails(starName);
        
        return $@"MANDATORY STYLE GUIDE:
Material & Texture: Glowing spherical model. Plasma/fire texture with internal glow. Clay-like surface detail but luminous. Visible surface details like sunspots, granulation, convection patterns.
Proportions: Spherical star with clean circular edge.
Lighting: Self-illuminating from within. Bright glowing core with subtle color gradients.
Composition: Star centered, clean circular shape.
Background: Solid black background (#000000) representing space.

{starDetails}

Subject: {starName}
Style: Sci-fi star with claymation aesthetic

CRITICAL - NO EXTERNAL EFFECTS:
- NO light rays extending outward
- NO lens flares
- NO corona extending beyond the star surface
- NO prominences or solar flares shooting out
- Just the SPHERE of the star itself with surface details
- We will add glow/flare effects dynamically in-game

**single star sphere, clean circular edge, no rays, no flares, no corona, glowing surface, game asset**
--no light rays, no lens flare, no corona, no prominences, no flares, no glow extending outward, planets, spaceships, text, labels";
    }
    
    private string GetStarDetails(string starName)
    {
        var lowerName = starName.ToLower();
        
        if (lowerName.Contains("yellow dwarf"))
            return "STAR TYPE: Yellow Dwarf (G-type like our Sun). Bright yellow-white color. Moderate corona. Stable and life-giving.";
        if (lowerName.Contains("red dwarf"))
            return "STAR TYPE: Red Dwarf. Small, dim, reddish-orange color. Weak corona. Most common star type.";
        if (lowerName.Contains("red giant"))
            return "STAR TYPE: Red Giant. Enormous, bloated red-orange star. Diffuse outer layers. Dying star.";
        if (lowerName.Contains("blue giant") || lowerName.Contains("blue supergiant"))
            return "STAR TYPE: Blue Giant/Supergiant. Extremely hot, bright blue-white. Intense radiation. Massive and short-lived.";
        if (lowerName.Contains("neutron") || lowerName.Contains("pulsar"))
            return "STAR TYPE: Neutron Star/Pulsar. Tiny, incredibly dense. Beams of radiation from poles. Rapid rotation implied.";
        if (lowerName.Contains("black hole"))
            return @"OBJECT TYPE: Black Hole (NOT a planet, NOT a star).
CRITICAL: This is a BLACK HOLE - a gravitational singularity in space.
- Central dark void/sphere (the event horizon) - pure black circle
- Bright glowing ACCRETION DISK around it - ring of superheated matter spiraling in
- Gravitational lensing effect - light bending around the void
- Orange/yellow/white hot accretion disk
- Reference: Interstellar movie black hole, or M87 black hole photo
- NO planet surface, NO star surface - just the void and disk";
        if (lowerName.Contains("white dwarf"))
            return "STAR TYPE: White Dwarf. Small, dense, dim white star. Remnant of dead star. Fading glow.";
        if (lowerName.Contains("binary"))
            return @"STAR TYPE: Binary Star System - TWO SEPARATE STARS orbiting each other.
CRITICAL: Show TWO DISTINCT STAR SPHERES with EMPTY SPACE between them.
- Two separate glowing star spheres, NOT touching
- Clear gap/space between the two stars
- Can be different colors (e.g., yellow + red, or blue + orange)
- NO umbilical cord, NO connecting material, NO plasma bridge
- Just two stars near each other in space
- They orbit each other but are NOT connected";
        if (lowerName.Contains("trinary"))
            return @"STAR TYPE: Trinary Star System - THREE SEPARATE STARS.
- Three distinct glowing star spheres
- Clear space between all three stars
- Different colors possible
- Triangular or linear arrangement
- NO connecting material between stars";
        if (lowerName.Contains("supernova"))
            return "STAR TYPE: Supernova Remnant. Expanding shell of glowing gas. Central neutron star or nothing. Beautiful destruction.";
            
        return "STAR TYPE: Standard stellar body with appropriate luminosity and corona.";
    }
    
    private string BuildAnomalyPrompt(string anomalyName)
    {
        var anomalyDetails = GetAnomalyDetails(anomalyName);
        var isNebula = anomalyName.ToLower().Contains("nebula");
        
        // Nebulae need different treatment - more gaseous/diffuse
        if (isNebula)
        {
            return $@"SPACE NEBULA - ASTRONOMICAL GAS CLOUD

{anomalyDetails}

CRITICAL - WHAT A NEBULA LOOKS LIKE:
- Vast, diffuse cloud of GLOWING GAS in space
- Wispy, smoky, fog-like appearance - NOT solid
- Soft edges that fade into space - NOT hard outlines  
- Semi-transparent layers of colorful gas
- Reference: Hubble telescope nebula photos (Orion, Carina, Pillars of Creation)
- Stars visible through the gas or embedded within

WHAT A NEBULA IS NOT:
- NOT a solid object with defined edges
- NOT an organism, creature, or living thing
- NOT bacteria, cells, or biological forms
- NOT tentacles, arms, or appendages
- NOT clay/plasticine - nebulae are GAS CLOUDS

Subject: {anomalyName}
Style: Realistic space nebula, Hubble-photo inspired

**space nebula gas cloud, diffuse wispy appearance, glowing colorful gas, astronomical, game asset**
--no solid edges, no organism, no creature, no bacteria, no tentacles, no clay texture, no defined shape, no biological";
        }
        
        return $@"MANDATORY STYLE GUIDE:
Material & Texture: Space phenomenon with appropriate appearance for type.
Lighting: Self-illuminating where appropriate. Dramatic space lighting.
Composition: Anomaly centered, showing its full effect.
Background: Solid black background (#000000) representing space.

{anomalyDetails}

Subject: {anomalyName}
Style: Sci-fi space anomaly for strategy game

**single space anomaly, game asset, space phenomenon**
--no spaceships, no planets unless specified, no text, no labels";
    }
    
    private string GetAnomalyDetails(string anomalyName)
    {
        var lowerName = anomalyName.ToLower();
        
        if (lowerName.Contains("nebula"))
        {
            var color = "colorful";
            if (lowerName.Contains("blue")) color = "blue and cyan";
            if (lowerName.Contains("red")) color = "red and orange";
            if (lowerName.Contains("green")) color = "green and teal";
            if (lowerName.Contains("purple")) color = "purple and violet";
            if (lowerName.Contains("dark")) color = "dark with faint red edges";
            
            return $@"NEBULA TYPE: {color} emission/reflection nebula.
- Vast cloud of glowing interstellar gas and dust
- Wispy, smoke-like, diffuse appearance
- Multiple layers of semi-transparent gas
- Soft glowing colors, NOT solid shapes
- Like colorful fog or smoke in space
- Reference: Real Hubble photos of nebulae";
        }
        if (lowerName.Contains("wormhole"))
            return "ANOMALY TYPE: Wormhole. Swirling vortex, tunnel through space. Glowing edges, dark center. Ring-shaped opening in space.";
        if (lowerName.Contains("black hole"))
            return "ANOMALY TYPE: Black Hole. Dark central void with bright accretion disk ring. Gravitational lensing distortion. Light bending around event horizon.";
        if (lowerName.Contains("ion storm") || lowerName.Contains("plasma storm"))
            return "ANOMALY TYPE: Energy Storm. Crackling energy, lightning-like discharges. Swirling dangerous cloud with electrical arcs.";
        if (lowerName.Contains("rift") || lowerName.Contains("subspace"))
            return "ANOMALY TYPE: Subspace Rift. Tear in space-time. Glowing crack or fissure. Energy leaking through from another dimension.";
        if (lowerName.Contains("asteroid"))
            return "ANOMALY TYPE: Asteroid Field. Many rocky bodies of various sizes. Dense or sparse depending on type. Navigation hazard.";
        if (lowerName.Contains("debris") || lowerName.Contains("wreck"))
            return "ANOMALY TYPE: Debris Field. Destroyed ships, stations, cargo. Floating wreckage. Signs of battle or disaster.";
        if (lowerName.Contains("gateway") || lowerName.Contains("iconian"))
            return "ANOMALY TYPE: Ancient Gateway. Mysterious alien structure. Glowing portal. Advanced technology beyond understanding.";
        if (lowerName.Contains("nexus"))
            return "ANOMALY TYPE: Nexus Ribbon. Swirling energy ribbon moving through space. Orange/yellow glow. Temporal properties.";
            
        return "ANOMALY TYPE: Mysterious space phenomenon with appropriate visual characteristics.";
    }
    
    private string BuildGalaxyTilePrompt(string tileName)
    {
        return $@"MANDATORY STYLE GUIDE:
Material & Texture: Clean digital render, smooth gradients. Professional strategy game map tile quality. Polished, not handmade.
Proportions: Square tile suitable for a hex or square grid map.
Lighting: FLAT, EVEN illumination - no directional shadows, no sun shadows. Uniform lighting across entire tile.
Composition: Top-down view of space sector.
Background: Deep space black with subtle star field.

Subject: Galaxy Map Tile - {tileName}
Style: Professional strategy game map tile (like Stellaris, Endless Space, Civilization)

CRITICAL:
- Clean, polished digital art style
- NO clay texture, NO soft organic look
- NO directional lighting or shadows
- Should tile seamlessly with other tiles
- Clear visual language for game UI

**single map tile, clean digital render, flat lighting, no shadows, strategy game asset, top-down view**
--no clay, no plasticine, no handmade, no soft edges, no directional shadows, 3D perspective, text, labels";
    }
    
    private string BuildSystemElementPrompt(string elementName)
    {
        var elementDetails = GetSystemElementDetails(elementName);
        
        return $@"MANDATORY STYLE GUIDE:
Material & Texture: Realistic space object render. Clean surfaces with appropriate textures (rocky, metallic, icy). Professional quality.
Proportions: Appropriate scale for the element type.
Lighting: FLAT, EVEN illumination - FULLY LIT with NO shadows, NO dark side. Entire object visible and evenly illuminated.
Composition: Element centered, isometric or 3/4 view.
Background: Solid black background (#000000).

{elementDetails}

Subject: {elementName}
Style: Realistic sci-fi space object for strategy game

CRITICAL LIGHTING:
- FULL even illumination - no shadows on object
- NO sun shadow, NO dark side
- We will add shadows dynamically in-game
- NO clay texture - realistic space object

**single space object, clean render, fully lit, no shadows, even illumination, game asset**
--no clay, no plasticine, no shadows, no dark side, no directional lighting, multiple objects, text, labels";
    }
    
    private string GetSystemElementDetails(string elementName)
    {
        var lowerName = elementName.ToLower();
        
        if (lowerName.Contains("asteroid"))
            return "ELEMENT TYPE: Asteroid. Irregular rocky body, cratered surface. Grey or brown coloring. Realistic rock texture.";
        if (lowerName.Contains("moon"))
            return "ELEMENT TYPE: Moon. Smaller spherical body. Cratered or smooth depending on type. Realistic lunar surface.";
        if (lowerName.Contains("comet"))
            return "ELEMENT TYPE: Comet. Icy body with glowing tail streaming behind. Blue-white ice core. NO shadow on comet itself.";
        if (lowerName.Contains("debris") || lowerName.Contains("wreck"))
            return "ELEMENT TYPE: Debris/Wreckage. Destroyed ship or station fragments. Twisted metal, floating in space. Realistic metal textures.";
        if (lowerName.Contains("satellite") || lowerName.Contains("buoy"))
            return "ELEMENT TYPE: Artificial Satellite. Small technological device. Antennas, solar panels, blinking lights. Clean metallic surfaces.";
        if (lowerName.Contains("mine"))
            return "ELEMENT TYPE: Space Mine. Dangerous explosive device. Spiky or spherical. Warning markings. Metallic finish.";
            
        return "ELEMENT TYPE: Space object with appropriate realistic characteristics.";
    }
    
    private string BuildEffectPrompt(FactionProfile profile, string effectName)
    {
        var effectDetails = GetEffectDetails(effectName, profile.Faction);
        
        return $@"MANDATORY STYLE GUIDE:
Material & Texture: Clean digital visual effect. Crisp energy/light rendering. Professional VFX quality like modern games or movies.
Proportions: Effect sized appropriately for its type.
Lighting: Self-illuminating for energy effects. Bright, vibrant colors.
Composition: Effect centered, clear visual read, suitable for overlay on game scene.
Background: Solid black background (#000000) - effect should work as transparent overlay.

{effectDetails}

Subject: {effectName}
Faction Style: {profile.Name} color scheme where applicable

CRITICAL:
- Clean, crisp digital VFX style
- NO clay texture, NO soft organic look
- Bright, vibrant, readable at small sizes
- Think: Star Trek movie/game visual effects
- Suitable for compositing as game overlay

**single visual effect, clean digital VFX, crisp edges, vibrant colors, game asset, transparent-ready**
--no clay, no plasticine, no soft edges, no handmade look, spaceships, characters, text, labels";
    }
    
    private string GetEffectDetails(string effectName, Faction faction)
    {
        var lowerName = effectName.ToLower();
        
        if (lowerName.Contains("phaser"))
            return "EFFECT TYPE: Phaser Beam. Orange-red continuous beam of energy. Federation style. Clean, precise beam with slight glow.";
        if (lowerName.Contains("disruptor"))
            return "EFFECT TYPE: Disruptor Beam. Green crackling energy beam. Klingon/Romulan style. Aggressive, powerful bolt.";
        if (lowerName.Contains("torpedo"))
            return "EFFECT TYPE: Torpedo. Glowing projectile with trail. Color varies: red photon, blue quantum, green plasma.";
        if (lowerName.Contains("shield"))
            return "EFFECT TYPE: Shield Effect. Bubble or impact flash of energy. Blue or faction-colored. Hexagonal pattern optional.";
        if (lowerName.Contains("explosion"))
            return "EFFECT TYPE: Explosion. Fiery blast with debris. Orange, yellow, white core. Expanding shockwave ring.";
        if (lowerName.Contains("warp"))
            return "EFFECT TYPE: Warp Effect. Stretched stars, flash of light, blue-white streak. Going to warp speed.";
        if (lowerName.Contains("transporter"))
            return "EFFECT TYPE: Transporter Beam. Sparkling column of light. Glittering particles. Federation: blue/white sparkles.";
        if (lowerName.Contains("cloak"))
            return "EFFECT TYPE: Cloaking Effect. Shimmering distortion, ripple effect. Semi-transparent. Romulan/Klingon technology.";
            
        return "EFFECT TYPE: Visual effect with crisp, clean energy/light characteristics.";
    }
}
