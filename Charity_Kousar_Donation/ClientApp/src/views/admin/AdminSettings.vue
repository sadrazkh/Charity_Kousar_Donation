<script setup>
import { ref, onMounted, computed, nextTick } from 'vue'
import { useI18n } from 'vue-i18n'
import { api } from '@/api/client'
import { useToast } from '@/composables/useToast'
import { progressFillStyle } from '@/utils/progress'
import { secKey, isKnownSection } from './home/sections'
import ImageUpload from '@/components/ImageUpload.vue'

const { t, locale } = useI18n()
const toast = useToast()
const groups = ref([])
const values = ref({})
const activeGroup = ref('site')
const translating = ref('')

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
    toast.success(t('ui.translated'))
  } catch (e) {
    toast.error(e.message)
  } finally {
    translating.value = ''
  }
}

/* ---- Home section order (shown here, edited in the home editor) ---- */
const homeOrder = computed(() =>
  (values.value['site.home.order'] || '').split(',').map(s => s.trim()).filter(Boolean))

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
        <p class="nav-title">{{ t('ui.sections') }}</p>
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
            {{ t('ui.templatePlaceholdersTitleDescCollected') }}
          </p>
          <p v-if="g.group === 'donation'" class="section-hint">
            {{ t('ui.quickAmountsCommaSeparatedIn') }}
          </p>
          <p v-if="g.group === 'donors'" class="section-hint">
            {{ t('ui.toAddContributorsManuallySet') }}
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
                <option value="auto">{{ t('ui.automaticRealDonations') }}</option>
                <option value="manual">{{ t('ui.manualListBelowOnly') }}</option>
                <option value="both">{{ t('ui.both') }}</option>
              </select>
            </template>

            <!-- Manual contributors editor (add / remove) -->
            <template v-else-if="item.key === 'donors.manual'">
              <label class="label">{{ label(item) }}</label>
              <div class="manual-list">
                <div v-for="(row, i) in manualRows" :key="i" class="manual-row">
                  <input v-model="row.name" class="input" :placeholder="t('ui.name')" @input="syncManual" />
                  <input v-model.number="row.amount" type="number" class="input amount-in"
                    :placeholder="t('ui.amount')" @input="syncManual" />
                  <button type="button" class="mini danger" @click="removeManual(i)">✕</button>
                </div>
                <button type="button" class="btn btn-ghost btn-sm add-manual" @click="addManual">
                  + {{ t('ui.addContributor') }}
                </button>
              </div>
            </template>

            <!-- Home section order — arranged by drag & drop in the home editor -->
            <template v-else-if="item.key === 'site.home.order'">
              <label class="label">{{ label(item) }}</label>
              <div class="order-readonly">
                <span v-for="id in homeOrder" :key="id" class="order-pill">
                  {{ isKnownSection(id) ? t(secKey(id)) : id }}
                </span>
                <span v-if="!homeOrder.length" class="muted">{{ t('homeEditor.noSections') }}</span>
              </div>
              <router-link to="/admin/home" class="more-link">{{ t('ui.editOrderInHomeEditor') }}</router-link>
            </template>

            <!-- Progress bar mode + preview -->
            <template v-else-if="item.key === 'site.progress.mode'">
              <label class="label">{{ label(item) }}</label>
              <select v-model="values[item.key]" class="select">
                <option value="shift">{{ t('ui.shiftToGreenRecommended') }}</option>
                <option value="solid">{{ t('ui.singleSolidColor') }}</option>
                <option value="gradient">{{ t('ui.twoColorGradient') }}</option>
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
                <option value="boxes">{{ t('ui.boxes') }}</option>
                <option value="inline">{{ t('ui.inline000000') }}</option>
              </select>
            </template>

            <!-- Color -->
            <template v-else-if="item.type === 'Color'">
              <label class="label">{{ label(item) }}</label>
              <div class="color-row">
                <input type="color" :value="values[item.key] || '#0d9488'" class="color-swatch"
                  @input="values[item.key] = $event.target.value" />
                <input type="text" v-model="values[item.key]" class="input input-ltr" dir="ltr"
                  :placeholder="t('ui.emptyThemeDefault')" />
                <button type="button" class="mini danger" :disabled="!values[item.key]"
                  :title="t('ui.resetToDefault')"
                  @click="values[item.key] = ''">✕</button>
              </div>
            </template>

            <!-- Boolean -->
            <template v-else-if="item.type === 'Boolean'">
              <label class="label">{{ label(item) }}</label>
              <select v-model="values[item.key]" class="select">
                <option value="true">{{ t('ui.enabled') }}</option>
                <option value="false">{{ t('ui.disabled') }}</option>
              </select>
            </template>

            <!-- TextArea (with translate when applicable) -->
            <template v-else-if="item.type === 'TextArea'">
              <div class="label-row">
                <label class="label">{{ label(item) }}</label>
                <button v-if="canTranslate(item)" type="button" class="translate-btn"
                  :disabled="translating === item.key" @click="translateField(item)">
                  {{ translating === item.key ? '...' : (t('ui.translateFromFa')) }}
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
                  {{ translating === item.key ? '...' : (t('ui.translateFromFa')) }}
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

.order-readonly { display: flex; flex-wrap: wrap; gap: 0.4rem; align-items: center; }
.order-pill {
  padding: 0.35rem 0.7rem; border-radius: 999px; font-size: 0.85rem;
  border: 1px solid var(--border); background: var(--input-bg); color: var(--text);
}
.order-readonly .muted { color: var(--muted); font-size: 0.85rem; }
.more-link { display: inline-block; margin-top: 0.6rem; font-size: 0.85rem; color: var(--primary); text-decoration: none; }
.mini { width: 30px; height: 30px; border: 1px solid var(--border); border-radius: 8px; background: var(--card); color: var(--text); cursor: pointer; font-size: 0.85rem; }
.mini:disabled { opacity: 0.35; cursor: not-allowed; }
.mini.danger { color: #f87171; }

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
