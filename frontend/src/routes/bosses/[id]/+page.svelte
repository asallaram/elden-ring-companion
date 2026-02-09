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
        <p class="text-neutral-400 text-sm">Loading boss details...</p>
      </div>
    </div>
  {:else if error}
    <div class="max-w-4xl mx-auto p-6">
      <button 
        on:click={() => goto('/boss-fight')} 
        class="text-amber-600 hover:text-amber-500 mb-4 flex items-center gap-2 text-sm"
      >
        <span>←</span> Back to Bosses
      </button>
      <div class="bg-red-950/30 border border-red-900 p-6 text-center">
        <p class="text-red-400">{error}</p>
      </div>
    </div>
  {:else if boss}
    <div class="max-w-6xl mx-auto p-6">
      <!-- Back Button -->
      <button 
        on:click={() => goto('/boss-fight')} 
        class="text-amber-600 hover:text-amber-500 mb-6 flex items-center gap-2 transition-colors text-sm"
      >
        <span>←</span> Back to Bosses
      </button>

      <!-- Hero Section -->
      <div class="bg-neutral-900 border border-neutral-800 overflow-hidden mb-6">
        <div class="grid md:grid-cols-2 gap-0">
          <!-- Boss Image -->
          <div class="relative h-96 bg-black border-r border-neutral-800">
            {#if boss.image}
              <img 
                src={boss.image} 
                alt={boss.name}
                class="w-full h-full object-cover opacity-90"
              />
              <div class="absolute inset-0 bg-gradient-to-t from-black via-transparent to-transparent"></div>
            {:else}
              <div class="w-full h-full flex items-center justify-center bg-neutral-950">
                <span class="text-neutral-700 text-9xl">👹</span>
              </div>
            {/if}
          </div>

          <!-- Boss Info -->
          <div class="p-8 flex flex-col justify-between">
            <div>
              <h1 class="text-4xl font-bold text-white mb-4 uppercase tracking-wide">{boss.name}</h1>
              
              <div class="space-y-3 mb-6">
                <!-- Location -->
                <div class="flex items-start gap-3">
                  <span class="text-amber-600 text-xl">📍</span>
                  <div>
                    <p class="text-neutral-300 font-medium">{boss.region}</p>
                    {#if boss.location && boss.location !== boss.region}
                      <p class="text-neutral-600 text-sm">{boss.location}</p>
                    {/if}
                  </div>
                </div>

                <!-- HP -->
                <div class="flex items-center gap-3">
                  <span class="text-red-600 text-xl">❤️</span>
                  <div>
                    <p class="text-neutral-300">
                      <span class="font-bold text-red-400">{formatHealthPoints(boss.healthPoints)}</span>
                      <span class="text-neutral-600 text-sm ml-2">Health Points</span>
                    </p>
                  </div>
                </div>
              </div>

              <!-- Description -->
              {#if boss.description}
                <div class="bg-neutral-950 border border-neutral-800 p-4">
                  <p class="text-neutral-400 leading-relaxed italic text-sm">{boss.description}</p>
                </div>
              {/if}
            </div>

            <!-- Start Fight Button -->
            <button
              on:click={startFight}
              disabled={isStartingFight}
              class="w-full bg-amber-900 hover:bg-amber-800 disabled:bg-neutral-800 disabled:cursor-not-allowed text-white font-bold py-4 transition-colors flex items-center justify-center gap-3 text-base mt-6"
            >
              {#if isStartingFight}
                <div class="w-4 h-4 border-2 border-neutral-700 border-t-white animate-spin"></div>
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
        <div class="bg-neutral-900 border border-neutral-800 p-6">
          <h2 class="text-xl font-bold text-white mb-4 pb-4 border-b border-neutral-800 uppercase tracking-wide flex items-center gap-2">
            <span>💎</span>
            <span>Drops</span>
          </h2>
          {#if boss.drops && boss.drops.length > 0}
            <div class="space-y-2">
              {#each boss.drops as drop}
                <div class="bg-neutral-950 border border-neutral-800 p-3">
                  <div class="flex justify-between items-center">
                    <span class="text-neutral-300 font-medium text-sm">{drop.name}</span>
                    <span class="text-amber-500 font-bold text-sm">{drop.amount}</span>
                  </div>
                </div>
              {/each}
            </div>
          {:else}
            <p class="text-neutral-600 italic text-sm">No drops recorded</p>
          {/if}
        </div>

        <!-- Weaknesses Section -->
        {#if bossStats}
          <div class="bg-neutral-900 border border-neutral-800 p-6">
            <h2 class="text-xl font-bold text-white mb-4 pb-4 border-b border-neutral-800 uppercase tracking-wide flex items-center gap-2">
              <span>🎯</span>
              <span>Weaknesses</span>
            </h2>
            <div class="bg-red-950/30 border border-red-800 p-4">
              <p class="text-red-400 text-base font-bold text-center">{bossStats.weakness}</p>
            </div>
            
            <div class="mt-4 bg-neutral-950 border border-neutral-800 p-4">
              <p class="text-neutral-500 text-xs mb-2 uppercase tracking-wide">Boss Tier</p>
              <div class="flex items-center gap-2">
                {#each Array(bossStats.tier) as _, i}
                  <span class="text-amber-500 text-xl">⭐</span>
                {/each}
                <span class="text-neutral-500 ml-2 text-sm">Tier {bossStats.tier}</span>
              </div>
            </div>
          </div>
        {:else}
          <div class="bg-neutral-900 border border-neutral-800 p-6">
            <h2 class="text-xl font-bold text-white mb-4 pb-4 border-b border-neutral-800 uppercase tracking-wide flex items-center gap-2">
              <span>🎯</span>
              <span>Weaknesses</span>
            </h2>
            <div class="bg-neutral-950 border border-neutral-800 p-4 text-center">
              <p class="text-neutral-600 italic mb-2 text-sm">Weakness data coming soon</p>
              <p class="text-neutral-700 text-xs">We're working on adding detailed damage resistances</p>
            </div>
          </div>
        {/if}
      </div>

      <!-- Damage Resistances Section -->
      {#if bossStats}
        <div class="mt-6 bg-neutral-900 border border-neutral-800 p-6">
          <h2 class="text-xl font-bold text-white mb-6 pb-4 border-b border-neutral-800 uppercase tracking-wide flex items-center gap-2">
            <span>🛡️</span>
            <span>Damage Resistances</span>
          </h2>
          
          <div class="grid md:grid-cols-2 gap-6">
            <!-- Physical Resistance -->
            <div>
              <div class="flex justify-between mb-2">
                <span class="text-neutral-400 font-medium text-sm">⚔️ Physical</span>
                <span class={`font-bold text-sm ${getResistanceColor(bossStats.physicalResist)}`}>
                  {bossStats.physicalResist}%
                </span>
              </div>
              <div class="h-2 bg-neutral-800 overflow-hidden">
                <div 
                  class="h-full bg-neutral-600 transition-all duration-500"
                  style="width: {getResistanceWidth(bossStats.physicalResist)}%"
                ></div>
              </div>
            </div>

            <!-- Magic Resistance -->
            <div>
              <div class="flex justify-between mb-2">
                <span class="text-neutral-400 font-medium text-sm">✨ Magic</span>
                <span class={`font-bold text-sm ${getResistanceColor(bossStats.magicResist)}`}>
                  {bossStats.magicResist}%
                </span>
              </div>
              <div class="h-2 bg-neutral-800 overflow-hidden">
                <div 
                  class="h-full bg-blue-600 transition-all duration-500"
                  style="width: {getResistanceWidth(bossStats.magicResist)}%"
                ></div>
              </div>
            </div>

            <!-- Fire Resistance -->
            <div>
              <div class="flex justify-between mb-2">
                <span class="text-neutral-400 font-medium text-sm">🔥 Fire</span>
                <span class={`font-bold text-sm ${getResistanceColor(bossStats.fireResist)}`}>
                  {bossStats.fireResist}%
                </span>
              </div>
              <div class="h-2 bg-neutral-800 overflow-hidden">
                <div 
                  class="h-full bg-orange-600 transition-all duration-500"
                  style="width: {getResistanceWidth(bossStats.fireResist)}%"
                ></div>
              </div>
            </div>

            <!-- Lightning Resistance -->
            <div>
              <div class="flex justify-between mb-2">
                <span class="text-neutral-400 font-medium text-sm">⚡ Lightning</span>
                <span class={`font-bold text-sm ${getResistanceColor(bossStats.lightningResist)}`}>
                  {bossStats.lightningResist}%
                </span>
              </div>
              <div class="h-2 bg-neutral-800 overflow-hidden">
                <div 
                  class="h-full bg-yellow-500 transition-all duration-500"
                  style="width: {getResistanceWidth(bossStats.lightningResist)}%"
                ></div>
              </div>
            </div>

            <!-- Holy Resistance -->
            <div>
              <div class="flex justify-between mb-2">
                <span class="text-neutral-400 font-medium text-sm">✝️ Holy</span>
                <span class={`font-bold text-sm ${getResistanceColor(bossStats.holyResist)}`}>
                  {bossStats.holyResist}%
                </span>
              </div>
              <div class="h-2 bg-neutral-800 overflow-hidden">
                <div 
                  class="h-full bg-amber-500 transition-all duration-500"
                  style="width: {getResistanceWidth(bossStats.holyResist)}%"
                ></div>
              </div>
            </div>
          </div>
        </div>

        <!-- Status Immunities Section -->
        <div class="mt-6 bg-neutral-900 border border-neutral-800 p-6">
          <h2 class="text-xl font-bold text-white mb-6 pb-4 border-b border-neutral-800 uppercase tracking-wide flex items-center gap-2">
            <span>🧪</span>
            <span>Status Immunities</span>
          </h2>
          
          <div class="grid grid-cols-2 md:grid-cols-3 gap-3">
            <!-- Bleed -->
            <div class={`border-2 p-4 ${bossStats.bleedImmune ? 'bg-red-950/30 border-red-800' : 'bg-green-950/30 border-green-800'}`}>
              <div class="text-center">
                <span class="text-3xl mb-2 block">{bossStats.bleedImmune ? '❌' : '✅'}</span>
                <p class="text-sm font-medium text-neutral-300">Bleed</p>
                <p class="text-xs text-neutral-600 uppercase tracking-wide">{bossStats.bleedImmune ? 'Immune' : 'Vulnerable'}</p>
              </div>
            </div>

            <!-- Poison -->
            <div class={`border-2 p-4 ${bossStats.poisonImmune ? 'bg-red-950/30 border-red-800' : 'bg-green-950/30 border-green-800'}`}>
              <div class="text-center">
                <span class="text-3xl mb-2 block">{bossStats.poisonImmune ? '❌' : '✅'}</span>
                <p class="text-sm font-medium text-neutral-300">Poison</p>
                <p class="text-xs text-neutral-600 uppercase tracking-wide">{bossStats.poisonImmune ? 'Immune' : 'Vulnerable'}</p>
              </div>
            </div>

            <!-- Frost -->
            <div class={`border-2 p-4 ${bossStats.frostImmune ? 'bg-red-950/30 border-red-800' : 'bg-green-950/30 border-green-800'}`}>
              <div class="text-center">
                <span class="text-3xl mb-2 block">{bossStats.frostImmune ? '❌' : '✅'}</span>
                <p class="text-sm font-medium text-neutral-300">Frost</p>
                <p class="text-xs text-neutral-600 uppercase tracking-wide">{bossStats.frostImmune ? 'Immune' : 'Vulnerable'}</p>
              </div>
            </div>

            <!-- Scarlet Rot -->
            <div class={`border-2 p-4 ${bossStats.scarletRotImmune ? 'bg-red-950/30 border-red-800' : 'bg-green-950/30 border-green-800'}`}>
              <div class="text-center">
                <span class="text-3xl mb-2 block">{bossStats.scarletRotImmune ? '❌' : '✅'}</span>
                <p class="text-sm font-medium text-neutral-300">Scarlet Rot</p>
                <p class="text-xs text-neutral-600 uppercase tracking-wide">{bossStats.scarletRotImmune ? 'Immune' : 'Vulnerable'}</p>
              </div>
            </div>

            <!-- Madness -->
            <div class={`border-2 p-4 ${bossStats.madnessImmune ? 'bg-red-950/30 border-red-800' : 'bg-green-950/30 border-green-800'}`}>
              <div class="text-center">
                <span class="text-3xl mb-2 block">{bossStats.madnessImmune ? '❌' : '✅'}</span>
                <p class="text-sm font-medium text-neutral-300">Madness</p>
                <p class="text-xs text-neutral-600 uppercase tracking-wide">{bossStats.madnessImmune ? 'Immune' : 'Vulnerable'}</p>
              </div>
            </div>

            <!-- Sleep -->
            <div class={`border-2 p-4 ${bossStats.sleepImmune ? 'bg-red-950/30 border-red-800' : 'bg-green-950/30 border-green-800'}`}>
              <div class="text-center">
                <span class="text-3xl mb-2 block">{bossStats.sleepImmune ? '❌' : '✅'}</span>
                <p class="text-sm font-medium text-neutral-300">Sleep</p>
                <p class="text-xs text-neutral-600 uppercase tracking-wide">{bossStats.sleepImmune ? 'Immune' : 'Vulnerable'}</p>
              </div>
            </div>
          </div>
        </div>
      {/if}

      <!-- Best Weapons Section -->
      <div class="mt-6 bg-neutral-900 border border-neutral-800 p-6">
        <h2 class="text-xl font-bold text-white mb-6 pb-4 border-b border-neutral-800 uppercase tracking-wide flex items-center gap-2">
          <span>⚔️</span>
          <span>Best Weapons for This Boss</span>
        </h2>

        {#if isLoadingWeapons}
          <div class="text-center py-8">
            <div class="w-10 h-10 border-2 border-neutral-700 border-t-amber-600 mx-auto mb-3 animate-spin"></div>
            <p class="text-neutral-500 text-sm">Analyzing weapons...</p>
          </div>
        {:else if weaponRecommendations.length > 0}
          <div class="grid md:grid-cols-2 lg:grid-cols-3 gap-3">
            {#each weaponRecommendations as weapon}
              <button
                on:click={() => goto(`/weapons/${weapon.weaponId}`)}
                class="bg-neutral-950 border border-neutral-800 hover:border-amber-800 transition-colors text-left p-4"
              >
                <div class="flex items-center gap-3 mb-3">
                  {#if weapon.weaponImage}
                    <img src={weapon.weaponImage} alt={weapon.weaponName} class="w-12 h-12 object-contain bg-black border border-neutral-900" />
                  {:else}
                    <div class="w-12 h-12 bg-neutral-900 border border-neutral-800 flex items-center justify-center text-2xl">⚔️</div>
                  {/if}
                  <div class="flex-1 min-w-0">
                    <h3 class="text-white font-semibold truncate text-sm">{weapon.weaponName}</h3>
                    <p class="text-neutral-500 text-xs">{weapon.category}</p>
                  </div>
                </div>

                <div class="flex items-center justify-between mb-2">
                  <span class="text-neutral-500 text-xs uppercase tracking-wide">Effectiveness</span>
                  <span class="text-amber-500 font-bold text-sm">{Math.round(weapon.effectivenessScore)}</span>
                </div>

                <div class="flex items-center justify-between">
                  <span class="text-neutral-500 text-xs uppercase tracking-wide">Rating</span>
                  <span class={`px-2 py-1 text-xs font-bold ${getRatingColor(weapon.rating)}`}>
                    {weapon.rating}
                  </span>
                </div>
              </button>
            {/each}
          </div>
        {:else}
          <div class="bg-neutral-950 border border-neutral-800 p-4 text-center">
            <p class="text-neutral-600 italic text-sm">No weapon recommendations available</p>
            <p class="text-neutral-700 text-xs mt-1">Boss stats required for weapon analysis</p>
          </div>
        {/if}
      </div>
    </div>
  {/if}
</div>