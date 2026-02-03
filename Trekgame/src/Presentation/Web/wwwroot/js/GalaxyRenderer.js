/**
 * GalaxyRenderer.js - Stellaris-style Galaxy Map using HTML5 Canvas
 * 
 * Features:
 * - WebGL-accelerated rendering via Canvas 2D (with hardware acceleration)
 * - Real star icons instead of colored dots
 * - Nebula background layers with parallax
 * - Smooth pan/zoom with momentum
 * - Territory overlays with faction colors
 * - Animated hyperlanes
 * - Fleet movement visualization
 */

class GalaxyRenderer {
    constructor(containerId) {
        this.container = document.getElementById(containerId);
        if (!this.container) {
            console.error('Galaxy container not found:', containerId);
            return;
        }

        // Create canvas layers
        this.setupCanvasLayers();
        
        // View state
        this.viewX = 0;
        this.viewY = 0;
        this.zoom = 1;
        this.targetZoom = 1;
        this.minZoom = 0.2;
        this.maxZoom = 4;
        
        // Interaction state
        this.isDragging = false;
        this.lastMouseX = 0;
        this.lastMouseY = 0;
        this.velocity = { x: 0, y: 0 };
        
        // Data
        this.systems = [];
        this.hyperlanes = [];
        this.fleets = [];
        this.territories = new Map();
        
        // Selection
        this.selectedSystemId = null;
        this.hoveredSystemId = null;
        
        // Assets
        this.assets = {
            stars: {},
            nebulae: [],
            icons: {}
        };
        
        // Callbacks
        this.onSystemSelected = null;
        this.onSystemHovered = null;
        
        // Animation
        this.animationFrame = null;
        this.lastFrameTime = 0;
        
        this.setupEventListeners();
        this.loadAssets().then(() => this.startRenderLoop());
    }

    setupCanvasLayers() {
        this.container.style.position = 'relative';
        this.container.style.overflow = 'hidden';
        
        const width = this.container.clientWidth;
        const height = this.container.clientHeight;
        
        // Background layer (nebulae, stars)
        this.bgCanvas = this.createCanvas('galaxy-bg', width, height, 1);
        this.bgCtx = this.bgCanvas.getContext('2d');
        
        // Main layer (systems, hyperlanes)
        this.mainCanvas = this.createCanvas('galaxy-main', width, height, 2);
        this.mainCtx = this.mainCanvas.getContext('2d');
        
        // UI layer (selection, tooltips)
        this.uiCanvas = this.createCanvas('galaxy-ui', width, height, 3);
        this.uiCtx = this.uiCanvas.getContext('2d');
    }

    createCanvas(id, width, height, zIndex) {
        const canvas = document.createElement('canvas');
        canvas.id = id;
        canvas.width = width;
        canvas.height = height;
        canvas.style.position = 'absolute';
        canvas.style.left = '0';
        canvas.style.top = '0';
        canvas.style.zIndex = zIndex;
        this.container.appendChild(canvas);
        return canvas;
    }

    async loadAssets() {
        // Star type images - we'll generate procedural ones
        const starTypes = ['yellow', 'orange', 'red', 'blue', 'white', 'neutron', 'blackhole'];
        
        for (const type of starTypes) {
            this.assets.stars[type] = this.generateStarImage(type);
        }
        
        // Generate nebula textures
        for (let i = 0; i < 5; i++) {
            this.assets.nebulae.push(this.generateNebulaImage(i));
        }
        
        // Generate icons
        this.assets.icons.colony = this.generateIcon('colony');
        this.assets.icons.fleet = this.generateIcon('fleet');
        this.assets.icons.starbase = this.generateIcon('starbase');
        
        console.log('Galaxy assets loaded');
    }

