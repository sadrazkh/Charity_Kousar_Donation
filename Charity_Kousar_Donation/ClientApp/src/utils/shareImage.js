/** Generate a share card PNG from campaign data */
export async function generateShareImage(pack, locale = 'fa') {
  const W = 1080
  const H = 1350
  const canvas = document.createElement('canvas')
  canvas.width = W
  canvas.height = H
  const ctx = canvas.getContext('2d')

  const title = locale === 'fa' ? pack.campaignTitleFa : pack.campaignTitleEn
  const grad = ctx.createLinearGradient(0, 0, W, H)
  grad.addColorStop(0, '#0f172a')
  grad.addColorStop(0.5, '#134e4a')
  grad.addColorStop(1, '#0f172a')
  ctx.fillStyle = grad
  ctx.fillRect(0, 0, W, H)

  if (pack.imageUrl) {
    try {
      const img = await loadImage(pack.imageUrl)
      ctx.drawImage(img, 0, 0, W, 520)
      ctx.fillStyle = 'rgba(15,23,42,0.55)'
      ctx.fillRect(0, 320, W, 200)
    } catch { /* skip */ }
  }

  ctx.fillStyle = '#f59e0b'
  ctx.font = 'bold 28px Tahoma, Arial'
  ctx.textAlign = 'center'
  ctx.fillText(locale === 'fa' ? 'خیریه کوثر' : 'Kousar Charity', W / 2, pack.imageUrl ? 380 : 120)

  ctx.fillStyle = '#f1f5f9'
  ctx.font = 'bold 52px Tahoma, Arial'
  wrapText(ctx, title, W / 2, pack.imageUrl ? 460 : 220, W - 120, 62)

  const collected = Number(pack.collectedAmount).toLocaleString(locale === 'fa' ? 'fa-IR' : 'en-US')
  const target = Number(pack.targetAmount).toLocaleString(locale === 'fa' ? 'fa-IR' : 'en-US')
  ctx.font = '32px Tahoma, Arial'
  ctx.fillStyle = '#94a3b8'
  const statsY = pack.imageUrl ? 680 : 420
  ctx.fillText(
    locale === 'fa' ? `${collected} از ${target} تومان (${pack.progressPercent}%)` : `${collected} / ${target} Toman (${pack.progressPercent}%)`,
    W / 2, statsY
  )

  const barY = statsY + 40
  ctx.fillStyle = 'rgba(148,163,184,0.25)'
  roundRect(ctx, 140, barY, W - 280, 18, 9)
  ctx.fill()
  ctx.fillStyle = '#0d9488'
  roundRect(ctx, 140, barY, (W - 280) * (pack.progressPercent / 100), 18, 9)
  ctx.fill()

  ctx.fillStyle = '#cbd5e1'
  ctx.font = '28px Tahoma, Arial'
  const msg = (locale === 'fa' ? pack.messageFa : pack.messageEn).split('\n').slice(0, 4)
  let y = barY + 80
  for (const line of msg) {
    if (line.startsWith('http') || line.includes('🔗') || line.includes('💳')) continue
    wrapText(ctx, line, W / 2, y, W - 140, 36)
    y += 50
  }

  ctx.fillStyle = '#0d9488'
  ctx.font = 'bold 30px Tahoma, Arial'
  ctx.fillText(locale === 'fa' ? '💳 لینک کمک:' : '💳 Donate:', W / 2, H - 180)
  ctx.fillStyle = '#fbbf24'
  ctx.font = '26px Tahoma, Arial'
  wrapText(ctx, pack.shortUrl, W / 2, H - 130, W - 100, 32)

  return canvas.toDataURL('image/png')
}

function loadImage(src) {
  return new Promise((resolve, reject) => {
    const img = new Image()
    img.crossOrigin = 'anonymous'
    img.onload = () => resolve(img)
    img.onerror = reject
    img.src = src
  })
}

function wrapText(ctx, text, x, y, maxWidth, lineHeight) {
  const words = text.split(' ')
  let line = ''
  for (const word of words) {
    const test = line + word + ' '
    if (ctx.measureText(test).width > maxWidth && line) {
      ctx.fillText(line.trim(), x, y)
      line = word + ' '
      y += lineHeight
    } else line = test
  }
  if (line) ctx.fillText(line.trim(), x, y)
}

function roundRect(ctx, x, y, w, h, r) {
  ctx.beginPath()
  ctx.moveTo(x + r, y)
  ctx.arcTo(x + w, y, x + w, y + h, r)
  ctx.arcTo(x + w, y + h, x, y + h, r)
  ctx.arcTo(x, y + h, x, y, r)
  ctx.arcTo(x, y, x + w, y, r)
  ctx.closePath()
}

export function downloadDataUrl(dataUrl, filename) {
  const a = document.createElement('a')
  a.href = dataUrl
  a.download = filename
  a.click()
}
