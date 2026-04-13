// @ts-nocheck
import { useState, useEffect, useRef, useCallback } from "react";
import { createRoot } from "react-dom/client";

/*
  Converted from ui/templates/romulan-ui.jsx for theme mockups.
*/

// ═══════════════════════════════════════════════════════
//  ROMULAN STAR EMPIRE THEME
// ═══════════════════════════════════════════════════════
const RFONT = "'Orbitron', 'Rajdhani', sans-serif";        // sleek geometric — readable
const RALIEN = "'RomulanFont', 'Orbitron', sans-serif";    // alien script decorative

const T = {
  frameOuter: "#00AA88",
  frameInner: "#008866",
  frameDark: "#004433",
  frameDim: "#003328",
  pink: "#CC4488",
  pinkBright: "#FF66AA",
  pinkDark: "#882255",
  pinkDim: "#551133",
  purple: "#7755AA",
  purpleBright: "#9977CC",
  purpleDark: "#443366",
  purpleDim: "#332244",
  orange: "#DD9922",
  orangeBright: "#FFBB33",
  orangeDim: "#886611",
  green: "#00CC99",
  greenBright: "#33FFBB",
  greenDim: "#006644",
  teal: "#00BBAA",
  text: "#88CCAA",
  textBright: "#AAFFDD",
  textDim: "#446655",
  bg: "#080E12",
  bgPanel: "#0C1820",
  bgDark: "#060A0E",
  accent: "#00DDAA",
};

const NAV = [
  { key:"galaxy",  icon:"◈", label:"SEKTOR" },
  { key:"planets", icon:"◉", label:"WELTEN" },
  { key:"fleets",  icon:"▲", label:"FLOTTEN" },
  { key:"research",icon:"◆", label:"FORSCHUNG" },
  { key:"diplomacy",icon:"⬡",label:"TAL SHIAR" },
  { key:"intel",   icon:"◎", label:"SPIONAGE" },
  { key:"leaders", icon:"⚡", label:"SENAT" },
  { key:"trade",   icon:"▼", label:"HANDEL" },
];

const RES = [
  { icon:"⚡",label:"DILITHIUM", val:2614 },
  { icon:"◈",label:"KREDITE",   val:5830 },
  { icon:"◎",label:"FORSCHUNG", val:1244 },
  { icon:"⬡",label:"PRODUKTION",val:3470 },
  { icon:"◆",label:"EINFLUSS",  val:892  },
];

const SYSTEMS=[
  {id:1,name:"Romulus",x:.45,y:.4,fac:"rom",type:"hw"},
  {id:2,name:"Remus",x:.48,y:.42,fac:"rom",type:"col"},
  {id:3,name:"Unroth III",x:.38,y:.34,fac:"rom",type:"col"},
  {id:4,name:"Gasko",x:.52,y:.32,fac:"rom",type:"col"},
  {id:5,name:"Nelvana III",x:.56,y:.38,fac:"rom",type:"col"},
  {id:6,name:"Virinat",x:.4,y:.48,fac:"rom",type:"col"},
  ...Array.from({length:14},(_,i)=>({id:7+i,name:"Unbekannt",x:.12+Math.sin(i*2.1)*.38+.35,y:.1+Math.cos(i*1.8)*.38+.35,fac:null,type:null}))
];
const LINKS=[[1,2],[1,3],[1,6],[2,4],[3,4],[4,5],[1,4],[5,6],[3,7],[5,9],[6,11],[4,12],[5,14]];

// ═══════════════════════════════════════════════════════
//  HEX GRID BACKGROUND (Romulan geometric pattern)
// ═══════════════════════════════════════════════════════
function HexGrid({ w, h }) {
  const ref = useRef(null);
  const raf = useRef(null);

  useEffect(() => {
    const ctx = ref.current?.getContext("2d");
    if (!ctx) return;
    let t = 0;
    const draw = () => {
      ctx.clearRect(0, 0, w, h);

      // Subtle teal radial glow
      const g1 = ctx.createRadialGradient(w*.5, h*.45, 0, w*.5, h*.45, w*.5);
      g1.addColorStop(0, "rgba(0,60,50,0.05)");
      g1.addColorStop(1, "transparent");
      ctx.fillStyle = g1;
      ctx.fillRect(0, 0, w, h);

      // Hex grid pattern
      const s = 30;
      const cols = Math.ceil(w / (s * 1.5)) + 1;
      const rows = Math.ceil(h / (s * Math.sqrt(3))) + 1;
      ctx.strokeStyle = `rgba(0,${80 + Math.sin(t * 0.02) * 20},${60 + Math.sin(t * 0.03) * 15},0.08)`;
      ctx.lineWidth = 0.5;
      for (let r = 0; r < rows; r++) {
        for (let c = 0; c < cols; c++) {
          const cx2 = c * s * 1.5;
          const cy2 = r * s * Math.sqrt(3) + (c % 2 ? s * Math.sqrt(3) * 0.5 : 0);
          ctx.beginPath();
          for (let i = 0; i < 6; i++) {
            const a = Math.PI / 3 * i - Math.PI / 6;
            const px = cx2 + s * 0.5 * Math.cos(a);
            const py = cy2 + s * 0.5 * Math.sin(a);
            i === 0 ? ctx.moveTo(px, py) : ctx.lineTo(px, py);
          }
          ctx.closePath();
          ctx.stroke();
        }
      }

      t++;
      raf.current = requestAnimationFrame(draw);
    };
    draw();
    return () => { if (raf.current) cancelAnimationFrame(raf.current); };
  }, [w, h]);

  return <canvas ref={ref} width={w} height={h} style={{ position:"absolute", inset:0, pointerEvents:"none" }} />;
}

