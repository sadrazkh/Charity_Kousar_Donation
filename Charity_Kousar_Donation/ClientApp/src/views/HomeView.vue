<script setup>
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import AppHeader from '@/components/AppHeader.vue'
import CampaignCard from '@/components/CampaignCard.vue'
import DonationModal from '@/components/DonationModal.vue'
import RecentDonors from '@/components/RecentDonors.vue'
import { api } from '@/api/client'
import { formatAmount } from '@/utils/amount'
import { useSiteConfig } from '@/composables/useSiteConfig'

const { t, locale } = useI18n()
const { config } = useSiteConfig()
const campaigns = ref([])
const total = ref(0)
const selected = ref(null)

const heroText = computed(() => locale.value === 'fa' ? config.heroTextFa : config.heroTextEn)

// Section order is admin-configurable (e.g. "hero,featured,campaigns,donors").
const KNOWN_SECTIONS = ['hero', 'featured', 'campaigns', 'donors']
const sections = computed(() => {
  const raw = (config.homeOrder || 'hero,featured,campaigns,donors')
    .split(',').map(s => s.trim()).filter(s => KNOWN_SECTIONS.includes(s))
  return raw.length ? [...new Set(raw)] : KNOWN_SECTIONS
})

const featured = computed(() => campaigns.value.filter(c => c.isFeatured))
const hasFeaturedSection = computed(() => sections.value.includes('featured') && featured.value.length > 0)
// Avoid showing the same campaign twice (featured highlight + grid).
const gridCampaigns = computed(() =>
  hasFeaturedSection.value ? campaigns.value.filter(c => !c.isFeatured) : campaigns.value)

onMounted(async () => {
  try {
    const [list, stats] = await Promise.all([
      api('/campaigns'),
      api('/donations/stats/total')
    ])
    campaigns.value = list
    total.value = stats.totalCollected
  } catch { /* */ }
})

const fmt = (n) => formatAmount(n, locale.value)
</script>

<template>
  <AppHeader />
  <main class="container home">
    <template v-for="section in sections" :key="section">
      <!-- Hero -->
      <section v-if="section === 'hero'" class="hero card">
        <p class="hero-text">{{ heroText }}</p>
        <p class="hero-label">{{ t('totalCollected') }}</p>
        <p class="stat-value">{{ fmt(total) }} <span class="unit">{{ t('toman') }}</span></p>
      </section>

      <!-- Featured highlight -->
      <section v-else-if="section === 'featured' && hasFeaturedSection" class="featured-section">
        <h2 class="section-title">{{ locale === 'fa' ? '⭐ پروژه‌های ویژه' : '⭐ Featured projects' }}</h2>
        <div class="cards-grid">
          <CampaignCard
            v-for="c in featured"
            :key="c.id"
            :campaign="c"
            @donate="selected = c"
          />
        </div>
      </section>

      <!-- Campaigns grid -->
      <section v-else-if="section === 'campaigns'">
        <h2 v-if="gridCampaigns.length" class="section-title">{{ t('campaigns') }}</h2>
        <div v-if="gridCampaigns.length" class="cards-grid">
          <CampaignCard
            v-for="c in gridCampaigns"
            :key="c.id"
            :campaign="c"
            @donate="selected = c"
          />
        </div>
        <p v-else-if="!campaigns.length" class="empty">{{ t('noCampaigns') }}</p>
      </section>

      <!-- Recent contributors (global) -->
      <section v-else-if="section === 'donors' && config.showDonorsHome !== false">
        <RecentDonors />
      </section>
    </template>
  </main>

  <DonationModal
    v-if="selected"
    :campaign="selected"
    :config="config"
    @close="selected = null"
  />
</template>

<style scoped>
.home { display: flex; flex-direction: column; gap: 2.25rem; }
.hero { text-align: center; padding: 2.5rem; position: relative; overflow: hidden; }
.hero::before {
  content: '';
  position: absolute;
  inset: 0;
  background: linear-gradient(135deg, color-mix(in srgb, var(--primary) 15%, transparent), color-mix(in srgb, var(--accent) 10%, transparent));
  pointer-events: none;
}
.hero-text { font-size: 1.35rem; font-weight: 600; margin-bottom: 1.25rem; position: relative; line-height: 1.7; }
.hero-label { color: var(--muted); margin-bottom: 0.5rem; position: relative; }
.stat-value { position: relative; }
.unit { font-size: 1rem; color: var(--muted); -webkit-text-fill-color: var(--muted); }
.section-title { margin-bottom: 1.25rem; font-size: 1.35rem; }
.empty { color: var(--muted); text-align: center; padding: 3rem; }

/* Responsive card grid: up to 3 per row on desktop. Cards keep a natural width
   (max ~360px) instead of stretching when there are only 1–2 items. */
.cards-grid {
  display: grid;
  gap: 1.5rem;
  grid-template-columns: repeat(auto-fill, minmax(280px, 360px));
  justify-content: center;
}
@media (max-width: 620px) { .cards-grid { grid-template-columns: 1fr; } }
</style>
