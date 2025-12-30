export interface Boss {
  id: string;
  name: string;
  image?: string;
  description?: string;
}

export interface ActiveFightSession {
  id: string;
  active: boolean;
  lastWeaponId?: string;
  VictoryWeaponId?: string;
}

export interface RecordAttemptInput {
  progressId: string;
  bossId: string;
  weaponId: string;
  notes: string;
  victory: boolean;
}

export interface BossFightAttempt {
  id: string;
  BossFightSessionId: string;
  PlayerProgressId: string;
  BossId: string;
  WeaponUsedId: string;
  WeaponUsedName: string;
  AttemptNumber: number;
  TimeSpentSeconds: number;
  Notes?: string;
  Victory: boolean;
}
