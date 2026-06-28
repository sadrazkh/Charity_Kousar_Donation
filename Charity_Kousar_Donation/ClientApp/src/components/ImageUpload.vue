<script setup>
import { ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { uploadFile } from '@/api/client'

defineProps({
  modelValue: { type: String, default: '' }
})
const emit = defineEmits(['update:modelValue'])

const { locale } = useI18n()
const uploading = ref(false)
const error = ref('')
const dragOver = ref(false)

async function handleFile(file) {
  if (!file) return
  if (!file.type.startsWith('image/')) {
    error.value = locale.value === 'fa' ? 'فقط فایل تصویری مجاز است' : 'Only image files are allowed'
    return
  }
  error.value = ''
  uploading.value = true
  try {
    const { url } = await uploadFile(file)
    emit('update:modelValue', url)
  } catch (err) {
    error.value = err.message
  } finally {
    uploading.value = false
  }
}

function onPick(e) {
  const file = e.target.files?.[0]
  handleFile(file)
  e.target.value = ''
}

function onDrop(e) {
  dragOver.value = false
  handleFile(e.dataTransfer?.files?.[0])
}
</script>

<template>
  <div class="image-upload">
    <div class="iu-row">
      <input :value="modelValue" class="input input-ltr" dir="ltr" placeholder="https://... یا آپلود کنید"
        @input="emit('update:modelValue', $event.target.value)" />
      <button v-if="modelValue" type="button" class="btn btn-ghost btn-sm" @click="emit('update:modelValue', '')">
        {{ locale === 'fa' ? 'حذف' : 'Remove' }}
      </button>
    </div>

    <label
      class="dropzone"
      :class="{ over: dragOver, busy: uploading }"
      @dragover.prevent="dragOver = true"
      @dragleave.prevent="dragOver = false"
      @drop.prevent="onDrop"
    >
      <img v-if="modelValue && !uploading" :src="modelValue" class="thumb" alt="" />
      <div v-else class="dz-inner">
        <span class="dz-icon">{{ uploading ? '⏳' : '📤' }}</span>
        <span class="dz-text">{{ uploading
          ? (locale === 'fa' ? 'در حال آپلود...' : 'Uploading...')
          : (locale === 'fa' ? 'کلیک کنید یا تصویر را اینجا بکشید' : 'Click or drag an image here') }}</span>
        <span class="dz-hint">JPG · PNG · WebP · GIF — حداکثر ۵MB</span>
      </div>
      <input type="file" accept="image/*" hidden @change="onPick" />
    </label>

    <p v-if="error" class="err">⚠ {{ error }}</p>
  </div>
</template>

<style scoped>
.image-upload { display: flex; flex-direction: column; gap: 0.5rem; }
.iu-row { display: flex; gap: 0.5rem; align-items: center; }
.iu-row .input { flex: 1; min-width: 0; }
.dropzone {
  display: flex; align-items: center; justify-content: center;
  min-height: 120px; padding: 0.75rem;
  border: 2px dashed var(--border); border-radius: 12px;
  background: var(--input-bg); cursor: pointer; text-align: center;
  transition: border-color 0.15s, background 0.15s;
}
.dropzone.over { border-color: var(--primary); background: color-mix(in srgb, var(--primary) 10%, transparent); }
.dropzone.busy { opacity: 0.7; cursor: progress; }
.dz-inner { display: flex; flex-direction: column; align-items: center; gap: 0.35rem; color: var(--muted); }
.dz-icon { font-size: 1.6rem; }
.dz-text { font-size: 0.9rem; }
.dz-hint { font-size: 0.72rem; opacity: 0.8; }
.thumb { max-width: 100%; max-height: 180px; object-fit: contain; border-radius: 8px; }
.err { color: #f87171; font-size: 0.82rem; margin: 0; }
</style>
