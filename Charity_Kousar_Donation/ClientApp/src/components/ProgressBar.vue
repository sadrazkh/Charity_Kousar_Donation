<script setup>
import { computed, onMounted, onBeforeUnmount, ref, watch } from 'vue'
import { useSiteConfig } from '@/composables/useSiteConfig'
import { progressFillStyle } from '@/utils/progress'

const props = defineProps({
  percent: { type: Number, default: 0 },
  height: { type: Number, default: 10 },
  showPercent: { type: Boolean, default: null }, // null = follow site setting
  animate: { type: Boolean, default: null },     // null = follow site setting
  cfg: { type: Object, default: null }           // overrides the site config (admin live preview)
})

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

const pct = computed(() => Math.round(shown.value))
const fillStyle = computed(() => ({
  ...progressFillStyle(shown.value, conf.value),
  // While the frame loop drives the width, CSS must not smooth it a second time.
  transition: running.value ? 'none' : 'width 0.6s ease, background 0.6s ease'
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
      <div class="progress-bar-fill" :style="fillStyle" />
    </div>
    <span v-if="showLabel" class="progress-pct" :style="{ color: fillStyle.background }">{{ pct }}%</span>
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
}
.progress-pct {
  font-size: 0.85rem;
  font-weight: 700;
  font-variant-numeric: tabular-nums;
  min-width: 2.8rem;
  text-align: end;
}
</style>