    generateStarImage(type) {
        const size = 64;
        const canvas = document.createElement('canvas');
        canvas.width = size;
        canvas.height = size;
        const ctx = canvas.getContext('2d');
        
        const colors = {
            yellow: { core: '#ffffff', mid: '#ffee88', outer: '#ffaa00', glow: '#ff880044' },
            orange: { core: '#ffffff', mid: '#ffcc66', outer: '#ff6600', glow: '#ff440044' },
            red: { core: '#ffdddd', mid: '#ff6666', outer: '#cc0000', glow: '#ff000033' },
            blue: { core: '#ffffff', mid: '#aaddff', outer: '#4488ff', glow: '#0066ff44' },
            white: { core: '#ffffff', mid: '#eeeeff', outer: '#ccccff', glow: '#ffffff33' },
            neutron: { core: '#ffffff', mid: '#88ffff', outer: '#00cccc', glow: '#00ffff55' },
            blackhole: { core: '#000000', mid: '#220022', outer: '#440044', glow: '#ff00ff22' }
        };
        
        const c = colors[type] || colors.yellow;
        const center = size / 2;
        
        // Outer glow
        const glowGrad = ctx.createRadialGradient(center, center, 0, center, center, size/2);
        glowGrad.addColorStop(0, c.glow);
        glowGrad.addColorStop(1, 'transparent');
        ctx.fillStyle = glowGrad;
        ctx.fillRect(0, 0, size, size);
        
        // Star body
        const starGrad = ctx.createRadialGradient(center, center, 0, center, center, size/4);
        starGrad.addColorStop(0, c.core);
        starGrad.addColorStop(0.3, c.mid);
        starGrad.addColorStop(1, c.outer);
        
        ctx.beginPath();
        ctx.arc(center, center, size/4, 0, Math.PI * 2);
        ctx.fillStyle = starGrad;
        ctx.fill();
        
        // Lens flare for bright stars
        if (type !== 'blackhole' && type !== 'red') {
            ctx.globalAlpha = 0.3;
            ctx.strokeStyle = c.mid;
            ctx.lineWidth = 1;
            
            // Horizontal flare
            ctx.beginPath();
            ctx.moveTo(center - size/2.5, center);
            ctx.lineTo(center + size/2.5, center);
            ctx.stroke();
            
            // Vertical flare
            ctx.beginPath();
            ctx.moveTo(center, center - size/2.5);
            ctx.lineTo(center, center + size/2.5);
            ctx.stroke();
            
            ctx.globalAlpha = 1;
        }
        
        // Accretion disk for black holes
        if (type === 'blackhole') {
            ctx.strokeStyle = '#ff66ff';
            ctx.lineWidth = 2;
            ctx.globalAlpha = 0.5;
            ctx.beginPath();
            ctx.ellipse(center, center, size/3, size/6, Math.PI/6, 0, Math.PI * 2);
            ctx.stroke();
            ctx.globalAlpha = 1;
        }
        
        return canvas;
    }

