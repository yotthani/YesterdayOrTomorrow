interface StarSystem {
  id: string;
  name: string;
  x: number;
  y: number;
  starType?: string;
  factionId?: string;
  factionName?: string;
  hasColony?: boolean;
  hasFleet?: boolean;
  visibilityLevel?: number;  // 0=Unknown, 1=Detected, 2=Partial, 3=Full, 4=FogOfWar
}

interface Hyperlane {
  fromId: string;
  toId: string;
}

interface Fleet {
  systemId: string;
  destinationId?: string;
  actionPoints?: number;
  maxActionPoints?: number;
  combatStrength?: number;
  flagshipClass?: string;
}

interface AsteroidField {
  id?: string;
  x: number;
  y: number;
  radius?: number;
  density?: number;
}

interface StationMarker {
  systemId: string;
  factionId: string;
  name: string;
  isOwn: boolean;
}

interface StarColors {
  core: string;
  mid: string;
  outer: string;
  glow: string;
}

interface StarGridPos { row: number; col: number }

const STAR_COLORS: Readonly<Record<string, StarColors>> = {
  yellow:    { core: '#ffffff', mid: '#ffee88', outer: '#ffaa00', glow: '#ff880044' },
  orange:    { core: '#ffffff', mid: '#ffcc66', outer: '#ff6600', glow: '#ff440044' },
  red:       { core: '#ffdddd', mid: '#ff6666', outer: '#cc0000', glow: '#ff000033' },
  blue:      { core: '#ffffff', mid: '#aaddff', outer: '#4488ff', glow: '#0066ff44' },
  white:     { core: '#ffffff', mid: '#eeeeff', outer: '#ccccff', glow: '#ffffff33' },
  neutron:   { core: '#ffffff', mid: '#88ffff', outer: '#00cccc', glow: '#00ffff55' },
  blackhole: { core: '#000000', mid: '#220022', outer: '#440044', glow: '#ff00ff22' },
} as const;

const FACTION_COLORS: Readonly<Record<string, string>> = {
  federation: '#3b82f6',
  klingon:    '#dc2626',
  romulan:    '#10b981',
  cardassian: '#d97706',
  ferengi:    '#eab308',
  borg:       '#22c55e',
  dominion:   '#7c3aed',
} as const;

const STAR_TYPE_ALIASES: Readonly<Record<string, string>> = {
  mainsequence: 'yellow',  main_sequence: 'yellow',
  yellowdwarf:  'yellow',  yellow_dwarf:  'yellow',
  orangedwarf:  'orange',  orange_dwarf:  'orange',
  reddwarf:     'red',     red_dwarf:     'red',
  bluegiant:    'blue',    blue_giant:    'blue',
  redgiant:     'redgiant', red_giant:   'redgiant',
  whitedwarf:   'whitedwarf', white_dwarf: 'whitedwarf',
  browndwarf:   'browndwarf', brown_dwarf: 'browndwarf',
  neutronstar:  'neutron', neutron_star:  'neutron',
  pulsar:       'neutron',
  binarysystem: 'binary',  binary_system: 'binary',
  trinarysystem:'trinary', trinary_system:'trinary',
  class_g: 'yellow', class_k: 'orange', class_m: 'red',
  class_o: 'blue',   class_b: 'blue',   class_a: 'white', class_f: 'yellow',
} as const;

const STAR_GRID_MAP: Readonly<Record<string, StarGridPos>> = {
  yellow:       { row: 0, col: 0 }, orange:       { row: 0, col: 1 },
  red:          { row: 0, col: 2 }, blue:         { row: 0, col: 3 },
  redgiant:     { row: 1, col: 0 }, bluesupergiant:{ row: 1, col: 1 },
  white:        { row: 1, col: 2 }, orangegiant:  { row: 1, col: 3 },
  neutron:      { row: 2, col: 0 }, blackhole:    { row: 2, col: 1 },
  whitedwarf:   { row: 2, col: 2 }, browndwarf:   { row: 2, col: 3 },
  binary:       { row: 3, col: 0 }, trinary:      { row: 3, col: 1 },
  protostar:    { row: 3, col: 2 }, supernova:    { row: 3, col: 3 },
} as const;

export class GalaxyRenderer {
  private container: HTMLElement;

  private bgCanvas!: HTMLCanvasElement;
  private bgCtx!: CanvasRenderingContext2D;
  private mainCanvas!: HTMLCanvasElement;
  private mainCtx!: CanvasRenderingContext2D;
  private uiCanvas!: HTMLCanvasElement;
  private uiCtx!: CanvasRenderingContext2D;

  private viewX = 0;
  private viewY = 0;
  private zoom = 1;
  private targetZoom = 1;
  private readonly minZoom = 0.2;
  private readonly maxZoom = 4;

  private isDragging = false;
  private lastMouseX = 0;
  private lastMouseY = 0;
  private velocity = { x: 0, y: 0 };

  private systems: StarSystem[] = [];
  private hyperlanes: Hyperlane[] = [];
  private fleets: Fleet[] = [];
  asteroidFields: AsteroidField[] = [];
  private stations: StationMarker[] = [];

  private selectedSystemId: string | null = null;
  private hoveredSystemId: string | null = null;

  private assets: {
    stars: Record<string, HTMLCanvasElement | HTMLImageElement>;
    nebulae: HTMLCanvasElement[];
    icons: Record<string, HTMLCanvasElement>;
  } = { stars: {}, nebulae: [], icons: {} };

