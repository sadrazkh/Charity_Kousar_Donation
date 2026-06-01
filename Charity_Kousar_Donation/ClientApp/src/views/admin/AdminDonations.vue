<script setup>
import { ref, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { api } from '@/api/client'
import { formatAmount } from '@/utils/amount'

const { t, locale } = useI18n()
const donations = ref([])

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
</script>

<template>
  <div>
    <h1>{{ t('manageDonations') }}</h1>
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
