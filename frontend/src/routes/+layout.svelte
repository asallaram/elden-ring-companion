<script lang="ts">
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { page } from '$app/stores';
  import { authStore } from '$lib/stores/auth';
  import '../app.css';

  const protectedRoutes = ['/dashboard', '/bosses', '/weapons', '/boss-fight', '/profile'];

  onMount(async () => {
    console.log('=== LAYOUT MOUNT ===');
    await authStore.init();
    console.log('=== INIT COMPLETE ===');
    
    authStore.subscribe(state => {
      console.log('Auth state:', state);
      
      if (!state.isLoading && !state.isAuthenticated) {
        const currentPath = window.location.pathname;
        const isProtected = protectedRoutes.some(route => currentPath.startsWith(route));
        
        if (isProtected) {
          console.log('Redirecting to login from:', currentPath);
          goto('/login');
        }
      }
    });
  });
</script>

<slot />