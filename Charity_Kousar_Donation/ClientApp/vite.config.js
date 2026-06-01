import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { fileURLToPath, URL } from 'node:url'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: { '@': fileURLToPath(new URL('./src', import.meta.url)) }
  },
  server: {
    port: 5173,
    proxy: { '/api': 'https://localhost:7208', '/d': 'https://localhost:7208' }
  },
  build: {
    outDir: '../wwwroot',
    emptyOutDir: true
  }
})
