<script setup>
import { ref, watch, computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { formatAmountTyping, parseAmount } from '@/utils/amount'

const props = defineProps({
  modelValue: { type: [Number, String], default: 0 },
  locale: { type: String, default: '' },
  placeholder: { type: String, default: '' },
  min: { type: Number, default: 0 },
  inputClass: { type: String, default: 'input' },
  dir: { type: String, default: 'ltr' }
})
const emit = defineEmits(['update:modelValue'])

const { locale: appLocale } = useI18n()
const loc = computed(() => props.locale || appLocale.value)
const display = ref('')

watch(
  () => props.modelValue,
  v => {
    const n = parseAmount(v)
    display.value = n ? formatAmountTyping(String(n), loc.value).display : ''
  },
  { immediate: true }
)

function onInput(e) {
  const { numeric, display: d } = formatAmountTyping(e.target.value, loc.value)
  display.value = d
  emit('update:modelValue', numeric)
}

function onBlur() {
  const n = parseAmount(props.modelValue)
  if (props.min && n < props.min) emit('update:modelValue', props.min)
  display.value = n ? formatAmountTyping(String(n), loc.value).display : ''
}
</script>

<template>
  <input
    :value="display"
    type="text"
    inputmode="numeric"
    :class="[inputClass, dir === 'ltr' ? 'input-ltr' : '']"
    :dir="dir"
    :placeholder="placeholder"
    autocomplete="off"
    @input="onInput"
    @blur="onBlur"
  />
</template>