  onSystemSelected: ((system: StarSystem | null) => void) | null = null;
  onSystemHovered:  ((system: StarSystem | null) => void) | null = null;

  private animationFrame: number | null = null;
  private lastFrameTime = 0;

  private starSpritesheet: HTMLImageElement | null = null;
  private starCellSize = 360;
  private useStarSpritesheet = false;

  private _debugCount = 0;

  constructor(containerId: string) {
    const el = document.getElementById(containerId);
    if (!el) {
      console.error('Galaxy container not found:', containerId);
      throw new Error(`Container #${containerId} not found`);
    }
    this.container = el;
    this.setupCanvasLayers();
    this.setupEventListeners();
    void this.loadAssets().then(() => this.startRenderLoop());
  }

  private setupCanvasLayers(): void {
    this.container.style.position = 'relative';
    this.container.style.overflow = 'hidden';
    const width  = this.container.clientWidth;
    const height = this.container.clientHeight;
    this.bgCanvas   = this.createCanvas('galaxy-bg',   width, height, 1);
    this.bgCtx      = this.bgCanvas.getContext('2d')!;
    this.mainCanvas = this.createCanvas('galaxy-main', width, height, 2);
    this.mainCtx    = this.mainCanvas.getContext('2d')!;
    this.uiCanvas   = this.createCanvas('galaxy-ui',   width, height, 3);
    this.uiCtx      = this.uiCanvas.getContext('2d')!;
  }

  private createCanvas(id: string, width: number, height: number, zIndex: number): HTMLCanvasElement {
    const canvas = document.createElement('canvas');
    canvas.id = id;
    canvas.width  = width;
    canvas.height = height;
    canvas.style.position = 'absolute';
    canvas.style.left   = '0';
    canvas.style.top    = '0';
    canvas.style.zIndex = String(zIndex);
    this.container.appendChild(canvas);
    return canvas;
  }

  async loadAssets(): Promise<void> {
    try {
      await this.loadStarSpritesheet();
      console.log('Star spritesheet loaded');
    } catch {
      const starTypes = ['yellow', 'orange', 'red', 'blue', 'white', 'neutron', 'blackhole'] as const;
      for (const type of starTypes) {
        this.assets.stars[type] = this.generateStarImage(type);
      }
    }
    for (let i = 0; i < 5; i++) this.assets.nebulae.push(this.generateNebulaImage(i));
    this.assets.icons['colony']   = this.generateIcon('colony');
    this.assets.icons['fleet']    = this.generateIcon('fleet');
    this.assets.icons['starbase'] = this.generateIcon('starbase');
    console.log('Galaxy assets loaded');
  }

  private async loadStarSpritesheet(): Promise<void> {
    this.starSpritesheet = new Image();
    this.starSpritesheet.crossOrigin = 'anonymous';
    this.starSpritesheet.src = '/assets/universal/stars_spritesheet.png';
    await new Promise<void>((resolve, reject) => {
      this.starSpritesheet!.onload  = () => resolve();
      this.starSpritesheet!.onerror = reject;
    });
    this.starCellSize = 360;
    this.useStarSpritesheet = true;
  }

  private getStarGridPosition(starType: string, systemName: string): StarGridPos {
    let type = (starType || 'yellow').toLowerCase();
    if (type === 'unknown' || type === '' || type === 'undefined') {
      const fallbacks = ['yellow', 'orange', 'red', 'blue', 'white'];
      const hash = (systemName || 'default').split('').reduce((a, c) => a + c.charCodeAt(0), 0);
      type = fallbacks[hash % fallbacks.length];
    }
    const mapped = STAR_TYPE_ALIASES[type] ?? type;
    return STAR_GRID_MAP[mapped] ?? STAR_GRID_MAP['yellow']!;
  }

  private generateStarImage(type: string): HTMLCanvasElement {
    const size = 64;
    const canvas = document.createElement('canvas');
    canvas.width = canvas.height = size;
    const ctx = canvas.getContext('2d')!;
    const c = STAR_COLORS[type] ?? STAR_COLORS['yellow']!;
    const center = size / 2;

    const glowGrad = ctx.createRadialGradient(center, center, 0, center, center, size / 2);
    glowGrad.addColorStop(0, c.glow);
    glowGrad.addColorStop(1, 'transparent');
    ctx.fillStyle = glowGrad;
    ctx.fillRect(0, 0, size, size);

    const starGrad = ctx.createRadialGradient(center, center, 0, center, center, size / 4);
    starGrad.addColorStop(0, c.core);
    starGrad.addColorStop(0.3, c.mid);
    starGrad.addColorStop(1, c.outer);
    ctx.beginPath();
    ctx.arc(center, center, size / 4, 0, Math.PI * 2);
    ctx.fillStyle = starGrad;
    ctx.fill();

    if (type !== 'blackhole' && type !== 'red') {
      ctx.globalAlpha  = 0.3;
      ctx.strokeStyle  = c.mid;
      ctx.lineWidth    = 1;
      ctx.beginPath(); ctx.moveTo(center - size / 2.5, center); ctx.lineTo(center + size / 2.5, center); ctx.stroke();
      ctx.beginPath(); ctx.moveTo(center, center - size / 2.5); ctx.lineTo(center, center + size / 2.5); ctx.stroke();
      ctx.globalAlpha = 1;
    }

    if (type === 'blackhole') {
      ctx.strokeStyle  = '#ff66ff';
      ctx.lineWidth    = 2;
      ctx.globalAlpha  = 0.5;
      ctx.beginPath();
      ctx.ellipse(center, center, size / 3, size / 6, Math.PI / 6, 0, Math.PI * 2);
      ctx.stroke();
      ctx.globalAlpha = 1;
    }

    return canvas;
  }

