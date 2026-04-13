import { useState, useEffect, useRef, useCallback } from "react";
import { createRoot } from "react-dom/client";

// ─── Theme ───────────────────────────────────────────────────────────────────

const KFONT  = "'Teko', 'Oxanium', sans-serif";
const KPIQAD = "'KlingonPiqad', 'Teko', sans-serif";

const T = {
  frameOuter: "#AA0000",
  frameInner: "#880000",
  frameDark:  "#550000",
  gold:       "#DAA520",
  goldBright: "#FFD700",
  goldDim:    "#8B6914",
  bronze:     "#B8860B",
  red:        "#CC0000",
  redBright:  "#FF2200",
  redDim:     "#660000",
  text:       "#CC8800",
  textBright: "#FFD700",
  textDim:    "#774400",
  bg:         "#0A0000",
  bgPanel:    "#1A0500",
  accent:     "#FF4400",
} as const;

// ─── Data ─────────────────────────────────────────────────────────────────────

const NAV = [
  { key: "galaxy",    icon: "⚔", label: "EMPIRE" },
  { key: "planets",   icon: "▼", label: "WORLDS" },
  { key: "fleets",    icon: "◆", label: "WARRIORS" },
  { key: "research",  icon: "◈", label: "SCIENCE" },
  { key: "diplomacy", icon: "⬡", label: "HONOR" },
  { key: "academy",   icon: "▲", label: "ACADEMY" },
  { key: "intel",     icon: "◉", label: "INTEL" },
  { key: "leaders",   icon: "☠", label: "COMMAND" },
];

const RES = [
  { icon: "⚡", label: "DILITHIUM",  val: 1842, subs: ["Abbau", "Raffination", "Reserve"] },
  { icon: "◈",  label: "CREDITS",    val: 3104, subs: ["Tribut", "Handel", "Beute"] },
  { icon: "◎",  label: "RESEARCH",   val: 876,  subs: ["Waffen", "Schilde", "Antrieb"] },
  { icon: "⬡",  label: "PRODUCTION", val: 4210, subs: ["Schiffbau", "Waffen", "Infrastruktur"] },
  { icon: "☠",  label: "WARRIORS",   val: 142,  subs: ["Offiziere", "Bekk", "Rekruten"] },
];

interface StarSystem {
  id: number; name: string; x: number; y: number;
  fac: string | null; type: string | null;
}

const SYSTEMS: StarSystem[] = [
  { id: 1,  name: "Qo'noS",      x: .42, y: .38, fac: "klg", type: "hw" },
  { id: 2,  name: "Boreth",      x: .36, y: .32, fac: "klg", type: "col" },
  { id: 3,  name: "Rura Penthe", x: .48, y: .28, fac: "klg", type: "col" },
  { id: 4,  name: "Narendra",    x: .52, y: .42, fac: "klg", type: "col" },
  { id: 5,  name: "Khitomer",    x: .55, y: .35, fac: "klg", type: "col" },
  { id: 6,  name: "Ty'Gokor",    x: .38, y: .46, fac: "klg", type: "col" },
  ...Array.from({ length: 14 }, (_, i) => ({
    id: 7 + i, name: "Unknown",
    x: .12 + Math.sin(i * 2.1) * .38 + .35,
    y: .10 + Math.cos(i * 1.8) * .38 + .35,
    fac: null, type: null,
  })),
];

const LINKS: [number, number][] = [
  [1,2],[1,4],[1,6],[2,3],[3,5],[4,5],[1,3],[4,6],[2,7],[5,9],[6,11],[3,12],[5,14],
];

// ─── Triangle Grid Background ─────────────────────────────────────────────────

function TriangleGrid({ w, h }: { w: number; h: number }) {
  const ref = useRef<HTMLCanvasElement>(null);
  const raf = useRef<number>(0);

  useEffect(() => {
    const ctx = ref.current?.getContext("2d");
    if (!ctx) return;
    let t = 0;
    const draw = () => {
      ctx.clearRect(0, 0, w, h);
      const g1 = ctx.createRadialGradient(w*.4, h*.4, 0, w*.4, h*.4, w*.5);
      g1.addColorStop(0, "rgba(80,0,0,0.06)"); g1.addColorStop(1, "transparent");
      ctx.fillStyle = g1; ctx.fillRect(0, 0, w, h);

      const size = 40, rowH = size * Math.sin(Math.PI / 3);
      ctx.strokeStyle = "rgba(120,0,0,0.08)"; ctx.lineWidth = 0.5;
      for (let row = -1; row < h / rowH + 1; row++) {
        for (let col = -1; col < w / size + 1; col++) {
          const x = col * size + (row % 2 ? size / 2 : 0), y = row * rowH;
          ctx.beginPath(); ctx.moveTo(x, y); ctx.lineTo(x + size/2, y + rowH); ctx.lineTo(x - size/2, y + rowH); ctx.closePath(); ctx.stroke();
        }
      }
      t += 0.012;
      for (let row = 0; row < h / rowH + 1; row++) {
        for (let col = 0; col < w / size + 1; col++) {
          const x = col * size + (row % 2 ? size / 2 : 0), y = row * rowH;
          const pulse = Math.sin(t + x * 0.01 + y * 0.015) * 0.5 + 0.5;
          if (pulse > 0.85) {
            ctx.beginPath(); ctx.arc(x, y, 1, 0, 6.28);
            ctx.fillStyle = `rgba(200,50,0,${pulse * 0.15})`; ctx.fill();
          }
        }
      }
      raf.current = requestAnimationFrame(draw);
    };
    draw();
    return () => cancelAnimationFrame(raf.current);
  }, [w, h]);

  return <canvas ref={ref} width={w} height={h} style={{ position: "absolute", inset: 0 }} />;
}

