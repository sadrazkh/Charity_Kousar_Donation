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
  highlight: { type: String, default: null },
  colors: { type: Object, default: null },   // { collected, target, remaining, percent, text }
  scale: { type: Number, default: null }     // font size in percent
})

const { locale } = useI18n()
const { config } = useSiteConfig()

const TOKENS = ['collected', 'target', 'remaining', 'percent']

const values = computed(() => {
  const remaining = Math.max(0, (props.target || 0) - (props.collected || 0))
  const percent = props.target > 0 ? Math.min(100, Math.round((props.collected / props.target) * 100)) : 0
  return {
    collected: formatAmount(props.collected, locale.value),
    target: formatAmount(props.target, locale.value),
    remaining: formatAmount(remaining, locale.value),
    percent: percent + '%'
  }
})

// Parse the admin-defined template into styled segments.
// Markup:  *bold*   ~highlighted~ (colored)
// Tokens:  {collected} {target} {remaining} {percent} — each has its own color setting.
const segments = computed(() => {
  let tpl = props.format ?? ((locale.value === 'fa' ? config.progressFormatFa : config.progressFormatEn) || '*{collected}*')
  // Legacy templates without markup: bold the collected amount by default.
  if (!/[*~]/.test(tpl)) tpl = tpl.replace('{collected}', '*{collected}*')

  const out = []
  const markup = /(\*[^*]+\*|~[^~]+~)/g
  let last = 0, m
  const push = (text, bold, colored) => {
    // Split the chunk further so every token can carry its own color.
    for (const part of text.split(/(\{collected\}|\{target\}|\{remaining\}|\{percent\})/g)) {
      if (!part) continue
      const token = TOKENS.find(k => part === `{${k}}`)
      out.push({ t: token ? values.value[token] : part, token, bold, colored })
    }
  }
  while ((m = markup.exec(tpl))) {
    if (m.index > last) push(tpl.slice(last, m.index), false, false)
    const tok = m[0]
    push(tok.slice(1, -1), tok.startsWith('*'), tok.startsWith('~'))
    last = markup.lastIndex
  }
  if (last < tpl.length) push(tpl.slice(last), false, false)
  return out
})

const highlight = computed(() => props.highlight || config.progressHighlight || 'var(--primary)')

const colors = computed(() => props.colors || {
  collected: config.amountColorCollected,
  target: config.amountColorTarget,
  remaining: config.amountColorRemaining,
  percent: config.amountColorPercent,
  text: config.amountTextColor
})

const rootStyle = computed(() => {
  const scale = Number(props.scale ?? config.amountFontScale) || 100
  return scale === 100 ? null : { fontSize: scale + '%' }
})

function segStyle(s) {
  const color = (s.token && colors.value[s.token]) || (s.colored ? highlight.value : colors.value.text) || null
  const weight = s.bold ? 800 : s.colored ? 700 : null
  return (color || weight) ? { color, fontWeight: weight } : null
}
</script>

<template>
  <span class="progress-amount" :style="rootStyle"><span
    v-for="(s, i) in segments"
    :key="i"
    :class="{ bold: s.bold }"
    :style="segStyle(s)"
  >{{ s.t }}</span></span>
</template>

<style scoped>
.progress-amount { font-variant-numeric: tabular-nums; }
.progress-amount .bold { font-weight: 800; color: var(--text); }
</style>
