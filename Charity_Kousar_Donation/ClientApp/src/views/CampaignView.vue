<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute } from 'vue-router'
import { useI18n } from 'vue-i18n'
import AppHeader from '@/components/AppHeader.vue'
import DonationModal from '@/components/DonationModal.vue'
import PageBlockRenderer from '@/components/PageBlockRenderer.vue'
import ShareModal from '@/components/ShareModal.vue'
import FeaturedBanner from '@/components/FeaturedBanner.vue'
import RecentDonors from '@/components/RecentDonors.vue'
import ProgressBar from '@/components/ProgressBar.vue'
import ProgressAmount from '@/components/ProgressAmount.vue'
import QrCode from '@/components/QrCode.vue'
import { whatsAppShareUrl, telegramShareUrl } from '@/utils/amount'
import { api } from '@/api/client'
import { useSiteConfig } from '@/composables/useSiteConfig'

const route = useRoute()
const { t, locale } = useI18n()
const { config } = useSiteConfig()
const campaign = ref(null)
const showDonate = ref(false)
const showShare = ref(false)
const copied = ref(false)

onMounted(async () => {
  campaign.value = await api(`/campaigns/${route.params.slug}`)
})

const title = computed(() =>
  locale.value === 'fa' ? campaign.value?.titleFa : campaign.value?.titleEn)

const shareText = computed(() => {
  const link = campaign.value?.shortUrl || ''
  return locale.value === 'fa'
    ? `🤲 کمک به «${title.value}»\n${link}`
    : `🤲 Support "${title.value}"\n${link}`
})

const waUrl = computed(() => whatsAppShareUrl(shareText.value))
const tgUrl = computed(() => telegramShareUrl(campaign.value?.shortUrl || '', shareText.value))

const blocks = computed(() => campaign.value?.pageBlocks || [])

async function copyLink() {
  await navigator.clipboard.writeText(campaign.value.shortUrl)
  copied.value = true
  setTimeout(() => { copied.value = false }, 2000)
}
</script>

<template>
  <AppHeader />
  <div v-if="campaign" class="container">
    <router-link to="/" class="back-home">
      <span class="bh-arrow">{{ locale === 'fa' ? '→' : '←' }}</span>
      {{ locale === 'fa' ? 'بازگشت به صفحه اصلی' : 'Back to home' }}
    </router-link>
  </div>
  <main v-if="campaign" class="container campaign-page">
    <article class="card detail">
      <FeaturedBanner v-if="campaign.isFeatured" :campaign="campaign" />

      <PageBlockRenderer
        :blocks="blocks"
        :campaign="campaign"
        @donate="showDonate = true"
      />

      <RecentDonors :campaign-id="campaign.id" class="campaign-donors" />

      <section class="share-section">
        <div class="share-header">
          <div class="share-icon">📤</div>
          <div>
            <h3>{{ locale === 'fa' ? 'اشتراک‌گذاری و دعوت دیگران' : 'Share & invite others' }}</h3>
            <p>{{ locale === 'fa' ? 'لینک را بفرستید یا با واتساپ/تلگرام به اشتراک بگذارید' : 'Send the link or share via WhatsApp/Telegram' }}</p>
          </div>
        </div>

        <div class="share-link-box">
          <input :value="campaign.shortUrl" class="input input-ltr share-input" dir="ltr" readonly />
          <button type="button" class="btn btn-primary btn-sm copy-btn" @click="copyLink">
            {{ copied ? '✓ ' + t('copied') : t('copyLink') }}
          </button>
        </div>

        <div class="share-channels">
          <a :href="waUrl" class="channel-btn whatsapp" target="_blank" rel="noopener">
            <span class="ch-icon">💬</span>
            <span>WhatsApp</span>
          </a>
          <a :href="tgUrl" class="channel-btn telegram" target="_blank" rel="noopener">
            <span class="ch-icon">✈️</span>
            <span>Telegram</span>
          </a>
          <button type="button" class="channel-btn ai" @click="showShare = true">
            <span class="ch-icon">📝</span>
            <span>{{ locale === 'fa' ? 'متن آماده' : 'Ready text' }}</span>
          </button>
        </div>

        <div class="qr-card">
          <QrCode :url="campaign.shortUrl" />
          <div class="qr-info">
            <strong>{{ locale === 'fa' ? 'اسکن QR' : 'Scan QR' }}</strong>
            <p>{{ locale === 'fa' ? 'برای پرداخت سریع، دوربین گوشی را روی کد بگیرید' : 'Point your camera at the code for quick donation' }}</p>
          </div>
        </div>
      </section>
    </article>

    <aside class="sticky-donate card">
      <p class="sticky-title">{{ title }}</p>
      <ProgressBar :percent="campaign.progressPercent" :height="12" />
      <p class="sticky-amount"><ProgressAmount :collected="campaign.collectedAmount" :target="campaign.targetAmount" /></p>
      <button class="btn btn-primary" style="width:100%" @click="showDonate = true">{{ t('pay') }}</button>
      <button type="button" class="btn btn-accent btn-sm share-side-btn" @click="showShare = true">
        {{ locale === 'fa' ? '📤 اشتراک‌گذاری' : '📤 Share' }}
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
.back-home {
  display: inline-flex; align-items: center; gap: 0.5rem;
  margin-bottom: 1rem; padding: 0.6rem 1.1rem;
  border-radius: 999px; font-size: 1rem; font-weight: 600;
  color: var(--primary); text-decoration: none;
  background: color-mix(in srgb, var(--primary) 12%, transparent);
  border: 1px solid color-mix(in srgb, var(--primary) 30%, transparent);
  transition: background 0.15s;
}
.back-home:hover { background: color-mix(in srgb, var(--primary) 20%, transparent); text-decoration: none; }
.bh-arrow { font-size: 1.15rem; }
.detail { padding: clamp(1.15rem, 3vw, 2rem); }
.campaign-donors { margin-top: 2rem; }

