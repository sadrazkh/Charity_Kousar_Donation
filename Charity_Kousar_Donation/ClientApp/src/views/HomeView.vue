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
// The chip above the hero title has its own text (separate from the header tagline).
const heroBadge = computed(() => locale.value === 'fa' ? config.heroBadgeFa : config.heroBadgeEn)
const activeCount = computed(() => openCampaigns.value.length)

function scrollToCampaigns() {
  const el = document.getElementById('campaigns-anchor')
  if (el) el.scrollIntoView({ behavior: 'smooth', block: 'start' })
  else window.scrollTo({ top: document.body.scrollHeight, behavior: 'smooth' })
}

// Section order is admin-configurable (e.g. "hero,featured,campaigns,donors").
const KNOWN_SECTIONS = ['hero', 'featured', 'campaigns', 'donors']
const sections = computed(() => {
  const raw = (config.homeOrder || 'hero,featured,campaigns,donors')
    .split(',').map(s => s.trim()).filter(s => KNOWN_SECTIONS.includes(s))
  return raw.length ? [...new Set(raw)] : KNOWN_SECTIONS
})

// Finished projects move out of the main list into their own tab.
const completedCampaigns = computed(() => campaigns.value.filter(c => c.isCompleted))
const openCampaigns = computed(() => campaigns.value.filter(c => !c.isCompleted))
const showCompletedTab = computed(() =>
  config.showCompletedTab !== false && completedCampaigns.value.length > 0)
const completedTitle = computed(() =>
  (locale.value === 'fa' ? config.completedTitleFa : config.completedTitleEn) ||
  (t('ui.completedProjects')))
const tab = ref('open')

const featured = computed(() => openCampaigns.value.filter(c => c.isFeatured))
// When "merge featured" is on, everything shows in one grid (featured just highlighted).
// Featured projects belong to the open list, so they step aside on the completed tab.
const hasFeaturedSection = computed(() =>
  !config.homeMergeFeatured && sections.value.includes('featured') &&
  featured.value.length > 0 && tab.value === 'open')
// Avoid showing the same campaign twice (featured highlight + grid).
const gridCampaigns = computed(() =>
  hasFeaturedSection.value ? openCampaigns.value.filter(c => !c.isFeatured) : openCampaigns.value)
// What the campaigns section renders for the selected tab.
const listedCampaigns = computed(() =>
  tab.value === 'completed' && showCompletedTab.value ? completedCampaigns.value : gridCampaigns.value)

