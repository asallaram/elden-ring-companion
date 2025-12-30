<script lang="ts">
  import { onMount } from 'svelte';
  import { page } from '$app/stores';
  import { goto } from '$app/navigation';
  import { api } from '$lib/api';
  import { authStore } from '$lib/stores/auth';

  interface Attack {
    name: string;
    amount: number;
  }

  interface Defence {
    name: string;
    amount: number;
  }

  interface ScalesWith {
    name: string;
    scaling: string;
  }

  interface RequiredAttribute {
    name: string;
    amount: number;
  }

  interface Weapon {
    id: string;
    name: string;
    image: string;
    description: string;
    category: string;
    weight: number;
    attack: Attack[];
    defence: Defence[];
    scalesWith: ScalesWith[];
    requiredAttributes: RequiredAttribute[];
  }

  interface BossMatchup {
    bossId: string;
    bossName: string;
    bossImage: string;
    region: string;
    effectivenessScore: number;
    rating: string;
  }

  let weapon: Weapon | null = null;
  let matchups: BossMatchup[] = [];
  let isLoading = true;
  let isLoadingMatchups = true;
  let error = '';

  onMount(() => {
  if ($authStore.isLoading) {
    const unsubscribe = authStore.subscribe(state => {
      if (!state.isLoading) {
        unsubscribe();
        if (state.isAuthenticated) {
          loadWeaponData();
        } else {
          goto('/login');
        }
      }
    });
  } else if ($authStore.isAuthenticated) {
    loadWeaponData();
  } else {
    goto('/login');
  }
});

async function loadWeaponData() {
  const weaponId = $page.params.id;
  if (!weaponId) {
    error = 'Invalid weapon ID';
    isLoading = false;
    return;
  }

  try {
    console.log('Fetching weapon:', weaponId);
    weapon = await api.weapons.getById(weaponId);
    console.log('Weapon loaded:', weapon);
    
    isLoadingMatchups = true;
    try {
      matchups = await api.weapons.getMatchups(weaponId);
      console.log('Matchups loaded:', matchups.length);
    } catch (matchupErr) {
      console.log('Could not load matchups:', matchupErr);
    } finally {
      isLoadingMatchups = false;
    }
  } catch (err) {
    console.error('Error loading weapon:', err);
    error = err instanceof Error ? err.message : 'Failed to load weapon details';
  } finally {
    isLoading = false;
  }
}

  function getScalingColor(scaling: string): string {
    const colors: Record<string, string> = {
      'S': 'text-purple-400',
      'A': 'text-green-400',
      'B': 'text-blue-400',
      'C': 'text-yellow-400',
      'D': 'text-orange-400',
      'E': 'text-gray-400',
      'None': 'text-gray-600',
      '-': 'text-gray-600'
    };
    return colors[scaling] || 'text-gray-400';
  }

  function getDamageTypeIcon(type: string): string {
    const icons: Record<string, string> = {
      'Phy': '⚔️',
      'Mag': '✨',
      'Fire': '🔥',
      'Ligt': '⚡',
      'Holy': '✝️',
      'Crit': '💥'
    };
    return icons[type] || '•';
  }

  function getRatingColor(rating: string): string {
    const colors: Record<string, string> = {
      'Excellent': 'bg-green-500/20 text-green-400 border border-green-500',
      'Good': 'bg-blue-500/20 text-blue-400 border border-blue-500',
      'Average': 'bg-yellow-500/20 text-yellow-400 border border-yellow-500',
      'Poor': 'bg-red-500/20 text-red-400 border border-red-500',
    };
    return colors[rating] || 'bg-gray-500/20 text-gray-400 border border-gray-500';
  }
</script>

