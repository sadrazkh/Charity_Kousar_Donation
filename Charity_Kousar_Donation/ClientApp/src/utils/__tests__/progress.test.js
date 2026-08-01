import { describe, it, expect } from 'vitest'
import { progressColor, progressFillStyle } from '../progress'

const shift = { progressMode: 'shift', progressColorStart: '#ef4444', progressColorEnd: '#22c55e' }

describe('progressFillStyle', () => {
  it('turns a percent into a bar width', () => {
    expect(progressFillStyle(42, shift).width).toBe('42%')
  })

  it('keeps the width inside the track', () => {
    expect(progressFillStyle(-20, shift).width).toBe('0%')
    expect(progressFillStyle(180, shift).width).toBe('100%')
    expect(progressFillStyle('nonsense', shift).width).toBe('0%')
  })

  it('uses a real gradient only in gradient mode', () => {
    expect(progressFillStyle(50, { ...shift, progressMode: 'gradient' }).background)
      .toBe('linear-gradient(90deg, #ef4444, #22c55e)')
    expect(progressFillStyle(50, shift).background).not.toContain('gradient')
  })
})

describe('progressColor', () => {
  it('stays on the end colour in solid mode', () => {
    expect(progressColor(10, { ...shift, progressMode: 'solid' })).toBe('#22c55e')
  })

  it('walks from the start hue to the end hue as the bar fills', () => {
    const hue = (c) => Number(c.match(/hsl\((-?\d+)/)[1])
    const start = hue(progressColor(0, shift))
    const middle = hue(progressColor(50, shift))
    const end = hue(progressColor(100, shift))

    expect(start).toBeLessThan(middle)
    expect(middle).toBeLessThan(end)
    expect(start).toBeCloseTo(0, 0)      // red
    expect(end).toBeGreaterThan(100)     // green
  })

  it('survives short and malformed hex colours', () => {
    expect(progressColor(50, { progressColorStart: '#f00', progressColorEnd: '#0f0' })).toMatch(/^hsl\(/)
    expect(progressColor(50, { progressColorStart: 'nope', progressColorEnd: '' })).toMatch(/^hsl\(/)
    expect(progressColor(50, {})).toMatch(/^hsl\(/)
  })
})
