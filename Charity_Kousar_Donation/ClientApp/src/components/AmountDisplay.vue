<script setup>
import { formatAmount } from '@/utils/amount'
import { useI18n } from 'vue-i18n'

defineProps({
  value: { type: [Number, String], default: 0 },
  suffix: { type: String, default: '' }
})
const { locale } = useI18n()
</script>

<template>
  <span class="amount-display">{{ formatAmount(value, locale) }}<template v-if="suffix"> {{ suffix }}</template></span>
</template>

<style scoped>
.amount-display { font-variant-numeric: tabular-nums; }
</style>
