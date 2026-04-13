// @ts-nocheck
import { useState, useEffect, useRef, useCallback } from "react";
import { createRoot } from "react-dom/client";

/*
  Converted from ui/templates/borg-ui.jsx for theme mockups.
  Original file provided dynamic Borg UI template; this TSX version adds mounting
  boilerplate and disables type checking since it's a design sandbox.
*/

const BFONT = "'Share Tech Mono', monospace";
const B = {
  green:"#00FF41",greenDim:"#00AA2A",greenDark:"#004D15",greenFade:"#002A0E",grid:"#001A08",
  bg:"#020502",bgAlt:"#040804",panel:"#0A0F0A",
  text:"#00DD38",textDim:"#006622",textBright:"#55FF88",
  red:"#FF2222",redDim:"#661111",amber:"#BBAA00",stroke:"#00CC33",
};

const SYSTEMS=[
  {id:1,name:"Unikomplex",x:.5,y:.42,fac:"borg",type:"hw"},
  {id:2,name:"Gitter 326",x:.42,y:.34,fac:"borg",type:"col"},
  {id:3,name:"Gitter 010",x:.58,y:.34,fac:"borg",type:"col"},
  {id:4,name:"Gitter 124",x:.40,y:.50,fac:"borg",type:"col"},
  {id:5,name:"Gitter 539",x:.60,y:.50,fac:"borg",type:"col"},
  {id:6,name:"Gitter 877",x:.34,y:.42,fac:"borg",type:"col"},
  {id:7,name:"Gitter 003",x:.66,y:.42,fac:"borg",type:"col"},
  ...Array.from({length:12},(_,i)=>({id:8+i,name:"Sektor "+(i*37+14)%999,x:.1+Math.sin(i*2.3)*.38+.36,y:.08+Math.cos(i*1.9)*.38+.36,fac:null,type:null}))
];
const LINKS=[[1,2],[1,3],[1,4],[1,5],[1,6],[1,7],[2,3],[4,5],[6,2],[7,3],[4,6],[5,7],[2,8],[3,10],[6,12],[7,14],[4,16],[5,18]];

// Data rain background
function DataRain({ w, h }) {
  const ref=useRef(null),cols=useRef([]),raf=useRef(null);
  useEffect(()=>{
    const ctx=ref.current?.getContext("2d");if(!ctx)return;
    const cW=16,n=Math.ceil(w/cW);
    if(cols.current.length!==n) cols.current=Array.from({length:n},()=>({y:Math.random()*h*2-h,speed:.2+Math.random()*.6,chars:Array.from({length:40},()=>String.fromCharCode(0x30A0+Math.random()*96)),bright:Math.random()}));
    const draw=()=>{
      ctx.fillStyle="rgba(2,5,2,0.08)";ctx.fillRect(0,0,w,h);
      ctx.strokeStyle="#001A0822";ctx.lineWidth=.3;
      for(let x=0;x<w;x+=50){ctx.beginPath();ctx.moveTo(x,0);ctx.lineTo(x,h);ctx.stroke();}
      for(let y=0;y<h;y+=50){ctx.beginPath();ctx.moveTo(0,y);ctx.lineTo(w,y);ctx.stroke();}
      ctx.font="10px "+BFONT;
      cols.current.forEach((col,i)=>{
        col.y+=col.speed;if(col.y>h+300){col.y=-200;col.bright=Math.random();}
        const x=i*cW+3;
        col.chars.forEach((ch,j)=>{
          const cy=col.y-j*13;if(cy<-10||cy>h+10)return;
          const fade=j===0?1:Math.max(0,1-j/25);const a=fade*(col.bright>.9?.15:.03);
          if(a<.005)return;
          ctx.fillStyle=j===0?`rgba(0,255,65,${a*2.5})`:`rgba(0,180,30,${a})`;
          ctx.fillText(ch,x,cy);
        });
        if(Math.random()>.96)col.chars[Math.random()*col.chars.length|0]=String.fromCharCode(0x30A0+Math.random()*96);
      });
      raf.current=requestAnimationFrame(draw);
    };draw();return()=>cancelAnimationFrame(raf.current);
  },[w,h]);
  return <canvas ref={ref} width={w} height={h} style={{position:"absolute",inset:0,zIndex:1,opacity:.6}}/>;
}

