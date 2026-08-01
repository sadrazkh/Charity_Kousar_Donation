<script setup>
import { useI18n } from 'vue-i18n'

// Highlight styles (color + badge label) a campaign can pick from.
const styles = defineModel({ type: Array, required: true })
const { t, locale } = useI18n()

function addStyle() {
  styles.value.push({
    id: 'style-' + Math.random().toString(36).slice(2, 7),
    color: '#0d9488', labelFa: '', labelEn: ''
  })
}
function removeStyle(i) { styles.value.splice(i, 1) }
const label = (s) => (locale.value === 'fa' ? s.labelFa : s.labelEn) || t('homeEditor.styleUntitled')
</script>

<template>
  <section class="card block">
    <h2>{{ t('homeEditor.stylesTitle') }}</h2>
    <p class="hint">{{ t('homeEditor.stylesHint') }}</p>
    <div class="style-list">
      <div v-for="(s, i) in styles" :key="s.id" class="style-row" :style="{ '--chip': s.color }">
        <span class="style-preview">{{ label(s) }}</span>
        <input type="color" v-model="s.color" class="swatch sm" :aria-label="t('homeEditor.color')" />
        <input v-model="s.labelFa" class="input input-sm" :placeholder="t('homeEditor.persianLabel')" />
        <input v-model="s.labelEn" class="input input-sm input-ltr" dir="ltr" :placeholder="t('homeEditor.englishLabel')" />
        <button type="button" class="mini danger" :aria-label="t('delete')" :title="t('delete')" @click="removeStyle(i)">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M3 6h18M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2M6 6l1 14a2 2 0 0 0 2 2h6a2 2 0 0 0 2-2l1-14"/></svg>
        </button>
      </div>
    </div>
    <button type="button" class="btn btn-ghost btn-sm add-style" @click="addStyle">
      + {{ t('homeEditor.addStyle') }}
    </button>
  </section>
</template>

<style scoped>
.style-list { display: flex; flex-direction: column; gap: 0.5rem; }
.style-row {
  display: grid; grid-template-columns: minmax(90px, 130px) auto 1fr 1fr auto;
  align-items: center; gap: 0.5rem;
  padding: 0.5rem; border-radius: 10px;
  border: 1px solid color-mix(in srgb, var(--chip) 40%, var(--border));
  background: color-mix(in srgb, var(--chip) 8%, transparent);
}
.style-preview {
  display: inline-block; padding: 0.2rem 0.6rem; border-radius: 999px;
  font-size: 0.78rem; font-weight: 700; text-align: center;
  color: var(--chip); background: color-mix(in srgb, var(--chip) 16%, transparent);
  overflow: hidden; text-overflow: ellipsis; white-space: nowrap;
}
.input-sm { padding: 0.4rem 0.55rem; font-size: 0.82rem; }
.add-style { margin-top: 0.6rem; }
@media (max-width: 700px) {
  .style-row { grid-template-columns: 1fr auto; grid-auto-rows: auto; }
  .style-row .input-sm { grid-column: 1 / -1; }
}
</style>
