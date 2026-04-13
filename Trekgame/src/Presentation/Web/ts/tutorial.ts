// ============================================================================
// TutorialHighlight — JS Interop for the Blazor Tutorial/Help system
// Provides element targeting, spotlight positioning, dialog placement,
// MutationObserver-based element watching, and resize tracking.
// ============================================================================

// Types: DotNetObjectReference, TutorialElementBounds, TutorialDialogPosition
// are declared globally in types/blazor.d.ts

// ---------------------------------------------------------------------------
// State
// ---------------------------------------------------------------------------

let observer: MutationObserver | null = null;
let resizeHandler: (() => void) | null = null;
let resizeDotNetRef: DotNetObjectReference | null = null;
let debounceTimer: ReturnType<typeof setTimeout> | null = null;

// ---------------------------------------------------------------------------
// Element Bounds
// ---------------------------------------------------------------------------

function getElementBounds(selector: string): TutorialElementBounds | null {
  const el = document.querySelector(selector);
  if (!el) return null;

  const rect = el.getBoundingClientRect();
  return {
    x: rect.x,
    y: rect.y,
    width: rect.width,
    height: rect.height,
  };
}

// ---------------------------------------------------------------------------
// Scroll Into View
// ---------------------------------------------------------------------------

function scrollToElement(selector: string): void {
  const el = document.querySelector(selector);
  if (el) {
    el.scrollIntoView({ behavior: 'smooth', block: 'center' });
  }
}

// ---------------------------------------------------------------------------
// MutationObserver — watch for element to appear in DOM
// ---------------------------------------------------------------------------

function observeElement(selector: string, dotNetRef: DotNetObjectReference): void {
  // Clean up any existing observer
  stopObserving();

  // Check if element already exists
  if (document.querySelector(selector)) {
    void dotNetRef.invokeMethodAsync('OnElementReady');
    return;
  }

  observer = new MutationObserver((_mutations) => {
    if (document.querySelector(selector)) {
      void dotNetRef.invokeMethodAsync('OnElementReady');
      stopObserving();
    }
  });

  observer.observe(document.body, {
    childList: true,
    subtree: true,
  });
}

function stopObserving(): void {
  if (observer) {
    observer.disconnect();
    observer = null;
  }
}

// ---------------------------------------------------------------------------
// Dialog Positioning
// ---------------------------------------------------------------------------

const GAP = 20;

function getDialogPosition(
  targetBounds: TutorialElementBounds,
  dialogWidth: number,
  dialogHeight: number,
  preferredPosition: string,
): TutorialDialogPosition {
  const vw = window.innerWidth;
  const vh = window.innerHeight;

  const { x: tx, y: ty, width: tw, height: th } = targetBounds;

  // Calculate candidate positions
  const positions: Record<string, TutorialDialogPosition> = {
    'top':          { x: tx + tw / 2 - dialogWidth / 2,  y: ty - dialogHeight - GAP },
    'bottom':       { x: tx + tw / 2 - dialogWidth / 2,  y: ty + th + GAP },
    'left':         { x: tx - dialogWidth - GAP,          y: ty + th / 2 - dialogHeight / 2 },
    'right':        { x: tx + tw + GAP,                   y: ty + th / 2 - dialogHeight / 2 },
    'top-left':     { x: tx - dialogWidth - GAP,          y: ty - dialogHeight - GAP },
    'top-right':    { x: tx + tw + GAP,                   y: ty - dialogHeight - GAP },
    'bottom-left':  { x: tx - dialogWidth - GAP,          y: ty + th + GAP },
    'bottom-right': { x: tx + tw + GAP,                   y: ty + th + GAP },
    'center':       { x: vw / 2 - dialogWidth / 2,        y: vh / 2 - dialogHeight / 2 },
  };

  // Try preferred position first
  let pos = positions[preferredPosition] ?? positions['bottom'];

  // Auto-flip if dialog overflows viewport
  if (!fitsInViewport(pos, dialogWidth, dialogHeight, vw, vh)) {
    // Try the opposite position
    const flipMap: Record<string, string> = {
      'top': 'bottom',
      'bottom': 'top',
      'left': 'right',
      'right': 'left',
      'top-left': 'bottom-right',
      'top-right': 'bottom-left',
      'bottom-left': 'top-right',
      'bottom-right': 'top-left',
    };

    const flipped = flipMap[preferredPosition];
    if (flipped && positions[flipped]) {
      const flippedPos = positions[flipped];
      if (fitsInViewport(flippedPos, dialogWidth, dialogHeight, vw, vh)) {
        pos = flippedPos;
      }
    }

    // If still doesn't fit, try all positions to find one that works
    if (!fitsInViewport(pos, dialogWidth, dialogHeight, vw, vh)) {
      for (const key of ['bottom', 'top', 'right', 'left', 'bottom-right', 'bottom-left', 'top-right', 'top-left', 'center']) {
        const candidate = positions[key];
        if (fitsInViewport(candidate, dialogWidth, dialogHeight, vw, vh)) {
          pos = candidate;
          break;
        }
      }
    }
  }

  // Final clamp to viewport
  pos = {
    x: Math.max(0, Math.min(pos.x, vw - dialogWidth)),
    y: Math.max(0, Math.min(pos.y, vh - dialogHeight)),
  };

  return pos;
}

function fitsInViewport(
  pos: TutorialDialogPosition,
  width: number,
  height: number,
  vw: number,
  vh: number,
): boolean {
  return pos.x >= 0 && pos.y >= 0 && pos.x + width <= vw && pos.y + height <= vh;
}

// ---------------------------------------------------------------------------
// Resize Listener (debounced)
// ---------------------------------------------------------------------------

function onResize(dotNetRef: DotNetObjectReference): void {
  offResize();

  resizeDotNetRef = dotNetRef;
  resizeHandler = () => {
    if (debounceTimer !== null) clearTimeout(debounceTimer);
    debounceTimer = setTimeout(() => {
      resizeDotNetRef?.invokeMethodAsync('OnWindowResized');
    }, 200);
  };

  window.addEventListener('resize', resizeHandler);
}

function offResize(): void {
  if (resizeHandler) {
    window.removeEventListener('resize', resizeHandler);
    resizeHandler = null;
  }
  if (debounceTimer !== null) {
    clearTimeout(debounceTimer);
    debounceTimer = null;
  }
  resizeDotNetRef = null;
}

// ---------------------------------------------------------------------------
// Expose on window for Blazor JS Interop
// ---------------------------------------------------------------------------

window.TutorialHighlight = {
  getElementBounds,
  scrollToElement,
  observeElement,
  stopObserving,
  getDialogPosition,
  onResize,
  offResize,
};
