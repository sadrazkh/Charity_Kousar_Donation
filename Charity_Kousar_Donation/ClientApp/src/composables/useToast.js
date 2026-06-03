import { ref } from 'vue'

export const toasts = ref([])
let nextId = 0

export function useToast() {
  function show(message, type = 'success', duration = 3200) {
    const id = ++nextId
    toasts.value.push({ id, message, type })
    setTimeout(() => {
      toasts.value = toasts.value.filter((t) => t.id !== id)
    }, duration)
  }

  return {
    toasts,
    show,
    success: (message) => show(message, 'success'),
    error: (message) => show(message, 'error')
  }
}
