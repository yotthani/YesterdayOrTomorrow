// ============================================================================
// TacticalViewer — Canvas 2D tactical battle renderer for Star Trek combat
// Renders ships as colored triangles on a dark space background with
// weapon fire animations, explosions, shield impacts, and formation movement.
// ============================================================================

// ---------------------------------------------------------------------------
// Interfaces
// ---------------------------------------------------------------------------

interface TacticalShip {
  shipId: string;
  name: string;
  shipClass: string;
  role: string;
  hull: number;
  maxHull: number;
  shields: number;
  maxShields: number;
  x: number;
  y: number;
  isDestroyed: boolean;
  isDisabled: boolean;
  isWebbed: boolean;
  targetId: string | null;
}

interface TacticalRoundResult {
  round: number;
  attacker: { ships: TacticalShip[] };
  defender: { ships: TacticalShip[] };
  events: string[];
  triggeredOrders: string[];
  isComplete: boolean;
  winnerId: string | null;
}

interface DotNetObjectReference {
  invokeMethodAsync(method: string, ...args: unknown[]): Promise<unknown>;
}

interface Particle {
  x: number;
  y: number;
  vx: number;
  vy: number;
  life: number;
  maxLife: number;
  color: string;
  size: number;
}

interface WeaponLine {
  fromX: number;
  fromY: number;
  toX: number;
  toY: number;
  progress: number;
  color: string;
  type: 'phaser' | 'torpedo' | 'disruptor';
}

interface BackgroundStar {
  x: number;
  y: number;
  size: number;
  brightness: number;
}

interface TweenTarget {
  shipId: string;
  targetX: number;
  targetY: number;
  startX: number;
  startY: number;
  startTime: number;
  duration: number;
}

type Side = 'attacker' | 'defender';
type FormationType = 'wedge' | 'sphere' | 'line' | 'dispersed' | 'echelon';

// ---------------------------------------------------------------------------
// Constants
// ---------------------------------------------------------------------------

const BG_COLOR = '#0a0a1a';
const ATTACKER_COLOR = '#22c55e';
const ATTACKER_GLOW = 'rgba(34,197,94,0.35)';
const DEFENDER_COLOR = '#ef4444';
const DEFENDER_GLOW = 'rgba(239,68,68,0.35)';
const SELECTION_COLOR = '#3b82f6';
const HOVER_COLOR = 'rgba(255,255,255,0.4)';
const DESTROYED_ALPHA = 0.25;
const DISABLED_COLOR = '#888888';

const SHIP_SIZES: Record<string, number> = {
  frigate: 8,
  destroyer: 10,
  cruiser: 12,
  battlecruiser: 14,
  battleship: 16,
  dreadnought: 18,
  flagship: 20,
};

const PHASER_COLOR = '#ffaa00';
const TORPEDO_COLOR = '#ff4444';
const DISRUPTOR_COLOR = '#44ff44';

// ---------------------------------------------------------------------------
// TacticalViewer class
// ---------------------------------------------------------------------------

class TacticalViewer {
  private canvas!: HTMLCanvasElement;
  private ctx!: CanvasRenderingContext2D;
  private attackerShips: TacticalShip[] = [];
  private defenderShips: TacticalShip[] = [];
  private dotNetRef: DotNetObjectReference | null = null;
  private selectedShipId: string | null = null;
  private hoveredShipId: string | null = null;
  private animationFrameId = 0;
  private particles: Particle[] = [];
  private weaponLines: WeaponLine[] = [];
  private stars: BackgroundStar[] = [];
  private disorderAttacker = 0;
  private disorderDefender = 0;
  private tweens: TweenTarget[] = [];
  private pulseTime = 0;
  private boundClickHandler: ((e: MouseEvent) => void) | null = null;
  private boundMoveHandler: ((e: MouseEvent) => void) | null = null;

  // ---------- init ----------------------------------------------------------

  init(
    canvasId: string,
    attackerShips: TacticalShip[],
    defenderShips: TacticalShip[],
    dotNetRef: DotNetObjectReference
  ): void {
    const el = document.getElementById(canvasId);
    if (!el || !(el instanceof HTMLCanvasElement)) {
      console.error(`[TacticalViewer] Canvas element '${canvasId}' not found`);
      return;
    }

    this.canvas = el;
    const ctx = this.canvas.getContext('2d');
    if (!ctx) {
      console.error('[TacticalViewer] Could not get 2D context');
      return;
    }
    this.ctx = ctx;
    this.dotNetRef = dotNetRef;
    this.attackerShips = attackerShips;
    this.defenderShips = defenderShips;

    // Ensure canvas size matches CSS size
    this.resizeCanvas();

    // Place ships in initial formations
    this.layoutFormation('attacker', 'wedge', this.attackerShips);
    this.layoutFormation('defender', 'wedge', this.defenderShips);

    // Generate star field
    this.generateStars();

    // Event listeners
    this.boundClickHandler = (e: MouseEvent) => this.handleClick(e);
    this.boundMoveHandler = (e: MouseEvent) => this.handleMouseMove(e);
    this.canvas.addEventListener('click', this.boundClickHandler);
    this.canvas.addEventListener('mousemove', this.boundMoveHandler);

    // Start render loop
    this.pulseTime = performance.now();
    this.renderLoop();

    console.log(
      `[TacticalViewer] Initialized — ${attackerShips.length} attackers, ${defenderShips.length} defenders`
    );
  }