    generateNebulaImage(seed) {
        const size = 512;
        const canvas = document.createElement('canvas');
        canvas.width = size;
        canvas.height = size;
        const ctx = canvas.getContext('2d');
        
        // Random color based on seed
        const hues = [200, 280, 320, 180, 30]; // Blue, Purple, Pink, Cyan, Orange
        const hue = hues[seed % hues.length];
        
        // Create cloudy nebula effect
        const imageData = ctx.createImageData(size, size);
        const data = imageData.data;
        
        // Simple perlin-like noise
        const noise = (x, y, scale) => {
            const nx = x * scale;
            const ny = y * scale;
            return (Math.sin(nx * 12.9898 + ny * 78.233 + seed) * 43758.5453) % 1;
        };
        
        for (let y = 0; y < size; y++) {
            for (let x = 0; x < size; x++) {
                const i = (y * size + x) * 4;
                
                // Multi-octave noise
                let n = 0;
                n += noise(x, y, 0.01) * 0.5;
                n += noise(x, y, 0.02) * 0.25;
                n += noise(x, y, 0.04) * 0.125;
                n = Math.abs(n);
                
                // Distance from center falloff
                const dx = (x - size/2) / size;
                const dy = (y - size/2) / size;
                const dist = Math.sqrt(dx*dx + dy*dy);
                const falloff = Math.max(0, 1 - dist * 1.5);
                
                n *= falloff;
                
                // HSL to RGB
                const h = hue / 360;
                const s = 0.6;
                const l = n * 0.3;
                
                const hue2rgb = (p, q, t) => {
                    if (t < 0) t += 1;
                    if (t > 1) t -= 1;
                    if (t < 1/6) return p + (q - p) * 6 * t;
                    if (t < 1/2) return q;
                    if (t < 2/3) return p + (q - p) * (2/3 - t) * 6;
                    return p;
                };
                
                const q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                const p = 2 * l - q;
                
                data[i] = hue2rgb(p, q, h + 1/3) * 255;
                data[i + 1] = hue2rgb(p, q, h) * 255;
                data[i + 2] = hue2rgb(p, q, h - 1/3) * 255;
                data[i + 3] = n * 100; // Alpha
            }
        }
        
        ctx.putImageData(imageData, 0, 0);
        return canvas;
    }

    generateIcon(type) {
        const size = 32;
        const canvas = document.createElement('canvas');
        canvas.width = size;
        canvas.height = size;
        const ctx = canvas.getContext('2d');
        const center = size / 2;
        
        ctx.shadowColor = '#000';
        ctx.shadowBlur = 4;
        
        switch (type) {
            case 'colony':
                // Settlement icon
                ctx.fillStyle = '#44ff44';
                ctx.strokeStyle = '#228822';
                ctx.lineWidth = 2;
                ctx.beginPath();
                ctx.roundRect(center - 8, center - 6, 16, 12, 2);
                ctx.fill();
                ctx.stroke();
                // Dome
                ctx.beginPath();
                ctx.arc(center, center - 6, 6, Math.PI, 0);
                ctx.fill();
                ctx.stroke();
                break;
                
            case 'fleet':
                // Ship icon (triangle)
                ctx.fillStyle = '#ffaa00';
                ctx.strokeStyle = '#886600';
                ctx.lineWidth = 2;
                ctx.beginPath();
                ctx.moveTo(center, center - 10);
                ctx.lineTo(center + 8, center + 8);
                ctx.lineTo(center - 8, center + 8);
                ctx.closePath();
                ctx.fill();
                ctx.stroke();
                break;
                
            case 'starbase':
                // Station icon (hexagon)
                ctx.fillStyle = '#8888ff';
                ctx.strokeStyle = '#4444aa';
                ctx.lineWidth = 2;
                ctx.beginPath();
                for (let i = 0; i < 6; i++) {
                    const angle = (i * 60 - 30) * Math.PI / 180;
                    const x = center + Math.cos(angle) * 10;
                    const y = center + Math.sin(angle) * 10;
                    if (i === 0) ctx.moveTo(x, y);
                    else ctx.lineTo(x, y);
                }
                ctx.closePath();
                ctx.fill();
                ctx.stroke();
                break;
        }
        
        return canvas;
    }

    setupEventListeners() {
        const canvas = this.uiCanvas;
        
        canvas.addEventListener('mousedown', (e) => this.onMouseDown(e));
        canvas.addEventListener('mousemove', (e) => this.onMouseMove(e));
        canvas.addEventListener('mouseup', (e) => this.onMouseUp(e));
        canvas.addEventListener('mouseleave', (e) => this.onMouseUp(e));
        canvas.addEventListener('wheel', (e) => this.onWheel(e));
        canvas.addEventListener('click', (e) => this.onClick(e));
        
        // Resize handler
        window.addEventListener('resize', () => this.onResize());
    }

    onMouseDown(e) {
        this.isDragging = true;
        this.lastMouseX = e.clientX;
        this.lastMouseY = e.clientY;
        this.velocity = { x: 0, y: 0 };
    }

