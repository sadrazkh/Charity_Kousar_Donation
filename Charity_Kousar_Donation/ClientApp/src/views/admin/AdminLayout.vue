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

const navItems = computed(() => [
  { to: '/admin', icon: '📊', label: t('dashboard'), exact: true },
  { to: '/admin/campaigns', icon: '📁', label: t('manageCampaigns'), exact: false },
  { to: '/admin/donations', icon: '💳', label: t('manageDonations'), exact: false },
  { to: '/admin/settings', icon: '⚙️', label: t('settings'), exact: false }
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
      <button type="button" class="topbar-btn" aria-label="Menu" @click="openMenu">☰</button>
      <span class="topbar-title">{{ t('admin') }}</span>
      <button type="button" class="topbar-btn" @click="toggleTheme">
        {{ isDark ? '☀️' : '🌙' }}
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
            <button type="button" class="topbar-btn" aria-label="Close" @click="closeMenu">✕</button>
          </div>

          <nav class="drawer-nav">
            <router-link v-for="n in navItems" :key="n.to" :to="n.to" :class="{ active: isActive(n) }" @click="closeMenu">
              <span class="nav-ic">{{ n.icon }}</span>{{ n.label }}
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
      <div class="brand"><span class="brand-mark">♥</span><h2>{{ t('admin') }}</h2></div>
      <nav>
        <router-link v-for="n in navItems" :key="n.to" :to="n.to" :class="{ active: isActive(n) }">
          <span class="nav-ic">{{ n.icon }}</span>{{ n.label }}
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
  display: flex; align-items: center; justify-content: center; font-size: 1.1rem; color: #fff;
  background: linear-gradient(135deg, var(--primary), var(--accent));
}
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
  gap: 0.6rem;
}
.nav-ic { font-size: 1.05rem; width: 1.4rem; text-align: center; }

.admin-sidebar nav a.active,
.drawer-nav a.active {
  background: color-mix(in srgb, var(--primary) 18%, transparent);
  color: var(--primary);
  font-weight: 600;
}

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
