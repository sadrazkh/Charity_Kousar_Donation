<script setup>
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import AppHeader from '@/components/AppHeader.vue'
import CampaignCard from '@/components/CampaignCard.vue'
import DonationModal from '@/components/DonationModal.vue'
import { api } from '@/api/client'

import { formatAmount } from '@/utils/amount'

const { t, locale } = useI18n()
const campaigns = ref([])
const total = ref(0)
const config = ref({})
const selected = ref(null)

const heroText = computed(() =>
  locale.value === 'fa' ? config.value.heroTextFa : config.value.heroTextEn)

onMounted(async () => {
  try {
    const [list, stats, cfg] = await Promise.all([
      api('/campaigns'),
      api('/donations/stats/total'),
      api('/settings/public')
    ])
    campaigns.value = list
    total.value = stats.totalCollected
    config.value = cfg
  } catch { /* */ }
})

const fmt = (n) => formatAmount(n, locale.value)
</script>

<template>
  <AppHeader />
  <main class="container">
    <section class="hero card">
      <p class="hero-text">{{ heroText }}</p>
      <p class="hero-label">{{ t('totalCollected') }}</p>
      <p class="stat-value">{{ fmt(total) }} <span class="unit">{{ t('toman') }}</span></p>
    </section>

    <h2 class="section-title">{{ t('campaigns') }}</h2>
    <div v-if="campaigns.length" class="grid grid-2">
      <CampaignCard
        v-for="c in campaigns"
        :key="c.id"
        :campaign="c"
        @donate="selected = c"
      />
    </div>
    <p v-else class="empty">{{ t('noCampaigns') }}</p>
  </main>

  <DonationModal
    v-if="selected"
    :campaign="selected"
    :config="config"
    @close="selected = null"
  />
</template>

<style scoped>
.hero { text-align: center; padding: 2.5rem; margin-bottom: 2.5rem; position: relative; overflow: hidden; }
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
</style>