    onMouseMove(e) {
        const rect = this.container.getBoundingClientRect();
        const mouseX = e.clientX - rect.left;
        const mouseY = e.clientY - rect.top;
        
        if (this.isDragging) {
            const dx = e.clientX - this.lastMouseX;
            const dy = e.clientY - this.lastMouseY;
            
            this.viewX -= dx / this.zoom;
            this.viewY -= dy / this.zoom;
            
            this.velocity = { x: dx, y: dy };
            
            this.lastMouseX = e.clientX;
            this.lastMouseY = e.clientY;
        } else {
            // Check for system hover
            this.checkSystemHover(mouseX, mouseY);
        }
    }

    onMouseUp(e) {
        this.isDragging = false;
    }

    onWheel(e) {
        e.preventDefault();
        
        const rect = this.container.getBoundingClientRect();
        const mouseX = e.clientX - rect.left;
        const mouseY = e.clientY - rect.top;
        
        // Zoom towards mouse position
        const worldX = this.screenToWorldX(mouseX);
        const worldY = this.screenToWorldY(mouseY);
        
        const zoomFactor = e.deltaY > 0 ? 0.9 : 1.1;
        this.targetZoom = Math.max(this.minZoom, Math.min(this.maxZoom, this.zoom * zoomFactor));
        
        // Adjust view to zoom towards mouse
        const newWorldX = this.screenToWorldX(mouseX);
        const newWorldY = this.screenToWorldY(mouseY);
        this.viewX += worldX - newWorldX;
        this.viewY += worldY - newWorldY;
    }

    onClick(e) {
        const rect = this.container.getBoundingClientRect();
        const mouseX = e.clientX - rect.left;
        const mouseY = e.clientY - rect.top;
        
        const system = this.getSystemAtPosition(mouseX, mouseY);
        if (system) {
            this.selectedSystemId = system.id;
            if (this.onSystemSelected) {
                this.onSystemSelected(system);
            }
        } else {
            this.selectedSystemId = null;
            if (this.onSystemSelected) {
                this.onSystemSelected(null);
            }
        }
    }

    onResize() {
        const width = this.container.clientWidth;
        const height = this.container.clientHeight;
        
        [this.bgCanvas, this.mainCanvas, this.uiCanvas].forEach(canvas => {
            canvas.width = width;
            canvas.height = height;
        });
    }

    // Coordinate conversion
    screenToWorldX(screenX) {
        return this.viewX + (screenX - this.mainCanvas.width / 2) / this.zoom;
    }

    screenToWorldY(screenY) {
        return this.viewY + (screenY - this.mainCanvas.height / 2) / this.zoom;
    }

    worldToScreenX(worldX) {
        return (worldX - this.viewX) * this.zoom + this.mainCanvas.width / 2;
    }

    worldToScreenY(worldY) {
        return (worldY - this.viewY) * this.zoom + this.mainCanvas.height / 2;
    }

    // Data setters
    setSystems(systems) {
        console.log('🌟 Galaxy: Setting systems:', systems?.length || 0, 'systems');
        this.systems = systems || [];
        if (this.systems.length > 0) {
            console.log('🌟 First system:', this.systems[0]);
            this.calculateBounds();
            console.log('🌟 View centered at:', this.viewX, this.viewY);
        }
    }

    setHyperlanes(hyperlanes) {
        console.log('🔗 Galaxy: Setting hyperlanes:', hyperlanes?.length || 0);
        this.hyperlanes = hyperlanes || [];
    }

    setFleets(fleets) {
        this.fleets = fleets;
    }

    setTerritories(territories) {
        this.territories = new Map(territories);
    }

