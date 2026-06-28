<script setup>
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { api } from '@/api/client'
import { formatAmount } from '@/utils/amount'
import { useSiteConfig } from '@/composables/useSiteConfig'

const props = defineProps({
  campaignId: { type: String, default: null }
})

const { locale, t } = useI18n()
const { config, loadSiteConfig } = useSiteConfig()
const donors = ref([])

onMounted(async () => {
  await loadSiteConfig()
  if (config.showRecentDonors === false) return
  try {
    const q = props.campaignId ? `?campaignId=${props.campaignId}` : ''
    donors.value = await api(`/donations/recent${q}`)
  } catch { /* */ }
})

const title = computed(() => locale.value === 'fa' ? config.donorsTitleFa : config.donorsTitleEn)
const anon = computed(() => locale.value === 'fa' ? config.donorAnonymousFa : config.donorAnonymousEn)

function donorName(d) {
  if (config.showDonorName && d.donorName) return d.donorName
  if (config.showDonorName && d.maskedPhone) return d.maskedPhone
  return anon.value
}
function fmtDate(d) {
  try {
    return new Date(d.paidAt).toLocaleDateString(locale.value === 'fa' ? 'fa-IR' : 'en-US',
      { month: 'short', day: 'numeric' })
  } catch { return '' }
}
const fmt = (n) => formatAmount(n, locale.value)
const show = computed(() => config.showRecentDonors !== false && donors.value.length > 0)
</script>

<template>
  <section v-if="show" class="recent-donors card">
    <h3>💚 {{ title }}</h3>
    <ul>
      <li v-for="(d, i) in donors" :key="i">
        <span class="d-name">{{ donorName(d) }}</span>
        <span class="d-meta">
          <span v-if="config.showDonorCampaign && d.campaignTitle" class="d-campaign">{{ d.campaignTitle }}</span>
          <span v-if="config.showDonorDate" class="d-date">{{ fmtDate(d) }}</span>
          <span v-if="config.showDonorAmount" class="d-amount">{{ fmt(d.amount) }} {{ t('toman') }}</span>
        </span>
      </li>
    </ul>
  </section>
</template>

<style scoped>
.recent-donors { padding: 1.25rem 1.5rem; }
.recent-donors h3 { font-size: 1rem; margin-bottom: 0.85rem; }
.recent-donors ul { list-style: none; padding: 0; margin: 0; }
.recent-donors li {
  display: flex; justify-content: space-between; align-items: center; gap: 0.75rem;
  padding: 0.6rem 0; border-bottom: 1px solid color-mix(in srgb, var(--muted) 12%, transparent);
  font-size: 0.92rem;
}
.recent-donors li:last-child { border-bottom: none; }
.d-name { font-weight: 600; }
.d-meta { display: flex; align-items: center; gap: 0.85rem; color: var(--muted); font-variant-numeric: tabular-nums; flex-wrap: wrap; justify-content: flex-end; }
.d-campaign { font-size: 0.8rem; }
.d-date { font-size: 0.82rem; }
.d-amount { color: var(--primary); font-weight: 600; }
</style>