// Galaxy map with square Borg markers
function GalaxyMap({systems,links,selected,onSelect,w,h}){
  const ref=useRef(null),camRef=useRef({x:0,y:0,z:1}),[cam,setCam]=useState({x:0,y:0,z:1}),drag=useRef({on:false,sx:0,sy:0,cx:0,cy:0}),[hov,setHov]=useState(null);
  useEffect(()=>{camRef.current=cam;},[cam]);
  const toScr=useCallback((sx,sy)=>{const c=camRef.current;return{x:(sx-.5)*w*c.z+w/2-c.x*c.z,y:(sy-.5)*h*c.z+h/2-c.y*c.z};},[w,h]);
  useEffect(()=>{const ctx=ref.current?.getContext("2d");if(!ctx)return;let t=0,af;const render=()=>{const c=camRef.current;t+=.016;ctx.clearRect(0,0,w,h);
    const s2=(sx,sy)=>({x:(sx-.5)*w*c.z+w/2-c.x*c.z,y:(sy-.5)*h*c.z+h/2-c.y*c.z});
    ctx.strokeStyle=B.grid+"55";ctx.lineWidth=.3;
    for(let x=0;x<w;x+=30){ctx.beginPath();ctx.moveTo(x,0);ctx.lineTo(x,h);ctx.stroke();}
    for(let y=0;y<h;y+=30){ctx.beginPath();ctx.moveTo(0,y);ctx.lineTo(w,y);ctx.stroke();}
    links.forEach(([a,b])=>{const sA=systems.find(s=>s.id===a),sB=systems.find(s=>s.id===b);if(!sA||!sB)return;const pA=s2(sA.x,sA.y),pB=s2(sB.x,sB.y);const bo=sA.fac==="borg"&&sB.fac==="borg";ctx.beginPath();ctx.moveTo(pA.x,pA.y);ctx.lineTo(pB.x,pB.y);ctx.strokeStyle=bo?B.green+"33":"#fff08";ctx.lineWidth=bo?1.5:.5;ctx.stroke();
    if(bo){const pulse=Math.sin(t*3+pA.x*.01)*.5+.5;ctx.strokeStyle=`rgba(0,255,65,${pulse*.08})`;ctx.lineWidth=5;ctx.stroke();}});
    systems.filter(s=>s.type==="hw").forEach(s=>{const p=s2(s.x,s.y),r=100*c.z;const g=ctx.createRadialGradient(p.x,p.y,0,p.x,p.y,r);g.addColorStop(0,"rgba(0,255,65,.05)");g.addColorStop(1,"transparent");ctx.fillStyle=g;ctx.fillRect(p.x-r,p.y-r,r*2,r*2);});
    systems.forEach(s=>{const p=s2(s.x,s.y);if(p.x<-30||p.x>w+30||p.y<-30||p.y>h+30)return;const isSel=selected?.id===s.id,isHov=hov?.id===s.id,bo=s.fac==="borg",hw=s.type==="hw";const baseR=(hw?7:bo?4:2.5)*c.z,r=Math.max(2,baseR);
    if(isSel){const pulse=.5+.5*Math.sin(t*3);ctx.strokeStyle=`rgba(0,255,65,${.4+pulse*.4})`;ctx.lineWidth=1.5;ctx.strokeRect(p.x-r-8,p.y-r-8,r*2+16,r*2+16);}
    if(isHov&&!isSel){ctx.strokeStyle=B.green+"44";ctx.lineWidth=1;ctx.strokeRect(p.x-r-5,p.y-r-5,r*2+10,r*2+10);}
    if(bo){ctx.fillStyle=hw?B.green:B.greenDim;ctx.fillRect(p.x-r,p.y-r,r*2,r*2);ctx.strokeStyle=B.green+"88";ctx.lineWidth=.5;ctx.strokeRect(p.x-r,p.y-r,r*2,r*2);if(hw){const scan=(t*20)%(r*2);ctx.fillStyle=B.green+"44";ctx.fillRect(p.x-r,p.y-r+scan,r*2,2);}}
    else{ctx.beginPath();ctx.arc(p.x,p.y,r,0,6.28);ctx.fillStyle=s.type?"#556":"#334";ctx.fill();}
    if(c.z>.5||hw||isSel||isHov){ctx.font=`${Math.max(8,10*c.z)}px "Share Tech Mono",monospace`;ctx.textAlign="center";ctx.fillStyle=isSel?B.green:isHov?B.text:bo?B.textDim:"#445";ctx.fillText(s.name.toUpperCase?s.name.toUpperCase():s.name,p.x,p.y+r+11*c.z);}});
    af=requestAnimationFrame(render);};render();return()=>cancelAnimationFrame(af);},[w,h,systems,links,selected,hov]);
  return <canvas ref={ref} width={w} height={h} onWheel={e=>{e.preventDefault();setCam(c=>({...c,z:Math.max(.3,Math.min(3.5,c.z*(e.deltaY<0?1.1:.91)))}));}} onPointerDown={e=>{drag.current={on:true,sx:e.clientX,sy:e.clientY,cx:cam.x,cy:cam.y};ref.current?.setPointerCapture(e.pointerId);}} onPointerMove={e=>{const rect=ref.current.getBoundingClientRect(),mx=e.clientX-rect.left,my=e.clientY-rect.top;let found=null;for(const s of systems){const p=toScr(s.x,s.y);if(Math.hypot(mx-p.x,my-p.y)<Math.max(12,6*cam.z)){found=s;break;}}setHov(found);ref.current.style.cursor=found?"pointer":drag.current.on?"grabbing":"default";if(!drag.current.on)return;const d=drag.current;setCam(c=>({...c,x:d.cx-(e.clientX-d.sx)/c.z,y:d.cy-(e.clientY-d.sy)/c.z}));}} onPointerUp={e=>{const d=drag.current;if(Math.hypot(e.clientX-d.sx,e.clientY-d.sy)<5&&hov)onSelect(hov);d.on=false;}} style={{width:"100%",height:"100%",touchAction:"none"}}/>;
}

