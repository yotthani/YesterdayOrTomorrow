import { useState, useEffect, useRef } from "react";
import { createRoot } from "react-dom/client";

// ─── Theme Definitions ────────────────────────────────────────────────────────

interface LcarsTheme {
  name: string;
  sub: string;
  elbowTL: string;
  elbowBL: string;
  elbowTR: string;
  elbowBR: string;
  topBar: string;
  botBar: string;
  subElbow: string;
  subBar: string;
  topPills: string[];
  navColors: string[];
  rightSegs: string[];
  accent: string;
  text: string;
  textBright: string;
  textDim: string;
  bg: string;
  resColors: string[];
}

const THEMES: Record<string, LcarsTheme> = {
  classic: {
    name: "LCARS • 47988", sub: "CLASSIC",
    elbowTL: "#CC6699", elbowBL: "#CC6666", elbowTR: "#FFCC99", elbowBR: "#CC6699",
    topBar: "#FFCC99", botBar: "#CC6666",
    subElbow: "#BB5588", subBar: "#EEBC88",
    topPills: ["#CC99CC", "#FF9966", "#CC6666"],
    navColors: ["#CC6699", "#CC6666", "#CC6699", "#CC6666", "#CC6699", "#CC6666", "#CC6699", "#CC6666", "#CC6699", "#CC6666"],
    rightSegs: ["#FFCC99", "#CC99CC", "#FF9966", "#CC6666", "#FFCC99"],
    accent: "#FF9900", text: "#FF9966", textBright: "#FFCC99", textDim: "#776655", bg: "#000",
    resColors: ["#CC6699", "#FFCC99", "#CC99CC", "#FF9966", "#CC6666"],
  },
  nemesis: {
    name: "LCARS • 56844", sub: "NEMESIS BLUE",
    elbowTL: "#BBAA88", elbowBL: "#4488CC", elbowTR: "#BBAA88", elbowBR: "#4488CC",
    topBar: "#BBAA88", botBar: "#4488CC",
    subElbow: "#3A75B0", subBar: "#99AA88",
    topPills: ["#5599DD", "#CC6666", "#4488CC"],
    navColors: ["#4488CC", "#5599DD", "#4488CC", "#5599DD", "#4488CC", "#5599DD", "#4488CC", "#5599DD", "#4488CC", "#5599DD"],
    rightSegs: ["#BBAA88", "#5599DD", "#4488CC", "#CC6666", "#5599DD"],
    accent: "#FF9900", text: "#88BBEE", textBright: "#CCDDFF", textDim: "#556688", bg: "#000",
    resColors: ["#5599DD", "#BBAA88", "#4488CC", "#CC6666", "#5599DD"],
  },
  lowerdecks: {
    name: "LCARS 2380", sub: "LOWER DECKS",
    elbowTL: "#FF6600", elbowBL: "#FF4400", elbowTR: "#FF6600", elbowBR: "#FF4400",
    topBar: "#FF6600", botBar: "#FF4400",
    subElbow: "#DD5500", subBar: "#DD5500",
    topPills: ["#CC4444", "#FF6600", "#FF4400"],
    navColors: ["#FF6600", "#FF4400", "#FF6600", "#FF4400", "#FF6600", "#FF4400", "#FF6600", "#FF4400", "#FF6600", "#FF4400"],
    rightSegs: ["#FF6600", "#CC4444", "#FF4400", "#FF6600", "#CC4444"],
    accent: "#FF8800", text: "#FF9955", textBright: "#FFBB88", textDim: "#884422", bg: "#000",
    resColors: ["#FF6600", "#FF4400", "#CC4444", "#FF6600", "#FF4400"],
  },
  padd: {
    name: "LCARS 57436.2", sub: "PADD",
    elbowTL: "#FF6600", elbowBL: "#FF6600", elbowTR: "#88BBCC", elbowBR: "#4488BB",
    topBar: "#88BBCC", botBar: "#4488BB",
    subElbow: "#DD5500", subBar: "#7099AA",
    topPills: ["#DDAA66", "#CC5544", "#88BBCC"],
    navColors: ["#FF6600", "#4488BB", "#FF6600", "#4488BB", "#FF6600", "#4488BB", "#FF6600", "#4488BB", "#FF6600", "#4488BB"],
    rightSegs: ["#88BBCC", "#CC5544", "#4488BB", "#FF6600", "#88BBCC"],
    accent: "#FF8800", text: "#AACCDD", textBright: "#DDEEFF", textDim: "#557788", bg: "#000",
    resColors: ["#FF6600", "#88BBCC", "#4488BB", "#CC5544", "#DDAA66"],
  },
};

