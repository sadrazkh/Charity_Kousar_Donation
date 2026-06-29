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
  { key: 'total', icon: '💰', label: t('totalCollected'), value: fmt(stats.value.totalCollected), accent: '#22c55e' },
  { key: 'today', icon: '📈', label: t('todayCollected'), value: fmt(stats.value.todayCollected), accent: '#0ea5e9' },
  { key: 'donors', icon: '🤝', label: t('donors'), value: stats.value.totalDonors, accent: '#f59e0b' },
  { key: 'active', icon: '📁', label: locale.value === 'fa' ? 'پروژه‌های فعال' : 'Active projects', value: stats.value.activeCampaigns, accent: '#8b5cf6' },
  { key: 'pending', icon: '⏳', label: t('pending'), value: stats.value.pendingDonations, accent: '#ef4444' }
] : [])

const topCampaigns = computed(() =>
  [...campaigns.value].sort((a, b) => b.collectedAmount - a.collectedAmount).slice(0, 5))
</script>

<template>
  <div v-if="stats" class="dashboard">
    <div class="dash-head">
      <div>
        <h1>{{ t('dashboard') }}</h1>
        <p class="sub">{{ locale === 'fa' ? 'نمای کلی فعالیت‌های خیریه' : 'Overview of charity activity' }}</p>
      </div>
      <div class="quick-actions">
        <router-link to="/admin/campaigns/new" class="btn btn-primary btn-sm">+ {{ locale === 'fa' ? 'پروژه جدید' : 'New project' }}</router-link>
        <a href="/" target="_blank" class="btn btn-ghost btn-sm">🌐 {{ locale === 'fa' ? 'مشاهده سایت' : 'View site' }}</a>
      </div>
    </div>

    <div class="stat-grid">
      <div v-for="c in cards" :key="c.key" class="card stat-card" :style="{ '--c': c.accent }">
        <div class="stat-icon">{{ c.icon }}</div>
        <div class="stat-body">
          <span class="stat-label">{{ c.label }}</span>
          <p class="stat-num">{{ c.value }}</p>
        </div>
      </div>
    </div>

    <transition-group v-if="liveFeed.length" name="feed" tag="div" class="live-feed">
      <div v-for="ev in liveFeed" :key="ev._id" class="card live-row">
        🎉 {{ locale === 'fa' ? 'کمک جدید' : 'New donation' }}:
        <strong>{{ fmt(ev.amount) }}</strong> — {{ ev.campaignTitle }}
        <span class="phone">{{ ev.phone }}</span>
      </div>
    </transition-group>

    <section class="card top-section">
      <div class="ts-head">
        <h2>{{ locale === 'fa' ? '🏆 پروژه‌های برتر' : '🏆 Top projects' }}</h2>
        <router-link to="/admin/campaigns" class="see-all">{{ locale === 'fa' ? 'مدیریت همه ←' : 'Manage all →' }}</router-link>
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
  display: flex; align-items: center; justify-content: center; font-size: 1.4rem;
  background: color-mix(in srgb, var(--c) 16%, transparent);
}
.stat-label { color: var(--muted); font-size: 0.82rem; }
.stat-num { font-size: 1.6rem; font-weight: 800; font-variant-numeric: tabular-nums; margin-top: 0.15rem; color: var(--text); }

.live-feed { display: flex; flex-direction: column; gap: 0.5rem; margin-top: 1.25rem; }
.live-row { padding: 0.75rem 1rem; border-inline-start: 4px solid var(--accent); font-size: 0.92rem; }
.live-row .phone { color: var(--muted); font-size: 0.8rem; margin-inline-start: 0.5rem; }
.feed-enter-active { transition: all 0.3s ease; }
.feed-enter-from { opacity: 0; transform: translateY(-8px); }

.top-section { margin-top: 1.5rem; }
.ts-head { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem; }
.ts-head h2 { font-size: 1.05rem; }
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
