<script setup>
import { ref, onMounted, computed, nextTick } from 'vue'
import { useI18n } from 'vue-i18n'
import { api } from '@/api/client'
import { useToast } from '@/composables/useToast'
import { progressFillStyle } from '@/utils/progress'
import ImageUpload from '@/components/ImageUpload.vue'

const { t, locale } = useI18n()
const toast = useToast()
const groups = ref([])
const values = ref({})
const activeGroup = ref('site')
const translating = ref('')

const HOME_SECTIONS = [
  { id: 'hero', fa: 'متن و مجموع کمک‌ها', en: 'Hero & total' },
  { id: 'featured', fa: 'پروژه‌های ویژه', en: 'Featured projects' },
  { id: 'campaigns', fa: 'لیست پروژه‌ها', en: 'Campaigns list' },
  { id: 'donors', fa: 'مشارکت‌کنندگان اخیر', en: 'Recent contributors' }
]
const TIMER_UNITS = [
  { id: 'days', fa: 'روز', en: 'Days' },
  { id: 'hours', fa: 'ساعت', en: 'Hours' },
  { id: 'minutes', fa: 'دقیقه', en: 'Minutes' },
  { id: 'seconds', fa: 'ثانیه', en: 'Seconds' }
]

const manualRows = ref([])

onMounted(async () => {
  groups.value = await api('/settings')
  for (const g of groups.value) {
    for (const item of g.items) values.value[item.key] = item.value
  }
  if (groups.value.length) activeGroup.value = groups.value[0].group

  // Parse manual contributors list into editable rows.
  try {
    const parsed = JSON.parse(values.value['donors.manual'] || '[]')
    manualRows.value = Array.isArray(parsed) ? parsed : []
  } catch { manualRows.value = [] }

  await nextTick()
  const observer = new IntersectionObserver(
    (entries) => {
      const visible = entries.filter(e => e.isIntersecting).sort((a, b) => b.intersectionRatio - a.intersectionRatio)[0]
      if (visible) activeGroup.value = visible.target.id.replace('settings-', '')
    },
    { rootMargin: '-15% 0px -55% 0px', threshold: [0, 0.25, 0.5] }
  )
  for (const g of groups.value) {
    const el = document.getElementById(`settings-${g.group}`)
    if (el) observer.observe(el)
  }
})

function label(item) {
  return locale.value === 'fa' ? item.labelFa : item.labelEn
}

function scrollToGroup(id) {
  activeGroup.value = id
  nextTick(() => {
    document.getElementById(`settings-${id}`)?.scrollIntoView({ behavior: 'smooth', block: 'start' })
  })
}

async function save() {
  try {
    await api('/settings', { method: 'PUT', body: JSON.stringify({ settings: values.value }) })
    toast.success(t('savedToast'))
  } catch (e) {
    toast.error(e.message)
  }
}

/* ---- AI translation (FA -> EN) ---- */
function faCounterpart(key) {
  if (key.endsWith('.en')) return key.slice(0, -3) + '.fa'
  if (key.endsWith('En')) return key.slice(0, -2) + 'Fa'
  return null
}
function canTranslate(item) {
  if (!['Text', 'TextArea'].includes(item.type)) return false
  const fa = faCounterpart(item.key)
  return fa != null && values.value[fa] != null && String(values.value[fa]).trim() !== ''
}
async function translateField(item) {
  const fa = faCounterpart(item.key)
  if (!fa) return
  translating.value = item.key
  try {
    const res = await api('/ai/translate', {
      method: 'POST',
      body: JSON.stringify({ text: values.value[fa], from: 'fa', to: 'en' })
    })
    values.value[item.key] = res.translated
    toast.success(locale.value === 'fa' ? 'ترجمه شد ✓' : 'Translated ✓')
  } catch (e) {
    toast.error(e.message)
  } finally {
    translating.value = ''
  }
}

