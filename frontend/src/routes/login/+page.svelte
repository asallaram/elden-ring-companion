<script lang="ts">
  import { authStore } from '$lib/stores/auth';
  import { goto } from '$app/navigation';

  let emailOrUsername = '';
  let password = '';
  let error = '';
  let isLoading = false;

  async function handleLogin() {
    error = '';
    isLoading = true;

    const result = await authStore.login(emailOrUsername, password);

    if (result.success) {
      goto('/dashboard');
    } else {
      error = result.error || 'Login failed';
      isLoading = false;
    }
  }
</script>

<svelte:head>
  <title>Login - Elden Ring Simulator</title>
</svelte:head>

<div class="min-h-screen bg-[#0a0a0a] flex items-center justify-center p-4">
  <div class="max-w-md w-full">
    <!-- Header -->
    <div class="text-center mb-8">
      <h1 class="text-4xl font-bold text-white mb-2 uppercase tracking-wide" style="color: #d4a25a;">Elden Ring</h1>
      <p class="text-neutral-500 text-sm">Sign in to your account</p>
    </div>

    <!-- Login Card -->
    <div class="bg-neutral-900 border-2 border-neutral-800 p-8">
      <form on:submit|preventDefault={handleLogin} class="space-y-6">
        <!-- Email/Username Input -->
        <div>
          <label for="emailOrUsername" class="block text-xs font-bold text-neutral-400 mb-2 uppercase tracking-wide">
            Email or Username
          </label>
          <input
            id="emailOrUsername"
            type="text"
            bind:value={emailOrUsername}
            required
            class="w-full px-4 py-3 bg-black border border-neutral-700 text-white placeholder-neutral-600 focus:outline-none focus:border-amber-700 transition-colors"
            placeholder="Enter your email or username"
          />
        </div>

        <!-- Password Input -->
        <div>
          <label for="password" class="block text-xs font-bold text-neutral-400 mb-2 uppercase tracking-wide">
            Password
          </label>
          <input
            id="password"
            type="password"
            bind:value={password}
            required
            class="w-full px-4 py-3 bg-black border border-neutral-700 text-white placeholder-neutral-600 focus:outline-none focus:border-amber-700 transition-colors"
            placeholder="Enter your password"
          />
        </div>

        <!-- Error Message -->
        {#if error}
          <div class="bg-red-950/30 border border-red-900 text-red-400 px-4 py-3">
            {error}
          </div>
        {/if}

        <!-- Login Button -->
        <button
          type="submit"
          disabled={isLoading}
          class="w-full bg-amber-900 hover:bg-amber-800 disabled:bg-neutral-800 text-white font-semibold py-3 px-4 transition-colors disabled:cursor-not-allowed"
        >
          {isLoading ? 'Signing in...' : 'Sign In'}
        </button>
      </form>

      <!-- Register Link -->
      <div class="mt-6 text-center pt-6 border-t border-neutral-800">
        <p class="text-neutral-500 text-sm">
          Don't have an account?
          <a href="/register" class="text-amber-500 hover:text-amber-400 font-semibold">
            Create one
          </a>
        </p>
      </div>
    </div>
  </div>
</div>