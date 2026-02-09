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

<div class="min-h-screen bg-[#0a0a0a] text-white">
  <div class="max-w-5xl mx-auto p-6">
    <!-- Back Button -->
    <button 
      on:click={() => goto('/dashboard')} 
      class="text-amber-600 hover:text-amber-500 mb-6 flex items-center gap-2 transition-colors text-sm"
    >
      <span>←</span> Back to Dashboard
    </button>

    <!-- Header -->
    <div class="mb-8">
      <h1 class="text-4xl font-bold text-white mb-2 uppercase tracking-wide">Tips & Strategies</h1>
      <p class="text-neutral-500 text-sm">Essential knowledge for surviving the Lands Between</p>
    </div>

    <!-- Personalized Tips Teaser -->
    <div class="mb-6 bg-neutral-900 border-2 border-amber-800 p-6">
      <div class="flex items-start gap-4">
        <div class="text-4xl">🔮</div>
        <div class="flex-1">
          <h3 class="text-lg font-bold text-amber-400 mb-2 uppercase tracking-wide">Personalized Tips Coming Soon!</h3>
          <p class="text-neutral-400 text-sm leading-relaxed">
            We're building an AI-powered system that will analyze your play style, boss attempts, and weapon choices to provide custom-tailored strategies just for you. Track your progress to unlock personalized insights!
          </p>
        </div>
      </div>
    </div>

    <!-- Tips Grid -->
    <div class="space-y-6">
      
      <!-- Combat Tips -->
      <div class="bg-neutral-900 border border-neutral-800 p-6">
        <h2 class="text-xl font-bold text-white mb-4 pb-4 border-b border-neutral-800 uppercase tracking-wide flex items-center gap-2">
          <span>⚔️</span>
          <span>Combat Essentials</span>
        </h2>
        <div class="space-y-3">
          <div class="bg-neutral-950 border border-neutral-800 p-4">
            <h3 class="text-white font-bold mb-2 text-sm">🛡️ Master the Roll</h3>
            <p class="text-neutral-400 text-sm leading-relaxed">Rolling grants invincibility frames. Don't panic roll—wait for the attack to start, then roll through it. Medium roll (under 70% equip load) is essential for most builds.</p>
          </div>
          <div class="bg-neutral-950 border border-neutral-800 p-4">
            <h3 class="text-white font-bold mb-2 text-sm">⏱️ Patience Over Aggression</h3>
            <p class="text-neutral-400 text-sm leading-relaxed">Bosses punish greed. Learn their combos, attack during openings, then retreat. Two safe hits beat five risky ones that get you killed.</p>
          </div>
          <div class="bg-neutral-950 border border-neutral-800 p-4">
            <h3 class="text-white font-bold mb-2 text-sm">🎯 Stance Breaking</h3>
            <p class="text-neutral-400 text-sm leading-relaxed">Heavy attacks, jump attacks, and charged R2s break enemy stance faster. A stance break opens them for a critical hit dealing massive damage.</p>
          </div>
          <div class="bg-neutral-950 border border-neutral-800 p-4">
            <h3 class="text-white font-bold mb-2 text-sm">⚡ Status Effects Are Powerful</h3>
            <p class="text-neutral-400 text-sm leading-relaxed">Bleed and Frostbite deal percentage-based damage, effective against high HP bosses. Poison/Scarlet Rot work great for slow, methodical fights.</p>
          </div>
        </div>
      </div>

      <!-- Character Building -->
      <div class="bg-neutral-900 border border-neutral-800 p-6">
        <h2 class="text-xl font-bold text-white mb-4 pb-4 border-b border-neutral-800 uppercase tracking-wide flex items-center gap-2">
          <span>📊</span>
          <span>Character Building</span>
        </h2>
        <div class="space-y-3">
          <div class="bg-neutral-950 border border-neutral-800 p-4">
            <h3 class="text-white font-bold mb-2 text-sm">⬆️ Vigor Is Never Wasted</h3>
            <p class="text-neutral-400 text-sm leading-relaxed">Aim for 40 Vigor minimum, 60 for late game. More HP means more room for error and learning boss patterns without dying in one hit.</p>
          </div>
          <div class="bg-neutral-950 border border-neutral-800 p-4">
            <h3 class="text-white font-bold mb-2 text-sm">🎯 Quality vs Specialization</h3>
            <p class="text-neutral-400 text-sm leading-relaxed">Pure builds (STR, DEX, INT, FTH) outscale hybrid builds. Pick one or two damage stats max and meet minimum requirements for your favorite weapons.</p>
          </div>
          <div class="bg-neutral-950 border border-neutral-800 p-4">
            <h3 class="text-white font-bold mb-2 text-sm">⚖️ Equipment Load Breakpoints</h3>
            <p class="text-neutral-400 text-sm leading-relaxed">Under 30% = Fast roll (PvP meta). Under 70% = Medium roll (best for PvE). Over 70% = Fat roll (avoid this). Level Endurance if you're close to 70%.</p>
          </div>
          <div class="bg-neutral-950 border border-neutral-800 p-4">
            <h3 class="text-white font-bold mb-2 text-sm">🔮 Soft Caps Matter</h3>
            <p class="text-neutral-400 text-sm leading-relaxed">Most stats have soft caps (diminishing returns): Vigor at 40/60, damage stats at 55/80. Plan your levels accordingly to avoid wasted points.</p>
          </div>
        </div>
      </div>

      <!-- Weapon & Gear -->
      <div class="bg-neutral-900 border border-neutral-800 p-6">
        <h2 class="text-xl font-bold text-white mb-4 pb-4 border-b border-neutral-800 uppercase tracking-wide flex items-center gap-2">
          <span>🗡️</span>
          <span>Weapons & Gear</span>
        </h2>
        <div class="space-y-3">
          <div class="bg-neutral-950 border border-neutral-800 p-4">
            <h3 class="text-white font-bold mb-2 text-sm">⚒️ Upgrade Over Leveling</h3>
            <p class="text-neutral-400 text-sm leading-relaxed">A +15 weapon outdamages 20 extra levels. Prioritize weapon upgrades early. Smithing Stones are more valuable than runes in early/mid game.</p>
          </div>
          <div class="bg-neutral-950 border border-neutral-800 p-4">
            <h3 class="text-white font-bold mb-2 text-sm">🔄 Ashes of War Are Game Changers</h3>
            <p class="text-neutral-400 text-sm leading-relaxed">Don't sleep on Ashes of War. Bloodhound's Step, Lion's Claw, and Hoarfrost Stomp can trivialize difficult encounters. Experiment freely.</p>
          </div>
          <div class="bg-neutral-950 border border-neutral-800 p-4">
            <h3 class="text-white font-bold mb-2 text-sm">🛡️ Shields Are Underrated</h3>
            <p class="text-neutral-400 text-sm leading-relaxed">Great Shields with Barricade Shield ash make you nearly invincible. Guard counters deal high stance damage. Don't dismiss defensive options.</p>
          </div>
          <div class="bg-neutral-950 border border-neutral-800 p-4">
            <h3 class="text-white font-bold mb-2 text-sm">📿 Talismans Make Builds</h3>
            <p class="text-neutral-400 text-sm leading-relaxed">Talismans can increase damage by 20%+ or negate weaknesses. Radagon's Soreseal is amazing early but bad late. Dragoncrest Greatshield is always good.</p>
          </div>
        </div>
      </div>

      <!-- Boss Strategies -->
      <div class="bg-neutral-900 border border-neutral-800 p-6">
        <h2 class="text-xl font-bold text-white mb-4 pb-4 border-b border-neutral-800 uppercase tracking-wide flex items-center gap-2">
          <span>👹</span>
          <span>Boss Fight Strategies</span>
        </h2>
        <div class="space-y-3">
          <div class="bg-neutral-950 border border-neutral-800 p-4">
            <h3 class="text-white font-bold mb-2 text-sm">📝 Track Your Attempts</h3>
            <p class="text-neutral-400 text-sm leading-relaxed">Use our boss tracker to record which attacks killed you and what weapons worked. Reviewing patterns between attempts accelerates learning dramatically.</p>
          </div>
          <div class="bg-neutral-950 border border-neutral-800 p-4">
            <h3 class="text-white font-bold mb-2 text-sm">🎭 Learn One Phase at a Time</h3>
            <p class="text-neutral-400 text-sm leading-relaxed">Don't worry about phase 2 until you can no-hit phase 1. Master each phase independently before attempting the full fight consistently.</p>
          </div>
          <div class="bg-neutral-950 border border-neutral-800 p-4">
            <h3 class="text-white font-bold mb-2 text-sm">🤝 Spirit Ashes Are Not Cheating</h3>
            <p class="text-neutral-400 text-sm leading-relaxed">Mimic Tear, Black Knife Tiche, and Lhutel are powerful summons. The game was balanced around using them. Solo is optional difficulty.</p>
          </div>
          <div class="bg-neutral-950 border border-neutral-800 p-4">
            <h3 class="text-white font-bold mb-2 text-sm">💊 Flask Management</h3>
            <p class="text-neutral-400 text-sm leading-relaxed">Find Sacred Tears (+healing per flask) and Golden Seeds (+flask count). Allocate more crimson for melee, more cerulean for casters. Adjust per boss.</p>
          </div>
          <div class="bg-neutral-950 border border-neutral-800 p-4">
            <h3 class="text-white font-bold mb-2 text-sm">🔥 Damage Type Weaknesses</h3>
            <p class="text-neutral-400 text-sm leading-relaxed">Most bosses are weak to either fire, lightning, or holy. Check our weapon recommendations for each boss to exploit their weaknesses effectively.</p>
          </div>
        </div>
      </div>

      <!-- Exploration -->
      <div class="bg-neutral-900 border border-neutral-800 p-6">
        <h2 class="text-xl font-bold text-white mb-4 pb-4 border-b border-neutral-800 uppercase tracking-wide flex items-center gap-2">
          <span>🗺️</span>
          <span>Exploration</span>
        </h2>
        <div class="space-y-3">
          <div class="bg-neutral-950 border border-neutral-800 p-4">
            <h3 class="text-white font-bold mb-2 text-sm">🔍 Side Dungeons Are Essential</h3>
            <p class="text-neutral-400 text-sm leading-relaxed">Catacombs, caves, and tunnels contain upgrade materials, unique weapons, and spells. Don't skip them—they make the main path much easier.</p>
          </div>
          <div class="bg-neutral-950 border border-neutral-800 p-4">
            <h3 class="text-white font-bold mb-2 text-sm">🌟 Golden Runes Are Safety Nets</h3>
            <p class="text-neutral-400 text-sm leading-relaxed">Hoard consumable golden runes (1-13). Use them when you're close to leveling before dangerous areas. They're emergency currency that can't be lost.</p>
          </div>
          <div class="bg-neutral-950 border border-neutral-800 p-4">
            <h3 class="text-white font-bold mb-2 text-sm">📍 Map Fragments First</h3>
            <p class="text-neutral-400 text-sm leading-relaxed">Grab map fragments before thorough exploration. They reveal Points of Interest and prevent you from missing important locations in large regions.</p>
          </div>
          <div class="bg-neutral-950 border border-neutral-800 p-4">
            <h3 class="text-white font-bold mb-2 text-sm">🏃 Torrent Mobility Tricks</h3>
            <p class="text-neutral-400 text-sm leading-relaxed">Double jump to cross gaps. Sprint + jump for maximum distance. You can drink flasks on horseback. Use Torrent to escape bad situations.</p>
          </div>
        </div>
      </div>

      <!-- Mental Game -->
      <div class="bg-neutral-900 border border-neutral-800 p-6">
        <h2 class="text-xl font-bold text-white mb-4 pb-4 border-b border-neutral-800 uppercase tracking-wide flex items-center gap-2">
          <span>💡</span>
          <span>Mental Game</span>
        </h2>
        <div class="space-y-3">
          <div class="bg-neutral-950 border border-neutral-800 p-4">
            <h3 class="text-white font-bold mb-2 text-sm">💰 Spend Runes Regularly</h3>
            <p class="text-neutral-400 text-sm leading-relaxed">Don't walk around with 50k+ runes. Level up or buy smithing stones. Losing progress to one death creates tilt and frustration.</p>
          </div>
          <div class="bg-neutral-950 border border-neutral-800 p-4">
            <h3 class="text-white font-bold mb-2 text-sm">😤 The Three-Death Rule</h3>
            <p class="text-neutral-400 text-sm leading-relaxed">If you die to the same boss three times in a row making the same mistake, take a break. Fatigue causes repeated errors. Come back fresh.</p>
          </div>
          <div class="bg-neutral-950 border border-neutral-800 p-4">
            <h3 class="text-white font-bold mb-2 text-sm">🎯 Stuck? Go Elsewhere</h3>
            <p class="text-neutral-400 text-sm leading-relaxed">Elden Ring is open-world. If a boss is too hard, explore other areas, get stronger, then return. There's no shame in coming back later.</p>
          </div>
          <div class="bg-neutral-950 border border-neutral-800 p-4">
            <h3 class="text-white font-bold mb-2 text-sm">🔄 Respec Is Always Available</h3>
            <p class="text-neutral-400 text-sm leading-relaxed">Rennala allows unlimited respecs after Liurnia. Experiment with builds. If something isn't working, change it. Build diversity is encouraged.</p>
          </div>
        </div>
      </div>

    </div>

    <!-- Footer -->
    <div class="mt-8 bg-neutral-900 border-2 border-amber-800 p-6 text-center">
      <h3 class="text-lg font-bold text-amber-400 mb-2 uppercase tracking-wide">Remember, Tarnished</h3>
      <p class="text-neutral-400 mb-4 text-sm leading-relaxed">"A lowly Tarnished, playing as a Lord." Every death teaches. Every attempt makes you stronger. Rise, and claim your destiny!</p>
      <button
        on:click={() => goto('/dashboard')}
        class="px-6 py-3 bg-amber-900 hover:bg-amber-800 text-white font-semibold transition-colors"
      >
        Back to Dashboard
      </button>
    </div>
  </div>
</div>