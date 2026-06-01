<script setup>
import { ref, computed } from 'vue'
import { useI18n } from 'vue-i18n'
import draggable from 'vuedraggable'
import AiAssistButton from '@/components/AiAssistButton.vue'
import ImageUpload from '@/components/ImageUpload.vue'
import NestedBlockList from '@/components/NestedBlockList.vue'
import {
  BLOCK_TYPES, NESTABLE_TYPES, createBlock, blockLabel, blockPreviewText,
  resizeColumns, moveBlock
} from '@/utils/pageBlocks'

const props = defineProps({
  modelValue: { type: Array, default: () => [] },
  campaignTitle: String,
  depth: { type: Number, default: 0 },
  compact: { type: Boolean, default: false }
})
const emit = defineEmits(['update:modelValue'])

const { locale } = useI18n()
const blocks = computed({
  get: () => props.modelValue,
  set: v => emit('update:modelValue', v)
})
const expanded = ref(null)

const paletteTypes = computed(() =>
  props.depth > 0
    ? BLOCK_TYPES.filter(b => NESTABLE_TYPES.includes(b.type))
    : BLOCK_TYPES)

function addBlock(type) {
  blocks.value = [...blocks.value, createBlock(type)]
  expanded.value = blocks.value[blocks.value.length - 1].id
}

function removeBlock(id) {
  blocks.value = blocks.value.filter(b => b.id !== id)
}

function duplicateBlock(id) {
  const src = blocks.value.find(b => b.id === id)
  if (!src) return
  const copy = createBlock(src.type)
  copy.data = JSON.parse(JSON.stringify(src.data))
  const idx = blocks.value.findIndex(b => b.id === id)
  const next = [...blocks.value]
  next.splice(idx + 1, 0, copy)
  blocks.value = next
}

function shiftBlock(id, dir) {
  blocks.value = moveBlock(blocks.value, id, dir)
}

function setColumnCount(block, n) {
  resizeColumns(block, n)
}

function addGalleryImage(block) {
  block.data.images.push({ url: '', captionFa: '', captionEn: '' })
}

function addListItem(block, lang) {
  const key = lang === 'fa' ? 'itemsFa' : 'itemsEn'
  if (!block.data[key]) block.data[key] = []
  block.data[key].push('')
}

function label(type) {
  return blockLabel(type, locale.value)
}
</script>