// ─── Galaxy Map (Klingon variant) ─────────────────────────────────────────────

interface Cam { x: number; y: number; z: number; }

function GalaxyMap({ systems, links, selected, onSelect, w, h }: {
  systems: StarSystem[]; links: [number,number][];
  selected: StarSystem | null; onSelect: (s: StarSystem) => void;
  w: number; h: number;
}) {
  const ref = useRef<HTMLCanvasElement>(null);
  const camRef = useRef<Cam>({ x:0, y:0, z:1 });
  const [cam, setCam] = useState<Cam>({ x:0, y:0, z:1 });
  const drag = useRef({ on:false, sx:0, sy:0, cx:0, cy:0 });
  const [hov, setHov] = useState<StarSystem | null>(null);

  useEffect(() => { camRef.current = cam; }, [cam]);

  const toScr = useCallback((sx: number, sy: number) => {
    const c = camRef.current;
    return { x: (sx-.5)*w*c.z + w/2 - c.x*c.z, y: (sy-.5)*h*c.z + h/2 - c.y*c.z };
  }, [w, h]);

  useEffect(() => {
    const ctx = ref.current?.getContext("2d");
    if (!ctx) return;
    let t = 0, af: number;
    const render = () => {
      const c = camRef.current; t += .018;
      ctx.clearRect(0, 0, w, h);
      const s2 = (sx: number, sy: number) => ({ x: (sx-.5)*w*c.z+w/2-c.x*c.z, y: (sy-.5)*h*c.z+h/2-c.y*c.z });

      links.forEach(([a, b]) => {
        const sA = systems.find(s => s.id===a), sB = systems.find(s => s.id===b);
        if (!sA||!sB) return;
        const pA = s2(sA.x,sA.y), pB = s2(sB.x,sB.y);
        ctx.beginPath(); ctx.moveTo(pA.x,pA.y); ctx.lineTo(pB.x,pB.y);
        const klg = sA.fac==="klg" && sB.fac==="klg";
        ctx.strokeStyle = klg ? "rgba(200,50,0,0.15)" : "rgba(255,255,255,0.03)";
        ctx.lineWidth = klg ? 1.2 : .5; ctx.stroke();
      });

      systems.filter(s => s.type==="hw").forEach(s => {
        const p = s2(s.x,s.y), r = 120*c.z;
        const g = ctx.createRadialGradient(p.x,p.y,0,p.x,p.y,r);
        g.addColorStop(0,"rgba(200,50,0,0.08)"); g.addColorStop(1,"transparent");
        ctx.fillStyle=g; ctx.beginPath(); ctx.arc(p.x,p.y,r,0,6.28); ctx.fill();
      });

      systems.forEach(s => {
        const p = s2(s.x,s.y);
        if (p.x<-30||p.x>w+30||p.y<-30||p.y>h+30) return;
        const isSel=selected?.id===s.id, isHov=hov?.id===s.id, klg=s.fac==="klg", hw=s.type==="hw";
        const r = Math.max(1.5, (hw ? 6 : klg ? 3.5 : 2.5) * c.z);

        if (isSel) {
          const pulse = .6+.4*Math.sin(t*2.5);
          ctx.beginPath(); ctx.arc(p.x,p.y,r+10,0,6.28);
          ctx.strokeStyle = `rgba(255,68,0,${.3+pulse*.3})`; ctx.lineWidth=1; ctx.stroke();
        }
        if (isHov&&!isSel) {
          ctx.beginPath(); ctx.arc(p.x,p.y,r+6,0,6.28);
          ctx.strokeStyle="rgba(255,200,0,0.25)"; ctx.lineWidth=.8; ctx.stroke();
        }
        if (klg) {
          ctx.save(); ctx.translate(p.x,p.y); ctx.rotate(Math.PI/4);
          ctx.fillStyle = hw ? T.redBright : T.red;
          ctx.fillRect(-r,-r,r*2,r*2); ctx.restore();
        } else {
          ctx.beginPath(); ctx.arc(p.x,p.y,r,0,6.28);
          ctx.fillStyle = s.type ? "#666" : "#444"; ctx.fill();
        }
        if (c.z>.5||hw||isSel||isHov) {
          ctx.font = `${hw?"700":"400"} ${Math.max(9,11*c.z)}px "Teko",sans-serif`;
          ctx.textAlign = "center";
          ctx.fillStyle = isSel ? T.goldBright : isHov ? T.gold : klg ? T.text+"AA" : "#555";
          ctx.fillText(s.name, p.x, p.y+r+13*c.z);
        }
      });
      af = requestAnimationFrame(render);
    };
    render();
    return () => cancelAnimationFrame(af);
  }, [w, h, systems, links, selected, hov]);

  return (
    <canvas ref={ref} width={w} height={h}
      onWheel={e => { e.preventDefault(); setCam(c => ({ ...c, z: Math.max(.3, Math.min(3.5, c.z*(e.deltaY<0?1.1:.91))) })); }}
      onPointerDown={e => { drag.current={on:true,sx:e.clientX,sy:e.clientY,cx:cam.x,cy:cam.y}; ref.current?.setPointerCapture(e.pointerId); }}
      onPointerMove={e => {
        const rect = ref.current!.getBoundingClientRect(), mx = e.clientX-rect.left, my = e.clientY-rect.top;
        let found: StarSystem|null = null;
        for (const s of systems) { const p=toScr(s.x,s.y); if (Math.hypot(mx-p.x,my-p.y)<Math.max(12,6*cam.z)) { found=s; break; } }
        setHov(found);
        ref.current!.style.cursor = found ? "pointer" : drag.current.on ? "grabbing" : "default";
        if (!drag.current.on) return;
        const d = drag.current;
        setCam(c => ({ ...c, x: d.cx-(e.clientX-d.sx)/c.z, y: d.cy-(e.clientY-d.sy)/c.z }));
      }}
      onPointerUp={e => {
        const d = drag.current;
        if (Math.hypot(e.clientX-d.sx,e.clientY-d.sy)<5 && hov) onSelect(hov);
        d.on=false;
      }}
      style={{ width:"100%", height:"100%", touchAction:"none" }}
    />
  );
}

