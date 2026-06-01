<script setup>
import { ref, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { api } from '@/api/client'

const { t, locale } = useI18n()
const groups = ref([])
const values = ref({})
const saved = ref(false)

onMounted(async () => {
  groups.value = await api('/settings')
  for (const g of groups.value) {
    for (const item of g.items) {
      values.value[item.key] = item.value
    }
  }
})

function label(item) {
  return locale.value === 'fa' ? item.labelFa : item.labelEn
}

async function save() {
  await api('/settings', { method: 'PUT', body: JSON.stringify({ settings: values.value }) })
  saved.value = true
  setTimeout(() => { saved.value = false }, 3000)
}
</script>

<template>
  <div>
    <div class="toolbar">
      <h1>{{ t('settings') }}</h1>
      <button class="btn btn-primary" @click="save">{{ t('save') }}</button>
    </div>
    <p v-if="saved" class="saved">✓ ذخیره شد</p>

    <section v-for="g in groups" :key="g.group" class="card settings-group">
      <h2>{{ locale === 'fa' ? g.groupLabelFa : g.groupLabelEn }}</h2>
      <div v-for="item in g.items" :key="item.key" class="field">
        <label class="label">{{ label(item) }}</label>
        <textarea v-if="item.type === 'TextArea'" v-model="values[item.key]" class="textarea" />
        <select v-else-if="item.type === 'Boolean'" v-model="values[item.key]" class="select">
          <option value="true">true</option>
          <option value="false">false</option>
        </select>
        <input v-else :type="item.type === 'Password' ? 'password' : item.type === 'Number' ? 'number' : 'text'"
          v-model="values[item.key]" class="input" />
      </div>
    </section>
  </div>
</template>

<style scoped>
.toolbar { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem; }
.settings-group { margin-top: 1rem; }
.settings-group h2 { font-size: 1rem; margin-bottom: 1rem; color: var(--primary); }
.field { margin-bottom: 1rem; }
.saved { color: #34d399; margin-bottom: 0.5rem; }
</style>