<template>
  <draggable
    v-model="blocks"
    item-key="id"
    handle=".drag-handle"
    class="block-list"
    :class="{ nested: depth > 0 }"
    ghost-class="ghost"
    animation="200"
    :delay="80"
    :delay-on-touch-only="true"
    :touch-start-threshold="5"
  >
    <template #item="{ element: block }">
      <div class="block-item card" :class="{ expanded: expanded === block.id, compact }">
        <div class="block-header" @click="expanded = expanded === block.id ? null : block.id">
          <span class="drag-handle" aria-label="Drag">⠿</span>
          <span class="block-type-badge">{{ label(block.type) }}</span>
          <span class="block-preview-text">{{ blockPreviewText(block) }}</span>
          <div class="block-actions" @click.stop>
            <button type="button" class="icon-btn" title="Up" @click="shiftBlock(block.id, -1)">↑</button>
            <button type="button" class="icon-btn" title="Down" @click="shiftBlock(block.id, 1)">↓</button>
            <button type="button" class="icon-btn" @click="duplicateBlock(block.id)">⧉</button>
            <button type="button" class="icon-btn danger" @click="removeBlock(block.id)">✕</button>
          </div>
        </div>

        <div v-if="expanded === block.id" class="block-editor">
          <!-- Columns -->
          <template v-if="block.type === 'columns'">
            <label class="label">{{ locale === 'fa' ? 'تعداد ستون (۱ تا ۵)' : 'Columns (1–5)' }}</label>
            <div class="col-picker">
              <button v-for="n in 5" :key="n" type="button"
                class="col-btn" :class="{ active: block.data.count === n }"
                @click="setColumnCount(block, n)">{{ n }}</button>
            </div>
            <label class="label">{{ locale === 'fa' ? 'فاصله' : 'Gap' }}</label>
            <select v-model="block.data.gap" class="select">
              <option value="sm">کم</option><option value="md">متوسط</option><option value="lg">زیاد</option>
            </select>
            <div class="columns-editor" :style="{ gridTemplateColumns: `repeat(${block.data.count}, 1fr)` }">
              <div v-for="(col, ci) in block.data.columns" :key="ci" class="col-cell">
                <p class="col-label">{{ locale === 'fa' ? `ستون ${ci + 1}` : `Col ${ci + 1}` }}</p>
                <NestedBlockList v-model="block.data.columns[ci]" :depth="depth + 1" :campaign-title="campaignTitle" compact />
              </div>
            </div>
          </template>

          <!-- Section -->
          <template v-else-if="block.type === 'section'">
            <label class="label">{{ locale === 'fa' ? 'رنگ پس‌زمینه' : 'Background' }}</label>
            <input v-model="block.data.bgColor" class="input" placeholder="#1e293b یا خالی" />
            <label class="label">{{ locale === 'fa' ? 'تراز' : 'Align' }}</label>
            <select v-model="block.data.align" class="select">
              <option value="start">{{ locale === 'fa' ? 'راست/چپ' : 'Start' }}</option>
              <option value="center">{{ locale === 'fa' ? 'وسط' : 'Center' }}</option>
            </select>
            <NestedBlockList v-model="block.data.blocks" :depth="depth + 1" :campaign-title="campaignTitle" />
          </template>

          <!-- Heading -->
          <template v-else-if="block.type === 'heading'">
            <label class="label">عنوان فارسی</label>
            <input v-model="block.data.textFa" class="input input-rtl" />
            <AiAssistButton v-model="block.data.textFa" language="fa" field-type="heading" :campaign-title="campaignTitle" />
            <label class="label">Title English</label>
            <input v-model="block.data.textEn" class="input input-ltr" dir="ltr" />
            <AiAssistButton v-model="block.data.textEn" language="en" field-type="heading" :campaign-title="campaignTitle" />
            <div class="row-2">
              <div><label class="label">سطح</label>
                <select v-model.number="block.data.level" class="select"><option :value="1">H1</option><option :value="2">H2</option><option :value="3">H3</option></select>
              </div>
              <div><label class="label">تراز</label>
                <select v-model="block.data.align" class="select"><option value="start">Start</option><option value="center">Center</option></select>
              </div>
            </div>
          </template>

          <!-- Text -->
          <template v-else-if="block.type === 'text'">
            <label class="label">متن فارسی</label>
            <textarea v-model="block.data.contentFa" class="textarea input-rtl" rows="5" />
            <AiAssistButton v-model="block.data.contentFa" language="fa" field-type="body" :campaign-title="campaignTitle" multiline />
            <label class="label">Text English</label>
            <textarea v-model="block.data.contentEn" class="textarea input-ltr" dir="ltr" rows="5" />
            <AiAssistButton v-model="block.data.contentEn" language="en" field-type="body" :campaign-title="campaignTitle" multiline />
            <select v-model="block.data.align" class="select"><option value="start">Start</option><option value="center">Center</option></select>
          </template>

          <!-- List -->
          <template v-else-if="block.type === 'list'">
            <label class="label">آیتم‌های فارسی</label>
            <div v-for="(item, i) in block.data.itemsFa" :key="'fa'+i" class="list-row">
              <input v-model="block.data.itemsFa[i]" class="input input-rtl" />
              <button type="button" class="icon-btn danger" @click="block.data.itemsFa.splice(i,1)">✕</button>
            </div>
            <button type="button" class="btn btn-ghost btn-sm" @click="addListItem(block,'fa')">+ FA</button>
            <label class="label">English items</label>
            <div v-for="(item, i) in block.data.itemsEn" :key="'en'+i" class="list-row">
              <input v-model="block.data.itemsEn[i]" class="input input-ltr" dir="ltr" />
              <button type="button" class="icon-btn danger" @click="block.data.itemsEn.splice(i,1)">✕</button>
            </div>
            <button type="button" class="btn btn-ghost btn-sm" @click="addListItem(block,'en')">+ EN</button>
          </template>

          <!-- Image -->
          <template v-else-if="block.type === 'image'">
            <label class="label">URL</label>
            <ImageUpload v-model="block.data.url" />
            <input v-model="block.data.captionFa" class="input input-rtl" placeholder="توضیح FA" />
            <input v-model="block.data.captionEn" class="input input-ltr" dir="ltr" placeholder="Caption EN" />
            <label><input type="checkbox" v-model="block.data.fullWidth" /> تمام‌عرض</label>
            <img v-if="block.data.url" :src="block.data.url" class="thumb-preview" alt="" />
          </template>

          <!-- Gallery -->
          <template v-else-if="block.type === 'gallery'">
            <label class="label">{{ locale === 'fa' ? 'ستون‌های گالری' : 'Gallery columns' }}</label>
            <select v-model.number="block.data.columns" class="select"><option v-for="n in 5" :key="n" :value="n">{{ n }}</option></select>
            <div v-for="(img, i) in block.data.images" :key="i" class="gallery-edit">
              <input v-model="img.url" class="input input-ltr" dir="ltr" placeholder="URL" />
              <button type="button" class="icon-btn danger" @click="block.data.images.splice(i,1)">✕</button>
            </div>
            <button type="button" class="btn btn-ghost btn-sm" @click="addGalleryImage(block)">+ {{ locale === 'fa' ? 'تصویر' : 'Image' }}</button>
          </template>

          <!-- Video -->
          <template v-else-if="block.type === 'video'">
            <input v-model="block.data.url" class="input input-ltr" dir="ltr" placeholder="YouTube URL" />
          </template>

          <!-- Quote -->
          <template v-else-if="block.type === 'quote'">
            <textarea v-model="block.data.textFa" class="textarea input-rtl" placeholder="FA" />
            <AiAssistButton v-model="block.data.textFa" language="fa" field-type="quote" :campaign-title="campaignTitle" />
            <textarea v-model="block.data.textEn" class="textarea input-ltr" dir="ltr" placeholder="EN" />
            <input v-model="block.data.authorFa" class="input input-rtl" placeholder="نویسنده" />
            <input v-model="block.data.authorEn" class="input input-ltr" dir="ltr" placeholder="Author" />
          </template>

          <!-- Callout -->
          <template v-else-if="block.type === 'callout'">
            <select v-model="block.data.style" class="select"><option value="info">Info</option><option value="success">Success</option><option value="warning">Warning</option></select>
            <textarea v-model="block.data.textFa" class="textarea input-rtl" />
            <AiAssistButton v-model="block.data.textFa" language="fa" field-type="body" :campaign-title="campaignTitle" />
            <textarea v-model="block.data.textEn" class="textarea input-ltr" dir="ltr" />
          </template>

          <!-- Card -->
          <template v-else-if="block.type === 'card'">
            <input v-model="block.data.icon" class="input" placeholder="Emoji icon" style="max-width:80px" />
            <input v-model="block.data.titleFa" class="input input-rtl" placeholder="عنوان FA" />
            <textarea v-model="block.data.textFa" class="textarea input-rtl" />
            <input v-model="block.data.titleEn" class="input input-ltr" dir="ltr" placeholder="Title EN" />
            <textarea v-model="block.data.textEn" class="textarea input-ltr" dir="ltr" />
          </template>

          <!-- CTA / Button -->
          <template v-else-if="block.type === 'cta'">
            <input v-model="block.data.textFa" class="input input-rtl" />
            <AiAssistButton v-model="block.data.textFa" language="fa" field-type="cta" :campaign-title="campaignTitle" />
            <input v-model="block.data.textEn" class="input input-ltr" dir="ltr" />
            <select v-model="block.data.align" class="select"><option value="center">Center</option><option value="start">Start</option></select>
          </template>
          <template v-else-if="block.type === 'button'">
            <input v-model="block.data.textFa" class="input input-rtl" />
            <input v-model="block.data.textEn" class="input input-ltr" dir="ltr" />
            <input v-model="block.data.url" class="input input-ltr" dir="ltr" placeholder="https://..." />
          </template>

          <!-- Spacer / Stats / Divider -->
          <template v-else-if="block.type === 'spacer'">
            <select v-model="block.data.size" class="select"><option value="sm">کوچک</option><option value="md">متوسط</option><option value="lg">بزرگ</option><option value="xl">خیلی بزرگ</option></select>
          </template>
          <template v-else-if="block.type === 'stats'">
            <p class="hint">{{ locale === 'fa' ? 'آمار خودکار از کمپین' : 'Auto stats from campaign' }}</p>
          </template>
          <template v-else-if="block.type === 'divider'">
            <p class="hint">{{ locale === 'fa' ? 'خط جداکننده' : 'Divider line' }}</p>
          </template>
        </div>
      </div>
    </template>
  </draggable>

  <div v-if="depth === 0 || compact" class="add-row">
    <button v-for="bt in paletteTypes.slice(0, compact ? 6 : paletteTypes.length)" :key="bt.type"
      type="button" class="add-chip" @click="addBlock(bt.type)">
      {{ bt.icon }} {{ label(bt.type) }}
    </button>
  </div>
