import { reactive } from 'vue'
import { api, uploadFile } from '@/api/client'

// Shared image library for the admin panel: the ready-made illustrations shipped in
// /presets plus the gallery the admin builds by uploading their own images.
const state = reactive({ presets: [], gallery: [], loaded: false })
let inflight = null

function parseGallery(json) {
  try {
    const arr = JSON.parse(json || '[]')
    return Array.isArray(arr) ? arr.filter(x => x && x.url) : []
  } catch {
    return []
  }
}

export async function loadMedia(force = false) {
  if (state.loaded && !force) return state
  if (inflight && !force) return inflight
  inflight = (async () => {
    try {
      const res = await api('/media')
      state.presets = Array.isArray(res.presets) ? res.presets : []
      state.gallery = parseGallery(res.gallery)
      state.loaded = true
    } finally {
      inflight = null
    }
    return state
  })()
  return inflight
}

async function persist() {
  await api('/media/gallery', { method: 'PUT', body: JSON.stringify({ json: JSON.stringify(state.gallery) }) })
}

/** Uploads files and appends them to the gallery. Returns the URLs that were added. */
async function addFiles(files) {
  const added = []
  for (const file of files) {
    if (!file?.type?.startsWith('image/')) continue
    const { url } = await uploadFile(file)
    if (state.gallery.some(g => g.url === url)) continue
    state.gallery.push({ url, titleFa: '', titleEn: '' })
    added.push(url)
  }
  if (added.length) await persist()
  return added
}

async function addUrl(url, titleFa = '', titleEn = '') {
  if (!url || state.gallery.some(g => g.url === url)) return false
  state.gallery.push({ url, titleFa, titleEn })
  await persist()
  return true
}

async function removeImage(url) {
  state.gallery = state.gallery.filter(g => g.url !== url)
  await persist()
}

async function saveGallery() {
  await persist()
}

export function useMediaLibrary() {
  return { media: state, loadMedia, addFiles, addUrl, removeImage, saveGallery }
}
