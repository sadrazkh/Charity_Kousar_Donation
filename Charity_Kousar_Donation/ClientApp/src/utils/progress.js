// Progress-bar coloring. The whole bar is ONE solid color that shifts from the
// start color toward the end color (green) as the bar fills — no multi-color gradient.

function hexToRgb(hex) {
  let h = String(hex || '').trim().replace('#', '')
  if (h.length === 3) h = h.split('').map(c => c + c).join('')
  if (h.length !== 6) return { r: 239, g: 68, b: 68 }
  return {
    r: parseInt(h.slice(0, 2), 16),
    g: parseInt(h.slice(2, 4), 16),
    b: parseInt(h.slice(4, 6), 16)
  }
}

function rgbToHsl({ r, g, b }) {
  r /= 255; g /= 255; b /= 255
  const max = Math.max(r, g, b), min = Math.min(r, g, b)
  let h = 0, s = 0
  const l = (max + min) / 2
  const d = max - min
  if (d !== 0) {
    s = l > 0.5 ? d / (2 - max - min) : d / (max + min)
    if (max === r) h = ((g - b) / d + (g < b ? 6 : 0))
    else if (max === g) h = (b - r) / d + 2
    else h = (r - g) / d + 4
    h *= 60
  }
  return { h, s: s * 100, l: l * 100 }
}

function clamp(n, min, max) { return Math.min(max, Math.max(min, n)) }

/** Returns a single CSS color for the given fill percent (0-100). */
export function progressColor(percent, cfg = {}) {
  const p = clamp(Number(percent) || 0, 0, 100) / 100
  const start = hexToRgb(cfg.progressColorStart || '#ef4444')
  const end = hexToRgb(cfg.progressColorEnd || '#22c55e')

  if (cfg.progressMode === 'solid') {
    return cfg.progressColorEnd || '#22c55e'
  }

  // Interpolate in HSL so red → green sweeps through warm/yellow tones (gauge-like).
  const a = rgbToHsl(start)
  const b = rgbToHsl(end)
  const h = a.h + (b.h - a.h) * p
  const s = a.s + (b.s - a.s) * p
  const l = a.l + (b.l - a.l) * p
  return `hsl(${h.toFixed(0)}, ${s.toFixed(0)}%, ${l.toFixed(0)}%)`
}

/** Style object for the fill element. */
export function progressFillStyle(percent, cfg = {}) {
  const pct = clamp(Number(percent) || 0, 0, 100)
  if (cfg.progressMode === 'gradient') {
    return {
      width: pct + '%',
      background: `linear-gradient(90deg, ${cfg.progressColorStart || '#ef4444'}, ${cfg.progressColorEnd || '#22c55e'})`
    }
  }
  return { width: pct + '%', background: progressColor(pct, cfg) }
}
