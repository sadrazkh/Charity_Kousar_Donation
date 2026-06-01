<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { api } from '@/api/client'

const { t, locale } = useI18n()
const router = useRouter()
const campaigns = ref([])

async function load() {
  campaigns.value = await api('/campaigns/admin/all')
}

onMounted(load)

async function remove(id) {
  if (!confirm(locale.value === 'fa' ? 'حذف شود؟' : 'Delete?')) return
  await api(`/campaigns/${id}`, { method: 'DELETE' })
  await load()
}

async function regenLink(id) {
  const res = await api(`/campaigns/${id}/regenerate-short-link`, { method: 'POST' })
  alert(res.shortUrl)
  await load()
}
</script>

<template>
  <div>
    <div class="toolbar">
      <h1>{{ t('manageCampaigns') }}</h1>
      <router-link to="/admin/campaigns/new" class="btn btn-primary btn-sm">{{ t('add') }}</router-link>
    </div>

    <p class="hint">{{ locale === 'fa' ? 'هر کمپین یک صفحه اختصاصی با ویرایشگر drag & drop دارد.' : 'Each campaign has a dedicated page with drag & drop editor.' }}</p>

    <div class="campaign-grid">
      <article v-for="c in campaigns" :key="c.id" class="card campaign-row">
        <div class="info">
          <h3>{{ c.titleFa }}</h3>
          <p class="meta">{{ c.titleEn }}</p>
          <div class="links">
            <a :href="`/c/${c.slug}`" target="_blank">{{ locale === 'fa' ? 'صفحه' : 'Page' }}</a>
            <span>·</span>
            <a :href="c.shortUrl" target="_blank">{{ c.shortCode }}</a>
          </div>
        </div>
        <div class="actions">
          <router-link :to="`/admin/campaigns/${c.id}/edit`" class="btn btn-primary btn-sm">
            {{ locale === 'fa' ? 'ویرایش صفحه' : 'Edit page' }}
          </router-link>
          <button class="btn btn-ghost btn-sm" @click="regenLink(c.id)">{{ t('regenerateLink') }}</button>
          <button class="btn btn-danger btn-sm" @click="remove(c.id)">{{ t('delete') }}</button>
        </div>
      </article>
    </div>

    <p v-if="!campaigns.length" class="empty">{{ t('noCampaigns') }}</p>
  </div>
</template>

<style scoped>
.toolbar { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.5rem; }
.hint { color: var(--muted); font-size: 0.9rem; margin-bottom: 1.25rem; }
.campaign-grid { display: flex; flex-direction: column; gap: 0.75rem; }
.campaign-row { display: flex; justify-content: space-between; align-items: center; gap: 1rem; flex-wrap: wrap; padding: 1.25rem; }
.info h3 { font-size: 1.05rem; margin-bottom: 0.2rem; }
.meta { color: var(--muted); font-size: 0.85rem; }
.links { font-size: 0.85rem; margin-top: 0.35rem; display: flex; gap: 0.5rem; align-items: center; }
.actions { display: flex; gap: 0.35rem; flex-wrap: wrap; }
.empty { color: var(--muted); text-align: center; padding: 3rem; }
</style>
