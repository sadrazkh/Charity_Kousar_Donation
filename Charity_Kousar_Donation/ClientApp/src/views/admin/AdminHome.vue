<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import draggable from 'vuedraggable'
import { api } from '@/api/client'
import { useToast } from '@/composables/useToast'
import { progressFillStyle } from '@/utils/progress'

const { t, locale } = useI18n()
const toast = useToast()

const fa = computed(() => locale.value === 'fa')
const values = ref({})
const loading = ref(true)
const saving = ref(false)
const translating = ref(false)

// Keys this page owns. Only these are sent on save.
const KEYS = [
  'site.hero.fa', 'site.hero.en', 'site.home.order',
  'site.progress.mode', 'site.progress.color.start', 'site.progress.color.end', 'site.progress.show.percent',
  'donors.source'
]

const SECTIONS = [
  { id: 'hero', icon: '✍️', fa: 'متن خوش‌آمد و مجموع کمک‌ها', en: 'Welcome text & total raised' },
  { id: 'featured', icon: '⭐', fa: 'پروژه‌های ویژه', en: 'Featured projects' },
  { id: 'campaigns', icon: '📁', fa: 'لیست پروژه‌ها', en: 'All projects' },
  { id: 'donors', icon: '👥', fa: 'مشارکت‌کنندگان اخیر', en: 'Recent contributors' }
]
const KNOWN = SECTIONS.map(s => s.id)

const PRESETS = [
  { id: 'classic', fa: 'کلاسیک', en: 'Classic', order: ['hero', 'featured', 'campaigns', 'donors'] },
  { id: 'featured', fa: 'ویژه در صدر', en: 'Featured first', order: ['featured', 'hero', 'campaigns', 'donors'] },
  { id: 'donations', fa: 'متمرکز بر کمک', en: 'Donations focus', order: ['hero', 'campaigns', 'donors'] },
  { id: 'minimal', fa: 'ساده', en: 'Minimal', order: ['hero', 'campaigns'] }
]

// Section order as an editable, draggable list of ids.
const sec = ref([])

onMounted(async () => {
  try {
    const groups = await api('/settings')
    const map = {}
    for (const g of groups) for (const it of g.items) map[it.key] = it.value
    values.value = map
    sec.value = (map['site.home.order'] || '').split(',').map(s => s.trim()).filter(s => KNOWN.includes(s))
  } catch (e) { toast.error(e.message) } finally { loading.value = false }
})

// Keep the setting string in sync with the draggable list.
watch(sec, (v) => { values.value['site.home.order'] = v.join(',') }, { deep: true })

function secLabel(id) { const s = SECTIONS.find(x => x.id === id); return s ? (fa.value ? s.fa : s.en) : id }
function secIcon(id) { return SECTIONS.find(x => x.id === id)?.icon || '▫' }

const hidden = computed(() => SECTIONS.filter(s => !sec.value.includes(s.id)))
function hide(id) { sec.value = sec.value.filter(s => s !== id) }
function show(id) { sec.value = [...sec.value, id] }
function applyPreset(p) { sec.value = [...p.order] }

const progressCfg = computed(() => ({
  progressMode: values.value['site.progress.mode'],
  progressColorStart: values.value['site.progress.color.start'],
  progressColorEnd: values.value['site.progress.color.end']
}))
function previewFill(p) { return progressFillStyle(p, progressCfg.value) }

async function translateHero() {
  const text = values.value['site.hero.fa']
  if (!text || !text.trim()) { toast.error(fa.value ? 'ابتدا متن فارسی را بنویسید' : 'Enter Persian text first'); return }
  translating.value = true
  try {
    const res = await api('/ai/translate', { method: 'POST', body: JSON.stringify({ text, from: 'fa', to: 'en' }) })
    values.value['site.hero.en'] = res.translated
    toast.success(fa.value ? 'ترجمه شد ✓' : 'Translated ✓')
  } catch (e) { toast.error(e.message) } finally { translating.value = false }
}

