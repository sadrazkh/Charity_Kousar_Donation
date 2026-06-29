<script setup>
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { api } from '@/api/client'
import { useToast } from '@/composables/useToast'
import { formatAmount } from '@/utils/amount'
import ProgressBar from '@/components/ProgressBar.vue'

const { t, locale } = useI18n()
const toast = useToast()
const campaigns = ref([])
const search = ref('')
const busy = ref('')

const fmt = (n) => formatAmount(n, locale.value)

async function load() {
  campaigns.value = await api('/campaigns/admin/all')
}
onMounted(load)

const filtered = computed(() => {
  const q = search.value.trim().toLowerCase()
  if (!q) return campaigns.value
  return campaigns.value.filter(c =>
    (c.titleFa || '').toLowerCase().includes(q) ||
    (c.titleEn || '').toLowerCase().includes(q) ||
    (c.slug || '').toLowerCase().includes(q))
})

async function toggleActive(c) {
  busy.value = c.id
  try {
    await api(`/campaigns/${c.id}/flags`, { method: 'PATCH', body: JSON.stringify({ isActive: !c.isActive }) })
    c.isActive = !c.isActive
  } catch (e) { toast.error(e.message) } finally { busy.value = '' }
}
async function toggleFeatured(c) {
  busy.value = c.id
  try {
    await api(`/campaigns/${c.id}/flags`, { method: 'PATCH', body: JSON.stringify({ isFeatured: !c.isFeatured }) })
    c.isFeatured = !c.isFeatured
  } catch (e) { toast.error(e.message) } finally { busy.value = '' }
}

async function move(i, dir) {
  const ni = i + dir
  if (ni < 0 || ni >= campaigns.value.length) return
  const arr = [...campaigns.value]
  ;[arr[i], arr[ni]] = [arr[ni], arr[i]]
  campaigns.value = arr
  try {
    await api('/campaigns/reorder', { method: 'POST', body: JSON.stringify({ ids: arr.map(c => c.id) }) })
  } catch (e) { toast.error(e.message); await load() }
}

async function duplicate(c) {
  busy.value = c.id
  try {
    await api(`/campaigns/${c.id}/duplicate`, { method: 'POST' })
    toast.success(locale.value === 'fa' ? 'کپی شد ✓' : 'Duplicated ✓')
    await load()
  } catch (e) { toast.error(e.message) } finally { busy.value = '' }
}

async function regenLink(c) {
  try {
    const res = await api(`/campaigns/${c.id}/regenerate-short-link`, { method: 'POST' })
    c.shortUrl = res.shortUrl
    await navigator.clipboard.writeText(res.shortUrl).catch(() => {})
    toast.success(locale.value === 'fa' ? 'لینک جدید کپی شد ✓' : 'New link copied ✓')
  } catch (e) { toast.error(e.message) }
}

async function remove(c) {
  if (!confirm(locale.value === 'fa' ? `«${c.titleFa}» حذف شود؟ این عمل بازگشت‌ناپذیر است.` : `Delete "${c.titleFa}"? This cannot be undone.`)) return
  try {
    await api(`/campaigns/${c.id}`, { method: 'DELETE' })
    toast.success(locale.value === 'fa' ? 'حذف شد' : 'Deleted')
    await load()
  } catch (e) { toast.error(e.message) }
}
</script>