<div class="min-h-screen bg-black text-white">
  {#if isLoading}
    <div class="flex items-center justify-center min-h-screen">
      <div class="text-center">
        <div class="animate-spin rounded-full h-16 w-16 border-t-2 border-b-2 border-amber-400 mx-auto mb-4"></div>
        <p class="text-amber-400 text-lg">Loading weapon details...</p>
      </div>
    </div>
  {:else if error}
    <div class="max-w-4xl mx-auto p-6">
      <button 
        on:click={() => goto('/weapons')} 
        class="text-amber-400 hover:text-amber-300 mb-4 flex items-center gap-2"
      >
        <span>←</span> Back to Weapons
      </button>
      <div class="bg-red-900/20 border border-red-500 rounded-lg p-6 text-center">
        <p class="text-red-400 text-lg">{error}</p>
      </div>
    </div>
  {:else if weapon}
    <div class="max-w-6xl mx-auto p-6">
      <button 
        on:click={() => goto('/weapons')} 
        class="text-amber-400 hover:text-amber-300 mb-6 flex items-center gap-2 transition-colors"
      >
        <span>←</span> Back to Weapons
      </button>

      <div class="bg-gradient-to-br from-gray-900 to-gray-800 rounded-2xl overflow-hidden border border-gray-700 mb-6">
        <div class="grid md:grid-cols-2 gap-0">
          <div class="relative h-96 bg-black flex items-center justify-center">
            {#if weapon.image}
              <img 
                src={weapon.image} 
                alt={weapon.name}
                class="max-h-full max-w-full object-contain opacity-90 p-8"
              />
              <div class="absolute inset-0 bg-gradient-to-t from-black via-transparent to-transparent"></div>
            {:else}
              <div class="text-gray-500 text-9xl">⚔️</div>
            {/if}
          </div>

          <div class="p-8 flex flex-col justify-between">
            <div>
              <h1 class="text-4xl font-bold text-amber-400 mb-2">{weapon.name}</h1>
              
              {#if weapon.category}
                <div class="inline-block bg-amber-600/80 px-4 py-1 rounded-full text-sm font-bold mb-4">
                  {weapon.category}
                </div>
              {/if}

              <div class="space-y-3 mb-6">
                <div class="flex items-center gap-3">
                  <span class="text-amber-500 text-xl">⚖️</span>
                  <div>
                    <p class="text-gray-300">
                      <span class="font-bold text-white">{weapon.weight}</span>
                      <span class="text-gray-500 text-sm ml-2">Weight</span>
                    </p>
                  </div>
                </div>
              </div>

              {#if weapon.description}
                <div class="bg-black/30 rounded-lg p-4 border border-gray-700">
                  <p class="text-gray-300 leading-relaxed italic text-sm">{weapon.description}</p>
                </div>
              {/if}
            </div>
          </div>
        </div>
      </div>

      <div class="grid md:grid-cols-2 gap-6 mb-6">
        <div class="bg-gradient-to-br from-gray-900 to-gray-800 rounded-xl p-6 border border-gray-700">
          <h2 class="text-2xl font-bold text-amber-400 mb-4 flex items-center gap-2">
            <span>⚔️</span>
            <span>Attack Power</span>
          </h2>
          {#if weapon.attack && weapon.attack.length > 0}
            <div class="space-y-3">
              {#each weapon.attack.filter(a => a.amount > 0) as atk}
                <div class="flex items-center justify-between bg-black/30 rounded-lg p-3 border border-gray-700">
                  <div class="flex items-center gap-2">
                    <span class="text-xl">{getDamageTypeIcon(atk.name)}</span>
                    <span class="text-gray-300">{atk.name}</span>
                  </div>
                  <span class="text-amber-400 font-bold text-lg">{atk.amount}</span>
                </div>
              {/each}
            </div>
          {:else}
            <p class="text-gray-500 italic">No attack data</p>
          {/if}
        </div>

        <div class="bg-gradient-to-br from-gray-900 to-gray-800 rounded-xl p-6 border border-gray-700">
          <h2 class="text-2xl font-bold text-amber-400 mb-4 flex items-center gap-2">
            <span>📋</span>
            <span>Requirements</span>
          </h2>
          {#if weapon.requiredAttributes && weapon.requiredAttributes.length > 0}
            <div class="space-y-3">
              {#each weapon.requiredAttributes.filter(r => r.name && r.name !== '-' && r.amount > 0) as req}
                <div class="flex items-center justify-between bg-black/30 rounded-lg p-3 border border-gray-700">
                  <span class="text-gray-300 font-medium">{req.name}</span>
                  <span class="text-white font-bold text-lg">{req.amount}</span>
                </div>
              {/each}
            </div>
          {:else}
            <p class="text-gray-500 italic">No requirements</p>
          {/if}
        </div>
      </div>

      <div class="bg-gradient-to-br from-gray-900 to-gray-800 rounded-xl p-6 border border-gray-700 mb-6">
        <h2 class="text-2xl font-bold text-amber-400 mb-4 flex items-center gap-2">
          <span>📈</span>
          <span>Attribute Scaling</span>
        </h2>
        {#if weapon.scalesWith && weapon.scalesWith.length > 0}
          <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
            {#each weapon.scalesWith.filter(s => s.name && s.name !== '-') as scale}
              <div class="bg-black/30 rounded-lg p-4 border border-gray-700 text-center">
                <div class="text-gray-400 text-sm mb-2">{scale.name}</div>
                <div class={`text-3xl font-bold ${getScalingColor(scale.scaling)}`}>
                  {scale.scaling}
                </div>
              </div>
            {/each}
          </div>
          <div class="mt-4 text-xs text-gray-500 text-center">
            S = Best • A = Excellent • B = Good • C = Average • D = Poor • E = Minimal
          </div>
        {:else}
          <p class="text-gray-500 italic">No scaling data</p>
        {/if}
      </div>

      {#if weapon.defence && weapon.defence.some(d => d.amount > 0)}
        <div class="bg-gradient-to-br from-gray-900 to-gray-800 rounded-xl p-6 border border-gray-700 mb-6">
          <h2 class="text-2xl font-bold text-amber-400 mb-4 flex items-center gap-2">
            <span>🛡️</span>
            <span>Defence</span>
          </h2>
          <div class="grid grid-cols-2 md:grid-cols-3 gap-4">
            {#each weapon.defence.filter(d => d.amount > 0) as def}
              <div class="flex items-center justify-between bg-black/30 rounded-lg p-3 border border-gray-700">
                <span class="text-gray-300 text-sm">{def.name}</span>
                <span class="text-blue-400 font-bold">{def.amount}</span>
              </div>
            {/each}
          </div>
        </div>
      {/if}

      <div class="bg-gradient-to-br from-gray-900 to-gray-800 rounded-xl p-6 border border-gray-700">
        <h2 class="text-2xl font-bold text-amber-400 mb-4 flex items-center gap-2">
          <span>🎯</span>
          <span>Effective Against</span>
        </h2>
        
        {#if isLoadingMatchups}
          <div class="text-center py-8">
            <div class="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-amber-400 mx-auto mb-3"></div>
            <p class="text-gray-400">Analyzing boss matchups...</p>
          </div>
        {:else if matchups.length > 0}
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {#each matchups.slice(0, 12) as matchup}
              <button
                on:click={() => goto(`/bosses/${matchup.bossId}`)}
                class="bg-black/30 rounded-lg p-4 border border-gray-700 hover:border-amber-500 transition-colors text-left"
              >
                <div class="flex items-center gap-3 mb-3">
                  {#if matchup.bossImage}
                    <img src={matchup.bossImage} alt={matchup.bossName} class="w-12 h-12 rounded object-cover" />
                  {:else}
                    <div class="w-12 h-12 rounded bg-gray-700 flex items-center justify-center text-2xl">👹</div>
                  {/if}
                  <div class="flex-1 min-w-0">
                    <h3 class="text-white font-semibold truncate">{matchup.bossName}</h3>
                    <p class="text-gray-400 text-xs">{matchup.region}</p>
                  </div>
                </div>

                <div class="flex items-center justify-between mb-2">
                  <span class="text-gray-400 text-sm">Effectiveness</span>
                  <span class="text-amber-400 font-bold">{Math.round(matchup.effectivenessScore)}</span>
                </div>

                <div class="flex items-center justify-between">
                  <span class="text-gray-400 text-sm">Rating</span>
                  <span class={`px-2 py-1 rounded text-xs font-bold ${getRatingColor(matchup.rating)}`}>
                    {matchup.rating}
                  </span>
                </div>
              </button>
            {/each}
          </div>

          {#if matchups.length > 12}
            <div class="mt-4 text-center">
              <p class="text-gray-500 text-sm">Showing top 12 of {matchups.length} boss matchups</p>
            </div>
          {/if}
        {:else}
          <div class="bg-black/30 rounded-lg p-4 border border-gray-700 text-center">
            <p class="text-gray-500 italic mb-2">No boss matchup data available</p>
            <p class="text-gray-600 text-sm">Unable to calculate effectiveness against bosses</p>
          </div>
        {/if}
      </div>
    </div>
  {/if}
</div>