  // ---------- canvas sizing -------------------------------------------------

  private resizeCanvas(): void {
    const rect = this.canvas.getBoundingClientRect();
    const dpr = window.devicePixelRatio || 1;
    this.canvas.width = rect.width * dpr;
    this.canvas.height = rect.height * dpr;
    this.ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
  }

  // ---------- star field ----------------------------------------------------

  private generateStars(): void {
    const count = 120 + Math.floor(Math.random() * 80);
    this.stars = [];
    for (let i = 0; i < count; i++) {
      this.stars.push({
        x: Math.random(),
        y: Math.random(),
        size: 0.4 + Math.random() * 1.2,
        brightness: 0.3 + Math.random() * 0.7,
      });
    }
  }

  // ---------- formation layouts ---------------------------------------------

  private layoutFormation(
    side: Side,
    formation: FormationType,
    ships: TacticalShip[]
  ): void {
    if (ships.length === 0) return;

    const isAttacker = side === 'attacker';
    const centerX = isAttacker ? 0.25 : 0.75;
    const centerY = 0.5;
    const spread = 0.12;

    const positions = this.getFormationPositions(
      formation,
      ships.length,
      centerX,
      centerY,
      spread,
      isAttacker
    );

    for (let i = 0; i < ships.length; i++) {
      ships[i].x = positions[i].x;
      ships[i].y = positions[i].y;
    }
  }

  private getFormationPositions(
    formation: FormationType,
    count: number,
    cx: number,
    cy: number,
    spread: number,
    facingRight: boolean
  ): { x: number; y: number }[] {
    const positions: { x: number; y: number }[] = [];
    const dir = facingRight ? 1 : -1;

    switch (formation) {
      case 'wedge': {
        // V-shape pointing at enemy, leader at tip
        for (let i = 0; i < count; i++) {
          const row = i === 0 ? 0 : Math.ceil(i / 2);
          const side = i % 2 === 1 ? -1 : 1;
          const offsetY = i === 0 ? 0 : side * row * (spread * 0.6);
          const offsetX = -row * (spread * 0.5) * dir;
          positions.push({ x: cx + offsetX, y: cy + offsetY });
        }
        break;
      }
      case 'sphere': {
        // Circular arrangement
        const radius = spread * 0.7;
        for (let i = 0; i < count; i++) {
          if (i === 0 && count > 1) {
            positions.push({ x: cx, y: cy }); // center
          } else {
            const angle = ((i - (count > 1 ? 1 : 0)) / (count > 1 ? count - 1 : count)) * Math.PI * 2;
            positions.push({
              x: cx + Math.cos(angle) * radius,
              y: cy + Math.sin(angle) * radius,
            });
          }
        }
        break;
      }
      case 'line': {
        // Horizontal line perpendicular to enemy
        const totalHeight = spread * 1.4;
        for (let i = 0; i < count; i++) {
          const t = count === 1 ? 0.5 : i / (count - 1);
          positions.push({
            x: cx,
            y: cy - totalHeight / 2 + t * totalHeight,
          });
        }
        break;
      }
      case 'dispersed': {
        // Scattered with seeded random offsets
        const rng = this.seededRandom(42);
        for (let i = 0; i < count; i++) {
          positions.push({
            x: cx + (rng() - 0.5) * spread * 2,
            y: cy + (rng() - 0.5) * spread * 2,
          });
        }
        break;
      }
      case 'echelon': {
        // Diagonal staircase
        for (let i = 0; i < count; i++) {
          const step = i - Math.floor(count / 2);
          positions.push({
            x: cx + step * spread * 0.4 * dir,
            y: cy + step * spread * 0.5,
          });
        }
        break;
      }
    }
    return positions;
  }

  private seededRandom(seed: number): () => number {
    let s = seed;
    return () => {
      s = (s * 16807 + 0) % 2147483647;
      return s / 2147483647;
    };
  }

  // ---------- event handlers ------------------------------------------------

