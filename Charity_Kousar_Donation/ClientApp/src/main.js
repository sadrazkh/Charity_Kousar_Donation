import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import i18n from './i18n'
// Fonts ship with the build instead of coming from Google's CDN, which is slow or
// blocked for many visitors in Iran. Only the subsets a page needs get downloaded.
import '@fontsource-variable/vazirmatn'
import '@fontsource-variable/manrope'
import './style.css'

createApp(App).use(router).use(i18n).mount('#app')
