<script setup>
import { ref, onMounted, computed, nextTick } from 'vue'
import { useI18n } from 'vue-i18n'
import { api } from '@/api/client'

const { t, locale } = useI18n()
const groups = ref([])
const values = ref({})
const saved = ref(false)
const activeGroup = ref('site')
const showTour = ref(false)
const tourStep = ref(0)

const sectionIds = computed(() => groups.value.map(g => g.group))

const tourSteps = computed(() => groups.value.map(g => ({
  id: g.group,
  title: locale.value === 'fa' ? g.groupLabelFa : g.groupLabelEn,
  hint: locale.value === 'fa'
    ? 'این بخش را در منوی کناری انتخاب کنید و تنظیمات را ویرایش کنید.'
    : 'Select this section in the sidebar and edit the settings.'
})))

onMounted(async () => {
  groups.value = await api('/settings')
  for (const g of groups.value) {
    for (const item of g.items) values.value[item.key] = item.value
  }
  if (groups.value.length) activeGroup.value = groups.value[0].group
  if (!localStorage.getItem('settings_tour_done')) showTour.value = true

  await nextTick()
  const observer = new IntersectionObserver(
    (entries) => {
      const visible = entries.filter(e => e.isIntersecting).sort((a, b) => b.intersectionRatio - a.intersectionRatio)[0]
      if (visible) activeGroup.value = visible.target.id.replace('settings-', '')
    },
    { rootMargin: '-15% 0px -55% 0px', threshold: [0, 0.25, 0.5] }
  )
  for (const g of groups.value) {
    const el = document.getElementById(`settings-${g.group}`)
    if (el) observer.observe(el)
  }
})

function label(item) {
  return locale.value === 'fa' ? item.labelFa : item.labelEn
}

function scrollToGroup(id) {
  activeGroup.value = id
  nextTick(() => {
    document.getElementById(`settings-${id}`)?.scrollIntoView({ behavior: 'smooth', block: 'start' })
  })
}

async function save() {
  await api('/settings', { method: 'PUT', body: JSON.stringify({ settings: values.value }) })
  saved.value = true
  setTimeout(() => { saved.value = false }, 3000)
}

function nextTour() {
  if (tourStep.value < tourSteps.value.length - 1) {
    tourStep.value++
    scrollToGroup(tourSteps.value[tourStep.value].id)
  } else {
    finishTour()
  }
}

function finishTour() {
  showTour.value = false
  localStorage.setItem('settings_tour_done', '1')
}

function restartTour() {
  tourStep.value = 0
  showTour.value = true
  if (tourSteps.value.length) scrollToGroup(tourSteps.value[0].id)
}
</script>

