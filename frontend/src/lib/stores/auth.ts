import { writable } from 'svelte/store';
import { api, authToken } from '$lib/api';


export interface User {
  userId: string;
  username: string;
  email: string;
  psnId?: string;
  playstyle?: string;
}


interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
}


function createAuthStore() {
  const { subscribe, set, update } = writable<AuthState>({
    user: null,
    isAuthenticated: false,
    isLoading: true,
  });

  return {
    subscribe,

    /* ---------- Init ---------- */
    async init() {
      const token = authToken.get();
      if (!token) {
        set({ user: null, isAuthenticated: false, isLoading: false });
        return;
      }

      try {
        const profile = (await api.auth.getProfile()) as User;

        set({
          user: profile,
          isAuthenticated: true,
          isLoading: false,
        });
      } catch (error) {
        console.error('Auth init failed:', error);
        authToken.remove();
        set({ user: null, isAuthenticated: false, isLoading: false });
      }
    },

    async register(
      username: string,
      email: string,
      password: string,
      psnId?: string,
      playstyle?: string
    ) {
      update(s => ({ ...s, isLoading: true }));

      try {
        const response = await api.auth.register(
          username,
          email,
          password,
          psnId,
          playstyle
        );

        authToken.set(response.token);

        const user: User = {
          userId: response.userId,
          username: response.username,
          email: response.email,
          psnId,
          playstyle,
        };

        set({
          user,
          isAuthenticated: true,
          isLoading: false,
        });

        return { success: true };
      } catch (error) {
        update(s => ({ ...s, isLoading: false }));
        return {
          success: false,
          error: error instanceof Error ? error.message : 'Registration failed',
        };
      }
    },

    async login(emailOrUsername: string, password: string) {
      update(s => ({ ...s, isLoading: true }));

      try {
        const response = await api.auth.login(emailOrUsername, password);
        authToken.set(response.token);

        const user: User = {
          userId: response.userId,
          username: response.username,
          email: response.email,
          psnId: response.psnId,
          playstyle: response.playstyle,
        };

        set({
          user,
          isAuthenticated: true,
          isLoading: false,
        });

        return { success: true };
      } catch (error) {
        update(s => ({ ...s, isLoading: false }));
        return {
          success: false,
          error: error instanceof Error ? error.message : 'Login failed',
        };
      }
    },

    logout() {
      authToken.remove();
      set({
        user: null,
        isAuthenticated: false,
        isLoading: false,
      });
    },
  };
}

export const authStore = createAuthStore();