  private generateNebulaImage(seed: number): HTMLCanvasElement {
    const size = 512;
    const canvas = document.createElement('canvas');
    canvas.width = canvas.height = size;
    const ctx = canvas.getContext('2d')!;
    const hues = [200, 280, 320, 180, 30];
    const hue  = hues[seed % hues.length]!;
    const imageData = ctx.createImageData(size, size);
    const data = imageData.data;

    const noise = (x: number, y: number, scale: number): number =>
      (Math.sin(x * scale * 12.9898 + y * scale * 78.233 + seed) * 43758.5453) % 1;

    const hue2rgb = (p: number, q: number, t: number): number => {
      let tt = t;
      if (tt < 0) tt += 1;
      if (tt > 1) tt -= 1;
      if (tt < 1 / 6) return p + (q - p) * 6 * tt;
      if (tt < 1 / 2) return q;
      if (tt < 2 / 3) return p + (q - p) * (2 / 3 - tt) * 6;
      return p;
    };

    for (let y = 0; y < size; y++) {
      for (let x = 0; x < size; x++) {
        const i = (y * size + x) * 4;
        let n = Math.abs(noise(x, y, 0.01) * 0.5 + noise(x, y, 0.02) * 0.25 + noise(x, y, 0.04) * 0.125);
        const dx = (x - size / 2) / size;
        const dy = (y - size / 2) / size;
        n *= Math.max(0, 1 - Math.sqrt(dx * dx + dy * dy) * 1.5);

        const h = hue / 360, s = 0.6, l = n * 0.3;
        const q = l < 0.5 ? l * (1 + s) : l + s - l * s;
        const p = 2 * l - q;
        data[i]     = hue2rgb(p, q, h + 1 / 3) * 255;
        data[i + 1] = hue2rgb(p, q, h)         * 255;
        data[i + 2] = hue2rgb(p, q, h - 1 / 3) * 255;
        data[i + 3] = n * 100;
      }
    }

    ctx.putImageData(imageData, 0, 0);
    return canvas;
  }

  private generateIcon(type: 'colony' | 'fleet' | 'starbase'): HTMLCanvasElement {
    const size = 32;
    const canvas = document.createElement('canvas');
    canvas.width = canvas.height = size;
    const ctx = canvas.getContext('2d')!;
    const center = size / 2;
    ctx.shadowColor = '#000';
    ctx.shadowBlur  = 4;

    if (type === 'colony') {
      ctx.fillStyle   = '#44ff44';
      ctx.strokeStyle = '#228822';
      ctx.lineWidth   = 2;
      ctx.beginPath(); ctx.roundRect(center - 8, center - 6, 16, 12, 2); ctx.fill(); ctx.stroke();
      ctx.beginPath(); ctx.arc(center, center - 6, 6, Math.PI, 0); ctx.fill(); ctx.stroke();
    } else if (type === 'fleet') {
      ctx.fillStyle   = '#ffaa00';
      ctx.strokeStyle = '#886600';
      ctx.lineWidth   = 2;
      ctx.beginPath();
      ctx.moveTo(center, center - 10);
      ctx.lineTo(center + 8, center + 8);
      ctx.lineTo(center - 8, center + 8);
      ctx.closePath();
      ctx.fill(); ctx.stroke();
    } else {
      ctx.fillStyle   = '#8888ff';
      ctx.strokeStyle = '#4444aa';
      ctx.lineWidth   = 2;
      ctx.beginPath();
      for (let i = 0; i < 6; i++) {
        const a = (i * 60 - 30) * Math.PI / 180;
        const x = center + Math.cos(a) * 10;
        const y = center + Math.sin(a) * 10;
        i === 0 ? ctx.moveTo(x, y) : ctx.lineTo(x, y);
      }
      ctx.closePath();
      ctx.fill(); ctx.stroke();
    }

    return canvas;
  }

  private setupEventListeners(): void {
    const c = this.uiCanvas;
    c.addEventListener('mousedown',  (e) => this.onMouseDown(e));
    c.addEventListener('mousemove',  (e) => this.onMouseMove(e));
    c.addEventListener('mouseup',    (e) => this.onMouseUp(e));
    c.addEventListener('mouseleave', (e) => this.onMouseUp(e));
    c.addEventListener('wheel',      (e) => this.onWheel(e), { passive: false });
    c.addEventListener('click',      (e) => this.onClick(e));
    window.addEventListener('resize', () => this.onResize());
  }

  private onMouseDown(e: MouseEvent): void {
    this.isDragging  = true;
    this.lastMouseX  = e.clientX;
    this.lastMouseY  = e.clientY;
    this.velocity    = { x: 0, y: 0 };
  }

