import { writable } from 'svelte/store';
import type { ActiveFightSession } from '$lib/types/fight';

export const session = writable<ActiveFightSession | null>(null);
