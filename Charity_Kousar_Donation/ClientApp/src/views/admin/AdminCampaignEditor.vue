<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { api } from '@/api/client'
import { useToast } from '@/composables/useToast'
import PageBuilder from '@/components/PageBuilder.vue'
import ShareModal from '@/components/ShareModal.vue'
import AmountInput from '@/components/AmountInput.vue'
import ImageUpload from '@/components/ImageUpload.vue'
import PersianDateTimePicker from '@/components/PersianDateTimePicker.vue'

const route = useRoute()
const router = useRouter()
const { t, locale } = useI18n()
const toast = useToast()

const isNew = computed(() => route.name === 'admin-campaign-new')
const campaignId = computed(() => route.params.id)
const tab = ref('info')
const saving = ref(false)
const pageBlocks = ref([])

const form = ref({
  titleFa: '', titleEn: '', descriptionFa: '', descriptionEn: '',
  targetAmount: 0, imageUrl: '', slug: '', isActive: true, isFeatured: false,
  featuredBannerFa: '', featuredBannerEn: '', featuredTimerEndsAt: null,
  sortOrder: 0
})

const previewCampaign = computed(() => ({
  titleFa: form.value.titleFa,
  titleEn: form.value.titleEn,
  targetAmount: form.value.targetAmount,
  collectedAmount: 0,
  progressPercent: 0,
  donorCount: 0
}))

const showShare = ref(false)
const pageUrl = ref('')
const shortUrl = ref('')
const translating = ref('')

async function translate(fromField, toField) {
  const text = form.value[fromField]
  if (!text || !String(text).trim()) {
    toast.error(locale.value === 'fa' ? 'ابتدا متن فارسی را وارد کنید' : 'Enter Persian text first')
    return
  }
  translating.value = toField
  try {
    const res = await api('/ai/translate', {
      method: 'POST',
      body: JSON.stringify({ text, from: 'fa', to: 'en' })
    })
    form.value[toField] = res.translated
    toast.success(locale.value === 'fa' ? 'ترجمه شد ✓' : 'Translated ✓')
  } catch (e) {
    toast.error(e.message)
  } finally {
    translating.value = ''
  }
}

onMounted(async () => {
  if (!isNew.value) await loadCampaign()
})

async function loadCampaign() {
  const c = await api(`/campaigns/admin/${campaignId.value}`)
  form.value = {
    titleFa: c.titleFa, titleEn: c.titleEn,
    descriptionFa: c.descriptionFa, descriptionEn: c.descriptionEn,
    targetAmount: c.targetAmount, imageUrl: c.imageUrl || '', slug: c.slug,
    isActive: c.isActive, isFeatured: c.isFeatured,
    featuredBannerFa: c.featuredBannerFa || '', featuredBannerEn: c.featuredBannerEn || '',
    featuredTimerEndsAt: c.featuredTimerEndsAt || null,
    sortOrder: c.sortOrder
  }
  pageBlocks.value = c.pageBlocks || []
  pageUrl.value = c.pageUrl
  shortUrl.value = c.shortUrl
}

async function saveInfo() {
  saving.value = true
  try {
    if (isNew.value) {
      const created = await api('/campaigns', { method: 'POST', body: JSON.stringify(form.value) })
      router.replace({ name: 'admin-campaign-edit', params: { id: created.id } })
      await loadCampaign()
    } else {
      await api(`/campaigns/${campaignId.value}`, { method: 'PUT', body: JSON.stringify(form.value) })
    }
    toast.success(t('savedToast'))
  } catch (e) {
    toast.error(e.message)
  } finally {
    saving.value = false
  }
}