/* ---- Home section order editor ---- */
const homeOrder = computed({
  get: () => (values.value['site.home.order'] || '').split(',').map(s => s.trim()).filter(Boolean),
  set: (arr) => { values.value['site.home.order'] = arr.join(',') }
})
function sectionLabel(id) {
  const s = HOME_SECTIONS.find(x => x.id === id)
  return s ? (locale.value === 'fa' ? s.fa : s.en) : id
}
const excludedSections = computed(() => HOME_SECTIONS.filter(s => !homeOrder.value.includes(s.id)))
function moveSection(i, dir) {
  const arr = [...homeOrder.value]
  const ni = i + dir
  if (ni < 0 || ni >= arr.length) return
  ;[arr[i], arr[ni]] = [arr[ni], arr[i]]
  homeOrder.value = arr
}
function removeSection(id) { homeOrder.value = homeOrder.value.filter(s => s !== id) }
function addSection(id) { homeOrder.value = [...homeOrder.value, id] }

/* ---- Featured timer units ---- */
const timerUnits = computed({
  get: () => (values.value['featured.units'] || '').split(',').map(s => s.trim()).filter(Boolean),
  set: (arr) => {
    const ordered = TIMER_UNITS.map(u => u.id).filter(id => arr.includes(id))
    values.value['featured.units'] = ordered.join(',')
  }
})
function toggleUnit(id) {
  const set = new Set(timerUnits.value)
  set.has(id) ? set.delete(id) : set.add(id)
  timerUnits.value = [...set]
}

/* ---- Manual contributors ---- */
function syncManual() {
  values.value['donors.manual'] = JSON.stringify(
    manualRows.value
      .filter(r => r.name && String(r.name).trim())
      .map(r => ({ name: String(r.name).trim(), amount: Number(r.amount) || 0 }))
  )
}
function addManual() { manualRows.value.push({ name: '', amount: 0 }) }
function removeManual(i) { manualRows.value.splice(i, 1); syncManual() }

/* ---- Progress preview ---- */
const progressCfg = computed(() => ({
  progressMode: values.value['site.progress.mode'],
  progressColorStart: values.value['site.progress.color.start'],
  progressColorEnd: values.value['site.progress.color.end']
}))
function previewStyle(p) { return progressFillStyle(p, progressCfg.value) }
</script>

