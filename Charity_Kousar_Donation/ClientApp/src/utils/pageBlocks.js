export const BLOCK_TYPES = [
  { type: 'columns', icon: '▥', labelFa: 'چند ستونه (۱–۵)', labelEn: 'Columns (1–5)', category: 'layout' },
  { type: 'section', icon: '▢', labelFa: 'بخش/سکشن', labelEn: 'Section', category: 'layout' },
  { type: 'spacer', icon: '↕', labelFa: 'فاصله', labelEn: 'Spacer', category: 'layout' },
  { type: 'divider', icon: '—', labelFa: 'جداکننده', labelEn: 'Divider', category: 'layout' },
  { type: 'heading', icon: 'H', labelFa: 'عنوان', labelEn: 'Heading', category: 'content' },
  { type: 'text', icon: '¶', labelFa: 'متن', labelEn: 'Text', category: 'content' },
  { type: 'list', icon: '•', labelFa: 'لیست', labelEn: 'List', category: 'content' },
  { type: 'quote', icon: '❝', labelFa: 'نقل‌قول', labelEn: 'Quote', category: 'content' },
  { type: 'callout', icon: '💡', labelFa: 'جعبه تأکید', labelEn: 'Callout', category: 'content' },
  { type: 'card', icon: '🃏', labelFa: 'کارت', labelEn: 'Card', category: 'content' },
  { type: 'faq', icon: '❓', labelFa: 'سوالات متداول', labelEn: 'FAQ', category: 'content' },
  { type: 'steps', icon: '①', labelFa: 'مراحل / تأثیر', labelEn: 'Steps / impact', category: 'content' },
  { type: 'image', icon: '🖼', labelFa: 'تصویر', labelEn: 'Image', category: 'media' },
  { type: 'gallery', icon: '▦', labelFa: 'گالری', labelEn: 'Gallery', category: 'media' },
  { type: 'video', icon: '▶', labelFa: 'ویدیو', labelEn: 'Video', category: 'media' },
  { type: 'stats', icon: '📊', labelFa: 'آمار کمپین', labelEn: 'Stats', category: 'campaign' },
  { type: 'banner', icon: '📣', labelFa: 'بنر فراخوان', labelEn: 'CTA banner', category: 'campaign' },
  { type: 'cta', icon: '♥', labelFa: 'دکمه کمک', labelEn: 'Donate button', category: 'campaign' },
  { type: 'button', icon: '🔗', labelFa: 'دکمه لینک', labelEn: 'Link button', category: 'campaign' }
]

export const NESTABLE_TYPES = BLOCK_TYPES.filter(b => !['columns', 'section'].includes(b.type)).map(b => b.type)

export function createBlock(type) {
  const id = Math.random().toString(36).slice(2, 10)
  const defaults = {
    columns: { count: 2, gap: 'md', columns: [[], []] },
    section: { bgColor: '', padding: 'md', align: 'start', blocks: [] },
    spacer: { size: 'md' },
    heading: { textFa: 'عنوان جدید', textEn: 'New heading', level: 2, align: 'start', color: '' },
    text: { contentFa: 'متن خود را اینجا بنویسید...', contentEn: 'Write your content here...', align: 'start', size: 'md' },
    list: { itemsFa: ['آیتم اول', 'آیتم دوم'], itemsEn: ['Item one', 'Item two'], style: 'bullet' },
    image: { url: '', captionFa: '', captionEn: '', fullWidth: false, rounded: true },
    gallery: { columns: 3, images: [{ url: '', captionFa: '', captionEn: '' }] },
    video: { url: '' },
    quote: { textFa: '', textEn: '', authorFa: '', authorEn: '' },
    callout: { textFa: '', textEn: '', style: 'info', icon: '💡' },
    card: { icon: '♥', titleFa: 'عنوان کارت', titleEn: 'Card title', textFa: '', textEn: '' },
    faq: { items: [{ qFa: 'سوال شما چیست؟', qEn: 'Your question?', aFa: 'پاسخ اینجا.', aEn: 'Answer here.' }] },
    steps: { columns: 3, items: [
      { icon: '①', titleFa: 'مرحله اول', titleEn: 'Step one', textFa: '', textEn: '' },
      { icon: '②', titleFa: 'مرحله دوم', titleEn: 'Step two', textFa: '', textEn: '' },
      { icon: '③', titleFa: 'مرحله سوم', titleEn: 'Step three', textFa: '', textEn: '' }
    ] },
    stats: {},
    banner: { titleFa: 'به ما بپیوندید', titleEn: 'Join us', textFa: 'با کمک شما این هدف محقق می‌شود.', textEn: 'Your help makes this possible.', btnFa: 'کمک می‌کنم', btnEn: 'I will help', color: '' },
    cta: { textFa: 'همین حالا کمک کنید', textEn: 'Donate now', align: 'center', size: 'lg', color: '' },
    button: { textFa: 'بیشتر بدانید', textEn: 'Learn more', url: '', style: 'ghost', align: 'start' },
    divider: { style: 'line' }
  }
  return { id, type, data: JSON.parse(JSON.stringify(defaults[type] || {})) }
}

export function createEmptyColumns(count) {
  return Array.from({ length: count }, () => [])
}

export function resizeColumns(block, newCount) {
  const cols = block.data.columns || createEmptyColumns(block.data.count || 2)
  const next = createEmptyColumns(newCount)
  for (let i = 0; i < newCount; i++) next[i] = cols[i] ? [...cols[i]] : []
  block.data.count = newCount
  block.data.columns = next
}

export function blockLabel(type, locale = 'fa') {
  const b = BLOCK_TYPES.find(x => x.type === type)
  return b ? (locale === 'fa' ? b.labelFa : b.labelEn) : type
}

export function getLocalized(data, key, locale) {
  if (!data) return ''
  const faKey = `${key}Fa`
  const enKey = `${key}En`
  if (locale === 'fa') return data[faKey] ?? data[enKey] ?? data[key] ?? ''
  return data[enKey] ?? data[faKey] ?? data[key] ?? ''
}

export function youtubeEmbed(url) {
  if (!url) return ''
  const m = url.match(/(?:youtube\.com\/watch\?v=|youtu\.be\/|youtube\.com\/embed\/)([\w-]+)/)
  return m ? `https://www.youtube.com/embed/${m[1]}` : url
}

export function blockPreviewText(block) {
  const d = block.data || {}
  return d.textFa || d.contentFa || d.titleFa || d.url || (d.count ? `${d.count} ستون` : '') || block.type
}

export function moveBlock(list, id, dir) {
  const idx = list.findIndex(b => b.id === id)
  if (idx < 0) return list
  const next = [...list]
  const ni = idx + dir
  if (ni < 0 || ni >= next.length) return list
  ;[next[idx], next[ni]] = [next[ni], next[idx]]
  return next
}

export const GAP_CLASS = { sm: 'gap-sm', md: 'gap-md', lg: 'gap-lg' }
export const SPACER_CLASS = { sm: 'sp-sm', md: 'sp-md', lg: 'sp-lg', xl: 'sp-xl' }
export const ALIGN_CLASS = { start: 'align-start', center: 'align-center', end: 'align-end' }