// ═══════════════════════════════════════════════════════
//  ROMULAN PANEL — elegant beveled shape
// ═══════════════════════════════════════════════════════
function RomulanPanel({ title, icon, color, defaultOpen = true, width = 220, children }) {
  const [open, setOpen] = useState(defaultOpen);
  if (!open) {
    return (
      <div onClick={()=>setOpen(true)} style={{
        width:34, height:34, cursor:"pointer",
        background:T.bgPanel,
        border:`1px solid ${color||T.frameOuter}44`,
        clipPath:"polygon(6px 0, calc(100% - 6px) 0, 100% 6px, 100% calc(100% - 6px), calc(100% - 6px) 100%, 6px 100%, 0 calc(100% - 6px), 0 6px)",
        display:"flex", alignItems:"center", justifyContent:"center",
        alignSelf:"flex-end",
      }}>
        <span style={{ fontSize:12, color:color||T.green }}>{icon}</span>
      </div>
    );
  }

  // Romulan panels: beveled rectangle with color accent bars
  // More elegant than Klingon — tapered notches, not aggressive chamfers
  const bev = 14; // bevel size

  return (
    <div style={{ width, position:"relative" }}>
      {/* Panel frame */}
      <div style={{
        clipPath:`polygon(
          ${bev}px 0, calc(100% - ${bev}px) 0,
          100% ${bev}px, 100% calc(100% - ${bev}px),
          calc(100% - ${bev}px) 100%, ${bev}px 100%,
          0 calc(100% - ${bev}px), 0 ${bev}px
        )`,
        border:`1px solid ${color||T.frameOuter}66`,
        background:T.bgPanel + "EE",
        overflow:"hidden",
      }}>
        {/* Top color bar strip (like reference) */}
        <div style={{ display:"flex", height:3 }}>
          <div style={{ flex:2, background:color||T.frameOuter }}/>
          <div style={{ flex:1, background:T.purple }}/>
          <div style={{ width:4, background:T.orange }}/>
          <div style={{ flex:1, background:T.frameDark }}/>
        </div>

        {/* Header */}
        <div style={{ display:"flex", alignItems:"center", justifyContent:"space-between", padding:"6px 10px" }}>
          <div style={{ display:"flex", alignItems:"center", gap:6 }}>
            <span style={{ color:color||T.green, fontSize:14 }}>{icon}</span>
            <span style={{ color:T.textBright, fontFamily:RFONT, fontSize:11, letterSpacing:1, textTransform:"uppercase" }}>{title}</span>
          </div>
          <span onClick={()=>setOpen(false)} style={{ cursor:"pointer", color:T.textDim, fontSize:10 }}>▼</span>
        </div>

        {/* Content */}
        <div style={{ padding:"4px 10px 10px" }}>
          {children}
        </div>
      </div>
    </div>
  );
}

// ═══════════════════════════════════════════════════════
//  MAIN APP
// ═══════════════════════════════════════════════════════
function RomulanTestApp() {
  return (
    <div style={{ width:"100vw", height:"100vh", background:T.bg, color:T.text, fontFamily:RFONT, position:"relative", overflow:"hidden" }}>
      <HexGrid w={1920} h={1080} />
      <div style={{ position:"relative", zIndex:1, padding:20 }}>
        <h1 style={{ color:T.accent, fontFamily:RALIEN, fontSize:28, margin:0 }}>ROMULAN STAR EMPIRE</h1>
        <p style={{ color:T.textDim, fontSize:12, marginTop:4 }}>Theme Mockup — Test Page</p>
      </div>
    </div>
  );
}

// Mount
const container = document.getElementById("romulan-root");
if (container) createRoot(container).render(<RomulanTestApp />);
