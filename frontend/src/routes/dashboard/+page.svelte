<script lang="ts">
  import { onMount } from 'svelte';
  import { authStore } from '$lib/stores/auth';
  import { characterStore } from '$lib/stores/character';
  import type { Character } from '$lib/stores/character';
  import { api } from '$lib/api';
  import { goto } from '$app/navigation';

  interface Activity {
    type: string;
    text: string;
    time: string;
    icon: string;
  }

  interface Weapon {
    id: string;
    name: string;
    image: string;
    category: string;
  }

  let characters: Character[] = [];
  let isLoading = true;
  let error = '';
  let showCreateModal = false;
  let newCharacterName = '';
  let startingLevel = 10;
  let isCreating = false;
  let allWeapons: Weapon[] = [];
  let recommendedWeapons: Weapon[] = [];

  let totalBosses = 0;
  let totalWeapons = 0;
  let totalPlaytime = 0;
  let totalDeaths = 0;

  $: selectedCharacter = $characterStore;

  $: if (selectedCharacter && allWeapons.length > 0) {
    const collectedWeaponIds = selectedCharacter.obtainedWeaponIds || [];
    const collectedWeapons = allWeapons.filter(w => collectedWeaponIds.includes(w.id));
    
    if (collectedWeapons.length >= 3) {
      const shuffled = [...collectedWeapons].sort(() => 0.5 - Math.random());
      recommendedWeapons = shuffled.slice(0, 3);
    } else {
      const shuffled = [...allWeapons].sort(() => 0.5 - Math.random());
      recommendedWeapons = shuffled.slice(0, 3);
    }
  }

  $: hasEnoughWeapons = selectedCharacter && (selectedCharacter.obtainedWeaponIds?.length || 0) >= 3;

  const featuredBosses = [
    {
      name: 'Margit, the Fell Omen',
      image: 'https://eldenring.wiki.fextralife.com/file/Elden-Ring/margit_the_fell_omen_bosses_elden_ring_wiki_600px.jpg',
      tip: 'Use Bloodhound\'s Fang for bleed damage',
      difficulty: 'Medium'
    },
    {
      name: 'Godrick the Grafted',
      image: 'https://eldenring.wiki.fextralife.com/file/Elden-Ring/godrick_the_grafted_bosses_elden_ring_wiki_600px.jpg',
      tip: 'Fire damage is highly effective',
      difficulty: 'Medium'
    },
    {
      name: 'Starscourge Radahn',
      image: 'https://eldenring.wiki.fextralife.com/file/Elden-Ring/starscourge_radahn_bosses_elden_ring_wiki_600px.jpg',
      tip: 'Summon allies to help in this epic fight',
      difficulty: 'Hard'
    },
    {
      name: 'Malenia, Blade of Miquella',
      image: 'https://eldenring.wiki.fextralife.com/file/Elden-Ring/malenia_blade_of_miquella_bosses_elden_ring_wiki_600px.jpg',
      tip: 'Master dodging - she heals on hit',
      difficulty: 'Very Hard'
    },
  ];

  let currentFeaturedIndex = 0;
  $: featuredBoss = featuredBosses[currentFeaturedIndex];

  let activityLog: Activity[] = [];

  $: user = $authStore.user;

  $: if (selectedCharacter) {
    totalBosses = selectedCharacter.defeatedBossIds.length;
    totalWeapons = selectedCharacter.obtainedWeaponIds.length;
    totalPlaytime = selectedCharacter.playtimeHours;
    totalDeaths = selectedCharacter.totalDeaths;
  } else if (characters.length > 0) {
    totalBosses = characters.reduce((sum, c) => sum + c.defeatedBossIds.length, 0);
    totalWeapons = characters.reduce((sum, c) => sum + c.obtainedWeaponIds.length, 0);
    totalPlaytime = characters.reduce((sum, c) => sum + c.playtimeHours, 0);
    totalDeaths = characters.reduce((sum, c) => sum + c.totalDeaths, 0);
  }

  onMount(() => {
    if ($authStore.isLoading) {
      const unsubscribe = authStore.subscribe(state => {
        if (!state.isLoading) {
          unsubscribe();
          if (state.isAuthenticated) {
            loadData();
          } else {
            goto('/login');
          }
        }
      });
    } else if ($authStore.isAuthenticated) {
      loadData();
    } else {
      goto('/login');
    }

    const interval = setInterval(() => {
      currentFeaturedIndex = (currentFeaturedIndex + 1) % featuredBosses.length;
    }, 5000);

    return () => clearInterval(interval);
  });

  async function loadData() {
    await Promise.all([loadCharacters(), loadWeapons()]);
  }

  async function loadCharacters() {
    isLoading = true;
    error = '';
    
    try {
      characters = await api.playerProgress.getMyCharacters();

      if (selectedCharacter) {
        const updated = characters.find(c => c.id === selectedCharacter.id);
        if (updated) {
          characterStore.updateData(updated);
        }
      }

      activityLog = [];
      characters.forEach(c => {
        c.recentBossAttempts.forEach(boss => activityLog.push({
          type: 'boss',
          text: `Attempted ${boss} with ${c.playerName}`,
          time: new Date().toLocaleTimeString(),
          icon: '⚔️'
        }));
        c.visitedAreas.forEach(area => activityLog.push({
          type: 'area',
          text: `${c.playerName} visited ${area}`,
          time: new Date().toLocaleTimeString(),
          icon: '🗺️'
        }));
      });
      activityLog = activityLog.slice(-50).reverse();

    } catch (err) {
      error = err instanceof Error ? err.message : 'Failed to load characters';
    } finally {
      isLoading = false;
    }
  }

  async function loadWeapons() {
    try {
      allWeapons = await api.weapons.getAll();
    } catch (err) {
      console.error('Failed to load weapons:', err);
    }
  }

  async function createCharacter() {
    if (!newCharacterName.trim()) return;

    isCreating = true;
    error = '';

    try {
      await api.playerProgress.createCharacter(newCharacterName, user?.psnId, startingLevel);
      await loadCharacters();
      showCreateModal = false;
      newCharacterName = '';
      startingLevel = 10;
    } catch (err) {
      error = err instanceof Error ? err.message : 'Failed to create character';
    } finally {
      isCreating = false;
    }
  }

  function selectCharacter(character: Character) {
    characterStore.select(character);
  }

  function handleLogout() {
    characterStore.clear();
    authStore.logout();
    goto('/login');
  }
