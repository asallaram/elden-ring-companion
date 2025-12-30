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

  // Stats (based on selected character or all characters)
  let totalBosses = 0;
  let totalWeapons = 0;
  let totalPlaytime = 0;
  let totalDeaths = 0;

  // Selected character from store
  $: selectedCharacter = $characterStore;

  // Get 3 random weapons for recommendations
  $: if (selectedCharacter && allWeapons.length > 0) {
    const collectedWeaponIds = selectedCharacter.obtainedWeaponIds || [];
    const collectedWeapons = allWeapons.filter(w => collectedWeaponIds.includes(w.id));
    
    if (collectedWeapons.length >= 3) {
      // Shuffle and pick 3 from collected
      const shuffled = [...collectedWeapons].sort(() => 0.5 - Math.random());
      recommendedWeapons = shuffled.slice(0, 3);
    } else {
      // Pick random weapons from full catalog
      const shuffled = [...allWeapons].sort(() => 0.5 - Math.random());
      recommendedWeapons = shuffled.slice(0, 3);
    }
  }

  $: hasEnoughWeapons = selectedCharacter && (selectedCharacter.obtainedWeaponIds?.length || 0) >= 3;

  // Featured boss rotation
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

  // Running Activity Log
  let activityLog: Activity[] = [];

  $: user = $authStore.user;

  // Recalculate stats when selected character changes
  $: if (selectedCharacter) {
    totalBosses = selectedCharacter.defeatedBossIds.length;
    totalWeapons = selectedCharacter.obtainedWeaponIds.length;
    totalPlaytime = selectedCharacter.playtimeHours;
    totalDeaths = selectedCharacter.totalDeaths;
  } else if (characters.length > 0) {
    // Show aggregate stats if no character selected
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

      // If there's a selected character, update it with fresh data
      if (selectedCharacter) {
        const updated = characters.find(c => c.id === selectedCharacter.id);
        if (updated) {
          characterStore.updateData(updated);
        }
      }

      // Update activity log
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
  <!-- Animated Background -->
  <div class="absolute inset-0 opacity-30">
    <div class="absolute inset-0 bg-gradient-to-br from-amber-950 via-gray-900 to-black"></div>
    <div class="absolute top-0 left-1/4 w-96 h-96 bg-amber-600/20 rounded-full blur-3xl animate-pulse-slow"></div>
    <div class="absolute bottom-0 right-1/4 w-96 h-96 bg-orange-600/20 rounded-full blur-3xl animate-pulse-slow" style="animation-delay: 2s;"></div>
  </div>

  <!-- Header -->
  <header class="relative bg-gradient-to-r from-gray-900/80 via-amber-900/40 to-gray-900/80 backdrop-blur-md border-b border-amber-600/30 shadow-2xl z-10">
    <div class="max-w-[1800px] mx-auto px-6 py-6 flex justify-between items-center">
      <div class="flex items-center gap-4">
        <div class="w-12 h-12 bg-gradient-to-br from-amber-500 to-orange-600 rounded-lg flex items-center justify-center shadow-lg">
          <span class="text-2xl">⚔️</span>
        </div>
        <div>
          <h1 class="text-3xl font-bold bg-gradient-to-r from-amber-400 via-orange-400 to-amber-500 bg-clip-text text-transparent">Elden Ring Simulator</h1>
          <p class="text-gray-400 text-sm flex items-center gap-2">
            <span class="w-2 h-2 bg-green-500 rounded-full animate-pulse-slow"></span>
            {#if selectedCharacter}
              Playing as <span class="text-amber-400 font-bold">{selectedCharacter.playerName}</span> (Lvl {selectedCharacter.currentLevel})
            {:else}
              Welcome back, {user?.username || 'Tarnished'}
            {/if}
          </p>
        </div>
      </div>
      <button on:click={handleLogout} class="px-6 py-2.5 bg-gradient-to-r from-red-600 to-red-700 hover:from-red-700 hover:to-red-800 text-white rounded-lg transition-all shadow-lg hover:shadow-red-500/50 font-medium">Logout</button>
    </div>
  </header>

  <main class="relative max-w-[1800px] mx-auto px-6 py-8 z-10">
    <!-- Top Stats Banner -->
    <div class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
      <div class="bg-gradient-to-br from-red-900/40 to-gray-800/40 border-2 border-red-700/50 rounded-2xl p-6 backdrop-blur-sm hover:scale-105 transition-all">
        <div class="flex items-center justify-between mb-2">
          <span class="text-red-400 text-sm font-semibold uppercase tracking-wide">Bosses Defeated</span>
          <span class="text-3xl">⚔️</span>
        </div>
        <div class="text-4xl font-bold text-white">{totalBosses}</div>
      </div>
      <div class="bg-gradient-to-br from-blue-900/40 to-gray-800/40 border-2 border-blue-700/50 rounded-2xl p-6 backdrop-blur-sm hover:scale-105 transition-all">
        <div class="flex items-center justify-between mb-2">
          <span class="text-blue-400 text-sm font-semibold uppercase tracking-wide">Weapons Collected</span>
          <span class="text-3xl">🗡️</span>
        </div>
        <div class="text-4xl font-bold text-white">{totalWeapons}</div>
      </div>
      <div class="bg-gradient-to-br from-amber-900/40 to-gray-800/40 border-2 border-amber-700/50 rounded-2xl p-6 backdrop-blur-sm hover:scale-105 transition-all">
        <div class="flex items-center justify-between mb-2">
          <span class="text-amber-400 text-sm font-semibold uppercase tracking-wide">Total Playtime</span>
          <span class="text-3xl">⏱️</span>
        </div>
        <div class="text-4xl font-bold text-white">{totalPlaytime.toFixed(1)}h</div>
      </div>
      <div class="bg-gradient-to-br from-purple-900/40 to-gray-800/40 border-2 border-purple-700/50 rounded-2xl p-6 backdrop-blur-sm hover:scale-105 transition-all">
        <div class="flex items-center justify-between mb-2">
          <span class="text-purple-400 text-sm font-semibold uppercase tracking-wide">Total Deaths</span>
          <span class="text-3xl">💀</span>
        </div>
        <div class="text-4xl font-bold text-white">{totalDeaths}</div>
      </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-12 gap-8">
      <!-- Characters -->
      <div class="lg:col-span-4">
        <div class="bg-gradient-to-br from-gray-900/80 to-gray-800/80 border-2 border-amber-700/40 rounded-2xl p-6 backdrop-blur-sm">
          <div class="flex justify-between items-center mb-6">
            <h2 class="text-2xl font-bold text-white">Your Characters</h2>
            <button on:click={() => showCreateModal = true} class="p-2 bg-amber-600/80 hover:bg-amber-600 rounded-lg transition-all" title="Create New Character"><span class="text-2xl">+</span></button>
          </div>
          {#if isLoading}
            <div class="text-center py-8 text-gray-400">Loading...</div>
          {:else if characters.length === 0}
            <div class="text-center py-12">
              <div class="text-6xl mb-4">⚔️</div>
              <p class="text-gray-400 mb-4">No characters yet</p>
              <button on:click={() => showCreateModal = true} class="px-6 py-3 bg-gradient-to-r from-amber-600 to-orange-600 hover:from-amber-500 hover:to-orange-500 text-white font-bold rounded-xl transition-all">Create First Character</button>
            </div>
          {:else}
            <div class="space-y-4 max-h-[600px] overflow-y-auto pr-2">
              {#each characters as character}
                <div class="bg-gray-800/50 border-2 rounded-xl p-4 transition-all {selectedCharacter?.id === character.id ? 'border-amber-500 bg-amber-900/10' : 'border-gray-700'}">
                  <button
                    on:click={() => selectCharacter(character)}
                    class="w-full text-left mb-3"
                  >
                    <div class="flex items-center gap-4">
                      <div class="w-16 h-16 bg-gradient-to-br from-amber-500 to-orange-600 rounded-lg flex items-center justify-center text-3xl flex-shrink-0">
                        {selectedCharacter?.id === character.id ? '✓' : '🗡️'}
                      </div>
                      <div class="flex-1 min-w-0">
                        <div class="flex items-center gap-2 mb-1">
                          <h3 class="text-lg font-bold text-white truncate">{character.playerName}</h3>
                          {#if selectedCharacter?.id === character.id}
                            <span class="bg-amber-500 text-black text-xs px-2 py-0.5 rounded-full font-bold">ACTIVE</span>
                          {/if}
                        </div>
                        <div class="flex items-center gap-3 text-sm text-gray-400">
                          <span>Lvl {character.currentLevel}</span>
                          <span>•</span>
                          <span>{character.defeatedBossIds.length} bosses</span>
                        </div>
                      </div>
                    </div>
                  </button>
                  
                  <a 
                    href="/character/{character.id}"
                    class="w-full bg-amber-500/80 hover:bg-amber-500 text-black font-bold py-2.5 px-4 rounded-lg transition-colors text-center block text-sm"
                  >
                    View Profile →
                  </a>
                </div>
              {/each}
            </div>
          {/if}
        </div>
      </div>

      <!-- Activity Log & Quick Actions -->
      <div class="lg:col-span-5 space-y-6">
        <div class="bg-gradient-to-br from-gray-900/80 to-gray-800/80 border-2 border-amber-700/40 rounded-2xl p-6 backdrop-blur-sm">
          <h2 class="text-2xl font-bold text-white mb-6">Recent Activity</h2>
          <div class="space-y-4 max-h-[600px] overflow-y-auto">
            {#if activityLog.length > 0}
              {#each activityLog as act}
                <div class="flex items-start gap-4 p-4 bg-gray-800/50 rounded-xl border border-gray-700 hover:border-amber-700/50 transition-all">
                  <div class="text-3xl flex-shrink-0">{act.icon}</div>
                  <div class="flex-1">
                    <p class="text-white font-semibold">{act.text}</p>
                    <p class="text-gray-400 text-sm">{act.time}</p>
                  </div>
                </div>
              {/each}
            {:else}
              <div class="text-center py-12 text-gray-500">
                <p class="text-4xl mb-2">📜</p>
                <p>No activity yet. Start your journey!</p>
              </div>
            {/if}
          </div>
        </div>
        <div class="grid grid-cols-3 gap-4">
          <button on:click={() => showCreateModal = true} class="p-6 bg-gradient-to-br from-amber-900/40 to-gray-800/40 border-2 border-amber-700/50 rounded-2xl hover:scale-105 transition-all backdrop-blur-sm text-left">
            <div class="text-4xl mb-2">➕</div>
            <div class="text-white font-bold">Create Character</div>
            <div class="text-gray-400 text-sm">Start a new journey</div>
          </button>
          <a href="/bosses" class="p-6 bg-gradient-to-br from-red-900/40 to-gray-800/40 border-2 border-red-700/50 rounded-2xl hover:scale-105 transition-all backdrop-blur-sm text-left">
            <div class="text-4xl mb-2">⚔️</div>
            <div class="text-white font-bold">Browse Bosses</div>
            <div class="text-gray-400 text-sm">View all bosses</div>
          </a>
          <a href="/weapons" class="p-6 bg-gradient-to-br from-blue-900/40 to-gray-800/40 border-2 border-blue-700/50 rounded-2xl hover:scale-105 transition-all backdrop-blur-sm text-left">
            <div class="text-4xl mb-2">🗡️</div>
            <div class="text-white font-bold">Browse Weapons</div>
            <div class="text-gray-400 text-sm">View all weapons</div>
          </a>
        </div>
      </div>

      <!-- Weapon Recommendations & Featured Boss -->
      <div class="lg:col-span-3 space-y-6">
        <!-- Recommended Weapons -->
        {#if selectedCharacter}
          <div class="bg-gradient-to-br from-gray-900/80 to-gray-800/80 border-2 border-blue-700/40 rounded-2xl p-4 backdrop-blur-sm">
            <h3 class="text-lg font-bold text-white mb-3">
              {recommendedWeapons.length > 0 && hasEnoughWeapons ? '🗡️ Your Arsenal' : '🔒 Unlock These Weapons'}
            </h3>
            {#if recommendedWeapons.length === 0}
              <!-- Loading skeleton -->
              <div class="grid grid-cols-3 gap-2">
                {#each [1, 2, 3] as _}
                  <div class="bg-gray-800/50 border border-gray-700 rounded-lg p-2 animate-pulse-fast">
                    <div class="w-full h-16 bg-gray-700 rounded mb-1"></div>
                    <div class="h-3 bg-gray-700 rounded mt-1"></div>
                  </div>
                {/each}
              </div>
            {:else}
              <div class="grid grid-cols-3 gap-2">
                {#each recommendedWeapons as weapon}
                  <a 
                    href="/weapons/{weapon.id}"
                    class="bg-gray-800/50 border border-gray-700 hover:border-blue-500 rounded-lg p-2 transition-all group"
                    title={weapon.name}
                  >
                    {#if weapon.image}
                      <img 
                        src={weapon.image} 
                        alt={weapon.name}
                        class="w-full h-16 object-contain bg-gray-900 rounded mb-1"
                      />
                    {:else}
                      <div class="w-full h-16 bg-gray-700 rounded flex items-center justify-center text-2xl mb-1">
                        ⚔️
                      </div>
                    {/if}
                    <p class="text-white text-xs font-medium truncate text-center">{weapon.name}</p>
                  </a>
                {/each}
              </div>
              {#if !hasEnoughWeapons}
                <p class="text-gray-400 text-xs mt-2 text-center">Collect weapons to build your arsenal!</p>
              {/if}
            {/if}
          </div>
        {/if}

        <!-- Featured Boss -->
        <div class="bg-gradient-to-br from-gray-900/80 to-gray-800/80 border-2 border-amber-700/40 rounded-2xl overflow-hidden backdrop-blur-sm">
          <div class="relative h-64 overflow-hidden">
            <img src={featuredBoss.image} alt={featuredBoss.name} class="w-full h-full object-cover transition-transform duration-500 hover:scale-110"
              on:error={(e) => { const img = e.currentTarget as HTMLImageElement; img.src = 'https://via.placeholder.com/400x300?text=Boss+Image'; }} />
            <div class="absolute inset-0 bg-gradient-to-t from-black via-black/50 to-transparent"></div>
            <div class="absolute bottom-0 left-0 right-0 p-4">
              <div class="inline-block px-3 py-1 bg-red-600/80 rounded-full text-white text-xs font-bold mb-2">{featuredBoss.difficulty}</div>
              <h3 class="text-xl font-bold text-white">{featuredBoss.name}</h3>
            </div>
          </div>
          <div class="p-4">
            <p class="text-gray-300 text-sm mb-3">💡 <strong>Tip:</strong> {featuredBoss.tip}</p>
            <a href="/tips" class="block w-full py-2 bg-amber-600/80 hover:bg-amber-600 text-white font-semibold rounded-lg transition-all text-center">View Strategy</a>
          </div>
        </div>
      </div>
    </div>
  </main>
</div>

{#if showCreateModal}
  <div class="fixed inset-0 bg-black/90 backdrop-blur-sm flex items-center justify-center p-4 z-50">
    <div class="bg-gradient-to-br from-gray-900 to-gray-800 rounded-2xl border-2 border-amber-700/50 p-10 max-w-lg w-full shadow-2xl">
      <h3 class="text-3xl font-bold bg-gradient-to-r from-amber-400 to-orange-400 bg-clip-text text-transparent mb-8">Create New Character</h3>
      <form on:submit|preventDefault={createCharacter} class="space-y-6">
        <div>
          <label for="characterName" class="block text-sm font-semibold text-gray-300 mb-3">Character Name</label>
          <input id="characterName" type="text" bind:value={newCharacterName} required placeholder="Enter your Tarnished's name" class="w-full px-5 py-4 bg-gray-800/50 border-2 border-gray-700 rounded-xl text-white placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-amber-500 focus:border-transparent transition-all"/>
        </div>
        <div>
          <label for="startingLevel" class="block text-sm font-semibold text-gray-300 mb-3">Starting Level</label>
          <input id="startingLevel" type="number" bind:value={startingLevel} min="1" max="713" class="w-full px-5 py-4 bg-gray-800/50 border-2 border-gray-700 rounded-xl text-white focus:outline-none focus:ring-2 focus:ring-amber-500 focus:border-transparent transition-all"/>
        </div>
        {#if error}
          <div class="bg-red-950/50 border-2 border-red-600/50 text-red-300 px-5 py-4 rounded-xl text-sm">{error}</div>
        {/if}
        <div class="flex gap-4 pt-4">
          <button type="button" on:click={() => { showCreateModal = false; error = ''; }} class="flex-1 py-4 bg-gray-700/50 hover:bg-gray-700 text-white font-semibold rounded-xl transition-all border-2 border-gray-600">Cancel</button>
          <button type="submit" disabled={isCreating} class="flex-1 py-4 bg-gradient-to-r from-amber-600 to-orange-600 hover:from-amber-500 hover:to-orange-500 text-white font-bold rounded-xl transition-all shadow-lg hover:shadow-amber-500/50 disabled:opacity-50">{isCreating ? 'Creating...' : 'Create Character'}</button>
        </div>
      </form>
    </div>
  </div>
{/if}

<style>
  @keyframes pulse-fast {
    0%, 100% { opacity: 1; }
    50% { opacity: 0.5; }
  }
  
  @keyframes pulse-slow {
    0%, 100% { opacity: 1; }
    50% { opacity: 0.8; }
  }
  
  .animate-pulse-fast {
    animation: pulse-fast 2s cubic-bezier(0.4, 0, 0.6, 1) infinite;
  }
  
  .animate-pulse-slow {
    animation: pulse-slow 3s cubic-bezier(0.4, 0, 0.6, 1) infinite;
  }
</style>