async function save() {
  saving.value = true
  try {
    const subset = {}
    for (const k of KEYS) if (values.value[k] !== undefined) subset[k] = values.value[k]
    await api('/settings', { method: 'PUT', body: JSON.stringify({ settings: subset }) })
    toast.success(t('savedToast'))
  } catch (e) { toast.error(e.message) } finally { saving.value = false }
}
</script>

<template>
  <div v-if="!loading" class="home-editor">
    <div class="toolbar">
      <div>
        <h1>🏠 {{ fa ? 'سفارشی‌سازی صفحه اصلی' : 'Customize home page' }}</h1>
        <p class="sub">{{ fa ? 'ترتیب و نمایش بخش‌ها، متن خوش‌آمد و ظاهر نوار پیشرفت را تعیین کنید.' : 'Arrange sections, edit the welcome text and the progress bar look.' }}</p>
      </div>
      <div class="tb-actions">
        <a href="/" target="_blank" class="btn btn-ghost btn-sm">🌐 {{ fa ? 'مشاهده' : 'Preview' }}</a>
        <button class="btn btn-primary" :disabled="saving" @click="save">{{ saving ? '...' : t('save') }}</button>
      </div>
    </div>

    <div class="editor-grid">
      <!-- Left: controls -->
      <div class="controls">
        <!-- Quick presets -->
        <section class="card block">
          <h2>{{ fa ? '⚡ قالب‌های آماده چیدمان' : '⚡ Quick layout presets' }}</h2>
          <p class="hint">{{ fa ? 'یک چیدمان آماده را انتخاب کنید، سپس در صورت نیاز با کشیدن تغییر دهید.' : 'Pick a ready layout, then drag to fine-tune.' }}</p>
          <div class="presets">
            <button v-for="p in PRESETS" :key="p.id" type="button" class="preset-chip" @click="applyPreset(p)">
              {{ fa ? p.fa : p.en }}
            </button>
          </div>
        </section>

        <!-- Sections arrangement (drag & drop) -->
        <section class="card block">
          <h2>{{ fa ? '۱) ترتیب و نمایش بخش‌ها' : '1) Sections order & visibility' }}</h2>
          <p class="hint">{{ fa ? 'برای جابه‌جایی، کارت‌ها را با دستگیرهٔ ⠿ بکشید. بخش‌های خاموش نمایش داده نمی‌شوند.' : 'Drag cards by the ⠿ handle to reorder. Hidden sections are not shown.' }}</p>
          <draggable v-model="sec" :item-key="el => el" handle=".sec-drag" class="sec-list" ghost-class="sec-ghost" animation="200" :delay="60" :delay-on-touch-only="true">
            <template #item="{ element: id }">
              <div class="sec-row">
                <span class="sec-drag" aria-label="Drag">⠿</span>
                <span class="sec-ic">{{ secIcon(id) }}</span>
                <span class="sec-name">{{ secLabel(id) }}</span>
                <button type="button" class="mini danger" :title="fa ? 'مخفی کن' : 'Hide'" @click="hide(id)">🚫</button>
              </div>
            </template>
          </draggable>
          <div v-if="hidden.length" class="hidden-row">
            <span class="muted">{{ fa ? 'مخفی:' : 'Hidden:' }}</span>
            <button v-for="s in hidden" :key="s.id" type="button" class="chip" @click="show(s.id)">
              + {{ secIcon(s.id) }} {{ secLabel(s.id) }}
            </button>
          </div>
        </section>

        <!-- Hero text -->
        <section class="card block">
          <h2>{{ fa ? '۲) متن خوش‌آمد (بنر اصلی)' : '2) Welcome text (hero)' }}</h2>
          <label class="label">{{ fa ? 'متن فارسی' : 'Persian text' }}</label>
          <textarea v-model="values['site.hero.fa']" class="textarea" rows="2" />
          <div class="label-row">
            <label class="label">{{ fa ? 'متن انگلیسی' : 'English text' }}</label>
            <button type="button" class="translate-btn" :disabled="translating" @click="translateHero">
              {{ translating ? '...' : (fa ? '🌐 ترجمه از فارسی' : '🌐 Translate from FA') }}
            </button>
          </div>
          <textarea v-model="values['site.hero.en']" class="textarea input-ltr" dir="ltr" rows="2" />
        </section>

        <!-- Progress bar -->
        <section class="card block">
          <h2>{{ fa ? '۳) نوار پیشرفت' : '3) Progress bar' }}</h2>
          <label class="label">{{ fa ? 'حالت رنگ' : 'Color mode' }}</label>
          <select v-model="values['site.progress.mode']" class="select">
            <option value="shift">{{ fa ? 'تغییر تدریجی به سبز (پیشنهادی)' : 'Shift to green (recommended)' }}</option>
            <option value="solid">{{ fa ? 'تک‌رنگ ثابت' : 'Single solid color' }}</option>
            <option value="gradient">{{ fa ? 'گرادیان دو رنگ' : 'Two-color gradient' }}</option>
          </select>
          <div class="colors">
            <div><label class="label">{{ fa ? 'رنگ شروع' : 'Start color' }}</label>
              <input type="color" v-model="values['site.progress.color.start']" class="swatch" /></div>
            <div><label class="label">{{ fa ? 'رنگ پایان (سبز)' : 'End color (green)' }}</label>
              <input type="color" v-model="values['site.progress.color.end']" class="swatch" /></div>
            <label class="chk"><input type="checkbox" :checked="values['site.progress.show.percent'] === 'true'"
              @change="values['site.progress.show.percent'] = $event.target.checked ? 'true' : 'false'" /> {{ fa ? 'نمایش درصد' : 'Show %' }}</label>
          </div>
          <div class="prog-preview">
            <div v-for="p in [25, 60, 95]" :key="p" class="pp-bar"><div class="pp-fill" :style="previewFill(p)" /><span>{{ p }}%</span></div>
          </div>
        </section>

        <!-- Contributors source -->
        <section class="card block">
          <h2>{{ fa ? '۴) مشارکت‌کنندگان' : '4) Contributors' }}</h2>
          <label class="label">{{ fa ? 'منبع لیست' : 'List source' }}</label>
          <select v-model="values['donors.source']" class="select">
            <option value="auto">{{ fa ? 'خودکار (کمک‌های واقعی)' : 'Automatic (real donations)' }}</option>
            <option value="manual">{{ fa ? 'دستی (لیست تنظیمات)' : 'Manual (settings list)' }}</option>
            <option value="both">{{ fa ? 'هر دو' : 'Both' }}</option>
          </select>
          <router-link to="/admin/settings" class="more-link">
            {{ fa ? 'تنظیمات کامل حامیان (لیست دستی، فیلدها...) ←' : 'Full contributor settings (manual list, fields...) →' }}
          </router-link>
        </section>
      </div>

      <!-- Right: live arrangement preview -->
      <aside class="preview">
        <p class="preview-title">{{ fa ? 'پیش‌نمایش چیدمان' : 'Layout preview' }}</p>
        <div class="phone">
          <div v-for="id in sec" :key="id" class="pv-block" :class="'pv-' + id">
            <span class="pv-ic">{{ secIcon(id) }}</span>
            <span>{{ secLabel(id) }}</span>
          </div>
          <p v-if="!sec.length" class="pv-empty">{{ fa ? 'هیچ بخشی فعال نیست' : 'No sections enabled' }}</p>
        </div>
      </aside>
    </div>
  </div>