// Card columns: 'auto' (responsive fill) or a fixed number (2/3/4) that still collapses on mobile.
const gridMode = computed(() => (config.homeColumns && config.homeColumns !== 'auto') ? 'fixed' : 'auto')
const gridStyle = computed(() => gridMode.value === 'fixed'
  ? { '--cols': Math.max(1, parseInt(config.homeColumns) || 3) } : {})

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
      <!-- Hero: storytelling + clear donate CTA + impact (charity best practice) -->
      <section v-if="section === 'hero'" class="hero">
        <div class="hero-content">
          <span v-if="heroBadge" class="hero-eyebrow">
            <svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20.8 4.6a5.5 5.5 0 0 0-7.8 0L12 5.6l-1-1a5.5 5.5 0 0 0-7.8 7.8l1 1L12 21l7.8-7.6 1-1a5.5 5.5 0 0 0 0-7.8z"/></svg>
            <span>{{ heroBadge }}</span>
          </span>
          <h1 class="hero-title">{{ heroText }}</h1>
          <div class="hero-cta">
            <button class="btn btn-primary btn-lg" @click="scrollToCampaigns">
              <svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20.8 4.6a5.5 5.5 0 0 0-7.8 0L12 5.6l-1-1a5.5 5.5 0 0 0-7.8 7.8l1 1L12 21l7.8-7.6 1-1a5.5 5.5 0 0 0 0-7.8z"/></svg>
              {{ t('donateNow') }}
            </button>
            <button class="btn btn-ghost btn-lg" @click="scrollToCampaigns">{{ t('campaigns') }}</button>
          </div>
          <div class="hero-trust">
            <span class="chip">
              <svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 3l7 3v5c0 4.5-3 7.6-7 9-4-1.4-7-4.5-7-9V6l7-3z"/><path d="M9 12l2 2 4-4"/></svg>
              {{ t('securePayment') }}
            </span>
          </div>
        </div>
        <div class="hero-impact card">
          <p class="impact-label">{{ t('totalCollected') }}</p>
          <p class="impact-value stat-value">{{ fmt(total) }} <span class="unit">{{ t('toman') }}</span></p>
          <div class="impact-mini">
            <div class="impact-cell">
              <strong>{{ fmt(activeCount) }}</strong>
              <span>{{ t('campaigns') }}</span>
            </div>
          </div>
        </div>
      </section>

      <!-- Featured highlight -->
      <section v-else-if="section === 'featured' && hasFeaturedSection" class="featured-section">
        <h2 class="section-title">
          <svg class="icon" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M12 2l2.9 6.1 6.6.9-4.8 4.6 1.2 6.6L12 17.8 6.1 20.8l1.2-6.6L2.5 9l6.6-.9L12 2z"/></svg>
          {{ t('ui.featuredProjects') }}
        </h2>
        <div class="cards-grid" :class="gridMode" :style="gridStyle">
          <CampaignCard
            v-for="c in featured"
            :key="c.id"
            :campaign="c"
            @donate="selected = c"
          />
        </div>
      </section>

      <!-- Campaigns grid — open projects, with finished ones on their own tab -->
      <section v-else-if="section === 'campaigns'" id="campaigns-anchor">
        <div v-if="campaigns.length" class="list-head">
          <h2 class="section-title">
            <svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20.8 4.6a5.5 5.5 0 0 0-7.8 0L12 5.6l-1-1a5.5 5.5 0 0 0-7.8 7.8l1 1L12 21l7.8-7.6 1-1a5.5 5.5 0 0 0 0-7.8z"/></svg>
            {{ tab === 'completed' ? completedTitle : t('campaigns') }}
          </h2>
          <div v-if="showCompletedTab" class="list-tabs" role="tablist">
            <button type="button" role="tab" :aria-selected="tab === 'open'"
              :class="{ active: tab === 'open' }" @click="tab = 'open'">
              {{ t('ui.inProgress') }}
              <span class="tab-count">{{ fmt(openCampaigns.length) }}</span>
            </button>
            <button type="button" role="tab" :aria-selected="tab === 'completed'"
              :class="{ active: tab === 'completed' }" @click="tab = 'completed'">
              {{ completedTitle }}
              <span class="tab-count">{{ fmt(completedCampaigns.length) }}</span>
            </button>
          </div>
        </div>

        <div v-if="listedCampaigns.length" class="cards-grid" :class="gridMode" :style="gridStyle">
          <CampaignCard
            v-for="c in listedCampaigns"
            :key="c.id"
            :campaign="c"
            @donate="selected = c"
          />
        </div>
        <p v-else-if="!campaigns.length" class="empty">{{ t('noCampaigns') }}</p>
        <p v-else-if="tab === 'completed'" class="empty">
          {{ t('ui.noCompletedProjectsYet') }}
        </p>
        <p v-else-if="!openCampaigns.length" class="empty">
          {{ t('ui.allProjectsAreFullyFunded') }}
        </p>
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
.home { display: flex; flex-direction: column; gap: 2.5rem; }