  private onMouseMove(e: MouseEvent): void {
    const rect   = this.container.getBoundingClientRect();
    const mouseX = e.clientX - rect.left;
    const mouseY = e.clientY - rect.top;

    if (this.isDragging) {
      const dx = e.clientX - this.lastMouseX;
      const dy = e.clientY - this.lastMouseY;
      this.viewX    -= dx / this.zoom;
      this.viewY    -= dy / this.zoom;
      this.velocity  = { x: dx, y: dy };
      this.lastMouseX = e.clientX;
      this.lastMouseY = e.clientY;
    } else {
      this.checkSystemHover(mouseX, mouseY);
    }
  }

  private onMouseUp(_e: MouseEvent): void {
    this.isDragging = false;
  }

  private onWheel(e: WheelEvent): void {
    e.preventDefault();
    const rect   = this.container.getBoundingClientRect();
    const mouseX = e.clientX - rect.left;
    const mouseY = e.clientY - rect.top;
    const worldX = this.screenToWorldX(mouseX);
    const worldY = this.screenToWorldY(mouseY);
    const factor = e.deltaY > 0 ? 0.9 : 1.1;
    this.targetZoom = Math.max(this.minZoom, Math.min(this.maxZoom, this.zoom * factor));
    this.viewX += worldX - this.screenToWorldX(mouseX);
    this.viewY += worldY - this.screenToWorldY(mouseY);
  }

  private onClick(e: MouseEvent): void {
    const rect   = this.container.getBoundingClientRect();
    const mouseX = e.clientX - rect.left;
    const mouseY = e.clientY - rect.top;
    const system = this.getSystemAtPosition(mouseX, mouseY);
    this.selectedSystemId = system?.id ?? null;
    this.onSystemSelected?.(system ?? null);
  }

  private onResize(): void {
    const w = this.container.clientWidth;
    const h = this.container.clientHeight;
    for (const c of [this.bgCanvas, this.mainCanvas, this.uiCanvas]) {
      c.width = w;
      c.height = h;
    }
  }

  private screenToWorldX(screenX: number): number {
    return this.viewX + (screenX - this.mainCanvas.width  / 2) / this.zoom;
  }
  private screenToWorldY(screenY: number): number {
    return this.viewY + (screenY - this.mainCanvas.height / 2) / this.zoom;
  }
  private worldToScreenX(worldX: number): number {
    return (worldX - this.viewX) * this.zoom + this.mainCanvas.width  / 2;
  }
  private worldToScreenY(worldY: number): number {
    return (worldY - this.viewY) * this.zoom + this.mainCanvas.height / 2;
  }

  setSystems(systems: StarSystem[]): void {
    console.log('🌟 Galaxy: Setting systems:', systems?.length ?? 0);
    this.systems = systems ?? [];
    if (this.systems.length > 0) {
      this.calculateBounds();
    }
  }

  setHyperlanes(hyperlanes: Hyperlane[]): void {
    console.log('🔗 Galaxy: Setting hyperlanes:', hyperlanes?.length ?? 0);
    this.hyperlanes = hyperlanes ?? [];
  }

  setFleets(fleets: Fleet[]): void {
    this.fleets = fleets;
  }

  public setStations(stations: StationMarker[]): void {
    this.stations = stations;
    this.render();
  }

  private calculateBounds(): void {
    if (this.systems.length === 0) return;
    let minX = Infinity, maxX = -Infinity, minY = Infinity, maxY = -Infinity;
    for (const s of this.systems) {
      minX = Math.min(minX, s.x); maxX = Math.max(maxX, s.x);
      minY = Math.min(minY, s.y); maxY = Math.max(maxY, s.y);
    }
    this.viewX  = (minX + maxX) / 2;
    this.viewY  = (minY + maxY) / 2;
  }

  private checkSystemHover(screenX: number, screenY: number): void {
    const system = this.getSystemAtPosition(screenX, screenY);
    const newId  = system?.id ?? null;
    if (newId !== this.hoveredSystemId) {
      this.hoveredSystemId = newId;
      this.onSystemHovered?.(system ?? null);
    }
  }

  private getSystemAtPosition(screenX: number, screenY: number): StarSystem | undefined {
    const hitRadius = 20 / this.zoom;
    const worldX = this.screenToWorldX(screenX);
    const worldY = this.screenToWorldY(screenY);
    return this.systems.find(s => {
      const dx = s.x - worldX, dy = s.y - worldY;
      return dx * dx + dy * dy < hitRadius * hitRadius;
    });
  }

  private startRenderLoop(): void {
    const render = (timestamp: number) => {
      const dt = (timestamp - this.lastFrameTime) / 1000;
      this.lastFrameTime = timestamp;
      this.update(dt);
      this.render();
      this.animationFrame = requestAnimationFrame(render);
    };
    this.animationFrame = requestAnimationFrame(render);
  }

  stopRenderLoop(): void {
    if (this.animationFrame !== null) cancelAnimationFrame(this.animationFrame);
  }

  private update(_dt: number): void {
    this.zoom += (this.targetZoom - this.zoom) * 0.1;
    if (!this.isDragging && (Math.abs(this.velocity.x) > 0.1 || Math.abs(this.velocity.y) > 0.1)) {
      this.viewX    -= this.velocity.x / this.zoom * 0.3;
      this.viewY    -= this.velocity.y / this.zoom * 0.3;
      this.velocity.x *= 0.95;
      this.velocity.y *= 0.95;
    }
  }

  private render(): void {
    this.renderBackground();
    this.renderMain();
    this.renderUI();
  }