  private handleClick(e: MouseEvent): void {
    const ship = this.findShipAtMouse(e);
    if (ship) {
      this.selectedShipId = ship.shipId;
      if (this.dotNetRef) {
        void this.dotNetRef.invokeMethodAsync('OnShipClicked', ship.shipId);
      }
    } else {
      this.selectedShipId = null;
    }
  }

  private handleMouseMove(e: MouseEvent): void {
    const ship = this.findShipAtMouse(e);
    this.hoveredShipId = ship ? ship.shipId : null;
    this.canvas.style.cursor = ship ? 'pointer' : 'default';
  }

  private findShipAtMouse(e: MouseEvent): TacticalShip | null {
    const rect = this.canvas.getBoundingClientRect();
    const mx = (e.clientX - rect.left) / rect.width;
    const my = (e.clientY - rect.top) / rect.height;
    const hitRadius = 0.025;

    let closest: TacticalShip | null = null;
    let closestDist = hitRadius;

    const checkShip = (ship: TacticalShip) => {
      if (ship.isDestroyed) return;
      const dx = ship.x - mx;
      const dy = ship.y - my;
      const dist = Math.sqrt(dx * dx + dy * dy);
      if (dist < closestDist) {
        closestDist = dist;
        closest = ship;
      }
    };

    this.attackerShips.forEach(checkShip);
    this.defenderShips.forEach(checkShip);
    return closest;
  }

  // ---------- coordinate helpers -------------------------------------------

  private toCanvasX(nx: number): number {
    return nx * this.canvas.getBoundingClientRect().width;
  }

  private toCanvasY(ny: number): number {
    return ny * this.canvas.getBoundingClientRect().height;
  }

  private canvasWidth(): number {
    return this.canvas.getBoundingClientRect().width;
  }

  private canvasHeight(): number {
    return this.canvas.getBoundingClientRect().height;
  }

  // ---------- render loop ---------------------------------------------------

  private renderLoop = (): void => {
    this.animationFrameId = requestAnimationFrame(this.renderLoop);
    this.update();
    this.render();
  };

  private update(): void {
    const now = performance.now();
    this.pulseTime = now;

    // Update tweens
    this.tweens = this.tweens.filter((tw) => {
      const elapsed = now - tw.startTime;
      const t = Math.min(elapsed / tw.duration, 1);
      const eased = this.easeInOutCubic(t);

      const ship = this.findShipById(tw.shipId);
      if (ship) {
        ship.x = tw.startX + (tw.targetX - tw.startX) * eased;
        ship.y = tw.startY + (tw.targetY - tw.startY) * eased;
      }
      return t < 1;
    });

    // Update weapon lines
    this.weaponLines = this.weaponLines.filter((wl) => {
      wl.progress += 0.025;
      return wl.progress < 1;
    });

    // Update particles
    this.particles = this.particles.filter((p) => {
      p.x += p.vx;
      p.y += p.vy;
      p.vx *= 0.97;
      p.vy *= 0.97;
      p.life -= 1;
      return p.life > 0;
    });
  }

  private easeInOutCubic(t: number): number {
    return t < 0.5 ? 4 * t * t * t : 1 - Math.pow(-2 * t + 2, 3) / 2;
  }

  // ---------- main render ---------------------------------------------------

  private render(): void {
    const ctx = this.ctx;
    const w = this.canvasWidth();
    const h = this.canvasHeight();

    // Clear
    ctx.fillStyle = BG_COLOR;
    ctx.fillRect(0, 0, w, h);

    // Stars
    this.drawStars(ctx, w, h);

    // Subtle center divider
    ctx.save();
    ctx.setLineDash([4, 8]);
    ctx.strokeStyle = 'rgba(255,255,255,0.06)';
    ctx.lineWidth = 1;
    ctx.beginPath();
    ctx.moveTo(w / 2, 0);
    ctx.lineTo(w / 2, h);
    ctx.stroke();
    ctx.restore();

    // Formation outlines
    this.drawFormationOutline(ctx, this.attackerShips, ATTACKER_GLOW);
    this.drawFormationOutline(ctx, this.defenderShips, DEFENDER_GLOW);

    // Weapon lines
    this.drawWeaponLines(ctx);

    // Ships
    this.drawShips(ctx, this.attackerShips, ATTACKER_COLOR, true);
    this.drawShips(ctx, this.defenderShips, DEFENDER_COLOR, false);

    // Particles
    this.drawParticles(ctx);

    // Side labels
    this.drawSideLabels(ctx, w, h);
  }

  // ---------- draw stars ----------------------------------------------------