// ─── Nav Items ────────────────────────────────────────────────────────────────

const NAV = [
  { key: "galaxy",    icon: "✧", label: "GALAXY" },
  { key: "systems",   icon: "●", label: "SYSTEMS" },
  { key: "fleets",    icon: "▲", label: "FLEETS" },
  { key: "diplomacy", icon: "⬡", label: "DIPLOMACY" },
  { key: "research",  icon: "◎", label: "RESEARCH" },
  { key: "economy",   icon: "◈", label: "ECONOMY" },
];

const RES = [
  { icon: "⚡", label: "ENERGY",    val: 12450 },
  { icon: "💎", label: "MINERALS",  val: 3280 },
  { icon: "🔬", label: "RESEARCH",  val: 890 },
  { icon: "🌾", label: "FOOD",      val: 5610 },
  { icon: "🔩", label: "ALLOYS",    val: 2100 },
];

// ─── Starfield ────────────────────────────────────────────────────────────────

interface Star { x: number; y: number; r: number; b: number; sp: number; ph: number; }

function Starfield({ w, h }: { w: number; h: number }) {
  const ref = useRef<HTMLCanvasElement>(null);
  const stars = useRef<Star[]>([]);
  const raf = useRef<number>(0);

  useEffect(() => {
    stars.current = Array.from(
      { length: Math.min(300, Math.floor(w * h / 3000)) },
      () => ({ x: Math.random() * w, y: Math.random() * h, r: Math.random() * 1.1 + .2, b: Math.random(), sp: Math.random() * .003 + .001, ph: Math.random() * 6.28 })
    );
  }, [w, h]);

  useEffect(() => {
    const ctx = ref.current?.getContext("2d");
    if (!ctx) return;
    let t = 0;
    const draw = () => {
      ctx.clearRect(0, 0, w, h);
      t += .016;
      stars.current.forEach(s => {
        const a = .15 + .6 * s.b * (.5 + .5 * Math.sin(t * s.sp * 250 + s.ph));
        ctx.beginPath();
        ctx.arc(s.x, s.y, s.r, 0, 6.28);
        ctx.fillStyle = `rgba(200,210,230,${a})`;
        ctx.fill();
      });
      raf.current = requestAnimationFrame(draw);
    };
    draw();
    return () => cancelAnimationFrame(raf.current);
  }, [w, h]);

  return <canvas ref={ref} width={w} height={h} style={{ position: "absolute", inset: 0 }} />;
}

// ─── Data Grid (top bar decoration) ──────────────────────────────────────────

function DataGrid({ count = 40 }: { count?: number }) {
  const nums = useRef(
    Array.from({ length: count }, () => {
      const l = 2 + Math.floor(Math.random() * 4);
      return Math.floor(Math.random() * (10 ** l)).toString().padStart(l, "0");
    })
  );
  return (
    <div style={{ display: "flex", flexWrap: "wrap", gap: "0 6px", alignItems: "center", overflow: "hidden", height: "100%" }}>
      {nums.current.map((n, i) => (
        <span key={i} style={{ fontSize: 9, fontFamily: "'Courier New',monospace", color: i % 7 === 0 ? "#00000066" : "#00000044", letterSpacing: .5, lineHeight: "11px" }}>{n}</span>
      ))}
    </div>
  );
}

// ─── UI Test Panels ───────────────────────────────────────────────────────────