// Floating Borg node panel with connection dots
function BorgNode({title,designation,x,y,w:nw,active,onClick,children,zIndex=12}){
  const [open,setOpen]=useState(true);
  return(
    <div style={{position:"absolute",left:x,top:y,width:open?nw:36,zIndex,pointerEvents:"auto",transition:"width .15s"}}>
      <div style={{position:"absolute",top:-3,left:-3,width:7,height:7,background:active?B.green:B.greenDark,boxShadow:active?`0 0 6px ${B.green}`:"none"}}/>
      <div style={{position:"absolute",top:"50%",right:-3,width:5,height:5,background:B.greenDark,marginTop:-2}}/>
      {open?(
        <div style={{border:`1px solid ${active?B.green+"88":B.green+"33"}`,background:B.panel+"EE",boxShadow:active?`0 0 15px ${B.green}11, inset 0 0 30px ${B.greenDark}44`:`inset 0 0 20px ${B.greenDark}22`,overflow:"hidden"}}>
          <div style={{height:1,background:`linear-gradient(90deg,transparent,${active?B.green+"44":B.green+"22"},transparent)`}}/>
          <div style={{height:20,display:"flex",alignItems:"center",padding:"0 6px",background:B.bgAlt,borderBottom:`1px solid ${B.green}22`,cursor:"pointer"}} onClick={()=>{onClick?.();setOpen(false);}}>
            <span style={{fontSize:6,color:B.textDim,fontFamily:BFONT,marginRight:4,opacity:.6}}>{designation}</span>
            <span style={{fontSize:9,color:active?B.green:B.textDim,fontFamily:BFONT,letterSpacing:2,flex:1}}>{title}</span>
            <span style={{fontSize:6,color:B.greenDark,fontFamily:BFONT}}{"▪▪▪"}</span>
          </div>
          <div style={{padding:"4px 6px"}}>{children}</div>
          <div style={{height:1,background:`linear-gradient(90deg,${B.green}11,${B.green}22,${B.green}11)`}}/>
        </div>
      ):(
        <div onClick={()=>setOpen(true)} style={{width:36,height:36,cursor:"pointer",border:`1px solid ${B.green}44`,background:B.panel,display:"flex",alignItems:"center",justifyContent:"center"}}>
          <span style={{fontSize:7,color:B.green,fontFamily:BFONT}}>{designation}</span>
        </div>
      )}
    </div>
  );
}