  private drawStars(ctx: CanvasRenderingContext2D, w: number, h: number): void {
    const time = this.pulseTime * 0.001;
    for (const star of this.stars) {
      const twinkle = 0.7 + 0.3 * Math.sin(time * 2 + star.x * 100 + star.y * 77);
      const alpha = star.brightness * twinkle;
      // Slight blue/white variation
      const blue = star.brightness > 0.6 ? 255 : 200;
      ctx.fillStyle = `rgba(${200 + Math.floor(star.brightness * 55)},${210 + Math.floor(star.brightness * 45)},${blue},${alpha})`;
      ctx.beginPath();
      ctx.arc(star.x * w, star.y * h, star.size, 0, Math.PI * 2);
      ctx.fill();
    }
  }

  // ---------- draw formation outline ----------------------------------------

  private drawFormationOutline(
    ctx: CanvasRenderingContext2D,
    ships: TacticalShip[],
    color: string
  ): void {
    const alive = ships.filter((s) => !s.isDestroyed);
    if (alive.length < 2) return;

    let cx = 0, cy = 0;
    for (const s of alive) { cx += s.x; cy += s.y; }
    cx /= alive.length;
    cy /= alive.length;

    let maxR = 0;
    for (const s of alive) {
      const dx = s.x - cx;
      const dy = s.y - cy;
      const r = Math.sqrt(dx * dx + dy * dy);
      if (r > maxR) maxR = r;
    }

    const px = this.toCanvasX(cx);
    const py = this.toCanvasY(cy);
    const pr = Math.max(this.toCanvasX(maxR + 0.03), 20);

    ctx.save();
    ctx.setLineDash([6, 10]);
    ctx.strokeStyle = color;
    ctx.lineWidth = 1;
    ctx.globalAlpha = 0.2;
    ctx.beginPath();
    ctx.arc(px, py, pr, 0, Math.PI * 2);
    ctx.stroke();
    ctx.restore();
  }

  // ---------- draw ships ----------------------------------------------------

  private drawShips(
    ctx: CanvasRenderingContext2D,
    ships: TacticalShip[],
    color: string,
    facingRight: boolean
  ): void {
    for (const ship of ships) {
      this.drawShip(ctx, ship, color, facingRight);
    }
  }

  private drawShip(
    ctx: CanvasRenderingContext2D,
    ship: TacticalShip,
    baseColor: string,
    facingRight: boolean
  ): void {
    const sz = this.getShipSize(ship.shipClass);
    let sx = this.toCanvasX(ship.x);
    let sy = this.toCanvasY(ship.y);

    // Disorder jitter
    const disorder = this.attackerShips.includes(ship)
      ? this.disorderAttacker
      : this.disorderDefender;
    if (disorder > 50) {
      const jitter = ((disorder - 50) / 50) * 3;
      sx += (Math.random() - 0.5) * jitter;
      sy += (Math.random() - 0.5) * jitter;
    }

    ctx.save();

    // Destroyed state
    if (ship.isDestroyed) {
      ctx.globalAlpha = DESTROYED_ALPHA;
      this.drawTriangle(ctx, sx, sy, sz, facingRight, '#555555');
      ctx.globalAlpha = 0.3;
      ctx.font = `bold ${Math.max(8, sz * 0.6)}px Orbitron, monospace`;
      ctx.fillStyle = '#ff4444';
      ctx.textAlign = 'center';
      ctx.fillText('X', sx, sy + sz + 12);
      ctx.restore();
      return;
    }

    // Disabled state
    if (ship.isDisabled) {
      ctx.globalAlpha = 0.5;
      this.drawTriangle(ctx, sx, sy, sz, facingRight, DISABLED_COLOR);
      ctx.globalAlpha = 0.7;
      ctx.font = `bold ${Math.max(7, sz * 0.5)}px Orbitron, monospace`;
      ctx.fillStyle = '#ffaa00';
      ctx.textAlign = 'center';
      ctx.fillText('DISABLED', sx, sy + sz + 14);
      this.drawHealthBars(ctx, ship, sx, sy, sz);
      ctx.restore();
      return;
    }

    // Selection glow (pulsing)
    if (ship.shipId === this.selectedShipId) {
      const pulse = 0.5 + 0.5 * Math.sin(this.pulseTime * 0.004);
      ctx.shadowColor = SELECTION_COLOR;
      ctx.shadowBlur = 10 + pulse * 8;
      this.drawTriangle(ctx, sx, sy, sz + 3, facingRight, 'transparent');
      ctx.shadowBlur = 0;

      ctx.strokeStyle = SELECTION_COLOR;
      ctx.lineWidth = 2;
      ctx.globalAlpha = 0.5 + pulse * 0.5;
      this.strokeTriangle(ctx, sx, sy, sz + 4, facingRight);
      ctx.globalAlpha = 1;
    }

    // Hover highlight
    if (ship.shipId === this.hoveredShipId && ship.shipId !== this.selectedShipId) {
      ctx.strokeStyle = HOVER_COLOR;
      ctx.lineWidth = 1.5;
      this.strokeTriangle(ctx, sx, sy, sz + 3, facingRight);
    }

    // Webbed overlay
    if (ship.isWebbed) {
      this.drawWebOverlay(ctx, sx, sy, sz);
    }

    // Ship body with subtle glow
    ctx.shadowColor = baseColor;
    ctx.shadowBlur = 6;
    this.drawTriangle(ctx, sx, sy, sz, facingRight, baseColor);
    ctx.shadowBlur = 0;

    // Ship name (small)
    ctx.globalAlpha = 0.6;
    ctx.font = `${Math.max(7, sz * 0.55)}px Roboto, sans-serif`;
    ctx.fillStyle = '#cccccc';
    ctx.textAlign = 'center';
    ctx.fillText(ship.name, sx, sy - sz - 14);
    ctx.globalAlpha = 1;

    // Health bars
    this.drawHealthBars(ctx, ship, sx, sy, sz);

    // Target line
    if (ship.targetId) {
      const target = this.findShipById(ship.targetId);
      if (target && !target.isDestroyed) {
        ctx.save();
        ctx.setLineDash([2, 4]);
        ctx.strokeStyle = 'rgba(255,100,100,0.2)';
        ctx.lineWidth = 0.5;
        ctx.beginPath();
        ctx.moveTo(sx, sy);
        ctx.lineTo(this.toCanvasX(target.x), this.toCanvasY(target.y));
        ctx.stroke();
        ctx.restore();
      }
    }

    ctx.restore();
  }

