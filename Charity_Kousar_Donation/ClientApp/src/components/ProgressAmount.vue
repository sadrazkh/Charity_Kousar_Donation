<script setup>
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { formatAmount } from '@/utils/amount'
import { useSiteConfig } from '@/composables/useSiteConfig'

const props = defineProps({
  collected: { type: Number, default: 0 },
  target: { type: Number, default: 0 }
})

const { locale } = useI18n()
const { config } = useSiteConfig()

const fmtCollected = computed(() => formatAmount(props.collected, locale.value))
const fmtTarget = computed(() => formatAmount(props.target, locale.value))

// Template uses {collected} and {target}; collected is rendered bold.
const parts = computed(() => {
  const tpl = (locale.value === 'fa' ? config.progressFormatFa : config.progressFormatEn)
    || '{collected} / {target}'
  const withTarget = tpl.replace('{target}', fmtTarget.value)
  const [before, after = ''] = withTarget.split('{collected}')
  return { before, after }
})
</script>

<template>
  <span class="progress-amount">{{ parts.before }}<strong>{{ fmtCollected }}</strong>{{ parts.after }}</span>
</template>

<style scoped>
.progress-amount { font-variant-numeric: tabular-nums; }
.progress-amount strong { font-weight: 800; color: var(--text); }
</style>
