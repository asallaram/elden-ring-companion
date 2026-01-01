<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api';
  import { authStore } from '$lib/stores/auth';
  import { characterStore } from '$lib/stores/character';
  import { goto } from '$app/navigation';

  interface Attack {
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
    scalesWith: ScalesWith[];
    requiredAttributes: RequiredAttribute[];
  }

  let weapons: Weapon[] = [];
  let filteredWeapons: Weapon[] = [];
  let isLoading = true;
  let error = '';
  let searchTerm = '';
  let selectedCategory = 'All';
  let collectedWeaponIds: string[] = [];

  $: selectedCharacter = $characterStore;

  // Update collected weapons when character changes
  $: if (selectedCharacter) {
    collectedWeaponIds = selectedCharacter.obtainedWeaponIds || [];
  }

  // Get unique categories
  $: categories = ['All', ...new Set(weapons.map(w => w.category).filter(Boolean))];

  // Filter weapons
  $: {
    filteredWeapons = weapons.filter(weapon => {
      const matchesSearch = weapon.name.toLowerCase().includes(searchTerm.toLowerCase());
      const matchesCategory = selectedCategory === 'All' || weapon.category === selectedCategory;
      return matchesSearch && matchesCategory;
    });
  }

  // Check if weapon is collected
  function isCollected(weaponId: string): boolean {
    return collectedWeaponIds.includes(weaponId);
  }

  onMount(() => {
  let hasLoaded = false; // Add this
  
  if ($authStore.isLoading) {
    const unsubscribe = authStore.subscribe(state => {
      if (!state.isLoading) {
        unsubscribe();
        if (state.isAuthenticated && !hasLoaded) { // Add check
          hasLoaded = true;
          loadWeapons();
        } else {
          goto('/login');
        }
      }
    });
  } else if ($authStore.isAuthenticated && !hasLoaded) { // Add check
    hasLoaded = true;
    loadWeapons();
  } else {
    goto('/login');
  }
});

  async function loadWeapons() {
    try {
      console.log('Fetching weapons...');
      weapons = await api.weapons.getAll();
      console.log('Weapons loaded:', weapons.length);
    } catch (err) {
      console.error('Error loading weapons:', err);
      error = err instanceof Error ? err.message : 'Failed to load weapons';
    } finally {
      isLoading = false;
    }
  }

  function viewWeaponDetails(weaponId: string) {
    goto(`/weapons/${weaponId}`);
  }

  function getPhysicalDamage(weapon: Weapon): number {
    const phyAttack = weapon.attack?.find(a => a.name === 'Phy' || a.name.toLowerCase().includes('phy'));
    return phyAttack?.amount || 0;
  }

  function getMainScaling(weapon: Weapon): string {
    if (!weapon.scalesWith || weapon.scalesWith.length === 0) return 'None';
    
    const scalingOrder: Record<string, number> = { 'S': 6, 'A': 5, 'B': 4, 'C': 3, 'D': 2, 'E': 1, 'None': 0, '-': 0 };
    const best = weapon.scalesWith
      .filter(s => s.name && s.name !== '-' && s.scaling !== 'None')
      .sort((a, b) => (scalingOrder[b.scaling] || 0) - (scalingOrder[a.scaling] || 0))[0];
    
    return best ? `${best.name} (${best.scaling})` : 'None';
  }

  function getRequirements(weapon: Weapon): string {
    if (!weapon.requiredAttributes || weapon.requiredAttributes.length === 0) return 'None';
    return weapon.requiredAttributes
      .filter(r => r.name && r.name !== '-' && r.amount > 0)
      .map(r => `${r.name} ${r.amount}`)
      .join(', ');
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
      <h1 class="text-5xl font-bold text-amber-400 mb-2">Weapons</h1>
      <p class="text-gray-400">Browse all weapons and find your perfect armament</p>
      {#if selectedCharacter}
        <p class="text-gray-500 text-sm mt-2">
          Playing as <span class="text-amber-400">{selectedCharacter.playerName}</span> • 
          <span class="text-blue-400">{collectedWeaponIds.length}</span> weapons collected
        </p>
      {/if}
    </div>

    <!-- Filters -->
    <div class="mb-6 grid grid-cols-1 md:grid-cols-2 gap-4">
      <!-- Search -->
      <div>
        <input
          type="text"
          bind:value={searchTerm}
          placeholder="Search weapons..."
          class="w-full bg-gray-800 text-white px-4 py-3 rounded-lg border border-gray-700 focus:border-amber-400 focus:outline-none"
        />
      </div>

      <!-- Category Filter -->
      <div>
        <select
          bind:value={selectedCategory}
          class="w-full bg-gray-800 text-white px-4 py-3 rounded-lg border border-gray-700 focus:border-amber-400 focus:outline-none"
        >
          {#each categories as category}
            <option value={category}>{category}</option>
          {/each}
        </select>
      </div>
    </div>

    {#if isLoading}
      <div class="flex items-center justify-center py-20">
        <div class="text-center">
          <div class="animate-spin rounded-full h-16 w-16 border-t-2 border-b-2 border-amber-400 mx-auto mb-4"></div>
          <p class="text-amber-400 text-lg">Loading weapons...</p>
        </div>
      </div>
    {:else if error}
      <div class="bg-red-900/20 border border-red-500 rounded-lg p-6 text-center">
        <p class="text-red-400 text-lg">{error}</p>
      </div>
    {:else}
      <!-- Results Count -->
      <div class="mb-4 text-gray-400">
        Showing {filteredWeapons.length} of {weapons.length} weapons
      </div>

      <!-- Weapons Grid -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
        {#each filteredWeapons as weapon}
          <div
            class="bg-gradient-to-br from-gray-900 to-gray-800 rounded-xl overflow-hidden cursor-pointer hover:shadow-2xl hover:shadow-amber-500/20 transition-all duration-300 hover:scale-105 border border-gray-700 hover:border-amber-500 relative"
            on:click={() => viewWeaponDetails(weapon.id)}
            on:keypress={(e) => e.key === 'Enter' && viewWeaponDetails(weapon.id)}
            role="button"
            tabindex="0"
          >
            <!-- Collected Badge -->
            {#if isCollected(weapon.id)}
              <div class="absolute top-2 left-2 bg-green-500 text-white px-2 py-1 rounded-full text-xs font-bold z-10 flex items-center gap-1 shadow-lg">
                <span>✓</span>
                <span>Collected</span>
              </div>
            {/if}

            <!-- Weapon Image -->
            <div class="relative h-48 overflow-hidden bg-black flex items-center justify-center">
              {#if weapon.image}
                <img
                  src={weapon.image}
                  alt={weapon.name}
                  class="max-h-full max-w-full object-contain opacity-90 hover:opacity-100 transition-opacity p-4"
                />
              {:else}
                <div class="text-gray-500 text-6xl">⚔️</div>
              {/if}
              
              <!-- Category Badge -->
              {#if weapon.category}
                <div class="absolute top-2 right-2 bg-amber-600/90 px-3 py-1 rounded-full text-xs font-bold">
                  {weapon.category}
                </div>
              {/if}
            </div>

            <!-- Weapon Details -->
            <div class="p-4">
              <!-- Weapon Name -->
              <h2 class="text-lg font-bold text-amber-400 mb-3 line-clamp-1">
                {weapon.name}
              </h2>

              <!-- Stats -->
              <div class="space-y-2 mb-4 text-sm">
                <!-- Damage -->
                <div class="flex items-center justify-between">
                  <span class="text-gray-500">⚔️ Physical Dmg</span>
                  <span class="text-white font-bold">{getPhysicalDamage(weapon)}</span>
                </div>

                <!-- Scaling -->
                <div class="flex items-center justify-between">
                  <span class="text-gray-500">📈 Best Scaling</span>
                  <span class="text-amber-300 text-xs font-bold">{getMainScaling(weapon)}</span>
                </div>

                <!-- Weight -->
                <div class="flex items-center justify-between">
                  <span class="text-gray-500">⚖️ Weight</span>
                  <span class="text-gray-300">{weapon.weight}</span>
                </div>
              </div>

              <!-- Requirements -->
              <div class="text-xs text-gray-500 mb-3 truncate">
                Req: {getRequirements(weapon)}
              </div>

              <!-- Action Button -->
              <button
                class="w-full bg-amber-500 hover:bg-amber-600 text-black font-bold py-2 rounded-lg transition-colors duration-200 flex items-center justify-center gap-2 text-sm"
                on:click|stopPropagation={() => viewWeaponDetails(weapon.id)}
              >
                <span>📊</span>
                <span>View Details</span>
              </button>
            </div>
          </div>
        {/each}
      </div>

      {#if filteredWeapons.length === 0}
        <div class="text-center py-20">
          <p class="text-gray-400 text-lg">No weapons found matching your filters</p>
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