<template>
  <div>
    <div class="toolbar">
      <div>
        <h1>{{ t('manageCampaigns') }}</h1>
        <p class="hint">{{ locale === 'fa' ? 'فعال/ویژه کردن، جابه‌جایی ترتیب، کپی و ویرایش صفحه اختصاصی.' : 'Toggle active/featured, reorder, duplicate and edit the dedicated page.' }}</p>
      </div>
      <router-link to="/admin/campaigns/new" class="btn btn-primary">+ {{ t('add') }}</router-link>
    </div>

    <input v-model="search" class="input search" :placeholder="locale === 'fa' ? '🔍 جستجوی پروژه...' : '🔍 Search projects...'" />

    <div class="campaign-grid">
      <article v-for="(c, i) in filtered" :key="c.id" class="card campaign-row" :class="{ inactive: !c.isActive }">
        <div class="thumb" :style="c.imageUrl ? { backgroundImage: `url(${c.imageUrl})` } : null">
          <span v-if="!c.imageUrl" class="thumb-ph">♥</span>
        </div>

        <div class="info">
          <div class="title-line">
            <h3>{{ c.titleFa }}</h3>
            <span v-if="c.isFeatured" class="badge badge-warning">⭐ {{ t('featured') }}</span>
            <span class="badge" :class="c.isActive ? 'badge-success' : 'badge-danger'">
              {{ c.isActive ? (locale === 'fa' ? 'فعال' : 'Active') : (locale === 'fa' ? 'غیرفعال' : 'Inactive') }}
            </span>
          </div>
          <ProgressBar :percent="c.progressPercent" :height="8" />
          <div class="metrics">
            <span><strong>{{ fmt(c.collectedAmount) }}</strong> / {{ fmt(c.targetAmount) }} {{ t('toman') }}</span>
            <span class="dot">·</span>
            <span>{{ c.donorCount }} {{ locale === 'fa' ? 'حامی' : 'donors' }}</span>
            <a :href="c.pageUrl" target="_blank" class="link">{{ locale === 'fa' ? 'صفحه' : 'Page' }}</a>
          </div>
        </div>

        <div class="actions">
          <div class="reorder">
            <button type="button" class="icon-btn" :disabled="i === 0" :title="locale==='fa'?'بالا':'Up'" @click="move(i, -1)">↑</button>
            <button type="button" class="icon-btn" :disabled="i === filtered.length - 1" :title="locale==='fa'?'پایین':'Down'" @click="move(i, 1)">↓</button>
          </div>
          <button type="button" class="icon-btn" :class="{ on: c.isActive }" :disabled="busy === c.id"
            :title="locale==='fa'?'فعال/غیرفعال':'Active toggle'" @click="toggleActive(c)">{{ c.isActive ? '👁' : '🚫' }}</button>
          <button type="button" class="icon-btn" :class="{ on: c.isFeatured }" :disabled="busy === c.id"
            :title="locale==='fa'?'ویژه':'Featured'" @click="toggleFeatured(c)">⭐</button>
          <router-link :to="`/admin/campaigns/${c.id}/edit`" class="btn btn-primary btn-sm">{{ t('edit') }}</router-link>
          <button type="button" class="icon-btn" :disabled="busy === c.id" :title="locale==='fa'?'کپی':'Duplicate'" @click="duplicate(c)">⧉</button>
          <button type="button" class="icon-btn" :title="t('regenerateLink')" @click="regenLink(c)">🔗</button>
          <button type="button" class="icon-btn danger" :title="t('delete')" @click="remove(c)">🗑</button>
        </div>
      </article>
    </div>

    <p v-if="!filtered.length" class="empty">{{ search ? (locale==='fa'?'نتیجه‌ای یافت نشد':'No results') : t('noCampaigns') }}</p>
  </div>
</template>

<style scoped>
.toolbar { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; flex-wrap: wrap; margin-bottom: 0.75rem; }
.hint { color: var(--muted); font-size: 0.88rem; margin-top: 0.2rem; }
.search { margin-bottom: 1.25rem; }
.campaign-grid { display: flex; flex-direction: column; gap: 0.75rem; }
.campaign-row { display: flex; align-items: center; gap: 1rem; padding: 1rem; flex-wrap: wrap; }
.campaign-row.inactive { opacity: 0.65; }
.thumb {
  width: 84px; height: 64px; border-radius: 10px; flex-shrink: 0;
  background-size: cover; background-position: center;
  background-color: var(--bg-soft);
  display: flex; align-items: center; justify-content: center;
}
.thumb-ph { font-size: 1.6rem; color: color-mix(in srgb, var(--primary) 60%, var(--text)); opacity: 0.55; }
.info { flex: 1; min-width: 200px; display: flex; flex-direction: column; gap: 0.5rem; }
.title-line { display: flex; align-items: center; gap: 0.5rem; flex-wrap: wrap; }
.title-line h3 { font-size: 1rem; }
.metrics { display: flex; align-items: center; gap: 0.5rem; font-size: 0.85rem; color: var(--muted); flex-wrap: wrap; font-variant-numeric: tabular-nums; }
.metrics strong { color: var(--text); }
.metrics .dot { opacity: 0.5; }
.metrics .link { color: var(--primary); text-decoration: none; margin-inline-start: auto; }
.actions { display: flex; align-items: center; gap: 0.3rem; flex-wrap: wrap; }
.reorder { display: flex; flex-direction: column; gap: 0.15rem; }
.reorder .icon-btn { width: 28px; height: 22px; }
.icon-btn {
  width: 36px; height: 36px; border: 1px solid var(--border); border-radius: 9px;
  background: var(--input-bg); color: var(--text); cursor: pointer; font-size: 0.95rem;
  display: inline-flex; align-items: center; justify-content: center;
}
.icon-btn:hover { border-color: color-mix(in srgb, var(--primary) 40%, transparent); }
.icon-btn:disabled { opacity: 0.35; cursor: not-allowed; }
.icon-btn.on { background: color-mix(in srgb, var(--accent) 22%, transparent); border-color: var(--accent); }
.icon-btn.danger:hover { border-color: #ef4444; color: #ef4444; }
.empty { color: var(--muted); text-align: center; padding: 3rem; }
</style>
