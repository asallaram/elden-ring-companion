<script lang="ts">
  import { onMount } from 'svelte';
  import { page } from '$app/stores';
  import { goto } from '$app/navigation';
  import { api } from '$lib/api';
  import { authStore } from '$lib/stores/auth';

  interface BossDrop {
    name: string;
    amount: string;
  }

  interface Boss {
    id: string;
    name: string;
    image: string;
    region: string;
    location: string;
    description: string;
    healthPoints: string;
    drops: BossDrop[];
  }

  interface BossStats {
    id: string;
    bossName: string;
    healthPoints: number;
    physicalResist: number;
    magicResist: number;
    fireResist: number;
    lightningResist: number;
    holyResist: number;
    bleedImmune: boolean;
    poisonImmune: boolean;
    frostImmune: boolean;
    scarletRotImmune: boolean;
    madnessImmune: boolean;
    sleepImmune: boolean;
    weakness: string;
    tier: number;
    averageDamage: number;
  }

  interface WeaponRecommendation {
    weaponId: string;
    weaponName: string;
    weaponImage: string;
    category: string;
    effectivenessScore: number;
    rating: string;
  }

  let boss: Boss | null = null;
  let bossStats: BossStats | null = null;
  let weaponRecommendations: WeaponRecommendation[] = [];
  let isLoading = true;
  let isLoadingWeapons = false;
  let error = '';
  let isStartingFight = false;

  onMount(() => {
    if ($authStore.isLoading) {
      const unsubscribe = authStore.subscribe(state => {
        if (!state.isLoading) {
          unsubscribe();
          if (state.isAuthenticated) {
            loadBossData();
          } else {
            goto('/login');
          }
        }
      });
    } else if ($authStore.isAuthenticated) {
      loadBossData();
    } else {
      goto('/login');
    }
  });

  async function loadBossData() {
    const bossId = $page.params.id;

    if (!bossId) {
      error = 'Invalid boss ID';
      isLoading = false;
      return;
    }

    try {
      boss = await api.bosses.getById(bossId);
      
      // Try to load stats
      try {
        bossStats = await api.bosses.getStats(bossId);
      } catch (statsErr) {
        console.log('Boss stats not available:', statsErr);
      }

      // Load weapon recommendations
      isLoadingWeapons = true;
      try {
        weaponRecommendations = await api.bosses.getWeaponRecommendations(bossId);
        console.log('Weapon recommendations loaded:', weaponRecommendations.length);
      } catch (weaponErr) {
        console.log('Could not load weapon recommendations:', weaponErr);
      } finally {
        isLoadingWeapons = false;
      }
    } catch (err) {
      console.error('Error loading boss:', err);
      error = err instanceof Error ? err.message : 'Failed to load boss details';
    } finally {
      isLoading = false;
    }
  }

  async function startFight() {
    if (!boss || !$authStore.user) return;

    isStartingFight = true;
    try {
      await api.fights.startSession(
        $authStore.user.userId,
        boss.id,
        boss.name
      );
      goto(`/bosses/${boss.id}/fight`);
    } catch (err) {
      console.error('Error starting fight:', err);
      error = err instanceof Error ? err.message : 'Failed to start fight session';
      isStartingFight = false;
    }
  }

  function formatHealthPoints(hp: string): string {
    if (!hp || hp === '???' || hp === 'N/A') return 'Unknown';
    return hp;
  }

  function getResistanceColor(value: number): string {
    if (value >= 40) return 'text-red-400';
    if (value >= 20) return 'text-orange-400';
    if (value >= 0) return 'text-yellow-400';
    return 'text-green-400';
  }

  function getResistanceWidth(value: number): number {
    return Math.min(Math.max(value, 0), 100);
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
        <p class="text-amber-400 text-lg">Loading boss details...</p>
      </div>
    </div>
  {:else if error}
    <div class="max-w-4xl mx-auto p-6">
      <button 
        on:click={() => goto('/boss-fight')} 
        class="text-amber-400 hover:text-amber-300 mb-4 flex items-center gap-2"
      >
        <span>←</span> Back to Bosses
      </button>
      <div class="bg-red-900/20 border border-red-500 rounded-lg p-6 text-center">
        <p class="text-red-400 text-lg">{error}</p>
      </div>
    </div>
  {:else if boss}
    <div class="max-w-6xl mx-auto p-6">
      <!-- Back Button -->
      <button 
        on:click={() => goto('/boss-fight')} 
        class="text-amber-400 hover:text-amber-300 mb-6 flex items-center gap-2 transition-colors"
      >
        <span>←</span> Back to Bosses
      </button>

      <!-- Hero Section -->
      <div class="bg-gradient-to-br from-gray-900 to-gray-800 rounded-2xl overflow-hidden border border-gray-700 mb-6">
        <div class="grid md:grid-cols-2 gap-0">
          <!-- Boss Image -->
          <div class="relative h-96 bg-black">
            {#if boss.image}
              <img 
                src={boss.image} 
                alt={boss.name}
                class="w-full h-full object-cover opacity-90"
              />
              <div class="absolute inset-0 bg-gradient-to-t from-black via-transparent to-transparent"></div>
            {:else}
              <div class="w-full h-full flex items-center justify-center bg-gray-800">
                <span class="text-gray-500 text-9xl">👹</span>
              </div>
            {/if}
          </div>

          <!-- Boss Info -->
          <div class="p-8 flex flex-col justify-between">
            <div>
              <h1 class="text-4xl font-bold text-amber-400 mb-4">{boss.name}</h1>
              
              <div class="space-y-3 mb-6">
                <!-- Location -->
                <div class="flex items-start gap-3">
                  <span class="text-amber-500 text-xl">📍</span>
                  <div>
                    <p class="text-gray-300 font-medium">{boss.region}</p>
                    {#if boss.location && boss.location !== boss.region}
                      <p class="text-gray-500 text-sm">{boss.location}</p>
                    {/if}
                  </div>
                </div>

                <!-- HP -->
                <div class="flex items-center gap-3">
                  <span class="text-red-500 text-xl">❤️</span>
                  <div>
                    <p class="text-gray-300">
                      <span class="font-bold text-red-400">{formatHealthPoints(boss.healthPoints)}</span>
                      <span class="text-gray-500 text-sm ml-2">Health Points</span>
                    </p>
                  </div>
                </div>
              </div>

              <!-- Description -->
              {#if boss.description}
                <div class="bg-black/30 rounded-lg p-4 border border-gray-700">
                  <p class="text-gray-300 leading-relaxed italic">{boss.description}</p>
                </div>
              {/if}
            </div>

            <!-- Start Fight Button -->
            <button
              on:click={startFight}
              disabled={isStartingFight}
              class="w-full bg-amber-500 hover:bg-amber-600 disabled:bg-gray-600 disabled:cursor-not-allowed text-black font-bold py-4 rounded-lg transition-all duration-200 flex items-center justify-center gap-3 text-lg mt-6 shadow-lg hover:shadow-amber-500/50"
            >
              {#if isStartingFight}
                <div class="animate-spin rounded-full h-5 w-5 border-t-2 border-b-2 border-black"></div>
                <span>Starting Fight...</span>
              {:else}
                <span>⚔️</span>
                <span>Start Fight</span>
              {/if}
            </button>
          </div>
        </div>
      </div>

      <div class="grid md:grid-cols-2 gap-6">
        <!-- Drops Section -->
        <div class="bg-gradient-to-br from-gray-900 to-gray-800 rounded-xl p-6 border border-gray-700">
          <h2 class="text-2xl font-bold text-amber-400 mb-4 flex items-center gap-2">
            <span>💎</span>
            <span>Drops</span>
          </h2>
          {#if boss.drops && boss.drops.length > 0}
            <div class="space-y-3">
              {#each boss.drops as drop}
                <div class="bg-black/30 rounded-lg p-3 border border-gray-700">
                  <div class="flex justify-between items-center">
                    <span class="text-gray-200 font-medium">{drop.name}</span>
                    <span class="text-amber-400 font-bold">{drop.amount}</span>
                  </div>
                </div>
              {/each}
            </div>
          {:else}
            <p class="text-gray-500 italic">No drops recorded</p>
          {/if}
        </div>

        <!-- Weaknesses Section -->
        {#if bossStats}
          <div class="bg-gradient-to-br from-gray-900 to-gray-800 rounded-xl p-6 border border-gray-700">
            <h2 class="text-2xl font-bold text-amber-400 mb-4 flex items-center gap-2">
              <span>🎯</span>
              <span>Weaknesses</span>
            </h2>
            <div class="bg-red-900/20 border border-red-500 rounded-lg p-4">
              <p class="text-red-400 text-lg font-bold text-center">{bossStats.weakness}</p>
            </div>
            
            <div class="mt-4 bg-black/30 rounded-lg p-4 border border-gray-700">
              <p class="text-gray-400 text-sm mb-2">Boss Tier</p>
              <div class="flex items-center gap-2">
                {#each Array(bossStats.tier) as _, i}
                  <span class="text-amber-400 text-xl">⭐</span>
                {/each}
                <span class="text-gray-400 ml-2">Tier {bossStats.tier}</span>
              </div>
            </div>
          </div>
        {:else}
          <div class="bg-gradient-to-br from-gray-900 to-gray-800 rounded-xl p-6 border border-gray-700">
            <h2 class="text-2xl font-bold text-amber-400 mb-4 flex items-center gap-2">
              <span>🎯</span>
              <span>Weaknesses</span>
            </h2>
            <div class="bg-black/30 rounded-lg p-4 border border-gray-700 text-center">
              <p class="text-gray-500 italic mb-2">Weakness data coming soon</p>
              <p class="text-gray-600 text-sm">We're working on adding detailed damage resistances and status immunities</p>
            </div>
          </div>
        {/if}
      </div>

      <!-- Damage Resistances Section -->
      {#if bossStats}
        <div class="mt-6 bg-gradient-to-br from-gray-900 to-gray-800 rounded-xl p-6 border border-gray-700">
          <h2 class="text-2xl font-bold text-amber-400 mb-6 flex items-center gap-2">
            <span>🛡️</span>
            <span>Damage Resistances</span>
          </h2>
          
          <div class="grid md:grid-cols-2 gap-6">
            <!-- Physical Resistance -->
            <div>
              <div class="flex justify-between mb-2">
                <span class="text-gray-300 font-medium">⚔️ Physical</span>
                <span class={`font-bold ${getResistanceColor(bossStats.physicalResist)}`}>
                  {bossStats.physicalResist}%
                </span>
              </div>
              <div class="h-3 bg-gray-700 rounded-full overflow-hidden">
                <div 
                  class="h-full bg-gradient-to-r from-gray-400 to-gray-500 transition-all duration-500"
                  style="width: {getResistanceWidth(bossStats.physicalResist)}%"
                ></div>
              </div>
            </div>

            <!-- Magic Resistance -->
            <div>
              <div class="flex justify-between mb-2">
                <span class="text-gray-300 font-medium">✨ Magic</span>
                <span class={`font-bold ${getResistanceColor(bossStats.magicResist)}`}>
                  {bossStats.magicResist}%
                </span>
              </div>
              <div class="h-3 bg-gray-700 rounded-full overflow-hidden">
                <div 
                  class="h-full bg-gradient-to-r from-blue-400 to-purple-500 transition-all duration-500"
                  style="width: {getResistanceWidth(bossStats.magicResist)}%"
                ></div>
              </div>
            </div>

            <!-- Fire Resistance -->
            <div>
              <div class="flex justify-between mb-2">
                <span class="text-gray-300 font-medium">🔥 Fire</span>
                <span class={`font-bold ${getResistanceColor(bossStats.fireResist)}`}>
                  {bossStats.fireResist}%
                </span>
              </div>
              <div class="h-3 bg-gray-700 rounded-full overflow-hidden">
                <div 
                  class="h-full bg-gradient-to-r from-orange-400 to-red-500 transition-all duration-500"
                  style="width: {getResistanceWidth(bossStats.fireResist)}%"
                ></div>
              </div>
            </div>

            <!-- Lightning Resistance -->
            <div>
              <div class="flex justify-between mb-2">
                <span class="text-gray-300 font-medium">⚡ Lightning</span>
                <span class={`font-bold ${getResistanceColor(bossStats.lightningResist)}`}>
                  {bossStats.lightningResist}%
                </span>
              </div>
              <div class="h-3 bg-gray-700 rounded-full overflow-hidden">
                <div 
                  class="h-full bg-gradient-to-r from-yellow-300 to-yellow-500 transition-all duration-500"
                  style="width: {getResistanceWidth(bossStats.lightningResist)}%"
                ></div>
              </div>
            </div>

            <!-- Holy Resistance -->
            <div>
              <div class="flex justify-between mb-2">
                <span class="text-gray-300 font-medium">✝️ Holy</span>
                <span class={`font-bold ${getResistanceColor(bossStats.holyResist)}`}>
                  {bossStats.holyResist}%
                </span>
              </div>
              <div class="h-3 bg-gray-700 rounded-full overflow-hidden">
                <div 
                  class="h-full bg-gradient-to-r from-yellow-200 to-amber-400 transition-all duration-500"
                  style="width: {getResistanceWidth(bossStats.holyResist)}%"
                ></div>
              </div>
            </div>
          </div>
        </div>

        <!-- Status Immunities Section -->
        <div class="mt-6 bg-gradient-to-br from-gray-900 to-gray-800 rounded-xl p-6 border border-gray-700">
          <h2 class="text-2xl font-bold text-amber-400 mb-6 flex items-center gap-2">
            <span>🧪</span>
            <span>Status Immunities</span>
          </h2>
          
          <div class="grid grid-cols-2 md:grid-cols-3 gap-4">
            <!-- Bleed -->
            <div class={`rounded-lg p-4 border-2 ${bossStats.bleedImmune ? 'bg-red-900/20 border-red-500' : 'bg-green-900/20 border-green-500'}`}>
              <div class="text-center">
                <span class="text-3xl mb-2 block">{bossStats.bleedImmune ? '❌' : '✅'}</span>
                <p class="text-sm font-medium text-gray-300">Bleed</p>
                <p class="text-xs text-gray-500">{bossStats.bleedImmune ? 'Immune' : 'Vulnerable'}</p>
              </div>
            </div>

            <!-- Poison -->
            <div class={`rounded-lg p-4 border-2 ${bossStats.poisonImmune ? 'bg-red-900/20 border-red-500' : 'bg-green-900/20 border-green-500'}`}>
              <div class="text-center">
                <span class="text-3xl mb-2 block">{bossStats.poisonImmune ? '❌' : '✅'}</span>
                <p class="text-sm font-medium text-gray-300">Poison</p>
                <p class="text-xs text-gray-500">{bossStats.poisonImmune ? 'Immune' : 'Vulnerable'}</p>
              </div>
            </div>

            <!-- Frost -->
            <div class={`rounded-lg p-4 border-2 ${bossStats.frostImmune ? 'bg-red-900/20 border-red-500' : 'bg-green-900/20 border-green-500'}`}>
              <div class="text-center">
                <span class="text-3xl mb-2 block">{bossStats.frostImmune ? '❌' : '✅'}</span>
                <p class="text-sm font-medium text-gray-300">Frost</p>
                <p class="text-xs text-gray-500">{bossStats.frostImmune ? 'Immune' : 'Vulnerable'}</p>
              </div>
            </div>

            <!-- Scarlet Rot -->
            <div class={`rounded-lg p-4 border-2 ${bossStats.scarletRotImmune ? 'bg-red-900/20 border-red-500' : 'bg-green-900/20 border-green-500'}`}>
              <div class="text-center">
                <span class="text-3xl mb-2 block">{bossStats.scarletRotImmune ? '❌' : '✅'}</span>
                <p class="text-sm font-medium text-gray-300">Scarlet Rot</p>
                <p class="text-xs text-gray-500">{bossStats.scarletRotImmune ? 'Immune' : 'Vulnerable'}</p>
              </div>
            </div>

            <!-- Madness -->
            <div class={`rounded-lg p-4 border-2 ${bossStats.madnessImmune ? 'bg-red-900/20 border-red-500' : 'bg-green-900/20 border-green-500'}`}>
              <div class="text-center">
                <span class="text-3xl mb-2 block">{bossStats.madnessImmune ? '❌' : '✅'}</span>
                <p class="text-sm font-medium text-gray-300">Madness</p>
                <p class="text-xs text-gray-500">{bossStats.madnessImmune ? 'Immune' : 'Vulnerable'}</p>
              </div>
            </div>

            <!-- Sleep -->
            <div class={`rounded-lg p-4 border-2 ${bossStats.sleepImmune ? 'bg-red-900/20 border-red-500' : 'bg-green-900/20 border-green-500'}`}>
              <div class="text-center">
                <span class="text-3xl mb-2 block">{bossStats.sleepImmune ? '❌' : '✅'}</span>
                <p class="text-sm font-medium text-gray-300">Sleep</p>
                <p class="text-xs text-gray-500">{bossStats.sleepImmune ? 'Immune' : 'Vulnerable'}</p>
              </div>
            </div>
          </div>
        </div>
      {/if}

      <!-- Best Weapons Section -->
      <div class="mt-6 bg-gradient-to-br from-gray-900 to-gray-800 rounded-xl p-6 border border-gray-700">
        <h2 class="text-2xl font-bold text-amber-400 mb-6 flex items-center gap-2">
          <span>⚔️</span>
          <span>Best Weapons for This Boss</span>
        </h2>

        {#if isLoadingWeapons}
          <div class="text-center py-8">
            <div class="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-amber-400 mx-auto mb-3"></div>
            <p class="text-gray-400">Analyzing weapons...</p>
          </div>
        {:else if weaponRecommendations.length > 0}
          <div class="grid md:grid-cols-2 lg:grid-cols-3 gap-4">
            {#each weaponRecommendations as weapon}
              <button
                on:click={() => goto(`/weapons/${weapon.weaponId}`)}
                class="bg-black/30 rounded-lg p-4 border border-gray-700 hover:border-amber-500 transition-colors text-left"
              >
                <div class="flex items-center gap-3 mb-3">
                  {#if weapon.weaponImage}
                    <img src={weapon.weaponImage} alt={weapon.weaponName} class="w-12 h-12 rounded object-contain bg-gray-800" />
                  {:else}
                    <div class="w-12 h-12 rounded bg-gray-700 flex items-center justify-center text-2xl">⚔️</div>
                  {/if}
                  <div class="flex-1 min-w-0">
                    <h3 class="text-white font-semibold truncate">{weapon.weaponName}</h3>
                    <p class="text-gray-400 text-xs">{weapon.category}</p>
                  </div>
                </div>

                <div class="flex items-center justify-between mb-2">
                  <span class="text-gray-400 text-sm">Effectiveness</span>
                  <span class="text-amber-400 font-bold">{Math.round(weapon.effectivenessScore)}</span>
                </div>

                <div class="flex items-center justify-between">
                  <span class="text-gray-400 text-sm">Rating</span>
                  <span class={`px-2 py-1 rounded text-xs font-bold ${getRatingColor(weapon.rating)}`}>
                    {weapon.rating}
                  </span>
                </div>
              </button>
            {/each}
          </div>
        {:else}
          <div class="bg-black/30 rounded-lg p-4 border border-gray-700 text-center">
            <p class="text-gray-500 italic">No weapon recommendations available</p>
            <p class="text-gray-600 text-sm mt-1">Boss stats required for weapon analysis</p>
          </div>
        {/if}
      </div>
    </div>
  {/if}
</div>