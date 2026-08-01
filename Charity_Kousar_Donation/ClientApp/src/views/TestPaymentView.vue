<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { useI18n } from 'vue-i18n'
import AppHeader from '@/components/AppHeader.vue'

const route = useRoute()
const { t, locale } = useI18n()

const donationId = computed(() => route.query.donationId || '')

function complete(success) {
  const status = success ? 'OK' : 'NOK'
  window.location.href = `/api/payment/test/callback?donationId=${donationId.value}&Status=${status}`
}
</script>

<template>
  <AppHeader />
  <main class="container test-pay">
    <div class="card gateway">
      <div class="gateway-head">
        <div class="logo">🧪</div>
        <div>
          <h1>{{ t('ui.testPaymentGateway') }}</h1>
          <p>{{ t('ui.forTestingOnlyNotA') }}</p>
        </div>
      </div>

      <div class="info-box">
        <p>{{ t('ui.choosePaymentResult') }}</p>
        <code v-if="donationId" class="tx-id">ID: {{ donationId }}</code>
      </div>

      <div class="actions">
        <button type="button" class="btn btn-primary btn-lg" @click="complete(true)">
          ✓ {{ t('ui.paymentSuccess') }}
        </button>
        <button type="button" class="btn btn-ghost btn-lg fail-btn" @click="complete(false)">
          ✕ {{ t('ui.paymentFailedCancel') }}
        </button>
      </div>
    </div>
  </main>
</template>

<style scoped>
.test-pay { display: flex; justify-content: center; padding: 2rem 1rem; }
.gateway { max-width: 420px; width: 100%; padding: 1.75rem; text-align: center; }
.gateway-head { display: flex; flex-direction: column; align-items: center; gap: 0.75rem; margin-bottom: 1.5rem; }
.logo {
  width: 64px; height: 64px; border-radius: 16px;
  background: linear-gradient(135deg, #f59e0b33, #0d948833);
  display: flex; align-items: center; justify-content: center; font-size: 2rem;
}
.gateway-head h1 { font-size: 1.15rem; margin-bottom: 0.25rem; }
.gateway-head p { color: var(--muted); font-size: 0.85rem; margin: 0; }
.info-box {
  background: rgba(0,0,0,0.2);
  border-radius: 10px;
  padding: 1rem;
  margin-bottom: 1.25rem;
  font-size: 0.9rem;
  color: var(--muted);
}
.tx-id { display: block; margin-top: 0.5rem; font-size: 0.75rem; word-break: break-all; color: var(--primary); }
.actions { display: flex; flex-direction: column; gap: 0.65rem; }
.btn-lg { min-height: 48px; font-size: 1rem; width: 100%; }
.fail-btn { border-color: rgba(248,113,113,0.4); color: #f87171; }
</style>
