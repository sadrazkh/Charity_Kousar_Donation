<script setup>
import { useI18n } from 'vue-i18n'

defineProps({ values: { type: Object, required: true } })
const { t } = useI18n()
</script>

<template>
  <section class="card block">
    <h2>{{ t('homeEditor.donorsTitle') }}</h2>
    <label class="label">{{ t('homeEditor.listSource') }}</label>
    <select v-model="values['donors.source']" class="select">
      <option value="auto">{{ t('homeEditor.sourceAuto') }}</option>
      <option value="manual">{{ t('homeEditor.sourceManual') }}</option>
      <option value="both">{{ t('homeEditor.sourceBoth') }}</option>
    </select>
    <router-link to="/admin/settings" class="more-link">{{ t('homeEditor.donorsMore') }}</router-link>
  </section>
</template>
