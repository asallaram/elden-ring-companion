import { writable, get } from 'svelte/store';
import type { Character } from '$lib/types/player';
import { api } from '$lib/api';

export interface CharacterStoreValue {
  character: Character | null;
  isLoading: boolean;
}

function createCharacterStore() {
  const store = writable<CharacterStoreValue>({
    character: null,
    isLoading: false
  });

  const { subscribe, set, update } = store;

  async function performAction(
    action: (characterId: string) => Promise<void>
  ) {
    const current = get(store);

    if (!current.character) {
      throw new Error('No character loaded');
    }

    update(s => ({ ...s, isLoading: true }));

    try {
      await action(current.character.id);

      const chars = await api.playerProgress.getMyCharacters();
      const updated =
        chars.find((c: { id: string }) => c.id === current.character!.id) ??
        null;

      set({ character: updated, isLoading: false });
    } catch (err) {
      update(s => ({ ...s, isLoading: false }));
      throw err;
    }
  }

  return {
    subscribe,

    async loadCharacter(characterId: string) {
      update(s => ({ ...s, isLoading: true }));

      try {
        const chars = await api.playerProgress.getMyCharacters();
        const found =
          chars.find((c: { id: string }) => c.id === characterId) ?? null;

        set({ character: found, isLoading: false });
      } catch (err) {
        console.error('Failed to load character', err);
        set({ character: null, isLoading: false });
      }
    },

    clearCharacter() {
      set({ character: null, isLoading: false });
    },

    defeatBoss(bossId: string) {
      return performAction(id =>
        api.playerProgress.defeatBoss(id, bossId)
      );
    },

    obtainWeapon(weaponId: string) {
      return performAction(id =>
        api.playerProgress.obtainWeapon(id, weaponId)
      );
    }
  };
}

export const characterStore = createCharacterStore();
