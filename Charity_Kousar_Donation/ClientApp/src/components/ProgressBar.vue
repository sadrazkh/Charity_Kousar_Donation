<script setup>
import { computed, onMounted, onBeforeUnmount, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useSiteConfig } from '@/composables/useSiteConfig'
import { progressFillStyle } from '@/utils/progress'
import { toPersianDigits } from '@/utils/jalali'

const props = defineProps({
  percent: { type: Number, default: 0 },
  height: { type: Number, default: 10 },
  showPercent: { type: Boolean, default: null }, // null = follow site setting
  animate: { type: Boolean, default: null },     // null = follow site setting
  flow: { type: Boolean, default: null },        // null = follow site setting
  cfg: { type: Object, default: null }           // overrides the site config (admin live preview)
})

const { locale } = useI18n()
const { config } = useSiteConfig()
const conf = computed(() => props.cfg || config)

function clamp(n) { return Math.min(100, Math.max(0, Number(n) || 0)) }

const target = computed(() => clamp(props.percent))
const shown = ref(0)          // value actually painted — sweeps up to `target`
const running = ref(false)
const wrapEl = ref(null)

const reduceMotion = window.matchMedia?.('(prefers-reduced-motion: reduce)').matches === true
const animated = computed(() =>
  !reduceMotion && (props.animate === null ? conf.value.progressAnimate !== false : props.animate))
const duration = computed(() => Math.max(0, Number(conf.value.progressAnimateMs) || 0))

// The fill keeps moving forever (not just while filling up), so the bar reads as alive.
const FLOW_STYLES = ['shimmer', 'stripes', 'glow', 'pulse']
const flowing = computed(() =>
  !reduceMotion && (props.flow === null ? conf.value.progressFlow !== false : props.flow))
const flowStyle = computed(() => {
  const s = String(conf.value.progressFlowStyle || 'shimmer')
  return FLOW_STYLES.includes(s) ? s : 'shimmer'
})
const flowMs = computed(() => Math.max(300, Number(conf.value.progressFlowMs) || 2400))
// The bar fills from the right in Persian, so the motion has to travel that way too.
const rtl = computed(() => locale.value === 'fa')

const pct = computed(() => Math.round(shown.value))
const pctLabel = computed(() => locale.value === 'fa'
  ? toPersianDigits(String(pct.value)) + '٪'
  : pct.value + '%')
const fillStyle = computed(() => ({
  ...progressFillStyle(shown.value, conf.value),
  // While the frame loop drives the width, CSS must not smooth it a second time.
  transition: running.value ? 'none' : 'width 0.6s ease, background 0.6s ease',
  '--flow-ms': flowMs.value + 'ms'
}))
const trackStyle = computed(() => ({
  height: props.height + 'px',
  ...(conf.value.progressTrackColor ? { background: conf.value.progressTrackColor } : null)
}))
const showLabel = computed(() =>
  props.showPercent === null ? conf.value.showProgressPercent !== false : props.showPercent)

let frame = 0
let observer = null
let started = false

function easeOutCubic(t) { return 1 - Math.pow(1 - t, 3) }

function sweepTo(value) {
  cancelAnimationFrame(frame)
  const from = shown.value
  const delta = value - from
  if (!animated.value || duration.value === 0 || delta === 0) {
    running.value = false
    shown.value = value
    return
  }
  const startedAt = performance.now()
  running.value = true
  const step = (now) => {
    const t = Math.min(1, (now - startedAt) / duration.value)
    shown.value = from + delta * easeOutCubic(t)
    if (t < 1) frame = requestAnimationFrame(step)
    else running.value = false
  }
  frame = requestAnimationFrame(step)
}

function start() {
  if (started) return
  started = true
  sweepTo(target.value)
}

