declare interface DotNetObjectReference {
  invokeMethodAsync<T = void>(methodName: string, ...args: unknown[]): Promise<T>;
  dispose(): void;
}

declare interface TooltipStat {
  icon: string;
  name: string;
  value: string;
  positive?: boolean;
  negative?: boolean;
}

declare interface TooltipEntry {
  title: string;
  content: string;
  hint?: string;
  stats?: TooltipStat[];
}

declare interface GameKeyboardApi {
  init(componentRef: DotNetObjectReference): void;
  setEnabled(value: boolean): void;
  addShortcut(key: string, action: string): void;
}

declare interface GameSoundsApi {
  play(soundType: string, volume?: number): void;
  setEnabled(value: boolean): void;
  setVolume(value: number): void;
  isEnabled(): boolean;
}

declare interface GameTooltipsApi {
  init(componentRef: DotNetObjectReference): void;
  addTooltipData(key: string, data: TooltipEntry): void;
}

declare interface Window {
  GameKeyboard: GameKeyboardApi;
  GameSounds: GameSoundsApi;
  GameTooltips: GameTooltipsApi;
  GalaxyRenderer: typeof import('../GalaxyRenderer').GalaxyRenderer;

  galaxyRenderer: import('../GalaxyRenderer').GalaxyRenderer | null;
  initGalaxyMap(containerId: string): Promise<boolean>;
  setGalaxySystems(systemsJson: string): void;
  setGalaxyHyperlanes(hyperlanesJson: string): void;
  setGalaxyFleets(fleetsJson: string): void;
  setGalaxyAsteroidFields(fieldsJson: string): void;
  setGalaxyStations(stationsJson: string): void;
  setGalaxyCallbacks(dotnetRef: DotNetObjectReference): void;
  centerGalaxyOnSystem(systemId: string): void;
  setGalaxyZoom(level: number): void;
  resetGalaxyView(): void;
  destroyGalaxyMap(): void;

  TutorialHighlight: TutorialHighlightApi;

  webkitAudioContext?: typeof AudioContext;
  AudioContext: typeof AudioContext;
}

declare interface TutorialElementBounds {
  x: number;
  y: number;
  width: number;
  height: number;
}

declare interface TutorialDialogPosition {
  x: number;
  y: number;
}

declare interface TutorialHighlightApi {
  getElementBounds(selector: string): TutorialElementBounds | null;
  scrollToElement(selector: string): void;
  observeElement(selector: string, dotNetRef: DotNetObjectReference): void;
  stopObserving(): void;
  getDialogPosition(
    targetBounds: TutorialElementBounds,
    dialogWidth: number,
    dialogHeight: number,
    preferredPosition: string,
  ): TutorialDialogPosition;
  onResize(dotNetRef: DotNetObjectReference): void;
  offResize(): void;
}
