<script lang="ts">
  import { onMount } from 'svelte';
  import { page } from '$app/stores';
  import { characterStore } from '$lib/stores/character';
  import { authStore } from '$lib/stores/auth';
  import { api } from '$lib/api';
  import { goto } from '$app/navigation';
  import { get } from 'svelte/store';

  interface Character {
    id: string;
    playerName: string;
    currentLevel: number;
    currentRunes: number;
    defeatedBossIds: string[];
    obtainedWeaponIds: string[];
    playtimeHours: number;
    totalDeaths: number;
    visitedAreas: string[];
    recentBossAttempts: string[];
    createdAt: string;
  }

  interface Boss {
    id: string;
    name: string;
    image: string;
    region: string;
  }

  interface Weapon {
    id: string;
    name: string;
    image: string;
    category: string;
  }

  let character: Character | null = null;
  let isLoading = true;
  let error = '';
  let allBosses: Boss[] = [];
  let allWeapons: Weapon[] = [];
  let defeatedBosses: Boss[] = [];
  let collectedWeapons: Weapon[] = [];
  let bossSearchTerm = '';
  let weaponSearchTerm = '';

  $: isActive = $characterStore?.id === character?.id;

  // Filter defeated bosses by search term
  $: filteredDefeatedBosses = defeatedBosses.filter(boss =>
    boss.name.toLowerCase().includes(bossSearchTerm.toLowerCase()) ||
    boss.region.toLowerCase().includes(bossSearchTerm.toLowerCase())
  );

  // Filter collected weapons by search term
  $: filteredCollectedWeapons = collectedWeapons.filter(weapon =>
    weapon.name.toLowerCase().includes(weaponSearchTerm.toLowerCase()) ||
    weapon.category.toLowerCase().includes(weaponSearchTerm.toLowerCase())
  );

  onMount(() => {
    if ($authStore.isLoading) {
      const unsubscribe = authStore.subscribe(state => {
        if (!state.isLoading) {
          unsubscribe();
          if (state.isAuthenticated) {
            const id = $page.params.id;
            if (id) {
              fetchCharacter(id);
            } else {
              error = 'Invalid character ID';
              isLoading = false;
            }
          } else {
            goto('/login');
          }
        }
      });
    } else if ($authStore.isAuthenticated) {
      const id = $page.params.id;
      if (id) {
        fetchCharacter(id);
      } else {
        error = 'Invalid character ID';
        isLoading = false;
      }
    } else {
      goto('/login');
    }
  });

  async function fetchCharacter(id: string) {
    isLoading = true;
    error = '';
    character = null;

    try {
      const stored = get(characterStore);
      if (stored?.id === id) {
        character = stored;
      } else {
        const characters: Character[] = await api.playerProgress.getMyCharacters();
        const found = characters.find(c => c.id === id);
        if (!found) {
          error = 'Character not found';
          isLoading = false;
          return;
        } else {
          found.defeatedBossIds = found.defeatedBossIds || [];
          found.obtainedWeaponIds = found.obtainedWeaponIds || [];
          found.visitedAreas = found.visitedAreas || [];
          found.recentBossAttempts = found.recentBossAttempts || [];

          character = found;
        }
      }

      // Fetch all bosses and weapons to get names/images
      [allBosses, allWeapons] = await Promise.all([
        api.bosses.getAll(),
        api.weapons.getAll()
      ]);

      // Filter to only defeated bosses
      defeatedBosses = allBosses.filter(boss => 
        character?.defeatedBossIds.includes(boss.id)
      );

      // Filter to only collected weapons
      collectedWeapons = allWeapons.filter(weapon => 
        character?.obtainedWeaponIds.includes(weapon.id)
      );

    } catch (err) {
      error = err instanceof Error ? err.message : 'Failed to load character';
    } finally {
      isLoading = false;
    }
  }

  async function removeBoss(bossId: string) {
    if (!character) return;
    
    if (!confirm('Remove this boss from defeated list?')) return;

    try {
      await api.playerProgress.removeBoss(character.id, bossId);
      
      // Update local state
      defeatedBosses = defeatedBosses.filter(b => b.id !== bossId);
      
      // Update character
      const updatedCharacter = {
        ...character,
        defeatedBossIds: character.defeatedBossIds.filter(id => id !== bossId)
      };
      character = updatedCharacter;
      
      // Update character store if this is the active character
      if ($characterStore?.id === character.id) {
        characterStore.updateData(updatedCharacter);
      }
    } catch (err) {
      alert('Failed to remove boss: ' + (err instanceof Error ? err.message : 'Unknown error'));
    }
  }

  async function removeWeapon(weaponId: string) {
    if (!character) return;
    
    if (!confirm('Remove this weapon from collection?')) return;

    try {
      await api.playerProgress.removeWeapon(character.id, weaponId);
      
      // Update local state
      collectedWeapons = collectedWeapons.filter(w => w.id !== weaponId);
      
      // Update character
      const updatedCharacter = {
        ...character,
        obtainedWeaponIds: character.obtainedWeaponIds.filter(id => id !== weaponId)
      };
      character = updatedCharacter;
      
      // Update character store if this is the active character
      if ($characterStore?.id === character.id) {
        characterStore.updateData(updatedCharacter);
      }
    } catch (err) {
      alert('Failed to remove weapon: ' + (err instanceof Error ? err.message : 'Unknown error'));
    }
  }

  function setAsActive() {
    if (character) {
      characterStore.select(character);
      goto('/dashboard');
    }
  }

  function goBack() {
    goto('/dashboard');
  }

  function getProgressPercentage(current: number, total: number): number {
    if (total === 0) return 0;
    return Math.round((current / total) * 100);
  }