</template>

<style scoped>
.block-list { display: flex; flex-direction: column; gap: 0.65rem; min-height: 40px; }
.block-list.nested { padding: 0.35rem; background: rgba(0,0,0,0.15); border-radius: 10px; }
.block-item { padding: 0; overflow: hidden; }
.block-item.compact .block-header { padding: 0.5rem 0.65rem; }
.block-header { display: flex; align-items: center; gap: 0.5rem; padding: 0.75rem 1rem; cursor: pointer; touch-action: manipulation; }
.drag-handle { cursor: grab; color: var(--muted); font-size: 1.25rem; padding: 0.25rem; min-width: 28px; text-align: center; touch-action: none; }
.block-type-badge { background: color-mix(in srgb, var(--primary) 20%, transparent); color: var(--primary); font-size: 0.7rem; font-weight: 600; padding: 0.15rem 0.5rem; border-radius: 999px; white-space: nowrap; }
.block-preview-text { flex: 1; color: var(--muted); font-size: 0.8rem; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; min-width: 0; }
.block-actions { display: flex; gap: 0.15rem; flex-shrink: 0; }
.icon-btn { background: none; border: none; color: var(--muted); cursor: pointer; padding: 0.35rem 0.45rem; border-radius: 6px; font-size: 0.85rem; min-width: 32px; min-height: 32px; touch-action: manipulation; }
.icon-btn.danger:hover { color: #f87171; }
.block-editor { padding: 0 1rem 1rem; display: flex; flex-direction: column; gap: 0.55rem; border-top: 1px solid rgba(148,163,184,0.1); }
.col-picker { display: flex; gap: 0.35rem; flex-wrap: wrap; }
.col-btn { width: 40px; height: 40px; border-radius: 10px; border: 1px solid rgba(148,163,184,0.3); background: rgba(15,23,42,0.5); color: var(--text); cursor: pointer; font-weight: 700; touch-action: manipulation; }
.col-btn.active { background: var(--primary); border-color: var(--primary); color: white; }
.columns-editor { display: grid; gap: 0.75rem; margin-top: 0.5rem; }
@media (max-width: 640px) { .columns-editor { grid-template-columns: 1fr !important; } }
.col-cell { border: 1px dashed rgba(148,163,184,0.25); border-radius: 10px; padding: 0.5rem; min-height: 80px; }
.col-label { font-size: 0.75rem; color: var(--muted); margin-bottom: 0.35rem; }
.row-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 0.5rem; }
.list-row, .gallery-edit { display: flex; gap: 0.35rem; align-items: center; }
.list-row .input, .gallery-edit .input { flex: 1; }
.thumb-preview { max-height: 100px; border-radius: 8px; object-fit: cover; }
.hint { color: var(--muted); font-size: 0.85rem; }
.add-row { display: flex; flex-wrap: wrap; gap: 0.35rem; margin-top: 0.65rem; }
.add-chip { padding: 0.4rem 0.65rem; border-radius: 999px; border: 1px dashed rgba(148,163,184,0.35); background: rgba(15,23,42,0.4); color: var(--text); font-size: 0.75rem; cursor: pointer; font-family: inherit; touch-action: manipulation; }
.ghost { opacity: 0.35; }
</style>
