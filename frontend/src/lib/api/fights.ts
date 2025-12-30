import apiFetch from './client';

export const fights = {
  getActiveSession: async (progressId: string, bossId: string) => {
    return apiFetch<any>(`/fights/active`, {
      queryParams: { progressId, bossId }
    });
  },
  
  startSession: async (progressId: string, bossId: string) => {
    return apiFetch<any>('/fights/session', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ progressId, bossId })
    });
  },
  
  recordAttempt: async (attempt: {
    progressId: string;
    bossId: string;
    weaponId: string;
    notes: string;
    victory: boolean;
  }) => {
    return apiFetch<any>('/fights/attempt', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(attempt)
    });
  },
  
  endSession: async (sessionId: string, victory: boolean) => {
    return apiFetch<any>(`/fights/session/${sessionId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ victory })
    });
  }
};