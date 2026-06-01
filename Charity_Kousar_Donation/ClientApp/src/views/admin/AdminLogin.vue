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
      <h1>{{ t('admin') }}</h1>
      <label class="label">{{ t('username') }}</label>
      <input v-model="username" class="input input-ltr" dir="ltr" autocomplete="username" required />
      <label class="label">{{ t('password') }}</label>
      <input v-model="password" type="password" class="input input-ltr" dir="ltr" autocomplete="current-password" required />
      <p v-if="error" class="error">{{ error }}</p>
      <button class="btn btn-primary" type="submit" :disabled="loading">{{ t('login') }}</button>
      <router-link to="/" class="back">{{ t('backHome') }}</router-link>
    </form>
  </div>
</template>

<style scoped>
.login-page { min-height: 100vh; display: flex; align-items: center; justify-content: center; padding: 1rem; }
.login-form { width: 100%; max-width: 400px; display: flex; flex-direction: column; gap: 0.75rem; }
.error { color: #f87171; }
.back { text-align: center; margin-top: 0.5rem; color: var(--muted); font-size: 0.9rem; }
</style>
