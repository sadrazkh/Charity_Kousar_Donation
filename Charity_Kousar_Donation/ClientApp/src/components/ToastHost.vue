<script setup>
import { toasts } from '@/composables/useToast'
</script>

<template>
  <Teleport to="body">
    <div class="toast-host" aria-live="polite">
      <TransitionGroup name="toast">
        <div v-for="t in toasts" :key="t.id" class="toast" :class="t.type">
          <span class="toast-icon">{{ t.type === 'success' ? '✓' : '!' }}</span>
          <span class="toast-msg">{{ t.message }}</span>
        </div>
      </TransitionGroup>
    </div>
  </Teleport>
</template>

<style scoped>
.toast-host {
  position: fixed;
  bottom: 1.25rem;
  inset-inline: 1rem;
  z-index: 9999;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
  pointer-events: none;
}

.toast {
  display: flex;
  align-items: center;
  gap: 0.65rem;
  padding: 0.85rem 1.15rem;
  border-radius: 12px;
  background: var(--card);
  border: 1px solid rgba(148, 163, 184, 0.25);
  box-shadow: 0 12px 40px rgba(0, 0, 0, 0.25);
  color: var(--text);
  font-size: 0.92rem;
  font-weight: 500;
  max-width: min(360px, 100%);
  pointer-events: auto;
  backdrop-filter: blur(12px);
}

.toast.success {
  border-color: color-mix(in srgb, #34d399 45%, transparent);
  background: color-mix(in srgb, #065f46 18%, var(--card));
}

.toast.error {
  border-color: color-mix(in srgb, #f87171 45%, transparent);
  background: color-mix(in srgb, #7f1d1d 18%, var(--card));
}

.toast-icon {
  width: 26px;
  height: 26px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.85rem;
  flex-shrink: 0;
}

.toast.success .toast-icon { background: #065f4633; color: #34d399; }
.toast.error .toast-icon { background: #7f1d1d33; color: #f87171; }

.toast-enter-active, .toast-leave-active { transition: all 0.28s ease; }
.toast-enter-from, .toast-leave-to { opacity: 0; transform: translateY(12px); }
.toast-move { transition: transform 0.28s ease; }

@media (max-width: 768px) {
  .toast-host { bottom: 1rem; }
}
</style>
