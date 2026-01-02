const API_BASE_URL = import.meta.env.MODE === 'production' 
  ? 'https://elden-ring-companion.onrender.com/api'
  : 'http://localhost:5019/api';export const authToken = {
  get: (): string | null => {
    if (typeof window !== 'undefined') {
      return localStorage.getItem('jwt_token');
    }
    return null;
  },
  set: (token: string) => {
    if (typeof window !== 'undefined') {
      localStorage.setItem('jwt_token', token);
    }
  },
  remove: () => {
    if (typeof window !== 'undefined') {
      localStorage.removeItem('jwt_token');
    }
  }
};

async function apiCall(endpoint: string, options: RequestInit = {}) {
  const token = authToken.get();
  
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
  };

  if (options.headers) {
    Object.entries(options.headers).forEach(([key, value]) => {
      headers[key] = value as string;
    });
  }

  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    ...options,
    headers,
  });

  if (!response.ok) {
    const error = await response.text();
    throw new Error(error || `HTTP error! status: ${response.status}`);
  }

  return response.json();
}

export const api = {
  auth: {
    register: async (username: string, email: string, password: string, psnId?: string, playstyle?: string) => {
      return apiCall('/auth/register', {
        method: 'POST',
        body: JSON.stringify({ username, email, password, psnId, playstyle }),
      });
    },
    
    login: async (emailOrUsername: string, password: string) => {
      return apiCall('/auth/login', {
        method: 'POST',
        body: JSON.stringify({ emailOrUsername, password }),
      });
    },
    
    getProfile: async () => {
      return apiCall('/auth/profile');
    },
  },

  playerProgress: {
    getMyCharacters: async () => {
      return apiCall('/playerprogress/my-characters');
    },
    
    createCharacter: async (playerName: string, psnId?: string, startingLevel: number = 10) => {
      return apiCall('/playerprogress', {
        method: 'POST',
        body: JSON.stringify({ playerName, psn_ID: psnId, startingLevel }),
      });
    },
    
    // Add boss (deprecated - use addBoss instead)
    defeatBoss: async (progressId: string, bossId: string) => {
      return apiCall(`/playerprogress/${progressId}/defeat-boss`, {
        method: 'POST',
        body: JSON.stringify({ bossId }),
      });
    },
    
    // Add weapon (deprecated - use addWeapon instead)
    obtainWeapon: async (progressId: string, weaponId: string) => {
      return apiCall(`/playerprogress/${progressId}/obtain-weapon`, {
        method: 'POST',
        body: JSON.stringify({ weaponId }),
      });
    },

    // NEW: Add weapon to character
    addWeapon: async (characterId: string, weaponId: string) => {
      return apiCall(`/playerprogress/${characterId}/obtain-weapon`, {
        method: 'POST',
        body: JSON.stringify({ weaponId }),
      });
    },

    // NEW: Remove weapon from character
    removeWeapon: async (characterId: string, weaponId: string) => {
      return apiCall(`/playerprogress/${characterId}/weapons/${weaponId}`, {
        method: 'DELETE',
      });
    },

    // NEW: Add defeated boss to character
    addBoss: async (characterId: string, bossId: string) => {
      return apiCall(`/playerprogress/${characterId}/defeat-boss`, {
        method: 'POST',
        body: JSON.stringify({ bossId }),
      });
    },

    // NEW: Remove boss from character
    removeBoss: async (characterId: string, bossId: string) => {
      return apiCall(`/playerprogress/${characterId}/bosses/${bossId}`, {
        method: 'DELETE',
      });
    },
  },

  bosses: {
    getAll: async () => {
      return apiCall('/bosses');
    },
    
    getById: async (id: string) => {
      return apiCall(`/bosses/${id}`);
    },

    getStats: async (id: string) => {
      return apiCall(`/bosses/${id}/stats`);
    },

    getWeaponRecommendations: async (bossId: string) => {
      return apiCall(`/bosses/${bossId}/weapon-recommendations`);
    },
  },

  weapons: {
    getAll: async () => {
      return apiCall('/weapons');
    },
    
    getById: async (id: string) => {
      return apiCall(`/weapons/${id}`);
    },

    getMatchups: async (weaponId: string) => {
      return apiCall(`/weapons/${weaponId}/matchups`);
    },
  },

  fights: {
    getActiveSession: async (progressId: string, bossId: string) => {
      return apiCall(`/bossfight/active?progressId=${progressId}&bossId=${bossId}`);
    },

    startSession: async (progressId: string, bossId: string, bossName: string) => {
      return apiCall('/bossfight/start', {
        method: 'POST',
        body: JSON.stringify({ 
          playerProgressId: progressId, 
          bossId: bossId,
          bossName: bossName
        }),
      });
    },

    recordAttempt: async (attempt: {
      progressId: string;
      bossId: string;
      bossName: string;
      weaponId?: string;
      weaponName?: string;
      notes?: string;
      victory?: boolean;
      timeSpentSeconds?: number;
    }) => {
      return apiCall('/bossfight/attempt', {
        method: 'POST',
        body: JSON.stringify({
          playerProgressId: attempt.progressId,
          bossId: attempt.bossId,
          bossName: attempt.bossName,
          weaponUsedId: attempt.weaponId || '',
          weaponUsedName: attempt.weaponName || '',
          victory: attempt.victory || false,
          timeSpentSeconds: attempt.timeSpentSeconds || 0,
          notes: attempt.notes || ''
        }),
      });
    },

    endSession: async (sessionId: string) => {
      return apiCall(`/bossfight/${sessionId}/end`, {
        method: 'POST',
      });
    },

    getSessionAttempts: async (sessionId: string) => {
      return apiCall(`/bossfight/session/${sessionId}/attempts`);
    },
  },
};