  // ---------- triangle drawing helpers --------------------------------------

  private drawTriangle(
    ctx: CanvasRenderingContext2D,
    cx: number,
    cy: number,
    size: number,
    facingRight: boolean,
    fillColor: string
  ): void {
    const dir = facingRight ? 1 : -1;
    ctx.beginPath();
    ctx.moveTo(cx + size * dir, cy);
    ctx.lineTo(cx - size * 0.6 * dir, cy - size * 0.65);
    ctx.lineTo(cx - size * 0.6 * dir, cy + size * 0.65);
    ctx.closePath();
    ctx.fillStyle = fillColor;
    ctx.fill();
  }

  private strokeTriangle(
    ctx: CanvasRenderingContext2D,
    cx: number,
    cy: number,
    size: number,
    facingRight: boolean
  ): void {
    const dir = facingRight ? 1 : -1;
    ctx.beginPath();
    ctx.moveTo(cx + size * dir, cy);
    ctx.lineTo(cx - size * 0.6 * dir, cy - size * 0.65);
    ctx.lineTo(cx - size * 0.6 * dir, cy + size * 0.65);
    ctx.closePath();
    ctx.stroke();
  }

  // ---------- health bars ---------------------------------------------------

  private drawHealthBars(
    ctx: CanvasRenderingContext2D,
    ship: TacticalShip,
    sx: number,
    sy: number,
    sz: number
  ): void {
    const barW = 22;
    const barH = 3;
    const barX = sx - barW / 2;
    const barY = sy - sz - 10;

    // Hull bar background
    ctx.fillStyle = 'rgba(0,0,0,0.6)';
    ctx.fillRect(barX - 1, barY - 1, barW + 2, barH + 2);

    // Hull bar fill
    const hullPct = ship.maxHull > 0 ? ship.hull / ship.maxHull : 0;
    const hullColor = hullPct > 0.5 ? '#22c55e' : hullPct > 0.25 ? '#eab308' : '#ef4444';
    ctx.fillStyle = hullColor;
    ctx.fillRect(barX, barY, barW * hullPct, barH);

    // Shield bar (below hull)
    if (ship.maxShields > 0) {
      const shieldY = barY + barH + 2;
      const shieldH = 2;
      ctx.fillStyle = 'rgba(0,0,0,0.6)';
      ctx.fillRect(barX - 1, shieldY - 1, barW + 2, shieldH + 2);

      const shieldPct = ship.shields / ship.maxShields;
      ctx.fillStyle = '#3b82f6';
      ctx.fillRect(barX, shieldY, barW * shieldPct, shieldH);
    }
  }

  // ---------- web overlay ---------------------------------------------------

  private drawWebOverlay(
    ctx: CanvasRenderingContext2D,
    sx: number,
    sy: number,
    sz: number
  ): void {
    const radius = sz + 6;
    ctx.save();
    ctx.strokeStyle = 'rgba(255,255,255,0.3)';
    ctx.lineWidth = 0.8;
    // Draw web-like lines
    for (let i = 0; i < 6; i++) {
      const angle = (i / 6) * Math.PI * 2;
      ctx.beginPath();
      ctx.moveTo(sx, sy);
      ctx.lineTo(sx + Math.cos(angle) * radius, sy + Math.sin(angle) * radius);
      ctx.stroke();
    }
    // Concentric rings
    for (let r = 1; r <= 2; r++) {
      ctx.beginPath();
      ctx.arc(sx, sy, radius * (r / 3), 0, Math.PI * 2);
      ctx.stroke();
    }
    ctx.restore();
  }

