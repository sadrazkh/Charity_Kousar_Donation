<script setup>
import { computed } from 'vue'
import { useSiteConfig } from '@/composables/useSiteConfig'
import { progressFillStyle } from '@/utils/progress'

const props = defineProps({
  percent: { type: Number, default: 0 },
  height: { type: Number, default: 10 },
  showPercent: { type: Boolean, default: null } // null = follow site setting
})

const { config } = useSiteConfig()

const pct = computed(() => Math.min(100, Math.max(0, Math.round(props.percent || 0))))
const fillStyle = computed(() => progressFillStyle(pct.value, config))
const showLabel = computed(() =>
  props.showPercent === null ? config.showProgressPercent !== false : props.showPercent)
</script>

<template>
  <div class="progress-wrap">
    <div class="progress-bar" :style="{ height: height + 'px' }">
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
  transition: width 0.6s ease, background 0.6s ease;
}
.progress-pct {
  font-size: 0.85rem;
  font-weight: 700;
  font-variant-numeric: tabular-nums;
  min-width: 2.8rem;
  text-align: end;
}
</style>
