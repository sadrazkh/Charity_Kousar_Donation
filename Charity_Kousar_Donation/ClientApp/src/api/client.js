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
