<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { toPersianDigits } from '@/utils/jalali'

const props = defineProps({
  campaign: { type: Object, required: true },
  compact: { type: Boolean, default: false }
})

const { locale } = useI18n()
const now = ref(Date.now())
let timer = null

onMounted(() => {
  if (endsAt.value) {
    timer = setInterval(() => { now.value = Date.now() }, 1000)
  }
})
onUnmounted(() => { if (timer) clearInterval(timer) })

const bannerText = computed(() => {
  const fa = props.campaign?.featuredBannerFa
  const en = props.campaign?.featuredBannerEn
  return locale.value === 'fa' ? (fa || en) : (en || fa)
})

const endsAt = computed(() => {
  const raw = props.campaign?.featuredTimerEndsAt
  if (!raw) return null
  const t = new Date(raw).getTime()
  return Number.isNaN(t) ? null : t
})

const remaining = computed(() => {
  if (!endsAt.value) return null
  const diff = endsAt.value - now.value
  if (diff <= 0) return { expired: true, days: 0, hours: 0, minutes: 0, seconds: 0 }
  const days = Math.floor(diff / 86400000)
  const hours = Math.floor((diff % 86400000) / 3600000)
  const minutes = Math.floor((diff % 3600000) / 60000)
  const seconds = Math.floor((diff % 60000) / 1000)
  return { expired: false, days, hours, minutes, seconds }
})

const show = computed(() =>
  props.campaign?.isFeatured && (bannerText.value || endsAt.value))

function fmtNum(n, pad = false) {
  const s = pad ? String(n).padStart(2, '0') : String(n)
  return locale.value === 'fa' ? toPersianDigits(s) : s
}
</script>

<template>
  <div v-if="show" class="featured-banner" :class="{ compact }">
    <span class="badge">{{ locale === 'fa' ? '⭐ ویژه' : '⭐ Featured' }}</span>
    <p v-if="bannerText" class="banner-text">{{ bannerText }}</p>
    <div v-if="remaining && endsAt" class="countdown" :class="{ expired: remaining.expired }">
      <template v-if="remaining.expired">
        {{ locale === 'fa' ? '⏱ زمان به پایان رسید' : '⏱ Time ended' }}
      </template>
      <template v-else>
        <div class="unit"><strong>{{ fmtNum(remaining.days) }}</strong><span>{{ locale === 'fa' ? 'روز' : 'd' }}</span></div>
        <div class="sep">:</div>
        <div class="unit"><strong>{{ fmtNum(remaining.hours, true) }}</strong><span>{{ locale === 'fa' ? 'ساعت' : 'h' }}</span></div>
        <div class="sep">:</div>
        <div class="unit"><strong>{{ fmtNum(remaining.minutes, true) }}</strong><span>{{ locale === 'fa' ? 'دقیقه' : 'm' }}</span></div>
        <div class="sep">:</div>
        <div class="unit"><strong>{{ fmtNum(remaining.seconds, true) }}</strong><span>{{ locale === 'fa' ? 'ثانیه' : 's' }}</span></div>
      </template>
    </div>
  </div>
</template>

<style scoped>
.featured-banner {
  background: linear-gradient(135deg,
    color-mix(in srgb, var(--accent) 22%, transparent),
    color-mix(in srgb, var(--primary) 18%, transparent));
  border: 1px solid color-mix(in srgb, var(--accent) 45%, transparent);
  border-radius: 12px;
  padding: 0.85rem 1rem;
  margin-bottom: 0.75rem;
}
.featured-banner.compact { padding: 0.65rem 0.75rem; margin-bottom: 0; }
.badge {
  display: inline-block;
  font-size: 0.72rem;
  font-weight: 700;
  color: var(--accent);
  background: color-mix(in srgb, var(--accent) 15%, transparent);
  padding: 0.15rem 0.55rem;
  border-radius: 999px;
  margin-bottom: 0.4rem;
}
.banner-text { font-size: 0.9rem; line-height: 1.6; margin: 0 0 0.5rem; font-weight: 500; }
.countdown {
  display: flex;
  align-items: center;
  gap: 0.35rem;
  flex-wrap: wrap;
  font-variant-numeric: tabular-nums;
  direction: ltr;
  justify-content: flex-start;
}
.countdown.expired { color: #f87171; font-size: 0.85rem; }
.unit {
  display: flex;
  flex-direction: column;
  align-items: center;
  min-width: 2.5rem;
  background: rgba(0,0,0,0.25);
  border-radius: 8px;
  padding: 0.25rem 0.4rem;
}
.unit strong { font-size: 1.05rem; color: var(--accent); }
.unit span { font-size: 0.65rem; color: var(--muted); }
.sep { color: var(--muted); font-weight: 700; padding-bottom: 0.5rem; }
</style>