    calculateBounds() {
        if (this.systems.length === 0) return;
        
        let minX = Infinity, maxX = -Infinity;
        let minY = Infinity, maxY = -Infinity;
        
        this.systems.forEach(s => {
            minX = Math.min(minX, s.x);
            maxX = Math.max(maxX, s.x);
            minY = Math.min(minY, s.y);
            maxY = Math.max(maxY, s.y);
        });
        
        this.bounds = { minX, maxX, minY, maxY };
        this.viewX = (minX + maxX) / 2;
        this.viewY = (minY + maxY) / 2;
    }

    // Interaction
    checkSystemHover(screenX, screenY) {
        const system = this.getSystemAtPosition(screenX, screenY);
        const newHoveredId = system ? system.id : null;
        
        if (newHoveredId !== this.hoveredSystemId) {
            this.hoveredSystemId = newHoveredId;
            if (this.onSystemHovered) {
                this.onSystemHovered(system);
            }
        }
    }

    getSystemAtPosition(screenX, screenY) {
        const hitRadius = 20 / this.zoom;
        const worldX = this.screenToWorldX(screenX);
        const worldY = this.screenToWorldY(screenY);
        
        for (const system of this.systems) {
            const dx = system.x - worldX;
            const dy = system.y - worldY;
            if (dx * dx + dy * dy < hitRadius * hitRadius) {
                return system;
            }
        }
        return null;
    }

    // Render loop
    startRenderLoop() {
        const render = (timestamp) => {
            const deltaTime = (timestamp - this.lastFrameTime) / 1000;
            this.lastFrameTime = timestamp;
            
            this.update(deltaTime);
            this.render();
            
            this.animationFrame = requestAnimationFrame(render);
        };
        
        this.animationFrame = requestAnimationFrame(render);
    }

    stopRenderLoop() {
        if (this.animationFrame) {
            cancelAnimationFrame(this.animationFrame);
        }
    }

    update(dt) {
        // Smooth zoom
        this.zoom += (this.targetZoom - this.zoom) * 0.1;
        
        // Momentum scrolling
        if (!this.isDragging && (Math.abs(this.velocity.x) > 0.1 || Math.abs(this.velocity.y) > 0.1)) {
            this.viewX -= this.velocity.x / this.zoom * 0.3;
            this.viewY -= this.velocity.y / this.zoom * 0.3;
            this.velocity.x *= 0.95;
            this.velocity.y *= 0.95;
        }
    }

    render() {
        this.renderBackground();
        this.renderMain();
        this.renderUI();
    }

    renderBackground() {
        const ctx = this.bgCtx;
        const w = this.bgCanvas.width;
        const h = this.bgCanvas.height;
        
        // Dark space background
        ctx.fillStyle = '#050510';
        ctx.fillRect(0, 0, w, h);
        
        // Background stars (static, small)
        ctx.fillStyle = 'rgba(255, 255, 255, 0.5)';
        for (let i = 0; i < 200; i++) {
            const x = (i * 137.5) % w;
            const y = (i * 97.3) % h;
            const size = (i % 3) * 0.5 + 0.5;
            ctx.beginPath();
            ctx.arc(x, y, size, 0, Math.PI * 2);
            ctx.fill();
        }
        
        // Nebulae (parallax)
        this.assets.nebulae.forEach((nebula, i) => {
            const parallax = 0.1 + i * 0.05;
            const x = ((-this.viewX * parallax) % 512) - 256;
            const y = ((-this.viewY * parallax) % 512) - 256;
            
            ctx.globalAlpha = 0.3;
            
            // Tile the nebula
            for (let tx = -1; tx <= Math.ceil(w / 512) + 1; tx++) {
                for (let ty = -1; ty <= Math.ceil(h / 512) + 1; ty++) {
                    ctx.drawImage(nebula, x + tx * 512, y + ty * 512);
                }
            }
        });
        
        ctx.globalAlpha = 1;
    }

    renderMain() {
        const ctx = this.mainCtx;
        const w = this.mainCanvas.width;
        const h = this.mainCanvas.height;
        
        ctx.clearRect(0, 0, w, h);
        
        // Render hyperlanes
        this.renderHyperlanes(ctx);
        
        // Render territory overlay
        this.renderTerritories(ctx);
        
        // Render systems
        this.renderSystems(ctx);
        
        // Render fleets
        this.renderFleets(ctx);
    }

