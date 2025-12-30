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

<div class="min-h-screen bg-gradient-to-br from-gray-900 via-amber-900 to-gray-900 flex items-center justify-center p-4">
  <div class="max-w-md w-full">
    <!-- Header -->
    <div class="text-center mb-8">
      <h1 class="text-4xl font-bold text-amber-400 mb-2">Elden Ring Simulator</h1>
      <p class="text-gray-300">Sign in to your account</p>
    </div>

    <!-- Login Card -->
    <div class="bg-gray-800 rounded-lg shadow-2xl p-8 border border-amber-700">
      <form on:submit|preventDefault={handleLogin} class="space-y-6">
        <!-- Email/Username Input -->
        <div>
          <label for="emailOrUsername" class="block text-sm font-medium text-gray-300 mb-2">
            Email or Username
          </label>
          <input
            id="emailOrUsername"
            type="text"
            bind:value={emailOrUsername}
            required
            class="w-full px-4 py-3 bg-gray-700 border border-gray-600 rounded-lg text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-amber-500 focus:border-transparent"
            placeholder="Enter your email or username"
          />
        </div>

        <!-- Password Input -->
        <div>
          <label for="password" class="block text-sm font-medium text-gray-300 mb-2">
            Password
          </label>
          <input
            id="password"
            type="password"
            bind:value={password}
            required
            class="w-full px-4 py-3 bg-gray-700 border border-gray-600 rounded-lg text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-amber-500 focus:border-transparent"
            placeholder="Enter your password"
          />
        </div>

        <!-- Error Message -->
        {#if error}
          <div class="bg-red-900/50 border border-red-700 text-red-300 px-4 py-3 rounded-lg">
            {error}
          </div>
        {/if}

        <!-- Login Button -->
        <button
          type="submit"
          disabled={isLoading}
          class="w-full bg-amber-600 hover:bg-amber-700 text-white font-bold py-3 px-4 rounded-lg transition duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {isLoading ? 'Signing in...' : 'Sign In'}
        </button>
      </form>

      <!-- Register Link -->
      <div class="mt-6 text-center">
        <p class="text-gray-400">
          Don't have an account?
          <a href="/register" class="text-amber-400 hover:text-amber-300 font-semibold">
            Create one
          </a>
        </p>
      </div>
    </div>
  </div>
</div>