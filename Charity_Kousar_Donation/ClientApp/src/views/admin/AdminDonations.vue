<script setup>
import { ref, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { api, downloadAuthFile } from '@/api/client'
import { formatAmount } from '@/utils/amount'

const { t, locale } = useI18n()
const donations = ref([])
const exporting = ref('')

onMounted(async () => {
  donations.value = await api('/donations/admin')
})

const fmt = (n) => formatAmount(n, locale.value)
function statusClass(s) {
  return s === 1 ? 'badge-success' : s === 0 ? 'badge-warning' : 'badge-danger'
}
function statusText(s) {
  return ['Pending', 'Paid', 'Failed', 'Cancelled'][s] || s
}

async function exportFile(type) {
  exporting.value = type
  try {
    const path = type === 'csv' ? '/donations/admin/export/csv' : '/donations/admin/export/report'
    const name = type === 'csv' ? `donations-${Date.now()}.csv` : `report-${Date.now()}.html`
    await downloadAuthFile(path, name)
  } finally {
    exporting.value = ''
  }
}
</script>

<template>
  <div>
    <div class="toolbar">
      <h1>{{ t('manageDonations') }}</h1>
      <div class="export-btns">
        <button type="button" class="btn btn-ghost btn-sm" :disabled="!!exporting" @click="exportFile('csv')">
          {{ exporting === 'csv' ? '...' : (locale === 'fa' ? '⬇ CSV' : '⬇ CSV') }}
        </button>
        <button type="button" class="btn btn-ghost btn-sm" :disabled="!!exporting" @click="exportFile('report')">
          {{ exporting === 'report' ? '...' : (locale === 'fa' ? '⬇ گزارش HTML' : '⬇ HTML report') }}
        </button>
      </div>
    </div>
    <table class="table card" style="margin-top:1rem">
      <thead>
        <tr>
          <th>{{ t('titleFa') }}</th>
          <th>{{ t('phone') }}</th>
          <th>{{ t('amount') }}</th>
          <th>Status</th>
          <th>Ref</th>
          <th>SMS</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="d in donations" :key="d.id">
          <td>{{ d.campaignTitle }}</td>
          <td>{{ d.phone }}</td>
          <td>{{ fmt(d.amount) }}</td>
          <td><span class="badge" :class="statusClass(d.status)">{{ statusText(d.status) }}</span></td>
          <td>{{ d.refId || '—' }}</td>
          <td>{{ d.smsSent ? '✓' : '—' }}</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style scoped>
.toolbar { display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 0.5rem; }
.export-btns { display: flex; gap: 0.5rem; }
</style>
