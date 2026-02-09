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
  let showDeleteModal = false;
  let isDeleting = false;

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
  <div class="min-h-screen flex items-center justify-center bg-[#0a0a0a]">
    <div class="text-center">
      <div class="w-12 h-12 border-2 border-neutral-700 border-t-amber-600 mx-auto mb-4 animate-spin"></div>
      <p class="text-neutral-400 text-sm">Loading character...</p>
    </div>
  </div>
{:else if error}
  <div class="min-h-screen flex flex-col items-center justify-center bg-[#0a0a0a]">
    <div class="bg-red-950/30 border border-red-900 p-6 text-center max-w-md">
      <p class="text-red-400 mb-4">{error}</p>
      <button on:click={goBack} class="px-6 py-3 bg-amber-900 hover:bg-amber-800 text-white font-semibold transition-colors">
        Back to Dashboard
      </button>
    </div>
  </div>
{:else if character}
  <div class="min-h-screen bg-[#0a0a0a] relative">
    <!-- Content -->
    <div class="relative max-w-7xl mx-auto p-6">
      <!-- Header -->
      <div class="flex items-center justify-between mb-8">
        <button 
          on:click={goBack} 
          class="text-amber-600 hover:text-amber-500 flex items-center gap-2 transition-colors text-sm"
        >
          <span>←</span> Back to Dashboard
        </button>
        <div class="flex items-center gap-3">
          {#if !isActive}
            <button
              on:click={setAsActive}
              class="px-6 py-3 bg-amber-900 hover:bg-amber-800 text-white font-semibold transition-colors text-sm"
            >
              Set as Active Character
            </button>
          {/if}
          <button
            on:click={() => showDeleteModal = true}
            class="px-6 py-3 bg-red-900 hover:bg-red-800 text-white font-semibold transition-colors text-sm"
          >
            Delete Character
          </button>
        </div>
      </div>

      <!-- Hero Section -->
      <div class="bg-neutral-900 border border-neutral-800 p-8 mb-8">
        <div class="flex items-center gap-6 mb-6">
          <div class="w-24 h-24 bg-amber-900 border-2 border-amber-700 flex items-center justify-center text-5xl">
            🗡️
          </div>
          <div class="flex-1">
            <div class="flex items-center gap-3 mb-2">
              <h1 class="text-4xl font-bold text-white uppercase tracking-wide">{character.playerName}</h1>
              {#if isActive}
                <span class="bg-amber-700 border border-amber-600 text-black text-xs px-3 py-1 font-bold uppercase tracking-wide">Active</span>
              {/if}
            </div>
            <div class="flex items-center gap-4 text-neutral-500 text-sm">
              <span>Level <span class="text-amber-500 font-bold">{character.currentLevel}</span></span>
              <span>•</span>
              <span>{character.currentRunes.toLocaleString()} Runes</span>
              <span>•</span>
              <span>{character.playtimeHours.toFixed(1)}h played</span>
            </div>
          </div>
        </div>

        <!-- Progress Stats -->
        <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div class="bg-neutral-950 border border-neutral-800 p-4">
            <div class="flex justify-between items-center mb-2">
              <span class="text-neutral-400 text-xs uppercase tracking-wide">Bosses Defeated</span>
              <span class="text-amber-500 font-bold text-sm">{defeatedBosses.length} / {allBosses.length}</span>
            </div>
            <div class="h-2 bg-neutral-800 overflow-hidden">
              <div 
                class="h-full bg-red-600 transition-all duration-500"
                style="width: {getProgressPercentage(defeatedBosses.length, allBosses.length)}%"
              ></div>
            </div>
          </div>

          <div class="bg-neutral-950 border border-neutral-800 p-4">
            <div class="flex justify-between items-center mb-2">
              <span class="text-neutral-400 text-xs uppercase tracking-wide">Weapons Collected</span>
              <span class="text-amber-500 font-bold text-sm">{collectedWeapons.length} / {allWeapons.length}</span>
            </div>
            <div class="h-2 bg-neutral-800 overflow-hidden">
              <div 
                class="h-full bg-blue-600 transition-all duration-500"
                style="width: {getProgressPercentage(collectedWeapons.length, allWeapons.length)}%"
              ></div>
            </div>
          </div>

          <div class="bg-neutral-950 border border-neutral-800 p-4">
            <div class="flex justify-between items-center mb-2">
              <span class="text-neutral-400 text-xs uppercase tracking-wide">Total Deaths</span>
              <span class="text-purple-400 font-bold text-sm">{character.totalDeaths}</span>
            </div>
            <div class="flex items-center gap-2 text-neutral-600 text-xs">
              <span>💀</span>
              <span>Every death makes you stronger</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Collections Grid -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
        <!-- Defeated Bosses -->
        <div class="bg-neutral-900 border border-neutral-800 p-6 flex flex-col">
          <div class="flex items-center justify-between mb-4 pb-4 border-b border-neutral-800">
            <h2 class="text-xl font-bold text-white flex items-center gap-2 uppercase tracking-wide">
              <span>⚔️</span>
              <span>Defeated Bosses</span>
              <span class="text-red-400 text-base">({defeatedBosses.length})</span>
            </h2>
            <a
              href="/boss-fight"
              class="px-4 py-2 bg-red-900 hover:bg-red-800 text-white text-xs font-semibold transition-colors"
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
                class="w-full bg-black text-white px-4 py-2 border border-neutral-700 focus:border-red-700 focus:outline-none text-sm"
              />
            </div>
          {/if}
          
          {#if defeatedBosses.length === 0}
            <div class="text-center py-12 flex-1 flex flex-col items-center justify-center">
              <p class="text-neutral-700 text-4xl mb-2">👹</p>
              <p class="text-neutral-500 text-sm">No bosses defeated yet</p>
              <p class="text-neutral-700 text-xs mt-1">Start your journey and conquer the Lands Between!</p>
            </div>
          {:else if filteredDefeatedBosses.length === 0}
            <div class="text-center py-12 flex-1 flex flex-col items-center justify-center">
              <p class="text-neutral-700 text-4xl mb-2">🔍</p>
              <p class="text-neutral-500 text-sm">No bosses found</p>
              <p class="text-neutral-700 text-xs mt-1">Try a different search term</p>
            </div>
          {:else}
            <div class="space-y-2 max-h-[600px] overflow-y-auto pr-2 flex-1 custom-scrollbar">
              {#each filteredDefeatedBosses as boss}
                <div class="bg-neutral-950 border border-neutral-800 hover:border-red-800 p-4 transition-colors">
                  <div class="flex items-center gap-4">
                    {#if boss.image}
                      <img 
                        src={boss.image} 
                        alt={boss.name}
                        class="w-16 h-16 object-cover border-2 border-red-800"
                      />
                    {:else}
                      <div class="w-16 h-16 bg-neutral-900 border-2 border-red-800 flex items-center justify-center text-3xl">
                        👹
                      </div>
                    {/if}
                    <div class="flex-1 min-w-0">
                      <h3 class="text-white font-bold truncate text-sm">{boss.name}</h3>
                      <p class="text-neutral-500 text-xs">{boss.region}</p>
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
                        class="text-neutral-600 hover:text-red-500 transition-colors text-lg"
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
        <div class="bg-neutral-900 border border-neutral-800 p-6 flex flex-col">
          <div class="flex items-center justify-between mb-4 pb-4 border-b border-neutral-800">
            <h2 class="text-xl font-bold text-white flex items-center gap-2 uppercase tracking-wide">
              <span>🗡️</span>
              <span>Collected Weapons</span>
              <span class="text-blue-400 text-base">({collectedWeapons.length})</span>
            </h2>
            <a
              href="/weapons"
              class="px-4 py-2 bg-blue-900 hover:bg-blue-800 text-white text-xs font-semibold transition-colors"
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
                class="w-full bg-black text-white px-4 py-2 border border-neutral-700 focus:border-blue-700 focus:outline-none text-sm"
              />
            </div>
          {/if}
          
          {#if collectedWeapons.length === 0}
            <div class="text-center py-12 flex-1 flex flex-col items-center justify-center">
              <p class="text-neutral-700 text-4xl mb-2">⚔️</p>
              <p class="text-neutral-500 text-sm">No weapons collected yet</p>
              <p class="text-neutral-700 text-xs mt-1">Explore the world to find powerful armaments!</p>
            </div>
          {:else if filteredCollectedWeapons.length === 0}
            <div class="text-center py-12 flex-1 flex flex-col items-center justify-center">
              <p class="text-neutral-700 text-4xl mb-2">🔍</p>
              <p class="text-neutral-500 text-sm">No weapons found</p>
              <p class="text-neutral-700 text-xs mt-1">Try a different search term</p>
            </div>
          {:else}
            <div class="space-y-2 max-h-[600px] overflow-y-auto pr-2 flex-1 custom-scrollbar">
              {#each filteredCollectedWeapons as weapon}
                <div class="bg-neutral-950 border border-neutral-800 hover:border-blue-800 p-4 transition-colors">
                  <div class="flex items-center gap-4">
                    {#if weapon.image}
                      <img 
                        src={weapon.image} 
                        alt={weapon.name}
                        class="w-16 h-16 object-contain bg-black border-2 border-blue-800 p-1"
                      />
                    {:else}
                      <div class="w-16 h-16 bg-neutral-900 border-2 border-blue-800 flex items-center justify-center text-3xl">
                        ⚔️
                      </div>
                    {/if}
                    <div class="flex-1 min-w-0">
                      <h3 class="text-white font-bold truncate text-sm">{weapon.name}</h3>
                      <p class="text-neutral-500 text-xs">{weapon.category}</p>
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
                        class="text-neutral-600 hover:text-red-500 transition-colors text-lg"
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
        <div class="bg-neutral-900 border border-neutral-800 p-6 mt-8">
          <h2 class="text-xl font-bold text-white mb-4 pb-4 border-b border-neutral-800 uppercase tracking-wide flex items-center gap-2">
            <span>🗺️</span>
            <span>Visited Areas</span>
            <span class="text-amber-500 text-base">({character.visitedAreas.length})</span>
          </h2>
          <div class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-3">
            {#each character.visitedAreas as area}
              <div class="bg-neutral-950 border border-neutral-800 p-3 text-center">
                <p class="text-neutral-400 text-sm">{area}</p>
              </div>
            {/each}
          </div>
        </div>
      {/if}
    </div>
  </div>
{/if}

<!-- Delete Confirmation Modal -->
{#if showDeleteModal}
  <div class="fixed inset-0 bg-black/95 flex items-center justify-center p-4 z-50">
    <div class="bg-neutral-900 border-2 border-red-800 p-8 max-w-md w-full">
      <h2 class="text-2xl font-bold text-white mb-4 uppercase tracking-wide">Delete Character?</h2>
      <p class="text-neutral-400 mb-6">
        Are you sure you want to delete <span class="text-white font-bold">{character?.playerName}</span>? 
        This action cannot be undone. All progress will be permanently lost.
      </p>
      
    </div>
  </div>
{/if}

<style>
  .custom-scrollbar::-webkit-scrollbar {
    width: 6px;
  }
  
  .custom-scrollbar::-webkit-scrollbar-track {
    background: #171717;
    border: 1px solid #262626;
  }
  
  .custom-scrollbar::-webkit-scrollbar-thumb {
    background: #404040;
    border: 1px solid #525252;
  }
  
  .custom-scrollbar::-webkit-scrollbar-thumb:hover {
    background: #525252;
  }
</style>