function LcarsPanel({ title, accentColor, barColor, children }: {
  title: string;
  accentColor: string;
  barColor: string;
  children: React.ReactNode;
}) {
  const hH = 18, fH = 10, lW = 5, rW = 3, eW = 22, eH = 10, iR = 7, oR = 5;
  return (
    <div style={{ position: "relative", marginBottom: 12 }}>
      {/* TL elbow */}
      <div style={{ position: "absolute", top: 0, left: 0, width: eW + iR, height: hH + eH, background: accentColor, borderTopLeftRadius: oR, overflow: "hidden", zIndex: 1 }}>
        <div style={{ position: "absolute", bottom: 0, right: 0, width: iR + 1, height: eH, background: "rgba(0,0,0,0.7)", borderTopLeftRadius: iR }} />
      </div>
      {/* Top bar */}
      <div style={{ position: "absolute", top: 0, left: eW + iR, right: eW + iR, height: hH, background: barColor, zIndex: 1, display: "flex", alignItems: "center", justifyContent: "center" }}>
        <span style={{ fontSize: 7, fontWeight: 700, letterSpacing: 2, color: "#000", fontFamily: "'Courier New',monospace" }}>{title}</span>
      </div>
      {/* TR elbow */}
      <div style={{ position: "absolute", top: 0, right: 0, width: eW + iR, height: hH + eH, background: barColor, borderTopRightRadius: oR, overflow: "hidden", zIndex: 1 }}>
        <div style={{ position: "absolute", bottom: 0, left: 0, width: iR + 1, height: eH, background: "rgba(0,0,0,0.7)", borderTopRightRadius: iR }} />
      </div>
      {/* Left bar */}
      <div style={{ position: "absolute", top: hH + eH, left: 0, bottom: fH + eH * .6, width: lW, background: accentColor, zIndex: 1 }} />
      {/* Right bar */}
      <div style={{ position: "absolute", top: hH + eH, right: 0, bottom: fH + eH * .6, width: rW, background: barColor, zIndex: 1 }} />
      {/* BL elbow */}
      <div style={{ position: "absolute", bottom: 0, left: 0, width: eW + iR * .7, height: fH + eH * .6, background: accentColor, borderBottomLeftRadius: oR, overflow: "hidden", zIndex: 1 }}>
        <div style={{ position: "absolute", top: 0, right: 0, width: iR + 1, height: eH * .6, background: "rgba(0,0,0,0.7)", borderBottomLeftRadius: iR * .7 }} />
      </div>
      {/* Bottom bar */}
      <div style={{ position: "absolute", bottom: 0, left: eW + iR * .7, right: eW + iR * .7, height: fH, background: barColor, zIndex: 1 }} />
      {/* BR elbow */}
      <div style={{ position: "absolute", bottom: 0, right: 0, width: eW + iR * .7, height: fH + eH * .6, background: barColor, borderBottomRightRadius: oR, overflow: "hidden", zIndex: 1 }}>
        <div style={{ position: "absolute", top: 0, left: 0, width: iR + 1, height: eH * .6, background: "rgba(0,0,0,0.7)", borderBottomRightRadius: iR * .7 }} />
      </div>
      {/* Body */}
      <div style={{ margin: `${hH}px ${rW}px ${fH}px ${lW}px`, background: "rgba(0,0,0,0.55)", backdropFilter: "blur(8px)", padding: "10px 12px", minHeight: 40 }}>
        {children}
      </div>
    </div>
  );
}

function LcarsBtn({ T, children, danger }: { T: LcarsTheme; children: React.ReactNode; danger?: boolean }) {
  const bg = danger ? "#882222" : T.elbowTL + "CC";
  const col = danger ? "#FF6666" : T.textBright;
  return (
    <button style={{ background: bg, border: `1px solid ${danger ? "#CC3333" : T.elbowTL}`, borderRadius: 12, padding: "5px 14px", fontSize: 8, fontWeight: 700, letterSpacing: 1.5, color: col, cursor: "pointer", fontFamily: "'Courier New',monospace", transition: "filter 0.15s" }}
      onMouseEnter={e => (e.currentTarget.style.filter = "brightness(1.3)")}
      onMouseLeave={e => (e.currentTarget.style.filter = "brightness(1)")}
    >{children}</button>
  );
}

function StatusBar({ label, value, color }: { label: string; value: number; color: string }) {
  return (
    <div style={{ display: "flex", alignItems: "center", gap: 8, marginBottom: 8 }}>
      <span style={{ width: 90, fontSize: 8, color: "#88888A", fontFamily: "'Courier New',monospace", letterSpacing: 1 }}>{label}</span>
      <div style={{ flex: 1, height: 6, background: "rgba(255,255,255,0.08)", borderRadius: 3, overflow: "hidden" }}>
        <div style={{ width: `${value}%`, height: "100%", background: color, borderRadius: 3 }} />
      </div>
      <span style={{ width: 32, fontSize: 9, fontWeight: 700, color: "#CCCCCC", fontFamily: "'Courier New',monospace", textAlign: "right" }}>{value}%</span>
    </div>
  );
}