    renderHyperlanes(ctx) {
        ctx.globalAlpha = 0.4;
        
        this.hyperlanes.forEach(lane => {
            const fromSys = this.systems.find(s => s.id === lane.fromId);
            const toSys = this.systems.find(s => s.id === lane.toId);
            
            if (!fromSys || !toSys) return;
            
            const x1 = this.worldToScreenX(fromSys.x);
            const y1 = this.worldToScreenY(fromSys.y);
            const x2 = this.worldToScreenX(toSys.x);
            const y2 = this.worldToScreenY(toSys.y);
            
            // Color based on ownership
            let color = '#334466';
            if (fromSys.factionId && fromSys.factionId === toSys.factionId) {
                color = this.getFactionColor(fromSys.factionId);
            }
            
            ctx.strokeStyle = color;
            ctx.lineWidth = this.zoom > 0.5 ? 2 : 1;
            ctx.beginPath();
            ctx.moveTo(x1, y1);
            ctx.lineTo(x2, y2);
            ctx.stroke();
        });
        
        ctx.globalAlpha = 1;
    }

    renderTerritories(ctx) {
        // Soft territory blobs around owned systems
        this.systems.forEach(system => {
            if (!system.factionId) return;
            
            const x = this.worldToScreenX(system.x);
            const y = this.worldToScreenY(system.y);
            const radius = 50 * this.zoom;
            
            const color = this.getFactionColor(system.factionId);
            const gradient = ctx.createRadialGradient(x, y, 0, x, y, radius);
            gradient.addColorStop(0, color + '44');
            gradient.addColorStop(1, color + '00');
            
            ctx.fillStyle = gradient;
            ctx.beginPath();
            ctx.arc(x, y, radius, 0, Math.PI * 2);
            ctx.fill();
        });
    }

    renderSystems(ctx) {
        this.systems.forEach(system => {
            const x = this.worldToScreenX(system.x);
            const y = this.worldToScreenY(system.y);
            
            // Skip if off-screen
            if (x < -50 || x > this.mainCanvas.width + 50 ||
                y < -50 || y > this.mainCanvas.height + 50) {
                return;
            }
            
            const isSelected = system.id === this.selectedSystemId;
            const isHovered = system.id === this.hoveredSystemId;
            const starSize = (isSelected || isHovered ? 40 : 32) * this.zoom;
            
            // Draw star
            const starType = system.starType || 'yellow';
            const starImage = this.assets.stars[starType] || this.assets.stars.yellow;
            
            ctx.drawImage(starImage, 
                x - starSize/2, y - starSize/2, 
                starSize, starSize);
            
            // Ownership ring
            if (system.factionId) {
                ctx.strokeStyle = this.getFactionColor(system.factionId);
                ctx.lineWidth = 2;
                ctx.beginPath();
                ctx.arc(x, y, starSize/2 + 4, 0, Math.PI * 2);
                ctx.stroke();
            }
            
            // Selection ring
            if (isSelected) {
                ctx.strokeStyle = '#ffcc00';
                ctx.lineWidth = 2;
                ctx.setLineDash([6, 4]);
                ctx.beginPath();
                ctx.arc(x, y, starSize/2 + 10, 0, Math.PI * 2);
                ctx.stroke();
                ctx.setLineDash([]);
            }
            
            // Colony/Fleet icons
            if (system.hasColony) {
                ctx.drawImage(this.assets.icons.colony, 
                    x + starSize/2 - 8, y - starSize/2 - 8, 20, 20);
            }
            
            if (system.hasFleet) {
                ctx.drawImage(this.assets.icons.fleet, 
                    x - starSize/2 - 12, y - starSize/2 - 8, 20, 20);
            }
            
            // System name (show if zoomed in or selected/hovered)
            if (this.zoom > 0.6 || isSelected || isHovered) {
                ctx.fillStyle = isSelected ? '#ffcc00' : (isHovered ? '#ffffff' : '#aabbcc');
                ctx.font = `${Math.max(10, 12 * this.zoom)}px 'Orbitron', sans-serif`;
                ctx.textAlign = 'center';
                ctx.fillText(system.name, x, y + starSize/2 + 14);
            }
        });
    }