  // ---------- weapon line rendering -----------------------------------------

  private drawWeaponLines(ctx: CanvasRenderingContext2D): void {
    for (const wl of this.weaponLines) {
      const x = wl.fromX + (wl.toX - wl.fromX) * wl.progress;
      const y = wl.fromY + (wl.toY - wl.fromY) * wl.progress;

      ctx.save();

      if (wl.type === 'phaser') {
        // Continuous beam
        const beamEnd = Math.min(wl.progress + 0.15, 1);
        const ex = wl.fromX + (wl.toX - wl.fromX) * beamEnd;
        const ey = wl.fromY + (wl.toY - wl.fromY) * beamEnd;

        ctx.strokeStyle = wl.color;
        ctx.lineWidth = 2;
        ctx.shadowColor = wl.color;
        ctx.shadowBlur = 8;
        ctx.globalAlpha = 0.9;
        ctx.beginPath();
        ctx.moveTo(this.toCanvasX(wl.fromX), this.toCanvasY(wl.fromY));
        ctx.lineTo(this.toCanvasX(ex), this.toCanvasY(ey));
        ctx.stroke();

        // Bright core
        ctx.strokeStyle = '#ffffff';
        ctx.lineWidth = 0.8;
        ctx.beginPath();
        ctx.moveTo(this.toCanvasX(wl.fromX), this.toCanvasY(wl.fromY));
        ctx.lineTo(this.toCanvasX(ex), this.toCanvasY(ey));
        ctx.stroke();
      } else if (wl.type === 'torpedo') {
        // Glowing projectile
        const cx = this.toCanvasX(x);
        const cy = this.toCanvasY(y);

        ctx.shadowColor = wl.color;
        ctx.shadowBlur = 12;
        ctx.fillStyle = wl.color;
        ctx.beginPath();
        ctx.arc(cx, cy, 3, 0, Math.PI * 2);
        ctx.fill();

        // Bright core
        ctx.fillStyle = '#ffffff';
        ctx.beginPath();
        ctx.arc(cx, cy, 1.2, 0, Math.PI * 2);
        ctx.fill();

        // Trail
        const trailLen = 0.04;
        const tx = wl.fromX + (wl.toX - wl.fromX) * Math.max(0, wl.progress - trailLen);
        const ty = wl.fromY + (wl.toY - wl.fromY) * Math.max(0, wl.progress - trailLen);
        const grad = ctx.createLinearGradient(
          this.toCanvasX(tx), this.toCanvasY(ty), cx, cy
        );
        grad.addColorStop(0, 'transparent');
        grad.addColorStop(1, wl.color);
        ctx.strokeStyle = grad;
        ctx.lineWidth = 2;
        ctx.globalAlpha = 0.6;
        ctx.beginPath();
        ctx.moveTo(this.toCanvasX(tx), this.toCanvasY(ty));
        ctx.lineTo(cx, cy);
        ctx.stroke();
      } else {
        // Disruptor — pulsing bolt
        const cx = this.toCanvasX(x);
        const cy = this.toCanvasY(y);
        const pulse = 2 + Math.sin(wl.progress * 30) * 1;

        ctx.shadowColor = wl.color;
        ctx.shadowBlur = 10;
        ctx.fillStyle = wl.color;
        ctx.beginPath();
        ctx.arc(cx, cy, pulse, 0, Math.PI * 2);
        ctx.fill();

        ctx.fillStyle = '#aaffaa';
        ctx.beginPath();
        ctx.arc(cx, cy, pulse * 0.4, 0, Math.PI * 2);
        ctx.fill();
      }

      ctx.restore();
    }
  }

  // ---------- particle rendering --------------------------------------------

  private drawParticles(ctx: CanvasRenderingContext2D): void {
    for (const p of this.particles) {
      const alpha = p.life / p.maxLife;
      ctx.save();
      ctx.globalAlpha = alpha;
      ctx.fillStyle = p.color;
      ctx.shadowColor = p.color;
      ctx.shadowBlur = 4;
      ctx.beginPath();
      ctx.arc(
        this.toCanvasX(p.x),
        this.toCanvasY(p.y),
        p.size * alpha,
        0,
        Math.PI * 2
      );
      ctx.fill();
      ctx.restore();
    }
  }

  // ---------- side labels ---------------------------------------------------

