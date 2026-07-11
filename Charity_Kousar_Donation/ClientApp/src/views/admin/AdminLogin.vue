<script setup>
import { ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { api, setToken } from '@/api/client'

const { t } = useI18n()
const router = useRouter()
const route = useRoute()
const username = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)

async function login() {
  error.value = ''
  loading.value = true
  try {
    const res = await api('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ username: username.value, password: password.value })
    })
    setToken(res.token)
    router.push(route.query.redirect || '/admin')
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="login-page">
    <form class="card login-form" @submit.prevent="login">
      <div class="login-brand">
        <span class="brand-mark" aria-hidden="true">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 3l7 3v5c0 4.5-3 7.6-7 9-4-1.4-7-4.5-7-9V6l7-3z"/><path d="M9 12l2 2 4-4"/></svg>
        </span>
        <h1>{{ t('admin') }}</h1>
      </div>
      <div class="field">
        <label class="label" for="login-username">{{ t('username') }}</label>
        <input id="login-username" v-model="username" class="input input-ltr" dir="ltr" autocomplete="username" required />
      </div>
      <div class="field">
        <label class="label" for="login-password">{{ t('password') }}</label>
        <input id="login-password" v-model="password" type="password" class="input input-ltr" dir="ltr" autocomplete="current-password" required />
      </div>
      <p v-if="error" class="error" role="alert">{{ error }}</p>
      <button class="btn btn-primary" type="submit" :disabled="loading">
        {{ loading ? '…' : t('login') }}
      </button>
      <router-link to="/" class="back">{{ t('backHome') }}</router-link>
    </form>
  </div>
</template>

<style scoped>
.login-page { min-height: 100vh; display: flex; align-items: center; justify-content: center; padding: 1rem; }
.login-form { width: 100%; max-width: 400px; display: flex; flex-direction: column; gap: 0.85rem; padding: 2rem; }
.login-brand { display: flex; flex-direction: column; align-items: center; gap: 0.75rem; margin-bottom: 0.5rem; text-align: center; }
.login-brand h1 { font-size: 1.3rem; }
.brand-mark {
  width: 56px; height: 56px; border-radius: 16px;
  display: flex; align-items: center; justify-content: center; color: #fff;
  background: linear-gradient(135deg, var(--primary), var(--accent));
  box-shadow: 0 10px 24px -8px color-mix(in srgb, var(--primary) 55%, transparent);
}
.brand-mark svg { width: 28px; height: 28px; }
.field { display: flex; flex-direction: column; gap: 0.35rem; }
.field .label { margin-bottom: 0; }
.error { color: var(--danger); font-size: 0.9rem; }
.back { text-align: center; margin-top: 0.5rem; color: var(--muted); font-size: 0.9rem; }
</style>
