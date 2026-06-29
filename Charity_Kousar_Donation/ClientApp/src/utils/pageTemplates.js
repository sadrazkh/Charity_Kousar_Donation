import { createBlock } from '@/utils/pageBlocks'

// Build a block of `type` with overridden data fields.
function b(type, data) {
  const block = createBlock(type)
  if (data) Object.assign(block.data, data)
  return block
}

// Ready-made dedicated-page layouts. Each build() returns a fresh block array.
export const PAGE_TEMPLATES = [
  {
    id: 'story', icon: '📖', fa: 'داستان تأثیرگذار', en: 'Impact story',
    descFa: 'تصویر بزرگ، روایت، آمار و فراخوان', descEn: 'Hero image, story, stats & CTA',
    build: () => [
      b('image', { fullWidth: true }),
      b('heading', { textFa: 'عنوان کمپین', textEn: 'Campaign title', level: 1, align: 'center' }),
      b('text', { contentFa: 'داستان این کمپین را اینجا بنویسید و بگویید کمک‌ها چه تغییری ایجاد می‌کنند.', contentEn: 'Tell the story of this campaign and what the donations change.' }),
      b('stats'),
      b('quote', { textFa: 'هر کمک کوچک، تغییری بزرگ می‌سازد.', textEn: 'Every small gift makes a big change.' }),
      b('cta', { textFa: 'همین حالا کمک کنید', textEn: 'Donate now', align: 'center' })
    ]
  },
  {
    id: 'goal', icon: '🎯', fa: 'متمرکز بر هدف', en: 'Goal-focused',
    descFa: 'آمار در ابتدا، توضیح و جعبه تأکید', descEn: 'Stats first, description & callout',
    build: () => [
      b('heading', { textFa: 'به این هدف کمک کنید', textEn: 'Help reach this goal', level: 1, align: 'center' }),
      b('stats'),
      b('text', { contentFa: 'توضیح کوتاه دربارهٔ این‌که مبلغ جمع‌آوری‌شده صرف چه چیزی می‌شود.', contentEn: 'A short note on what the raised amount will be used for.' }),
      b('callout', { style: 'success', textFa: 'تمام کمک‌ها مستقیماً به نیازمندان می‌رسد.', textEn: 'All donations go directly to those in need.' }),
      b('cta', { textFa: 'مشارکت می‌کنم', textEn: 'I want to help', align: 'center' })
    ]
  },
  {
    id: 'gallery', icon: '🖼', fa: 'گالری تصاویر', en: 'Photo gallery',
    descFa: 'نمایش تصاویر فعالیت‌ها', descEn: 'Showcase activity photos',
    build: () => [
      b('heading', { textFa: 'نگاهی به فعالیت‌های ما', textEn: 'A look at our work', level: 1, align: 'center' }),
      b('text', { contentFa: 'تصاویری از کمک‌رسانی و تأثیر واقعی مشارکت شما.', contentEn: 'Photos of our relief work and the real impact of your support.' }),
      b('gallery', { columns: 3, images: [{ url: '', captionFa: '', captionEn: '' }, { url: '', captionFa: '', captionEn: '' }, { url: '', captionFa: '', captionEn: '' }] }),
      b('stats'),
      b('cta', { textFa: 'کمک به این پروژه', textEn: 'Support this project', align: 'center' })
    ]
  },
  {
    id: 'steps', icon: '①', fa: 'سه گام تأثیر', en: 'Three-step impact',
    descFa: 'توضیح مسیر کمک در سه مرحله', descEn: 'Explain the impact in 3 steps',
    build: () => [
      b('heading', { textFa: 'کمک شما چگونه کار می‌کند', textEn: 'How your help works', level: 1, align: 'center' }),
      b('steps'),
      b('stats'),
      b('banner', { titleFa: 'همراه ما باشید', titleEn: 'Be part of it', textFa: 'با هر مبلغ می‌توانید سهیم شوید.', textEn: 'Any amount helps.' })
    ]
  },
  {
    id: 'twocol', icon: '▥', fa: 'دو ستونه (متن + تصویر)', en: 'Two columns (text + image)',
    descFa: 'متن در کنار تصویر', descEn: 'Text beside an image',
    build: () => {
      const cols = b('columns', { count: 2, gap: 'lg' })
      cols.data.columns = [
        [b('heading', { textFa: 'دربارهٔ این کمپین', textEn: 'About this campaign', level: 2 }), b('text', { contentFa: 'توضیحات کامل اینجا...', contentEn: 'Full description here...' })],
        [b('image', {})]
      ]
      return [cols, b('stats'), b('cta', { textFa: 'کمک کنید', textEn: 'Donate', align: 'center' })]
    }
  },
  {
    id: 'video', icon: '▶', fa: 'ویدیویی', en: 'Video-led',
    descFa: 'ویدیو معرفی + توضیح', descEn: 'Intro video + description',
    build: () => [
      b('heading', { textFa: 'این کمپین را در ویدیو ببینید', textEn: 'Watch this campaign', level: 1, align: 'center' }),
      b('video', {}),
      b('text', { contentFa: 'توضیح تکمیلی دربارهٔ ویدیو و هدف کمپین.', contentEn: 'More about the video and the goal.' }),
      b('stats'),
      b('cta', { textFa: 'همین حالا کمک کنید', textEn: 'Donate now', align: 'center' })
    ]
  },
  {
    id: 'faq', icon: '❓', fa: 'با سوالات متداول', en: 'With FAQ',
    descFa: 'توضیح، آمار و پرسش‌های پرتکرار', descEn: 'Description, stats & FAQ',
    build: () => [
      b('heading', { textFa: 'عنوان کمپین', textEn: 'Campaign title', level: 1, align: 'center' }),
      b('text', { contentFa: 'توضیح کوتاه کمپین.', contentEn: 'Short campaign description.' }),
      b('stats'),
      b('faq', { items: [
        { qFa: 'کمک‌های من کجا هزینه می‌شود؟', qEn: 'Where do my donations go?', aFa: 'مستقیماً صرف این پروژه می‌شود.', aEn: 'Directly to this project.' },
        { qFa: 'آیا رسید دریافت می‌کنم؟', qEn: 'Will I get a receipt?', aFa: 'بله، پس از پرداخت پیامک تأیید ارسال می‌شود.', aEn: 'Yes, an SMS confirmation is sent.' }
      ] }),
      b('cta', { textFa: 'کمک کنید', textEn: 'Donate', align: 'center' })
    ]
  }
]