<template>
  <div class="settings-page">
    <div class="toolbar">
      <h1>{{ t('settings') }}</h1>
      <button class="btn btn-primary" @click="save">{{ t('save') }}</button>
    </div>

    <div class="settings-layout">
      <nav class="settings-nav card">
        <p class="nav-title">{{ locale === 'fa' ? 'بخش‌ها' : 'Sections' }}</p>
        <button v-for="g in groups" :key="g.group" type="button"
          class="nav-item" :class="{ active: activeGroup === g.group }"
          @click="scrollToGroup(g.group)">
          {{ locale === 'fa' ? g.groupLabelFa : g.groupLabelEn }}
        </button>
      </nav>

      <div class="settings-content">
        <section v-for="g in groups" :key="g.group" :id="`settings-${g.group}`" class="card settings-group">
          <h2>{{ locale === 'fa' ? g.groupLabelFa : g.groupLabelEn }}</h2>

          <p v-if="g.group === 'share'" class="section-hint">
            {{ locale === 'fa'
              ? 'متغیرهای قالب آماده: {title} {desc} {collected} {target} {progress} {link}'
              : 'Template placeholders: {title} {desc} {collected} {target} {progress} {link}' }}
          </p>
          <p v-if="g.group === 'donation'" class="section-hint">
            {{ locale === 'fa'
              ? 'مبالغ پیشنهادی را با کاما جدا کنید، مثلاً: 50000,100000,200000 — در «قالب متن مبلغ» از {collected} و {target} استفاده کنید.'
              : 'Quick amounts comma-separated, e.g. 50000,100000,200000 — in the amount format use {collected} and {target}.' }}
          </p>
          <p v-if="g.group === 'donors'" class="section-hint">
            {{ locale === 'fa'
              ? 'برای افزودن دستی حامیان، «منبع لیست» را روی دستی یا هر دو بگذارید و در لیست زیر نام و مبلغ را وارد کنید.'
              : 'To add contributors manually, set the source to manual/both and fill the list below.' }}
          </p>

          <div v-for="item in g.items" :key="item.key" class="field">
            <!-- Logo image upload -->
            <template v-if="item.key === 'site.logo.url'">
              <label class="label">{{ label(item) }}</label>
              <ImageUpload v-model="values[item.key]" />
            </template>

            <!-- Contributors source -->
            <template v-else-if="item.key === 'donors.source'">
              <label class="label">{{ label(item) }}</label>
              <select v-model="values[item.key]" class="select">
                <option value="auto">{{ locale === 'fa' ? 'خودکار (از کمک‌های واقعی)' : 'Automatic (real donations)' }}</option>
                <option value="manual">{{ locale === 'fa' ? 'دستی (فقط لیست زیر)' : 'Manual (list below only)' }}</option>
                <option value="both">{{ locale === 'fa' ? 'هر دو' : 'Both' }}</option>
              </select>
            </template>

            <!-- Manual contributors editor (add / remove) -->
            <template v-else-if="item.key === 'donors.manual'">
              <label class="label">{{ label(item) }}</label>
              <div class="manual-list">
                <div v-for="(row, i) in manualRows" :key="i" class="manual-row">
                  <input v-model="row.name" class="input" :placeholder="locale === 'fa' ? 'نام' : 'Name'" @input="syncManual" />
                  <input v-model.number="row.amount" type="number" class="input amount-in"
                    :placeholder="locale === 'fa' ? 'مبلغ (تومان)' : 'Amount'" @input="syncManual" />
                  <button type="button" class="mini danger" @click="removeManual(i)">✕</button>
                </div>
                <button type="button" class="btn btn-ghost btn-sm add-manual" @click="addManual">
                  + {{ locale === 'fa' ? 'افزودن مشارکت‌کننده' : 'Add contributor' }}
                </button>
              </div>
            </template>

            <!-- Home section order -->
            <template v-else-if="item.key === 'site.home.order'">
              <label class="label">{{ label(item) }}</label>
              <div class="order-editor">
                <div v-for="(id, i) in homeOrder" :key="id" class="order-row">
                  <span class="order-handle">≡</span>
                  <span class="order-name">{{ sectionLabel(id) }}</span>
                  <div class="order-btns">
                    <button type="button" class="mini" :disabled="i === 0" @click="moveSection(i, -1)">↑</button>
                    <button type="button" class="mini" :disabled="i === homeOrder.length - 1" @click="moveSection(i, 1)">↓</button>
                    <button type="button" class="mini danger" @click="removeSection(id)">✕</button>
                  </div>
                </div>
                <div v-if="excludedSections.length" class="order-add">
                  <span class="muted">{{ locale === 'fa' ? 'افزودن:' : 'Add:' }}</span>
                  <button v-for="s in excludedSections" :key="s.id" type="button" class="mini add"
                    @click="addSection(s.id)">+ {{ sectionLabel(s.id) }}</button>
                </div>
              </div>
            </template>

            <!-- Progress bar mode + preview -->
            <template v-else-if="item.key === 'site.progress.mode'">
              <label class="label">{{ label(item) }}</label>
              <select v-model="values[item.key]" class="select">
                <option value="shift">{{ locale === 'fa' ? 'تغییر تدریجی به سبز (پیشنهادی)' : 'Shift to green (recommended)' }}</option>
                <option value="solid">{{ locale === 'fa' ? 'تک‌رنگ ثابت' : 'Single solid color' }}</option>
                <option value="gradient">{{ locale === 'fa' ? 'گرادیان دو رنگ' : 'Two-color gradient' }}</option>
              </select>
              <div class="prog-preview">
                <div v-for="p in [25, 60, 95]" :key="p" class="pp-bar">
                  <div class="pp-fill" :style="previewStyle(p)" /><span>{{ p }}%</span>
                </div>
              </div>
            </template>

            <!-- Featured timer units -->
            <template v-else-if="item.key === 'featured.units'">
              <label class="label">{{ label(item) }}</label>
              <div class="chips">
                <button v-for="u in TIMER_UNITS" :key="u.id" type="button"
                  class="chip" :class="{ on: timerUnits.includes(u.id) }" @click="toggleUnit(u.id)">
                  {{ locale === 'fa' ? u.fa : u.en }}
                </button>
              </div>
            </template>

            <!-- Featured layout -->
            <template v-else-if="item.key === 'featured.layout'">
              <label class="label">{{ label(item) }}</label>
              <select v-model="values[item.key]" class="select">
                <option value="boxes">{{ locale === 'fa' ? 'جعبه‌ای' : 'Boxes' }}</option>
                <option value="inline">{{ locale === 'fa' ? 'خطی (00:00:00)' : 'Inline (00:00:00)' }}</option>
              </select>
            </template>

            <!-- Color -->
            <template v-else-if="item.type === 'Color'">
              <label class="label">{{ label(item) }}</label>
              <div class="color-row">
                <input type="color" v-model="values[item.key]" class="color-swatch" />
                <input type="text" v-model="values[item.key]" class="input input-ltr" dir="ltr" />
              </div>
            </template>

            <!-- Boolean -->
            <template v-else-if="item.type === 'Boolean'">
              <label class="label">{{ label(item) }}</label>
              <select v-model="values[item.key]" class="select">
                <option value="true">{{ locale === 'fa' ? 'فعال' : 'Enabled' }}</option>
                <option value="false">{{ locale === 'fa' ? 'غیرفعال' : 'Disabled' }}</option>
              </select>
            </template>

            <!-- TextArea (with translate when applicable) -->
            <template v-else-if="item.type === 'TextArea'">
              <div class="label-row">
                <label class="label">{{ label(item) }}</label>
                <button v-if="canTranslate(item)" type="button" class="translate-btn"
                  :disabled="translating === item.key" @click="translateField(item)">
                  {{ translating === item.key ? '...' : (locale === 'fa' ? '🌐 ترجمه از فارسی' : '🌐 Translate from FA') }}
                </button>
              </div>
              <textarea v-model="values[item.key]" class="textarea" rows="6" />
            </template>

            <!-- Text / Number / Password (with translate when applicable) -->
            <template v-else>
              <div class="label-row">
                <label class="label">{{ label(item) }}</label>
                <button v-if="canTranslate(item)" type="button" class="translate-btn"
                  :disabled="translating === item.key" @click="translateField(item)">
                  {{ translating === item.key ? '...' : (locale === 'fa' ? '🌐 ترجمه از فارسی' : '🌐 Translate from FA') }}
                </button>
              </div>
              <input :type="item.type === 'Password' ? 'password' : item.type === 'Number' ? 'number' : 'text'"
                v-model="values[item.key]" class="input" />
            </template>
          </div>
        </section>
      </div>
    </div>
  </div>
