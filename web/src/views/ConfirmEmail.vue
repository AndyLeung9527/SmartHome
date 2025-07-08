<script setup lang="ts">
import { ref, onMounted } from 'vue'
import useAuthentication from '@/hooks/useAuthentication'

const { confirmEmail } = useAuthentication()

let props = defineProps(['email', 'token'])
let email = ref(props.email)
let token = ref(props.token)

let msg = ref('');
const loading = ref(true)

onMounted(async () => {
    try {
        await confirmEmail({
            email: email.value,
            token: token.value
        })
        msg.value = `邮箱确认成功！<a href='${window.location.origin}/login'>点击跳转登录页</a>`
    } catch (error) {
        msg.value = `确认邮箱失败，请稍后再试`
        console.error(error)
    } finally {
        loading.value = false
    }
})
</script>

<template>
    <div v-if="loading">
        <span>正在确认邮箱，请稍候...</span>
    </div>
    <div v-else>
        <span v-html="msg"></span>
    </div>
</template>