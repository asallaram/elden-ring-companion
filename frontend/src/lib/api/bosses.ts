import apiFetch from './client';

export const bosses = {
  getAll: async () => {
    return apiFetch<any[]>('/bosses');
  },
  
  getById: async (id: string) => {
    return apiFetch<any>(`/bosses/${id}`);
  },  // ADD COMMA HERE
  
  getStats: async (id: string) => {
    return apiFetch<any>(`/bosses/${id}/stats`);  // CHANGE apiCall to apiFetch
  }
};