  private drawSideLabels(
    ctx: CanvasRenderingContext2D,
    w: number,
    h: number
  ): void {
    ctx.save();
    ctx.font = 'bold 11px Orbitron, monospace';
    ctx.globalAlpha = 0.35;

    ctx.fillStyle = ATTACKER_COLOR;
    ctx.textAlign = 'center';
    ctx.fillText('ATTACKER', w * 0.25, h - 10);

    ctx.fillStyle = DEFENDER_COLOR;
    ctx.fillText('DEFENDER', w * 0.75, h - 10);
    ctx.restore();
  }

  // ---------- ship size by class -------------------------------------------

  private getShipSize(shipClass: string): number {
    const key = shipClass.toLowerCase();
    return SHIP_SIZES[key] ?? 10;
  }

  // ---------- find ship by id ----------------------------------------------

  private findShipById(shipId: string): TacticalShip | null {
    return (
      this.attackerShips.find((s) => s.shipId === shipId) ??
      this.defenderShips.find((s) => s.shipId === shipId) ??
      null
    );
  }

  // ---------- particle / weapon spawners -----------------------------------

  private spawnExplosion(nx: number, ny: number, count: number, color: string): void {
    for (let i = 0; i < count; i++) {
      const angle = Math.random() * Math.PI * 2;
      const speed = 0.001 + Math.random() * 0.003;
      this.particles.push({
        x: nx,
        y: ny,
        vx: Math.cos(angle) * speed,
        vy: Math.sin(angle) * speed,
        life: 30 + Math.floor(Math.random() * 30),
        maxLife: 60,
        color,
        size: 1.5 + Math.random() * 2.5,
      });
    }
  }

  private spawnShieldImpact(nx: number, ny: number): void {
    for (let i = 0; i < 8; i++) {
      const angle = Math.random() * Math.PI * 2;
      const speed = 0.0005 + Math.random() * 0.001;
      this.particles.push({
        x: nx,
        y: ny,
        vx: Math.cos(angle) * speed,
        vy: Math.sin(angle) * speed,
        life: 15 + Math.floor(Math.random() * 10),
        maxLife: 25,
        color: '#66bbff',
        size: 1 + Math.random() * 1.5,
      });
    }
  }

  private spawnWeaponLine(
    from: TacticalShip,
    to: TacticalShip,
    type: WeaponLine['type']
  ): void {
    const colorMap: Record<WeaponLine['type'], string> = {
      phaser: PHASER_COLOR,
      torpedo: TORPEDO_COLOR,
      disruptor: DISRUPTOR_COLOR,
    };
    this.weaponLines.push({
      fromX: from.x,
      fromY: from.y,
      toX: to.x,
      toY: to.y,
      progress: 0,
      color: colorMap[type],
      type,
    });
  }

  // ---------- event parsing ------------------------------------------------

  private parseWeaponType(eventText: string): WeaponLine['type'] {
    const lower = eventText.toLowerCase();
    if (lower.includes('phaser') || lower.includes('beam')) return 'phaser';
    if (lower.includes('torpedo') || lower.includes('missile') || lower.includes('quantum')) return 'torpedo';
    if (lower.includes('disruptor') || lower.includes('polaron') || lower.includes('plasma')) return 'disruptor';
    // Default to phaser for generic hits
    return 'phaser';
  }

  // ---------- public API: updateRound --------------------------------------

