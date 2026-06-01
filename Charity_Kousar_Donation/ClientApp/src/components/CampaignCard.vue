<script setup>
import { useI18n } from 'vue-i18n'
import { formatAmount } from '@/utils/amount'

const props = defineProps({ campaign: { type: Object, required: true } })
const emit = defineEmits(['donate'])
const { locale, t } = useI18n()

function title() {
  return locale.value === 'fa' ? props.campaign.titleFa : props.campaign.titleEn
}
function desc() {
  const d = locale.value === 'fa' ? props.campaign.descriptionFa : props.campaign.descriptionEn
  return d?.length > 120 ? d.slice(0, 120) + '…' : d
}
const fmt = (n) => formatAmount(n, locale.value)
</script>

<template>
  <article class="card campaign-card" :class="{ featured: campaign.isFeatured }">
    <div v-if="campaign.imageUrl" class="thumb" :style="{ backgroundImage: `url(${campaign.imageUrl})` }" />
    <div v-else class="thumb placeholder" />
    <div class="body">
      <h3>{{ title() }}</h3>
      <p class="desc">{{ desc() }}</p>
      <div class="progress-bar"><div class="progress-bar-fill" :style="{ width: campaign.progressPercent + '%' }" /></div>
      <div class="stats">
        <span>{{ fmt(campaign.collectedAmount) }} / {{ fmt(campaign.targetAmount) }} {{ t('toman') }}</span>
        <span class="pct">{{ campaign.progressPercent }}%</span>
      </div>
      <div class="actions">
        <router-link :to="`/c/${campaign.slug}`" class="btn btn-ghost btn-sm">{{ t('donate') }}</router-link>
        <button class="btn btn-primary btn-sm" @click="emit('donate', campaign)">{{ t('donateNow') }}</button>
      </div>
    </div>
  </article>
</template>

<style scoped>
.campaign-card { padding: 0; overflow: hidden; display: flex; flex-direction: column; }
.campaign-card.featured { border-color: color-mix(in srgb, var(--accent) 50%, transparent); }
.thumb { height: 160px; background-size: cover; background-position: center; }
.thumb.placeholder { background: linear-gradient(135deg, var(--bg-soft), var(--primary)); opacity: 0.5; }
.body { padding: 1.25rem; flex: 1; display: flex; flex-direction: column; gap: 0.75rem; }
.body h3 { font-size: 1.1rem; }
.desc { color: var(--muted); font-size: 0.9rem; flex: 1; }
.stats { display: flex; justify-content: space-between; font-size: 0.85rem; color: var(--muted); font-variant-numeric: tabular-nums; }
.pct { color: var(--accent); font-weight: 600; }
.actions { display: flex; gap: 0.5rem; justify-content: flex-end; flex-wrap: wrap; }
</style>
