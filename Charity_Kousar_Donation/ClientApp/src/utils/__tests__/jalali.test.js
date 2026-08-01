import { describe, it, expect } from 'vitest'
import { toPersianDigits, toEnglishDigits, daysInJalaliMonth, JALALI_MONTHS } from '../jalali'

describe('digit conversion', () => {
  it('shows numbers in Persian digits', () => {
    expect(toPersianDigits('72%')).toBe('۷۲%')
    expect(toPersianDigits(1404)).toBe('۱۴۰۴')
  })

  it('round-trips back to Latin digits', () => {
    expect(toEnglishDigits(toPersianDigits('2026-08-01'))).toBe('2026-08-01')
  })

  it('handles empty input without throwing', () => {
    expect(toEnglishDigits(null)).toBe('')
    expect(toPersianDigits('')).toBe('')
  })
})

describe('Jalali calendar', () => {
  it('has twelve month names', () => {
    expect(JALALI_MONTHS).toHaveLength(12)
    expect(JALALI_MONTHS[0]).toBe('فروردین')
  })

  it('gives 31 days to the first six months and 30 to the next five', () => {
    for (let m = 1; m <= 6; m++) expect(daysInJalaliMonth(1403, m)).toBe(31)
    for (let m = 7; m <= 11; m++) expect(daysInJalaliMonth(1403, m)).toBe(30)
  })

  it('gives Esfand 30 days in a leap year and 29 otherwise', () => {
    expect(daysInJalaliMonth(1403, 12)).toBe(30) // 1403 is a leap year
    expect(daysInJalaliMonth(1404, 12)).toBe(29)
  })
})
