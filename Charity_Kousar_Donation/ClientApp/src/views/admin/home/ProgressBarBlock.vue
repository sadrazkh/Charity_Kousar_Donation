<script setup>
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import ProgressBar from '@/components/ProgressBar.vue'

const props = defineProps({ values: { type: Object, required: true } })
const { t } = useI18n()

const FLOW_STYLES = ['shimmer', 'stripes', 'glow', 'pulse']
const flowLabel = (id) => t('homeEditor.flow' + id[0].toUpperCase() + id.slice(1))

const v = computed(() => props.values)
const flowOff = computed(() => v.value['site.progress.flow'] === 'false')
const animOff = computed(() => v.value['site.progress.animate'] === 'false')

// What the preview bars below render with, straight from the unsaved form values.
const cfg = computed(() => ({
  progressMode: v.value['site.progress.mode'],
  progressColorStart: v.value['site.progress.color.start'],
  progressColorEnd: v.value['site.progress.color.end'],
  progressTrackColor: v.value['site.progress.track.color'],
  progressAnimate: v.value['site.progress.animate'] !== 'false',
  progressAnimateMs: Number(v.value['site.progress.animate.ms']) || 0,
  progressFlow: v.value['site.progress.flow'] !== 'false',
  progressFlowStyle: v.value['site.progress.flow.style'] || 'shimmer',
  progressFlowMs: Number(v.value['site.progress.flow.ms']) || 2400,
  showProgressPercent: v.value['site.progress.show.percent'] !== 'false'
}))

// Bumping the key remounts the preview bars so the fill animation replays.
const replayKey = ref(0)
const seconds = (ms, fallback) => ((Number(ms) || fallback) / 1000).toFixed(1) + 's'
</script>

<template>
  <section class="card block">
    <h2>{{ t('homeEditor.progressTitle') }}</h2>
    <label class="label">{{ t('homeEditor.colorMode') }}</label>
    <select v-model="values['site.progress.mode']" class="select">
      <option value="shift">{{ t('homeEditor.modeShift') }}</option>
      <option value="solid">{{ t('homeEditor.modeSolid') }}</option>
      <option value="gradient">{{ t('homeEditor.modeGradient') }}</option>
    </select>

    <div class="colors">
      <div>
        <label class="label">{{ t('homeEditor.startColor') }}</label>
        <input type="color" v-model="values['site.progress.color.start']" class="swatch" />
      </div>
      <div>
        <label class="label">{{ t('homeEditor.endColor') }}</label>
        <input type="color" v-model="values['site.progress.color.end']" class="swatch" />
      </div>
      <div>
        <label class="label">{{ t('homeEditor.trackColor') }}</label>
        <div class="swatch-row">
          <input type="color" :value="values['site.progress.track.color'] || '#94a3b8'" class="swatch"
            @input="values['site.progress.track.color'] = $event.target.value" />
          <button type="button" class="reset-btn" :disabled="!values['site.progress.track.color']"
            @click="values['site.progress.track.color'] = ''">{{ t('homeEditor.default') }}</button>
        </div>
      </div>
      <label class="chk">
        <input type="checkbox" :checked="values['site.progress.show.percent'] === 'true'"
          @change="values['site.progress.show.percent'] = $event.target.checked ? 'true' : 'false'" />
        {{ t('homeEditor.showPercent') }}
      </label>
    </div>

    <div class="anim-row">
      <label class="chk">
        <input type="checkbox" :checked="!animOff"
          @change="values['site.progress.animate'] = $event.target.checked ? 'true' : 'false'" />
        {{ t('homeEditor.animateFill') }}
      </label>
      <div class="speed" :class="{ off: animOff }">
        <label class="label">{{ t('homeEditor.fillSpeed') }}</label>
        <div class="speed-row">
          <input type="range" min="300" max="4000" step="100" :disabled="animOff"
            :value="Number(values['site.progress.animate.ms']) || 1400"
            @input="values['site.progress.animate.ms'] = $event.target.value" />
          <span class="speed-val">{{ seconds(values['site.progress.animate.ms'], 1400) }}</span>
        </div>
      </div>
    </div>
    <p class="hint">{{ t('homeEditor.animateHint') }}</p>

    <div class="anim-row">
      <label class="chk">
        <input type="checkbox" :checked="!flowOff"
          @change="values['site.progress.flow'] = $event.target.checked ? 'true' : 'false'" />
        {{ t('homeEditor.flowOn') }}
      </label>
      <div class="speed" :class="{ off: flowOff }">
        <label class="label">{{ t('homeEditor.flowSpeed') }}</label>
        <div class="speed-row">
          <input type="range" min="600" max="6000" step="100" :disabled="flowOff"
            :value="Number(values['site.progress.flow.ms']) || 2400"
            @input="values['site.progress.flow.ms'] = $event.target.value" />
          <span class="speed-val">{{ seconds(values['site.progress.flow.ms'], 2400) }}</span>
        </div>
      </div>
    </div>
    <div class="flow-styles" :class="{ off: flowOff }">
      <button v-for="s in FLOW_STYLES" :key="s" type="button" :disabled="flowOff"
        class="flow-opt" :class="{ on: (values['site.progress.flow.style'] || 'shimmer') === s }"
        @click="values['site.progress.flow.style'] = s">
        {{ flowLabel(s) }}
      </button>
    </div>
    <p class="hint">{{ t('homeEditor.flowHint') }}</p>

    <div class="prog-preview">
      <div class="preview-head">
        <span class="label">{{ t('homeEditor.livePreview') }}</span>
        <button type="button" class="reset-btn" @click="replayKey++">{{ t('homeEditor.replay') }}</button>
      </div>
      <ProgressBar v-for="p in [25, 60, 95]" :key="`${replayKey}-${p}`" :percent="p" :cfg="cfg" />
    </div>
  </section>
</template>

<style scoped>
.prog-preview { display: flex; flex-direction: column; gap: 0.5rem; margin-top: 0.75rem; }
.preview-head { display: flex; align-items: center; justify-content: space-between; gap: 0.5rem; }
.preview-head .label { margin: 0; }

.flow-styles { display: flex; flex-wrap: wrap; gap: 0.4rem; margin-bottom: 0.6rem; }
.flow-styles.off { opacity: 0.45; }
.flow-opt {
  padding: 0.4rem 0.9rem; border-radius: 999px; cursor: pointer;
  border: 1px solid var(--border); background: transparent;
  color: var(--muted); font-family: inherit; font-size: 0.85rem;
}
.flow-opt:disabled { cursor: not-allowed; }
.flow-opt.on {
  border-color: color-mix(in srgb, var(--primary) 45%, transparent);
  background: color-mix(in srgb, var(--primary) 16%, transparent);
  color: var(--primary); font-weight: 700;
}
</style>