// ─── KlingonPanel ─────────────────────────────────────────────────────────────

function KlingonPanel({ title, icon, color, defaultOpen=true, width=220, children }: {
  title: string; icon: string; color?: string;
  defaultOpen?: boolean; width?: number; children: React.ReactNode;
}) {
  const [open, setOpen] = useState(defaultOpen);
  const C = color || T.red;
  if (!open) {
    return (
      <div onClick={() => setOpen(true)} style={{ width:34, height:34, cursor:"pointer", background:C, clipPath:"polygon(8px 0, 100% 0, calc(100% - 8px) 100%, 0 100%)", display:"flex", alignItems:"center", justifyContent:"center", alignSelf:"flex-end" }}>
        <span style={{ fontSize:12, color:T.goldBright }}>{icon}</span>
      </div>
    );
  }
  const C1=28, C2=10, C3=8, C4=20, slant=6;
  const clip = `polygon(${C1}px 0, calc(100% - ${C2}px) 0, 100% ${C2}px, 100% calc(100% - ${C3}px), calc(100% - ${C3}px) 100%, ${C4}px 100%, 0 calc(100% - ${C4}px), ${slant}px ${C1}px)`;

  return (
    <div style={{ width, position:"relative" }}>
      <div style={{ clipPath:clip, background:C+"88", padding:2 }}>
        <div style={{ clipPath:clip, background:T.bg+"EE", overflow:"hidden" }}>
          {/* Header */}
          <div style={{ height:26, background:`linear-gradient(90deg, ${C}CC, ${C}44)`, display:"flex", alignItems:"center", paddingLeft:C1+2, paddingRight:C2+6, cursor:"pointer", position:"relative" }} onClick={() => setOpen(false)}>
            <span style={{ fontSize:11, color:T.goldBright, marginRight:6 }}>{icon}</span>
            <span style={{ fontSize:14, fontWeight:700, letterSpacing:3, color:T.goldBright, fontFamily:KPIQAD }}>{title}</span>
            <div style={{ flex:1 }}/>
            <span style={{ fontSize:7, color:T.gold+"88", fontFamily:KFONT }}>▼</span>
            <div style={{ position:"absolute", right:C2+30, top:0, width:2, height:"100%", background:T.goldBright+"33", transform:"skewX(-20deg)" }}/>
            <div style={{ position:"absolute", right:C2+36, top:0, width:1, height:"100%", background:T.goldBright+"22", transform:"skewX(-20deg)" }}/>
          </div>
          {/* Top teeth */}
          <div style={{ height:6, background:`linear-gradient(90deg, ${C}55, ${C}22)`, overflow:"hidden" }}>
            <svg style={{ width:"100%", height:6 }}>{Array.from({length:40},(_,i)=><polygon key={i} points={`${i*12},0 ${i*12+6},5 ${i*12+12},0`} fill={T.gold+"33"}/>)}</svg>
          </div>
          {/* Body */}
          <div style={{ padding:`6px ${C2+4}px 6px ${slant+8}px`, minHeight:20 }}>{children}</div>
          {/* Bottom teeth */}
          <div style={{ height:6, background:`linear-gradient(90deg, ${C}22, ${C}55)`, overflow:"hidden" }}>
            <svg style={{ width:"100%", height:6 }}>{Array.from({length:40},(_,i)=><polygon key={i} points={`${i*12},6 ${i*12+6},1 ${i*12+12},6`} fill={T.gold+"33"}/>)}</svg>
          </div>
          <div style={{ height:C4/2, background:T.bg+"EE" }}/>
        </div>
      </div>
      {/* Corner accents */}
      <div style={{ position:"absolute", top:0, left:C1-6, width:0, height:0, borderBottom:`6px solid ${C}`, borderLeft:"6px solid transparent", borderRight:"6px solid transparent" }}/>
      <div style={{ position:"absolute", bottom:0, right:C3-4, width:0, height:0, borderTop:`5px solid ${C}`, borderLeft:"5px solid transparent", borderRight:"5px solid transparent" }}/>
    </div>
  );
}

function KDivider({ color }: { color?: string }) {
  const C = color || T.red;
  return (
    <div style={{ display:"flex", alignItems:"center", gap:4, margin:"4px 0" }}>
      <div style={{ width:8, height:1, background:C }}/>
      <div style={{ flex:1, height:1, background:C+"44" }}/>
      <div style={{ width:5, height:5, background:T.gold+"66", transform:"rotate(45deg)" }}/>
      <div style={{ flex:1, height:1, background:C+"44" }}/>
      <div style={{ width:8, height:1, background:C }}/>
    </div>
  );
}

