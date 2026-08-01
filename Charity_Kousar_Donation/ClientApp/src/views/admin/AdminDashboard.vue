<script setup>
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { api, getToken } from '@/api/client'
import { formatAmount } from '@/utils/amount'
import ProgressBar from '@/components/ProgressBar.vue'
import * as signalR from '@microsoft/signalr'

const { t, locale } = useI18n()
const stats = ref(null)
const campaigns = ref([])
const liveFeed = ref([])
let connection = null

onMounted(async () => {
  try {
    const [s, list] = await Promise.all([
      api('/donations/admin/dashboard'),
      api('/campaigns/admin/all')
    ])
    stats.value = s
    campaigns.value = list
  } catch { /* */ }

  const token = getToken()
  if (token) {
    connection = new signalR.HubConnectionBuilder()
      .withUrl(`/hubs/donations?access_token=${encodeURIComponent(token)}`)
      .withAutomaticReconnect()
      .build()
    connection.on('DonationPaid', (data) => {
      liveFeed.value.unshift({ ...data, _id: Math.random() })
      liveFeed.value = liveFeed.value.slice(0, 5)
      if (stats.value) {
        stats.value.todayCollected = (stats.value.todayCollected || 0) + data.amount
        stats.value.totalCollected = (stats.value.totalCollected || 0) + data.amount
      }
    })
    connection.start().then(() => connection.invoke('JoinAdmin')).catch(() => {})
  }
})

onUnmounted(() => connection?.stop())

const fmt = (n) => formatAmount(n, locale.value)

const cards = computed(() => stats.value ? [
  { key: 'total', icon: 'M12 1v22M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6', label: t('totalCollected'), value: fmt(stats.value.totalCollected), accent: '#22c55e' },
  { key: 'today', icon: 'M3 17l6-6 4 4 8-8M15 7h6v6', label: t('todayCollected'), value: fmt(stats.value.todayCollected), accent: '#0ea5e9' },
  { key: 'donors', icon: 'M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2M9 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8M23 21v-2a4 4 0 0 0-3-3.9M16 3.1a4 4 0 0 1 0 7.8', label: t('donors'), value: stats.value.totalDonors, accent: '#f59e0b' },
  { key: 'active', icon: 'M3 7a2 2 0 0 1 2-2h4l2 2h8a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z', label: t('ui.activeProjects'), value: stats.value.activeCampaigns, accent: '#8b5cf6' },
  { key: 'pending', icon: 'M12 7v5l3 2M12 21a9 9 0 1 0 0-18 9 9 0 0 0 0 18z', label: t('pending'), value: stats.value.pendingDonations, accent: '#ef4444' }
] : [])

const topCampaigns = computed(() =>
  [...campaigns.value].sort((a, b) => b.collectedAmount - a.collectedAmount).slice(0, 5))
</script>

<template>
  <div v-if="stats" class="dashboard">
    <div class="dash-head">
      <div>
        <h1>{{ t('dashboard') }}</h1>
        <p class="sub">{{ t('ui.overviewOfCharityActivity') }}</p>
      </div>
      <div class="quick-actions">
        <router-link to="/admin/campaigns/new" class="btn btn-primary btn-sm">
          <svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M12 5v14M5 12h14"/></svg>
          {{ t('ui.newProject') }}
        </router-link>
        <a href="/" target="_blank" rel="noopener" class="btn btn-ghost btn-sm">
          <svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="9"/><path d="M3 12h18M12 3a14 14 0 0 1 0 18M12 3a14 14 0 0 0 0 18"/></svg>
          {{ t('ui.viewSite') }}
        </a>
      </div>
    </div>

    <div class="stat-grid">
      <div v-for="c in cards" :key="c.key" class="card stat-card" :style="{ '--c': c.accent }">
        <div class="stat-icon" aria-hidden="true">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path :d="c.icon"/></svg>
        </div>
        <div class="stat-body">
          <span class="stat-label">{{ c.label }}</span>
          <p class="stat-num">{{ c.value }}</p>
        </div>
      </div>
    </div>

    <transition-group v-if="liveFeed.length" name="feed" tag="div" class="live-feed">
      <div v-for="ev in liveFeed" :key="ev._id" class="card live-row">
        <svg class="live-ic" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M20 12v9H4v-9M2 7h20v5H2zM12 21V7M12 7a2.5 2.5 0 1 1 3-4 2.5 2.5 0 0 1-3 4M12 7a2.5 2.5 0 1 0-3-4 2.5 2.5 0 0 0 3 4"/></svg>
        <span>{{ t('ui.newDonation') }}:
        <strong>{{ fmt(ev.amount) }}</strong> — {{ ev.campaignTitle }}
        <span class="phone">{{ ev.phone }}</span></span>
      </div>
    </transition-group>

    <section class="card top-section">
      <div class="ts-head">
        <h2>
          <svg class="ts-ic" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M8 21h8M12 17v4M7 4h10v4a5 5 0 0 1-10 0zM7 4H4v2a3 3 0 0 0 3 3M17 4h3v2a3 3 0 0 1-3 3"/></svg>
          {{ t('ui.topProjects') }}
        </h2>
        <router-link to="/admin/campaigns" class="see-all">{{ t('ui.manageAll') }}</router-link>
      </div>
      <div v-if="topCampaigns.length" class="top-list">
        <router-link v-for="c in topCampaigns" :key="c.id" :to="`/admin/campaigns/${c.id}/edit`" class="top-row">
          <span class="top-title">{{ c.titleFa }}</span>
          <div class="top-prog">
            <ProgressBar :percent="c.progressPercent" :height="8" :show-percent="false" />
          </div>
          <span class="top-amount">{{ fmt(c.collectedAmount) }} / {{ fmt(c.targetAmount) }}</span>
        </router-link>
      </div>
      <p v-else class="empty">{{ t('noCampaigns') }}</p>
    </section>
  </div>
