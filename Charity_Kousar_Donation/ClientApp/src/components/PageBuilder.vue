<script setup>
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { api } from '@/api/client'
import { useToast } from '@/composables/useToast'
import { BLOCK_TYPES, createBlock, blockLabel } from '@/utils/pageBlocks'
import { PAGE_TEMPLATES } from '@/utils/pageTemplates'
import NestedBlockList from '@/components/NestedBlockList.vue'
import PageBlockRenderer from '@/components/PageBlockRenderer.vue'

const props = defineProps({
  modelValue: { type: Array, default: () => [] },
  campaign: { type: Object, default: null }
})
const emit = defineEmits(['update:modelValue', 'save'])

const { locale } = useI18n()
const toast = useToast()
const blocks = computed({
  get: () => props.modelValue,
  set: v => emit('update:modelValue', v)
})
const showPreview = ref(false)
const paletteOpen = ref(false)
const showTemplates = ref(false)
const customTemplates = ref([])

const campaignTitle = computed(() => props.campaign?.titleFa || '')

onMounted(async () => {
  try {
    const r = await api('/settings/templates')
    customTemplates.value = JSON.parse(r.json || '[]')
  } catch { /* */ }
})

// Give every block (and nested blocks) a fresh id when a template is applied.
function refreshIds(list) {
  return (list || []).map(b => {
    const nb = JSON.parse(JSON.stringify(b))
    nb.id = Math.random().toString(36).slice(2, 10)
    if (Array.isArray(nb.data?.columns)) nb.data.columns = nb.data.columns.map(refreshIds)
    if (Array.isArray(nb.data?.blocks)) nb.data.blocks = refreshIds(nb.data.blocks)
    return nb
  })
}

function confirmReplace() {
  return !blocks.value.length || confirm(locale.value === 'fa'
    ? 'محتوای فعلی صفحه با این قالب جایگزین شود؟'
    : 'Replace the current page content with this template?')
}

function applyTemplate(tpl) {
  if (!confirmReplace()) return
  blocks.value = tpl.build()
  showTemplates.value = false
}

function applyCustom(tpl) {
  if (!confirmReplace()) return
  blocks.value = refreshIds(tpl.blocks)
  showTemplates.value = false
}

async function persistTemplates(arr) {
  await api('/settings/templates', { method: 'PUT', body: JSON.stringify({ json: JSON.stringify(arr) }) })
  customTemplates.value = arr
}

async function saveAsTemplate() {
  if (!blocks.value.length) {
    toast.error(locale.value === 'fa' ? 'صفحه خالی است' : 'Page is empty')
    return
  }
  const name = (prompt(locale.value === 'fa' ? 'نام قالب (برای ویرایش، همان نام قبلی را وارد کنید):' : 'Template name (reuse a name to overwrite):') || '').trim()
  if (!name) return
  const snapshot = JSON.parse(JSON.stringify(blocks.value))
  const next = [...customTemplates.value]
  const idx = next.findIndex(t => t.name === name)
  if (idx >= 0) next[idx] = { ...next[idx], blocks: snapshot }
  else next.push({ id: Math.random().toString(36).slice(2, 10), name, blocks: snapshot })
  try {
    await persistTemplates(next)
    toast.success(locale.value === 'fa' ? 'قالب ذخیره شد ✓' : 'Template saved ✓')
  } catch (e) { toast.error(e.message) }
}

async function deleteCustom(tpl) {
  if (!confirm(locale.value === 'fa' ? `قالب «${tpl.name}» حذف شود؟` : `Delete template "${tpl.name}"?`)) return
  try {
    await persistTemplates(customTemplates.value.filter(t => t.id !== tpl.id))
    toast.success(locale.value === 'fa' ? 'حذف شد' : 'Deleted')
  } catch (e) { toast.error(e.message) }
}

const categories = computed(() => {
  const cats = { layout: [], content: [], media: [], campaign: [] }
  for (const b of BLOCK_TYPES) (cats[b.category] || cats.content).push(b)
  return cats
})

function addBlock(type) {
  blocks.value = [...blocks.value, createBlock(type)]
  paletteOpen.value = false
}

function label(type) {
  return blockLabel(type, locale.value)
}
</script>