function KStat({ label, value, bar, color }: { label: string; value: string; bar?: number; color?: string }) {
  const C = color || T.red;
  return (
    <div style={{ display:"flex", alignItems:"center", gap:6, height:18 }}>
      <span style={{ fontSize:9, color:T.textDim, fontFamily:KPIQAD, letterSpacing:1, width:70, flexShrink:0 }}>{label}</span>
      {bar != null && (
        <div style={{ flex:1, height:4, background:T.frameDark, position:"relative", clipPath:"polygon(2px 0, 100% 0, calc(100% - 2px) 100%, 0 100%)" }}>
          <div style={{ position:"absolute", left:0, top:0, bottom:0, width:`${bar}%`, background:`linear-gradient(90deg, ${C}, ${T.gold})` }}/>
        </div>
      )}
      <span style={{ fontSize:13, fontWeight:700, color:T.gold, fontFamily:KFONT, textAlign:"right", minWidth:44 }}>{value}</span>
    </div>
  );
}

function KButton({ label, color, onClick }: { label: string; color?: string; onClick?: () => void }) {
  const C = color || T.red;
  return (
    <button onClick={onClick} style={{ flex:1, border:`1px solid ${C}66`, padding:"5px 0", fontSize:7, fontWeight:700, letterSpacing:2, background:`linear-gradient(180deg, ${T.frameDark}, ${T.bg})`, color:T.gold, cursor:"pointer", fontFamily:KFONT, clipPath:"polygon(6px 0, calc(100% - 6px) 0, 100% 6px, 100% calc(100% - 3px), calc(100% - 3px) 100%, 3px 100%, 0 calc(100% - 3px), 0 6px)" }}>{label}</button>
  );
}

// ─── Main App ─────────────────────────────────────────────────────────────────