</template>

<style scoped>
.settings-page { padding-bottom: 2rem; }
.toolbar { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem; position: sticky; top: 0; z-index: 5; background: var(--bg); padding: 0.5rem 0; }
.settings-layout { display: grid; grid-template-columns: minmax(200px, 240px) 1fr; gap: 1rem; align-items: start; }
.settings-nav { position: sticky; top: 4rem; padding: 1rem; max-height: calc(100vh - 6rem); overflow-y: auto; }
.nav-title { font-size: 0.75rem; color: var(--muted); margin-bottom: 0.75rem; text-transform: uppercase; letter-spacing: 0.05em; }
.nav-item {
  display: block; width: 100%; text-align: start; padding: 0.55rem 0.75rem; margin-bottom: 0.25rem;
  border: none; border-radius: 8px; background: transparent; color: var(--muted); cursor: pointer; font-family: inherit; font-size: 0.88rem;
}
.nav-item.active, .nav-item:hover { background: color-mix(in srgb, var(--primary) 15%, transparent); color: var(--primary); }
.settings-group { margin-bottom: 1rem; scroll-margin-top: 5rem; }
.settings-group h2 { font-size: 1rem; margin-bottom: 1rem; color: var(--primary); }
.section-hint { font-size: 0.8rem; color: var(--muted); margin: -0.5rem 0 1rem; line-height: 1.6; }
.field { margin-bottom: 1rem; }
.label-row { display: flex; justify-content: space-between; align-items: center; gap: 0.5rem; }
.translate-btn {
  background: color-mix(in srgb, var(--accent) 16%, transparent); color: var(--accent);
  border: none; border-radius: 999px; padding: 0.2rem 0.7rem; font-size: 0.75rem; cursor: pointer; font-family: inherit;
}
.translate-btn:disabled { opacity: 0.5; cursor: progress; }
.color-row { display: flex; gap: 0.5rem; align-items: center; }
.color-swatch { width: 48px; height: 42px; border: 1px solid var(--border); border-radius: 10px; background: none; cursor: pointer; padding: 2px; }

