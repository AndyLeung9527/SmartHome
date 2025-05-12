import { createApp } from 'vue'
import App from './App.vue'
import router from '@/router'
import ElementPlus from 'element-plus'
import zhCn from 'element-plus/es/locale/lang/zh-cn'
import { createPinia } from 'pinia'

const app = createApp(App)

app.use(router)
app.use(ElementPlus, { locale: zhCn, size: 'small', zIndex: 2000 })
app.use(createPinia())

app.mount('#app')