    renderFleets(ctx) {
        this.fleets.forEach(fleet => {
            const system = this.systems.find(s => s.id === fleet.systemId);
            if (!system) return;
            
            const x = this.worldToScreenX(system.x);
            const y = this.worldToScreenY(system.y);
            
            // Fleet movement path
            if (fleet.destinationId) {
                const destSystem = this.systems.find(s => s.id === fleet.destinationId);
                if (destSystem) {
                    const dx = this.worldToScreenX(destSystem.x);
                    const dy = this.worldToScreenY(destSystem.y);
                    
                    ctx.strokeStyle = '#ffaa00';
                    ctx.lineWidth = 2;
                    ctx.setLineDash([8, 4]);
                    ctx.beginPath();
                    ctx.moveTo(x, y);
                    ctx.lineTo(dx, dy);
                    ctx.stroke();
                    ctx.setLineDash([]);
                }
            }
        });
    }

    renderUI() {
        const ctx = this.uiCtx;
        const w = this.uiCanvas.width;
        const h = this.uiCanvas.height;
        
        ctx.clearRect(0, 0, w, h);
        
        // Hover tooltip
        if (this.hoveredSystemId) {
            const system = this.systems.find(s => s.id === this.hoveredSystemId);
            if (system) {
                const x = this.worldToScreenX(system.x);
                const y = this.worldToScreenY(system.y);
                this.renderTooltip(ctx, system, x, y - 50);
            }
        }
    }

    renderTooltip(ctx, system, x, y) {
        const padding = 10;
        const lineHeight = 18;
        
        const lines = [
            system.name,
            `Star: ${system.starType || 'Unknown'}`,
            system.factionId ? `Owner: ${system.factionName || 'Unknown'}` : 'Unclaimed',
            system.hasColony ? '🏠 Colony' : '',
            system.hasFleet ? '🚀 Fleet Present' : ''
        ].filter(l => l);
        
        const maxWidth = Math.max(...lines.map(l => ctx.measureText(l).width)) + padding * 2;
        const height = lines.length * lineHeight + padding * 2;
        
        // Clamp position to screen
        x = Math.max(maxWidth/2, Math.min(this.uiCanvas.width - maxWidth/2, x));
        y = Math.max(height, y);
        
        // Background
        ctx.fillStyle = 'rgba(10, 15, 30, 0.9)';
        ctx.strokeStyle = '#446688';
        ctx.lineWidth = 1;
        ctx.beginPath();
        ctx.roundRect(x - maxWidth/2, y - height, maxWidth, height, 6);
        ctx.fill();
        ctx.stroke();
        
        // Text
        ctx.fillStyle = '#ffffff';
        ctx.font = '14px "Orbitron", sans-serif';
        ctx.textAlign = 'center';
        
        lines.forEach((line, i) => {
            const textY = y - height + padding + (i + 1) * lineHeight - 4;
            if (i === 0) {
                ctx.fillStyle = '#ffcc00';
                ctx.font = 'bold 14px "Orbitron", sans-serif';
            } else {
                ctx.fillStyle = '#aabbcc';
                ctx.font = '12px "Roboto", sans-serif';
            }
            ctx.fillText(line, x, textY);
        });
    }