  private renderBackground(): void {
    const ctx = this.bgCtx, w = this.bgCanvas.width, h = this.bgCanvas.height;
    ctx.fillStyle = '#050510';
    ctx.fillRect(0, 0, w, h);

    ctx.fillStyle = 'rgba(255,255,255,0.5)';
    for (let i = 0; i < 200; i++) {
      const x = (i * 137.5) % w, y = (i * 97.3) % h;
      ctx.beginPath();
      ctx.arc(x, y, (i % 3) * 0.5 + 0.5, 0, Math.PI * 2);
      ctx.fill();
    }

    this.assets.nebulae.forEach((nebula, i) => {
      const parallax = 0.1 + i * 0.05;
      const x = ((-this.viewX * parallax) % 512) - 256;
      const y = ((-this.viewY * parallax) % 512) - 256;
      ctx.globalAlpha = 0.3;
      for (let tx = -1; tx <= Math.ceil(w / 512) + 1; tx++) {
        for (let ty = -1; ty <= Math.ceil(h / 512) + 1; ty++) {
          ctx.drawImage(nebula, x + tx * 512, y + ty * 512);
        }
      }
    });
    ctx.globalAlpha = 1;
  }

  private renderMain(): void {
    const ctx = this.mainCtx, w = this.mainCanvas.width, h = this.mainCanvas.height;
    ctx.clearRect(0, 0, w, h);
    this.renderAsteroidFields(ctx);
    this.renderHyperlanes(ctx);
    this.renderTerritories(ctx);
    this.renderSystems(ctx);
    this.renderFleets(ctx);
    this.renderStations(ctx);
  }

  private renderHyperlanes(ctx: CanvasRenderingContext2D): void {
    for (const lane of this.hyperlanes) {
      const from = this.systems.find(s => s.id === lane.fromId);
      const to   = this.systems.find(s => s.id === lane.toId);
      if (!from || !to) continue;

      const fromVis = from.visibilityLevel ?? 3;
      const toVis = to.visibilityLevel ?? 3;
      if (fromVis === 0 || toVis === 0) continue;

      const x1 = this.worldToScreenX(from.x), y1 = this.worldToScreenY(from.y);
      const x2 = this.worldToScreenX(to.x),   y2 = this.worldToScreenY(to.y);
      let color = '#334466';
      if (from.factionId && from.factionId === to.factionId) {
        color = this.getFactionColor(from.factionId);
      }

      const minVis = Math.min(fromVis, toVis);
      if (minVis <= 1) {
        ctx.globalAlpha = 0.15;
        color = '#334455';
      } else if (minVis === 4) {
        ctx.globalAlpha = 0.25;
      } else {
        ctx.globalAlpha = 0.4;
      }

      ctx.strokeStyle = color;
      ctx.lineWidth   = this.zoom > 0.5 ? 2 : 1;
      ctx.beginPath();
      ctx.moveTo(x1, y1);
      ctx.lineTo(x2, y2);
      ctx.stroke();
    }
    ctx.globalAlpha = 1;
  }

  private renderTerritories(ctx: CanvasRenderingContext2D): void {
    for (const system of this.systems) {
      if (!system.factionId) continue;
      const x = this.worldToScreenX(system.x), y = this.worldToScreenY(system.y);
      const r = 50 * this.zoom;
      const color = this.getFactionColor(system.factionId);
      const grad  = ctx.createRadialGradient(x, y, 0, x, y, r);
      grad.addColorStop(0, color + '44');
      grad.addColorStop(1, color + '00');
      ctx.fillStyle = grad;
      ctx.beginPath(); ctx.arc(x, y, r, 0, Math.PI * 2); ctx.fill();
    }
  }

  private renderAsteroidFields(ctx: CanvasRenderingContext2D): void {
    for (const field of this.asteroidFields) {
      const cx     = this.worldToScreenX(field.x);
      const cy     = this.worldToScreenY(field.y);
      const radius = (field.radius ?? 40) * this.zoom;
      const w = this.mainCanvas.width, h = this.mainCanvas.height;
      if (cx < -radius || cx > w + radius || cy < -radius || cy > h + radius) continue;

      const density = field.density ?? 0.5;
      const grad = ctx.createRadialGradient(cx, cy, 0, cx, cy, radius);
      grad.addColorStop(0,   `rgba(140,110,70,${0.25 * density})`);
      grad.addColorStop(0.5, `rgba(120,90,50,${0.15 * density})`);
      grad.addColorStop(1,   'rgba(100,80,40,0)');
      ctx.fillStyle = grad;
      ctx.beginPath(); ctx.arc(cx, cy, radius, 0, Math.PI * 2); ctx.fill();

      if (this.zoom > 1.2) this.renderAsteroidDots(ctx, field, cx, cy, radius);
    }
  }

  private renderAsteroidDots(
    ctx: CanvasRenderingContext2D,
    field: AsteroidField, cx: number, cy: number, radius: number,
  ): void {
    const seed  = field.id ? field.id.charCodeAt(0) : 42;
    const count = Math.floor((field.density ?? 0.5) * 60);
    for (let i = 0; i < count; i++) {
      const angle = i * 2.39996;
      const dist  = Math.sqrt(i / count) * radius * 0.9;
      const ax = cx + Math.cos(angle) * dist;
      const ay = cy + Math.sin(angle) * dist;
      const size = 1 + ((seed + i * 7) % 4) * this.zoom * 0.5;
      const b = 100 + ((seed + i * 13) % 80);
      ctx.fillStyle = `rgb(${b},${Math.round(b * 0.75)},${Math.round(b * 0.5)})`;
      ctx.beginPath(); ctx.arc(ax, ay, size, 0, Math.PI * 2); ctx.fill();
    }
  }

