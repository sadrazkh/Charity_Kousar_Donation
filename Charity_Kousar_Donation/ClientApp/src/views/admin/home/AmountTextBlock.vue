<script setup>
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import ProgressAmount from '@/components/ProgressAmount.vue'

const props = defineProps({ values: { type: Object, required: true } })
const { t, locale } = useI18n()

// Each piece of the amount sentence can carry its own color.
const PARTS = [
  { key: 'donation.progress.color.collected', token: '{collected}', label: 'partCollected' },
  { key: 'donation.progress.color.target', token: '{target}', label: 'partTarget' },
  { key: 'donation.progress.color.remaining', token: '{remaining}', label: 'partRemaining' },
  { key: 'donation.progress.color.percent', token: '{percent}', label: 'partPercent' },
  { key: 'donation.progress.color.text', token: 'abc', label: 'partPlain' }
]

const colors = computed(() => ({
  collected: props.values['donation.progress.color.collected'],
  target: props.values['donation.progress.color.target'],
  remaining: props.values['donation.progress.color.remaining'],
  percent: props.values['donation.progress.color.percent'],
  text: props.values['donation.progress.color.text']
}))
const scale = computed(() => Number(props.values['donation.progress.size']) || 100)
const format = computed(() => locale.value === 'fa'
  ? props.values['donation.progress.format.fa']
  : props.values['donation.progress.format.en'])
</script>

<template>
  <section class="card block">
    <h2>{{ t('homeEditor.amountTitle') }}</h2>
    <label class="label">{{ t('homeEditor.faFormat') }}</label>
    <input v-model="values['donation.progress.format.fa']" class="input input-rtl" />
    <label class="label">{{ t('homeEditor.enFormat') }}</label>
    <input v-model="values['donation.progress.format.en']" class="input input-ltr" dir="ltr" />
    <label class="label">{{ t('homeEditor.highlightColor') }}</label>
    <div class="colors"><input type="color" v-model="values['donation.progress.highlight']" class="swatch" /></div>

    <label class="label">{{ t('homeEditor.partColors') }}</label>
    <div class="part-colors">
      <div v-for="p in PARTS" :key="p.key" class="part">
        <span class="part-name">{{ t('homeEditor.' + p.label) }}</span>
        <code class="part-token">{{ p.token }}</code>
        <div class="swatch-row">
          <input type="color" :value="values[p.key] || '#0d9488'" class="swatch sm"
            @input="values[p.key] = $event.target.value" />
          <button type="button" class="reset-btn" :disabled="!values[p.key]" @click="values[p.key] = ''">
            {{ t('homeEditor.default') }}
          </button>
        </div>
      </div>
    </div>
    <p class="hint">{{ t('homeEditor.partsHint') }}</p>

    <label class="label">{{ t('homeEditor.amountSize') }}</label>
    <div class="speed-row">
      <input type="range" min="80" max="180" step="5" :value="scale"
        @input="values['donation.progress.size'] = $event.target.value" />
      <span class="speed-val">{{ scale }}%</span>
    </div>

    <div class="help">
      <strong>{{ t('homeEditor.guide') }}</strong>
      <ul>
        <li><code>{collected}</code> — {{ t('homeEditor.guideCollected') }}</li>
        <li><code>{target}</code> — {{ t('homeEditor.guideTarget') }}</li>
        <li><code>{remaining}</code> — {{ t('homeEditor.guideRemaining') }}</li>
        <li><code>{percent}</code> — {{ t('homeEditor.guidePercent') }}</li>
        <li><code>*…*</code> — {{ t('homeEditor.guideBold') }}</li>
        <li><code>~…~</code> — {{ t('homeEditor.guideColored') }}</li>
      </ul>
      <p class="ex">{{ t('homeEditor.example') }}</p>
      <p class="ex live">{{ t('homeEditor.previewLabel') }}
        <ProgressAmount :collected="6500000" :target="10000000" :format="format"
          :highlight="values['donation.progress.highlight']" :colors="colors" :scale="scale" />
      </p>
    </div>
  </section>
</template>

<style scoped>
.part-colors { display: flex; flex-wrap: wrap; gap: 0.75rem; }
.part {
  display: flex; flex-direction: column; gap: 0.3rem;
  padding: 0.5rem 0.6rem; border: 1px solid var(--border); border-radius: 10px; background: var(--input-bg);
}
.part-name { font-size: 0.85rem; }
.part-token { font-size: 0.72rem; color: var(--muted); direction: ltr; }
.help {
  margin-top: 0.85rem; padding: 0.85rem 1rem; border-radius: 10px;
  background: color-mix(in srgb, var(--primary) 7%, transparent);
  border: 1px solid var(--border); font-size: 0.85rem;
}
.help ul {
  margin: 0.5rem 0; padding-inline-start: 1.1rem;
  display: flex; flex-direction: column; gap: 0.25rem; color: var(--muted);
}
.help code {
  background: color-mix(in srgb, var(--muted) 18%, transparent);
  padding: 0.05rem 0.35rem; border-radius: 5px; direction: ltr; display: inline-block;
  font-family: ui-monospace, 'Cascadia Mono', Menlo, Consolas, monospace;
}
.help .ex { margin-top: 0.5rem; color: var(--text); }
.help .live { margin-top: 0.6rem; font-weight: 600; }
</style>
