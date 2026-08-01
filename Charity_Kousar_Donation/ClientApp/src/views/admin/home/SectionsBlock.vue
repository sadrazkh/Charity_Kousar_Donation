<script setup>
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import draggable from 'vuedraggable'
import { SECTIONS, PRESETS, secIcon, secKey } from './sections'

// The order of the home page sections, as a draggable list of ids.
const order = defineModel({ type: Array, required: true })
const { t } = useI18n()

const hidden = computed(() => SECTIONS.filter(s => !order.value.includes(s.id)))
function hide(id) { order.value = order.value.filter(s => s !== id) }
function show(id) { order.value = [...order.value, id] }
function applyPreset(p) { order.value = [...p.order] }
</script>

<template>
  <section class="card block">
    <h2>{{ t('homeEditor.presetsTitle') }}</h2>
    <p class="hint">{{ t('homeEditor.presetsHint') }}</p>
    <div class="presets">
      <button v-for="p in PRESETS" :key="p.id" type="button" class="preset-chip" @click="applyPreset(p)">
        {{ t('homeEditor.' + p.label) }}
      </button>
    </div>
  </section>

  <section class="card block">
    <h2>{{ t('homeEditor.sectionsTitle') }}</h2>
    <p class="hint">{{ t('homeEditor.sectionsHint') }}</p>
    <draggable v-model="order" :item-key="el => el" handle=".sec-drag" class="sec-list"
      ghost-class="sec-ghost" animation="200" :delay="60" :delay-on-touch-only="true">
      <template #item="{ element: id }">
        <div class="sec-row">
          <span class="sec-drag" aria-label="Drag" title="Drag">
            <svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><circle cx="9" cy="6" r="1.5"/><circle cx="9" cy="12" r="1.5"/><circle cx="9" cy="18" r="1.5"/><circle cx="15" cy="6" r="1.5"/><circle cx="15" cy="12" r="1.5"/><circle cx="15" cy="18" r="1.5"/></svg>
          </span>
          <svg class="sec-ic" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path :d="secIcon(id)"/></svg>
          <span class="sec-name">{{ t(secKey(id)) }}</span>
          <button type="button" class="mini danger" :aria-label="t('homeEditor.hide')" :title="t('homeEditor.hide')" @click="hide(id)">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M9.9 4.2A10 10 0 0 1 12 4c6.5 0 10 8 10 8a15 15 0 0 1-2.9 3.7M6.6 6.6A15 15 0 0 0 2 12s3.5 7 10 7a10 10 0 0 0 4.4-1M3 3l18 18"/></svg>
          </button>
        </div>
      </template>
    </draggable>
    <div v-if="hidden.length" class="hidden-row">
      <span class="muted">{{ t('homeEditor.hiddenLabel') }}</span>
      <button v-for="s in hidden" :key="s.id" type="button" class="chip" @click="show(s.id)">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M12 5v14M5 12h14"/></svg>
        <svg class="chip-ic" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path :d="secIcon(s.id)"/></svg>
        {{ t(secKey(s.id)) }}
      </button>
    </div>
  </section>
</template>

<style scoped>
.presets { display: flex; flex-wrap: wrap; gap: 0.5rem; }
.preset-chip {
  padding: 0.45rem 0.9rem; border-radius: 999px; border: 1px solid var(--border);
  background: var(--input-bg); color: var(--text); cursor: pointer; font-family: inherit; font-size: 0.85rem;
}
.preset-chip:hover { border-color: color-mix(in srgb, var(--primary) 50%, transparent); color: var(--primary); }

.sec-list { display: flex; flex-direction: column; gap: 0.5rem; min-height: 40px; }
.sec-row {
  display: flex; align-items: center; gap: 0.6rem; padding: 0.6rem 0.75rem;
  border: 1px solid var(--border); border-radius: 10px; background: var(--input-bg);
}
.sec-drag { cursor: grab; color: var(--muted); touch-action: none; display: inline-flex; }
.sec-drag svg { width: 20px; height: 20px; }
.sec-ghost { opacity: 0.4; }
.sec-ic { width: 20px; height: 20px; color: var(--primary); flex-shrink: 0; }
.sec-name { flex: 1; font-size: 0.92rem; }
.hidden-row { display: flex; flex-wrap: wrap; gap: 0.4rem; align-items: center; margin-top: 0.75rem; }
.hidden-row .muted { color: var(--muted); font-size: 0.85rem; }
.chip {
  display: inline-flex; align-items: center; gap: 0.3rem; padding: 0.35rem 0.7rem;
  border-radius: 999px; border: 1px dashed var(--border); background: transparent;
  color: var(--text); cursor: pointer; font-family: inherit; font-size: 0.82rem;
}
.chip svg { width: 14px; height: 14px; }
.chip .chip-ic { color: var(--primary); }
</style>
