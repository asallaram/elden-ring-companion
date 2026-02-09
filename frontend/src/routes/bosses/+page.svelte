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

<div class="min-h-screen bg-[#0a0a0a] p-6 text-white">
  <div class="max-w-7xl mx-auto">
    <!-- Back Button -->
    <button 
      on:click={() => goto('/dashboard')} 
      class="text-amber-600 hover:text-amber-500 mb-6 flex items-center gap-2 transition-colors text-sm"
    >
      <span>←</span> Back to Dashboard
    </button>

    <!-- Header -->
    <div class="mb-8">
      <h1 class="text-4xl font-bold text-white mb-2 uppercase tracking-wide">Bosses</h1>
      <p class="text-neutral-500 text-sm">Select a boss to view details and track your attempts</p>
      {#if selectedCharacter}
        <p class="text-neutral-600 text-xs mt-2">
          Playing as <span class="text-amber-500">{selectedCharacter.playerName}</span> • 
          <span class="text-red-500">{defeatedBossIds.length}</span> bosses defeated
        </p>
      {/if}
    </div>

    {#if isLoading}
      <div class="flex items-center justify-center py-20">
        <div class="text-center">
          <div class="w-12 h-12 border-2 border-neutral-700 border-t-amber-600 mx-auto mb-4 animate-spin"></div>
          <p class="text-neutral-400 text-sm">Loading bosses...</p>
        </div>
      </div>
    {:else if error}
      <div class="bg-red-950/30 border border-red-900 p-6 text-center">
        <p class="text-red-400">{error}</p>
      </div>
    {:else}
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
        {#each bosses as boss}
          <div 
            class="bg-neutral-900 border border-neutral-800 overflow-hidden cursor-pointer hover:border-amber-800 transition-colors relative"
            on:click={() => viewBossDetails(boss.id)}
            on:keypress={(e) => e.key === 'Enter' && viewBossDetails(boss.id)}
            role="button"
            tabindex="0"
          >
            <!-- Defeated Badge -->
            {#if isDefeated(boss.id)}
              <div class="absolute top-2 left-2 bg-red-900 border border-red-700 text-red-300 px-2 py-1 text-xs font-bold z-10 flex items-center gap-1">
                <span>✓</span>
                <span>Defeated</span>
              </div>
            {/if}

            <!-- Boss Image -->
            <div class="relative h-56 overflow-hidden bg-black border-b border-neutral-800">
              {#if boss.image}
                <img 
                  src={boss.image} 
                  alt={boss.name} 
                  class="w-full h-full object-cover opacity-90 hover:opacity-100 transition-opacity"
                />
              {:else}
                <div class="w-full h-full flex items-center justify-center bg-neutral-950">
                  <span class="text-neutral-700 text-4xl">👹</span>
                </div>
              {/if}
              
              <!-- Health Bar Overlay -->
              <div class="absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black to-transparent p-3">
                <div class="flex items-center gap-2">
                  <div class="h-1.5 flex-1 bg-neutral-800 overflow-hidden">
                    <div class="h-full bg-red-600 w-full"></div>
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
              <h2 class="text-lg font-bold text-white mb-3 line-clamp-1 uppercase tracking-wide">
                {boss.name}
              </h2>

              <!-- Location Info -->
              <div class="space-y-2 mb-4">
                <div class="flex items-start gap-2">
                  <span class="text-neutral-600 text-sm">📍</span>
                  <div class="flex-1">
                    <p class="text-neutral-400 text-sm font-medium">{boss.region}</p>
                    {#if boss.location && boss.location !== boss.region}
                      <p class="text-neutral-600 text-xs">{boss.location}</p>
                    {/if}
                  </div>
                </div>

                <!-- Main Drop -->
                {#if boss.drops && boss.drops.length > 0}
                  <div class="flex items-center gap-2">
                    <span class="text-neutral-600 text-sm">💎</span>
                    <p class="text-neutral-500 text-sm line-clamp-1">
                      {getMainDrop(boss.drops)}
                    </p>
                  </div>
                {/if}
              </div>

              <!-- Action Button -->
              <button
                class="w-full bg-amber-900 hover:bg-amber-800 text-white font-semibold py-3 transition-colors flex items-center justify-center gap-2 text-sm"
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
          <p class="text-neutral-500">No bosses found</p>
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