<template>
  <div class="builder">
    <!-- Mobile palette FAB -->
    <button type="button" class="fab" @click="paletteOpen = !paletteOpen" aria-label="Add block">
      {{ paletteOpen ? '✕' : '+' }}
    </button>

    <aside class="palette card" :class="{ open: paletteOpen }">
      <h3>{{ locale === 'fa' ? 'افزودن بلوک' : 'Add block' }}</h3>
      <div v-for="(items, cat) in categories" :key="cat" class="cat-group">
        <p class="cat-label">{{ cat }}</p>
        <button v-for="bt in items" :key="bt.type" type="button" class="palette-btn" @click="addBlock(bt.type)">
          <span class="icon">{{ bt.icon }}</span>
          <span>{{ label(bt.type) }}</span>
        </button>
      </div>
    </aside>

    <div class="canvas">
      <div class="canvas-toolbar">
        <span>{{ locale === 'fa' ? 'صفحه‌ساز — برای جابه‌جایی بکشید ⠿' : 'Page builder — drag ⠿ to reorder' }}</span>
        <div class="toolbar-actions">
          <button type="button" class="btn btn-ghost btn-sm" @click="showTemplates = !showTemplates">
            🎨 {{ locale === 'fa' ? 'قالب آماده' : 'Templates' }}
          </button>
          <button type="button" class="btn btn-ghost btn-sm hide-mobile" @click="showPreview = !showPreview">
            {{ showPreview ? '▣' : '◫' }} {{ locale === 'fa' ? 'پیش‌نمایش' : 'Preview' }}
          </button>
          <button type="button" class="btn btn-primary btn-sm" @click="emit('save')">{{ locale === 'fa' ? 'ذخیره' : 'Save' }}</button>
        </div>
      </div>

      <div v-if="showTemplates" class="templates card">
        <p class="tpl-hint">{{ locale === 'fa' ? 'یک قالب را انتخاب کنید تا چیدمان آماده ساخته شود، سپس آزادانه ویرایش/بکشید:' : 'Pick a layout to scaffold the page, then edit/drag freely:' }}</p>
        <div class="tpl-grid">
          <button v-for="tpl in PAGE_TEMPLATES" :key="tpl.id" type="button" class="tpl-card" @click="applyTemplate(tpl)">
            <span class="tpl-ic">{{ tpl.icon }}</span>
            <strong>{{ locale === 'fa' ? tpl.fa : tpl.en }}</strong>
            <small>{{ locale === 'fa' ? tpl.descFa : tpl.descEn }}</small>
          </button>
        </div>

        <div class="custom-tpls">
          <div class="ct-head">
            <strong>{{ locale === 'fa' ? '💾 قالب‌های من' : '💾 My templates' }}</strong>
            <button type="button" class="btn btn-ghost btn-sm" @click="saveAsTemplate">
              {{ locale === 'fa' ? 'ذخیرهٔ صفحهٔ فعلی به‌عنوان قالب' : 'Save current page as template' }}
            </button>
          </div>
          <p class="tpl-hint">{{ locale === 'fa' ? 'برای ویرایش یک قالب: «استفاده» را بزنید، تغییر دهید و دوباره با همان نام ذخیره کنید.' : 'To edit a template: Use it, change it, then save again with the same name.' }}</p>
          <div v-if="customTemplates.length" class="tpl-grid">
            <div v-for="tpl in customTemplates" :key="tpl.id" class="tpl-card custom">
              <strong>{{ tpl.name }}</strong>
              <small>{{ (tpl.blocks || []).length }} {{ locale === 'fa' ? 'بلوک' : 'blocks' }}</small>
              <div class="ct-actions">
                <button type="button" class="btn btn-primary btn-sm" @click="applyCustom(tpl)">{{ locale === 'fa' ? 'استفاده' : 'Use' }}</button>
                <button type="button" class="ct-del" @click="deleteCustom(tpl)">🗑</button>
              </div>
            </div>
          </div>
          <p v-else class="ct-empty">{{ locale === 'fa' ? 'هنوز قالبی ذخیره نکرده‌اید.' : 'No saved templates yet.' }}</p>
        </div>
      </div>

      <NestedBlockList v-model="blocks" :campaign-title="campaignTitle" />

      <p v-if="!blocks.length" class="empty">
        {{ locale === 'fa' ? '➕ بلوک اضافه کنید' : '➕ Add blocks' }}
      </p>

      <div v-if="showPreview && blocks.length && campaign" class="preview-panel card">
        <h3>{{ locale === 'fa' ? 'پیش‌نمایش موبایل/دسکتاپ' : 'Preview' }}</h3>
        <PageBlockRenderer :blocks="blocks" :campaign="campaign" preview @donate="() => {}" />
      </div>
    </div>
  </div>
