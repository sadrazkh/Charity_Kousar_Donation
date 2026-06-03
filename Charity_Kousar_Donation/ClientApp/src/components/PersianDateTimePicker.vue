<script setup>
import { ref, computed, watch } from 'vue'
import {
  JALALI_MONTHS, WEEKDAYS_FA, daysInJalaliMonth, jalaliMonthStartWeekday,
  isoToJalaliParts, jalaliPartsToIso, formatJalaliDisplay, currentJalaliParts,
  toPersianDigits
} from '@/utils/jalali'

const props = defineProps({
  modelValue: { type: String, default: null }
})
const emit = defineEmits(['update:modelValue'])

const open = ref(false)
const viewJy = ref(1404)
const viewJm = ref(1)
const selJy = ref(1404)
const selJm = ref(1)
const selJd = ref(1)
const hh = ref(12)
const mm = ref(0)

function syncFromModel() {
  const p = isoToJalaliParts(props.modelValue) || currentJalaliParts()
  viewJy.value = selJy.value = p.jy
  viewJm.value = selJm.value = p.jm
  selJd.value = p.jd
  hh.value = p.hh
  mm.value = p.mm
}

watch(() => props.modelValue, syncFromModel, { immediate: true })

const displayText = computed(() =>
  props.modelValue ? formatJalaliDisplay(props.modelValue) : '')

const calendarCells = computed(() => {
  const start = jalaliMonthStartWeekday(viewJy.value, viewJm.value)
  const days = daysInJalaliMonth(viewJy.value, viewJm.value)
  const cells = []
  for (let i = 0; i < start; i++) cells.push({ empty: true })
  for (let d = 1; d <= days; d++) {
    cells.push({
      day: d,
      selected: viewJy.value === selJy.value && viewJm.value === selJm.value && d === selJd.value,
      today: isToday(d)
    })
  }
  return cells
})

function isToday(d) {
  const t = currentJalaliParts()
  return t.jy === viewJy.value && t.jm === viewJm.value && t.jd === d
}

function prevMonth() {
  if (viewJm.value === 1) { viewJm.value = 12; viewJy.value-- }
  else viewJm.value--
}

function nextMonth() {
  if (viewJm.value === 12) { viewJm.value = 1; viewJy.value++ }
  else viewJm.value++
}

function pickDay(d) {
  selJy.value = viewJy.value
  selJm.value = viewJm.value
  selJd.value = d
}

function apply() {
  const iso = jalaliPartsToIso(selJy.value, selJm.value, selJd.value, hh.value, mm.value)
  emit('update:modelValue', iso)
  open.value = false
}

function clear() {
  emit('update:modelValue', null)
  open.value = false
}

function setNow() {
  const t = currentJalaliParts()
  viewJy.value = selJy.value = t.jy
  viewJm.value = selJm.value = t.jm
  selJd.value = t.jd
  hh.value = t.hh
  mm.value = t.mm
}

const hourOptions = Array.from({ length: 24 }, (_, i) => i)
const minuteOptions = Array.from({ length: 60 }, (_, i) => i)

function fmt(n) {
  return toPersianDigits(String(n).padStart(2, '0'))
}
</script>

