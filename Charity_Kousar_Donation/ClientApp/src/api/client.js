const TOKEN_KEY = 'kousar_admin_token'

export function getToken() {
  return localStorage.getItem(TOKEN_KEY)
}

export function setToken(token) {
  localStorage.setItem(TOKEN_KEY, token)
}

export function clearToken() {
  localStorage.removeItem(TOKEN_KEY)
}

export async function api(path, options = {}) {
  const headers = { 'Content-Type': 'application/json', ...options.headers }
  const token = getToken()
  if (token) headers.Authorization = `Bearer ${token}`

  const res = await fetch(`/api${path}`, { ...options, headers })
  if (res.status === 401 && path.startsWith('/')) {
    clearToken()
    if (window.location.pathname.startsWith('/admin') && !path.includes('/auth/login')) {
      window.location.href = '/admin/login'
    }
  }
  const data = res.headers.get('content-type')?.includes('json')
    ? await res.json()
    : null
  if (!res.ok) throw new Error(data?.message || 'خطا در ارتباط با سرور')
  return data
}

export async function uploadFile(file) {
  const form = new FormData()
  form.append('file', file)
  const headers = {}
  const token = getToken()
  if (token) headers.Authorization = `Bearer ${token}`
  const res = await fetch('/api/upload', { method: 'POST', headers, body: form })
  const data = res.headers.get('content-type')?.includes('json') ? await res.json() : null
  if (!res.ok) throw new Error(data?.message || 'آپلود ناموفق')
  return data
}

export async function downloadAuthFile(path, filename) {
  const headers = {}
  const token = getToken()
  if (token) headers.Authorization = `Bearer ${token}`
  const res = await fetch(`/api${path}`, { headers })
  if (!res.ok) throw new Error('دانلود ناموفق')
  const blob = await res.blob()
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  a.click()
  URL.revokeObjectURL(url)
}
