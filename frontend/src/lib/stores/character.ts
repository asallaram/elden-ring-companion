import { writable } from 'svelte/store';

export interface Character {
  id: string;
  playerName: string;
  currentLevel: number;
  currentRunes: number;
  defeatedBossIds: string[];
  obtainedWeaponIds: string[];
  playtimeHours: number;
  totalDeaths: number;
  visitedAreas: string[];
  recentBossAttempts: string[];
  createdAt: string;
}

const storedCharacter = typeof window !== 'undefined' 
  ? localStorage.getItem('selectedCharacter')
  : null;

const initialCharacter: Character | null = storedCharacter 
  ? JSON.parse(storedCharacter) 
  : null;

function createCharacterStore() {
  const { subscribe, set, update } = writable<Character | null>(initialCharacter);

  return {
    subscribe,
    
    select: (character: Character) => {
      if (typeof window !== 'undefined') {
        localStorage.setItem('selectedCharacter', JSON.stringify(character));
      }
      set(character);
    },
    
    clear: () => {
      if (typeof window !== 'undefined') {
        localStorage.removeItem('selectedCharacter');
      }
      set(null);
    },
    
    updateData: (updatedCharacter: Character) => {
      if (typeof window !== 'undefined') {
        localStorage.setItem('selectedCharacter', JSON.stringify(updatedCharacter));
      }
      set(updatedCharacter);
    }
  };
}

export const characterStore = createCharacterStore();