<script setup>
import { ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { api } from '@/api/client'
import { useToast } from '@/composables/useToast'

const props = defineProps({ values: { type: Object, required: true } })
const { t } = useI18n()
const toast = useToast()
const translating = ref(false)

async function translateHero() {
  const text = props.values['site.hero.fa']
  if (!text || !text.trim()) { toast.error(t('homeEditor.needPersianFirst')); return }
  translating.value = true
  try {
    const res = await api('/ai/translate', { method: 'POST', body: JSON.stringify({ text, from: 'fa', to: 'en' }) })
    props.values['site.hero.en'] = res.translated
    toast.success(t('homeEditor.translated'))
  } catch (e) { toast.error(e.message) } finally { translating.value = false }
}
</script>

<template>
  <section class="card block">
    <h2>{{ t('homeEditor.heroTitle') }}</h2>
    <label class="label">{{ t('homeEditor.persianText') }}</label>
    <textarea v-model="values['site.hero.fa']" class="textarea" rows="2" />
    <div class="label-row">
      <label class="label">{{ t('homeEditor.englishText') }}</label>
      <button type="button" class="translate-btn" :disabled="translating" @click="translateHero">
        <svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M4 5h10M9 3v2c0 5-2.5 8-6 9M6 9c0 3 3 5 6 6M13 21l4-9 4 9M14.5 17h5"/></svg>
        {{ translating ? '…' : t('homeEditor.translateFromFa') }}
      </button>
    </div>
    <textarea v-model="values['site.hero.en']" class="textarea input-ltr" dir="ltr" rows="2" />

    <label class="label">{{ t('homeEditor.heroBadgeFa') }}</label>
    <input v-model="values['site.hero.badge.fa']" class="input" />
    <label class="label">{{ t('homeEditor.heroBadgeEn') }}</label>
    <input v-model="values['site.hero.badge.en']" class="input input-ltr" dir="ltr" />
    <p class="hint">{{ t('homeEditor.heroBadgeHint') }}</p>
  </section>
</template>

<style scoped>
.translate-btn {
  display: inline-flex; align-items: center; gap: 0.3rem;
  background: color-mix(in srgb, var(--accent) 16%, transparent); color: var(--accent);
  border: none; border-radius: 999px; padding: 0.2rem 0.7rem;
  font-size: 0.75rem; cursor: pointer; font-family: inherit;
}
.translate-btn:disabled { opacity: 0.5; }
.translate-btn .icon { width: 14px; height: 14px; }
</style>
