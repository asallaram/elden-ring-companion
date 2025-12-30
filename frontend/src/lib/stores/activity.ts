// src/lib/stores/activity.ts
import { writable } from 'svelte/store';

export interface Activity {
  id: string;
  type: 'boss' | 'weapon' | 'level' | 'map' | 'session';
  text: string;
  timestamp: string;
  icon: string;
}

export const runningLog = writable<Activity[]>([]);

export function addActivity(activity: Omit<Activity, 'id' | 'timestamp'>) {
  runningLog.update(log => {
    const newEntry: Activity = {
      id: crypto.randomUUID(),
      timestamp: new Date().toLocaleTimeString(),
      ...activity
    };
    const updatedLog = [newEntry, ...log];
    return updatedLog.slice(0, 50); 
  });
}