.share-section {
  margin-top: 2rem;
  padding: 1.25rem;
  border-radius: 14px;
  background: linear-gradient(145deg,
    color-mix(in srgb, var(--primary) 12%, transparent),
    color-mix(in srgb, var(--accent) 8%, transparent));
  border: 1px solid color-mix(in srgb, var(--primary) 25%, transparent);
}
.share-header { display: flex; gap: 0.85rem; align-items: flex-start; margin-bottom: 1.1rem; }
.share-icon {
  width: 44px; height: 44px; border-radius: 12px; flex-shrink: 0;
  background: color-mix(in srgb, var(--primary) 20%, transparent);
  display: flex; align-items: center; justify-content: center; font-size: 1.35rem;
}
.share-header h3 { font-size: 1rem; margin-bottom: 0.2rem; }
.share-header p { font-size: 0.82rem; color: var(--muted); margin: 0; line-height: 1.5; }
.share-link-box { display: flex; gap: 0.5rem; flex-wrap: wrap; margin-bottom: 1rem; }
.share-input { flex: 1; min-width: 180px; background: rgba(0,0,0,0.25); }
.copy-btn { min-width: 100px; }
.share-channels { display: grid; grid-template-columns: repeat(3, 1fr); gap: 0.5rem; margin-bottom: 1rem; }
@media (max-width: 480px) { .share-channels { grid-template-columns: 1fr; } }
.channel-btn {
  display: flex; flex-direction: column; align-items: center; gap: 0.25rem;
  padding: 0.75rem 0.5rem; border-radius: 10px; border: 1px solid rgba(148,163,184,0.2);
  background: rgba(0,0,0,0.2); color: inherit; text-decoration: none;
  font-family: inherit; font-size: 0.82rem; cursor: pointer; transition: transform 0.15s, border-color 0.15s;
  min-height: 72px; justify-content: center;
}
.channel-btn:hover { transform: translateY(-2px); border-color: color-mix(in srgb, var(--primary) 40%, transparent); }
.channel-btn.whatsapp { border-color: rgba(37,211,102,0.3); }
.channel-btn.telegram { border-color: rgba(42,171,238,0.3); }
.channel-btn.ai { border-color: color-mix(in srgb, var(--accent) 40%, transparent); }
.ch-icon { font-size: 1.4rem; }
.qr-card {
  display: flex; align-items: center; gap: 1rem; flex-wrap: wrap;
  padding: 0.85rem; border-radius: 10px; background: rgba(0,0,0,0.2);
}
.qr-info strong { display: block; font-size: 0.9rem; margin-bottom: 0.25rem; }
.qr-info p { font-size: 0.78rem; color: var(--muted); margin: 0; line-height: 1.5; max-width: 280px; }

.sticky-donate { position: sticky; top: 1.5rem; padding: 1.25rem; }
.sticky-title { font-weight: 600; margin-bottom: 0.75rem; font-size: 0.95rem; }
.sticky-amount { color: var(--muted); font-size: 0.85rem; margin: 0.75rem 0 1rem; text-align: center; font-variant-numeric: tabular-nums; }
.share-side-btn { width: 100%; margin-top: 0.5rem; }
</style>