</script>

<svelte:head>
  <title>{character?.playerName || 'Character'} - Profile</title>
</svelte:head>

{#if isLoading}
  <div class="min-h-screen flex items-center justify-center bg-black">
    <div class="text-center">
      <div class="animate-spin rounded-full h-16 w-16 border-t-2 border-b-2 border-amber-400 mx-auto mb-4"></div>
      <p class="text-amber-400 text-lg">Loading character...</p>
    </div>
  </div>
{:else if error}
  <div class="min-h-screen flex flex-col items-center justify-center bg-black">
    <div class="bg-red-900/20 border border-red-500 rounded-lg p-6 text-center max-w-md">
      <p class="text-red-400 text-lg mb-4">{error}</p>
      <button on:click={goBack} class="px-6 py-3 bg-amber-600 hover:bg-amber-700 rounded-lg text-white font-bold transition-colors">
        Back to Dashboard
      </button>
    </div>
  </div>
{:else if character}
  <div class="min-h-screen bg-black relative overflow-hidden">
    <!-- Background -->
    <div class="absolute inset-0 opacity-30">
      <div class="absolute inset-0 bg-gradient-to-br from-amber-950 via-gray-900 to-black"></div>
      <div class="absolute top-0 right-1/4 w-96 h-96 bg-amber-600/20 rounded-full blur-3xl animate-pulse"></div>
    </div>

    <!-- Content -->
    <div class="relative max-w-7xl mx-auto p-6 z-10">
      <!-- Header -->
      <div class="flex items-center justify-between mb-8">
        <button 
          on:click={goBack} 
          class="text-amber-400 hover:text-amber-300 flex items-center gap-2 transition-colors"
        >
          <span>←</span> Back to Dashboard
        </button>
        {#if !isActive}
          <button
            on:click={setAsActive}
            class="px-6 py-3 bg-gradient-to-r from-amber-600 to-orange-600 hover:from-amber-500 hover:to-orange-500 text-white font-bold rounded-lg transition-all shadow-lg"
          >
            Set as Active Character
          </button>
        {/if}
      </div>

      <!-- Hero Section -->
      <div class="bg-gradient-to-br from-gray-900/80 to-gray-800/80 border-2 border-amber-700/40 rounded-2xl p-8 mb-8 backdrop-blur-sm">
        <div class="flex items-center gap-6 mb-6">
          <div class="w-24 h-24 bg-gradient-to-br from-amber-500 to-orange-600 rounded-xl flex items-center justify-center text-5xl shadow-lg">
            🗡️
          </div>
          <div class="flex-1">
            <div class="flex items-center gap-3 mb-2">
              <h1 class="text-4xl font-bold text-white">{character.playerName}</h1>
              {#if isActive}
                <span class="bg-amber-500 text-black text-sm px-3 py-1 rounded-full font-bold">ACTIVE</span>
              {/if}
            </div>
            <div class="flex items-center gap-4 text-gray-400">
              <span class="text-lg">Level <span class="text-amber-400 font-bold">{character.currentLevel}</span></span>
              <span>•</span>
              <span>{character.currentRunes.toLocaleString()} Runes</span>
              <span>•</span>
              <span>{character.playtimeHours.toFixed(1)}h played</span>
            </div>
          </div>
        </div>

        <!-- Progress Stats -->
        <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div class="bg-black/30 rounded-lg p-4 border border-gray-700">
            <div class="flex justify-between items-center mb-2">
              <span class="text-gray-300 text-sm">Bosses Defeated</span>
              <span class="text-amber-400 font-bold">{defeatedBosses.length} / {allBosses.length}</span>
            </div>
            <div class="h-2 bg-gray-700 rounded-full overflow-hidden">
              <div 
                class="h-full bg-gradient-to-r from-red-500 to-red-600 transition-all duration-500"
                style="width: {getProgressPercentage(defeatedBosses.length, allBosses.length)}%"
              ></div>
            </div>
          </div>

          <div class="bg-black/30 rounded-lg p-4 border border-gray-700">
            <div class="flex justify-between items-center mb-2">
              <span class="text-gray-300 text-sm">Weapons Collected</span>
              <span class="text-amber-400 font-bold">{collectedWeapons.length} / {allWeapons.length}</span>
            </div>
            <div class="h-2 bg-gray-700 rounded-full overflow-hidden">
              <div 
                class="h-full bg-gradient-to-r from-blue-500 to-blue-600 transition-all duration-500"
                style="width: {getProgressPercentage(collectedWeapons.length, allWeapons.length)}%"
              ></div>
            </div>
          </div>

          <div class="bg-black/30 rounded-lg p-4 border border-gray-700">
            <div class="flex justify-between items-center mb-2">
              <span class="text-gray-300 text-sm">Total Deaths</span>
              <span class="text-purple-400 font-bold">{character.totalDeaths}</span>
            </div>
            <div class="flex items-center gap-2 text-gray-400 text-sm">
              <span>💀</span>
              <span>Every death makes you stronger</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Collections Grid -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
        <!-- Defeated Bosses -->
        <div class="bg-gradient-to-br from-gray-900/80 to-gray-800/80 border-2 border-red-700/40 rounded-2xl p-6 backdrop-blur-sm flex flex-col">
          <div class="flex items-center justify-between mb-4">
            <h2 class="text-2xl font-bold text-white flex items-center gap-2">
              <span>⚔️</span>
              <span>Defeated Bosses</span>
              <span class="text-red-400 text-lg">({defeatedBosses.length})</span>
            </h2>
            <a
              href="/boss-fight"
              class="px-4 py-2 bg-red-600/80 hover:bg-red-600 text-white text-sm font-bold rounded-lg transition-colors"
            >
              Browse Bosses
            </a>
          </div>

          {#if defeatedBosses.length > 0}
            <!-- Search Input -->
            <div class="mb-3">
              <input
                type="text"
                bind:value={bossSearchTerm}
                placeholder="🔍 Search defeated bosses..."
                class="w-full bg-gray-700 text-white px-4 py-2 rounded-lg border border-gray-600 focus:border-red-400 focus:outline-none text-sm"
              />
            </div>
          {/if}
          
          {#if defeatedBosses.length === 0}
            <div class="text-center py-12 flex-1 flex flex-col items-center justify-center">
              <p class="text-gray-500 text-4xl mb-2">👹</p>
              <p class="text-gray-400">No bosses defeated yet</p>
              <p class="text-gray-500 text-sm mt-1">Start your journey and conquer the Lands Between!</p>
            </div>
          {:else if filteredDefeatedBosses.length === 0}
            <div class="text-center py-12 flex-1 flex flex-col items-center justify-center">
              <p class="text-gray-500 text-4xl mb-2">🔍</p>
              <p class="text-gray-400">No bosses found</p>
              <p class="text-gray-500 text-sm mt-1">Try a different search term</p>
            </div>
          {:else}
            <div class="space-y-3 max-h-[600px] overflow-y-auto pr-2 flex-1">
              {#each filteredDefeatedBosses as boss}
                <div class="bg-gray-800/50 border border-gray-700 hover:border-red-500 rounded-lg p-4 transition-all group relative">
                  <div class="flex items-center gap-4">
                    {#if boss.image}
                      <img 
                        src={boss.image} 
                        alt={boss.name}
                        class="w-16 h-16 rounded object-cover border-2 border-red-700 group-hover:border-red-500 transition-colors"
                      />
                    {:else}
                      <div class="w-16 h-16 rounded bg-gray-700 flex items-center justify-center text-3xl border-2 border-red-700">
                        👹
                      </div>
                    {/if}
                    <div class="flex-1 min-w-0">
                      <h3 class="text-white font-bold truncate">{boss.name}</h3>
                      <p class="text-gray-400 text-sm">{boss.region}</p>
                    </div>
                    <div class="flex items-center gap-2">
                      <button
                        on:click={() => goto(`/bosses/${boss.id}`)}
                        class="text-red-400 hover:text-red-300 transition-colors"
                        title="View Boss"
                      >
                        →
                      </button>
                      <button
                        on:click={() => removeBoss(boss.id)}
                        class="text-gray-400 hover:text-red-500 transition-colors text-lg"
                        title="Remove from defeated"
                      >
                        ✕
                      </button>
                    </div>
                  </div>
                </div>
              {/each}
            </div>
          {/if}
        </div>

        <!-- Collected Weapons -->
        <div class="bg-gradient-to-br from-gray-900/80 to-gray-800/80 border-2 border-blue-700/40 rounded-2xl p-6 backdrop-blur-sm flex flex-col">
          <div class="flex items-center justify-between mb-4">
            <h2 class="text-2xl font-bold text-white flex items-center gap-2">
              <span>🗡️</span>
              <span>Collected Weapons</span>
              <span class="text-blue-400 text-lg">({collectedWeapons.length})</span>
            </h2>
            <a
              href="/weapons"
              class="px-4 py-2 bg-blue-600/80 hover:bg-blue-600 text-white text-sm font-bold rounded-lg transition-colors"
            >
              Browse Weapons
            </a>
          </div>

          {#if collectedWeapons.length > 0}
            <!-- Search Input -->
            <div class="mb-3">
              <input
                type="text"
                bind:value={weaponSearchTerm}
                placeholder="🔍 Search collected weapons..."
                class="w-full bg-gray-700 text-white px-4 py-2 rounded-lg border border-gray-600 focus:border-blue-400 focus:outline-none text-sm"
              />
            </div>
          {/if}
          
          {#if collectedWeapons.length === 0}
            <div class="text-center py-12 flex-1 flex flex-col items-center justify-center">
              <p class="text-gray-500 text-4xl mb-2">⚔️</p>
              <p class="text-gray-400">No weapons collected yet</p>
              <p class="text-gray-500 text-sm mt-1">Explore the world to find powerful armaments!</p>
            </div>
          {:else if filteredCollectedWeapons.length === 0}
            <div class="text-center py-12 flex-1 flex flex-col items-center justify-center">
              <p class="text-gray-500 text-4xl mb-2">🔍</p>
              <p class="text-gray-400">No weapons found</p>
              <p class="text-gray-500 text-sm mt-1">Try a different search term</p>
            </div>
          {:else}
            <div class="space-y-3 max-h-[600px] overflow-y-auto pr-2 flex-1">
              {#each filteredCollectedWeapons as weapon}
                <div class="bg-gray-800/50 border border-gray-700 hover:border-blue-500 rounded-lg p-4 transition-all group relative">
                  <div class="flex items-center gap-4">
                    {#if weapon.image}
                      <img 
                        src={weapon.image} 
                        alt={weapon.name}
                        class="w-16 h-16 rounded object-contain bg-gray-900 border-2 border-blue-700 group-hover:border-blue-500 transition-colors p-1"
                      />
                    {:else}
                      <div class="w-16 h-16 rounded bg-gray-700 flex items-center justify-center text-3xl border-2 border-blue-700">
                        ⚔️
                      </div>
                    {/if}
                    <div class="flex-1 min-w-0">
                      <h3 class="text-white font-bold truncate">{weapon.name}</h3>
                      <p class="text-gray-400 text-sm">{weapon.category}</p>
                    </div>
                    <div class="flex items-center gap-2">
                      <button
                        on:click={() => goto(`/weapons/${weapon.id}`)}
                        class="text-blue-400 hover:text-blue-300 transition-colors"
                        title="View Weapon"
                      >
                        →
                      </button>
                      <button
                        on:click={() => removeWeapon(weapon.id)}
                        class="text-gray-400 hover:text-red-500 transition-colors text-lg"
                        title="Remove from collection"
                      >
                        ✕
                      </button>
                    </div>
                  </div>
                </div>
              {/each}
            </div>
          {/if}
        </div>
      </div>

      <!-- Visited Areas -->
      {#if character.visitedAreas && character.visitedAreas.length > 0}
        <div class="bg-gradient-to-br from-gray-900/80 to-gray-800/80 border-2 border-amber-700/40 rounded-2xl p-6 backdrop-blur-sm mt-8">
          <h2 class="text-2xl font-bold text-white mb-4 flex items-center gap-2">
            <span>🗺️</span>
            <span>Visited Areas</span>
            <span class="text-amber-400 text-lg">({character.visitedAreas.length})</span>
          </h2>
          <div class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-3">
            {#each character.visitedAreas as area}
              <div class="bg-gray-800/50 border border-gray-700 rounded-lg p-3 text-center">
                <p class="text-gray-300 text-sm">{area}</p>
              </div>
            {/each}
          </div>
        </div>
      {/if}
    </div>
  </div>
{/if}

<style>
  .hover\:scale-102:hover {
    transform: scale(1.02);
  }
</style>