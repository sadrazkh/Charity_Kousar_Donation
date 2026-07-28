<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import draggable from 'vuedraggable'
import { api } from '@/api/client'
import { useToast } from '@/composables/useToast'
import ProgressAmount from '@/components/ProgressAmount.vue'
import ProgressBar from '@/components/ProgressBar.vue'

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
  'site.home.columns', 'site.home.merge.featured', 'site.card.image.fit',
  'site.progress.mode', 'site.progress.color.start', 'site.progress.color.end', 'site.progress.show.percent',
  'site.progress.animate', 'site.progress.animate.ms', 'site.progress.track.color',
  'donation.progress.format.fa', 'donation.progress.format.en', 'donation.progress.highlight',
  'donation.progress.color.collected', 'donation.progress.color.target',
  'donation.progress.color.remaining', 'donation.progress.color.percent',
  'donation.progress.color.text', 'donation.progress.size',
  'donors.source'
]

// Amount-text pieces that can each get their own color.
const AMOUNT_PARTS = [
  { key: 'donation.progress.color.collected', token: '{collected}', fa: 'مبلغ جمع‌آوری‌شده', en: 'Raised amount' },
  { key: 'donation.progress.color.target', token: '{target}', fa: 'مبلغ هدف', en: 'Goal amount' },
  { key: 'donation.progress.color.remaining', token: '{remaining}', fa: 'مبلغ باقی‌مانده', en: 'Remaining' },
  { key: 'donation.progress.color.percent', token: '{percent}', fa: 'درصد پیشرفت', en: 'Percent' },
  { key: 'donation.progress.color.text', token: 'abc', fa: 'متن ساده', en: 'Plain text' }
]

