<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { toPersianDigits } from '@/utils/jalali'
import { useSiteConfig } from '@/composables/useSiteConfig'

const props = defineProps({
  campaign: { type: Object, required: true },
  compact: { type: Boolean, default: false }
})

const { locale } = useI18n()
const { config } = useSiteConfig()
const now = ref(Date.now())
let timer = null

onMounted(() => {
  if (endsAt.value) {
    timer = setInterval(() => { now.value = Date.now() }, 1000)
  }
})
onUnmounted(() => { if (timer) clearInterval(timer) })

const UNIT_ORDER = [
  ['days', 86400000, 'روز', 'd'],
  ['hours', 3600000, 'ساعت', 'h'],
  ['minutes', 60000, 'دقیقه', 'm'],
  ['seconds', 1000, 'ثانیه', 's']
]

const selectedUnits = computed(() => {
  const raw = (config.featuredUnits || 'days,hours,minutes,seconds')
    .split(',').map(s => s.trim()).filter(Boolean)
  return UNIT_ORDER.filter(([u]) => raw.includes(u))
})

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

const badgeText = computed(() => locale.value === 'fa' ? config.featuredBadgeFa : config.featuredBadgeEn)
const expiredText = computed(() => locale.value === 'fa' ? config.featuredExpiredFa : config.featuredExpiredEn)
const accent = computed(() => config.featuredColor || 'var(--accent)')
const isInline = computed(() => config.featuredLayout === 'inline')

const remaining = computed(() => {
  if (!endsAt.value) return null
  const diff = endsAt.value - now.value
  if (diff <= 0) return { expired: true, parts: [] }
  let ms = diff
  const parts = []
  for (const [unit, div, labelFa, labelEn] of selectedUnits.value) {
    const value = Math.floor(ms / div)
    ms -= value * div
    parts.push({ unit, value, label: locale.value === 'fa' ? labelFa : labelEn })
  }
  return { expired: false, parts }
})

const show = computed(() =>
  props.campaign?.isFeatured && (bannerText.value || endsAt.value || config.featuredBadgeShow))

function fmtNum(n) {
  const s = String(n).padStart(2, '0')
  return locale.value === 'fa' ? toPersianDigits(s) : s
}
</script>

<template>
  <div v-if="show" class="featured-banner" :class="{ compact }"
    :style="{ '--fb-accent': accent }">
    <span v-if="config.featuredBadgeShow && badgeText" class="badge">{{ badgeText }}</span>
    <p v-if="bannerText" class="banner-text">{{ bannerText }}</p>
    <div v-if="remaining && endsAt" class="countdown" :class="{ expired: remaining.expired, inline: isInline }">
      <template v-if="remaining.expired">{{ expiredText }}</template>
      <template v-else>
        <template v-for="(p, i) in remaining.parts" :key="p.unit">
          <span v-if="i > 0 && isInline" class="sep">:</span>
          <div class="unit"><strong>{{ fmtNum(p.value) }}</strong><span>{{ p.label }}</span></div>
        </template>
      </template>
    </div>
  </div>
</template>

<style scoped>
.featured-banner {
  background: linear-gradient(135deg,
    color-mix(in srgb, var(--fb-accent) 22%, transparent),
    color-mix(in srgb, var(--primary) 16%, transparent));
  border: 1px solid color-mix(in srgb, var(--fb-accent) 45%, transparent);
  border-radius: 12px;
  padding: 0.85rem 1rem;
  margin-bottom: 0.75rem;
}
.featured-banner.compact { padding: 0.65rem 0.75rem; margin-bottom: 0; }
.badge {
  display: inline-block;
  font-size: 0.72rem;
  font-weight: 700;
  color: var(--fb-accent);
  background: color-mix(in srgb, var(--fb-accent) 15%, transparent);
  padding: 0.15rem 0.55rem;
  border-radius: 999px;
  margin-bottom: 0.4rem;
}
.banner-text { font-size: 0.92rem; line-height: 1.6; margin: 0 0 0.5rem; font-weight: 500; }
.countdown {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  flex-wrap: wrap;
  font-variant-numeric: tabular-nums;
  direction: ltr;
  justify-content: flex-start;
}
.countdown.expired { color: #f87171; font-size: 0.9rem; font-weight: 600; }
.unit {
  display: flex;
  flex-direction: column;
  align-items: center;
  min-width: 2.6rem;
  /* var(--card) adapts to light/dark, so boxes stay readable in both themes */
  background: color-mix(in srgb, var(--fb-accent) 14%, var(--card));
  border: 1px solid color-mix(in srgb, var(--fb-accent) 25%, transparent);
  border-radius: 8px;
  padding: 0.3rem 0.45rem;
}
.countdown.inline .unit { background: transparent; padding: 0 0.15rem; min-width: auto; flex-direction: row; gap: 0.2rem; align-items: baseline; }
.unit strong { font-size: 1.1rem; color: var(--fb-accent); }
.unit span { font-size: 0.65rem; color: var(--muted); }
.sep { color: var(--muted); font-weight: 700; }
</style>
