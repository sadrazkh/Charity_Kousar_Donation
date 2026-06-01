<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute } from 'vue-router'
import { useI18n } from 'vue-i18n'
import AppHeader from '@/components/AppHeader.vue'
import DonationModal from '@/components/DonationModal.vue'
import PageBlockRenderer from '@/components/PageBlockRenderer.vue'
import ShareModal from '@/components/ShareModal.vue'
import { formatAmount } from '@/utils/amount'
import { api } from '@/api/client'

const route = useRoute()
const { t, locale } = useI18n()
const campaign = ref(null)
const config = ref({})
const showDonate = ref(false)
const showShare = ref(false)
const copied = ref(false)

onMounted(async () => {
  const [c, cfg] = await Promise.all([
    api(`/campaigns/${route.params.slug}`),
    api('/settings/public')
  ])
  campaign.value = c
  config.value = cfg
})

const title = computed(() =>
  locale.value === 'fa' ? campaign.value?.titleFa : campaign.value?.titleEn)

const blocks = computed(() => campaign.value?.pageBlocks || [])
const fmt = (n) => formatAmount(n, locale.value)
async function copyLink() {
  await navigator.clipboard.writeText(campaign.value.shortUrl)
  copied.value = true
  setTimeout(() => { copied.value = false }, 2000)
}
</script>

<template>
  <AppHeader />
  <main v-if="campaign" class="container campaign-page">
    <article class="card detail">
      <PageBlockRenderer
        :blocks="blocks"
        :campaign="campaign"
        @donate="showDonate = true"
      />

      <div class="share-row">
        <label class="label">{{ t('shortLink') }}</label>
        <div class="link-row">
          <input :value="campaign.shortUrl" class="input input-ltr" dir="ltr" readonly />
          <button class="btn btn-ghost btn-sm" @click="copyLink">{{ copied ? t('copied') : t('copyLink') }}</button>
          <button type="button" class="btn btn-accent btn-sm" @click="showShare = true">
            {{ locale === 'fa' ? '📤 اشتراک AI' : '📤 AI Share' }}
          </button>
        </div>
      </div>
    </article>

    <aside class="sticky-donate card">
      <p class="sticky-title">{{ title }}</p>
      <div class="progress-bar"><div class="progress-bar-fill" :style="{ width: campaign.progressPercent + '%' }" /></div>
      <p class="sticky-amount">{{ fmt(campaign.collectedAmount) }} / {{ fmt(campaign.targetAmount) }} {{ t('toman') }}</p>
      <button class="btn btn-primary" style="width:100%" @click="showDonate = true">{{ t('donateNow') }}</button>
      <button type="button" class="btn btn-ghost btn-sm share-side-btn" @click="showShare = true">
        {{ locale === 'fa' ? '📤 اشتراک در واتساپ/تلگرام' : '📤 Share' }}
      </button>
    </aside>
  </main>

  <DonationModal v-if="showDonate && campaign" :campaign="campaign" :config="config" @close="showDonate = false" />
  <ShareModal v-if="campaign" :slug="campaign.slug" :show="showShare" @close="showShare = false" />
</template>

<style scoped>
.campaign-page {
  display: grid;
  grid-template-columns: 1fr minmax(240px, 280px);
  gap: 1.25rem;
  align-items: start;
  max-width: 960px;
}
@media (max-width: 768px) {
  .campaign-page { grid-template-columns: 1fr; gap: 1rem; }
  .sticky-donate { order: -1; position: static !important; }
  .detail { padding: 1.25rem; }
}
.detail { padding: clamp(1.15rem, 3vw, 2rem); }
.share-row { margin-top: 2rem; padding-top: 1.5rem; border-top: 1px solid rgba(148,163,184,0.15); }
.link-row { display: flex; gap: 0.5rem; flex-wrap: wrap; margin-top: 0.35rem; }
.link-row .input { flex: 1; min-width: 180px; }
.sticky-donate { position: sticky; top: 1.5rem; padding: 1.25rem; }
.sticky-title { font-weight: 600; margin-bottom: 0.75rem; font-size: 0.95rem; }
.sticky-amount { color: var(--muted); font-size: 0.85rem; margin: 0.75rem 0 1rem; text-align: center; font-variant-numeric: tabular-nums; }
.share-side-btn { width: 100%; margin-top: 0.5rem; }
</style>
