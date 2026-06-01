<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { api, getToken } from '@/api/client'
import { formatAmount } from '@/utils/amount'
import * as signalR from '@microsoft/signalr'

const { t, locale } = useI18n()
const stats = ref(null)
const toast = ref(null)
let connection = null

onMounted(async () => {
  stats.value = await api('/donations/admin/dashboard')
  const token = getToken()
  if (token) {
    connection = new signalR.HubConnectionBuilder()
      .withUrl(`/hubs/donations?access_token=${encodeURIComponent(token)}`)
      .withAutomaticReconnect()
      .build()
    connection.on('DonationPaid', (data) => {
      toast.value = data
      if (stats.value) stats.value.todayCollected = (stats.value.todayCollected || 0) + data.amount
      setTimeout(() => { toast.value = null }, 8000)
    })
    connection.start().then(() => connection.invoke('JoinAdmin')).catch(() => {})
  }
})

onUnmounted(() => connection?.stop())

const fmt = (n) => formatAmount(n, locale.value)
</script>

<template>
  <div v-if="stats">
    <h1>{{ t('dashboard') }}</h1>

    <div v-if="toast" class="live-toast card">
      🎉 {{ locale === 'fa' ? 'کمک جدید' : 'New donation' }}:
      {{ fmt(toast.amount) }} — {{ toast.campaignTitle }}
    </div>

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
.live-toast {
  margin-top: 1rem; padding: 0.85rem 1rem; border-inline-start: 4px solid var(--accent);
  animation: slideIn 0.3s ease;
}
@keyframes slideIn { from { opacity: 0; transform: translateY(-8px); } to { opacity: 1; transform: none; } }
</style>
