<script setup>
import { ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { api } from '@/api/client'

const props = defineProps({
  modelValue: { type: String, default: '' },
  language: { type: String, default: 'fa' },
  fieldType: { type: String, default: 'body' },
  campaignTitle: String,
  multiline: Boolean
})
const emit = defineEmits(['update:modelValue'])

const { locale } = useI18n()
const loading = ref(false)
const suggestion = ref(null)
const error = ref('')

async function optimize() {
  if (!props.modelValue?.trim()) return
  loading.value = true
  error.value = ''
  suggestion.value = null
  try {
    const res = await api('/ai/optimize', {
      method: 'POST',
      body: JSON.stringify({
        text: props.modelValue,
        language: props.language,
        fieldType: props.fieldType,
        campaignTitle: props.campaignTitle
      })
    })
    suggestion.value = res
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}

function apply(text) {
  emit('update:modelValue', text)
  suggestion.value = null
}
</script>

<template>
  <div class="ai-wrap">
    <button type="button" class="btn-ai" :disabled="loading || !props.modelValue?.trim()" @click="optimize">
      {{ loading ? '...' : (locale === 'fa' ? '✨ بهینه‌سازی AI' : '✨ AI optimize') }}
    </button>
    <p v-if="error" class="ai-error">{{ error }}</p>
    <div v-if="suggestion" class="ai-suggestion card">
      <p class="ai-label">{{ locale === 'fa' ? 'پیشنهاد AI' : 'AI suggestion' }}</p>
      <p class="ai-text">{{ suggestion.optimized }}</p>
      <div class="ai-actions">
        <button type="button" class="btn btn-primary btn-sm" @click="apply(suggestion.optimized)">
          {{ locale === 'fa' ? 'اعمال' : 'Apply' }}
        </button>
        <button v-if="suggestion.alternative" type="button" class="btn btn-ghost btn-sm" @click="apply(suggestion.alternative)">
          {{ locale === 'fa' ? 'جایگزین' : 'Alternative' }}
        </button>
        <button type="button" class="btn btn-ghost btn-sm" @click="suggestion = null">{{ locale === 'fa' ? 'بستن' : 'Close' }}</button>
      </div>
      <p v-if="suggestion.tips" class="ai-tip">💡 {{ suggestion.tips }}</p>
    </div>
  </div>
</template>

<style scoped>
.ai-wrap { margin-top: 0.35rem; }
.btn-ai {
  background: linear-gradient(135deg, #6366f1, #8b5cf6);
  border: none; color: white; padding: 0.4rem 0.85rem;
  border-radius: 999px; font-size: 0.8rem; cursor: pointer; font-family: inherit; font-weight: 600;
}
.btn-ai:disabled { opacity: 0.5; cursor: not-allowed; }
.ai-error { color: #f87171; font-size: 0.8rem; margin-top: 0.35rem; }
.ai-suggestion { margin-top: 0.5rem; padding: 0.85rem; background: rgba(99,102,241,0.1); border-color: rgba(99,102,241,0.3); }
.ai-label { font-size: 0.75rem; color: #a5b4fc; margin-bottom: 0.35rem; }
.ai-text { white-space: pre-wrap; line-height: 1.7; font-size: 0.9rem; }
.ai-actions { display: flex; gap: 0.35rem; flex-wrap: wrap; margin-top: 0.65rem; }
.ai-tip { font-size: 0.8rem; color: var(--muted); margin-top: 0.5rem; }
</style>