  async updateRound(roundResult: TacticalRoundResult): Promise<void> {
    // Update ship states
    for (const ship of roundResult.attacker.ships) {
      const existing = this.attackerShips.find((s) => s.shipId === ship.shipId);
      if (existing) {
        Object.assign(existing, {
          hull: ship.hull,
          maxHull: ship.maxHull,
          shields: ship.shields,
          maxShields: ship.maxShields,
          isDestroyed: ship.isDestroyed,
          isDisabled: ship.isDisabled,
          isWebbed: ship.isWebbed,
          targetId: ship.targetId,
        });
      }
    }
    for (const ship of roundResult.defender.ships) {
      const existing = this.defenderShips.find((s) => s.shipId === ship.shipId);
      if (existing) {
        Object.assign(existing, {
          hull: ship.hull,
          maxHull: ship.maxHull,
          shields: ship.shields,
          maxShields: ship.maxShields,
          isDestroyed: ship.isDestroyed,
          isDisabled: ship.isDisabled,
          isWebbed: ship.isWebbed,
          targetId: ship.targetId,
        });
      }
    }

    // Parse events for weapon fire and effects
    const allShips = [...this.attackerShips, ...this.defenderShips];
    for (const evt of roundResult.events) {
      // Try to parse "X hits Y" pattern
      const hitMatch = evt.match(/^(.+?)\s+hits?\s+(.+?)(?:\s+for|\s+with|\s*[-—]|$)/i);
      if (hitMatch) {
        const attackerName = hitMatch[1].trim();
        const defenderName = hitMatch[2].trim();
        const fromShip = allShips.find(
          (s) => s.name.toLowerCase() === attackerName.toLowerCase()
        );
        const toShip = allShips.find(
          (s) => s.name.toLowerCase() === defenderName.toLowerCase()
        );
        if (fromShip && toShip) {
          const wtype = this.parseWeaponType(evt);
          this.spawnWeaponLine(fromShip, toShip, wtype);

          // Shield impact if shields took damage
          if (evt.toLowerCase().includes('shield')) {
            this.spawnShieldImpact(toShip.x, toShip.y);
          }
        }
      }

      // Destroyed ships get explosions
      const destroyMatch = evt.match(/(.+?)\s+(?:destroyed|eliminated|explodes)/i);
      if (destroyMatch) {
        const shipName = destroyMatch[1].trim();
        const destroyed = allShips.find(
          (s) => s.name.toLowerCase() === shipName.toLowerCase()
        );
        if (destroyed) {
          this.spawnExplosion(destroyed.x, destroyed.y, 25, '#ff6622');
          this.spawnExplosion(destroyed.x, destroyed.y, 15, '#ffcc44');
        }
      }
    }

    // Wait for animations
    const animDuration = this.weaponLines.length > 0 ? 900 : 500;
    await this.delay(animDuration);
  }

  // ---------- public API: updateFormation -----------------------------------

  updateFormation(side: Side, formation: FormationType): void {
    const ships = side === 'attacker' ? this.attackerShips : this.defenderShips;
    const isAttacker = side === 'attacker';
    const centerX = isAttacker ? 0.25 : 0.75;
    const centerY = 0.5;
    const spread = 0.12;

    const targets = this.getFormationPositions(
      formation,
      ships.length,
      centerX,
      centerY,
      spread,
      isAttacker
    );

    const now = performance.now();
    for (let i = 0; i < ships.length; i++) {
      this.tweens.push({
        shipId: ships[i].shipId,
        targetX: targets[i].x,
        targetY: targets[i].y,
        startX: ships[i].x,
        startY: ships[i].y,
        startTime: now,
        duration: 500,
      });
    }
  }

  // ---------- public API: highlight / select / disorder ---------------------

  highlightShip(shipId: string): void {
    this.hoveredShipId = shipId;
  }

  selectShip(shipId: string): void {
    this.selectedShipId = shipId;
  }

  setDisorder(side: Side, percent: number): void {
    if (side === 'attacker') {
      this.disorderAttacker = percent;
    } else {
      this.disorderDefender = percent;
    }
  }

  // ---------- dispose -------------------------------------------------------

  dispose(): void {
    if (this.animationFrameId) {
      cancelAnimationFrame(this.animationFrameId);
      this.animationFrameId = 0;
    }
    if (this.boundClickHandler) {
      this.canvas.removeEventListener('click', this.boundClickHandler);
    }
    if (this.boundMoveHandler) {
      this.canvas.removeEventListener('mousemove', this.boundMoveHandler);
    }
    this.attackerShips = [];
    this.defenderShips = [];
    this.particles = [];
    this.weaponLines = [];
    this.tweens = [];
    this.dotNetRef = null;
    console.log('[TacticalViewer] Disposed');
  }

  // ---------- utility -------------------------------------------------------

  private delay(ms: number): Promise<void> {
    return new Promise((resolve) => setTimeout(resolve, ms));
  }
}

// ---------------------------------------------------------------------------
// Window export for Blazor JS Interop
// ---------------------------------------------------------------------------

let viewer: TacticalViewer | null = null;

(window as Record<string, unknown>).TacticalViewer = {
  init: (
    canvasId: string,
    attackerShips: TacticalShip[],
    defenderShips: TacticalShip[],
    dotNetRef: DotNetObjectReference
  ) => {
    viewer = new TacticalViewer();
    viewer.init(canvasId, attackerShips, defenderShips, dotNetRef);
  },
  updateRound: async (result: TacticalRoundResult) => {
    await viewer?.updateRound(result);
  },
  updateFormation: (side: string, formation: string) => {
    viewer?.updateFormation(side as Side, formation as FormationType);
  },
  highlightShip: (shipId: string) => {
    viewer?.highlightShip(shipId);
  },
  selectShip: (shipId: string) => {
    viewer?.selectShip(shipId);
  },
  setDisorder: (side: string, percent: number) => {
    viewer?.setDisorder(side as Side, percent);
  },
  dispose: () => {
    viewer?.dispose();
    viewer = null;
  },
};
