<script setup>
import { ref, computed, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useMediaLibrary } from '@/composables/useMediaLibrary'
import { useToast } from '@/composables/useToast'

const props = defineProps({
  show: Boolean,
  current: { type: String, default: '' }
})
const emit = defineEmits(['close', 'select'])

const { locale } = useI18n()
const toast = useToast()
const { media, loadMedia, addFiles } = useMediaLibrary()

const fa = computed(() => locale.value === 'fa')
const tab = ref('presets')
const loading = ref(false)
const uploading = ref(false)
const dragOver = ref(false)

const items = computed(() => tab.value === 'presets'
  ? media.presets.map(url => ({ url, title: prettyName(url) }))
  : media.gallery.map(g => ({ url: g.url, title: (fa.value ? g.titleFa : g.titleEn) || prettyName(g.url) })))

function prettyName(url) {
  const file = String(url).split('/').pop() || ''
  return decodeURIComponent(file.replace(/\.[a-z0-9]+$/i, '')).replace(/[-_]+/g, ' ')
}

async function open() {
  loading.value = true
  try {
    await loadMedia(true)
    // Start on whichever shelf actually has images.
    tab.value = media.presets.length ? 'presets' : 'gallery'
  } catch (e) {
    toast.error(e.message)
  } finally {
    loading.value = false
  }
}

watch(() => props.show, v => { if (v) open() })

function choose(url) {
  emit('select', url)
  emit('close')
}

async function upload(files) {
  const list = [...(files || [])]
  if (!list.length) return
  uploading.value = true
  try {
    const added = await addFiles(list)
    tab.value = 'gallery'
    if (added.length === 1) choose(added[0])
    else if (added.length) toast.success(fa.value ? `${added.length} تصویر افزوده شد ✓` : `${added.length} images added ✓`)
    else toast.error(fa.value ? 'تصویر تکراری یا نامعتبر بود' : 'Duplicate or invalid image')
  } catch (e) {
    toast.error(e.message)
  } finally {
    uploading.value = false
  }
}

function onPick(e) {
  upload(e.target.files)
  e.target.value = ''
}

function onDrop(e) {
  dragOver.value = false
  upload(e.dataTransfer?.files)
}
</script>

<template>
  <div v-if="show" class="modal-overlay" @click.self="emit('close')">
    <div class="card modal picker">
      <div class="modal-head">
        <h2>{{ fa ? 'انتخاب تصویر' : 'Choose an image' }}</h2>
        <button type="button" class="icon-btn" :aria-label="fa ? 'بستن' : 'Close'" @click="emit('close')">✕</button>
      </div>

      <div class="tabs">
        <button type="button" :class="{ active: tab === 'presets' }" @click="tab = 'presets'">
          {{ fa ? 'تصاویر پیشنهادی' : 'Ready-made' }}
          <span class="count">{{ media.presets.length }}</span>
        </button>
        <button type="button" :class="{ active: tab === 'gallery' }" @click="tab = 'gallery'">
          {{ fa ? 'گالری من' : 'My gallery' }}
          <span class="count">{{ media.gallery.length }}</span>
        </button>
      </div>

      <p v-if="loading" class="state">{{ fa ? 'در حال بارگذاری...' : 'Loading...' }}</p>

      <div v-else class="grid" :class="{ over: dragOver }"
        @dragover.prevent="dragOver = true" @dragleave.prevent="dragOver = false" @drop.prevent="onDrop">
        <button v-for="it in items" :key="it.url" type="button"
          class="tile" :class="{ selected: it.url === current }" :title="it.title" @click="choose(it.url)">
          <img :src="it.url" :alt="it.title" loading="lazy" />
          <span class="tile-name">{{ it.title }}</span>
        </button>

        <p v-if="!items.length && tab === 'presets'" class="state empty">
          {{ fa
            ? 'هنوز تصویر آماده‌ای موجود نیست. فایل‌ها را در پوشهٔ ClientApp/public/presets بگذارید یا از همین‌جا آپلود کنید.'
            : 'No ready-made images yet. Drop files into ClientApp/public/presets, or upload them here.' }}
        </p>
        <p v-else-if="!items.length" class="state empty">
          {{ fa ? 'گالری خالی است — تصویر خود را آپلود کنید.' : 'Your gallery is empty — upload an image.' }}
        </p>
      </div>

      <div class="foot">
        <label class="btn btn-primary btn-sm upload-btn">
          {{ uploading ? (fa ? 'در حال آپلود...' : 'Uploading...') : (fa ? '+ آپلود تصویر جدید' : '+ Upload new image') }}
          <input type="file" accept="image/*" multiple hidden :disabled="uploading" @change="onPick" />
        </label>
        <router-link to="/admin/media" class="manage-link" @click="emit('close')">
          {{ fa ? 'مدیریت گالری ←' : 'Manage gallery →' }}
        </router-link>
      </div>
    </div>
  </div>
</template>

<style scoped>
.picker { max-width: 720px; width: 100%; max-height: 92vh; overflow-y: auto; padding: 1.5rem; }
.modal-head { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.75rem; }
.modal-head h2 { font-size: 1.15rem; }
.icon-btn { background: none; border: none; color: var(--muted); font-size: 1.2rem; cursor: pointer; padding: 0.35rem; }

.tabs { display: flex; gap: 0.35rem; margin-bottom: 1rem; }
.tabs button {
  display: inline-flex; align-items: center; gap: 0.4rem;
  padding: 0.45rem 1rem; border: none; border-radius: 999px;
  background: transparent; color: var(--muted); cursor: pointer; font-family: inherit; font-size: 0.9rem;
}
.tabs button.active { background: color-mix(in srgb, var(--primary) 18%, transparent); color: var(--primary); font-weight: 600; }
.count { font-size: 0.72rem; opacity: 0.75; }

.grid {
  display: grid; grid-template-columns: repeat(auto-fill, minmax(140px, 1fr)); gap: 0.75rem;
  border: 2px dashed transparent; border-radius: 12px; padding: 0.25rem; min-height: 140px;
}
.grid.over { border-color: var(--primary); background: color-mix(in srgb, var(--primary) 8%, transparent); }
.tile {
  display: flex; flex-direction: column; gap: 0.35rem; padding: 0.4rem;
  border: 1px solid var(--border); border-radius: 12px; background: var(--input-bg);
  cursor: pointer; font-family: inherit; text-align: center;
}
.tile:hover { border-color: color-mix(in srgb, var(--primary) 45%, transparent); }
.tile.selected { border-color: var(--primary); box-shadow: 0 0 0 2px color-mix(in srgb, var(--primary) 35%, transparent); }
.tile img { width: 100%; aspect-ratio: 1; object-fit: contain; border-radius: 8px; background: var(--bg-soft); }
.tile-name { font-size: 0.72rem; color: var(--muted); overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }

.state { color: var(--muted); font-size: 0.88rem; padding: 1rem 0; }
.state.empty { grid-column: 1 / -1; text-align: center; line-height: 1.8; }
.foot { display: flex; align-items: center; justify-content: space-between; gap: 0.75rem; margin-top: 1rem; flex-wrap: wrap; }
.upload-btn { cursor: pointer; }
.manage-link { font-size: 0.85rem; color: var(--primary); text-decoration: none; }
</style>
