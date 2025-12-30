<script lang="ts">
  import { authStore } from '$lib/stores/auth';
  import { goto } from '$app/navigation';

  let username = '';
  let email = '';
  let password = '';
  let confirmPassword = '';
  let psnId = '';
  let error = '';
  let isLoading = false;

  

  async function handleRegister() {
    error = '';

    // Validation
    if (password !== confirmPassword) {
      error = 'Passwords do not match';
      return;
    }

    if (password.length < 6) {
      error = 'Password must be at least 6 characters';
      return;
    }

    isLoading = true;

    const result = await authStore.register(username, email, password, psnId || undefined);

    if (result.success) {
      goto('/dashboard');
    } else {
      error = result.error || 'Registration failed';
      isLoading = false;
    }
  }
</script>

<svelte:head>
  <title>Register - Elden Ring Simulator</title>
</svelte:head>

<div class="min-h-screen bg-black relative overflow-hidden flex items-center justify-center p-4 py-12">
  <!-- Animated Background -->
  <div class="absolute inset-0 opacity-30">
    <div class="absolute inset-0 bg-gradient-to-br from-amber-950 via-gray-900 to-black"></div>
    <div class="absolute top-0 left-1/4 w-96 h-96 bg-amber-600/20 rounded-full blur-3xl animate-pulse"></div>
    <div class="absolute bottom-0 right-1/4 w-96 h-96 bg-orange-600/20 rounded-full blur-3xl animate-pulse" style="animation-delay: 2s;"></div>
  </div>

  <div class="relative max-w-xl w-full">
    <!-- Header -->
    <div class="text-center mb-10">
      <div class="inline-block w-16 h-16 bg-gradient-to-br from-amber-500 to-orange-600 rounded-xl flex items-center justify-center shadow-2xl shadow-amber-500/50 mb-4">
        <span class="text-3xl">⚔️</span>
      </div>
      <h1 class="text-5xl font-bold bg-gradient-to-r from-amber-400 via-orange-400 to-amber-500 bg-clip-text text-transparent mb-2">
        Elden Ring Simulator
      </h1>
      <p class="text-gray-300 text-lg">Begin your journey through the Lands Between</p>
    </div>

    <!-- Register Card -->
    <div class="bg-gradient-to-br from-gray-900/90 to-gray-800/90 rounded-2xl shadow-2xl p-10 border-2 border-amber-700/40 backdrop-blur-sm">
      <h2 class="text-2xl font-bold text-white mb-8">Create Your Account</h2>
      
      <form on:submit|preventDefault={handleRegister} class="space-y-6">
        <!-- Username -->
        <div>
          <label for="username" class="block text-sm font-medium text-gray-300 mb-2">
            Username *
          </label>
          <input
            id="username"
            type="text"
            bind:value={username}
            required
            class="w-full px-4 py-3 bg-gray-800/50 border-2 border-gray-700 rounded-xl text-white placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-amber-500 focus:border-transparent transition-all"
            placeholder="Choose your username"
          />
        </div>

        <!-- Email -->
        <div>
          <label for="email" class="block text-sm font-medium text-gray-300 mb-2">
            Email *
          </label>
          <input
            id="email"
            type="email"
            bind:value={email}
            required
            class="w-full px-4 py-3 bg-gray-800/50 border-2 border-gray-700 rounded-xl text-white placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-amber-500 focus:border-transparent transition-all"
            placeholder="Enter your email"
          />
        </div>

        <!-- Password -->
        <div>
          <label for="password" class="block text-sm font-medium text-gray-300 mb-2">
            Password *
          </label>
          <input
            id="password"
            type="password"
            bind:value={password}
            required
            class="w-full px-4 py-3 bg-gray-800/50 border-2 border-gray-700 rounded-xl text-white placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-amber-500 focus:border-transparent transition-all"
            placeholder="Create a password (min 6 characters)"
          />
        </div>

        <!-- Confirm Password -->
        <div>
          <label for="confirmPassword" class="block text-sm font-medium text-gray-300 mb-2">
            Confirm Password *
          </label>
          <input
            id="confirmPassword"
            type="password"
            bind:value={confirmPassword}
            required
            class="w-full px-4 py-3 bg-gray-800/50 border-2 border-gray-700 rounded-xl text-white placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-amber-500 focus:border-transparent transition-all"
            placeholder="Confirm your password"
          />
        </div>

        <!-- PSN ID (Optional) -->
        <div>
          <label for="psnId" class="block text-sm font-medium text-gray-300 mb-2">
            PSN ID <span class="text-gray-500">(optional)</span>
          </label>
          <input
            id="psnId"
            type="text"
            bind:value={psnId}
            class="w-full px-4 py-3 bg-gray-800/50 border-2 border-gray-700 rounded-xl text-white placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-amber-500 focus:border-transparent transition-all"
            placeholder="Your PlayStation Network ID"
          />
        </div>


        <!-- Error Message -->
        {#if error}
          <div class="bg-red-950/50 border-2 border-red-600/50 text-red-300 px-4 py-3 rounded-xl">
            <div class="flex items-center gap-2">
              <span class="text-lg">⚠️</span>
              <span>{error}</span>
            </div>
          </div>
        {/if}

        <!-- Register Button -->
        <div class="pt-2">
          <button
            type="submit"
            disabled={isLoading}
            class="w-full bg-gradient-to-r from-amber-600 to-orange-600 hover:from-amber-500 hover:to-orange-500 text-white font-bold py-4 px-4 rounded-xl transition-all shadow-lg hover:shadow-amber-500/50 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isLoading ? 'Creating Account...' : 'Create Account'}
          </button>
        </div>
      </form>

      <!-- Login Link -->
      <div class="mt-8 text-center">
        <p class="text-gray-400">
          Already have an account?
          <a href="/login" class="text-amber-400 hover:text-amber-300 font-semibold transition-colors">
            Sign in
          </a>
        </p>
      </div>
    </div>
  </div>
</div>