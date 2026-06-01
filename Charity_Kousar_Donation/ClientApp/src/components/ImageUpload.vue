<script setup>
import { ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { uploadFile } from '@/api/client'

const props = defineProps({
  modelValue: { type: String, default: '' }
})
const emit = defineEmits(['update:modelValue'])

const { locale } = useI18n()
const uploading = ref(false)
const error = ref('')

async function onPick(e) {
  const file = e.target.files?.[0]
  if (!file) return
  error.value = ''
  uploading.value = true
  try {
    const { url } = await uploadFile(file)
    emit('update:modelValue', url)
  } catch (err) {
    error.value = err.message
  } finally {
    uploading.value = false
    e.target.value = ''
  }
}
</script>

<template>
  <div class="image-upload">
    <input :value="modelValue" class="input input-ltr" dir="ltr" placeholder="https://..."
      @input="emit('update:modelValue', $event.target.value)" />
    <label class="btn btn-ghost btn-sm upload-btn">
      {{ uploading ? '...' : (locale === 'fa' ? '📤 آپلود' : '📤 Upload') }}
      <input type="file" accept="image/*" hidden @change="onPick" />
    </label>
    <img v-if="modelValue" :src="modelValue" class="thumb" alt="" />
    <p v-if="error" class="err">{{ error }}</p>
  </div>
</template>

<style scoped>
.image-upload { display: flex; flex-wrap: wrap; gap: 0.5rem; align-items: center; }
.image-upload .input { flex: 1; min-width: 160px; }
.upload-btn { cursor: pointer; white-space: nowrap; }
.thumb { width: 64px; height: 64px; object-fit: cover; border-radius: 8px; border: 1px solid rgba(148,163,184,0.25); }
.err { color: #f87171; font-size: 0.8rem; width: 100%; }
</style>
