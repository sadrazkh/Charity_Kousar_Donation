<script setup>
import { useI18n } from 'vue-i18n'

// `values` is the shared settings bag; blocks write straight into their own keys.
defineProps({ values: { type: Object, required: true } })
const { t } = useI18n()
</script>

<template>
  <section class="card block">
    <h2>{{ t('homeEditor.cardsTitle') }}</h2>
    <div class="row-2">
      <div>
        <label class="label">{{ t('homeEditor.columns') }}</label>
        <select v-model="values['site.home.columns']" class="select">
          <option value="auto">{{ t('homeEditor.columnsAuto') }}</option>
          <option value="2">۲ / 2</option>
          <option value="3">۳ / 3</option>
          <option value="4">۴ / 4</option>
        </select>
      </div>
      <label class="chk merge">
        <input type="checkbox" :checked="values['site.home.merge.featured'] === 'true'"
          @change="values['site.home.merge.featured'] = $event.target.checked ? 'true' : 'false'" />
        {{ t('homeEditor.mergeFeatured') }}
      </label>
    </div>
    <p class="hint">{{ t('homeEditor.mergeHint') }}</p>

    <label class="label">{{ t('homeEditor.cardImage') }}</label>
    <select v-model="values['site.card.image.fit']" class="select">
      <option value="cover">{{ t('homeEditor.imageCover') }}</option>
      <option value="contain">{{ t('homeEditor.imageContain') }}</option>
    </select>
    <p class="hint">{{ t('homeEditor.imageHint') }}</p>
  </section>
</template>

<style scoped>
.chk.merge { white-space: nowrap; padding-bottom: 0.5rem; }
</style>
