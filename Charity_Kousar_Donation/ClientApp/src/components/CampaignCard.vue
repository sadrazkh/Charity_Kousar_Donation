<script setup>
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import FeaturedBanner from '@/components/FeaturedBanner.vue'
import ProgressBar from '@/components/ProgressBar.vue'
import ProgressAmount from '@/components/ProgressAmount.vue'
import { useSiteConfig } from '@/composables/useSiteConfig'
import { featuredStyleFor } from '@/utils/featuredStyles'

const props = defineProps({ campaign: { type: Object, required: true } })
const emit = defineEmits(['donate'])
const { locale, t } = useI18n()
const { config } = useSiteConfig()

// The highlight ring follows the campaign's featured style (gold, red, blue, ...).
const accent = computed(() => featuredStyleFor(props.campaign, config).color)
const completed = computed(() => props.campaign.isCompleted === true)

// Square ready-made illustrations look best uncropped ("contain"); photos usually want "cover".
const contain = computed(() => config.cardImageFit === 'contain')
const thumbStyle = computed(() => ({
  backgroundImage: `url(${props.campaign.imageUrl})`,
  backgroundSize: contain.value ? 'contain' : 'cover'
}))

function title() {
  return locale.value === 'fa' ? props.campaign.titleFa : props.campaign.titleEn
}
function desc() {
  const d = locale.value === 'fa' ? props.campaign.descriptionFa : props.campaign.descriptionEn
  return d?.length > 120 ? d.slice(0, 120) + '…' : d
}
</script>

<template>
  <article class="card campaign-card" :class="{ featured: campaign.isFeatured && !completed, done: completed }"
    :style="{ '--card-accent': accent }">
    <div v-if="campaign.imageUrl" class="thumb" :class="{ contain }" :style="thumbStyle" />
    <div v-else class="thumb placeholder" aria-hidden="true">
      <svg class="ph-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round"><path d="M20.8 4.6a5.5 5.5 0 0 0-7.8 0L12 5.6l-1-1a5.5 5.5 0 0 0-7.8 7.8l1 1L12 21l7.8-7.6 1-1a5.5 5.5 0 0 0 0-7.8z"/></svg>
    </div>
    <div class="body">
      <span v-if="completed" class="done-badge">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M20 6 9 17l-5-5"/></svg>
        {{ locale === 'fa' ? 'هدف تأمین شد' : 'Goal reached' }}
      </span>
      <FeaturedBanner v-else-if="campaign.isFeatured" :campaign="campaign" compact />
      <h3><router-link :to="`/c/${campaign.slug}`" class="title-link">{{ title() }}</router-link></h3>
      <p class="desc">{{ desc() }}</p>
      <ProgressBar :percent="campaign.progressPercent" />
      <div class="stats">
        <ProgressAmount :collected="campaign.collectedAmount" :target="campaign.targetAmount" />
      </div>
      <div class="actions">
        <router-link v-if="completed" :to="`/c/${campaign.slug}`" class="btn btn-ghost btn-sm pay-btn">
          {{ locale === 'fa' ? 'مشاهده پرونده' : 'View project' }}
        </router-link>
        <button v-else class="btn btn-primary btn-sm pay-btn" @click="emit('donate', campaign)">{{ t('pay') }}</button>
      </div>
    </div>
  </article>
</template>

<style scoped>
.campaign-card { padding: 0; overflow: hidden; display: flex; flex-direction: column; }
.campaign-card.featured {
  border-color: color-mix(in srgb, var(--card-accent, var(--accent)) 50%, transparent);
  box-shadow: 0 0 0 1px color-mix(in srgb, var(--card-accent, var(--accent)) 20%, transparent),
    0 8px 24px color-mix(in srgb, var(--card-accent, var(--accent)) 12%, transparent);
}
.campaign-card.done { border-color: color-mix(in srgb, var(--success) 40%, transparent); }
.campaign-card.done .thumb { filter: saturate(0.85); }
.done-badge {
  display: inline-flex; align-items: center; gap: 0.35rem; align-self: flex-start;
  padding: 0.25rem 0.7rem; border-radius: 999px;
  font-size: 0.78rem; font-weight: 700;
  color: var(--success);
  background: color-mix(in srgb, var(--success) 15%, transparent);
}
.done-badge svg { width: 0.9rem; height: 0.9rem; }
.thumb { height: 160px; background-size: cover; background-position: center; background-repeat: no-repeat; }
.thumb.contain { background-color: var(--bg-soft); }
.thumb.placeholder {
  display: flex; align-items: center; justify-content: center;
  background:
    radial-gradient(circle at 30% 30%, color-mix(in srgb, var(--primary) 28%, transparent), transparent 60%),
    radial-gradient(circle at 75% 70%, color-mix(in srgb, var(--accent) 22%, transparent), transparent 55%),
    var(--bg-soft);
}
.ph-icon { width: 3rem; height: 3rem; color: color-mix(in srgb, var(--primary) 70%, var(--text)); opacity: 0.55; }
.campaign-card { transition: transform 0.18s ease, box-shadow 0.18s ease, border-color 0.18s ease; }
.campaign-card:hover { transform: translateY(-3px); border-color: color-mix(in srgb, var(--primary) 35%, var(--border)); }
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
