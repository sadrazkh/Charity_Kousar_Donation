<script setup>
import { useI18n } from 'vue-i18n'
import FeaturedBanner from '@/components/FeaturedBanner.vue'
import ProgressBar from '@/components/ProgressBar.vue'
import ProgressAmount from '@/components/ProgressAmount.vue'

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
</script>

<template>
  <article class="card campaign-card" :class="{ featured: campaign.isFeatured }">
    <div v-if="campaign.imageUrl" class="thumb" :style="{ backgroundImage: `url(${campaign.imageUrl})` }" />
    <div v-else class="thumb placeholder" />
    <div class="body">
      <FeaturedBanner v-if="campaign.isFeatured" :campaign="campaign" compact />
      <h3><router-link :to="`/c/${campaign.slug}`" class="title-link">{{ title() }}</router-link></h3>
      <p class="desc">{{ desc() }}</p>
      <ProgressBar :percent="campaign.progressPercent" />
      <div class="stats">
        <ProgressAmount :collected="campaign.collectedAmount" :target="campaign.targetAmount" />
      </div>
      <div class="actions">
        <button class="btn btn-primary btn-sm pay-btn" @click="emit('donate', campaign)">{{ t('pay') }}</button>
      </div>
    </div>
  </article>
</template>

<style scoped>
.campaign-card { padding: 0; overflow: hidden; display: flex; flex-direction: column; }
.campaign-card.featured {
  border-color: color-mix(in srgb, var(--accent) 50%, transparent);
  box-shadow: 0 0 0 1px color-mix(in srgb, var(--accent) 20%, transparent),
    0 8px 24px color-mix(in srgb, var(--accent) 12%, transparent);
}
.thumb { height: 160px; background-size: cover; background-position: center; }
.thumb.placeholder { background: linear-gradient(135deg, var(--bg-soft), var(--primary)); opacity: 0.5; }
.body { padding: 1.25rem; flex: 1; display: flex; flex-direction: column; gap: 0.75rem; }
.body h3 { font-size: 1.1rem; }
.desc { color: var(--muted); font-size: 0.9rem; flex: 1; }
.stats { display: flex; justify-content: space-between; font-size: 0.85rem; color: var(--muted); font-variant-numeric: tabular-nums; }
.pct { color: var(--accent); font-weight: 600; }
.actions { display: flex; gap: 0.5rem; justify-content: stretch; flex-wrap: wrap; }
.pay-btn { width: 100%; justify-content: center; }
.title-link { color: inherit; text-decoration: none; }
.title-link:hover { color: var(--primary); }
</style>
