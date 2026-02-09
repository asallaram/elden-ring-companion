<script lang="ts">
  import { onMount, onDestroy } from 'svelte';
  import { page } from '$app/stores';
  import { goto } from '$app/navigation';
  import { api } from '$lib/api';
  import { authStore } from '$lib/stores/auth';
  import { characterStore } from '$lib/stores/character';

  interface Boss {
    id: string;
    name: string;
    image?: string;
    region?: string;
    healthPoints?: string;
  }

  interface Weapon {
    id: string;
    name: string;
  }

  interface Attempt {
    id: string;
    weaponId: string;
    weaponName?: string;
    notes: string;
    victory: boolean;
    timestamp: string;
    timeSpentSeconds: number;
  }

  let boss: Boss | null = null;
  let weapons: Weapon[] = [];
  let attempts: Attempt[] = [];
  let isLoading = true;
  let error = '';
  
  // Form state
  let selectedWeaponId = '';
  let notes = '';
  let isVictory = false;
  let isRecording = false;
  let showVictoryModal = false;

  let sessionId = '';
  let userId = '';
  
  // Timer state
  let sessionStartTime: number = Date.now();
  let attemptStartTime: number = Date.now();
  let sessionElapsed: number = 0;
  let attemptElapsed: number = 0;
  let timerInterval: ReturnType<typeof setInterval> | null = null;

  onMount(async () => {
    if ($authStore.isLoading) {
      const unsubscribe = authStore.subscribe(state => {
        if (!state.isLoading) {
          unsubscribe();
          if (state.isAuthenticated) {
            loadTrackerData();
          } else {
            goto('/login');
          }
        }
      });
    } else if ($authStore.isAuthenticated) {
      loadTrackerData();
    } else {
      goto('/login');
    }
  });

  async function loadTrackerData() {
    const user = $authStore.user;
    if (user) {
      userId = user.userId;
    }

    const bossId = $page.params.id;
    if (!bossId) {
      error = 'Invalid boss ID';
      isLoading = false;
      return;
    }

    try {
      // Load boss data
      boss = await api.bosses.getById(bossId);
      
      // Load weapons
      weapons = await api.weapons.getAll();

      // Get active session
      try {
        const activeSession = await api.fights.getActiveSession(userId, bossId);
        if (activeSession) {
          sessionId = activeSession.id;
          
          // Load attempts for this session
          try {
            const sessionAttempts = await api.fights.getSessionAttempts(sessionId);
            attempts = sessionAttempts || [];
          } catch (err) {
            console.log('Could not load session attempts');
          }
        }
      } catch (err) {
        console.log('No active session found');
      }

      // Start timers
      sessionStartTime = Date.now();
      attemptStartTime = Date.now();
      
      timerInterval = setInterval(() => {
        sessionElapsed = Math.floor((Date.now() - sessionStartTime) / 1000);
        attemptElapsed = Math.floor((Date.now() - attemptStartTime) / 1000);
      }, 1000);

    } catch (err) {
      console.error('Error loading tracker:', err);
      error = err instanceof Error ? err.message : 'Failed to load tracker';
    } finally {
      isLoading = false;
    }
  }

  onDestroy(() => {
    if (timerInterval) {
      clearInterval(timerInterval);
    }
  });

  function formatTime(seconds: number): string {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  }

  function formatTimestamp(timestamp: string): string {
    const date = new Date(timestamp);
    return date.toLocaleTimeString('en-US', { 
      hour: '2-digit', 
      minute: '2-digit',
      second: '2-digit'
    });
  }

  async function recordAttempt() {
    if (!selectedWeaponId) {
      alert('Please select a weapon');
      return;
    }

    if (!boss || !userId) return;

    const wasVictory = isVictory;
    const timeSpent = attemptElapsed;

    isRecording = true;

    // ✨ If victory, mark boss as defeated FIRST
    if (wasVictory) {
      console.log('🎉 Victory detected!');
      console.log('Character store:', $characterStore);
      
      if (!$characterStore) {
        console.error('❌ No character selected!');
        alert('Please select a character on the dashboard first!');
        isRecording = false;
        return;
      }

      try {
        console.log('Calling API to mark boss as defeated...');
        console.log('Character ID:', $characterStore.id);
        console.log('Boss ID:', boss.id);
        
        await api.playerProgress.addBoss($characterStore.id, boss.id);
        
        // Update character store
        const updatedDefeatedBosses = $characterStore.defeatedBossIds.includes(boss.id)
          ? $characterStore.defeatedBossIds
          : [...$characterStore.defeatedBossIds, boss.id];
        
        const updatedCharacter = {
          ...$characterStore,
          defeatedBossIds: updatedDefeatedBosses
        };
        
        characterStore.updateData(updatedCharacter);
        
        console.log('✅ Boss marked as defeated!');
        console.log('Updated defeated bosses:', updatedDefeatedBosses);
      } catch (err) {
        console.error('❌ Failed to mark boss as defeated:', err);
        alert('Failed to mark boss as defeated: ' + (err instanceof Error ? err.message : 'Unknown error'));
        isRecording = false;
        return;
      }
    }

    try {
      const selectedWeapon = weapons.find(w => w.id === selectedWeaponId);
      
      const attemptData = {
        progressId: userId,
        bossId: boss.id,
        bossName: boss.name,
        weaponId: selectedWeaponId,
        weaponName: selectedWeapon?.name || '',
        notes,
        victory: isVictory,
        timeSpentSeconds: timeSpent
      };

      const newAttempt = await api.fights.recordAttempt(attemptData);
      
      // Add to local attempts list
      attempts = [...attempts, {
        id: newAttempt.id || Date.now().toString(),
        weaponId: selectedWeaponId,
        weaponName: selectedWeapon?.name,
        notes,
        victory: isVictory,
        timestamp: new Date().toISOString(),
        timeSpentSeconds: timeSpent
      }];

      console.log('✅ Attempt recorded!');

      // Reset form and attempt timer
      notes = '';
      isVictory = false;
      attemptStartTime = Date.now();
      attemptElapsed = 0;

      // If victory, show victory modal
      if (wasVictory) {
        showVictoryModal = true;
      }
    } catch (err) {
      console.error('Error recording attempt:', err);
      error = err instanceof Error ? err.message : 'Failed to record attempt';
    } finally {
      isRecording = false;
    }
  }

  async function endSession() {
    if (timerInterval) clearInterval(timerInterval);
    
    if (sessionId) {
      try {
        await api.fights.endSession(sessionId);
      } catch (err) {
        console.error('Error ending session:', err);
      }
    }
    
    goto('/bosses');
  }

  function getVictoryCount(): number {
    return attempts.filter(a => a.victory).length;
  }

  function getDefeatCount(): number {
    return attempts.filter(a => !a.victory).length;
  }

  function continueAfterVictory() {
    showVictoryModal = false;
  }

  async function returnToDashboard() {
    showVictoryModal = false;
    await endSession();
  }