  private renderSystems(ctx: CanvasRenderingContext2D): void {
    for (const system of this.systems) {
      const x = this.worldToScreenX(system.x), y = this.worldToScreenY(system.y);
      const w = this.mainCanvas.width, h = this.mainCanvas.height;
      if (x < -50 || x > w + 50 || y < -50 || y > h + 50) continue;

      const visLevel = system.visibilityLevel ?? 3; // default Full
      if (visLevel === 0) continue; // Unknown = skip

      // Set alpha based on visibility
      const alphaMap: Record<number, number> = { 1: 0.3, 2: 0.6, 3: 1.0, 4: 0.5 };
      ctx.globalAlpha = alphaMap[visLevel] ?? 1.0;

      const isSelected = system.id === this.selectedSystemId;
      const isHovered  = system.id === this.hoveredSystemId;
      const starSize   = (isSelected || isHovered ? 40 : 32) * this.zoom;
      const starType   = system.starType ?? 'yellow';

      if (this._debugCount < 5) {
        console.log(`🌟 ${system.name}: type="${starType}" spritesheet=${this.useStarSpritesheet}`);
        this._debugCount++;
      }

      if (visLevel <= 1) {
        // Detected: simple gray circle
        ctx.fillStyle = '#556677';
        ctx.beginPath();
        ctx.arc(x, y, starSize / 4, 0, Math.PI * 2);
        ctx.fill();
      } else {
        // Normal star sprite rendering
        if (this.useStarSpritesheet && this.starSpritesheet?.complete) {
          const gp = this.getStarGridPosition(starType, system.name);
          ctx.drawImage(
            this.starSpritesheet,
            gp.col * this.starCellSize, gp.row * this.starCellSize, this.starCellSize, this.starCellSize,
            x - starSize / 2, y - starSize / 2, starSize, starSize,
          );
        } else {
          let pt = starType;
          if (pt === 'unknown' || !this.assets.stars[pt]) {
            const types = ['yellow', 'orange', 'red', 'blue', 'white'];
            const hash = (system.name || 'default').split('').reduce((a, c) => a + c.charCodeAt(0), 0);
            pt = types[hash % types.length]!;
          }
          const img = this.assets.stars[pt] ?? this.assets.stars['yellow'];
          if (img) ctx.drawImage(img, x - starSize / 2, y - starSize / 2, starSize, starSize);
        }
      }

      if (system.factionId) {
        ctx.strokeStyle = this.getFactionColor(system.factionId);
        ctx.lineWidth = 2;
        ctx.beginPath(); ctx.arc(x, y, starSize / 2 + 4, 0, Math.PI * 2); ctx.stroke();
      }

      if (visLevel === 4) {
        ctx.strokeStyle = '#887766';
        ctx.lineWidth = 1;
        ctx.setLineDash([4, 4]);
        ctx.beginPath();
        ctx.arc(x, y, starSize / 2 + 6, 0, Math.PI * 2);
        ctx.stroke();
        ctx.setLineDash([]);
      }

      if (isSelected) {
        ctx.strokeStyle = '#ffcc00';
        ctx.lineWidth   = 2;
        ctx.setLineDash([6, 4]);
        ctx.beginPath(); ctx.arc(x, y, starSize / 2 + 10, 0, Math.PI * 2); ctx.stroke();
        ctx.setLineDash([]);
      }

      const colonyIcon = this.assets.icons['colony'];
      if (system.hasColony && colonyIcon) {
        ctx.drawImage(colonyIcon, x + starSize / 2 - 8, y - starSize / 2 - 8, 20, 20);
      }
      const fleetIcon = this.assets.icons['fleet'];
      if (system.hasFleet && fleetIcon) {
        ctx.drawImage(fleetIcon, x - starSize / 2 - 12, y - starSize / 2 - 8, 20, 20);
      }

      if ((this.zoom > 0.6 || isSelected || isHovered) && visLevel >= 2) {
        ctx.fillStyle = isSelected ? '#ffcc00' : (isHovered ? '#ffffff' : '#aabbcc');
        ctx.font      = `${Math.max(10, 12 * this.zoom)}px 'Orbitron', sans-serif`;
        ctx.textAlign = 'center';
        ctx.fillText(system.name, x, y + starSize / 2 + 14);
      }

      ctx.globalAlpha = 1.0;
    }
  }

