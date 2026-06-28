<script setup>
import { ref, computed, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { api } from '@/api/client'
import { useSiteConfig } from '@/composables/useSiteConfig'

const props = defineProps({
  slug: { type: String, required: true },
  show: Boolean
})
const emit = defineEmits(['close'])

const { locale } = useI18n()
const { config } = useSiteConfig()
const loading = ref(false)
const pack = ref(null)
const copied = ref(false)

const editable = ref('')
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
    editable.value = message.value
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
  await navigator.clipboard.writeText(editable.value || message.value)
  copied.value = true
  setTimeout(() => { copied.value = false }, 2000)
}

watch(() => props.show, v => { if (v) { pack.value = null; load() } })
</script>

<template>
  <div v-if="show" class="modal-overlay" @click.self="emit('close')">
    <div class="card modal share-modal">
      <div class="modal-head">
        <h2>📤 {{ locale === 'fa' ? 'اشتراک‌گذاری پروژه' : 'Share project' }}</h2>
        <button type="button" class="icon-btn" @click="emit('close')">✕</button>
      </div>

      <p class="hint">{{ locale === 'fa'
        ? 'این متن آماده است. می‌توانید آن را ویرایش، کپی یا مستقیم ارسال کنید.'
        : 'This text is ready. You can edit, copy, or send it directly.' }}</p>

      <div v-if="loading" class="loading">{{ locale === 'fa' ? 'در حال آماده‌سازی...' : 'Preparing...' }}</div>

      <template v-else-if="pack">
        <textarea class="textarea share-text" v-model="editable" rows="9" />

        <div class="share-actions">
          <button type="button" class="btn btn-success big" @click="openShare(waUrl)">💬 WhatsApp</button>
          <button type="button" class="btn btn-info big" @click="openShare(tgUrl)">✈️ Telegram</button>
          <button type="button" class="btn btn-primary big" @click="copyText">
            {{ copied ? '✓ ' + (locale === 'fa' ? 'کپی شد' : 'Copied') : (locale === 'fa' ? '📋 کپی متن' : '📋 Copy') }}
          </button>
          <button v-if="config.shareAiEnabled" type="button" class="btn btn-ghost big" @click="refresh">
            ✨ {{ locale === 'fa' ? 'متن جدید' : 'New text' }}
          </button>
        </div>
      </template>
    </div>
  </div>
</template>

<style scoped>
.share-modal { max-width: 520px; width: 100%; max-height: 92vh; overflow-y: auto; padding: 1.5rem; }
.modal-head { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.5rem; }
.modal-head h2 { font-size: 1.25rem; }
.icon-btn { background: none; border: none; color: var(--muted); font-size: 1.3rem; cursor: pointer; padding: 0.35rem; }
.hint { color: var(--muted); font-size: 0.95rem; margin-bottom: 1rem; line-height: 1.7; }
.loading { text-align: center; padding: 2rem; color: var(--muted); }
.share-text { font-size: 1.02rem; line-height: 1.9; margin-bottom: 1.1rem; min-height: 180px; }
.share-actions { display: grid; grid-template-columns: 1fr 1fr; gap: 0.6rem; }
.share-actions .big { min-height: 52px; font-size: 1rem; }
.btn-success { background: #25d366; color: #06351c; }
.btn-info { background: #2aabee; color: #04263a; }
@media (max-width: 400px) { .share-actions { grid-template-columns: 1fr; } }
</style>
