<script setup>
import { useI18n } from 'vue-i18n'
import { formatAmount } from '@/utils/amount'
import { getLocalized, youtubeEmbed, SPACER_CLASS, ALIGN_CLASS, GAP_CLASS } from '@/utils/pageBlocks'
import PageBlockRenderer from '@/components/PageBlockRenderer.vue'
import ProgressBar from '@/components/ProgressBar.vue'

const props = defineProps({
  blocks: { type: Array, default: () => [] },
  campaign: { type: Object, default: null },
  preview: { type: Boolean, default: false }
})
const emit = defineEmits(['donate'])
const { locale, t } = useI18n()

const fmt = (n) => formatAmount(n, locale.value)
function txt(data, key) {
  return getLocalized(data, key, locale.value)
}
function headingTag(level) {
  const l = Number(level) || 2
  return l <= 1 ? 'h1' : l === 2 ? 'h2' : 'h3'
}
function listItems(data) {
  const key = locale.value === 'fa' ? 'itemsFa' : 'itemsEn'
  const items = data?.[key] || data?.itemsFa || data?.itemsEn || []
  return items.filter(Boolean)
}
function alignCls(a) {
  return ALIGN_CLASS[a] || 'align-start'
}
</script>

<template>
  <div class="page-blocks" :class="{ preview }">
    <template v-for="block in blocks" :key="block.id">
      <!-- Columns -->
      <div v-if="block.type === 'columns'" class="block-columns" :class="[GAP_CLASS[block.data?.gap] || 'gap-md']">
        <div
          v-for="(col, ci) in (block.data?.columns || [])"
          :key="ci"
          class="col"
          :style="{ flex: `1 1 ${100 / (block.data?.count || col.length || 1)}%` }"
        >
          <PageBlockRenderer :blocks="col" :campaign="campaign" :preview="preview" @donate="emit('donate')" />
        </div>
      </div>

      <!-- Section -->
      <div
        v-else-if="block.type === 'section'"
        class="block-section"
        :class="alignCls(block.data?.align)"
        :style="block.data?.bgColor ? { background: block.data.bgColor, padding: '1.5rem', borderRadius: '12px' } : {}"
      >
        <PageBlockRenderer :blocks="block.data?.blocks || []" :campaign="campaign" :preview="preview" @donate="emit('donate')" />
      </div>

      <!-- Spacer -->
      <div v-else-if="block.type === 'spacer'" class="block-spacer" :class="SPACER_CLASS[block.data?.size] || 'sp-md'" />

      <!-- Heading -->
      <component
        v-else-if="block.type === 'heading'"
        :is="headingTag(block.data?.level)"
        class="block-heading"
        :class="alignCls(block.data?.align)"
      >{{ txt(block.data, 'text') }}</component>

      <!-- Text -->
      <div v-else-if="block.type === 'text'" class="block-text" :class="alignCls(block.data?.align)">{{ txt(block.data, 'content') }}</div>

      <!-- List -->
      <ul v-else-if="block.type === 'list'" class="block-list-items">
        <li v-for="(item, i) in listItems(block.data)" :key="i">{{ item }}</li>
      </ul>

      <!-- Image -->
      <figure v-else-if="block.type === 'image' && block.data?.url" class="block-image" :class="{ full: block.data.fullWidth }">
        <img :src="block.data.url" :alt="txt(block.data, 'caption')" loading="lazy" />
        <figcaption v-if="txt(block.data, 'caption')">{{ txt(block.data, 'caption') }}</figcaption>
      </figure>

      <!-- Gallery -->
      <div v-else-if="block.type === 'gallery'" class="block-gallery" :style="{ gridTemplateColumns: `repeat(${Math.min(block.data?.columns || 3, 5)}, 1fr)` }">
        <figure v-for="(img, i) in (block.data?.images || [])" :key="i" v-show="img.url">
          <img :src="img.url" :alt="getLocalized(img, 'caption', locale)" loading="lazy" />
        </figure>
      </div>

      <!-- Video -->
      <div v-else-if="block.type === 'video' && block.data?.url" class="block-video">
        <iframe :src="youtubeEmbed(block.data.url)" frameborder="0" allowfullscreen loading="lazy" />
      </div>

      <!-- Quote -->
      <blockquote v-else-if="block.type === 'quote'" class="block-quote">
        <p>{{ txt(block.data, 'text') }}</p>
        <footer v-if="txt(block.data, 'author')">— {{ txt(block.data, 'author') }}</footer>
      </blockquote>

      <!-- Callout -->
      <div v-else-if="block.type === 'callout'" class="block-callout" :class="'callout-' + (block.data?.style || 'info')">
        <span v-if="block.data?.icon">{{ block.data.icon }}</span>
        <p>{{ txt(block.data, 'text') }}</p>
      </div>

      <!-- Card -->
      <div v-else-if="block.type === 'card'" class="block-card card">
        <span class="card-icon">{{ block.data?.icon || '♥' }}</span>
        <h3>{{ txt(block.data, 'title') }}</h3>
        <p>{{ txt(block.data, 'text') }}</p>
      </div>

      <!-- Stats -->
      <div v-else-if="block.type === 'stats' && campaign" class="block-stats card">
        <ProgressBar :percent="campaign.progressPercent" :height="12" />
        <div class="stats-row">
          <div><span class="label">{{ t('collected') }}</span><strong>{{ fmt(campaign.collectedAmount) }}</strong></div>
          <div><span class="label">{{ t('target') }}</span><strong>{{ fmt(campaign.targetAmount) }}</strong></div>
          <div><span class="label">{{ t('donors') }}</span><strong>{{ campaign.donorCount }}</strong></div>
        </div>
      </div>

      <!-- CTA -->
      <div v-else-if="block.type === 'cta'" class="block-cta" :class="alignCls(block.data?.align)">
        <button class="btn btn-primary btn-lg" @click="emit('donate')">{{ txt(block.data, 'text') || t('pay') }}</button>
      </div>

      <!-- Link button -->
      <div v-else-if="block.type === 'button' && block.data?.url" class="block-cta" :class="alignCls(block.data?.align)">
        <a :href="block.data.url" class="btn btn-ghost" target="_blank" rel="noopener">{{ txt(block.data, 'text') }}</a>
      </div>

      <!-- Divider -->
      <hr v-else-if="block.type === 'divider'" class="block-divider" />
    </template>
  </div>