  private renderFleets(ctx: CanvasRenderingContext2D): void {
    // Moving fleet travel lines
    for (const fleet of this.fleets) {
      if (!fleet.destinationId) continue;
      const sys  = this.systems.find(s => s.id === fleet.systemId);
      const dest = this.systems.find(s => s.id === fleet.destinationId);
      if (!sys || !dest) continue;
      ctx.strokeStyle = '#ffaa00';
      ctx.lineWidth   = 2;
      ctx.setLineDash([8, 4]);
      ctx.beginPath();
      ctx.moveTo(this.worldToScreenX(sys.x),  this.worldToScreenY(sys.y));
      ctx.lineTo(this.worldToScreenX(dest.x), this.worldToScreenY(dest.y));
      ctx.stroke();
      ctx.setLineDash([]);
    }

    // Fleet HUD — AP bar + flagship label + strength badge (per system)
    if (this.zoom < 0.4) return;
    const fleetsPerSystem = new Map<string, typeof this.fleets>();
    for (const fleet of this.fleets) {
      const key = fleet.systemId;
      if (!fleetsPerSystem.has(key)) fleetsPerSystem.set(key, []);
      fleetsPerSystem.get(key)!.push(fleet);
    }

    for (const [sysId, sysFleets] of fleetsPerSystem) {
      const sys = this.systems.find(s => s.id === sysId);
      if (!sys) continue;
      const sx = this.worldToScreenX(sys.x);
      const sy = this.worldToScreenY(sys.y);
      let yOffset = -28;

      for (const fleet of sysFleets) {
        const ap    = fleet.actionPoints    ?? 3;
        const maxAp = fleet.maxActionPoints ?? 3;
        const cs    = fleet.combatStrength  ?? 0;
        const fc    = fleet.flagshipClass;

        // Flagship class label
        if (fc) {
          ctx.font      = `bold ${Math.max(8, 9 * this.zoom)}px 'Orbitron',sans-serif`;
          ctx.textAlign = 'center';
          ctx.fillStyle = '#ffcc66';
          ctx.fillText(fc.toUpperCase(), sx, sy + yOffset);
          yOffset -= 12;
        }

        // Action Point pips
        const pipR  = Math.max(2.5, 3.5 * this.zoom);
        const pipGap = pipR * 2.8;
        const pipStartX = sx - ((maxAp - 1) / 2) * pipGap;
        for (let i = 0; i < maxAp; i++) {
          const px = pipStartX + i * pipGap;
          const py = sy + yOffset;
          ctx.beginPath();
          ctx.arc(px, py, pipR, 0, Math.PI * 2);
          ctx.fillStyle   = i < ap ? '#ffcc00' : 'rgba(255,204,0,0.2)';
          ctx.strokeStyle = '#aa8800';
          ctx.lineWidth   = 1;
          ctx.fill();
          ctx.stroke();
        }
        yOffset -= 12;

        // Combat strength badge (only if we have data)
        if (cs > 0 && this.zoom > 0.6) {
          const label = cs >= 1000 ? `${(cs / 1000).toFixed(1)}k` : String(cs);
          ctx.font      = `${Math.max(8, 9 * this.zoom)}px 'Orbitron',sans-serif`;
          const tw      = ctx.measureText(`⚔ ${label}`).width + 8;
          const bx      = sx - tw / 2;
          const by      = sy + yOffset - 10;
          ctx.fillStyle   = 'rgba(10,15,30,0.75)';
          ctx.strokeStyle = '#cc4444';
          ctx.lineWidth   = 1;
          ctx.beginPath();
          ctx.roundRect(bx, by, tw, 13, 3);
          ctx.fill(); ctx.stroke();
          ctx.fillStyle = '#ff6666';
          ctx.textAlign = 'center';
          ctx.fillText(`⚔ ${label}`, sx, by + 10);
          yOffset -= 16;
        }
      }
    }
  }

  private renderStations(ctx: CanvasRenderingContext2D): void {
    for (const station of this.stations) {
      const system = this.systems.find(s => s.id === station.systemId);
      if (!system) continue;

      const visLevel = system.visibilityLevel ?? 3;
      if (!station.isOwn && visLevel < 2) continue;

      const x = this.worldToScreenX(system.x);
      const y = this.worldToScreenY(system.y);
      const w = this.mainCanvas.width, h = this.mainCanvas.height;
      if (x < -20 || x > w + 20 || y < -20 || y > h + 20) continue;

      const sz = 6 * this.zoom;
      const ox = sz + 8 * this.zoom;  // offset right of star
      const oy = -sz;  // offset above center

      // Diamond shape
      const color = station.isOwn ? '#00ccff' : this.getFactionColor(station.factionId);
      ctx.fillStyle = color;
      ctx.globalAlpha = station.isOwn ? 1.0 : 0.7;
      ctx.beginPath();
      ctx.moveTo(x + ox, y + oy - sz);
      ctx.lineTo(x + ox + sz, y + oy);
      ctx.lineTo(x + ox, y + oy + sz);
      ctx.lineTo(x + ox - sz, y + oy);
      ctx.closePath();
      ctx.fill();

      // Label on zoom
      if (this.zoom > 1.2 && station.isOwn) {
        ctx.fillStyle = '#88ccff';
        ctx.font = `${Math.max(8, 9 * this.zoom)}px 'Orbitron', sans-serif`;
        ctx.textAlign = 'left';
        ctx.fillText(station.name, x + ox + sz + 4, y + oy + 3);
      }

      ctx.globalAlpha = 1.0;
    }
  }

  private renderUI(): void {
    const ctx = this.uiCtx, w = this.uiCanvas.width, h = this.uiCanvas.height;
    ctx.clearRect(0, 0, w, h);
    if (!this.hoveredSystemId) return;
    const system = this.systems.find(s => s.id === this.hoveredSystemId);
    if (system) this.renderTooltip(ctx, system, this.worldToScreenX(system.x), this.worldToScreenY(system.y) - 50);
  }

