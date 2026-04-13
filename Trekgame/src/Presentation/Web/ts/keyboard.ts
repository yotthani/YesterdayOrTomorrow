const shortcuts: Record<string, string> = {
  Space:   'endTurn',
  KeyG:    'navigateGalaxy',
  KeyR:    'navigateResearch',
  KeyD:    'navigateDiplomacy',
  KeyF:    'navigateFleets',
  KeyC:    'navigateColonies',
  Escape:  'closeModal',
  F1:      'showHelp',
  KeyS:    'quickSave',
  KeyL:    'quickLoad',
  Digit1:  'selectFleet1',
  Digit2:  'selectFleet2',
  Digit3:  'selectFleet3',
};

let blazorComponent: DotNetObjectReference | null = null;
let enabled = true;

function init(componentRef: DotNetObjectReference): void {
  blazorComponent = componentRef;
  document.addEventListener('keydown', handleKeyDown);
}

function handleKeyDown(e: KeyboardEvent): void {
  if (!enabled) return;

  const target = e.target as HTMLElement;
  if (
    target.tagName === 'INPUT' ||
    target.tagName === 'TEXTAREA' ||
    target.tagName === 'SELECT'
  ) {
    return;
  }

  const action = shortcuts[e.code];
  if (action && blazorComponent) {
    e.preventDefault();
    blazorComponent.invokeMethodAsync('HandleShortcut', action);
  }
}

window.GameKeyboard = {
  init,
  setEnabled(value: boolean): void {
    enabled = value;
  },
  addShortcut(key: string, action: string): void {
    shortcuts[key] = action;
  },
};

console.log('⌨️ Keyboard shortcuts loaded');