</template>

<style scoped>
.dash-head { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; flex-wrap: wrap; }
h1 { margin-bottom: 0.2rem; }
.sub { color: var(--muted); font-size: 0.9rem; }
.quick-actions { display: flex; gap: 0.5rem; flex-wrap: wrap; }

.stat-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 1rem; margin-top: 1.5rem; }
.stat-card { display: flex; align-items: center; gap: 0.9rem; padding: 1.1rem 1.25rem; border-inline-start: 4px solid var(--c); }
.stat-icon {
  width: 46px; height: 46px; border-radius: 12px; flex-shrink: 0;
  display: flex; align-items: center; justify-content: center;
  color: var(--c);
  background: color-mix(in srgb, var(--c) 16%, transparent);
}
.stat-icon svg { width: 24px; height: 24px; }
.stat-label { color: var(--muted); font-size: 0.82rem; }
.stat-num { font-size: 1.6rem; font-weight: 800; font-variant-numeric: tabular-nums; margin-top: 0.15rem; color: var(--text); }

.live-feed { display: flex; flex-direction: column; gap: 0.5rem; margin-top: 1.25rem; }
.live-row { display: flex; align-items: center; gap: 0.6rem; padding: 0.75rem 1rem; border-inline-start: 4px solid var(--accent); font-size: 0.92rem; }
.live-ic { width: 20px; height: 20px; flex-shrink: 0; color: var(--accent); }
.live-row .phone { color: var(--muted); font-size: 0.8rem; margin-inline-start: 0.5rem; }
.feed-enter-active { transition: all 0.3s ease; }
.feed-enter-from { opacity: 0; transform: translateY(-8px); }

.top-section { margin-top: 1.5rem; }
.ts-head { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem; }
.ts-head h2 { font-size: 1.05rem; display: flex; align-items: center; gap: 0.5rem; }
.ts-ic { width: 1.2rem; height: 1.2rem; color: var(--accent); }
.see-all { font-size: 0.85rem; color: var(--primary); text-decoration: none; }
.top-list { display: flex; flex-direction: column; }
.top-row {
  display: grid; grid-template-columns: 1.4fr 1fr 1.2fr; align-items: center; gap: 1rem;
  padding: 0.7rem 0; border-bottom: 1px solid color-mix(in srgb, var(--muted) 12%, transparent);
  text-decoration: none; color: var(--text);
}
.top-row:last-child { border-bottom: none; }
.top-row:hover { background: color-mix(in srgb, var(--primary) 6%, transparent); }
.top-title { font-weight: 600; font-size: 0.92rem; }
.top-amount { text-align: end; font-size: 0.85rem; color: var(--muted); font-variant-numeric: tabular-nums; }
.empty { color: var(--muted); text-align: center; padding: 2rem; }
@media (max-width: 600px) {
  .top-row { grid-template-columns: 1fr; gap: 0.4rem; }
  .top-amount { text-align: start; }
}
</style>
