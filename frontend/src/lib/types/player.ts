// src/lib/types/player.ts

export interface Character {
  id: string;                 // unique character ID
  name: string;
  class: string;
  level: number;
  runes: number;

  // progress tracking
  defeatedBossIds: string[];
  obtainedWeaponIds: string[];
  obtainedAshesIds?: string[];
  visitedLocationIds: string[];
  discoveredGraceIds: string[];
}

export interface Boss {
  id: string;
  name: string;
  locationId: string;
}

export interface Weapon {
  id: string;
  name: string;
  type: string;
}

export interface Ash {
  id: string;
  name: string;
}

export interface Location {
  id: string;
  name: string;
}

export interface Grace {
  id: string;
  name: string;
}
