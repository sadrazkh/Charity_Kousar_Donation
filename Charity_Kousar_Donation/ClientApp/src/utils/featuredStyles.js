// Highlight styles for featured campaigns (the colored ring + badge on a card).
// The list is admin-editable and stored as JSON in the "featured.styles" setting;
// each campaign points at one of them by id.

export const DEFAULT_FEATURED_STYLES = [
  { id: 'gold', color: '#f59e0b', labelFa: 'ویژه', labelEn: 'Featured' },
  { id: 'urgent', color: '#ef4444', labelFa: 'اضطراری', labelEn: 'Urgent' },
  { id: 'limited', color: '#fb923c', labelFa: 'فرصت محدود', labelEn: 'Limited time' },
  { id: 'important', color: '#3b82f6', labelFa: 'مهم', labelEn: 'Important' },
  { id: 'almost', color: '#22c55e', labelFa: 'نزدیک به تکمیل', labelEn: 'Almost funded' },
  { id: 'spotlight', color: '#a855f7', labelFa: 'ویژهٔ ماه', labelEn: 'Spotlight' }
]

export function parseFeaturedStyles(json) {
  try {
    const arr = JSON.parse(json || '[]')
    const clean = Array.isArray(arr) ? arr.filter(s => s && s.id) : []
    return clean.length ? clean : DEFAULT_FEATURED_STYLES
  } catch {
    return DEFAULT_FEATURED_STYLES
  }
}

/**
 * Resolves the style a campaign should use. Campaigns saved before styles existed
 * (or pointing at a deleted style) fall back to the global featured color/badge.
 */
export function featuredStyleFor(campaign, config) {
  const list = parseFeaturedStyles(config.featuredStyles)
  const match = list.find(s => s.id === campaign?.featuredStyle)
  if (match) return match
  return {
    id: '',
    color: config.featuredColor || '#f59e0b',
    labelFa: config.featuredBadgeFa || 'ویژه',
    labelEn: config.featuredBadgeEn || 'Featured'
  }
}

export function styleLabel(style, locale) {
  return (locale === 'fa' ? style?.labelFa : style?.labelEn) || style?.labelFa || style?.labelEn || ''
}