  private renderTooltip(ctx: CanvasRenderingContext2D, system: StarSystem, x: number, y: number): void {
    const padding = 10, lh = 18;
    const lines = [
      system.name,
      `Star: ${system.starType ?? 'Unknown'}`,
      system.factionId ? `Owner: ${system.factionName ?? 'Unknown'}` : 'Unclaimed',
      system.hasColony ? '🏠 Colony' : '',
      system.hasFleet  ? '🚀 Fleet Present' : '',
    ].filter(Boolean) as string[];

    const maxWidth = Math.max(...lines.map(l => ctx.measureText(l).width)) + padding * 2;
    const boxH     = lines.length * lh + padding * 2;
    const cx       = Math.max(maxWidth / 2, Math.min(this.uiCanvas.width - maxWidth / 2, x));
    const cy       = Math.max(boxH, y);

    ctx.fillStyle   = 'rgba(10,15,30,0.9)';
    ctx.strokeStyle = '#446688';
    ctx.lineWidth   = 1;
    ctx.beginPath();
    ctx.roundRect(cx - maxWidth / 2, cy - boxH, maxWidth, boxH, 6);
    ctx.fill(); ctx.stroke();

    ctx.textAlign = 'center';
    lines.forEach((line, i) => {
      const ty = cy - boxH + padding + (i + 1) * lh - 4;
      if (i === 0) { ctx.fillStyle = '#ffcc00'; ctx.font = 'bold 14px "Orbitron",sans-serif'; }
      else         { ctx.fillStyle = '#aabbcc'; ctx.font = '12px "Roboto",sans-serif'; }
      ctx.fillText(line, cx, ty);
    });
  }

  private getFactionColor(factionId: string): string {
    const idLower = factionId.toLowerCase();
    for (const [key, color] of Object.entries(FACTION_COLORS)) {
      if (idLower.includes(key)) return color;
    }
    let hash = 0;
    for (let i = 0; i < factionId.length; i++) {
      hash = factionId.charCodeAt(i) + ((hash << 5) - hash);
    }
    const r = (hash & 0xff0000) >> 16;
    const g = (hash & 0x00ff00) >>  8;
    const b =  hash & 0x0000ff;
    return `#${r.toString(16).padStart(2,'0')}${g.toString(16).padStart(2,'0')}${b.toString(16).padStart(2,'0')}`;
  }

  centerOnSystem(systemId: string): void {
    const s = this.systems.find(x => x.id === systemId);
    if (s) { this.viewX = s.x; this.viewY = s.y; this.targetZoom = 1.5; }
  }

  setZoom(level: number): void {
    this.targetZoom = Math.max(this.minZoom, Math.min(this.maxZoom, level));
  }

  resetView(): void {
    this.calculateBounds();
    this.targetZoom = 1;
  }

  destroy(): void {
    this.stopRenderLoop();
    this.container.innerHTML = '';
  }
}

window.GalaxyRenderer = GalaxyRenderer;
window.galaxyRenderer = null;

window.initGalaxyMap = async (containerId: string): Promise<boolean> => {
  window.galaxyRenderer?.destroy();
  window.galaxyRenderer = new GalaxyRenderer(containerId);
  console.log('🌌 Galaxy renderer initialized, loading assets...');
  await window.galaxyRenderer.loadAssets();
  console.log('🌌 Galaxy renderer assets loaded');
  return true;
};

window.setGalaxySystems = (systemsJson: string): void => {
  if (!window.galaxyRenderer) { console.error('🌟 No galaxy renderer!'); return; }
  const systems = JSON.parse(systemsJson) as StarSystem[];
  console.log('🌟 Parsed systems:', systems?.length ?? 0);
  if (systems?.length > 0) {
    console.log('🌟 Star types:', systems.slice(0, 10).map(s => `${s.name}:${s.starType}`));
  }
  window.galaxyRenderer.setSystems(systems);
};

window.setGalaxyHyperlanes = (json: string): void => {
  console.log('🔗 setGalaxyHyperlanes called');
  window.galaxyRenderer?.setHyperlanes(JSON.parse(json) as Hyperlane[]);
};

window.setGalaxyFleets = (json: string): void => {
  window.galaxyRenderer?.setFleets(JSON.parse(json) as Fleet[]);
};

window.setGalaxyAsteroidFields = (json: string): void => {
  console.log('🪨 setGalaxyAsteroidFields called');
  if (window.galaxyRenderer) window.galaxyRenderer.asteroidFields = JSON.parse(json) as AsteroidField[];
};

window.setGalaxyStations = (json: string): void => {
  window.galaxyRenderer?.setStations(JSON.parse(json) as StationMarker[]);
};

window.setGalaxyCallbacks = (dotnetRef: DotNetObjectReference): void => {
  if (!window.galaxyRenderer) return;
  window.galaxyRenderer.onSystemSelected = (system) => {
    void dotnetRef.invokeMethodAsync('OnSystemSelected', system ? JSON.stringify(system) : null);
  };
  window.galaxyRenderer.onSystemHovered = (system) => {
    void dotnetRef.invokeMethodAsync('OnSystemHovered', system ? JSON.stringify(system) : null);
  };
};

window.centerGalaxyOnSystem = (systemId: string): void => {
  window.galaxyRenderer?.centerOnSystem(systemId);
};

window.setGalaxyZoom = (level: number): void => {
  window.galaxyRenderer?.setZoom(level);
};

window.resetGalaxyView = (): void => {
  window.galaxyRenderer?.resetView();
};

window.destroyGalaxyMap = (): void => {
  window.galaxyRenderer?.destroy();
  window.galaxyRenderer = null;
};
