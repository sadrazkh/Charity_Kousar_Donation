<script setup>
import { ref, onMounted, watch } from 'vue'
import QRCode from 'qrcode'

const props = defineProps({
  url: { type: String, required: true },
  size: { type: Number, default: 140 }
})
const canvasRef = ref(null)

async function render() {
  if (!canvasRef.value || !props.url) return
  await QRCode.toCanvas(canvasRef.value, props.url, { width: props.size, margin: 1 })
}

onMounted(render)
watch(() => props.url, render)
</script>

<template>
  <canvas ref="canvasRef" class="qr-canvas" />
</template>

<style scoped>
.qr-canvas { border-radius: 8px; background: #fff; padding: 4px; }
</style>