async function savePage() {
  if (isNew.value) {
    toast.error(locale.value === 'fa' ? 'اول اطلاعات پایه را ذخیره کنید' : 'Save basic info first')
    tab.value = 'info'
    return
  }
  saving.value = true
  try {
    await api(`/campaigns/${campaignId.value}/page`, {
      method: 'PUT',
      body: JSON.stringify({ blocks: pageBlocks.value })
    })
    toast.success(t('savedToast'))
  } catch (e) {
    toast.error(e.message)
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <div class="editor">
    <div class="editor-header">
      <div>
        <router-link to="/admin/campaigns" class="back">← {{ t('manageCampaigns') }}</router-link>
        <h1>{{ isNew ? t('add') : form.titleFa || t('edit') }}</h1>
      </div>
      <div class="header-links" v-if="pageUrl">
        <a :href="pageUrl" target="_blank" class="btn btn-ghost btn-sm">{{ locale === 'fa' ? 'مشاهده صفحه' : 'View page' }}</a>
        <button v-if="!isNew" type="button" class="btn btn-accent btn-sm" @click="showShare = true">
          {{ locale === 'fa' ? '📤 اشتراک AI' : '📤 AI Share' }}
        </button>
        <a :href="shortUrl" target="_blank" class="btn btn-ghost btn-sm">{{ t('shortLink') }}</a>
      </div>
    </div>

    <div class="tabs">
      <button type="button" :class="{ active: tab === 'info' }" @click="tab = 'info'">
        {{ locale === 'fa' ? 'اطلاعات پایه' : 'Basic info' }}
      </button>
      <button type="button" :class="{ active: tab === 'page' }" @click="tab = 'page'" :disabled="isNew">
        {{ locale === 'fa' ? 'صفحه اختصاصی' : 'Dedicated page' }}
      </button>
    </div>

    <!-- Basic info tab -->
    <div v-if="tab === 'info'" class="card form-card">
      <div class="grid grid-2">
        <div><label class="label">{{ t('titleFa') }}</label><input v-model="form.titleFa" class="input" /></div>
        <div>
          <div class="label-row">
            <label class="label">{{ t('titleEn') }}</label>
            <button type="button" class="translate-btn" :disabled="translating === 'titleEn'"
              @click="translate('titleFa', 'titleEn')">
              {{ translating === 'titleEn' ? '...' : (locale === 'fa' ? '🌐 ترجمه' : '🌐 Translate') }}
            </button>
          </div>
          <input v-model="form.titleEn" class="input input-ltr" dir="ltr" />
        </div>
      </div>
      <label class="label">{{ t('descFa') }}</label>
      <textarea v-model="form.descriptionFa" class="textarea" rows="3" />
      <div class="label-row">
        <label class="label">{{ t('descEn') }}</label>
        <button type="button" class="translate-btn" :disabled="translating === 'descriptionEn'"
          @click="translate('descriptionFa', 'descriptionEn')">
          {{ translating === 'descriptionEn' ? '...' : (locale === 'fa' ? '🌐 ترجمه از فارسی' : '🌐 Translate from FA') }}
        </button>
      </div>
      <textarea v-model="form.descriptionEn" class="textarea input-ltr" dir="ltr" rows="3" />
      <div class="grid grid-2">
        <div><label class="label">{{ t('targetAmount') }}</label><AmountInput v-model="form.targetAmount" dir="ltr" /></div>
        <div><label class="label">{{ t('imageUrl') }}</label><ImageUpload v-model="form.imageUrl" /></div>
      </div>
      <div class="grid grid-2">
        <div><label class="label">{{ t('slug') }}</label><input v-model="form.slug" class="input input-ltr" dir="ltr" /></div>
        <div><label class="label">{{ t('sortOrder') }}</label><input v-model.number="form.sortOrder" type="number" class="input" /></div>
      </div>
      <label><input type="checkbox" v-model="form.isActive" /> {{ t('active') }}</label>
      <label><input type="checkbox" v-model="form.isFeatured" /> {{ t('featured') }}</label>

      <div v-if="form.isFeatured" class="featured-config card">
        <h3>{{ locale === 'fa' ? '⭐ تنظیمات بخش ویژه' : '⭐ Featured section' }}</h3>
        <label class="label">{{ locale === 'fa' ? 'متن بنر (فارسی)' : 'Banner text (FA)' }}</label>
        <textarea v-model="form.featuredBannerFa" class="textarea" rows="2"
          :placeholder="locale === 'fa' ? 'مثلاً: امروز باید جمع شود!' : 'e.g. Must be collected today!'" />
        <label class="label">{{ locale === 'fa' ? 'متن بنر (انگلیسی)' : 'Banner text (EN)' }}</label>
        <textarea v-model="form.featuredBannerEn" class="textarea input-ltr" dir="ltr" rows="2" />
        <label class="label">{{ locale === 'fa' ? 'پایان شمارش معکوس (شمسی، اختیاری)' : 'Countdown end (Jalali, optional)' }}</label>
        <PersianDateTimePicker v-model="form.featuredTimerEndsAt" />
        <p class="hint">{{ locale === 'fa' ? 'تاریخ و ساعت به وقت تهران — اگر خالی باشد تایمر نمایش داده نمی‌شود' : 'Tehran time — leave empty to hide timer' }}</p>
      </div>

      <div class="actions">
        <button class="btn btn-primary" :disabled="saving" @click="saveInfo">{{ t('save') }}</button>
        <button v-if="!isNew" class="btn btn-ghost" @click="tab = 'page'">
          {{ locale === 'fa' ? 'رفتن به صفحه‌ساز ←' : 'Go to page builder →' }}
        </button>
      </div>
    </div>

    <!-- Page builder tab -->
    <PageBuilder
      v-if="tab === 'page' && !isNew"
      v-model="pageBlocks"
      :campaign="previewCampaign"
      @save="savePage"
    />
    <ShareModal v-if="form.slug && !isNew" :slug="form.slug" :show="showShare" @close="showShare = false" />
  </div>
</template>

<style scoped>
.editor-header { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; flex-wrap: wrap; margin-bottom: 1rem; }
.back { color: var(--muted); font-size: 0.9rem; text-decoration: none; display: block; margin-bottom: 0.35rem; }
.header-links { display: flex; gap: 0.5rem; flex-wrap: wrap; }
.tabs { display: flex; gap: 0.35rem; margin-bottom: 1.25rem; border-bottom: 1px solid rgba(148,163,184,0.15); padding-bottom: 0.5rem; }
.tabs button {
  padding: 0.5rem 1.25rem; border: none; background: transparent; color: var(--muted);
  cursor: pointer; font-family: inherit; font-size: 0.95rem; border-radius: 999px;
}
.tabs button.active { background: color-mix(in srgb, var(--primary) 20%, transparent); color: var(--primary); font-weight: 600; }
.tabs button:disabled { opacity: 0.4; cursor: not-allowed; }
.form-card { display: flex; flex-direction: column; gap: 0.75rem; }
.actions { display: flex; gap: 0.5rem; margin-top: 0.5rem; flex-wrap: wrap; }
.featured-config { padding: 1rem; margin-top: 0.5rem; display: flex; flex-direction: column; gap: 0.65rem; background: color-mix(in srgb, var(--accent) 8%, transparent); border: 1px solid color-mix(in srgb, var(--accent) 25%, transparent); }
.featured-config h3 { font-size: 0.95rem; margin: 0; }
.featured-config .hint { font-size: 0.8rem; color: var(--muted); margin: 0; }
.label-row { display: flex; justify-content: space-between; align-items: center; gap: 0.5rem; }
.translate-btn {
  background: color-mix(in srgb, var(--accent) 16%, transparent); color: var(--accent);
  border: none; border-radius: 999px; padding: 0.2rem 0.7rem; font-size: 0.75rem; cursor: pointer; font-family: inherit;
}
.translate-btn:disabled { opacity: 0.5; cursor: progress; }
</style>