</template>

<style scoped>
.builder { display: grid; grid-template-columns: minmax(200px, 240px) 1fr; gap: 1rem; align-items: start; position: relative; }
@media (max-width: 768px) { .builder { grid-template-columns: 1fr; padding-bottom: 4rem; } }
.palette { position: sticky; top: 0.75rem; padding: 1rem; max-height: calc(100vh - 2rem); overflow-y: auto; }
@media (max-width: 768px) {
  .palette { position: fixed; bottom: 4.5rem; left: 0.75rem; right: 0.75rem; top: auto; z-index: 90; max-height: 55vh; transform: translateY(120%); transition: transform 0.25s; }
  .palette.open { transform: translateY(0); }
}
.fab {
  display: none; position: fixed; bottom: 1.25rem; left: 1.25rem; z-index: 100;
  width: 52px; height: 52px; border-radius: 50%; border: none;
  background: var(--primary); color: white; font-size: 1.5rem; font-weight: 700;
  box-shadow: 0 8px 24px rgba(0,0,0,0.4); cursor: pointer; touch-action: manipulation;
}
@media (max-width: 768px) { .fab { display: flex; align-items: center; justify-content: center; } }
.cat-group { margin-bottom: 0.75rem; }
.cat-label { font-size: 0.7rem; text-transform: uppercase; color: var(--muted); margin-bottom: 0.35rem; letter-spacing: 0.05em; }
.palette-btn {
  display: flex; align-items: center; gap: 0.5rem; width: 100%;
  padding: 0.5rem 0.65rem; margin-bottom: 0.25rem;
  border: 1px dashed rgba(148,163,184,0.25); border-radius: 8px;
  background: rgba(15,23,42,0.4); color: var(--text); cursor: pointer;
  font-family: inherit; font-size: 0.8rem; touch-action: manipulation;
}
.palette-btn:active { background: color-mix(in srgb, var(--primary) 15%, transparent); }
.icon { width: 1.25rem; text-align: center; }
.canvas-toolbar { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.75rem; flex-wrap: wrap; gap: 0.5rem; font-size: 0.9rem; }
.toolbar-actions { display: flex; gap: 0.35rem; }
.templates { padding: 1rem; margin-bottom: 0.85rem; }
.tpl-hint { font-size: 0.82rem; color: var(--muted); margin-bottom: 0.75rem; }
.tpl-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(150px, 1fr)); gap: 0.6rem; }
.tpl-card {
  display: flex; flex-direction: column; gap: 0.2rem; text-align: start;
  padding: 0.75rem; border: 1px solid var(--border); border-radius: 12px;
  background: var(--input-bg); color: var(--text); cursor: pointer; font-family: inherit;
  transition: border-color 0.15s, transform 0.1s;
}
.tpl-card:hover { border-color: color-mix(in srgb, var(--primary) 50%, transparent); transform: translateY(-2px); }
.tpl-ic { font-size: 1.4rem; }
.tpl-card strong { font-size: 0.88rem; }
.tpl-card small { font-size: 0.72rem; color: var(--muted); line-height: 1.4; }
.custom-tpls { margin-top: 1rem; padding-top: 0.85rem; border-top: 1px solid var(--border); }
.ct-head { display: flex; justify-content: space-between; align-items: center; gap: 0.5rem; flex-wrap: wrap; margin-bottom: 0.4rem; }
.tpl-card.custom { gap: 0.35rem; }
.ct-actions { display: flex; align-items: center; gap: 0.35rem; margin-top: 0.35rem; }
.ct-actions .btn { flex: 1; }
.ct-del { border: none; background: none; cursor: pointer; font-size: 0.95rem; padding: 0.3rem; }
.ct-del:hover { filter: brightness(1.3); }
.ct-empty { color: var(--muted); font-size: 0.82rem; }
.empty { text-align: center; color: var(--muted); padding: 2rem 1rem; border: 2px dashed rgba(148,163,184,0.2); border-radius: 12px; }
.preview-panel { margin-top: 1rem; padding: 1rem; overflow-x: auto; }
.preview-panel h3 { margin-bottom: 0.75rem; font-size: 0.85rem; color: var(--muted); }
@media (max-width: 480px) { .hide-mobile { display: none; } }
</style>