</script>

<div class="min-h-screen bg-[#0a0a0a] p-6 text-white">
  {#if isLoading}
    <div class="flex items-center justify-center min-h-screen">
      <div class="text-center">
        <div class="w-12 h-12 border-2 border-neutral-700 border-t-amber-600 mx-auto mb-4 animate-spin"></div>
        <p class="text-neutral-400 text-sm">Loading tracker...</p>
      </div>
    </div>
  {:else if error}
    <div class="max-w-4xl mx-auto">
      <div class="bg-red-950/30 border border-red-900 p-6 text-center">
        <p class="text-red-400 mb-4">{error}</p>
        <button 
          on:click={() => goto('/bosses')}
          class="bg-amber-900 hover:bg-amber-800 text-white font-semibold py-2 px-6 transition-colors"
        >
          Back to Bosses
        </button>
      </div>
    </div>
  {:else if boss}
    <div class="max-w-5xl mx-auto">
      <!-- Header -->
      <div class="mb-6">
        <button 
          on:click={endSession} 
          class="text-amber-600 hover:text-amber-500 mb-4 flex items-center gap-2 transition-colors text-sm"
        >
          <span>←</span> End Session & Return
        </button>

        <div class="bg-neutral-900 border border-neutral-800 p-6">
          <div class="flex items-center gap-6">
            {#if boss.image}
              <img 
                src={boss.image} 
                alt={boss.name} 
                class="w-24 h-24 object-cover border-2 border-amber-700"
              />
            {/if}
            <div class="flex-1">
              <h1 class="text-3xl font-bold text-white mb-2">{boss.name}</h1>
              {#if boss.region}
                <p class="text-neutral-500 text-sm uppercase tracking-wide">{boss.region}</p>
              {/if}
            </div>
            <div class="text-right">
              <div class="text-3xl font-mono text-amber-500 font-bold mb-1">
                {formatTime(sessionElapsed)}
              </div>
              <p class="text-neutral-600 text-xs uppercase tracking-wide">Session Time</p>
            </div>
          </div>

          <!-- Stats Bar -->
          <div class="grid grid-cols-3 gap-4 mt-6 pt-6 border-t border-neutral-800">
            <div class="text-center">
              <p class="text-2xl font-bold text-white">{attempts.length}</p>
              <p class="text-neutral-500 text-xs uppercase tracking-wide">Total Attempts</p>
            </div>
            <div class="text-center">
              <p class="text-2xl font-bold text-green-500">{getVictoryCount()}</p>
              <p class="text-neutral-500 text-xs uppercase tracking-wide">Victories</p>
            </div>
            <div class="text-center">
              <p class="text-2xl font-bold text-red-500">{getDefeatCount()}</p>
              <p class="text-neutral-500 text-xs uppercase tracking-wide">Defeats</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Record Attempt Form -->
      <div class="bg-neutral-900 border border-neutral-800 p-6 mb-6">
        <div class="flex justify-between items-center mb-6 pb-4 border-b border-neutral-800">
          <h2 class="text-xl font-bold text-white uppercase tracking-wide">Record Attempt</h2>
          <div class="flex items-center gap-3">
            <span class="text-neutral-500 text-sm">Current Attempt:</span>
            <div class="text-2xl font-mono text-amber-500 font-bold">
              {formatTime(attemptElapsed)}
            </div>
          </div>
        </div>
        
        <div class="space-y-4">
          <!-- Weapon Selection -->
          <div>
            <label class="block text-xs font-bold mb-2 text-neutral-400 uppercase tracking-wide">Weapon Used</label>
            <select 
              bind:value={selectedWeaponId} 
              class="w-full bg-black text-white p-3 border border-neutral-700 focus:border-amber-700 focus:outline-none transition-colors"
            >
              <option value="">Select a weapon...</option>
              {#each weapons as weapon}
                <option value={weapon.id}>{weapon.name}</option>
              {/each}
            </select>
          </div>

          <!-- Notes -->
          <div>
            <label class="block text-xs font-bold mb-2 text-neutral-400 uppercase tracking-wide">Notes</label>
            <textarea 
              bind:value={notes}
              placeholder="What happened? What did you learn? Any strategies that worked?"
              rows="3"
              class="w-full bg-black text-white p-3 border border-neutral-700 focus:border-amber-700 focus:outline-none transition-colors resize-none"
            ></textarea>
          </div>

          <!-- Victory Checkbox -->
          <label class="flex items-center gap-3 cursor-pointer bg-neutral-950 p-4 border border-neutral-800 hover:border-amber-800 transition-colors">
            <input 
              type="checkbox" 
              bind:checked={isVictory}
              class="w-5 h-5 accent-amber-700 cursor-pointer"
            />
            <span class="text-base font-semibold">{isVictory ? '🎉 Victory!' : 'Mark as Victory'}</span>
          </label>

          <!-- Buttons -->
          <div class="flex gap-3 pt-2">
            <button 
              on:click={recordAttempt}
              disabled={isRecording}
              class="flex-1 bg-amber-900 hover:bg-amber-800 disabled:bg-neutral-800 disabled:cursor-not-allowed text-white font-semibold py-4 transition-colors flex items-center justify-center gap-2"
            >
              {#if isRecording}
                <div class="w-4 h-4 border-2 border-neutral-700 border-t-white animate-spin"></div>
                <span>Recording...</span>
              {:else}
                <span>📝</span>
                <span>Record Attempt</span>
              {/if}
            </button>
            <button 
              on:click={endSession}
              class="bg-neutral-800 hover:bg-neutral-700 text-white font-semibold px-8 py-4 transition-colors"
            >
              End Session
            </button>
          </div>
        </div>
      </div>

      <!-- Attempts History -->
      {#if attempts.length > 0}
        <div class="bg-neutral-900 border border-neutral-800 p-6">
          <h2 class="text-xl font-bold text-white mb-4 pb-4 border-b border-neutral-800 uppercase tracking-wide">
            Session History ({attempts.length} {attempts.length === 1 ? 'attempt' : 'attempts'})
          </h2>
          <div class="space-y-2">
            {#each attempts.slice().reverse() as attempt, i}
              <div class="bg-neutral-950 p-4 border border-neutral-800 hover:border-neutral-700 transition-colors">
                <div class="flex justify-between items-start mb-2">
                  <div class="flex-1">
                    <div class="flex items-center gap-3 mb-1">
                      <span class="font-bold text-amber-500 text-sm">Attempt #{attempts.length - i}</span>
                      <span class="text-neutral-600 text-xs">{formatTimestamp(attempt.timestamp)}</span>
                    </div>
                    <div class="text-sm text-neutral-300">
                      <span class="font-medium">{attempt.weaponName || weapons.find(w => w.id === attempt.weaponId)?.name || 'Unknown Weapon'}</span>
                      <span class="text-neutral-700 mx-2">•</span>
                      <span class="text-neutral-500">{formatTime(attempt.timeSpentSeconds)}</span>
                    </div>
                  </div>
                  {#if attempt.victory}
                    <span class="bg-green-900 border border-green-700 text-green-300 px-3 py-1 text-xs font-bold whitespace-nowrap">
                      ✓ VICTORY
                    </span>
                  {:else}
                    <span class="bg-red-900 border border-red-700 text-red-300 px-3 py-1 text-xs font-bold whitespace-nowrap">
                      ✗ Defeat
                    </span>
                  {/if}
                </div>
                {#if attempt.notes}
                  <p class="text-neutral-400 text-sm mt-3 pl-3 border-l-2 border-neutral-700 italic">
                    "{attempt.notes}"
                  </p>
                {/if}
              </div>
            {/each}
          </div>
        </div>
      {:else}
        <div class="bg-neutral-900 border border-neutral-800 p-8 text-center">
          <p class="text-neutral-500 mb-1">No attempts recorded yet</p>
          <p class="text-neutral-600 text-sm">Start fighting and record your attempts above!</p>
        </div>
      {/if}
    </div>
  {/if}
</div>

<!-- Victory Modal -->
{#if showVictoryModal}
  <div class="fixed inset-0 bg-black/95 flex items-center justify-center p-4 z-50">
    <div class="bg-neutral-900 border-2 border-amber-700 p-8 max-w-md w-full">
      <div class="text-center mb-6">
        <div class="text-6xl mb-4">🎉</div>
        <h2 class="text-3xl font-bold text-amber-500 mb-2 uppercase tracking-wide">Victory!</h2>
        <p class="text-neutral-400">Boss marked as defeated!</p>
      </div>
      
      <div class="space-y-3">
        <button
          on:click={continueAfterVictory}
          class="w-full bg-amber-900 hover:bg-amber-800 text-white font-semibold py-4 transition-colors"
        >
          Continue Fighting
        </button>
        <button
          on:click={returnToDashboard}
          class="w-full bg-neutral-800 hover:bg-neutral-700 text-white font-semibold py-4 transition-colors"
        >
          Return to Dashboard
        </button>
      </div>
    </div>
  </div>
{/if}