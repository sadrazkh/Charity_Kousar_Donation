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
      <router-link to="/admin/campaigns/new" class="btn btn-primary">
        <svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M12 5v14M5 12h14"/></svg>
        {{ t('add') }}
      </router-link>
    </div>

    <div class="search-field">
      <svg class="search-ic" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="11" cy="11" r="7"/><path d="m21 21-4.3-4.3"/></svg>
      <input v-model="search" class="input search" :aria-label="locale === 'fa' ? 'جستجو' : 'Search'" :placeholder="locale === 'fa' ? 'جستجوی پروژه...' : 'Search projects...'" />
    </div>

    <div class="campaign-grid">
      <article v-for="(c, i) in filtered" :key="c.id" class="card campaign-row" :class="{ inactive: !c.isActive }">
        <div class="thumb" :style="c.imageUrl ? { backgroundImage: `url(${c.imageUrl})` } : null">
          <svg v-if="!c.imageUrl" class="thumb-ph" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M20.8 4.6a5.5 5.5 0 0 0-7.8 0L12 5.6l-1-1a5.5 5.5 0 0 0-7.8 7.8l1 1L12 21l7.8-7.6 1-1a5.5 5.5 0 0 0 0-7.8z"/></svg>
        </div>

        <div class="info">
          <div class="title-line">
            <h3>{{ c.titleFa }}</h3>
            <span v-if="c.isFeatured" class="badge badge-warning">
              <svg class="badge-ic" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M12 2l2.9 6.1 6.6.9-4.8 4.6 1.2 6.6L12 17.8 6.1 20.8l1.2-6.6L2.5 9l6.6-.9L12 2z"/></svg>
              {{ t('featured') }}
            </span>
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
            <button type="button" class="icon-btn" :disabled="i === 0" :aria-label="locale==='fa'?'بالا':'Move up'" :title="locale==='fa'?'بالا':'Up'" @click="move(i, -1)">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="m6 15 6-6 6 6"/></svg>
            </button>
            <button type="button" class="icon-btn" :disabled="i === filtered.length - 1" :aria-label="locale==='fa'?'پایین':'Move down'" :title="locale==='fa'?'پایین':'Down'" @click="move(i, 1)">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="m6 9 6 6 6-6"/></svg>
            </button>
          </div>
          <button type="button" class="icon-btn" :class="{ on: c.isActive }" :disabled="busy === c.id"
            :aria-label="locale==='fa'?'فعال/غیرفعال':'Toggle active'" :title="locale==='fa'?'فعال/غیرفعال':'Active toggle'" @click="toggleActive(c)">
            <svg v-if="c.isActive" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7-10-7-10-7z"/><circle cx="12" cy="12" r="3"/></svg>
            <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M9.9 4.2A10 10 0 0 1 12 4c6.5 0 10 8 10 8a15 15 0 0 1-2.9 3.7M6.6 6.6A15 15 0 0 0 2 12s3.5 7 10 7a10 10 0 0 0 4.4-1M3 3l18 18"/></svg>
          </button>
          <button type="button" class="icon-btn" :class="{ on: c.isFeatured }" :disabled="busy === c.id"
            :aria-label="locale==='fa'?'ویژه':'Featured'" :title="locale==='fa'?'ویژه':'Featured'" @click="toggleFeatured(c)">
            <svg viewBox="0 0 24 24" :fill="c.isFeatured ? 'currentColor' : 'none'" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 2l2.9 6.1 6.6.9-4.8 4.6 1.2 6.6L12 17.8 6.1 20.8l1.2-6.6L2.5 9l6.6-.9L12 2z"/></svg>
          </button>
          <router-link :to="`/admin/campaigns/${c.id}/edit`" class="btn btn-primary btn-sm">{{ t('edit') }}</router-link>
          <button type="button" class="icon-btn" :disabled="busy === c.id" :aria-label="locale==='fa'?'کپی':'Duplicate'" :title="locale==='fa'?'کپی':'Duplicate'" @click="duplicate(c)">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="9" y="9" width="12" height="12" rx="2"/><path d="M5 15V5a2 2 0 0 1 2-2h10"/></svg>
          </button>
          <button type="button" class="icon-btn" :aria-label="t('regenerateLink')" :title="t('regenerateLink')" @click="regenLink(c)">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M10 13a5 5 0 0 0 7 0l3-3a5 5 0 0 0-7-7l-1 1M14 11a5 5 0 0 0-7 0l-3 3a5 5 0 0 0 7 7l1-1"/></svg>
          </button>
          <button type="button" class="icon-btn danger" :aria-label="t('delete')" :title="t('delete')" @click="remove(c)">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M3 6h18M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2M6 6l1 14a2 2 0 0 0 2 2h6a2 2 0 0 0 2-2l1-14"/></svg>
          </button>
        </div>
      </article>
    </div>

    <p v-if="!filtered.length" class="empty">{{ search ? (locale==='fa'?'نتیجه‌ای یافت نشد':'No results') : t('noCampaigns') }}</p>
  </div>
</template>

<style scoped>
.toolbar { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; flex-wrap: wrap; margin-bottom: 0.75rem; }
.hint { color: var(--muted); font-size: 0.88rem; margin-top: 0.2rem; }
.search-field { position: relative; display: flex; align-items: center; margin-bottom: 1.25rem; }
.search-ic { position: absolute; inset-inline-start: 0.75rem; width: 18px; height: 18px; color: var(--muted); pointer-events: none; }
.search { padding-inline-start: 2.4rem; }
.badge-ic { width: 0.85rem; height: 0.85rem; vertical-align: -0.1em; margin-inline-end: 0.15rem; }
.badge { display: inline-flex; align-items: center; }
.campaign-grid { display: flex; flex-direction: column; gap: 0.75rem; }
.campaign-row { display: flex; align-items: center; gap: 1rem; padding: 1rem; flex-wrap: wrap; }
.campaign-row.inactive { opacity: 0.65; }
.thumb {
  width: 84px; height: 64px; border-radius: 10px; flex-shrink: 0;
  background-size: cover; background-position: center;
  background-color: var(--bg-soft);
  display: flex; align-items: center; justify-content: center;
}
.thumb-ph { width: 30px; height: 30px; color: color-mix(in srgb, var(--primary) 65%, var(--text)); opacity: 0.5; }
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
  background: var(--input-bg); color: var(--text); cursor: pointer;
  display: inline-flex; align-items: center; justify-content: center;
  transition: border-color 0.15s, color 0.15s, background 0.15s;
}
.icon-btn svg { width: 18px; height: 18px; }
.reorder .icon-btn svg { width: 16px; height: 16px; }
.icon-btn:hover { border-color: color-mix(in srgb, var(--primary) 40%, transparent); }
.icon-btn:disabled { opacity: 0.35; cursor: not-allowed; }
.icon-btn.on { background: color-mix(in srgb, var(--accent) 22%, transparent); border-color: var(--accent); color: var(--accent); }
.icon-btn.danger:hover { border-color: var(--danger); color: var(--danger); }
.empty { color: var(--muted); text-align: center; padding: 3rem; }
</style>
