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
      'Excellent': 'bg-green-900 text-green-300 border border-green-700',
      'Good': 'bg-blue-900 text-blue-300 border border-blue-700',
      'Average': 'bg-yellow-900 text-yellow-300 border border-yellow-700',
      'Poor': 'bg-red-900 text-red-300 border border-red-700',
    };
    return colors[rating] || 'bg-neutral-800 text-neutral-400 border border-neutral-700';
  }
</script>

<div class="min-h-screen bg-[#0a0a0a] text-white">
  {#if isLoading}
    <div class="flex items-center justify-center min-h-screen">
      <div class="text-center">
        <div class="w-12 h-12 border-2 border-neutral-700 border-t-amber-600 mx-auto mb-4 animate-spin"></div>
        <p class="text-neutral-400 text-sm">Loading weapon details...</p>
      </div>
    </div>
  {:else if error}
    <div class="max-w-4xl mx-auto p-6">
      <button 
        on:click={() => goto('/weapons')} 
        class="text-amber-600 hover:text-amber-500 mb-4 flex items-center gap-2 text-sm"
      >
        <span>←</span> Back to Weapons
      </button>
      <div class="bg-red-950/30 border border-red-900 p-6 text-center">
        <p class="text-red-400">{error}</p>
      </div>
    </div>
  {:else if weapon}
    <div class="max-w-6xl mx-auto p-6">
      <button 
        on:click={() => goto('/weapons')} 
        class="text-amber-600 hover:text-amber-500 mb-6 flex items-center gap-2 transition-colors text-sm"
      >
        <span>←</span> Back to Weapons
      </button>

      <div class="bg-neutral-900 border border-neutral-800 overflow-hidden mb-6">
        <div class="grid md:grid-cols-2 gap-0">
          <div class="relative h-96 bg-black border-r border-neutral-800 flex items-center justify-center">
            {#if weapon.image}
              <img 
                src={weapon.image} 
                alt={weapon.name}
                class="max-h-full max-w-full object-contain opacity-90 p-8"
              />
              <div class="absolute inset-0 bg-gradient-to-t from-black via-transparent to-transparent"></div>
            {:else}
              <div class="text-neutral-700 text-9xl">⚔️</div>
            {/if}
          </div>

          <div class="p-8 flex flex-col justify-between">
            <div>
              <h1 class="text-4xl font-bold text-white mb-2 uppercase tracking-wide">{weapon.name}</h1>
              
              {#if weapon.category}
                <div class="inline-block bg-amber-900 border border-amber-700 px-4 py-1 text-xs font-bold mb-4 uppercase tracking-wide">
                  {weapon.category}
                </div>
              {/if}

              <div class="space-y-3 mb-6">
                <div class="flex items-center gap-3">
                  <span class="text-amber-600 text-xl">⚖️</span>
                  <div>
                    <p class="text-neutral-300">
                      <span class="font-bold text-white">{weapon.weight}</span>
                      <span class="text-neutral-600 text-sm ml-2">Weight</span>
                    </p>
                  </div>
                </div>
              </div>

              {#if weapon.description}
                <div class="bg-neutral-950 border border-neutral-800 p-4">
                  <p class="text-neutral-400 leading-relaxed italic text-sm">{weapon.description}</p>
                </div>
              {/if}
            </div>
          </div>
        </div>
      </div>

      <div class="grid md:grid-cols-2 gap-6 mb-6">
        <div class="bg-neutral-900 border border-neutral-800 p-6">
          <h2 class="text-xl font-bold text-white mb-4 pb-4 border-b border-neutral-800 uppercase tracking-wide flex items-center gap-2">
            <span>⚔️</span>
            <span>Attack Power</span>
          </h2>
          {#if weapon.attack && weapon.attack.length > 0}
            <div class="space-y-2">
              {#each weapon.attack.filter(a => a.amount > 0) as atk}
                <div class="flex items-center justify-between bg-neutral-950 border border-neutral-800 p-3">
                  <div class="flex items-center gap-2">
                    <span class="text-xl">{getDamageTypeIcon(atk.name)}</span>
                    <span class="text-neutral-400 text-sm">{atk.name}</span>
                  </div>
                  <span class="text-amber-500 font-bold">{atk.amount}</span>
                </div>
              {/each}
            </div>
          {:else}
            <p class="text-neutral-600 italic text-sm">No attack data</p>
          {/if}
        </div>

        <div class="bg-neutral-900 border border-neutral-800 p-6">
          <h2 class="text-xl font-bold text-white mb-4 pb-4 border-b border-neutral-800 uppercase tracking-wide flex items-center gap-2">
            <span>📋</span>
            <span>Requirements</span>
          </h2>
          {#if weapon.requiredAttributes && weapon.requiredAttributes.length > 0}
            <div class="space-y-2">
              {#each weapon.requiredAttributes.filter(r => r.name && r.name !== '-' && r.amount > 0) as req}
                <div class="flex items-center justify-between bg-neutral-950 border border-neutral-800 p-3">
                  <span class="text-neutral-400 font-medium text-sm">{req.name}</span>
                  <span class="text-white font-bold">{req.amount}</span>
                </div>
              {/each}
            </div>
          {:else}
            <p class="text-neutral-600 italic text-sm">No requirements</p>
          {/if}
        </div>
      </div>

      <div class="bg-neutral-900 border border-neutral-800 p-6 mb-6">
        <h2 class="text-xl font-bold text-white mb-4 pb-4 border-b border-neutral-800 uppercase tracking-wide flex items-center gap-2">
          <span>📈</span>
          <span>Attribute Scaling</span>
        </h2>
        {#if weapon.scalesWith && weapon.scalesWith.length > 0}
          <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
            {#each weapon.scalesWith.filter(s => s.name && s.name !== '-') as scale}
              <div class="bg-neutral-950 border border-neutral-800 p-4 text-center">
                <div class="text-neutral-500 text-xs mb-2 uppercase tracking-wide">{scale.name}</div>
                <div class={`text-3xl font-bold ${getScalingColor(scale.scaling)}`}>
                  {scale.scaling}
                </div>
              </div>
            {/each}
          </div>
          <div class="mt-4 text-xs text-neutral-600 text-center">
            S = Best • A = Excellent • B = Good • C = Average • D = Poor • E = Minimal
          </div>
        {:else}
          <p class="text-neutral-600 italic text-sm">No scaling data</p>
        {/if}
      </div>

      {#if weapon.defence && weapon.defence.some(d => d.amount > 0)}
        <div class="bg-neutral-900 border border-neutral-800 p-6 mb-6">
          <h2 class="text-xl font-bold text-white mb-4 pb-4 border-b border-neutral-800 uppercase tracking-wide flex items-center gap-2">
            <span>🛡️</span>
            <span>Defence</span>
          </h2>
          <div class="grid grid-cols-2 md:grid-cols-3 gap-3">
            {#each weapon.defence.filter(d => d.amount > 0) as def}
              <div class="flex items-center justify-between bg-neutral-950 border border-neutral-800 p-3">
                <span class="text-neutral-400 text-sm">{def.name}</span>
                <span class="text-blue-400 font-bold">{def.amount}</span>
              </div>
            {/each}
          </div>
        </div>
      {/if}

      <div class="bg-neutral-900 border border-neutral-800 p-6">
        <h2 class="text-xl font-bold text-white mb-4 pb-4 border-b border-neutral-800 uppercase tracking-wide flex items-center gap-2">
          <span>🎯</span>
          <span>Effective Against</span>
        </h2>
        
        {#if isLoadingMatchups}
          <div class="text-center py-8">
            <div class="w-10 h-10 border-2 border-neutral-700 border-t-amber-600 mx-auto mb-3 animate-spin"></div>
            <p class="text-neutral-500 text-sm">Analyzing boss matchups...</p>
          </div>
        {:else if matchups.length > 0}
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
            {#each matchups.slice(0, 12) as matchup}
              <button
                on:click={() => goto(`/bosses/${matchup.bossId}`)}
                class="bg-neutral-950 border border-neutral-800 hover:border-amber-800 transition-colors text-left p-4"
              >
                <div class="flex items-center gap-3 mb-3">
                  {#if matchup.bossImage}
                    <img src={matchup.bossImage} alt={matchup.bossName} class="w-12 h-12 object-cover border border-neutral-900" />
                  {:else}
                    <div class="w-12 h-12 bg-neutral-900 border border-neutral-800 flex items-center justify-center text-2xl">👹</div>
                  {/if}
                  <div class="flex-1 min-w-0">
                    <h3 class="text-white font-semibold truncate text-sm">{matchup.bossName}</h3>
                    <p class="text-neutral-500 text-xs">{matchup.region}</p>
                  </div>
                </div>

                <div class="flex items-center justify-between mb-2">
                  <span class="text-neutral-500 text-xs uppercase tracking-wide">Effectiveness</span>
                  <span class="text-amber-500 font-bold text-sm">{Math.round(matchup.effectivenessScore)}</span>
                </div>

                <div class="flex items-center justify-between">
                  <span class="text-neutral-500 text-xs uppercase tracking-wide">Rating</span>
                  <span class={`px-2 py-1 text-xs font-bold ${getRatingColor(matchup.rating)}`}>
                    {matchup.rating}
                  </span>
                </div>
              </button>
            {/each}
          </div>

          {#if matchups.length > 12}
            <div class="mt-4 text-center">
              <p class="text-neutral-600 text-sm">Showing top 12 of {matchups.length} boss matchups</p>
            </div>
          {/if}
        {:else}
          <div class="bg-neutral-950 border border-neutral-800 p-4 text-center">
            <p class="text-neutral-600 italic mb-2 text-sm">No boss matchup data available</p>
            <p class="text-neutral-700 text-xs">Unable to calculate effectiveness against bosses</p>
          </div>
        {/if}
      </div>
    </div>
  {/if}
</div>