// ─── Main Content Panels (mirrors ThemeTest.razor) ───────────────────────────

function ContentPanels({ T }: { T: LcarsTheme }) {
  return (
    <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>

      {/* Buttons Test */}
      <LcarsPanel title="TEMPLATED BUTTONS" accentColor={T.elbowTL} barColor={T.topBar}>
        <div style={{ display: "flex", flexWrap: "wrap", gap: 8, marginBottom: 10 }}>
          <LcarsBtn T={T}>PRIMARY</LcarsBtn>
          <LcarsBtn T={T}>SECONDARY</LcarsBtn>
          <LcarsBtn T={T} danger>DANGER</LcarsBtn>
        </div>
        <p style={{ fontSize: 9, color: T.textDim, fontFamily: "'Courier New',monospace", margin: 0, lineHeight: 1.6 }}>
          Shape: <span style={{ color: T.text }}>rounded</span><br />
          Structure: <span style={{ color: T.text }}>icon → label → value</span>
        </p>
      </LcarsPanel>

      {/* Status Display */}
      <LcarsPanel title="STATUS DISPLAY" accentColor={T.elbowTR} barColor={T.topBar}>
        <StatusBar label="Hull Integrity" value={85} color="#44FF88" />
        <StatusBar label="Shield Power"   value={62} color="#4488FF" />
        <StatusBar label="Critical"       value={23} color="#FF4444" />
      </LcarsPanel>

      {/* Fleet Roster */}
      <LcarsPanel title="FLEET ROSTER" accentColor={T.elbowBL} barColor={T.botBar}>
        {[
          { name: "1st Exploration Fleet", status: "ACTIVE",  active: true },
          { name: "Home Defense Force",    status: "PATROL",  active: true },
          { name: "Construction Corps",    status: "DOCKED",  active: false },
        ].map(f => (
          <div key={f.name} style={{ display: "flex", justifyContent: "space-between", alignItems: "center", padding: "6px 8px", background: "rgba(255,255,255,0.04)", borderRadius: 4, marginBottom: 5, borderLeft: `2px solid ${f.active ? T.elbowBL : "#444"}` }}>
            <span style={{ fontSize: 9, fontWeight: 700, color: T.textBright, fontFamily: "'Courier New',monospace" }}>{f.name}</span>
            <span style={{ fontSize: 7, padding: "2px 6px", borderRadius: 3, background: f.active ? "rgba(68,255,136,0.15)" : "rgba(100,100,100,0.15)", color: f.active ? "#44FF88" : "#888" }}>{f.status}</span>
          </div>
        ))}
      </LcarsPanel>

      {/* Color Swatches */}
      <LcarsPanel title="THEME COLORS" accentColor={T.elbowBR} barColor={T.botBar}>
        <div style={{ display: "flex", gap: 8, marginBottom: 10 }}>
          {[["P", T.elbowTL], ["S", T.topBar], ["A", T.accent], ["BL", T.elbowBL], ["BR", T.elbowBR]].map(([label, color]) => (
            <div key={label} style={{ width: 32, height: 32, borderRadius: 4, background: color as string, display: "flex", alignItems: "center", justifyContent: "center", fontSize: 8, fontWeight: 700, color: "#000", fontFamily: "'Courier New',monospace" }}>{label}</div>
          ))}
        </div>
        <div style={{ display: "flex", flexDirection: "column", gap: 3 }}>
          {[["Primary", T.elbowTL], ["Top Bar", T.topBar], ["Accent", T.accent]].map(([label, color]) => (
            <div key={label} style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
              <span style={{ fontSize: 8, color: T.textDim, fontFamily: "'Courier New',monospace", letterSpacing: 1 }}>{label as string}</span>
              <span style={{ fontSize: 8, fontWeight: 700, color: color as string, fontFamily: "'Courier New',monospace" }}>{color as string}</span>
            </div>
          ))}
        </div>
      </LcarsPanel>
    </div>
  );
}