.order-editor { display: flex; flex-direction: column; gap: 0.4rem; }
.order-row { display: flex; align-items: center; gap: 0.5rem; padding: 0.5rem 0.6rem; border: 1px solid var(--border); border-radius: 10px; background: var(--input-bg); }
.order-handle { color: var(--muted); }
.order-name { flex: 1; font-size: 0.9rem; }
.order-btns { display: flex; gap: 0.25rem; }
.mini { width: 30px; height: 30px; border: 1px solid var(--border); border-radius: 8px; background: var(--card); color: var(--text); cursor: pointer; font-size: 0.85rem; }
.mini:disabled { opacity: 0.35; cursor: not-allowed; }
.mini.danger { color: #f87171; }
.mini.add { width: auto; padding: 0 0.6rem; }
.order-add { display: flex; flex-wrap: wrap; gap: 0.4rem; align-items: center; margin-top: 0.25rem; }
.order-add .muted { color: var(--muted); font-size: 0.85rem; }

.manual-list { display: flex; flex-direction: column; gap: 0.5rem; }
.manual-row { display: flex; gap: 0.5rem; align-items: center; }
.manual-row .input { flex: 1; }
.manual-row .amount-in { max-width: 150px; }
.add-manual { align-self: flex-start; }

.chips { display: flex; flex-wrap: wrap; gap: 0.45rem; }
.chip { padding: 0.4rem 0.9rem; border-radius: 999px; border: 1px solid var(--border); background: var(--input-bg); color: var(--muted); cursor: pointer; font-family: inherit; font-size: 0.85rem; }
.chip.on { background: color-mix(in srgb, var(--primary) 18%, transparent); color: var(--primary); border-color: color-mix(in srgb, var(--primary) 45%, transparent); font-weight: 600; }

.prog-preview { display: flex; flex-direction: column; gap: 0.45rem; margin-top: 0.6rem; }
.pp-bar { display: flex; align-items: center; gap: 0.5rem; }
.pp-bar > .pp-fill { height: 10px; border-radius: 999px; }
.pp-bar { background: color-mix(in srgb, var(--muted) 18%, transparent); border-radius: 999px; padding-right: 0.5rem; }
.pp-bar span { font-size: 0.75rem; color: var(--muted); min-width: 2.4rem; }

@media (max-width: 768px) {
  .settings-layout { grid-template-columns: 1fr; }
  .settings-nav { position: static; display: flex; flex-wrap: wrap; gap: 0.35rem; max-height: none; }
  .nav-item { width: auto; flex: 1 1 calc(50% - 0.35rem); font-size: 0.78rem; padding: 0.45rem; }
}
</style>