onMounted(() => {
  if (!animated.value) { shown.value = target.value; started = true; return }
  // Fill from zero the moment the bar scrolls into view.
  if (!window.IntersectionObserver) { start(); return }
  observer = new IntersectionObserver((entries) => {
    if (entries.some(e => e.isIntersecting)) {
      observer?.disconnect()
      observer = null
      start()
    }
  }, { threshold: 0.25 })
  if (wrapEl.value) observer.observe(wrapEl.value)
  else start()
})

// Live updates (a new donation arrives, or the admin preview changes) keep animating.
watch(target, (v) => { if (started) sweepTo(v) })

onBeforeUnmount(() => {
  cancelAnimationFrame(frame)
  observer?.disconnect()
})
</script>

<template>
  <div ref="wrapEl" class="progress-wrap">
    <div class="progress-bar" :style="trackStyle"
      role="progressbar" :aria-valuenow="Math.round(target)" aria-valuemin="0" aria-valuemax="100">
      <div class="progress-bar-fill" :style="fillStyle"
        :class="flowing ? [`flow`, `flow-${flowStyle}`, { rtl }] : null" />
    </div>
    <span v-if="showLabel" class="progress-pct" :style="{ color: fillStyle.background }">{{ pctLabel }}</span>
  </div>
</template>

<style scoped>
.progress-wrap { display: flex; align-items: center; gap: 0.6rem; }
.progress-bar {
  flex: 1;
  background: color-mix(in srgb, var(--muted) 22%, transparent);
  border-radius: 999px;
  overflow: hidden;
}
.progress-bar-fill {
  height: 100%;
  border-radius: 999px;
  position: relative;
  overflow: hidden;
}

/* Continuous motion: keeps running once the bar is full, so it never looks frozen.
   The overlay sits on top of the fill color, which stays admin-configurable. */
.flow::after {
  content: '';
  position: absolute;
  inset: 0;
  pointer-events: none;
}
/* A highlight slides across the fill, over and over. */
.flow-shimmer::after {
  background-image: linear-gradient(90deg,
    transparent, color-mix(in srgb, #fff 55%, transparent), transparent);
  background-size: 45% 100%;
  background-repeat: no-repeat;
  background-position: -60% 0;
  animation: flow-sweep var(--flow-ms, 2400ms) linear infinite;
}
/* Diagonal bars marching along the fill, like a live transfer. */
.flow-stripes::after {
  background-image: repeating-linear-gradient(115deg,
    color-mix(in srgb, #fff 20%, transparent) 0 10px, transparent 10px 22px);
  background-size: 46px 100%;
  animation: flow-stripes var(--flow-ms, 2400ms) linear infinite;
}
/* The whole fill breathes brighter and dimmer. */
.flow-glow::after {
  background-image: linear-gradient(90deg,
    transparent, color-mix(in srgb, #fff 32%, transparent), transparent);
  animation: flow-fade var(--flow-ms, 2400ms) ease-in-out infinite;
}
/* Only the leading edge pulses — quieter on long pages. */
.flow-pulse::after {
  inset-inline-start: auto;
  width: 16%;
  background-image: linear-gradient(to right,
    transparent, color-mix(in srgb, #fff 55%, transparent));
  animation: flow-fade var(--flow-ms, 2400ms) ease-in-out infinite;
}

@keyframes flow-sweep { to { background-position: 160% 0; } }
@keyframes flow-stripes { to { background-position: 46px 0; } }
@keyframes flow-fade { 0%, 100% { opacity: 0.2; } 50% { opacity: 1; } }

/* Motion follows the text direction: the bar fills from the right in RTL. */
.flow-shimmer.rtl::after,
.flow-stripes.rtl::after { animation-direction: reverse; }
.flow-pulse.rtl::after {
  background-image: linear-gradient(to left,
    transparent, color-mix(in srgb, #fff 55%, transparent));
}

@media (prefers-reduced-motion: reduce) {
  .flow::after { animation: none; }
}
.progress-pct {
  font-size: 0.85rem;
  font-weight: 700;
  font-variant-numeric: tabular-nums;
  min-width: 2.8rem;
  text-align: end;
}
</style>