</template>

<style scoped>
.page-blocks { display: flex; flex-direction: column; gap: 1.25rem; }
.block-columns { display: flex; flex-wrap: wrap; width: 100%; }
.block-columns.gap-sm { gap: 0.75rem; }
.block-columns.gap-md { gap: 1.25rem; }
.block-columns.gap-lg { gap: 2rem; }
.col { min-width: 0; flex: 1 1 200px; }
@media (max-width: 640px) { .block-columns { flex-direction: column; } .col { flex: 1 1 100%; } }
.block-section { width: 100%; }
.align-start { text-align: start; }
.align-center { text-align: center; }
.align-end { text-align: end; }
.block-spacer.sp-sm { height: 0.75rem; }
.block-spacer.sp-md { height: 1.5rem; }
.block-spacer.sp-lg { height: 2.5rem; }
.block-spacer.sp-xl { height: 4rem; }
.block-heading { font-size: clamp(1.25rem, 4vw, 1.75rem); line-height: 1.4; word-break: break-word; }
.block-heading:first-child { font-size: clamp(1.5rem, 5vw, 2rem); }
.block-text { color: var(--muted); white-space: pre-wrap; line-height: 1.85; font-size: clamp(0.95rem, 2.5vw, 1.05rem); word-break: break-word; }
.block-list-items { padding-inline-start: 1.25rem; color: var(--muted); line-height: 1.9; }
.block-image img { width: 100%; border-radius: 12px; display: block; }
.block-image.full img { border-radius: 0; margin: 0 -1.5rem; width: calc(100% + 3rem); max-width: none; }
@media (max-width: 640px) { .block-image.full img { margin: 0; width: 100%; } }
.block-image figcaption { text-align: center; color: var(--muted); font-size: 0.85rem; margin-top: 0.5rem; }
.block-gallery { display: grid; gap: 0.75rem; }
@media (max-width: 480px) { .block-gallery { grid-template-columns: 1fr 1fr !important; } }
.block-gallery img { width: 100%; border-radius: 10px; aspect-ratio: 4/3; object-fit: cover; }
.block-video { position: relative; padding-bottom: 56.25%; height: 0; border-radius: 12px; overflow: hidden; }
.block-video iframe { position: absolute; inset: 0; width: 100%; height: 100%; }
.block-quote { border-inline-start: 4px solid var(--accent); padding: 1rem 1.25rem; background: rgba(245,158,11,0.08); border-radius: 0 12px 12px 0; font-style: italic; }
.block-callout { display: flex; gap: 0.75rem; padding: 1rem 1.25rem; border-radius: 12px; align-items: flex-start; }
.callout-info { background: rgba(59,130,246,0.12); border: 1px solid rgba(59,130,246,0.25); }
.callout-success { background: rgba(16,185,129,0.12); border: 1px solid rgba(16,185,129,0.25); }
.callout-warning { background: rgba(245,158,11,0.12); border: 1px solid rgba(245,158,11,0.25); }
.block-card { text-align: center; padding: 1.5rem; }
.card-icon { font-size: 2rem; display: block; margin-bottom: 0.5rem; }
.block-card h3 { margin-bottom: 0.5rem; }
.block-card p { color: var(--muted); font-size: 0.95rem; }
.block-stats { padding: 1.25rem; }
.stats-row { display: grid; grid-template-columns: repeat(3, 1fr); gap: 0.75rem; margin-top: 1rem; text-align: center; }
@media (max-width: 400px) { .stats-row { grid-template-columns: 1fr; gap: 0.5rem; } }
.stats-row strong { display: block; font-size: 1.1rem; margin-top: 0.25rem; }
.block-cta { padding: 0.35rem 0; }
.btn-lg { padding: 0.85rem 2rem; font-size: clamp(0.95rem, 2.5vw, 1.05rem); width: 100%; max-width: 320px; }
.block-divider { border: none; border-top: 1px solid rgba(148,163,184,0.2); margin: 0.25rem 0; }
.preview .block-image.full img { margin: 0; width: 100%; }
</style>
