<script setup>
import { ref, watch, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { api } from '@/api/client'
import { useToast } from '@/composables/useToast'
import { parseFeaturedStyles } from '@/utils/featuredStyles'
import { KNOWN, secIcon, secKey } from './home/sections'
import SectionsBlock from './home/SectionsBlock.vue'
import CardLayoutBlock from './home/CardLayoutBlock.vue'
import HeroTextBlock from './home/HeroTextBlock.vue'
import FeaturedStylesBlock from './home/FeaturedStylesBlock.vue'
import CompletedBlock from './home/CompletedBlock.vue'
import ProgressBarBlock from './home/ProgressBarBlock.vue'
import AmountTextBlock from './home/AmountTextBlock.vue'
import ContributorsBlock from './home/ContributorsBlock.vue'

const { t } = useI18n()
const toast = useToast()

const values = ref({})
const loading = ref(true)
const saving = ref(false)

// Keys this page owns. Only these are sent on save.
const KEYS = [
  'site.hero.fa', 'site.hero.en', 'site.hero.badge.fa', 'site.hero.badge.en', 'site.home.order',
  'featured.styles',
  'site.completed.show', 'site.completed.title.fa', 'site.completed.title.en',
  'site.home.columns', 'site.home.merge.featured', 'site.card.image.fit',
  'site.progress.mode', 'site.progress.color.start', 'site.progress.color.end', 'site.progress.show.percent',
  'site.progress.animate', 'site.progress.animate.ms', 'site.progress.track.color',
  'site.progress.flow', 'site.progress.flow.style', 'site.progress.flow.ms',
  'donation.progress.format.fa', 'donation.progress.format.en', 'donation.progress.highlight',
  'donation.progress.color.collected', 'donation.progress.color.target',
  'donation.progress.color.remaining', 'donation.progress.color.percent',
  'donation.progress.color.text', 'donation.progress.size',
  'donors.source'
]

// Two settings are edited as structured data and written back as strings on change.
const order = ref([])
const styleRows = ref([])
watch(order, (v) => { values.value['site.home.order'] = v.join(',') }, { deep: true })
watch(styleRows, (rows) => {
  values.value['featured.styles'] = JSON.stringify(rows.filter(s => s.id && (s.labelFa || s.labelEn)))
}, { deep: true })

onMounted(async () => {
  try {
    const groups = await api('/settings')
    const map = {}
    for (const g of groups) for (const it of g.items) map[it.key] = it.value
    // "featured.styles" is hidden from the raw settings list, so read it on its own.
    map['featured.styles'] = (await api('/settings/featured-styles')).json
    values.value = map
    order.value = (map['site.home.order'] || '').split(',').map(s => s.trim()).filter(s => KNOWN.includes(s))
    styleRows.value = parseFeaturedStyles(map['featured.styles'])
  } catch (e) { toast.error(e.message) } finally { loading.value = false }
})

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
        <h1>{{ t('homeEditor.title') }}</h1>
        <p class="sub">{{ t('homeEditor.subtitle') }}</p>
      </div>
      <div class="tb-actions">
        <a href="/" target="_blank" rel="noopener" class="btn btn-ghost btn-sm">
          <svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="9"/><path d="M3 12h18M12 3a14 14 0 0 1 0 18M12 3a14 14 0 0 0 0 18"/></svg>
          {{ t('homeEditor.viewSite') }}
        </a>
        <button class="btn btn-primary" :disabled="saving" @click="save">{{ saving ? '...' : t('save') }}</button>
      </div>
    </div>

    <div class="editor-grid">
      <div class="controls">
        <SectionsBlock v-model="order" />
        <CardLayoutBlock :values="values" />
        <HeroTextBlock :values="values" />
        <FeaturedStylesBlock v-model="styleRows" />
        <CompletedBlock :values="values" />
        <ProgressBarBlock :values="values" />
        <AmountTextBlock :values="values" />
        <ContributorsBlock :values="values" />
      </div>

      <!-- Live arrangement preview -->
      <aside class="preview">
        <p class="preview-title">{{ t('homeEditor.layoutPreview') }}</p>
        <div class="phone">
          <div v-for="id in order" :key="id" class="pv-block" :class="'pv-' + id">
            <svg class="pv-ic" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path :d="secIcon(id)"/></svg>
            <span>{{ t(secKey(id)) }}</span>
          </div>
          <p v-if="!order.length" class="pv-empty">{{ t('homeEditor.noSections') }}</p>
        </div>
      </aside>
    </div>
  </div>
</template>

<!-- Form styling shared by the blocks; namespaced under .home-editor, so not scoped. -->
<style src="./home/blocks.css"></style>

<style scoped>
.toolbar { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; flex-wrap: wrap; margin-bottom: 1.25rem; }
.sub { color: var(--muted); font-size: 0.9rem; margin-top: 0.2rem; }
.tb-actions { display: flex; gap: 0.5rem; }
.editor-grid { display: grid; grid-template-columns: 1fr 300px; gap: 1.25rem; align-items: start; }
.controls { display: flex; flex-direction: column; gap: 1rem; }

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
