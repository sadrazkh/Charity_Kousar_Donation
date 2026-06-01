<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { clearToken } from '@/api/client'

const router = useRouter()
const { t } = useI18n()
const menuOpen = ref(false)

function logout() {
  clearToken()
  router.push('/admin/login')
}

function closeMenu() {
  menuOpen.value = false
}
</script>

<template>
  <div class="admin-wrap">
    <button type="button" class="menu-toggle" @click="menuOpen = !menuOpen" aria-label="Menu">☰</button>
    <div v-if="menuOpen" class="overlay" @click="closeMenu" />

    <aside class="sidebar card" :class="{ open: menuOpen }">
      <h2>{{ t('admin') }}</h2>
      <nav @click="closeMenu">
        <router-link to="/admin">{{ t('dashboard') }}</router-link>
        <router-link to="/admin/campaigns">{{ t('manageCampaigns') }}</router-link>
        <router-link to="/admin/donations">{{ t('manageDonations') }}</router-link>
        <router-link to="/admin/settings">{{ t('settings') }}</router-link>
      </nav>
      <button class="btn btn-ghost btn-sm" @click="logout">{{ t('logout') }}</button>
      <router-link to="/" class="home-link" @click="closeMenu">{{ t('backHome') }}</router-link>
    </aside>

    <main class="admin-main">
      <router-view />
    </main>
  </div>
</template>

<style scoped>
.admin-wrap { display: flex; min-height: 100vh; gap: 1rem; padding: 1rem; position: relative; }
@media (min-width: 769px) { .admin-wrap { gap: 1.5rem; padding: 1.5rem; } }
.menu-toggle {
  display: none; position: fixed; top: 0.85rem; z-index: 110;
  inset-inline-start: 0.85rem; width: 44px; height: 44px;
  border-radius: 10px; border: 1px solid rgba(148,163,184,0.3);
  background: var(--card); color: var(--text); font-size: 1.25rem; cursor: pointer;
}
@media (max-width: 768px) { .menu-toggle { display: flex; align-items: center; justify-content: center; } }
.overlay { display: none; }
@media (max-width: 768px) {
  .overlay { display: block; position: fixed; inset: 0; background: rgba(0,0,0,0.5); z-index: 95; }
}
.sidebar {
  width: 240px; flex-shrink: 0; display: flex; flex-direction: column; gap: 1rem;
  padding: 1.25rem; height: fit-content; position: sticky; top: 1rem;
}
@media (max-width: 768px) {
  .sidebar {
    position: fixed; top: 0; bottom: 0; inset-inline-start: 0; z-index: 100;
    width: min(280px, 85vw); transform: translateX(-110%);
    transition: transform 0.25s ease; border-radius: 0; height: 100vh; overflow-y: auto;
  }
  [dir="rtl"] .sidebar { transform: translateX(110%); inset-inline-start: auto; inset-inline-end: 0; }
  .sidebar.open { transform: translateX(0); }
}
.sidebar nav { display: flex; flex-direction: column; gap: 0.35rem; flex: 1; }
.sidebar nav a { padding: 0.65rem 0.85rem; border-radius: 8px; color: var(--muted); text-decoration: none; min-height: 44px; display: flex; align-items: center; }
.sidebar nav a.router-link-active { background: color-mix(in srgb, var(--primary) 20%, transparent); color: var(--primary); }
.admin-main { flex: 1; min-width: 0; padding-top: 0; }
@media (max-width: 768px) { .admin-main { padding-top: 2.5rem; } }
.home-link { font-size: 0.85rem; color: var(--muted); }
</style>