<template>
  <div class="jdp">
    <button type="button" class="jdp-trigger" @click="open = true">
      <span v-if="displayText" class="jdp-value">{{ displayText }}</span>
      <span v-else class="jdp-placeholder">انتخاب تاریخ و ساعت (شمسی)</span>
      <span class="jdp-icon">📅</span>
    </button>

    <Teleport to="body">
      <div v-if="open" class="jdp-backdrop" @click.self="open = false">
        <div class="jdp-panel card" dir="rtl" @click.stop>
          <div class="jdp-head">
            <button type="button" class="nav-btn" @click="prevMonth">‹</button>
            <span class="month-label">{{ JALALI_MONTHS[viewJm - 1] }} {{ toPersianDigits(viewJy) }}</span>
            <button type="button" class="nav-btn" @click="nextMonth">›</button>
            <button type="button" class="close-btn" @click="open = false">✕</button>
          </div>

          <div class="weekdays">
            <span v-for="w in WEEKDAYS_FA" :key="w">{{ w }}</span>
          </div>

          <div class="days-grid">
            <template v-for="(cell, i) in calendarCells" :key="i">
              <span v-if="cell.empty" class="day empty" />
              <button
                v-else
                type="button"
                class="day"
                :class="{ selected: cell.selected, today: cell.today }"
                @click="pickDay(cell.day)"
              >
                {{ toPersianDigits(cell.day) }}
              </button>
            </template>
          </div>

          <div class="time-row">
            <label>
              <span>ساعت</span>
              <select v-model.number="hh" class="input">
                <option v-for="h in hourOptions" :key="h" :value="h">{{ fmt(h) }}</option>
              </select>
            </label>
            <label>
              <span>دقیقه</span>
              <select v-model.number="mm" class="input">
                <option v-for="m in minuteOptions" :key="m" :value="m">{{ fmt(m) }}</option>
              </select>
            </label>
          </div>

          <div class="jdp-actions">
            <button type="button" class="btn btn-ghost btn-sm" @click="setNow">همین الان</button>
            <button type="button" class="btn btn-ghost btn-sm" @click="clear">پاک</button>
            <button type="button" class="btn btn-primary btn-sm" @click="apply">تأیید</button>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<style scoped>
.jdp { width: 100%; }

.jdp-trigger {
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
  padding: 0.65rem 0.85rem;
  min-height: 44px;
  border-radius: 10px;
  border: 1px solid var(--border);
  background: var(--input-bg);
  color: var(--text);
  font-family: inherit;
  font-size: 0.95rem;
  cursor: pointer;
  text-align: right;
  direction: rtl;
}

.jdp-placeholder { color: var(--muted); }
.jdp-value { font-variant-numeric: tabular-nums; }

.jdp-backdrop {
  position: fixed;
  inset: 0;
  z-index: 2000;
  background: var(--overlay);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
}

.jdp-panel {
  width: min(340px, 100%);
  max-height: 90vh;
  overflow-y: auto;
  padding: 1rem;
  color: var(--text);
}

.jdp-head {
  display: flex;
  align-items: center;
  gap: 0.35rem;
  margin-bottom: 0.75rem;
}

.month-label {
  flex: 1;
  text-align: center;
  font-weight: 700;
  font-size: 0.95rem;
}

.nav-btn, .close-btn {
  width: 36px;
  height: 36px;
  border: 1px solid var(--border);
  border-radius: 8px;
  background: var(--input-bg);
  color: var(--text);
  cursor: pointer;
  font-size: 1.1rem;
  display: flex;
  align-items: center;
  justify-content: center;
}

.weekdays {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 2px;
  margin-bottom: 4px;
  text-align: center;
  font-size: 0.75rem;
  color: var(--muted);
}

.days-grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 4px;
  margin-bottom: 0.85rem;
}

.day {
  aspect-ratio: 1;
  border: none;
  border-radius: 8px;
  background: transparent;
  color: var(--text);
  font-family: inherit;
  font-size: 0.85rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
}

.day.empty { pointer-events: none; }
.day.today { border: 1px solid var(--accent); }
.day.selected {
  background: var(--primary);
  color: #fff;
  font-weight: 700;
}

.time-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.65rem;
  margin-bottom: 0.85rem;
}

.time-row label {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  font-size: 0.8rem;
  color: var(--muted);
}

.time-row select { direction: rtl; text-align: right; }

.jdp-actions {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
  justify-content: flex-end;
}

@media (max-width: 480px) {
  .jdp-backdrop { align-items: flex-end; padding: 0; }
  .jdp-panel {
    width: 100%;
    max-height: 85vh;
    border-radius: 16px 16px 0 0;
  }
}
</style>
