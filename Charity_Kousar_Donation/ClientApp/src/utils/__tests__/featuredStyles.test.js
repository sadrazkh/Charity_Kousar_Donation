import { describe, it, expect } from 'vitest'
import {
  DEFAULT_FEATURED_STYLES, parseFeaturedStyles, featuredStyleFor, styleLabel
} from '../featuredStyles'

const config = {
  featuredStyles: JSON.stringify([
    { id: 'urgent', color: '#ef4444', labelFa: 'اضطراری', labelEn: 'Urgent' }
  ]),
  featuredColor: '#f59e0b',
  featuredBadgeFa: 'ویژه',
  featuredBadgeEn: 'Featured'
}

describe('parseFeaturedStyles', () => {
  it('reads the admin list', () => {
    expect(parseFeaturedStyles(config.featuredStyles)).toHaveLength(1)
  })

  it('falls back to the built-ins for empty, broken or unusable JSON', () => {
    expect(parseFeaturedStyles('')).toEqual(DEFAULT_FEATURED_STYLES)
    expect(parseFeaturedStyles('not json')).toEqual(DEFAULT_FEATURED_STYLES)
    expect(parseFeaturedStyles('[]')).toEqual(DEFAULT_FEATURED_STYLES)
    expect(parseFeaturedStyles('{"id":"x"}')).toEqual(DEFAULT_FEATURED_STYLES)
    expect(parseFeaturedStyles('[{"color":"#fff"}]')).toEqual(DEFAULT_FEATURED_STYLES)  // no id
  })
})

describe('featuredStyleFor', () => {
  it('uses the style the campaign points at', () => {
    expect(featuredStyleFor({ featuredStyle: 'urgent' }, config).color).toBe('#ef4444')
  })

  it('falls back to the site badge for campaigns saved before styles existed', () => {
    const style = featuredStyleFor({ featuredStyle: null }, config)
    expect(style.color).toBe('#f59e0b')
    expect(style.labelFa).toBe('ویژه')
  })

  it('falls back when the style was deleted from the list', () => {
    expect(featuredStyleFor({ featuredStyle: 'deleted-one' }, config).color).toBe('#f59e0b')
  })

  it('still returns something usable with an empty config', () => {
    const style = featuredStyleFor({}, {})
    expect(style.color).toBeTruthy()
    expect(styleLabel(style, 'fa')).toBeTruthy()
  })
})

describe('styleLabel', () => {
  const style = { labelFa: 'اضطراری', labelEn: 'Urgent' }

  it('picks the label for the current language', () => {
    expect(styleLabel(style, 'fa')).toBe('اضطراری')
    expect(styleLabel(style, 'en')).toBe('Urgent')
  })

  it('uses whichever label exists when one is missing', () => {
    expect(styleLabel({ labelFa: 'فقط فارسی' }, 'en')).toBe('فقط فارسی')
    expect(styleLabel({ labelEn: 'EN only' }, 'fa')).toBe('EN only')
    expect(styleLabel(undefined, 'fa')).toBe('')
  })
})
