<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { useI18n } from 'vue-i18n'
import AppHeader from '@/components/AppHeader.vue'

defineProps({ success: Boolean })
const route = useRoute()
const { t } = useI18n()
const message = computed(() => route.query.message || '')
</script>

<template>
  <AppHeader />
  <main class="container result">
    <div class="card" style="text-align:center; max-width:480px; margin:2rem auto;">
      <div class="result-icon" :class="success ? 'ok' : 'fail'" aria-hidden="true">
        <svg v-if="success" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M20 6 9 17l-5-5"/></svg>
        <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M18 6 6 18M6 6l12 12"/></svg>
      </div>
      <h1>{{ success ? t('paymentSuccess') : t('paymentFailed') }}</h1>
      <p v-if="message" class="msg">{{ message }}</p>
      <router-link to="/" class="btn btn-primary" style="margin-top:1.5rem">{{ t('backHome') }}</router-link>
    </div>
  </main>
</template>

<style scoped>
.result-icon {
  width: 84px; height: 84px; border-radius: 50%;
  display: flex; align-items: center; justify-content: center;
  margin: 0 auto 1.25rem;
  animation: pop 0.4s cubic-bezier(0.2, 0.9, 0.3, 1.4);
}
.result-icon svg { width: 42px; height: 42px; }
.ok { background: color-mix(in srgb, var(--success) 20%, transparent); color: var(--success); box-shadow: 0 0 0 8px color-mix(in srgb, var(--success) 8%, transparent); }
.fail { background: color-mix(in srgb, var(--danger) 20%, transparent); color: var(--danger); box-shadow: 0 0 0 8px color-mix(in srgb, var(--danger) 8%, transparent); }
@keyframes pop { from { transform: scale(0.5); opacity: 0; } to { transform: scale(1); opacity: 1; } }
.msg { color: var(--muted); margin-top: 0.75rem; }
</style>