/* Storytelling hero: message + CTA on one side, live impact on the other. */
.hero {
  display: grid;
  grid-template-columns: 1.4fr 1fr;
  gap: 1.75rem;
  align-items: center;
  padding: 0.5rem 0 1rem;
}
.hero-content { display: flex; flex-direction: column; gap: 1.25rem; align-items: flex-start; }
.hero-eyebrow {
  display: inline-flex; align-items: center; gap: 0.45rem;
  padding: 0.4rem 0.9rem; border-radius: 999px;
  background: var(--warm-soft); color: var(--accent);
  font-size: 0.85rem; font-weight: 600;
}
[data-theme="light"] .hero-eyebrow { color: #b45309; }
.hero-title {
  font-size: clamp(1.7rem, 4.5vw, 2.9rem);
  font-weight: 800; line-height: 1.25; margin: 0;
  letter-spacing: -0.5px;
}
.hero-cta { display: flex; gap: 0.75rem; flex-wrap: wrap; }
.hero-trust { display: flex; gap: 0.6rem; flex-wrap: wrap; }

.hero-impact {
  text-align: center;
  background:
    radial-gradient(120% 100% at 50% 0%, color-mix(in srgb, var(--primary) 16%, transparent), transparent 70%),
    var(--card);
  padding: 2rem 1.5rem;
}
.impact-label { color: var(--muted); font-size: 0.9rem; margin-bottom: 0.5rem; }
.impact-value { font-size: clamp(1.6rem, 5vw, 2.4rem); }
.impact-mini { margin-top: 1.25rem; padding-top: 1.25rem; border-top: 1px solid var(--border); }
.impact-cell { display: flex; flex-direction: column; gap: 0.15rem; }
.impact-cell strong { font-size: 1.5rem; font-weight: 700; font-variant-numeric: tabular-nums; }
.impact-cell span { color: var(--muted); font-size: 0.85rem; }
.unit { font-size: 1rem; color: var(--muted); -webkit-text-fill-color: var(--muted); }

.section-title {
  display: flex; align-items: center; gap: 0.55rem;
  margin-bottom: 1.25rem; font-size: 1.4rem;
}
.section-title .icon { width: 1.2rem; height: 1.2rem; color: var(--accent); }

.list-head {
  display: flex; align-items: center; justify-content: space-between;
  gap: 1rem; flex-wrap: wrap;
}
.list-head .section-title { margin-bottom: 1.25rem; }
.list-tabs { display: flex; gap: 0.35rem; margin-bottom: 1.25rem; flex-wrap: wrap; }
.list-tabs button {
  display: inline-flex; align-items: center; gap: 0.4rem;
  padding: 0.5rem 1.1rem; border-radius: 999px;
  border: 1px solid var(--border); background: transparent;
  color: var(--muted); cursor: pointer; font-family: inherit; font-size: 0.9rem;
  transition: background 0.15s, color 0.15s, border-color 0.15s;
}
.list-tabs button:hover { color: var(--text); }
.list-tabs button.active {
  background: color-mix(in srgb, var(--primary) 16%, transparent);
  border-color: color-mix(in srgb, var(--primary) 40%, transparent);
  color: var(--primary); font-weight: 700;
}
.tab-count { font-size: 0.78rem; opacity: 0.8; font-variant-numeric: tabular-nums; }

@media (max-width: 820px) {
  .hero { grid-template-columns: 1fr; gap: 1.25rem; }
  .hero-content { align-items: center; text-align: center; }
  .hero-cta, .hero-trust { justify-content: center; }
}
.empty { color: var(--muted); text-align: center; padding: 3rem; }

/* Card grid. 'auto' = responsive fill; 'fixed' = a chosen number of columns.
   Both keep cards at a natural width and collapse to fewer columns on small screens. */
.cards-grid { display: grid; gap: 1.5rem; justify-content: center; }
.cards-grid.auto { grid-template-columns: repeat(auto-fill, minmax(280px, 360px)); }
.cards-grid.fixed { grid-template-columns: repeat(var(--cols), minmax(0, 360px)); }
@media (max-width: 900px) { .cards-grid.fixed { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
@media (max-width: 620px) {
  .cards-grid.auto, .cards-grid.fixed { grid-template-columns: 1fr; }
}
</style>