    getFactionColor(factionId) {
        // Default colors for common faction IDs (hex format for gradient compatibility)
        const colors = {
            'federation': '#3b82f6',
            'klingon': '#dc2626',
            'romulan': '#10b981',
            'cardassian': '#d97706',
            'ferengi': '#eab308',
            'borg': '#22c55e',
            'dominion': '#7c3aed'
        };
        
        // Try to match faction name
        if (factionId) {
            const idLower = factionId.toLowerCase();
            for (const [key, color] of Object.entries(colors)) {
                if (idLower.includes(key)) {
                    return color;
                }
            }
        }
        
        // Generate consistent hex color from ID
        let hash = 0;
        const str = String(factionId);
        for (let i = 0; i < str.length; i++) {
            hash = str.charCodeAt(i) + ((hash << 5) - hash);
        }
        // Convert to hex color
        const r = (hash & 0xFF0000) >> 16;
        const g = (hash & 0x00FF00) >> 8;
        const b = hash & 0x0000FF;
        return `#${r.toString(16).padStart(2, '0')}${g.toString(16).padStart(2, '0')}${b.toString(16).padStart(2, '0')}`;
    }

    // Public API for Blazor
    centerOnSystem(systemId) {
        const system = this.systems.find(s => s.id === systemId);
        if (system) {
            this.viewX = system.x;
            this.viewY = system.y;
            this.targetZoom = 1.5;
        }
    }

    setZoom(level) {
        this.targetZoom = Math.max(this.minZoom, Math.min(this.maxZoom, level));
    }

    resetView() {
        this.calculateBounds();
        this.targetZoom = 1;
    }

    destroy() {
        this.stopRenderLoop();
        this.container.innerHTML = '';
    }
}

// Export for Blazor interop
window.GalaxyRenderer = GalaxyRenderer;

// Blazor interop functions
window.initGalaxyMap = function(containerId) {
    if (window.galaxyRenderer) {
        window.galaxyRenderer.destroy();
    }
    window.galaxyRenderer = new GalaxyRenderer(containerId);
    console.log('🌌 Galaxy renderer initialized');
    return true;
};

window.setGalaxySystems = function(systemsJson) {
    console.log('🌟 setGalaxySystems called, renderer exists:', !!window.galaxyRenderer);
    if (window.galaxyRenderer) {
        const systems = JSON.parse(systemsJson);
        console.log('🌟 Parsed systems:', systems?.length || 0);
        window.galaxyRenderer.setSystems(systems);
    } else {
        console.error('🌟 No galaxy renderer!');
    }
};

window.setGalaxyHyperlanes = function(hyperlanesJson) {
    console.log('🔗 setGalaxyHyperlanes called');
    if (window.galaxyRenderer) {
        window.galaxyRenderer.setHyperlanes(JSON.parse(hyperlanesJson));
    }
};

window.setGalaxyFleets = function(fleetsJson) {
    if (window.galaxyRenderer) {
        window.galaxyRenderer.setFleets(JSON.parse(fleetsJson));
    }
};

window.setGalaxyCallbacks = function(dotnetRef) {
    if (window.galaxyRenderer) {
        window.galaxyRenderer.onSystemSelected = (system) => {
            dotnetRef.invokeMethodAsync('OnSystemSelected', system ? JSON.stringify(system) : null);
        };
        window.galaxyRenderer.onSystemHovered = (system) => {
            dotnetRef.invokeMethodAsync('OnSystemHovered', system ? JSON.stringify(system) : null);
        };
    }
};

window.centerGalaxyOnSystem = function(systemId) {
    if (window.galaxyRenderer) {
        window.galaxyRenderer.centerOnSystem(systemId);
    }
};

window.setGalaxyZoom = function(level) {
    if (window.galaxyRenderer) {
        window.galaxyRenderer.setZoom(level);
    }
};

window.resetGalaxyView = function() {
    if (window.galaxyRenderer) {
        window.galaxyRenderer.resetView();
    }
};

window.destroyGalaxyMap = function() {
    if (window.galaxyRenderer) {
        window.galaxyRenderer.destroy();
        window.galaxyRenderer = null;
    }
};
