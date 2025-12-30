import type { Character, Boss, Weapon } from '$lib/types/player';

export function bossProgress(character: Character, bosses: Boss[]) {
  const defeated = new Set(character.defeatedBossIds);

  return {
    total: bosses.length,
    defeated: bosses.filter(b => defeated.has(b.id)),
    remaining: bosses.filter(b => !defeated.has(b.id)),
    percent:
      bosses.length === 0
        ? 0
        : Math.round((defeated.size / bosses.length) * 100),
  };
}

export function weaponProgress(character: Character, weapons: Weapon[]) {
  const owned = new Set(character.obtainedWeaponIds);

  return {
    total: weapons.length,
    owned: weapons.filter(w => owned.has(w.id)),
    missing: weapons.filter(w => !owned.has(w.id)),
    percent:
      weapons.length === 0
        ? 0
        : Math.round((owned.size / weapons.length) * 100),
  };
}
