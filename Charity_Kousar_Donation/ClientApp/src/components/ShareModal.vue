<script setup>
import { ref, computed, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { api } from '@/api/client'

const props = defineProps({
  slug: { type: String, required: true },
  show: Boolean
})
const emit = defineEmits(['close'])

const { locale } = useI18n()
const loading = ref(false)
const pack = ref(null)
const copied = ref(false)

const message = computed(() => {
  if (!pack.value) return ''
  return locale.value === 'fa' ? pack.value.messageFa : pack.value.messageEn
})

const waUrl = computed(() => locale.value === 'fa' ? pack.value?.whatsAppUrlFa : pack.value?.whatsAppUrlEn)
const tgUrl = computed(() => locale.value === 'fa' ? pack.value?.telegramUrlFa : pack.value?.telegramUrlEn)

async function load() {
  loading.value = true
  try {
    pack.value = await api(`/campaigns/${props.slug}/share-pack?t=${Date.now()}`)
  } finally {
    loading.value = false
  }
}

async function refresh() {
  pack.value = null
  await load()
}

function openShare(url) {
  window.open(url, '_blank', 'noopener,noreferrer')
}

async function copyText() {
  await navigator.clipboard.writeText(message.value)
  copied.value = true
  setTimeout(() => { copied.value = false }, 2000)
}

watch(() => props.show, v => { if (v) { pack.value = null; load() } })
</script>

<template>
  <div v-if="show" class="modal-overlay" @click.self="emit('close')">
    <div class="card modal share-modal">
      <div class="modal-head">
        <h2>{{ locale === 'fa' ? '📤 اشتراک‌گذاری' : '📤 Share' }}</h2>
        <button type="button" class="icon-btn" @click="emit('close')">✕</button>
      </div>

      <p class="hint">{{ locale === 'fa'
        ? 'AI بر اساس محتوای همین کمپین متن مناسب واتساپ/تلگرام می‌سازد'
        : 'AI builds a WhatsApp/Telegram message from this campaign content' }}</p>

      <div v-if="loading" class="loading">{{ locale === 'fa' ? 'در حال آماده‌سازی...' : 'Preparing...' }}</div>

      <template v-else-if="pack">
        <textarea class="textarea share-text" :value="message" readonly rows="10" />

        <div class="share-actions">
          <a :href="waUrl" class="btn btn-primary" target="_blank" rel="noopener" @click.prevent="openShare(waUrl)">
            WhatsApp
          </a>
          <a :href="tgUrl" class="btn btn-primary" target="_blank" rel="noopener" @click.prevent="openShare(tgUrl)">
            Telegram
          </a>
          <button type="button" class="btn btn-ghost" @click="copyText">{{ copied ? '✓' : (locale === 'fa' ? 'کپی متن' : 'Copy') }}</button>
          <button type="button" class="btn btn-accent btn-sm" @click="refresh">✨ {{ locale === 'fa' ? 'متن جدید' : 'Regenerate' }}</button>
        </div>
      </template>
    </div>
  </div>
</template>

<style scoped>
.share-modal { max-width: 480px; width: 100%; max-height: 92vh; overflow-y: auto; padding: 1.25rem; }
.modal-head { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.5rem; }
.modal-head h2 { font-size: 1.1rem; }
.icon-btn { background: none; border: none; color: var(--muted); font-size: 1.1rem; cursor: pointer; padding: 0.35rem; }
.hint { color: var(--muted); font-size: 0.85rem; margin-bottom: 1rem; }
.loading { text-align: center; padding: 2rem; color: var(--muted); }
.share-text { font-size: 0.9rem; line-height: 1.7; margin-bottom: 1rem; resize: none; }
.share-actions { display: flex; flex-wrap: wrap; gap: 0.5rem; }
.share-actions .btn { flex: 1 1 calc(50% - 0.25rem); min-width: 120px; min-height: 44px; }
@media (max-width: 400px) { .share-actions .btn { flex: 1 1 100%; } }
</style>
