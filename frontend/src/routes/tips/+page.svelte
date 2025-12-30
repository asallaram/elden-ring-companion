<script lang="ts">
  import { goto } from '$app/navigation';
  import { onMount } from 'svelte';
  import { authStore } from '$lib/stores/auth';

  onMount(() => {
    if ($authStore.isLoading) {
      const unsubscribe = authStore.subscribe(state => {
        if (!state.isLoading) {
          unsubscribe();
          if (!state.isAuthenticated) {
            goto('/login');
          }
        }
      });
    } else if (!$authStore.isAuthenticated) {
      goto('/login');
    }
  });
</script>

<svelte:head>
  <title>Tips & Strategies - Elden Ring Sim</title>
</svelte:head>

<div class="min-h-screen bg-black text-white">
  <div class="max-w-5xl mx-auto p-6">
    <!-- Back Button -->
    <button 
      on:click={() => goto('/dashboard')} 
      class="text-amber-400 hover:text-amber-300 mb-6 flex items-center gap-2 transition-colors"
    >
      <span>←</span> Back to Dashboard
    </button>

    <!-- Header -->
    <div class="mb-8">
      <h1 class="text-5xl font-bold text-amber-400 mb-2">Tips & Strategies</h1>
      <p class="text-gray-400">Essential knowledge for surviving the Lands Between</p>
    </div>

    <!-- Tips Grid -->
    <div class="space-y-6">
      
      <!-- Combat Tips -->
      <div class="bg-gradient-to-br from-gray-900 to-gray-800 rounded-xl p-6 border border-gray-700">
        <h2 class="text-2xl font-bold text-amber-400 mb-4 flex items-center gap-2">
          <span>⚔️</span>
          <span>Combat Essentials</span>
        </h2>
        <div class="space-y-4">
          <div class="bg-black/30 rounded-lg p-4 border border-gray-700">
            <h3 class="text-white font-bold mb-2">🛡️ Learn to Dodge Roll</h3>
            <p class="text-gray-300 text-sm">Rolling gives you invincibility frames (i-frames). Time your rolls to avoid damage entirely, rather than blocking every attack.</p>
          </div>
          <div class="bg-black/30 rounded-lg p-4 border border-gray-700">
            <h3 class="text-white font-bold mb-2">⏱️ Be Patient</h3>
            <p class="text-gray-300 text-sm">Don't get greedy with attacks. Learn boss patterns, attack when safe, then back off. One or two hits at a time is often better than going all-in.</p>
          </div>
          <div class="bg-black/30 rounded-lg p-4 border border-gray-700">
            <h3 class="text-white font-bold mb-2">🎯 Aim for Weak Points</h3>
            <p class="text-gray-300 text-sm">Many bosses have specific weak points. Breaking stance can lead to critical hits that deal massive damage.</p>
          </div>
        </div>
      </div>

      <!-- Character Building -->
      <div class="bg-gradient-to-br from-gray-900 to-gray-800 rounded-xl p-6 border border-gray-700">
        <h2 class="text-2xl font-bold text-amber-400 mb-4 flex items-center gap-2">
          <span>📊</span>
          <span>Character Building</span>
        </h2>
        <div class="space-y-4">
          <div class="bg-black/30 rounded-lg p-4 border border-gray-700">
            <h3 class="text-white font-bold mb-2">⬆️ Level Vigor First</h3>
            <p class="text-gray-300 text-sm">HP (Vigor) is the most important stat early game. Aim for at least 40 Vigor before heavily investing in damage stats.</p>
          </div>
          <div class="bg-black/30 rounded-lg p-4 border border-gray-700">
            <h3 class="text-white font-bold mb-2">🎯 Specialize Your Build</h3>
            <p class="text-gray-300 text-sm">Don't spread stats too thin. Pick 1-2 damage stats (STR, DEX, INT, FTH, ARC) and focus on those for your weapon scaling.</p>
          </div>
          <div class="bg-black/30 rounded-lg p-4 border border-gray-700">
            <h3 class="text-white font-bold mb-2">⚖️ Watch Your Equipment Load</h3>
            <p class="text-gray-300 text-sm">Stay under 70% equipment load for medium roll. Heavy roll (70%+) is slow and will get you killed. Level Endurance if needed.</p>
          </div>
        </div>
      </div>

      <!-- Exploration -->
      <div class="bg-gradient-to-br from-gray-900 to-gray-800 rounded-xl p-6 border border-gray-700">
        <h2 class="text-2xl font-bold text-amber-400 mb-4 flex items-center gap-2">
          <span>🗺️</span>
          <span>Exploration</span>
        </h2>
        <div class="space-y-4">
          <div class="bg-black/30 rounded-lg p-4 border border-gray-700">
            <h3 class="text-white font-bold mb-2">🔍 Explore Thoroughly</h3>
            <p class="text-gray-300 text-sm">Don't rush through areas. Hidden caves, dungeons, and secrets contain powerful weapons, spells, and upgrade materials.</p>
          </div>
          <div class="bg-black/30 rounded-lg p-4 border border-gray-700">
            <h3 class="text-white font-bold mb-2">🏰 Clear Dungeons for Upgrades</h3>
            <p class="text-gray-300 text-sm">Mini-dungeons often reward Smithing Stones to upgrade your weapons. A +10 weapon is much more important than 5 more levels.</p>
          </div>
          <div class="bg-black/30 rounded-lg p-4 border border-gray-700">
            <h3 class="text-white font-bold mb-2">📍 Use Sites of Grace Wisely</h3>
            <p class="text-gray-300 text-sm">Rest at Sites of Grace to respawn enemies and refill flasks, but remember this also respawns all non-boss enemies in the area.</p>
          </div>
        </div>
      </div>

      <!-- Boss Strategies -->
      <div class="bg-gradient-to-br from-gray-900 to-gray-800 rounded-xl p-6 border border-gray-700">
        <h2 class="text-2xl font-bold text-amber-400 mb-4 flex items-center gap-2">
          <span>👹</span>
          <span>Boss Fight Strategies</span>
        </h2>
        <div class="space-y-4">
          <div class="bg-black/30 rounded-lg p-4 border border-gray-700">
            <h3 class="text-white font-bold mb-2">📝 Track Your Attempts</h3>
            <p class="text-gray-300 text-sm">Use our boss fight tracker to record what worked and what didn't. Learning from each death is how you improve.</p>
          </div>
          <div class="bg-black/30 rounded-lg p-4 border border-gray-700">
            <h3 class="text-white font-bold mb-2">🗡️ Try Different Weapons</h3>
            <p class="text-gray-300 text-sm">If you're stuck, experiment with different weapon types. Some bosses are weak to certain damage types or weapon speeds.</p>
          </div>
          <div class="bg-black/30 rounded-lg p-4 border border-gray-700">
            <h3 class="text-white font-bold mb-2">🤝 Summon Spirit Ashes</h3>
            <p class="text-gray-300 text-sm">Spirit Ashes can distract bosses and give you breathing room. Mimic Tear is particularly powerful for solo players.</p>
          </div>
          <div class="bg-black/30 rounded-lg p-4 border border-gray-700">
            <h3 class="text-white font-bold mb-2">💊 Upgrade Your Flask</h3>
            <p class="text-gray-300 text-sm">Find Golden Seeds and Sacred Tears to increase flask charges and healing amount. This can make all the difference.</p>
          </div>
        </div>
      </div>

      <!-- General Tips -->
      <div class="bg-gradient-to-br from-gray-900 to-gray-800 rounded-xl p-6 border border-gray-700">
        <h2 class="text-2xl font-bold text-amber-400 mb-4 flex items-center gap-2">
          <span>💡</span>
          <span>General Tips</span>
        </h2>
        <div class="space-y-4">
          <div class="bg-black/30 rounded-lg p-4 border border-gray-700">
            <h3 class="text-white font-bold mb-2">💰 Don't Hoard Runes</h3>
            <p class="text-gray-300 text-sm">Spend your runes regularly. It's better to level up than lose 50,000 runes to a random death.</p>
          </div>
          <div class="bg-black/30 rounded-lg p-4 border border-gray-700">
            <h3 class="text-white font-bold mb-2">🔄 Experiment and Adapt</h3>
            <p class="text-gray-300 text-sm">There's no "wrong" build. Try different playstyles and find what's fun for you. You can always respec later.</p>
          </div>
          <div class="bg-black/30 rounded-lg p-4 border border-gray-700">
            <h3 class="text-white font-bold mb-2">😤 Take Breaks</h3>
            <p class="text-gray-300 text-sm">Stuck on a boss? Take a break and come back fresh. Sometimes you just need to clear your head.</p>
          </div>
          <div class="bg-black/30 rounded-lg p-4 border border-gray-700">
            <h3 class="text-white font-bold mb-2">📚 Learn the Lore (Optional)</h3>
            <p class="text-gray-300 text-sm">Item descriptions tell the story. Reading them adds depth to the world, but it's totally optional.</p>
          </div>
        </div>
      </div>

    </div>

    <!-- Footer -->
    <div class="mt-8 bg-gradient-to-br from-gray-900 to-gray-800 rounded-xl p-6 border border-amber-700/40 text-center">
      <h3 class="text-xl font-bold text-amber-400 mb-2">Remember</h3>
      <p class="text-gray-300 mb-4">"A lowly Tarnished, playing as a Lord." - Every death is a lesson. Keep trying, Tarnished!</p>
      <button
        on:click={() => goto('/dashboard')}
        class="px-6 py-3 bg-amber-500 hover:bg-amber-600 text-black font-bold rounded-lg transition-colors"
      >
        Back to Dashboard
      </button>
    </div>
  </div>
</div>