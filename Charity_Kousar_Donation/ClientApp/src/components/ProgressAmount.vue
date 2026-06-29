<script setup>
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { formatAmount } from '@/utils/amount'
import { useSiteConfig } from '@/composables/useSiteConfig'

const props = defineProps({
  collected: { type: Number, default: 0 },
  target: { type: Number, default: 0 },
  // Optional overrides (used for live preview in the admin editor)
  format: { type: String, default: null },
  highlight: { type: String, default: null }
})

const { locale } = useI18n()
const { config } = useSiteConfig()

const tokens = computed(() => {
  const remaining = Math.max(0, (props.target || 0) - (props.collected || 0))
  const percent = props.target > 0 ? Math.min(100, Math.round((props.collected / props.target) * 100)) : 0
  return {
    '{collected}': formatAmount(props.collected, locale.value),
    '{target}': formatAmount(props.target, locale.value),
    '{remaining}': formatAmount(remaining, locale.value),
    '{percent}': percent + '%'
  }
})

// Parse the admin-defined template into styled segments.
// Markup:  *bold*   ~highlighted~ (colored)
// Tokens:  {collected} {target} {remaining} {percent}
const segments = computed(() => {
  let tpl = props.format ?? ((locale.value === 'fa' ? config.progressFormatFa : config.progressFormatEn) || '*{collected}*')
  // Legacy templates without markup: bold the collected amount by default.
  if (!/[*~]/.test(tpl)) tpl = tpl.replace('{collected}', '*{collected}*')
  for (const [k, v] of Object.entries(tokens.value)) tpl = tpl.split(k).join(v)

  const out = []
  const re = /(\*[^*]+\*|~[^~]+~)/g
  let last = 0, m
  while ((m = re.exec(tpl))) {
    if (m.index > last) out.push({ t: tpl.slice(last, m.index) })
    const tok = m[0]
    if (tok.startsWith('*')) out.push({ t: tok.slice(1, -1), bold: true })
    else out.push({ t: tok.slice(1, -1), color: true })
    last = re.lastIndex
  }
  if (last < tpl.length) out.push({ t: tpl.slice(last) })
  return out
})

const highlight = computed(() => props.highlight || config.progressHighlight || 'var(--primary)')
</script>

<template>
  <span class="progress-amount"><span
    v-for="(s, i) in segments"
    :key="i"
    :class="{ bold: s.bold }"
    :style="s.color ? { color: highlight, fontWeight: 700 } : null"
  >{{ s.t }}</span></span>
</template>

<style scoped>
.progress-amount { font-variant-numeric: tabular-nums; }
.progress-amount .bold { font-weight: 800; color: var(--text); }
</style>