const SECTIONS = [
  { id: 'hero', icon: 'M12 20h9M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4z', fa: 'متن خوش‌آمد و مجموع کمک‌ها', en: 'Welcome text & total raised' },
  { id: 'featured', icon: 'M12 2l2.9 6.1 6.6.9-4.8 4.6 1.2 6.6L12 17.8 6.1 20.8l1.2-6.6L2.5 9l6.6-.9L12 2z', fa: 'پروژه‌های ویژه', en: 'Featured projects' },
  { id: 'campaigns', icon: 'M3 7a2 2 0 0 1 2-2h4l2 2h8a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z', fa: 'لیست پروژه‌ها', en: 'All projects' },
  { id: 'donors', icon: 'M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2M9 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8M23 21v-2a4 4 0 0 0-3-3.9M16 3.1a4 4 0 0 1 0 7.8', fa: 'مشارکت‌کنندگان اخیر', en: 'Recent contributors' }
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
function secIcon(id) { return SECTIONS.find(x => x.id === id)?.icon || 'M4 6h16M4 12h16M4 18h16' }

const hidden = computed(() => SECTIONS.filter(s => !sec.value.includes(s.id)))
function hide(id) { sec.value = sec.value.filter(s => s !== id) }
function show(id) { sec.value = [...sec.value, id] }
function applyPreset(p) { sec.value = [...p.order] }

const progressCfg = computed(() => ({
  progressMode: values.value['site.progress.mode'],
  progressColorStart: values.value['site.progress.color.start'],
  progressColorEnd: values.value['site.progress.color.end'],
  progressTrackColor: values.value['site.progress.track.color'],
  progressAnimate: values.value['site.progress.animate'] !== 'false',
  progressAnimateMs: Number(values.value['site.progress.animate.ms']) || 0,
  showProgressPercent: values.value['site.progress.show.percent'] !== 'false'
}))

// Bumping the key remounts the preview bars so the fill animation replays.
const replayKey = ref(0)
function replayPreview() { replayKey.value++ }

const amountColors = computed(() => ({
  collected: values.value['donation.progress.color.collected'],
  target: values.value['donation.progress.color.target'],
  remaining: values.value['donation.progress.color.remaining'],
  percent: values.value['donation.progress.color.percent'],
  text: values.value['donation.progress.color.text']
}))
function resetColor(key) { values.value[key] = '' }

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
        <h1>{{ fa ? 'سفارشی‌سازی صفحه اصلی' : 'Customize home page' }}</h1>
        <p class="sub">{{ fa ? 'ترتیب و نمایش بخش‌ها، متن خوش‌آمد و ظاهر نوار پیشرفت را تعیین کنید.' : 'Arrange sections, edit the welcome text and the progress bar look.' }}</p>
      </div>
      <div class="tb-actions">
        <a href="/" target="_blank" rel="noopener" class="btn btn-ghost btn-sm">
          <svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="9"/><path d="M3 12h18M12 3a14 14 0 0 1 0 18M12 3a14 14 0 0 0 0 18"/></svg>
          {{ fa ? 'مشاهده' : 'Preview' }}
        </a>
        <button class="btn btn-primary" :disabled="saving" @click="save">{{ saving ? '...' : t('save') }}</button>
      </div>
    </div>

    <div class="editor-grid">
      <!-- Left: controls -->
      <div class="controls">
        <!-- Quick presets -->
        <section class="card block">
          <h2>{{ fa ? 'قالب‌های آماده چیدمان' : 'Quick layout presets' }}</h2>
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
                <span class="sec-drag" aria-label="Drag" title="Drag">
                  <svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><circle cx="9" cy="6" r="1.5"/><circle cx="9" cy="12" r="1.5"/><circle cx="9" cy="18" r="1.5"/><circle cx="15" cy="6" r="1.5"/><circle cx="15" cy="12" r="1.5"/><circle cx="15" cy="18" r="1.5"/></svg>
                </span>
                <svg class="sec-ic" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path :d="secIcon(id)"/></svg>
                <span class="sec-name">{{ secLabel(id) }}</span>
                <button type="button" class="mini danger" :aria-label="fa ? 'مخفی کن' : 'Hide'" :title="fa ? 'مخفی کن' : 'Hide'" @click="hide(id)">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M9.9 4.2A10 10 0 0 1 12 4c6.5 0 10 8 10 8a15 15 0 0 1-2.9 3.7M6.6 6.6A15 15 0 0 0 2 12s3.5 7 10 7a10 10 0 0 0 4.4-1M3 3l18 18"/></svg>
                </button>
              </div>
            </template>
          </draggable>
          <div v-if="hidden.length" class="hidden-row">
            <span class="muted">{{ fa ? 'مخفی:' : 'Hidden:' }}</span>
            <button v-for="s in hidden" :key="s.id" type="button" class="chip" @click="show(s.id)">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M12 5v14M5 12h14"/></svg>
              <svg class="chip-ic" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path :d="secIcon(s.id)"/></svg>
              {{ secLabel(s.id) }}
            </button>
          </div>
        </section>

        <!-- Card layout -->
        <section class="card block">
          <h2>{{ fa ? '۲) چیدمان کارت‌ها' : '2) Card layout' }}</h2>
          <div class="row-2">
            <div>
              <label class="label">{{ fa ? 'تعداد ستون در هر ردیف' : 'Columns per row' }}</label>
              <select v-model="values['site.home.columns']" class="select">
                <option value="auto">{{ fa ? 'خودکار (متناسب با صفحه)' : 'Auto (fit screen)' }}</option>
                <option value="2">۲ / 2</option>
                <option value="3">۳ / 3</option>
                <option value="4">۴ / 4</option>
              </select>
            </div>
            <label class="chk merge">
              <input type="checkbox" :checked="values['site.home.merge.featured'] === 'true'"
                @change="values['site.home.merge.featured'] = $event.target.checked ? 'true' : 'false'" />
              {{ fa ? 'ویژه و عادی در یک کادر' : 'Merge featured + normal' }}
            </label>
          </div>
          <p class="hint">{{ fa ? 'اگر تیک بزنید، پروژه‌های ویژه جدا نمایش داده نمی‌شوند و همه در یک گرید با هم می‌آیند. در موبایل ستون‌ها خودکار کم می‌شوند.' : 'When checked, featured projects are not separated — all appear in one grid. Columns auto-reduce on mobile.' }}</p>
          <label class="label">{{ fa ? 'نمایش تصویر روی کارت' : 'Card image display' }}</label>
          <select v-model="values['site.card.image.fit']" class="select">
            <option value="cover">{{ fa ? 'پر کردن کادر (برش لبه‌ها)' : 'Fill the box (crop edges)' }}</option>
            <option value="contain">{{ fa ? 'نمایش کامل تصویر (بدون برش)' : 'Show the whole image (no crop)' }}</option>
          </select>
          <p class="hint">{{ fa ? 'برای تصویرهای آمادهٔ مربعی، حالت «نمایش کامل» مناسب‌تر است.' : 'For the square ready-made illustrations, “show the whole image” fits better.' }}</p>
        </section>

        <!-- Hero text -->
        <section class="card block">
          <h2>{{ fa ? '۳) متن خوش‌آمد (بنر اصلی)' : '3) Welcome text (hero)' }}</h2>
          <label class="label">{{ fa ? 'متن فارسی' : 'Persian text' }}</label>
          <textarea v-model="values['site.hero.fa']" class="textarea" rows="2" />
          <div class="label-row">
            <label class="label">{{ fa ? 'متن انگلیسی' : 'English text' }}</label>
            <button type="button" class="translate-btn" :disabled="translating" @click="translateHero">
              <svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M4 5h10M9 3v2c0 5-2.5 8-6 9M6 9c0 3 3 5 6 6M13 21l4-9 4 9M14.5 17h5"/></svg>
              {{ translating ? '…' : (fa ? 'ترجمه از فارسی' : 'Translate from FA') }}
            </button>
          </div>
          <textarea v-model="values['site.hero.en']" class="textarea input-ltr" dir="ltr" rows="2" />
        </section>

        <!-- Progress bar -->
        <section class="card block">
          <h2>{{ fa ? '۴) نوار پیشرفت' : '4) Progress bar' }}</h2>
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
            <div>
              <label class="label">{{ fa ? 'رنگ زمینه نوار' : 'Track color' }}</label>
              <div class="swatch-row">
                <input type="color" :value="values['site.progress.track.color'] || '#94a3b8'" class="swatch"
                  @input="values['site.progress.track.color'] = $event.target.value" />
                <button type="button" class="reset-btn" :disabled="!values['site.progress.track.color']"
                  @click="resetColor('site.progress.track.color')">{{ fa ? 'پیش‌فرض' : 'Default' }}</button>
              </div>
            </div>
            <label class="chk"><input type="checkbox" :checked="values['site.progress.show.percent'] === 'true'"
              @change="values['site.progress.show.percent'] = $event.target.checked ? 'true' : 'false'" /> {{ fa ? 'نمایش درصد' : 'Show %' }}</label>
          </div>

          <div class="anim-row">
            <label class="chk"><input type="checkbox" :checked="values['site.progress.animate'] !== 'false'"
              @change="values['site.progress.animate'] = $event.target.checked ? 'true' : 'false'" />
              {{ fa ? 'پر شدن متحرک (از صفر تا مقدار واقعی)' : 'Animate the fill (sweep from zero)' }}</label>
            <div class="speed" :class="{ off: values['site.progress.animate'] === 'false' }">
              <label class="label">{{ fa ? 'سرعت پر شدن' : 'Fill speed' }}</label>
              <div class="speed-row">
                <input type="range" min="300" max="4000" step="100"
                  :value="Number(values['site.progress.animate.ms']) || 1400"
                  :disabled="values['site.progress.animate'] === 'false'"
                  @input="values['site.progress.animate.ms'] = $event.target.value" />
                <span class="speed-val">{{ ((Number(values['site.progress.animate.ms']) || 1400) / 1000).toFixed(1) }}s</span>
              </div>
            </div>
          </div>
          <p class="hint">{{ fa
            ? 'نوار وقتی وارد دید کاربر می‌شود از صفر تا مقدار واقعی پر می‌شود و رنگ هم همراه آن تغییر می‌کند. برای کاربرانی که «کاهش انیمیشن» را در سیستم‌عامل فعال کرده‌اند، مقدار نهایی بدون حرکت نمایش داده می‌شود.'
            : 'The bar sweeps from zero to the real value when it scrolls into view, with the color shifting along. Users with reduced-motion enabled see the final value instantly.' }}</p>

          <div class="prog-preview">
            <div class="preview-head">
              <span class="label">{{ fa ? 'پیش‌نمایش زنده' : 'Live preview' }}</span>
              <button type="button" class="reset-btn" @click="replayPreview">{{ fa ? 'پخش دوباره' : 'Replay' }}</button>
            </div>
            <ProgressBar v-for="p in [25, 60, 95]" :key="`${replayKey}-${p}`" :percent="p" :cfg="progressCfg" />
          </div>
        </section>

        <!-- Amount text format -->
        <section class="card block">
          <h2>{{ fa ? '۵) نمایش مبلغ (چقدر از چقدر)' : '5) Amount text (raised / goal)' }}</h2>
          <label class="label">{{ fa ? 'قالب فارسی' : 'Persian format' }}</label>
          <input v-model="values['donation.progress.format.fa']" class="input input-rtl" />
          <label class="label">{{ fa ? 'قالب انگلیسی' : 'English format' }}</label>
          <input v-model="values['donation.progress.format.en']" class="input input-ltr" dir="ltr" />
          <label class="label">{{ fa ? 'رنگ تأکید (برای ~متن~)' : 'Highlight color (for ~text~)' }}</label>
          <div class="colors"><input type="color" v-model="values['donation.progress.highlight']" class="swatch" /></div>

          <label class="label">{{ fa ? 'رنگ هر بخش از متن مبلغ' : 'Color of each amount part' }}</label>
          <div class="part-colors">
            <div v-for="p in AMOUNT_PARTS" :key="p.key" class="part">
              <span class="part-name">{{ fa ? p.fa : p.en }}</span>
              <code class="part-token">{{ p.token }}</code>
              <div class="swatch-row">
                <input type="color" :value="values[p.key] || '#0d9488'" class="swatch sm"
                  @input="values[p.key] = $event.target.value" />
                <button type="button" class="reset-btn" :disabled="!values[p.key]" @click="resetColor(p.key)">
                  {{ fa ? 'پیش‌فرض' : 'Default' }}
                </button>
              </div>
            </div>
          </div>
          <p class="hint">{{ fa
            ? 'رنگ خالی یعنی «رنگ پیش‌فرض قالب». رنگ هر بخش بر رنگ تأکید اولویت دارد.'
            : 'An empty color means “theme default”. A part color overrides the highlight color.' }}</p>

          <label class="label">{{ fa ? 'اندازه متن مبلغ' : 'Amount text size' }}</label>
          <div class="speed-row">
            <input type="range" min="80" max="180" step="5"
              :value="Number(values['donation.progress.size']) || 100"
              @input="values['donation.progress.size'] = $event.target.value" />
            <span class="speed-val">{{ Number(values['donation.progress.size']) || 100 }}%</span>
          </div>

          <div class="help">
            <strong>{{ fa ? 'راهنما:' : 'Guide:' }}</strong>
            <ul>
              <li><code>{collected}</code> — {{ fa ? 'مبلغ جمع‌آوری‌شده' : 'amount raised' }}</li>
              <li><code>{target}</code> — {{ fa ? 'مبلغ هدف' : 'goal amount' }}</li>
              <li><code>{remaining}</code> — {{ fa ? 'مبلغ باقی‌مانده' : 'remaining amount' }}</li>
              <li><code>{percent}</code> — {{ fa ? 'درصد پیشرفت' : 'progress percent' }}</li>
              <li><code>*…*</code> — {{ fa ? 'متن داخل ستاره بولد می‌شود' : 'text inside stars becomes bold' }}</li>
              <li><code>~…~</code> — {{ fa ? 'متن داخل موج با رنگ تأکید نمایش داده می‌شود' : 'text inside tildes is colored' }}</li>
            </ul>
            <p class="ex">{{ fa
              ? 'مثال: «*{collected}* از {target} تومان — ~{remaining}~ مانده»'
              : 'Example: "*{collected}* of {target} — ~{remaining}~ left"' }}</p>
            <p class="ex live">{{ fa ? 'پیش‌نمایش:' : 'Preview:' }}
              <ProgressAmount :collected="6500000" :target="10000000"
                :format="fa ? values['donation.progress.format.fa'] : values['donation.progress.format.en']"
                :highlight="values['donation.progress.highlight']"
                :colors="amountColors"
                :scale="Number(values['donation.progress.size']) || 100" /></p>
          </div>
        </section>

        <!-- Contributors source -->
        <section class="card block">
          <h2>{{ fa ? '۶) مشارکت‌کنندگان' : '6) Contributors' }}</h2>
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
            <svg class="pv-ic" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path :d="secIcon(id)"/></svg>
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
.sec-drag { cursor: grab; color: var(--muted); touch-action: none; display: inline-flex; }
.sec-drag svg { width: 20px; height: 20px; }
.sec-ghost { opacity: 0.4; }
.sec-ic { width: 20px; height: 20px; color: var(--primary); flex-shrink: 0; }
.sec-name { flex: 1; font-size: 0.92rem; }
.mini { width: 32px; height: 32px; border: 1px solid var(--border); border-radius: 8px; background: var(--card); color: var(--text); cursor: pointer; display: inline-flex; align-items: center; justify-content: center; }
.mini svg { width: 17px; height: 17px; }
.mini.danger { color: var(--danger); }
.presets { display: flex; flex-wrap: wrap; gap: 0.5rem; }
.preset-chip { padding: 0.45rem 0.9rem; border-radius: 999px; border: 1px solid var(--border); background: var(--input-bg); color: var(--text); cursor: pointer; font-family: inherit; font-size: 0.85rem; }
.preset-chip:hover { border-color: color-mix(in srgb, var(--primary) 50%, transparent); color: var(--primary); }
.hidden-row { display: flex; flex-wrap: wrap; gap: 0.4rem; align-items: center; margin-top: 0.75rem; }
.hidden-row .muted { color: var(--muted); font-size: 0.85rem; }
.chip { display: inline-flex; align-items: center; gap: 0.3rem; padding: 0.35rem 0.7rem; border-radius: 999px; border: 1px dashed var(--border); background: transparent; color: var(--text); cursor: pointer; font-family: inherit; font-size: 0.82rem; }
.chip svg { width: 14px; height: 14px; }
.chip .chip-ic { color: var(--primary); }
.translate-btn { display: inline-flex; align-items: center; gap: 0.3rem; }
.translate-btn .icon { width: 14px; height: 14px; }

.colors { display: flex; gap: 1rem; align-items: flex-end; flex-wrap: wrap; }
.swatch { width: 56px; height: 38px; border: 1px solid var(--border); border-radius: 8px; background: none; cursor: pointer; padding: 2px; }
.chk { display: flex; align-items: center; gap: 0.4rem; font-size: 0.88rem; color: var(--muted); }
.prog-preview { display: flex; flex-direction: column; gap: 0.5rem; margin-top: 0.75rem; }
.preview-head { display: flex; align-items: center; justify-content: space-between; gap: 0.5rem; }
.preview-head .label { margin: 0; }

.swatch-row { display: flex; align-items: center; gap: 0.4rem; }
.swatch.sm { width: 44px; height: 32px; }
.reset-btn {
  border: 1px solid var(--border); background: var(--input-bg); color: var(--muted);
  border-radius: 999px; padding: 0.25rem 0.7rem; font-size: 0.75rem; cursor: pointer; font-family: inherit;
}
.reset-btn:hover:not(:disabled) { color: var(--primary); border-color: color-mix(in srgb, var(--primary) 45%, transparent); }
.reset-btn:disabled { opacity: 0.45; cursor: default; }

.anim-row { display: flex; flex-wrap: wrap; gap: 1rem 1.5rem; align-items: center; margin-top: 0.9rem; }
.speed.off { opacity: 0.45; }
.speed .label { margin-top: 0; }
.speed-row { display: flex; align-items: center; gap: 0.6rem; }
.speed-row input[type="range"] { width: 180px; accent-color: var(--primary); }
.speed-val { font-size: 0.82rem; color: var(--muted); font-variant-numeric: tabular-nums; min-width: 3rem; }

.part-colors { display: grid; grid-template-columns: repeat(auto-fit, minmax(160px, 1fr)); gap: 0.6rem; }
.part {
  display: flex; flex-direction: column; gap: 0.35rem;
  padding: 0.6rem; border: 1px solid var(--border); border-radius: 10px; background: var(--input-bg);
}
.part-name { font-size: 0.85rem; }
.part-token { font-size: 0.72rem; color: var(--muted); direction: ltr; align-self: flex-start; }
.more-link { display: inline-block; margin-top: 0.75rem; font-size: 0.85rem; color: var(--primary); text-decoration: none; }
.row-2 { display: grid; grid-template-columns: 1fr auto; gap: 1rem; align-items: end; }
.chk.merge { white-space: nowrap; padding-bottom: 0.5rem; }
.help { margin-top: 0.85rem; padding: 0.85rem 1rem; border-radius: 10px; background: color-mix(in srgb, var(--primary) 7%, transparent); border: 1px solid var(--border); font-size: 0.85rem; }
.help ul { margin: 0.5rem 0; padding-inline-start: 1.1rem; display: flex; flex-direction: column; gap: 0.25rem; color: var(--muted); }
.help code { background: color-mix(in srgb, var(--muted) 18%, transparent); padding: 0.05rem 0.35rem; border-radius: 5px; font-family: 'Inter', monospace; direction: ltr; display: inline-block; }
.help .ex { margin-top: 0.5rem; color: var(--text); }
.help .live { margin-top: 0.6rem; font-weight: 600; }

.preview { position: sticky; top: 1rem; }
.preview-title { font-size: 0.8rem; color: var(--muted); margin-bottom: 0.5rem; text-transform: uppercase; letter-spacing: 0.05em; }
.phone { border: 1px solid var(--border); border-radius: 18px; padding: 0.75rem; background: var(--bg-soft); display: flex; flex-direction: column; gap: 0.5rem; min-height: 200px; }
.pv-block { display: flex; align-items: center; gap: 0.5rem; padding: 0.85rem 0.75rem; border-radius: 10px; font-size: 0.85rem; color: var(--text); background: var(--card); border: 1px solid var(--border); }
.pv-hero { background: linear-gradient(135deg, color-mix(in srgb, var(--primary) 22%, transparent), color-mix(in srgb, var(--accent) 14%, transparent)); font-weight: 700; }
.pv-featured { border-color: color-mix(in srgb, var(--accent) 50%, transparent); }
.pv-ic { width: 18px; height: 18px; flex-shrink: 0; }
.pv-empty { color: var(--muted); text-align: center; font-size: 0.85rem; padding: 2rem 0; }

@media (max-width: 820px) {
  .editor-grid { grid-template-columns: 1fr; }
  .preview { position: static; }
}
</style>
