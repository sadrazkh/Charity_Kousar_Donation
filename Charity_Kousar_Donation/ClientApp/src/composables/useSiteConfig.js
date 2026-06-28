import { reactive, readonly } from 'vue'
import { api } from '@/api/client'

// Sensible defaults so components render correctly before /settings/public resolves.
const DEFAULTS = {
  siteNameFa: '', siteNameEn: '', taglineFa: '', taglineEn: '',
  heroTextFa: '', heroTextEn: '',
  logoUrl: null, logoHeight: 48, showLogoText: true,
  primaryColor: '#0d9488', accentColor: '#f59e0b', backgroundColor: '#0f172a',
  footerTextFa: '', footerTextEn: '',
  homeOrder: 'hero,featured,campaigns,donors',
  progressMode: 'shift', progressColorStart: '#ef4444', progressColorEnd: '#22c55e',
  showProgressPercent: true,
  featuredUnits: 'days,hours,minutes,seconds', featuredLayout: 'boxes',
  featuredBadgeShow: true, featuredBadgeFa: '⭐ ویژه', featuredBadgeEn: '⭐ Featured',
  featuredColor: '#f59e0b', featuredExpiredFa: '⏱ فرصت به پایان رسید', featuredExpiredEn: '⏱ Time ended',
  cryptoEnabled: false, zarinPalEnabled: true,
  minDonationAmount: 10000, quickDonationAmounts: [],
  showRecentDonors: true, recentDonorsCount: 10, showDonorsHome: true,
  showDonorName: true, showDonorAmount: true, showDonorDate: false, showDonorCampaign: false,
  donorAnonymousFa: 'نیکوکار', donorAnonymousEn: 'Well-wisher',
  donorsTitleFa: 'حامیان اخیر', donorsTitleEn: 'Recent supporters',
  shareAiEnabled: true,
  otpEnabled: false, otpThresholdAmount: 5000000, paymentBypassEnabled: false
}

const state = reactive({ ...DEFAULTS, loaded: false })
let inflight = null

export async function loadSiteConfig(force = false) {
  if (state.loaded && !force) return state
  if (inflight && !force) return inflight
  inflight = (async () => {
    try {
      const cfg = await api('/settings/public')
      Object.assign(state, DEFAULTS, cfg, { loaded: true })
    } catch {
      Object.assign(state, DEFAULTS, { loaded: true })
    } finally {
      inflight = null
    }
    return state
  })()
  return inflight
}

export function useSiteConfig() {
  return { config: readonly(state), loadSiteConfig }
}