export default function KlingonUI() {
  const [sel, setSel] = useState<StarSystem | null>(null);
  const [activeNav, setActiveNav] = useState("galaxy");
  const [navExpanded, setNavExpanded] = useState(true);
  const [resExpanded, setResExpanded] = useState(false);
  const rootRef = useRef<HTMLDivElement>(null);
  const [dims, setDims] = useState({ w:1200, h:800 });

  useEffect(() => {
    const link = document.createElement("link");
    link.href = "https://fonts.googleapis.com/css2?family=Teko:wght@400;500;600;700&family=Oxanium:wght@400;600;700;800&display=swap";
    link.rel = "stylesheet";
    document.head.appendChild(link);
    return () => { document.head.removeChild(link); };
  }, []);

  useEffect(() => {
    const m = () => { if (rootRef.current) setDims({ w:rootRef.current.offsetWidth, h:rootRef.current.offsetHeight }); };
    m(); window.addEventListener("resize", m); return () => window.removeEventListener("resize", m);
  }, []);

  const W = dims.w, H = dims.h;
  const FW=4, CUT=40, TH=34, BH=16, FM=16, G=3;
  const LM = navExpanded ? 100 : 44;
  const LW=10, RW=10;
  const tabW = navExpanded ? 90 : 34;

  const resBaseH=22, resExpandH=52;
  const resH = resBaseH + (resExpanded ? resExpandH : 0);
  const resTop = TH + G;

  const mapT = resTop + resH + G;
  const mapL = LM + LW + G;
  const mapW = Math.max(100, W - mapL - RW - G);
  const mapH = Math.max(100, H - mapT - BH - FM - G);

  const framePath = `M ${LM+CUT},0 L ${W-CUT},0 L ${W},${CUT} L ${W},${H-CUT-FM} L ${W-CUT},${H-FM} L ${LM+CUT},${H-FM} L ${LM},${H-CUT-FM} L ${LM},${CUT} Z`;
  const innerPath = `M ${LM+CUT+FW},${FW} L ${W-CUT-FW},${FW} L ${W-FW},${CUT+FW} L ${W-FW},${H-CUT-FM-FW} L ${W-CUT-FW},${H-FM-FW} L ${LM+CUT+FW},${H-FM-FW} L ${LM+FW},${H-CUT-FM-FW} L ${LM+FW},${CUT+FW} Z`;

  return (
    <div ref={rootRef} style={{ width:"100vw", height:"100vh", background:T.bg, overflow:"hidden", position:"relative", userSelect:"none" }}>
      <TriangleGrid w={W} h={H} />

      {/* ── SVG FRAME ── */}
      <svg style={{ position:"absolute", inset:0, zIndex:8, pointerEvents:"none" }} width={W} height={H}>
        <defs>
          <filter id="redGlow"><feGaussianBlur stdDeviation="3" result="blur"/><feMerge><feMergeNode in="blur"/><feMergeNode in="SourceGraphic"/></feMerge></filter>
        </defs>
        <path d={framePath} fill="none" stroke={T.red+"44"} strokeWidth={8} filter="url(#redGlow)"/>
        <path d={framePath} fill="none" stroke={T.frameOuter} strokeWidth={FW}/>
        <path d={innerPath} fill="none" stroke={T.frameDark} strokeWidth={1}/>
        <polygon points={`${LM+CUT-10},${FW+2} ${LM+CUT+15},${FW+2} ${LM+CUT-10},${FW+27}`} fill={T.red+"88"}/>
        <polygon points={`${W-CUT+10},${FW+2} ${W-CUT-15},${FW+2} ${W-CUT+10},${FW+27}`} fill={T.red+"88"}/>
        <polygon points={`${LM+CUT-10},${H-FM-FW-2} ${LM+CUT+15},${H-FM-FW-2} ${LM+CUT-10},${H-FM-FW-27}`} fill={T.red+"88"}/>
        <polygon points={`${W-CUT+10},${H-FM-FW-2} ${W-CUT-15},${H-FM-FW-2} ${W-CUT+10},${H-FM-FW-27}`} fill={T.red+"88"}/>
        <line x1={LM+LW} y1={TH} x2={W-RW} y2={TH} stroke={T.frameDark} strokeWidth={1}/>
        <line x1={LM+LW} y1={H-FM-BH} x2={W-RW} y2={H-FM-BH} stroke={T.frameDark} strokeWidth={1}/>
        <line x1={LM+LW} y1={TH} x2={LM+LW} y2={H-BH} stroke={T.frameDark} strokeWidth={1}/>
        <line x1={W-RW} y1={TH} x2={W-RW} y2={H-BH} stroke={T.frameDark} strokeWidth={1}/>
      </svg>

      {/* ── TOP BAR ── */}
      <div style={{ position:"absolute", top:FW, left:LM+CUT, right:CUT, height:TH-FW, zIndex:10, background:`linear-gradient(180deg, ${T.red}CC 0%, ${T.frameDark}EE 100%)`, overflow:"hidden" }}>
        <svg style={{ position:"absolute", top:0, left:0, width:"100%", height:8 }}>{Array.from({length:80},(_,i)=><polygon key={i} points={`${i*14},0 ${i*14+7},7 ${i*14+14},0`} fill={T.frameDark+"CC"}/>)}</svg>
        <svg style={{ position:"absolute", bottom:0, left:0, width:"100%", height:8 }}>{Array.from({length:80},(_,i)=><polygon key={i} points={`${i*14},8 ${i*14+7},1 ${i*14+14},8`} fill={T.frameDark+"CC"}/>)}</svg>
        <div style={{ position:"relative", zIndex:1, display:"flex", alignItems:"center", height:"100%", padding:"0 16px" }}>
          {[0,1,2,3,4,5].map(i => <div key={i} style={{ width:0, height:0, borderLeft:"4px solid transparent", borderRight:"4px solid transparent", borderBottom:`6px solid ${i%2===0?T.gold+"66":T.red+"88"}`, marginRight:3 }}/>)}
          <div style={{ width:12 }}/>
          <span style={{ fontSize:18, fontWeight:700, letterSpacing:2, color:T.goldBright, fontFamily:KPIQAD }}>tlhIngan maH</span>
          <span style={{ fontSize:10, color:T.textDim, fontFamily:KPIQAD, letterSpacing:1, marginLeft:8 }}>taghwI</span>
          <div style={{ flex:1 }}/>
          <div style={{ width:0, height:0, borderLeft:"8px solid transparent", borderRight:"8px solid transparent", borderBottom:`12px solid ${T.goldBright}` }}/>
          <div style={{ width:12 }}/>
          <span style={{ fontSize:10, color:T.textDim, fontFamily:KPIQAD, letterSpacing:2 }}>STERNZEIT</span>
          <span style={{ fontSize:16, fontWeight:700, color:T.goldBright, fontFamily:KFONT, marginLeft:8 }}>48623.5</span>
          <div style={{ flex:1 }}/>
          <div style={{ display:"flex", gap:4, alignItems:"center", pointerEvents:"auto" }}>
            <button style={{ background:T.frameDark, border:`1px solid ${T.red}44`, width:26, height:22, fontSize:11, color:T.gold, cursor:"pointer", display:"flex", alignItems:"center", justifyContent:"center", clipPath:"polygon(4px 0, 100% 0, calc(100% - 4px) 100%, 0 100%)" }}>⚙</button>
            <button style={{ background:T.frameDark, border:`1px solid ${T.red}44`, width:26, height:22, fontSize:11, color:T.gold, cursor:"pointer", display:"flex", alignItems:"center", justifyContent:"center", clipPath:"polygon(4px 0, 100% 0, calc(100% - 4px) 100%, 0 100%)" }}>☰</button>
            <button style={{ background:`linear-gradient(180deg, ${T.red}, ${T.frameDark})`, border:`1px solid ${T.redBright}44`, padding:"4px 18px", fontSize:14, fontWeight:700, letterSpacing:2, color:T.goldBright, cursor:"pointer", fontFamily:KPIQAD, clipPath:"polygon(6px 0, 100% 0, calc(100% - 6px) 100%, 0 100%)" }}>Qapla'</button>
          </div>
          <div style={{ width:12 }}/>
          {[0,1,2,3,4,5].map(i => <div key={i} style={{ width:0, height:0, borderLeft:"4px solid transparent", borderRight:"4px solid transparent", borderTop:`6px solid ${i%2===0?T.gold+"66":T.red+"88"}`, marginLeft:3 }}/>)}
        </div>
      </div>

      {/* ── RESOURCE BAR ── */}
      <div style={{ position:"absolute", top:resTop, left:LM+LW+G, right:RW+G, height:resH, zIndex:10, background:`linear-gradient(180deg, ${T.frameDark}AA, ${T.red}44, ${T.frameDark}AA)`, overflow:"hidden", cursor:"pointer", transition:"height 0.25s cubic-bezier(0.4,0,0.2,1)" }} onClick={() => setResExpanded(v => !v)}>
        <svg style={{ position:"absolute", top:0, left:0, width:"100%", height:4, zIndex:1 }}>{Array.from({length:100},(_,i)=><polygon key={i} points={`${i*10},0 ${i*10+5},3 ${i*10+10},0`} fill={T.red+"66"}/>)}</svg>
        <svg style={{ position:"absolute", bottom:0, left:0, width:"100%", height:4, zIndex:1 }}>{Array.from({length:100},(_,i)=><polygon key={i} points={`${i*10},4 ${i*10+5},1 ${i*10+10},4`} fill={T.red+"66"}/>)}</svg>
        <div style={{ height:resBaseH, display:"flex", alignItems:"center", position:"relative", zIndex:2 }}>
          {RES.map((r, i) => (
            <div key={r.label} style={{ flex:1, borderRight:i<RES.length-1?`1px solid ${T.red}44`:"none", display:"flex", alignItems:"center", justifyContent:"center", gap:5, height:"100%" }}>
              <span style={{ fontSize:9, color:T.red }}>{r.icon}</span>
              <span style={{ fontSize:9, color:T.textDim, fontFamily:KPIQAD, letterSpacing:1 }}>{r.label}</span>
              <span style={{ fontSize:14, fontWeight:600, color:T.gold, fontFamily:KFONT }}>{r.val.toLocaleString()}</span>
            </div>
          ))}
          <div style={{ marginLeft:4, marginRight:8, fontSize:8, color:T.gold+"88", transform:resExpanded?"rotate(180deg)":"none", transition:"transform 0.2s" }}>▼</div>
        </div>
        {resExpanded && (
          <div style={{ display:"flex", gap:0, padding:"4px 0 6px", borderTop:`1px solid ${T.red}44`, position:"relative", zIndex:2 }}>
            {RES.map((r, i) => (
              <div key={r.label} style={{ flex:1, borderRight:i<RES.length-1?`1px solid ${T.red}33`:"none", padding:"0 8px", display:"flex", flexDirection:"column", gap:2 }}>
                {r.subs.map((sub, j) => (
                  <div key={sub} style={{ display:"flex", alignItems:"center", justifyContent:"space-between" }}>
                    <span style={{ fontSize:7, color:T.textDim, fontFamily:KFONT, letterSpacing:.5 }}>{sub}</span>
                    <span style={{ fontSize:8, fontWeight:700, color:T.gold+"CC", fontFamily:KFONT }}>{Math.floor(r.val/r.subs.length+(j*37-20))}</span>
                  </div>
                ))}
              </div>
            ))}
          </div>
        )}
      </div>

      {/* ── LEFT ELBOW (SVG) ── */}
      <svg style={{ position:"absolute", left:0, top:0, width:W, height:H, zIndex:15, pointerEvents:"none" }}>
        <polygon points={`${LM+CUT+120},${H-FM+4} ${LM+CUT-4},${H-FM+4} ${LM-4},${H-CUT-FM+4} ${LM-28},${H-CUT-FM+28} ${LM+CUT-10},${H-4} ${LM+CUT+120},${H-4}`} fill={T.red}/>
        <polygon points={`${LM-4},${H-CUT-FM+4} ${LM-4},${H-CUT-FM-20} ${LM-28},${H-CUT-FM-20} ${LM-28},${H-CUT-FM+28}`} fill={T.red}/>
        <polygon points={`${LM-4},${H-CUT-FM-20} ${LM-4},${H-CUT-FM-50} ${LM-tabW-4},${H-CUT-FM-50} ${LM-28},${H-CUT-FM-20}`} fill={T.red}/>
      </svg>

      {/* ── NAV BUTTONS ── */}
      {(() => {
        const tabH=28, tabGap=2;
        const bRight = LM-4, bLeft = bRight - tabW;
        const tabsBottom = H-CUT-FM-50;
        const skew = navExpanded ? 6 : 4;
        return NAV.map((item, idx) => {
          const isActive = activeNav===item.key;
          const y = tabsBottom - (NAV.length-idx)*(tabH+tabGap);
          return (
            <div key={item.key} onClick={() => setActiveNav(item.key)} style={{ position:"absolute", top:y, left:bLeft, width:tabW, height:tabH, zIndex:16, cursor:"pointer", pointerEvents:"auto", display:"flex", alignItems:"center", justifyContent:navExpanded?"flex-end":"center", paddingRight:navExpanded?10:0, gap:navExpanded?8:0, background:isActive?T.red:T.frameDark, clipPath:`polygon(${skew}px 0, 100% 0, ${tabW-skew}px 100%, 0 100%)`, transition:"all 0.2s" }}
              onMouseEnter={e => { if (!isActive) (e.currentTarget as HTMLDivElement).style.background=T.red+"77"; }}
              onMouseLeave={e => { (e.currentTarget as HTMLDivElement).style.background=isActive?T.red:T.frameDark; }}
            >
              <span style={{ fontSize:navExpanded?12:14, color:isActive?T.goldBright:T.textDim, lineHeight:1 }}>{item.icon}</span>
              {navExpanded && <span style={{ fontSize:12, fontWeight:700, letterSpacing:2, color:isActive?T.goldBright:T.textDim, fontFamily:KPIQAD }}>{item.label}</span>}
            </div>
          );
        });
      })()}

      {/* ── NAV TOGGLE ── */}
      <div onClick={() => setNavExpanded(v => !v)} style={{ position:"absolute", top:H-CUT-FM-50-NAV.length*30-24, left:LM-4-tabW, width:tabW, height:16, zIndex:16, cursor:"pointer", pointerEvents:"auto", background:T.frameDark, display:"flex", alignItems:"center", justifyContent:"center", clipPath:`polygon(6px 0, 100% 0, ${tabW-6}px 100%, 0 100%)` }}>
        <span style={{ fontSize:8, color:T.gold, fontFamily:KFONT }}>{navExpanded ? "◀" : "▶"}</span>
      </div>

      {/* ── RIGHT SEGMENTS ── */}
      {[0,1,2,3,4].map(i => {
        const segTop=TH+20, segBot=H-FM-BH-20, segH=(segBot-segTop)/5;
        return <div key={i} style={{ position:"absolute", top:segTop+i*segH+1, right:FW+1, width:RW-FW-2, height:segH-2, background:i%2===0?T.frameDark:T.red+"44", zIndex:10 }}/>;
      })}

      {/* ── BOTTOM BAR ── */}
      <div style={{ position:"absolute", bottom:FM+FW, left:LM+CUT, right:CUT, height:BH-FW, zIndex:10, display:"flex", alignItems:"center", justifyContent:"center", gap:20, padding:"0 16px" }}>
        <span style={{ fontSize:7, fontWeight:600, color:T.textDim, fontFamily:KPIQAD, letterSpacing:1 }}>RUNDE 47</span>
        <span style={{ fontSize:12, fontWeight:700, color:T.gold, fontFamily:KPIQAD, letterSpacing:1 }}>KLINGONISCHES REICH</span>
        <span style={{ fontSize:7, fontWeight:600, color:T.textDim, fontFamily:KPIQAD, letterSpacing:1 }}>2371</span>
      </div>

      {/* ── BOTTOM DECORATIVE STRIP ── */}
      <div style={{ position:"absolute", bottom:0, left:LM+CUT+124, right:CUT+4, height:FM, zIndex:12, overflow:"hidden" }}>
        <div style={{ position:"relative", width:"100%", height:"100%", background:`linear-gradient(180deg, ${T.frameDark}CC 0%, ${T.red}55 50%, ${T.frameDark}CC 100%)`, display:"flex", alignItems:"center", justifyContent:"center" }}>
          <svg style={{ position:"absolute", top:0, left:0, width:"100%", height:5 }}>{Array.from({length:80},(_,i)=><polygon key={i} points={`${i*14},0 ${i*14+7},4 ${i*14+14},0`} fill={T.red+"77"}/>)}</svg>
          <svg style={{ position:"absolute", bottom:0, left:0, width:"100%", height:5 }}>{Array.from({length:80},(_,i)=><polygon key={i} points={`${i*14},5 ${i*14+7},1 ${i*14+14},5`} fill={T.red+"77"}/>)}</svg>
          <div style={{ display:"flex", alignItems:"center", gap:10, zIndex:1 }}>
            <span style={{ fontSize:6, color:T.gold+"88", fontFamily:KFONT, letterSpacing:2 }}>◂◂◂</span>
            <span style={{ fontSize:10, fontWeight:700, color:T.gold, fontFamily:KPIQAD, letterSpacing:3 }}>QA'PLA</span>
            <div style={{ width:0, height:0, borderLeft:"5px solid transparent", borderRight:"5px solid transparent", borderBottom:`7px solid ${T.goldBright}` }}/>
            <span style={{ fontSize:10, fontWeight:700, color:T.gold, fontFamily:KPIQAD, letterSpacing:3 }}>BEKK</span>
            <span style={{ fontSize:6, color:T.gold+"88", fontFamily:KFONT, letterSpacing:2 }}>▸▸▸</span>
          </div>
        </div>
      </div>

      {/* ── LEFT PANEL DECORATIONS ── */}
      {(() => {
        const bRight = LM-4;
        const navTopY = H-CUT-FM-50-NAV.length*30-24;
        const decoTop = CUT+10;
        const decoH = navTopY - decoTop - 8;
        if (decoH < 60) return null;
        return (
          <div style={{ position:"absolute", left:0, top:decoTop, width:bRight, height:decoH, zIndex:16, pointerEvents:"none", display:"flex", flexDirection:"column", alignItems:"center", overflow:"hidden" }}>
            <div style={{ width:"70%", height:2, background:`linear-gradient(90deg, transparent, ${T.red}, transparent)`, marginTop:8 }}/>
            <svg width={navExpanded?60:32} height={navExpanded?70:38} viewBox="0 0 60 70" style={{ opacity:.85, marginTop:8, flexShrink:0 }}>
              <path d="M30,2 L34,30 L30,68 L26,30 Z" fill={T.red} stroke={T.goldDim} strokeWidth="0.5"/>
              <path d="M28,28 Q10,20 4,8 Q8,22 14,32 Q20,38 26,34 Z" fill={T.red} stroke={T.goldDim} strokeWidth="0.5"/>
              <path d="M32,28 Q50,20 56,8 Q52,22 46,32 Q40,38 34,34 Z" fill={T.red} stroke={T.goldDim} strokeWidth="0.5"/>
              <circle cx="30" cy="30" r="5" fill={T.frameDark} stroke={T.gold} strokeWidth="1"/>
              <circle cx="30" cy="30" r="2" fill={T.gold}/>
            </svg>
            <div style={{ width:"60%", height:1, background:`linear-gradient(90deg, transparent, ${T.gold}88, transparent)`, marginTop:6 }}/>
            {navExpanded && <span style={{ fontSize:11, color:T.textDim, fontFamily:KPIQAD, letterSpacing:4, marginTop:4 }}>KDF</span>}
            <div style={{ display:"flex", gap:6, marginTop:8, alignItems:"center" }}>
              {[0,1,2,3,4].map(i => <div key={i} style={{ width:i===2?6:4, height:i===2?6:4, background:i===2?T.gold:T.red+"88", transform:"rotate(45deg)" }}/>)}
            </div>
            <div style={{ flex:1, display:"flex", flexDirection:"column", alignItems:"center", justifyContent:"space-evenly", gap:4, marginTop:6, width:"100%", minHeight:0 }}>
              {Array.from({length:Math.min(8, Math.floor((decoH-160)/18))}, (_,i) => (
                <div key={i} style={{ display:"flex", alignItems:"center", gap:4 }}>
                  <div style={{ width:navExpanded?20:8, height:1, background:T.red+"44" }}/>
                  <div style={{ width:3, height:3, background:i%3===0?T.gold+"66":T.red+"55", transform:"rotate(45deg)" }}/>
                  <div style={{ width:navExpanded?20:8, height:1, background:T.red+"44" }}/>
                </div>
              ))}
            </div>
            <div style={{ display:"flex", gap:8, marginBottom:4, alignItems:"center" }}>
              <div style={{ width:0, height:0, borderLeft:"6px solid transparent", borderRight:"6px solid transparent", borderBottom:`8px solid ${T.red}66` }}/>
              <div style={{ width:0, height:0, borderLeft:"6px solid transparent", borderRight:"6px solid transparent", borderTop:`8px solid ${T.red}66` }}/>
            </div>
            <div style={{ width:"70%", height:2, background:`linear-gradient(90deg, transparent, ${T.red}, transparent)`, marginBottom:4 }}/>
          </div>
        );
      })()}

      {/* ── GALAXY MAP ── */}
      <div style={{ position:"absolute", top:mapT, left:mapL, right:RW+G, bottom:BH+FM+G, overflow:"hidden", zIndex:5 }}>
        <GalaxyMap systems={SYSTEMS} links={LINKS} selected={sel} onSelect={setSel} w={mapW} h={mapH} />
      </div>

      {/* ── RIGHT PANELS ── */}
      <div style={{ position:"absolute", top:mapT+12, right:RW+G+8, width:220, zIndex:14, display:"flex", flexDirection:"column", gap:6, pointerEvents:"auto" }}>
        <KlingonPanel title="KOLONIE" icon="⚔" color={T.red} defaultOpen={true}>
          <div style={{ display:"flex", flexDirection:"column", gap:2 }}>
            <div style={{ display:"flex", justifyContent:"space-between", alignItems:"baseline", marginBottom:2 }}>
              <span style={{ fontSize:18, fontWeight:800, color:T.goldBright, fontFamily:KPIQAD, letterSpacing:2 }}>QO'NOS</span>
              <span style={{ fontSize:7, color:T.red, fontFamily:KFONT, letterSpacing:2, background:T.frameDark, padding:"2px 6px", clipPath:"polygon(4px 0, 100% 0, calc(100% - 4px) 100%, 0 100%)" }}>KLASSE M</span>
            </div>
            <KStat label="BEVÖLKERUNG" value="8.7 Mrd" bar={87}/>
            <KStat label="KRIEGER"     value="Lvl 9"  bar={95} color={T.redBright}/>
            <KStat label="VERTEIDIGUNG" value="Maximum" bar={100} color={T.redBright}/>
            <KStat label="PRODUKTION"  value="+67/t"  bar={67}/>
            <KStat label="FORSCHUNG"   value="+21/t"  bar={42} color={T.gold}/>
            <KDivider/>
            <div style={{ display:"flex", gap:4 }}><KButton label="BAUEN"/><KButton label="UPGRADE"/><KButton label="TRANSFER"/></div>
          </div>
        </KlingonPanel>

        <KlingonPanel title="KRIEGER" icon="◆" color={T.frameOuter} defaultOpen={true}>
          <div style={{ display:"flex", flexDirection:"column", gap:4 }}>
            {[
              { name:"1. Flotte", ships:18, status:"ORBIT",       loc:"Qo'noS", pwr:90 },
              { name:"2. Flotte", ships:12, status:"ANGRIFF",     loc:"→ Romulus", pwr:75 },
              { name:"3. Flotte", ships:7,  status:"PATROUILLE",  loc:"Sektor 23", pwr:45 },
            ].map(f => (
              <div key={f.name} style={{ background:`linear-gradient(90deg, ${T.frameDark}88, ${T.bg}44)`, padding:"5px 8px", clipPath:"polygon(0 0, calc(100% - 10px) 0, 100% 10px, 100% 100%, 10px 100%, 0 calc(100% - 10px))", position:"relative" }}>
                <div style={{ position:"absolute", left:0, top:0, width:2, height:"100%", background:f.status==="ANGRIFF"?T.redBright:T.red+"88" }}/>
                <div style={{ display:"flex", justifyContent:"space-between", alignItems:"baseline", marginBottom:3 }}>
                  <span style={{ fontSize:14, fontWeight:700, color:T.goldBright, fontFamily:KPIQAD, letterSpacing:1 }}>{f.name}</span>
                  <span style={{ fontSize:8, color:T.gold, fontFamily:KFONT }}>{f.ships} ⚔</span>
                </div>
                <div style={{ height:3, background:T.frameDark, clipPath:"polygon(1px 0, 100% 0, calc(100% - 1px) 100%, 0 100%)", marginBottom:3 }}>
                  <div style={{ height:"100%", width:`${f.pwr}%`, background:`linear-gradient(90deg, ${T.red}, ${f.pwr>70?T.gold:T.redBright})` }}/>
                </div>
                <div style={{ display:"flex", justifyContent:"space-between", alignItems:"center" }}>
                  <span style={{ fontSize:7, color:f.status==="ANGRIFF"?T.redBright:T.textDim, fontFamily:KFONT, letterSpacing:2, fontWeight:700 }}>{f.status}</span>
                  <span style={{ fontSize:8, color:T.text+"AA", fontFamily:KFONT }}>{f.loc}</span>
                </div>
              </div>
            ))}
            <KDivider/>
            <div style={{ display:"flex", gap:4 }}><KButton label="NEUE FLOTTE"/><KButton label="ALLE ZEIGEN"/></div>
          </div>
        </KlingonPanel>
      </div>
    </div>
  );
}

// ─── Mount ───────────────────────────────────────────────────────────────────

const container = document.getElementById("klingon-root");
if (container) createRoot(container).render(<KlingonUI />);