</script>

<svelte:head>
  <title>Dashboard - Elden Ring Simulator</title>
</svelte:head>

<div class="min-h-screen bg-black relative overflow-hidden">
  <!-- Dark background like wiki -->
  <div class="absolute inset-0 bg-[#0a0a0a]"></div>

  <!-- Header -->
  <header class="relative border-b border-neutral-800 bg-black/90">
    <div class="max-w-[1800px] mx-auto px-6 py-4">
      <div class="flex justify-between items-center">
        <div class="flex items-center gap-4">
          <div class="w-12 h-12 bg-amber-700 flex items-center justify-center">
            <span class="text-2xl">⚔️</span>
          </div>
          <div>
            <h1 class="text-2xl font-bold tracking-wide mb-0.5" style="color: #d4a25a;">
              Elden Ring
            </h1>
            <div class="flex items-center gap-2 text-xs">
              <span class="w-1.5 h-1.5 bg-green-500 rounded-full"></span>
              {#if selectedCharacter}
                <span class="text-neutral-400">{selectedCharacter.playerName}</span>
                <span class="text-neutral-600">·</span>
                <span class="text-amber-500">Level {selectedCharacter.currentLevel}</span>
              {:else}
                <span class="text-neutral-400">{user?.username || 'Tarnished'}</span>
              {/if}
            </div>
          </div>
        </div>
        <button 
          on:click={handleLogout} 
          class="px-5 py-2 bg-red-900 hover:bg-red-800 text-white font-semibold text-sm transition-colors"
        >
          Logout
        </button>
      </div>
    </div>
  </header>

  <main class="relative max-w-[1800px] mx-auto px-8 py-8 z-10">
    <!-- Stats Grid -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
      <!-- Bosses -->
      <div class="bg-neutral-900 border border-neutral-800 hover:border-red-900 transition-colors p-5">
        <div class="flex items-start justify-between mb-3">
          <div>
            <p class="text-[10px] font-semibold text-red-500 uppercase tracking-wider mb-1.5">Bosses</p>
            <h3 class="text-4xl font-bold text-white">{totalBosses}</h3>
          </div>
          <div class="w-10 h-10 bg-red-950/50 border border-red-900 flex items-center justify-center text-xl">
            ⚔️
          </div>
        </div>
      </div>

      <!-- Weapons -->
      <div class="bg-neutral-900 border border-neutral-800 hover:border-blue-900 transition-colors p-5">
        <div class="flex items-start justify-between mb-3">
          <div>
            <p class="text-[10px] font-semibold text-blue-500 uppercase tracking-wider mb-1.5">Weapons</p>
            <h3 class="text-4xl font-bold text-white">{totalWeapons}</h3>
          </div>
          <div class="w-10 h-10 bg-blue-950/50 border border-blue-900 flex items-center justify-center text-xl">
            🗡️
          </div>
        </div>
      </div>

      <!-- Playtime -->
      <div class="bg-neutral-900 border border-neutral-800 hover:border-amber-900 transition-colors p-5">
        <div class="flex items-start justify-between mb-3">
          <div>
            <p class="text-[10px] font-semibold uppercase tracking-wider mb-1.5" style="color: #d4a25a;">Playtime</p>
            <h3 class="text-4xl font-bold text-white">{totalPlaytime.toFixed(1)}<span class="text-xl text-neutral-500 ml-1">h</span></h3>
          </div>
          <div class="w-10 h-10 border border-amber-900 flex items-center justify-center text-xl" style="background-color: rgba(212, 162, 90, 0.1);">
            ⏱️
          </div>
        </div>
      </div>

      <!-- Deaths -->
      <div class="bg-neutral-900 border border-neutral-800 hover:border-purple-900 transition-colors p-5">
        <div class="flex items-start justify-between mb-3">
          <div>
            <p class="text-[10px] font-semibold text-purple-500 uppercase tracking-wider mb-1.5">Deaths</p>
            <h3 class="text-4xl font-bold text-white">{totalDeaths}</h3>
          </div>
          <div class="w-10 h-10 bg-purple-950/50 border border-purple-900 flex items-center justify-center text-xl">
            💀
          </div>
        </div>
      </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-12 gap-8">
      <!-- Characters -->
      <div class="lg:col-span-4">
        <div class="bg-neutral-900 border border-neutral-800 p-6">
          <div class="flex justify-between items-center mb-6 pb-4 border-b border-neutral-800">
            <h2 class="text-xl font-bold text-white uppercase tracking-wide">Characters</h2>
            <button 
              on:click={() => showCreateModal = true} 
              class="w-8 h-8 bg-amber-900 hover:bg-amber-800 border border-amber-700 transition-colors flex items-center justify-center text-amber-300 font-bold text-lg"
              title="Create New Character"
            >
              +
            </button>
          </div>

          {#if isLoading}
            <div class="text-center py-16">
              <div class="inline-block w-10 h-10 border-2 border-neutral-700 border-t-amber-600 animate-spin"></div>
              <p class="text-neutral-600 mt-3 text-sm">Loading...</p>
            </div>
          {:else if characters.length === 0}
            <div class="text-center py-16 px-4">
              <div class="w-16 h-16 mx-auto mb-4 bg-neutral-800 border border-neutral-700 flex items-center justify-center text-3xl">
                ⚔️
              </div>
              <h3 class="text-lg font-bold text-white mb-2">No Characters</h3>
              <p class="text-neutral-500 text-sm mb-6">Create your first character</p>
              <button 
                on:click={() => showCreateModal = true} 
                class="px-6 py-3 bg-amber-900 hover:bg-amber-800 text-white font-semibold text-sm transition-colors uppercase tracking-wide"
              >
                Create Character
              </button>
            </div>
          {:else}
            <div class="space-y-3 max-h-[600px] overflow-y-auto pr-2 custom-scrollbar">
              {#each characters as character}
                <div class="bg-neutral-950 border transition-colors {selectedCharacter?.id === character.id ? 'border-amber-700' : 'border-neutral-800 hover:border-neutral-700'} p-4">
                  <button
                    on:click={() => selectCharacter(character)}
                    class="w-full text-left mb-3"
                  >
                    <div class="flex items-center gap-3">
                      <div class="relative flex-shrink-0">
                        <div class="w-14 h-14 bg-amber-900 border border-amber-700 flex items-center justify-center text-xl">
                          {selectedCharacter?.id === character.id ? '✓' : '🗡️'}
                        </div>
                      </div>
                      <div class="flex-1 min-w-0">
                        <div class="flex items-center gap-2 mb-1">
                          <h3 class="text-base font-bold text-white truncate">{character.playerName}</h3>
                          {#if selectedCharacter?.id === character.id}
                            <span class="bg-amber-700 text-black text-[9px] px-1.5 py-0.5 font-bold uppercase">Active</span>
                          {/if}
                        </div>
                        <div class="flex items-center gap-2 text-xs">
                          <span class="text-amber-500 font-semibold">Lvl {character.currentLevel}</span>
                          <span class="text-neutral-700">·</span>
                          <span class="text-neutral-400">{character.defeatedBossIds.length} bosses</span>
                        </div>
                      </div>
                    </div>
                  </button>
                  
                  <a 
                    href="/character/{character.id}"
                    class="w-full bg-amber-900 hover:bg-amber-800 text-white font-semibold py-2.5 px-4 transition-colors text-center block text-sm"
                  >
                    View Profile →
                  </a>
                </div>
              {/each}
            </div>
          {/if}
        </div>
      </div>

      <!-- Activity -->
      <div class="lg:col-span-5 space-y-5">
        <div class="bg-neutral-900 border border-neutral-800 p-6">
          <h2 class="text-xl font-bold text-white mb-6 pb-4 border-b border-neutral-800 uppercase tracking-wide">Recent Activity</h2>
          <div class="space-y-2 max-h-[450px] overflow-y-auto pr-2 custom-scrollbar">
            {#if activityLog.length > 0}
              {#each activityLog as act}
                <div class="bg-neutral-950 border border-neutral-800 hover:border-neutral-700 p-3 transition-colors">
                  <div class="flex items-start gap-3">
                    <div class="w-8 h-8 bg-neutral-800 border border-neutral-700 flex items-center justify-center text-base flex-shrink-0">
                      {act.icon}
                    </div>
                    <div class="flex-1 min-w-0">
                      <p class="text-neutral-300 text-sm">{act.text}</p>
                      <p class="text-neutral-600 text-xs mt-0.5">{act.time}</p>
                    </div>
                  </div>
                </div>
              {/each}
            {:else}
              <div class="text-center py-16">
                <div class="w-16 h-16 mx-auto mb-3 bg-neutral-800 border border-neutral-700 flex items-center justify-center text-3xl">
                  📜
                </div>
                <p class="text-neutral-500 text-sm">No activity</p>
              </div>
            {/if}
          </div>
        </div>

        <!-- Quick Actions -->
        <div class="grid grid-cols-3 gap-3">
          <button 
            on:click={() => showCreateModal = true} 
            class="bg-neutral-900 border border-neutral-800 hover:border-amber-800 p-5 transition-colors text-left"
          >
            <div class="text-3xl mb-2">➕</div>
            <div class="text-white font-semibold text-sm mb-0.5">New</div>
            <div class="text-neutral-600 text-xs">Character</div>
          </button>

          <a 
            href="/bosses" 
            class="bg-neutral-900 border border-neutral-800 hover:border-red-800 p-5 transition-colors text-left"
          >
            <div class="text-3xl mb-2">⚔️</div>
            <div class="text-white font-semibold text-sm mb-0.5">Bosses</div>
            <div class="text-neutral-600 text-xs">View All</div>
          </a>

          <a 
            href="/weapons" 
            class="bg-neutral-900 border border-neutral-800 hover:border-blue-800 p-5 transition-colors text-left"
          >
            <div class="text-3xl mb-2">🗡️</div>
            <div class="text-white font-semibold text-sm mb-0.5">Weapons</div>
            <div class="text-neutral-600 text-xs">Browse</div>
          </a>
        </div>
      </div>

      <!-- Sidebar -->
      <div class="lg:col-span-3 space-y-5">
        <!-- Weapons -->
        {#if selectedCharacter}
          <div class="bg-neutral-900 border border-neutral-800 p-5">
            <h3 class="text-base font-bold text-white mb-4 pb-3 border-b border-neutral-800 uppercase tracking-wide flex items-center gap-2">
              <span class="text-lg">{hasEnoughWeapons ? '🗡️' : '🔒'}</span>
              {hasEnoughWeapons ? 'Arsenal' : 'Locked'}
            </h3>
            
            {#if recommendedWeapons.length === 0}
              <div class="grid grid-cols-3 gap-2">
                {#each [1, 2, 3] as _}
                  <div class="bg-neutral-950 border border-neutral-800 p-2 animate-pulse">
                    <div class="w-full h-14 bg-neutral-800 mb-1"></div>
                    <div class="h-2 bg-neutral-800"></div>
                  </div>
                {/each}
              </div>
            {:else}
              <div class="grid grid-cols-3 gap-2">
                {#each recommendedWeapons as weapon}
                  <a 
                    href="/weapons/{weapon.id}"
                    class="bg-neutral-950 border border-neutral-800 hover:border-blue-700 p-2 transition-colors"
                    title={weapon.name}
                  >
                    {#if weapon.image}
                      <div class="relative overflow-hidden mb-1 bg-black border border-neutral-900">
                        <img 
                          src={weapon.image} 
                          alt={weapon.name}
                          class="w-full h-14 object-contain"
                        />
                      </div>
                    {:else}
                      <div class="w-full h-14 bg-neutral-800 border border-neutral-700 flex items-center justify-center text-xl mb-1">
                        ⚔️
                      </div>
                    {/if}
                    <p class="text-neutral-400 text-[10px] font-semibold truncate text-center">{weapon.name}</p>
                  </a>
                {/each}
              </div>
              {#if !hasEnoughWeapons}
                <p class="text-neutral-600 text-xs mt-2 text-center">Collect 3+ to unlock</p>
              {/if}
            {/if}
          </div>
        {/if}

        <!-- Featured Boss -->
        <div class="overflow-hidden bg-neutral-900 border border-neutral-800">
          <div class="relative h-64 overflow-hidden border-b border-neutral-800">
            <img 
              src={featuredBoss.image} 
              alt={featuredBoss.name} 
              class="w-full h-full object-cover"
              on:error={(e) => { const img = e.currentTarget as HTMLImageElement; img.src = 'https://via.placeholder.com/400x300?text=Boss+Image'; }} 
            />
            <div class="absolute inset-0 bg-gradient-to-t from-black via-black/70 to-transparent"></div>
            <div class="absolute top-3 right-3">
              <span class="inline-block px-2.5 py-1 bg-red-900 border border-red-800 text-red-100 text-[10px] font-bold uppercase tracking-wide">
                {featuredBoss.difficulty}
              </span>
            </div>
            <div class="absolute bottom-0 left-0 right-0 p-4">
              <h3 class="text-lg font-bold text-white">{featuredBoss.name}</h3>
            </div>
          </div>
          <div class="p-5">
            <div class="bg-neutral-950 border border-neutral-800 p-3 mb-4">
              <p class="text-amber-600 text-[10px] font-bold uppercase tracking-wide mb-1.5">Tip</p>
              <p class="text-neutral-400 text-xs leading-relaxed">{featuredBoss.tip}</p>
            </div>
            <a 
              href="/tips" 
              class="block w-full py-2.5 bg-amber-900 hover:bg-amber-800 text-white font-semibold transition-colors text-center text-sm"
            >
              Learn More →
            </a>
          </div>
        </div>
      </div>
    </div>
  </main>
</div>

<!-- Modal -->
{#if showCreateModal}
  <div class="fixed inset-0 bg-black/95 flex items-center justify-center p-4 z-50">
    <div class="bg-neutral-900 border-2 border-neutral-700 p-8 max-w-md w-full">
      <h3 class="text-2xl font-bold mb-1 text-white uppercase tracking-wide">Create Character</h3>
      <p class="text-neutral-500 text-sm mb-8">Begin your journey</p>
      
      <form on:submit|preventDefault={createCharacter} class="space-y-6">
        <div>
          <label for="characterName" class="block text-xs font-bold text-neutral-400 mb-2 uppercase tracking-wide">Name</label>
          <input 
            id="characterName" 
            type="text" 
            bind:value={newCharacterName} 
            required 
            placeholder="Enter name" 
            class="w-full px-4 py-3 bg-black border border-neutral-700 text-white placeholder-neutral-600 focus:outline-none focus:border-amber-700 transition-colors"
          />
        </div>
        
        <div>
          <label for="startingLevel" class="block text-xs font-bold text-neutral-400 mb-2 uppercase tracking-wide">Level</label>
          <input 
            id="startingLevel" 
            type="number" 
            bind:value={startingLevel} 
            min="1" 
            max="713" 
            class="w-full px-4 py-3 bg-black border border-neutral-700 text-white focus:outline-none focus:border-amber-700 transition-colors"
          />
        </div>
        
        {#if error}
          <div class="bg-red-950/50 border border-red-800 text-red-300 px-4 py-3 text-sm">
            {error}
          </div>
        {/if}
        
        <div class="flex gap-3 pt-4">
          <button 
            type="button" 
            on:click={() => { showCreateModal = false; error = ''; }} 
            class="flex-1 py-3 bg-neutral-800 hover:bg-neutral-700 text-neutral-300 font-semibold transition-colors"
          >
            Cancel
          </button>
          <button 
            type="submit" 
            disabled={isCreating} 
            class="flex-1 py-3 bg-amber-900 hover:bg-amber-800 text-white font-semibold transition-colors disabled:opacity-50"
          >
            {isCreating ? 'Creating...' : 'Create'}
          </button>
        </div>
      </form>
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