<script setup>
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useToast } from '@/composables/useToast'
import { useMediaLibrary } from '@/composables/useMediaLibrary'

const { t, locale } = useI18n()
const toast = useToast()
const { media, loadMedia, addFiles, addUrl, removeImage, saveGallery } = useMediaLibrary()

const fa = computed(() => locale.value === 'fa')
const loading = ref(true)
const uploading = ref(false)
const saving = ref(false)
const dragOver = ref(false)
const urlInput = ref('')

onMounted(async () => {
  try {
    await loadMedia(true)
  } catch (e) {
    toast.error(e.message)
  } finally {
    loading.value = false
  }
})

function fileName(url) {
  return decodeURIComponent(String(url).split('/').pop() || '')
}

async function upload(files) {
  const list = [...(files || [])].filter(f => f.type?.startsWith('image/'))
  if (!list.length) return
  uploading.value = true
  try {
    const added = await addFiles(list)
    toast.success(fa.value ? `${added.length} تصویر افزوده شد ✓` : `${added.length} images added ✓`)
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

async function addFromUrl() {
  const url = urlInput.value.trim()
  if (!url) return
  try {
    const ok = await addUrl(url)
    if (!ok) { toast.error(fa.value ? 'این تصویر قبلاً در گالری هست' : 'Already in the gallery'); return }
    urlInput.value = ''
    toast.success(t('savedToast'))
  } catch (e) { toast.error(e.message) }
}

/** Copies a shipped illustration into the gallery so it can be renamed like the rest. */
async function addPreset(url) {
  try {
    const ok = await addUrl(url)
    toast[ok ? 'success' : 'error'](ok
      ? (fa.value ? 'به گالری افزوده شد ✓' : 'Added to gallery ✓')
      : (fa.value ? 'قبلاً در گالری هست' : 'Already in the gallery'))
  } catch (e) { toast.error(e.message) }
}

async function remove(url) {
  if (!confirm(fa.value ? 'این تصویر از گالری حذف شود؟' : 'Remove this image from the gallery?')) return
  try {
    await removeImage(url)
    toast.success(t('savedToast'))
  } catch (e) { toast.error(e.message) }
}

async function saveTitles() {
  saving.value = true
  try {
    await saveGallery()
    toast.success(t('savedToast'))
  } catch (e) { toast.error(e.message) } finally { saving.value = false }
}
</script>

<template>
  <div class="media-page">
    <div class="toolbar">
      <div>
        <h1>{{ fa ? 'تصاویر پروژه‌ها' : 'Project images' }}</h1>
        <p class="sub">{{ fa
          ? 'تصویرهای آماده و گالری خودتان را اینجا مدیریت کنید؛ هنگام ساخت هر پروژه از همین‌ها انتخاب می‌کنید.'
          : 'Manage the ready-made images and your own gallery; pick from them when editing a project.' }}</p>
      </div>
      <button class="btn btn-primary" :disabled="saving" @click="saveTitles">{{ saving ? '...' : t('save') }}</button>
    </div>

    <p v-if="loading" class="state">{{ fa ? 'در حال بارگذاری...' : 'Loading...' }}</p>

    <template v-else>
      <!-- Ready-made images shipped with the site -->
      <section class="card block">
        <h2>{{ fa ? 'تصاویر پیشنهادی' : 'Ready-made images' }}</h2>
        <p class="hint">{{ fa
          ? 'این‌ها فایل‌های پوشهٔ ClientApp/public/presets هستند. هر تصویری در آن پوشه بگذارید، بدون تغییر کد اینجا و در انتخاب‌گر تصویر پروژه‌ها ظاهر می‌شود.'
          : 'These are the files in ClientApp/public/presets. Any image dropped into that folder shows up here and in the project image picker — no code change needed.' }}</p>
        <div v-if="media.presets.length" class="grid">
          <figure v-for="url in media.presets" :key="url" class="tile">
            <img :src="url" :alt="fileName(url)" loading="lazy" />
            <figcaption>{{ fileName(url) }}</figcaption>
            <button type="button" class="btn btn-ghost btn-sm" @click="addPreset(url)">
              {{ fa ? '+ افزودن به گالری' : '+ Add to gallery' }}
            </button>
          </figure>
        </div>
        <p v-else class="state">{{ fa
          ? 'هنوز تصویر آماده‌ای در پوشهٔ presets نیست.'
          : 'The presets folder is still empty.' }}</p>
      </section>

      <!-- Admin-managed gallery -->
      <section class="card block">
        <h2>{{ fa ? 'گالری من' : 'My gallery' }}</h2>
        <label class="dropzone" :class="{ over: dragOver, busy: uploading }"
          @dragover.prevent="dragOver = true" @dragleave.prevent="dragOver = false" @drop.prevent="onDrop">
          <span class="dz-text">{{ uploading
            ? (fa ? 'در حال آپلود...' : 'Uploading...')
            : (fa ? 'تصویرها را اینجا بکشید یا کلیک کنید (چندتایی هم می‌شود)' : 'Drop images here or click (multiple allowed)') }}</span>
          <span class="dz-hint">JPG · PNG · WebP · GIF · SVG — {{ fa ? 'حداکثر ۵ مگابایت' : 'max 5MB' }}</span>
          <input type="file" accept="image/*" multiple hidden :disabled="uploading" @change="onPick" />
        </label>

        <div class="url-row">
          <input v-model="urlInput" class="input input-ltr" dir="ltr" placeholder="https://..." />
          <button type="button" class="btn btn-ghost btn-sm" :disabled="!urlInput.trim()" @click="addFromUrl">
            {{ fa ? 'افزودن با آدرس' : 'Add by URL' }}
          </button>
        </div>

        <div v-if="media.gallery.length" class="grid">
          <figure v-for="img in media.gallery" :key="img.url" class="tile">
            <img :src="img.url" :alt="img.titleFa || fileName(img.url)" loading="lazy" />
            <input v-model="img.titleFa" class="input input-sm" :placeholder="fa ? 'نام فارسی' : 'Persian name'" />
            <input v-model="img.titleEn" class="input input-sm input-ltr" dir="ltr" placeholder="English name" />
            <button type="button" class="btn btn-ghost btn-sm danger" @click="remove(img.url)">{{ t('delete') }}</button>
          </figure>
        </div>
        <p v-else class="state">{{ fa ? 'گالری خالی است.' : 'The gallery is empty.' }}</p>
        <p class="hint">{{ fa
          ? 'نام‌ها فقط برای پیدا کردن راحت‌تر تصویر در انتخاب‌گر است. بعد از تغییر نام، دکمهٔ ذخیره را بزنید.'
          : 'Names only help you find images in the picker. Press save after renaming.' }}</p>
      </section>
    </template>
  </div>
</template>

<style scoped>
.toolbar { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; flex-wrap: wrap; margin-bottom: 1.25rem; }
.sub { color: var(--muted); font-size: 0.9rem; margin-top: 0.2rem; }
.block { margin-bottom: 1rem; }
.block h2 { font-size: 1rem; color: var(--primary); margin-bottom: 0.6rem; }
.hint { font-size: 0.82rem; color: var(--muted); line-height: 1.8; margin: 0.75rem 0; }
.state { color: var(--muted); font-size: 0.88rem; padding: 0.75rem 0; }

.grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(160px, 1fr)); gap: 0.85rem; margin-top: 0.5rem; }
.tile {
  display: flex; flex-direction: column; gap: 0.4rem; margin: 0; padding: 0.5rem;
  border: 1px solid var(--border); border-radius: 12px; background: var(--input-bg);
}
.tile img { width: 100%; aspect-ratio: 1; object-fit: contain; border-radius: 8px; background: var(--bg-soft); }
.tile figcaption { font-size: 0.72rem; color: var(--muted); overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.input-sm { padding: 0.35rem 0.5rem; font-size: 0.8rem; }
.danger { color: var(--danger); }

.dropzone {
  display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 0.3rem;
  min-height: 96px; padding: 0.75rem; text-align: center; cursor: pointer;
  border: 2px dashed var(--border); border-radius: 12px; background: var(--input-bg); color: var(--muted);
  transition: border-color 0.15s, background 0.15s;
}
.dropzone.over { border-color: var(--primary); background: color-mix(in srgb, var(--primary) 10%, transparent); }
.dropzone.busy { opacity: 0.7; cursor: progress; }
.dz-text { font-size: 0.9rem; }
.dz-hint { font-size: 0.72rem; opacity: 0.8; }
.url-row { display: flex; gap: 0.5rem; align-items: center; margin-top: 0.75rem; }
.url-row .input { flex: 1; min-width: 0; }
</style>
