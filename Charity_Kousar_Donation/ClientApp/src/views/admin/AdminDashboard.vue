<script setup>
import { ref, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { api } from '@/api/client'
import { formatAmount } from '@/utils/amount'

const { t, locale } = useI18n()
const stats = ref(null)

onMounted(async () => {
  stats.value = await api('/donations/admin/dashboard')
})

const fmt = (n) => formatAmount(n, locale.value)
</script>

<template>
  <div v-if="stats">
    <h1>{{ t('dashboard') }}</h1>
    <div class="grid grid-2" style="margin-top:1.5rem">
      <div class="card stat-card"><span>{{ t('totalCollected') }}</span><p class="stat-value">{{ fmt(stats.totalCollected) }}</p></div>
      <div class="card stat-card"><span>{{ t('todayCollected') }}</span><p class="stat-value">{{ fmt(stats.todayCollected) }}</p></div>
      <div class="card stat-card"><span>{{ t('donors') }}</span><p class="stat-value">{{ stats.totalDonors }}</p></div>
      <div class="card stat-card"><span>{{ t('pending') }}</span><p class="stat-value">{{ stats.pendingDonations }}</p></div>
    </div>
  </div>
</template>

<style scoped>
h1 { margin-bottom: 0.5rem; }
.stat-card span { color: var(--muted); font-size: 0.9rem; }
.stat-card .stat-value { margin-top: 0.5rem; font-size: 1.75rem; }
</style>