</template>

<style scoped>
.toolbar { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; flex-wrap: wrap; margin-bottom: 1.25rem; }
.sub { color: var(--muted); font-size: 0.9rem; margin-top: 0.2rem; }
.tb-actions { display: flex; gap: 0.5rem; }
.editor-grid { display: grid; grid-template-columns: 1fr 300px; gap: 1.25rem; align-items: start; }
.controls { display: flex; flex-direction: column; gap: 1rem; }
.block h2 { font-size: 1rem; color: var(--primary); margin-bottom: 0.6rem; }
.hint { font-size: 0.82rem; color: var(--muted); margin-bottom: 0.75rem; }
.label { display: block; margin: 0.5rem 0 0.3rem; font-size: 0.88rem; color: var(--muted); }
.label-row { display: flex; justify-content: space-between; align-items: center; }
.translate-btn { background: color-mix(in srgb, var(--accent) 16%, transparent); color: var(--accent); border: none; border-radius: 999px; padding: 0.2rem 0.7rem; font-size: 0.75rem; cursor: pointer; font-family: inherit; }
.translate-btn:disabled { opacity: 0.5; }

.sec-list { display: flex; flex-direction: column; gap: 0.5rem; min-height: 40px; }
.sec-row { display: flex; align-items: center; gap: 0.6rem; padding: 0.6rem 0.75rem; border: 1px solid var(--border); border-radius: 10px; background: var(--input-bg); }
.sec-drag { cursor: grab; color: var(--muted); font-size: 1.2rem; touch-action: none; }
.sec-ghost { opacity: 0.4; }
.sec-ic { font-size: 1.1rem; }
.sec-name { flex: 1; font-size: 0.92rem; }
.mini { width: 32px; height: 32px; border: 1px solid var(--border); border-radius: 8px; background: var(--card); color: var(--text); cursor: pointer; }
.mini.danger { color: #f87171; }
.presets { display: flex; flex-wrap: wrap; gap: 0.5rem; }
.preset-chip { padding: 0.45rem 0.9rem; border-radius: 999px; border: 1px solid var(--border); background: var(--input-bg); color: var(--text); cursor: pointer; font-family: inherit; font-size: 0.85rem; }
.preset-chip:hover { border-color: color-mix(in srgb, var(--primary) 50%, transparent); color: var(--primary); }
.hidden-row { display: flex; flex-wrap: wrap; gap: 0.4rem; align-items: center; margin-top: 0.75rem; }
.hidden-row .muted { color: var(--muted); font-size: 0.85rem; }
.chip { padding: 0.35rem 0.7rem; border-radius: 999px; border: 1px dashed var(--border); background: transparent; color: var(--text); cursor: pointer; font-family: inherit; font-size: 0.82rem; }

.colors { display: flex; gap: 1rem; align-items: flex-end; flex-wrap: wrap; }
.swatch { width: 56px; height: 38px; border: 1px solid var(--border); border-radius: 8px; background: none; cursor: pointer; padding: 2px; }
.chk { display: flex; align-items: center; gap: 0.4rem; font-size: 0.88rem; color: var(--muted); }
.prog-preview { display: flex; flex-direction: column; gap: 0.4rem; margin-top: 0.75rem; }
.pp-bar { display: flex; align-items: center; gap: 0.5rem; background: color-mix(in srgb, var(--muted) 18%, transparent); border-radius: 999px; padding-inline-end: 0.5rem; }
.pp-fill { height: 10px; border-radius: 999px; }
.pp-bar span { font-size: 0.75rem; color: var(--muted); min-width: 2.4rem; }
.more-link { display: inline-block; margin-top: 0.75rem; font-size: 0.85rem; color: var(--primary); text-decoration: none; }

.preview { position: sticky; top: 1rem; }
.preview-title { font-size: 0.8rem; color: var(--muted); margin-bottom: 0.5rem; text-transform: uppercase; letter-spacing: 0.05em; }
.phone { border: 1px solid var(--border); border-radius: 18px; padding: 0.75rem; background: var(--bg-soft); display: flex; flex-direction: column; gap: 0.5rem; min-height: 200px; }
.pv-block { display: flex; align-items: center; gap: 0.5rem; padding: 0.85rem 0.75rem; border-radius: 10px; font-size: 0.85rem; color: var(--text); background: var(--card); border: 1px solid var(--border); }
.pv-hero { background: linear-gradient(135deg, color-mix(in srgb, var(--primary) 22%, transparent), color-mix(in srgb, var(--accent) 14%, transparent)); font-weight: 700; }
.pv-featured { border-color: color-mix(in srgb, var(--accent) 50%, transparent); }
.pv-ic { font-size: 1.1rem; }
.pv-empty { color: var(--muted); text-align: center; font-size: 0.85rem; padding: 2rem 0; }

@media (max-width: 820px) {
  .editor-grid { grid-template-columns: 1fr; }
  .preview { position: static; }
}
</style>
