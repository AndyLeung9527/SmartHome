import { createRouter, createWebHistory } from "vue-router"
import Index from "@/views/Index.vue"
import Register from "@/views/Register.vue"
import Login from '@/views/Login.vue'
import ConfirmEmail from "@/views/ConfirmEmail.vue"
import ForgotPassword from "@/views/ForgotPassword.vue"
import ResetPassword from "@/views/ResetPassword.vue"
import Message from "@/views/Message.vue"

const router = createRouter({
    history: createWebHistory(),
    routes: [
        {
            path: '/',
            component: Index
        },
        {
            path: '/register',
            component: Register
        },
        {
            path: '/confirmemail',
            component: ConfirmEmail,
            props(route) {
                return route.query
            }
        },
        {
            path: '/login',
            component: Login
        },
        {
            path: '/forgotPassword',
            component: ForgotPassword
        },
        {
            path: '/resetPassword',
            component: ResetPassword,
            props(route) {
                return route.query
            }
        },
        {
            name: 'message',
            path: '/message/:content?',
            component: Message,
            props: true
        }
    ]
})

export default router