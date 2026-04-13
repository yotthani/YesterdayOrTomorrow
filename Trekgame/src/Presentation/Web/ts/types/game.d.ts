export interface StarSystem {
  id: string;
  name: string;
  x: number;
  y: number;
  starType: string;
  ownerId?: string;
  ownerColor?: string;
  ownerName?: string;
  hasColony: boolean;
  hasStarbase: boolean;
  fleetCount: number;
  population?: number;
  isSelected?: boolean;
}

export interface Hyperlane {
  fromSystemId: string;
  toSystemId: string;
  isActive?: boolean;
}

export interface Fleet {
  id: string;
  systemId: string;
  ownerId?: string;
  ownerColor?: string;
  shipCount: number;
  isMoving?: boolean;
}

export interface AsteroidField {
  x: number;
  y: number;
  radius: number;
  density?: number;
}

export interface StarGridPosition {
  row: number;
  col: number;
}

export type StarTypeName =
  | 'yellow' | 'orange' | 'red' | 'blue' | 'white'
  | 'redgiant' | 'bluesupergiant' | 'orangegiant'
  | 'neutron' | 'blackhole' | 'whitedwarf' | 'browndwarf'
  | 'binary' | 'trinary' | 'protostar' | 'supernova';

export interface GalaxyAssets {
  stars: Record<string, HTMLCanvasElement | HTMLImageElement>;
  nebulae: HTMLCanvasElement[];
  icons: Record<string, HTMLCanvasElement>;
}

export interface Velocity {
  x: number;
  y: number;
}

export type OnSystemCallback = (system: StarSystem | null) => void;