// ─── Right Context Panel ──────────────────────────────────────────────────────

function ContextPanel({ T }: { T: LcarsTheme }) {
  return (
    <LcarsPanel title="SELECTION INFO" accentColor={T.elbowTR} barColor={T.topBar}>
      <div style={{ fontSize: 14, fontWeight: 700, color: T.textBright, fontFamily: "'Courier New',monospace", marginBottom: 2 }}>USS Enterprise</div>
      <div style={{ fontSize: 9, color: T.textDim, fontFamily: "'Courier New',monospace", marginBottom: 10 }}>Constitution-class · NCC-1701</div>
      <div style={{ height: 1, background: T.elbowTL + "44", marginBottom: 10 }} />
      {[["Hull", "2,500"], ["Shields", "1,800"], ["Crew", "430"]].map(([k, v]) => (
        <div key={k} style={{ display: "flex", justifyContent: "space-between", padding: "4px 0" }}>
          <span style={{ fontSize: 9, color: T.textDim, fontFamily: "'Courier New',monospace" }}>{k}</span>
          <span style={{ fontSize: 10, fontWeight: 700, color: T.text, fontFamily: "'Courier New',monospace" }}>{v}</span>
        </div>
      ))}
      <div style={{ height: 1, background: T.elbowTL + "44", margin: "10px 0" }} />
      <div style={{ display: "flex", flexDirection: "column", gap: 6 }}>
        <LcarsBtn T={T}>MOVE</LcarsBtn>
        <LcarsBtn T={T}>PATROL</LcarsBtn>
        <LcarsBtn T={T} danger>ATTACK</LcarsBtn>
      </div>
    </LcarsPanel>
  );
}

// ─── Main App ────────────────────────────────────────────────────────────────

