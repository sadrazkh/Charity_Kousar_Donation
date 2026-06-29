<script setup>
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { api, downloadAuthFile } from '@/api/client'
import { formatAmount } from '@/utils/amount'

const { t, locale } = useI18n()
const donations = ref([])
const exporting = ref('')
const search = ref('')
const statusFilter = ref('all')

onMounted(async () => {
  try { donations.value = await api('/donations/admin') } catch { /* */ }
})

const fmt = (n) => formatAmount(n, locale.value)
function statusClass(s) { return s === 1 ? 'badge-success' : s === 0 ? 'badge-warning' : 'badge-danger' }
function statusText(s) {
  const fa = ['در انتظار', 'پرداخت‌شده', 'ناموفق', 'لغو شده']
  const en = ['Pending', 'Paid', 'Failed', 'Cancelled']
  return (locale.value === 'fa' ? fa : en)[s] ?? s
}
function methodText(m) {
  return m === 1 ? (locale.value === 'fa' ? 'رمزارز' : 'Crypto') : (locale.value === 'fa' ? 'زرین‌پال' : 'ZarinPal')
}
function fmtDate(d) {
  if (!d) return '—'
  try { return new Date(d).toLocaleDateString(locale.value === 'fa' ? 'fa-IR' : 'en-US', { year: '2-digit', month: 'short', day: 'numeric' }) }
  catch { return '—' }
}

const filtered = computed(() => {
  const q = search.value.trim().toLowerCase()
  return donations.value.filter(d => {
    if (statusFilter.value !== 'all' && String(d.status) !== statusFilter.value) return false
    if (!q) return true
    return (d.phone || '').toLowerCase().includes(q) ||
      (d.donorName || '').toLowerCase().includes(q) ||
      (d.campaignTitle || '').toLowerCase().includes(q)
  })
})

const summary = computed(() => {
  const paid = donations.value.filter(d => d.status === 1)
  return {
    paidAmount: paid.reduce((s, d) => s + d.amount, 0),
    paidCount: paid.length,
    pending: donations.value.filter(d => d.status === 0).length,
    total: donations.value.length
  }
})

async function exportFile(type) {
  exporting.value = type
  try {
    const path = type === 'csv' ? '/donations/admin/export/csv' : '/donations/admin/export/report'
    const name = type === 'csv' ? `donations-${Date.now()}.csv` : `report-${Date.now()}.html`
    await downloadAuthFile(path, name)
  } finally { exporting.value = '' }
}
</script>

<template>
  <div>
    <div class="toolbar">
      <h1>{{ t('manageDonations') }}</h1>
      <div class="export-btns">
        <button type="button" class="btn btn-ghost btn-sm" :disabled="!!exporting" @click="exportFile('csv')">
          {{ exporting === 'csv' ? '...' : '⬇ CSV' }}
        </button>
        <button type="button" class="btn btn-ghost btn-sm" :disabled="!!exporting" @click="exportFile('report')">
          {{ exporting === 'report' ? '...' : (locale === 'fa' ? '⬇ گزارش HTML' : '⬇ HTML report') }}
        </button>
      </div>
    </div>

    <div class="summary">
      <div class="card s-card"><span>{{ locale === 'fa' ? 'مجموع پرداخت‌شده' : 'Total paid' }}</span><strong>{{ fmt(summary.paidAmount) }}</strong></div>
      <div class="card s-card"><span>{{ locale === 'fa' ? 'تعداد پرداخت موفق' : 'Successful' }}</span><strong>{{ summary.paidCount }}</strong></div>
      <div class="card s-card"><span>{{ t('pending') }}</span><strong>{{ summary.pending }}</strong></div>
      <div class="card s-card"><span>{{ locale === 'fa' ? 'کل تراکنش‌ها' : 'Total records' }}</span><strong>{{ summary.total }}</strong></div>
    </div>

    <div class="filters">
      <input v-model="search" class="input" :placeholder="locale === 'fa' ? '🔍 جستجو نام/موبایل/پروژه' : '🔍 Search name/phone/project'" />
      <select v-model="statusFilter" class="select status-sel">
        <option value="all">{{ locale === 'fa' ? 'همه وضعیت‌ها' : 'All statuses' }}</option>
        <option value="1">{{ statusText(1) }}</option>
        <option value="0">{{ statusText(0) }}</option>
        <option value="2">{{ statusText(2) }}</option>
        <option value="3">{{ statusText(3) }}</option>
      </select>
    </div>

    <div class="table-wrap card">
      <table class="table">
        <thead>
          <tr>
            <th>{{ locale === 'fa' ? 'پروژه' : 'Project' }}</th>
            <th>{{ locale === 'fa' ? 'حامی' : 'Donor' }}</th>
            <th>{{ t('amount') }}</th>
            <th>{{ locale === 'fa' ? 'روش' : 'Method' }}</th>
            <th>{{ locale === 'fa' ? 'وضعیت' : 'Status' }}</th>
            <th>{{ locale === 'fa' ? 'تاریخ' : 'Date' }}</th>
            <th>SMS</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="d in filtered" :key="d.id">
            <td>{{ d.campaignTitle }}</td>
            <td><div class="donor"><span v-if="d.donorName">{{ d.donorName }}</span><span class="ph" dir="ltr">{{ d.phone }}</span></div></td>
            <td class="amount"><strong>{{ fmt(d.amount) }}</strong></td>
            <td>{{ methodText(d.paymentMethod) }}</td>
            <td><span class="badge" :class="statusClass(d.status)">{{ statusText(d.status) }}</span></td>
            <td>{{ fmtDate(d.paidAt || d.createdAt) }}</td>
            <td>{{ d.smsSent ? '✓' : '—' }}</td>
          </tr>
        </tbody>
      </table>
      <p v-if="!filtered.length" class="empty">{{ locale === 'fa' ? 'تراکنشی یافت نشد' : 'No records' }}</p>
    </div>
  </div>
</template>

<style scoped>
.toolbar { display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 0.5rem; }
.export-btns { display: flex; gap: 0.5rem; }
.summary { display: grid; grid-template-columns: repeat(auto-fit, minmax(150px, 1fr)); gap: 0.75rem; margin: 1.25rem 0; }
.s-card { padding: 0.85rem 1rem; display: flex; flex-direction: column; gap: 0.25rem; }
.s-card span { color: var(--muted); font-size: 0.8rem; }
.s-card strong { font-size: 1.3rem; font-variant-numeric: tabular-nums; }
.filters { display: flex; gap: 0.5rem; margin-bottom: 1rem; flex-wrap: wrap; }
.filters .input { flex: 1; min-width: 200px; }
.status-sel { max-width: 200px; }
.table-wrap { padding: 0.5rem; overflow-x: auto; }
.donor { display: flex; flex-direction: column; }
.donor .ph { color: var(--muted); font-size: 0.8rem; }
.amount strong { font-weight: 700; }
.empty { color: var(--muted); text-align: center; padding: 2rem; }
</style>