function BStat({label,value,bar,alert}){
  return(<div style={{display:"flex",alignItems:"center",gap:4,height:15}}>
    <span style={{fontSize:7,color:B.textDim,fontFamily:BFONT,width:62,flexShrink:0}}>{label}</span>
    {bar!=null&&( <div style={{flex:1,height:2,background:B.greenDark}}><div style={{height:"100%",width:`${bar}%`,background:alert?B.red:B.green}}/></div>)}
    <span style={{fontSize:9,color:alert?B.red:B.green,fontFamily:BFONT,textAlign:"right",minWidth:36}}>{value}</span>
  </div>);
}
function BDivider(){return <div style={{height:1,background:B.green+"18",margin:"2px 0"}}/>;}
function BButton({label,alert,onClick}){
  return(<button onClick={onClick} style={{flex:1,border:`1px solid ${alert?B.red:B.green}22`,padding:"3px 0",fontSize:6,letterSpacing:2,background:alert?B.redDim+"22":B.greenDark+"22",color:alert?B.red:B.green,cursor:"pointer",fontFamily:BFONT}}>{label}</button>);
}

// ═══════════════════════════════════════════════════════
//  MAIN — BORG COLLECTIVE (no frame, network of nodes)
// ═══════════════════════════════════════════════════════
export default function BorgUI(){
  const [sel,setSel]=useState(null);
  const [activeNode,setActiveNode]=useState("map");
  const rootRef=useRef(null);
  const [dims,setDims]=useState({w:1200,h:800});
  const [tick,setTick]=useState(0);

  useEffect(()=>{
    const link=document.createElement("link");
    link.href="https://fonts.googleapis.com/css2?family=Share+Tech+Mono&display=swap";
    link.rel="stylesheet";document.head.appendChild(link);
    return()=>document.head.removeChild(link);
  },[]);
  useEffect(()=>{
    const m=()=>{if(rootRef.current)setDims({w:rootRef.current.offsetWidth,h:rootRef.current.offsetHeight});};
    m();window.addEventListener("resize",m);return()=>window.removeEventListener("resize",m);
  },[]);
  useEffect(()=>{const iv=setInterval(()=>setTick(t=>t+1),1000);return()=>clearInterval(iv);},[]);

  const W=dims.w,H=dims.h;

  const RES=[
    {icon:"\u2B21",label:"ENRG",val:"99.841"},
    {icon:"\u25C8",label:"NANO",val:"48.204"},
    {icon:"\u25A4",label:"DRHN",val:"126.7K"},
    {icon:"\u25FB",label:"MATR",val:"71.883"},
    {icon:"\u229E",label:"SPZS",val:"8.472"},
  ];
  const NAV=[
    {key:"map",label:"SEKTOR"},{key:"worlds",label:"WELTEN"},
    {key:"cubes",label:"KUBEN"},{key:"adapt",label:"ADAPTION"},
    {key:"assim",label:"ASSIMILATION"},{key:"sense",label:"SENSORIK"},
    {key:"drones",label:"DROHNEN"},{key:"net",label:"NETZWERK"},
  ];

  return(
    <div ref={rootRef} style={{width:"100vw",height:"100vh",background:B.bg,overflow:"hidden",position:"relative",userSelect:"none"}}>
      <DataRain w={W} h={H}/>

      {/* SVG network connections between nodes */}
      <svg style={{position:"absolute",inset:0,zIndex:3,pointerEvents:"none"}} width={W} height={H}>
        {[
          [208,30,W-275,28],[100,56,100,H-104],[W-275,50,W-218,60],
          [W-113,200,W-113,262],[W-113,410,W-113,440],[288,H-80,4+1,H-42],
        ].map(([x1,y1,x2,y2],i)=>(
          <g key={`cn${i}`}>
            <line x1={x1} y1={y1} x2={x2} y2={y2} stroke={B.green+"15"} strokeWidth={1} strokeDasharray="4,8"/>
            <circle r={2} fill={B.green+"44"}>
              <animateMotion dur={`${3+i*.7}s`} repeatCount="indefinite" path={`M${x1},${y1} L${x2},${y2}`}/>
            </circle>
          </g>
        ))}
        {/* Grid coords */}
        {Array.from({length:Math.floor(W/80)},(_,i)=>(
          <text key={`gx${i}`} x={i*80+40} y={H-2} textAnchor="middle" fill={B.green+"15"} fontSize={6} fontFamily="Share Tech Mono">{String(i).padStart(3,"0")}</text>
        ))}
        {Array.from({length:Math.floor(H/60)},(_,i)=>(
          <text key={`gy${i}`} x={3} y={i*60+30} fill={B.green+"15"} fontSize={6} fontFamily="Share Tech Mono">{String(i).padStart(2,"0")}</text>
        ))}
        <text x={W/2} y={H/2+60} textAnchor="middle" fill={B.green+"08"} fontSize={11} fontFamily="Share Tech Mono" letterSpacing={8}>WIDERSTAND IST ZWECKLOS</text>
        {/* Crosshair */}
        {(()=>{const cx=(W-218)/2+4,cy=H/2;return(<g opacity=".12"><line x1={cx-20} y1={cy} x2={cx+20} y2={cy} stroke={B.green} strokeWidth=".5"/><line x1={cx} y1={cy-20} x2={cx} y2={cy+20} stroke={B.green} strokeWidth=".5"/><rect x={cx-8} y={cy-8} width={16} height={16} fill="none" stroke={B.green} strokeWidth=".5"/></g>);})()}
      </svg>

      {/* Galaxy map — takes most space */}
      <div style={{position:"absolute",top:54,left:4,right:222,bottom:54,overflow:"hidden",zIndex:5}}>
        <GalaxyMap systems={SYSTEMS} links={LINKS} selected={sel} onSelect={setSel} w={Math.max(100,W-230)} h={Math.max(100,H-112)}/>
      </div>

      {/* TOP-LEFT: Collective status */}
      <BorgNode title="KOLLEKTIV" designation="001" x={8} y={6} w={200} active={true} zIndex={12}>
        <div style={{display:"flex",alignItems:"center",gap:8}}>
          <svg width={28} height={28} viewBox="0 0 40 40" style={{flexShrink:0}}>
            <polygon points="12,6 28,6 34,12 18,12" fill={B.greenDark} stroke={B.green+"66"} strokeWidth=".5"/>
            <polygon points="6,12 6,28 12,34 12,6" fill={B.greenDark+"CC"} stroke={B.green+"66"} strokeWidth=".5"/>
            <rect x="6" y="12" width="22" height="22" fill={B.panel} stroke={B.green} strokeWidth=".8"/>
            {[0,1,2].map(i=><g key={i}><line x1={6+5.5*(i+1)} y1={12} x2={6+5.5*(i+1)} y2={34} stroke={B.green+"33"} strokeWidth=".3"/><line x1={6} y1={12+5.5*(i+1)} x2={28} y2={12+5.5*(i+1)} stroke={B.green+"33"} strokeWidth=".3"/></g>)}
            <circle cx="17" cy="23" r="2" fill={B.green} opacity=".6"/>
            <polygon points="28,12 34,6 34,28 28,34" fill={B.greenDark+"88"} stroke={B.green+"66"} strokeWidth=".5"/>
          </svg>
          <div>
            <div style={{fontSize:11,color:B.green,fontFamily:BFONT}}>BORG KOLLEKTIV</div>
            </div>
          </div>
        </BorgNode>

        <BorgNode title="ASSIM.-LOG" designation="031" x={W-218} y={440} w={210} active={activeNode==="log"} onClick={()=>setActiveNode("log")} zIndex={14}>
          <div style={{display:"flex",flexDirection:"column",gap:2}}>
            {[{msg:"Spezies 6339 — Assimilation 94%",col:B.green},{msg:"Spezies 8472 — Resistenz erkannt",col:B.red},{msg:"Sektor 010 — Scan komplett",col:B.textDim}].map((r,i)=>(
              <div key={i} style={{padding:"2px 4px",borderLeft:`2px solid ${r.col}44`,background:B.bgAlt}}>
                <span style={{fontSize:7,color:B.text,fontFamily:BFONT,lineHeight:1.3,display:"block"}}>{r.msg}</span>
              </div>
            ))}
          </div>
        </BorgNode>

        {/* BOTTOM-LEFT: Collective voice */}
        <BorgNode title="STIMME DES KOLLEKTIVS" designation="000" x={8} y={H-100} w={280} active={tick%4<2} zIndex={12}>
          <div style={{fontSize:9,color:B.green,fontFamily:BFONT,lineHeight:1.5,opacity:.8}}>
            {tick%8<4?"WIR SIND DIE BORG. SIE WERDEN ASSIMILIERT WERDEN.":"IHRE BIOLOGISCHE UND TECHNOLOGISCHE EIGENART WIRD ADAPTIERT."}
            <span style={{opacity:tick%2===0?1:0,transition:"opacity .3s"}}>_</span>
          </div>
        </BorgNode>

        {/* BOTTOM: Nav */}
        <div style={{position:"absolute",bottom:4,left:4,right:4,height:36,zIndex:12,display:"flex",gap:1,pointerEvents:"auto"}}>
          {NAV.map(item=>{
            const act=activeNode===item.key;
            return(<div key={item.key} onClick={()=>setActiveNode(item.key)} style={{
              flex:1,display:"flex",alignItems:"center",justifyContent:"center",cursor:"pointer",
              background:act?B.greenDark+"55":B.panel+"CC",
              border:`1px solid ${act?B.green+"66":B.green+"22"}`,
              borderTop:act?`2px solid ${B.green}`:`2px solid transparent`,
              transition:"all .1s",
            }}
            onMouseEnter={e=>{if(!act)e.currentTarget.style.background=B.greenDark+"33";}}
            onMouseLeave={e=>{if(!act)e.currentTarget.style.background=B.panel+"CC";}}
            ><span style={{fontSize:7,letterSpacing:2,color:act?B.green:B.textDim,fontFamily:BFONT}}>{item.label}</span></div>);
          })}
        </div>

        {/* Full-screen scan line */}
        <div style={{position:"absolute",left:0,right:0,top:`${(tick*2.3)%100}%`,height:1,background:`linear-gradient(90deg,transparent,${B.green}08,transparent)`,zIndex:20,pointerEvents:"none"}}/>
      </div>
    );
  }

  // ─── Mount ───────────────────────────────────────────────────────────────────

  const borgContainer = document.getElementById("borg-root");
  if (borgContainer) createRoot(borgContainer).render(<BorgUI />);