<template>
  <div class="settings-page">
    <div class="toolbar">
      <h1>{{ t('settings') }}</h1>
      <div class="toolbar-actions">
        <button type="button" class="btn btn-ghost btn-sm" @click="restartTour">?</button>
        <button class="btn btn-primary" @click="save">{{ t('save') }}</button>
      </div>
    </div>
    <p v-if="saved" class="saved">✓ {{ locale === 'fa' ? 'ذخیره شد' : 'Saved' }}</p>

    <div class="settings-layout">
      <nav class="settings-nav card">
        <p class="nav-title">{{ locale === 'fa' ? 'بخش‌ها' : 'Sections' }}</p>
        <button v-for="g in groups" :key="g.group" type="button"
          class="nav-item" :class="{ active: activeGroup === g.group }"
          @click="scrollToGroup(g.group)">
          {{ locale === 'fa' ? g.groupLabelFa : g.groupLabelEn }}
        </button>
      </nav>

      <div class="settings-content">
        <section v-for="g in groups" :key="g.group" :id="`settings-${g.group}`" class="card settings-group">
          <h2>{{ locale === 'fa' ? g.groupLabelFa : g.groupLabelEn }}</h2>
          <p v-if="g.group === 'share'" class="section-hint">
            {{ locale === 'fa'
              ? 'متغیرها: {titleFa} {titleEn} {descriptionFa} {descriptionEn} {pageContentFa} {pageContentEn} {collected} {target} {progress} {link} {pageUrl}'
              : 'Placeholders: {titleFa} {titleEn} {descriptionFa} {descriptionEn} {pageContentFa} {pageContentEn} {collected} {target} {progress} {link} {pageUrl}' }}
          </p>
          <p v-if="g.group === 'donation'" class="section-hint">
            {{ locale === 'fa'
              ? 'مبالغ پیشنهادی را با کاما جدا کنید، مثلاً: 50000,100000,200000'
              : 'Quick amounts comma-separated, e.g. 50000,100000,200000' }}
          </p>
          <div v-for="item in g.items" :key="item.key" class="field">
            <label class="label">{{ label(item) }}</label>
            <textarea v-if="item.type === 'TextArea'" v-model="values[item.key]" class="textarea" rows="6" />
            <select v-else-if="item.type === 'Boolean'" v-model="values[item.key]" class="select">
              <option value="true">{{ locale === 'fa' ? 'فعال' : 'true' }}</option>
              <option value="false">{{ locale === 'fa' ? 'غیرفعال' : 'false' }}</option>
            </select>
            <input v-else :type="item.type === 'Password' ? 'password' : item.type === 'Number' ? 'number' : 'text'"
              v-model="values[item.key]" class="input" />
          </div>
        </section>
      </div>
    </div>

    <div v-if="showTour && tourSteps.length" class="tour-overlay">
      <div class="tour-card card">
        <p class="tour-step">{{ tourStep + 1 }} / {{ tourSteps.length }}</p>
        <h3>{{ tourSteps[tourStep].title }}</h3>
        <p>{{ tourSteps[tourStep].hint }}</p>
        <div class="tour-actions">
          <button type="button" class="btn btn-ghost" @click="finishTour">{{ locale === 'fa' ? 'رد کردن' : 'Skip' }}</button>
          <button type="button" class="btn btn-primary" @click="nextTour">
            {{ tourStep < tourSteps.length - 1 ? (locale === 'fa' ? 'بعدی' : 'Next') : (locale === 'fa' ? 'شروع' : 'Done') }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.settings-page { padding-bottom: 2rem; }
.toolbar { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem; position: sticky; top: 0; z-index: 5; background: var(--bg, #0f172a); padding: 0.5rem 0; }
.toolbar-actions { display: flex; gap: 0.5rem; }
.settings-layout { display: grid; grid-template-columns: minmax(200px, 240px) 1fr; gap: 1rem; align-items: start; }
.settings-nav { position: sticky; top: 4rem; padding: 1rem; max-height: calc(100vh - 6rem); overflow-y: auto; }
.nav-title { font-size: 0.75rem; color: var(--muted); margin-bottom: 0.75rem; text-transform: uppercase; letter-spacing: 0.05em; }
.nav-item {
  display: block; width: 100%; text-align: start; padding: 0.55rem 0.75rem; margin-bottom: 0.25rem;
  border: none; border-radius: 8px; background: transparent; color: var(--muted); cursor: pointer; font-family: inherit; font-size: 0.88rem;
}
.nav-item.active, .nav-item:hover { background: color-mix(in srgb, var(--primary) 15%, transparent); color: var(--primary); }
.settings-group { margin-bottom: 1rem; scroll-margin-top: 5rem; }
.settings-group h2 { font-size: 1rem; margin-bottom: 1rem; color: var(--primary); }
.section-hint { font-size: 0.8rem; color: var(--muted); margin: -0.5rem 0 1rem; line-height: 1.6; }
.field { margin-bottom: 1rem; }
.saved { color: #34d399; margin-bottom: 0.5rem; }
.tour-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.55); z-index: 100; display: flex; align-items: flex-end; justify-content: center; padding: 1rem; }
.tour-card { max-width: 420px; width: 100%; padding: 1.25rem; }
.tour-step { color: var(--muted); font-size: 0.8rem; }
.tour-actions { display: flex; gap: 0.5rem; justify-content: flex-end; margin-top: 1rem; }
@media (max-width: 768px) {
  .settings-layout { grid-template-columns: 1fr; }
  .settings-nav { position: static; display: flex; flex-wrap: wrap; gap: 0.35rem; max-height: none; }
  .nav-item { width: auto; flex: 1 1 calc(50% - 0.35rem); font-size: 0.78rem; padding: 0.45rem; }
}
</style>