export default function LCARSNav() {
  const [themeKey, setThemeKey] = useState<string>("classic");
  const T = THEMES[themeKey];
  const [activeNav, setActiveNav] = useState("galaxy");
  const [navOpen, setNavOpen] = useState(false);
  const rootRef = useRef<HTMLDivElement>(null);
  const [dims, setDims] = useState({ w: 1200, h: 800 });

  useEffect(() => {
    const m = () => { if (rootRef.current) setDims({ w: rootRef.current.offsetWidth, h: rootRef.current.offsetHeight }); };
    m();
    window.addEventListener("resize", m);
    return () => window.removeEventListener("resize", m);
  }, []);

  const W = dims.w, H = dims.h;
  const LW_MIN = 70, LW_MAX = 180;
  const LW = navOpen ? LW_MAX : LW_MIN;

  const TH = 32, RW = 8, BH = 12, oR = 18, iR = 30, elbowH = 50, G = 3;

  const SUB_H = 22, SUB_GAP = 3, subRElbow = 14, subRW = 6;
  const subTop = TH + SUB_GAP;
  const subElbowRW = subRElbow + subRW + 4;
  const subBarRight = RW + 4 + subElbowRW - 2;
  const subElbowH = 30;

  const [resExpanded, setResExpanded] = useState(false);
  const resExpandH = 48;
  const subTotalH = SUB_H + (resExpanded ? resExpandH : 0);

  const ease = "all 0.25s cubic-bezier(0.4, 0, 0.2, 1)";
  const navTop = TH + elbowH;
  const navBot = H - BH - elbowH;
  const rightSegTop = TH + Math.floor(elbowH * .6);
  const rightSegBot = H - BH - Math.floor(elbowH * .6);
  const rightSegH = (rightSegBot - rightSegTop) / T.rightSegs.length;

  const totalTopH = subTop + subTotalH;
  const contentT = totalTopH + G;
  const contentL = LW_MIN + G;
  const contentR = RW + G + 240; // leave room for context panel on right

  return (
    <div ref={rootRef} style={{ width: "100vw", height: "100vh", background: T.bg, overflow: "hidden", position: "relative", userSelect: "none" }}>
      <Starfield w={W} h={H} />

      {/* ── TL ELBOW ── */}
      <div style={{ position: "absolute", top: 0, left: 0, width: LW + iR + 20, height: TH + elbowH, background: T.elbowTL, borderTopLeftRadius: oR, zIndex: 10, transition: ease, overflow: "hidden" }}>
        <div style={{ position: "absolute", bottom: 0, right: 0, width: iR + 20, height: elbowH, background: T.bg, borderTopLeftRadius: iR, transition: ease }} />
      </div>

      {/* ── TOP BAR ── */}
      <div style={{ position: "absolute", top: 0, left: LW + iR + 20, right: RW + iR + 10, height: TH, background: T.topBar, zIndex: 10, transition: ease }} />

      {/* ── TR CORNER ── */}
      <div style={{ position: "absolute", top: 0, right: 0, width: RW + iR + 10, height: TH + elbowH * .6, background: T.elbowTR, borderTopRightRadius: oR, zIndex: 10, overflow: "hidden" }}>
        <div style={{ position: "absolute", bottom: 0, left: 0, width: iR + 10, height: elbowH * .6, background: T.bg, borderTopRightRadius: iR * .6 }} />
      </div>

      {/* ── SUB-FRAME: resource bar ── */}
      <div
        style={{ position: "absolute", top: subTop, left: LW + iR + 20, right: subBarRight, height: subTotalH, background: T.subBar, zIndex: 11, overflow: "hidden", transition: ease, cursor: "pointer" }}
        onClick={() => setResExpanded(v => !v)}
      >
        <div style={{ height: SUB_H, display: "flex", alignItems: "center", padding: "0 8px", gap: 0 }}>
          {RES.map((r, i) => (
            <div key={r.label} style={{ flex: 1, display: "flex", alignItems: "center", justifyContent: "center", gap: 4, borderRight: i === RES.length - 1 ? "none" : `1px solid ${T.bg}`, height: "100%", padding: "0 2px" }}>
              <span style={{ fontSize: 9, color: "#00000066", lineHeight: 1 }}>{r.icon}</span>
              <span style={{ fontSize: 7, fontWeight: 600, color: "#00000066", fontFamily: "'Courier New',monospace", letterSpacing: 1, lineHeight: 1 }}>{r.label}</span>
              <span style={{ fontSize: 11, fontWeight: 700, color: "#000", fontFamily: "'Courier New',monospace", letterSpacing: .5, lineHeight: 1 }}>{r.val.toLocaleString()}</span>
            </div>
          ))}
          <div style={{ marginLeft: 4, fontSize: 8, color: "#00000066", transform: resExpanded ? "rotate(180deg)" : "none", transition: "transform 0.2s" }}>▼</div>
        </div>
        {resExpanded && (
          <div style={{ padding: "4px 10px 6px", display: "flex", gap: 0 }}>
            {RES.map((r, i) => (
              <div key={r.label} style={{ flex: 1, borderRight: i === RES.length - 1 ? "none" : `1px solid ${T.bg}`, padding: "0 6px" }}>
                <div style={{ display: "flex", justifyContent: "space-between" }}>
                  <span style={{ fontSize: 7, color: "#00000077", fontFamily: "'Courier New',monospace" }}>Income</span>
                  <span style={{ fontSize: 8, fontWeight: 700, color: "#00000099", fontFamily: "'Courier New',monospace" }}>+{Math.floor(r.val / 120)}</span>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* ── SUB-FRAME: right elbow ── */}
      <div style={{ position: "absolute", top: subTop, right: RW + 4, width: subElbowRW, height: subTotalH + subElbowH * .5, background: T.subBar, zIndex: 12, overflow: "hidden", transition: ease, borderTopRightRadius: subRElbow, borderBottomRightRadius: subRElbow * .6 }}>
        <div style={{ position: "absolute", bottom: 0, left: 0, width: subRElbow + 4, height: subElbowH * .5, background: T.bg, borderTopRightRadius: subRElbow, transition: ease }} />
      </div>

      {/* ── LEFT NAV ── */}
      <div
        onMouseEnter={() => setNavOpen(true)}
        onMouseLeave={() => setNavOpen(false)}
        style={{ position: "absolute", top: navTop, left: 0, width: LW, height: navBot - navTop, zIndex: 12, transition: ease, background: T.elbowTL, boxShadow: "inset 0 0 0 100px rgba(0,0,0,0.25)" }}
      >
        <div style={{ position: "absolute", inset: 0, display: "flex", flexDirection: "column", justifyContent: "center", gap: 4, padding: "6px" }}>
          {NAV.map((item, i) => {
            const isActive = activeNav === item.key;
            const btnColor = T.navColors[i % 2 === 0 ? 0 : 1];
            const pillH = Math.min(28, Math.max(18, ((navBot - navTop - 12 - (NAV.length - 1) * 4) / NAV.length)));
            return (
              <button
                key={item.key}
                onClick={() => setActiveNav(item.key)}
                style={{ height: pillH, minHeight: pillH, border: "none", cursor: "pointer", borderRadius: pillH / 2, background: isActive ? btnColor : btnColor + "BB", display: "flex", alignItems: "center", justifyContent: navOpen ? "flex-start" : "center", gap: 8, padding: navOpen ? `0 ${pillH / 2}px` : "0", overflow: "hidden", transition: "background 0.15s, padding 0.25s" }}
                onMouseEnter={e => { (e.currentTarget as HTMLButtonElement).style.background = btnColor; }}
                onMouseLeave={e => { (e.currentTarget as HTMLButtonElement).style.background = isActive ? btnColor : btnColor + "BB"; }}
              >
                <span style={{ fontSize: 11, color: "#000", lineHeight: 1, flexShrink: 0, opacity: isActive ? 1 : 0.75 }}>{item.icon}</span>
                <span style={{ fontSize: 8, fontWeight: 700, letterSpacing: 2, color: "#000", fontFamily: "'Courier New',monospace", whiteSpace: "nowrap", opacity: navOpen ? (isActive ? 0.9 : 0.65) : 0, maxWidth: navOpen ? 120 : 0, transition: `opacity 0.15s ${navOpen ? "0.1s" : "0s"}, max-width 0.25s` }}>{item.label}</span>
              </button>
            );
          })}
        </div>
      </div>

      {/* ── BL ELBOW ── */}
      <div style={{ position: "absolute", bottom: 0, left: 0, width: LW + iR + 20, height: BH + elbowH, background: T.elbowBL, borderBottomLeftRadius: oR, zIndex: 10, transition: ease, overflow: "hidden" }}>
        <div style={{ position: "absolute", top: 0, right: 0, width: iR + 20, height: elbowH, background: T.bg, borderBottomLeftRadius: iR, transition: ease }} />
      </div>

      {/* ── BOTTOM BAR ── */}
      <div style={{ position: "absolute", bottom: 0, left: LW + iR + 20, right: RW + iR + 10, height: BH, background: T.botBar, zIndex: 10, transition: ease, display: "flex", alignItems: "center", justifyContent: "center", gap: 24 }}>
        <span style={{ fontSize: 7, fontWeight: 600, color: "#00000055", fontFamily: "'Courier New',monospace", letterSpacing: 1 }}>STARFLEET COMMAND: ONLINE</span>
        <span style={{ fontSize: 7, fontWeight: 700, color: "#00000077", fontFamily: "'Courier New',monospace", letterSpacing: 1 }}>STERNZEIT 47988.1</span>
        <span style={{ fontSize: 7, fontWeight: 600, color: "#00000055", fontFamily: "'Courier New',monospace", letterSpacing: 1 }}>MISSION: EXPLORATION</span>
      </div>

      {/* ── BR CORNER ── */}
      <div style={{ position: "absolute", bottom: 0, right: 0, width: RW + iR + 10, height: BH + elbowH * .6, background: T.elbowBR, borderBottomRightRadius: oR, zIndex: 10, overflow: "hidden" }}>
        <div style={{ position: "absolute", top: 0, left: 0, width: iR + 10, height: elbowH * .6, background: T.bg, borderBottomRightRadius: iR * .6 }} />
      </div>

      {/* ── RIGHT SEGMENTS ── */}
      {T.rightSegs.map((c, i) => (
        <div key={`rs${i}`} style={{ position: "absolute", top: rightSegTop + i * rightSegH, right: 0, width: RW, height: rightSegH + .5, background: c, zIndex: 10 }} />
      ))}

      {/* ── TOP BAR CONTENT ── */}
      <div style={{ position: "absolute", top: 0, left: 0, right: 0, height: TH, zIndex: 15, display: "flex", alignItems: "center", pointerEvents: "none" }}>
        <div style={{ width: LW_MIN + iR + 20, flexShrink: 0, display: "flex", alignItems: "center", justifyContent: "space-between", padding: "0 10px" }}>
          <div>
            <div style={{ fontSize: 9, fontWeight: 700, letterSpacing: 2, color: "#000", fontFamily: "'Courier New',monospace", lineHeight: 1 }}>LCARS</div>
            <div style={{ fontSize: 6.5, color: "#00000066", fontFamily: "'Courier New',monospace", letterSpacing: 1, lineHeight: "9px" }}>GALACTIC STRATEGY</div>
          </div>
          <div style={{ textAlign: "right" }}>
            <div style={{ fontSize: 6, color: "#00000055", fontFamily: "'Courier New',monospace", letterSpacing: 1, lineHeight: 1 }}>STERNZEIT</div>
            <div style={{ fontSize: 10, fontWeight: 700, color: "#000", fontFamily: "'Courier New',monospace", letterSpacing: .5, lineHeight: 1 }}>47988.1</div>
          </div>
        </div>
        <div style={{ flex: 1, overflow: "hidden", display: "flex", alignItems: "center", padding: "0 8px" }}>
          <DataGrid count={Math.max(15, Math.floor((W - LW_MIN - 400) / 30))} />
        </div>
        <div style={{ display: "flex", alignItems: "center", gap: 4, paddingRight: RW + iR + 18, pointerEvents: "auto" }}>
          <button style={{ background: T.topPills[2] || T.topBar, border: "none", borderRadius: 14, padding: "7px 12px", fontSize: 13, fontWeight: 700, color: "#000", cursor: "pointer", lineHeight: 1 }}>⚙</button>
          <button style={{ background: T.topPills[0] || T.topBar, border: "none", borderRadius: 14, padding: "7px 12px", fontSize: 13, fontWeight: 700, color: "#000", cursor: "pointer", lineHeight: 1 }}>☰</button>
          <button style={{ background: T.elbowBL, border: "none", borderRadius: 14, padding: "7px 18px", fontSize: 11, fontWeight: 700, letterSpacing: 2, color: "#fff", cursor: "pointer", fontFamily: "'Courier New',monospace", boxShadow: `0 0 12px ${T.elbowBL}44` }}>END TURN</button>
        </div>
      </div>

      {/* ── MAIN CONTENT PANELS ── */}
      <div style={{ position: "absolute", top: contentT, left: contentL, right: contentR, bottom: BH + G, overflowY: "auto", overflowX: "hidden", zIndex: 5, padding: "12px 16px 12px 12px" }}>
        <ContentPanels T={T} />
      </div>

      {/* ── RIGHT CONTEXT PANEL ── */}
      <div style={{ position: "absolute", top: contentT + 8, right: RW + G + 6, width: 230, bottom: BH + G + 8, overflowY: "auto", zIndex: 14, pointerEvents: "auto" }}>
        <ContextPanel T={T} />
      </div>

      {/* ── THEME SWITCHER ── */}
      <div style={{ position: "absolute", bottom: BH + 12, left: contentL + 8, zIndex: 30, display: "flex", gap: 3 }}>
        {Object.entries(THEMES).map(([k, t]) => {
          const active = themeKey === k;
          return (
            <button
              key={k}
              onClick={() => setThemeKey(k)}
              style={{ background: active ? t.elbowTL : t.elbowTL + "33", border: "none", borderRadius: 10, padding: "3px 10px", fontSize: 8, fontWeight: 700, letterSpacing: 1.2, color: active ? "#000" : t.elbowTL, cursor: "pointer", fontFamily: "'Courier New',monospace", transition: "all .15s" }}
              onMouseEnter={e => { if (!active) (e.currentTarget as HTMLButtonElement).style.background = t.elbowTL + "66"; }}
              onMouseLeave={e => { if (!active) (e.currentTarget as HTMLButtonElement).style.background = t.elbowTL + "33"; }}
            >{t.sub}</button>
          );
        })}
      </div>
    </div>
  );
}

// ─── Mount ───────────────────────────────────────────────────────────────────

const container = document.getElementById("lcars-root");
if (container) {
  createRoot(container).render(<LCARSNav />);
}
