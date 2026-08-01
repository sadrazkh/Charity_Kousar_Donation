<script setup>
import { useI18n } from 'vue-i18n'

defineProps({ values: { type: Object, required: true } })
const { t } = useI18n()
</script>

<template>
  <section class="card block">
    <h2>{{ t('homeEditor.completedTitle') }}</h2>
    <label class="chk">
      <input type="checkbox" :checked="values['site.completed.show'] !== 'false'"
        @change="values['site.completed.show'] = $event.target.checked ? 'true' : 'false'" />
      {{ t('homeEditor.completedShow') }}
    </label>
    <p class="hint">{{ t('homeEditor.completedHint') }}</p>
    <label class="label">{{ t('homeEditor.tabTitleFa') }}</label>
    <input v-model="values['site.completed.title.fa']" class="input" />
    <label class="label">{{ t('homeEditor.tabTitleEn') }}</label>
    <input v-model="values['site.completed.title.en']" class="input input-ltr" dir="ltr" />
  </section>
</template>
