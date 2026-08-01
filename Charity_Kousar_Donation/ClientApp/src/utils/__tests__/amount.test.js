import { describe, it, expect } from 'vitest'
import { formatAmount, parseAmount, formatAmountTyping, toEnglishDigits } from '../amount'

describe('formatAmount', () => {
  it('groups thousands with Persian digits in Persian', () => {
    expect(formatAmount(1500000, 'fa')).toBe('۱٬۵۰۰٬۰۰۰')
  })

  it('groups thousands with Latin digits in English', () => {
    expect(formatAmount(1500000, 'en')).toBe('1,500,000')
  })

  it('never shows a fraction of a Toman', () => {
    expect(formatAmount(1234.67, 'en')).toBe('1,235')
  })

  it('falls back to 0 for junk instead of NaN', () => {
    expect(formatAmount(undefined)).toBe('0')
    expect(formatAmount('abc')).toBe('0')
    expect(formatAmount(Infinity)).toBe('0')
  })
})

describe('parseAmount', () => {
  it('reads what a donor typed, in either script', () => {
    expect(parseAmount('۱٬۵۰۰٬۰۰۰')).toBe(1500000)
    expect(parseAmount('1,500,000')).toBe(1500000)
    expect(parseAmount('١٢٣')).toBe(123)          // Arabic-Indic digits
  })

  it('ignores stray characters and empty input', () => {
    expect(parseAmount('50 000 تومان')).toBe(50000)
    expect(parseAmount('')).toBe(0)
    expect(parseAmount(null)).toBe(0)
  })

  it('keeps a number as-is', () => {
    expect(parseAmount(2500)).toBe(2500)
    expect(parseAmount(NaN)).toBe(0)
  })
})

describe('formatAmountTyping', () => {
  it('shows nothing while the field is empty', () => {
    expect(formatAmountTyping('')).toEqual({ numeric: 0, display: '' })
  })

  it('reformats as the donor types', () => {
    expect(formatAmountTyping('50000', 'en')).toEqual({ numeric: 50000, display: '50,000' })
  })
})

describe('toEnglishDigits', () => {
  it('normalises Persian and Arabic digits', () => {
    expect(toEnglishDigits('۰۹۱۲۳۴۵۶۷۸۹')).toBe('09123456789')
    expect(toEnglishDigits('٠٩١٢')).toBe('0912')
  })

  it('leaves other text alone', () => {
    expect(toEnglishDigits('تومان')).toBe('تومان')
    expect(toEnglishDigits(null)).toBe('')
  })
})
