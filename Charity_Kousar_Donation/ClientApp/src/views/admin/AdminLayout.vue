<script setup>
import { ref, watch, onUnmounted, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { clearToken } from '@/api/client'
import { useTheme } from '@/composables/useTheme'

const router = useRouter()
const route = useRoute()
const { t } = useI18n()
const { isDark, toggleTheme } = useTheme()
const menuOpen = ref(false)

// SVG path data (stroke style) per nav item — replaces emoji icons.
const navItems = computed(() => [
  { to: '/admin', icon: 'M4 4h7v7H4zM13 4h7v5h-7zM13 13h7v7h-7zM4 15h7v5H4z', label: t('dashboard'), exact: true },
  { to: '/admin/home', icon: 'M3 10.5 12 3l9 7.5M5 9.5V21h14V9.5', label: t('homePage'), exact: false },
  { to: '/admin/campaigns', icon: 'M3 7a2 2 0 0 1 2-2h4l2 2h8a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z', label: t('manageCampaigns'), exact: false },
  { to: '/admin/media', icon: 'M3 5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2zM8.5 9.5a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3zM21 16l-5-5L5 21', label: t('mediaLibrary'), exact: false },
  { to: '/admin/donations', icon: 'M4 5h16a2 2 0 0 1 2 2v10a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V7a2 2 0 0 1 2-2zM2 9h20M6 15h4', label: t('manageDonations'), exact: false },
  { to: '/admin/settings', icon: 'M12 15a3 3 0 1 0 0-6 3 3 0 0 0 0 6zM19.4 13a1.7 1.7 0 0 0 .3 1.9l.1.1a2 2 0 1 1-2.8 2.8l-.1-.1a1.7 1.7 0 0 0-2.9 1.2V21a2 2 0 1 1-4 0v-.2a1.7 1.7 0 0 0-2.9-1.1l-.1.1a2 2 0 1 1-2.8-2.8l.1-.1a1.7 1.7 0 0 0-1.1-2.9H3a2 2 0 1 1 0-4h.2a1.7 1.7 0 0 0 1.1-2.9l-.1-.1a2 2 0 1 1 2.8-2.8l.1.1a1.7 1.7 0 0 0 1.9.3H9a1.7 1.7 0 0 0 1-1.5V3a2 2 0 1 1 4 0v.2a1.7 1.7 0 0 0 1 1.5 1.7 1.7 0 0 0 1.9-.3l.1-.1a2 2 0 1 1 2.8 2.8l-.1.1a1.7 1.7 0 0 0-.3 1.9V9a1.7 1.7 0 0 0 1.5 1H21a2 2 0 1 1 0 4h-.2a1.7 1.7 0 0 0-1.4 1z', label: t('settings'), exact: false }
])

function isActive(n) {
  return n.exact ? route.path === n.to : route.path.startsWith(n.to)
}

function logout() {
  clearToken()
  router.push('/admin/login')
}

function openMenu() {
  menuOpen.value = true
}

function closeMenu() {
  menuOpen.value = false
}

watch(menuOpen, (open) => {
  document.body.classList.toggle('admin-menu-open', open)
})

watch(() => route.path, closeMenu)

onUnmounted(() => document.body.classList.remove('admin-menu-open'))
</script>

<template>
  <div class="admin-wrap">
    <header class="admin-topbar">
      <button type="button" class="topbar-btn" aria-label="Menu" @click="openMenu">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M3 6h18M3 12h18M3 18h18"/></svg>
      </button>
      <span class="topbar-title">{{ t('admin') }}</span>
      <button type="button" class="topbar-btn" :aria-label="isDark ? t('themeLight') : t('themeDark')" @click="toggleTheme">
        <svg v-if="isDark" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="4.5"/><path d="M12 2v2M12 20v2M4 12H2M22 12h-2M5 5l1.5 1.5M17.5 17.5 19 19M19 5l-1.5 1.5M6.5 17.5 5 19"/></svg>
        <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 12.8A8.5 8.5 0 1 1 11.2 3a6.6 6.6 0 0 0 9.8 9.8z"/></svg>
      </button>
    </header>

    <main class="admin-main">
      <router-view />
    </main>

    <Teleport to="body">
      <template v-if="menuOpen">
        <div class="admin-overlay" @click="closeMenu" />
        <aside class="admin-drawer card">
          <div class="drawer-head">
            <h2>{{ t('admin') }}</h2>
            <button type="button" class="topbar-btn" aria-label="Close" @click="closeMenu">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M18 6 6 18M6 6l12 12"/></svg>
            </button>
          </div>

          <nav class="drawer-nav">
            <router-link v-for="n in navItems" :key="n.to" :to="n.to" :class="{ active: isActive(n) }" @click="closeMenu">
              <svg class="nav-ic" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path :d="n.icon"/></svg><span>{{ n.label }}</span>
            </router-link>
          </nav>

          <div class="drawer-foot">
            <button type="button" class="btn btn-ghost btn-sm" @click="toggleTheme">
              {{ isDark ? t('themeLight') : t('themeDark') }}
            </button>
            <button type="button" class="btn btn-ghost btn-sm" @click="logout">{{ t('logout') }}</button>
            <router-link to="/" class="home-link" @click="closeMenu">{{ t('backHome') }}</router-link>
          </div>
        </aside>
      </template>
    </Teleport>

    <!-- Desktop sidebar (not teleported) -->
    <aside class="admin-sidebar card">
      <div class="brand">
        <span class="brand-mark" aria-hidden="true">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20.8 4.6a5.5 5.5 0 0 0-7.8 0L12 5.6l-1-1a5.5 5.5 0 0 0-7.8 7.8l1 1L12 21l7.8-7.6 1-1a5.5 5.5 0 0 0 0-7.8z"/></svg>
        </span>
        <h2>{{ t('admin') }}</h2>
      </div>
      <nav>
        <router-link v-for="n in navItems" :key="n.to" :to="n.to" :class="{ active: isActive(n) }">
          <svg class="nav-ic" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path :d="n.icon"/></svg><span>{{ n.label }}</span>
        </router-link>
      </nav>
      <div class="drawer-foot">
        <button type="button" class="btn btn-ghost btn-sm" @click="toggleTheme">
          {{ isDark ? t('themeLight') : t('themeDark') }}
        </button>
        <button type="button" class="btn btn-ghost btn-sm" @click="logout">{{ t('logout') }}</button>
        <router-link to="/" class="home-link">{{ t('backHome') }}</router-link>
      </div>
    </aside>
  </div>
</template>

<style scoped>
.admin-wrap {
  min-height: 100vh;
  display: flex;
  gap: 1.5rem;
  padding: 1.5rem;
  position: relative;
}

.admin-topbar { display: none; }

.admin-sidebar {
  width: 260px;
  flex-shrink: 0;
  padding: 1.25rem;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  position: sticky;
  top: 1rem;
  align-self: flex-start;
  height: fit-content;
  color: var(--text);
}

.admin-sidebar h2 { font-size: 1.05rem; }
.brand { display: flex; align-items: center; gap: 0.6rem; padding-bottom: 0.5rem; }
.brand-mark {
  width: 36px; height: 36px; border-radius: 10px; flex-shrink: 0;
  display: flex; align-items: center; justify-content: center; color: #fff;
  background: linear-gradient(135deg, var(--primary), var(--accent));
}
.brand-mark svg { width: 20px; height: 20px; }
.drawer-head .brand-mark { display: none; }

.admin-sidebar nav,
.drawer-nav {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  flex: 1;
}

.admin-sidebar nav a,
.drawer-nav a {
  padding: 0.7rem 0.9rem;
  border-radius: 10px;
  color: var(--muted);
  text-decoration: none;
  min-height: 44px;
  display: flex;
  align-items: center;
  gap: 0.7rem;
  transition: background 0.15s, color 0.15s;
}
.admin-sidebar nav a:hover,
.drawer-nav a:hover { background: var(--chip-bg); color: var(--text); }
.nav-ic { width: 1.25rem; height: 1.25rem; flex-shrink: 0; }

.admin-sidebar nav a.active,
.drawer-nav a.active {
  background: color-mix(in srgb, var(--primary) 16%, transparent);
  color: var(--primary);
  font-weight: 600;
}

.topbar-btn svg { width: 22px; height: 22px; }

.drawer-foot {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  margin-top: auto;
  padding-top: 0.75rem;
  border-top: 1px solid var(--border);
}

.drawer-foot .btn { justify-content: flex-start; width: 100%; }

.home-link {
  font-size: 0.85rem;
  color: var(--muted);
  text-decoration: none;
  padding: 0.35rem 0.5rem;
}

.admin-main {
  flex: 1;
  min-width: 0;
  color: var(--text);
}

/* Mobile drawer (teleported) */
.admin-overlay {
  position: fixed;
  inset: 0;
  z-index: 3000;
  background: rgba(0, 0, 0, 0.55);
}

.admin-drawer {
  position: fixed;
  top: 0;
  right: 0;
  bottom: 0;
  width: min(300px, 88vw);
  z-index: 3001;
  padding: 1rem;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  border-radius: 0;
  overflow-y: auto;
  color: var(--text);
  box-shadow: -4px 0 24px rgba(0, 0, 0, 0.35);
  animation: slideIn 0.25s ease;
}

@keyframes slideIn {
  from { transform: translateX(100%); }
  to { transform: translateX(0); }
}

.drawer-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
}

.drawer-head h2 { font-size: 1.05rem; }

.topbar-btn {
  width: 44px;
  height: 44px;
  border: 1px solid var(--border);
  border-radius: 10px;
  background: var(--input-bg);
  color: var(--text);
  font-size: 1.15rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

@media (max-width: 768px) {
  .admin-wrap {
    flex-direction: column;
    padding: 0;
    padding-top: 56px;
    gap: 0;
  }

  .admin-sidebar { display: none; }

  .admin-topbar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    height: 56px;
    padding: 0 0.75rem;
    z-index: 2000;
    background: var(--card);
    border-bottom: 1px solid var(--border);
    backdrop-filter: blur(12px);
  }

  .topbar-title {
    font-weight: 700;
    font-size: 0.95rem;
    color: var(--text);
  }

  .admin-main { padding: 1rem; }
}
</style>
