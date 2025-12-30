import apiFetch from './client';

export const weapons = {
  getAll: async () => {
    return apiFetch<any[]>('/weapons');
  },
  
  getById: async (id: string) => {
    return apiFetch<any>(`/weapons/${id}`);
  },
  
  getMatchups: async (id: string) => {  // ADD THIS
    return apiFetch<any[]>(`/weapons/${id}/matchups`);
  }
};