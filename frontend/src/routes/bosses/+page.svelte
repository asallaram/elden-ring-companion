<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api';
  import { authStore } from '$lib/stores/auth';
  import { characterStore } from '$lib/stores/character';
  import { goto } from '$app/navigation';
  
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
  
  let bosses: Boss[] = [];
  let isLoading = true;
  let error = '';
  let defeatedBossIds: string[] = [];
  
  $: user = $authStore.user;
  $: selectedCharacter = $characterStore;

  // Update defeated bosses when character changes
  $: if (selectedCharacter) {
    defeatedBossIds = selectedCharacter.defeatedBossIds || [];
  }

  // Check if boss is defeated
  function isDefeated(bossId: string): boolean {
    return defeatedBossIds.includes(bossId);
  }

  onMount(() => {
    if ($authStore.isLoading) {
      const unsubscribe = authStore.subscribe(state => {
        if (!state.isLoading) {
          unsubscribe();
          if (state.isAuthenticated) {
            loadBosses();
          } else {
            goto('/login');
          }
        }
      });
    } else if ($authStore.isAuthenticated) {
      loadBosses();
    } else {
      goto('/login');
    }
  });

  async function loadBosses() {
    try { 
      console.log('Fetching bosses...');
      bosses = await api.bosses.getAll();
      console.log('Bosses loaded:', bosses);
    } 
    catch (err) { 
      console.error('Error loading bosses:', err);
      error = err instanceof Error ? err.message : 'Failed to load bosses'; 
    } 
    finally { 
      isLoading = false; 
    }
  }

  function viewBossDetails(bossId: string) {
    goto(`/bosses/${bossId}`);
  }

  function formatHealthPoints(hp: string): string {
    if (!hp || hp === '???' || hp === 'N/A') return 'Unknown';
    return hp;
  }

  function getMainDrop(drops: BossDrop[]): string {
    if (!drops || drops.length === 0) return 'Unknown';
    const mainDrop = drops.find(d => !d.name.toLowerCase().includes('rune')) || drops[0];
    return mainDrop.name;
  }
</script>

<div class="min-h-screen bg-black p-6 text-white">
  <div class="max-w-7xl mx-auto">
    <!-- Back Button -->
    <button 
      on:click={() => goto('/dashboard')} 
      class="text-amber-400 hover:text-amber-300 mb-6 flex items-center gap-2 transition-colors"
    >
      <span>←</span> Back to Dashboard
    </button>

    <!-- Header -->
    <div class="mb-8">
      <h1 class="text-5xl font-bold text-amber-400 mb-2">Bosses</h1>
      <p class="text-gray-400">Select a boss to view details and track your attempts</p>
      {#if selectedCharacter}
        <p class="text-gray-500 text-sm mt-2">
          Playing as <span class="text-amber-400">{selectedCharacter.playerName}</span> • 
          <span class="text-red-400">{defeatedBossIds.length}</span> bosses defeated
        </p>
      {/if}
    </div>

    {#if isLoading}
      <div class="flex items-center justify-center py-20">
        <div class="text-center">
          <div class="animate-spin rounded-full h-16 w-16 border-t-2 border-b-2 border-amber-400 mx-auto mb-4"></div>
          <p class="text-amber-400 text-lg">Loading bosses...</p>
        </div>
      </div>
    {:else if error}
      <div class="bg-red-900/20 border border-red-500 rounded-lg p-6 text-center">
        <p class="text-red-400 text-lg">{error}</p>
      </div>
    {:else}
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {#each bosses as boss}
          <div 
            class="bg-gradient-to-br from-gray-900 to-gray-800 rounded-xl overflow-hidden cursor-pointer hover:shadow-2xl hover:shadow-amber-500/20 transition-all duration-300 hover:scale-105 border border-gray-700 hover:border-amber-500 relative"
            on:click={() => viewBossDetails(boss.id)}
            on:keypress={(e) => e.key === 'Enter' && viewBossDetails(boss.id)}
            role="button"
            tabindex="0"
          >
            <!-- Defeated Badge -->
            {#if isDefeated(boss.id)}
              <div class="absolute top-2 left-2 bg-red-500 text-white px-2 py-1 rounded-full text-xs font-bold z-10 flex items-center gap-1 shadow-lg">
                <span>✓</span>
                <span>Defeated</span>
              </div>
            {/if}

            <!-- Boss Image -->
            <div class="relative h-56 overflow-hidden bg-black">
              {#if boss.image}
                <img 
                  src={boss.image} 
                  alt={boss.name} 
                  class="w-full h-full object-cover opacity-90 hover:opacity-100 transition-opacity"
                />
              {:else}
                <div class="w-full h-full flex items-center justify-center bg-gray-800">
                  <span class="text-gray-500 text-4xl">👹</span>
                </div>
              {/if}
              
              <!-- Health Bar Overlay -->
              <div class="absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black to-transparent p-3">
                <div class="flex items-center gap-2">
                  <div class="h-2 flex-1 bg-gray-700 rounded-full overflow-hidden">
                    <div class="h-full bg-red-500 w-full"></div>
                  </div>
                  <span class="text-xs text-red-400 font-bold whitespace-nowrap">
                    {formatHealthPoints(boss.healthPoints)} HP
                  </span>
                </div>
              </div>
            </div>

            <!-- Boss Details -->
            <div class="p-5">
              <!-- Boss Name -->
              <h2 class="text-xl font-bold text-amber-400 mb-3 line-clamp-1">
                {boss.name}
              </h2>

              <!-- Location Info -->
              <div class="space-y-2 mb-4">
                <div class="flex items-start gap-2">
                  <span class="text-gray-500 text-sm">📍</span>
                  <div class="flex-1">
                    <p class="text-gray-300 text-sm font-medium">{boss.region}</p>
                    {#if boss.location && boss.location !== boss.region}
                      <p class="text-gray-500 text-xs">{boss.location}</p>
                    {/if}
                  </div>
                </div>

                <!-- Main Drop -->
                {#if boss.drops && boss.drops.length > 0}
                  <div class="flex items-center gap-2">
                    <span class="text-gray-500 text-sm">💎</span>
                    <p class="text-gray-400 text-sm line-clamp-1">
                      {getMainDrop(boss.drops)}
                    </p>
                  </div>
                {/if}
              </div>

              <!-- Action Button -->
              <button
                class="w-full bg-amber-500 hover:bg-amber-600 text-black font-bold py-3 rounded-lg transition-colors duration-200 flex items-center justify-center gap-2"
                on:click|stopPropagation={() => viewBossDetails(boss.id)}
              >
                <span>📊</span>
                <span>View Details</span>
              </button>
            </div>
          </div>
        {/each}
      </div>

      {#if bosses.length === 0}
        <div class="text-center py-20">
          <p class="text-gray-400 text-lg">No bosses found</p>
        </div>
      {/if}
    {/if}
  </div>
</div>

<style>
  .line-clamp-1 {
    display: -webkit-box;
    -webkit-line-clamp: 1;
    -webkit-box-orient: vertical;
    overflow: hidden;
  }
</style>