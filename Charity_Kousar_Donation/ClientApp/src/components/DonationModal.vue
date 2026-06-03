<script setup>
import { ref, computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { api } from '@/api/client'
import AmountInput from '@/components/AmountInput.vue'
import { formatAmount } from '@/utils/amount'

const props = defineProps({
  campaign: Object,
  config: { type: Object, default: () => ({}) }
})
const emit = defineEmits(['close'])
const { t, locale } = useI18n()

const phone = ref('')
const amount = ref(0)
const name = ref('')
const loading = ref(false)
const error = ref('')
const cryptoStep = ref(null)
const otpStep = ref(null)
const otpCode = ref('')

const quickAmounts = computed(() => {
  const fromConfig = props.config?.quickDonationAmounts
  if (Array.isArray(fromConfig) && fromConfig.length) return fromConfig
  return [50_000, 100_000, 200_000, 500_000, 1_000_000]
})

const minAmount = computed(() => props.config?.minDonationAmount || 10000)

const title = computed(() =>
  locale.value === 'fa' ? props.campaign?.titleFa : props.campaign?.titleEn)

function pickAmount(n) {
  amount.value = n
}

async function submit() {
  error.value = ''
  if (!amount.value || amount.value < minAmount.value) {
    error.value = locale.value === 'fa'
      ? `حداقل مبلغ ${formatAmount(minAmount.value, locale.value)} تومان`
      : `Minimum ${formatAmount(minAmount.value, locale.value)} Toman`
    return
  }
  loading.value = true
  try {
    const res = await api('/donations/start', {
      method: 'POST',
      body: JSON.stringify({
        campaignId: props.campaign.id,
        phone: phone.value,
        amount: amount.value,
        donorName: name.value || null,
        paymentMethod: (props.config.cryptoEnabled && props.config.zarinPalEnabled === false) ? 1 : 0,
        isRecurring: false
      })
    })
    if (res.requiresOtp) {
      otpStep.value = res
      return
    }
    if (res.paymentUrl) {
      window.location.href = res.paymentUrl
    } else {
      cryptoStep.value = res
    }
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}

async function verifyOtp() {
  error.value = ''
  loading.value = true
  try {
    const res = await api('/donations/verify-otp', {
      method: 'POST',
      body: JSON.stringify({ donationId: otpStep.value.donationId, otpCode: otpCode.value })
    })
    if (res.paymentUrl) window.location.href = res.paymentUrl
    else cryptoStep.value = res
    otpStep.value = null
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}

async function resendOtp() {
  try {
    await api('/donations/resend-otp', {
      method: 'POST',
      body: JSON.stringify({ donationId: otpStep.value.donationId })
    })
  } catch (e) {
    error.value = e.message
  }
}

async function confirmCrypto() {
  const tx = document.getElementById('txHash')?.value
  if (!tx) return
  loading.value = true
  try {
    await api('/donations/crypto/confirm', {
      method: 'POST',
      body: JSON.stringify({ donationId: cryptoStep.value.donationId, txHash: tx })
    })
    emit('close')
    window.location.href = '/payment/success'
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="modal-overlay" @click.self="emit('close')">
    <div class="card modal">
      <h2>{{ t('pay') }} — {{ title }}</h2>

      <template v-if="otpStep">
        <p class="hint">{{ otpStep.message }}</p>
        <label class="label">{{ locale === 'fa' ? 'کد تأیید پیامک' : 'SMS verification code' }}</label>
        <input v-model="otpCode" class="input input-ltr" dir="ltr" inputmode="numeric" maxlength="6" />
        <p v-if="error" class="error">{{ error }}</p>
        <div class="actions">
          <button type="button" class="btn btn-ghost btn-sm" @click="resendOtp">{{ locale === 'fa' ? 'ارسال مجدد' : 'Resend' }}</button>
          <button class="btn btn-primary" :disabled="loading" @click="verifyOtp">{{ loading ? '...' : (locale === 'fa' ? 'تأیید و پرداخت' : 'Verify & pay') }}</button>
        </div>
      </template>

      <template v-else-if="!cryptoStep">
        <div class="form">
          <label class="label">{{ t('name') }}</label>
          <input v-model="name" class="input" type="text" :placeholder="locale === 'fa' ? 'اختیاری' : 'Optional'" />

          <label class="label">{{ t('phone') }}</label>
          <input v-model="phone" class="input input-ltr" dir="ltr" type="tel" inputmode="tel"
            :placeholder="locale === 'fa' ? 'اختیاری — 09123456789' : 'Optional'" />

          <label class="label">{{ t('amount') }}</label>
          <AmountInput v-model="amount" dir="ltr" placeholder="100,000" />

          <div class="quick-amounts">
            <button v-for="n in quickAmounts" :key="n" type="button" class="quick-btn"
              :class="{ active: amount === n }" @click="pickAmount(n)">
              {{ formatAmount(n, locale) }}
            </button>
          </div>

          <p v-if="config.paymentBypassEnabled" class="test-hint">
            🧪 {{ locale === 'fa' ? 'حالت تست پرداخت فعال است' : 'Payment test mode is on' }}
          </p>

          <p v-if="error" class="error">{{ error }}</p>
          <div class="actions">
            <button class="btn btn-ghost" @click="emit('close')">{{ t('cancel') }}</button>
            <button class="btn btn-primary" :disabled="loading" @click="submit">
              {{ loading ? '...' : t('pay') }}
            </button>
          </div>
        </div>
      </template>

      <template v-else>
        <p>{{ cryptoStep.message }}</p>
        <p class="label">{{ t('cryptoNetwork') }}: {{ cryptoStep.cryptoNetwork }}</p>
        <code class="wallet">{{ cryptoStep.cryptoAddress }}</code>
        <label class="label">{{ t('txHash') }}</label>
        <input id="txHash" class="input input-ltr" dir="ltr" />
        <p v-if="error" class="error">{{ error }}</p>
        <button class="btn btn-primary" :disabled="loading" @click="confirmCrypto">{{ t('confirmCrypto') }}</button>
      </template>
    </div>
  </div>
</template>

<style scoped>
.form { display: flex; flex-direction: column; gap: 0.75rem; margin-top: 1rem; }
.quick-amounts { display: flex; flex-wrap: wrap; gap: 0.35rem; }
.quick-btn {
  padding: 0.4rem 0.65rem; border-radius: 999px; border: 1px solid rgba(148,163,184,0.3);
  background: rgba(15,23,42,0.5); color: var(--muted); font-size: 0.8rem; cursor: pointer;
  font-family: inherit; min-height: 36px; touch-action: manipulation;
}
.quick-btn.active { border-color: var(--primary); color: var(--primary); background: color-mix(in srgb, var(--primary) 12%, transparent); }
.actions { display: flex; gap: 0.5rem; justify-content: flex-end; margin-top: 0.5rem; flex-wrap: wrap; }
.actions .btn { flex: 1; min-width: 120px; }
.error { color: #f87171; font-size: 0.9rem; }
.test-hint { font-size: 0.8rem; color: var(--accent); background: color-mix(in srgb, var(--accent) 10%, transparent); padding: 0.45rem 0.65rem; border-radius: 8px; margin: 0; }
.hint { color: var(--muted); margin: 1rem 0; }
.wallet { display: block; word-break: break-all; padding: 0.75rem; background: rgba(0,0,0,0.3); border-radius: 8px; margin: 0.5rem 0; font-size: 0.85rem; }
h2 { font-size: 1.05rem; line-height: 1.5; }
</style>
