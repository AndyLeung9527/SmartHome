<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { type FormInstance, type FormRules } from 'element-plus'
import useAuthentication from '@/hooks/useAuthentication'
import type { AxiosError } from 'axios'

interface resetPasswordInter {
    password: string
    confirmPassword: string
}

const router = useRouter()
const { resetPassword } = useAuthentication()

let props = defineProps(['email', 'token'])
let email = ref(props.email)
let token = ref(props.token)

let loading = ref(false)
const resetPasswordFormRef = ref<FormInstance>()
const resetPasswordForm = reactive<resetPasswordInter>({
    password: '',
    confirmPassword: ''
})
const equalToPassword = (rule: any, value: any, callback: any) => {
    if (value === '') {
        callback(new Error('请再次输入密码'))
    } else if (value !== resetPasswordForm.password) {
        callback(new Error('两次输入密码不一致'))
    } else {
        callback()
    }
}
const rules = reactive<FormRules<resetPasswordInter>>({
    password: [
        { required: true, message: '请输入密码', trigger: 'blur' }
    ],
    confirmPassword: [
        { required: true, validator: equalToPassword, trigger: 'blur' }
    ]
})
const submitForm = async (formEl: FormInstance | undefined) => {
    if (!formEl) {
        return
    }
    await formEl.validate(async (valid, fields) => {
        if (valid) {
            loading.value = true
            try {
                await resetPassword({
                    email: email.value,
                    token: token.value,
                    password: resetPasswordForm.password,
                    confirmPassword: resetPasswordForm.confirmPassword
                })
                ElMessage({
                    message: '重置密码成功',
                    type: 'success'
                })
                router.replace('/login')
            } catch (error) {
                ElMessage({
                    message: (error as AxiosError).response?.data as string,
                    type: 'error'
                })
            }
            loading.value = false
        }
    })
}
</script>

<template>
    <div class="resetPassword-container">
        <el-card class="resetPassword-box">
            <h2 class="resetPassword-title">重置密码</h2>
            <el-form ref="resetPasswordFormRef" :model="resetPasswordForm" :rules="rules" label-width="auto"
                label-position="top">
                <el-form-item label="密码" prop="password">
                    <el-input v-model="resetPasswordForm.password" placeholder="输入密码" type="password" clearable
                        show-password />
                </el-form-item>

                <el-form-item label="确认密码" prop="confirmPassword">
                    <el-input v-model="resetPasswordForm.confirmPassword" placeholder="再次输入密码" type="password" clearable
                        show-password />
                </el-form-item>

                <el-form-item>
                    <el-button :plain="true" :loading="loading" type="primary" class="resetPassword-btn"
                        @click="submitForm(resetPasswordFormRef)" @keydown.enter="submitForm(resetPasswordFormRef)">
                        重置密码
                    </el-button>
                </el-form-item>

                <div class="link-container">
                    <RouterLink class="text-link" to="/register">立即注册</RouterLink>
                    <RouterLink class="text-link" to="/forgotPassword">已有账号</RouterLink>
                </div>
            </el-form>
        </el-card>
    </div>
</template>

<style scoped>
.resetPassword-container {
    min-height: 100vh;
    display: flex;
    justify-content: center;
    align-items: center;
    background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
    font-family: 'Segoe UI', 'PingFang SC', 'Microsoft YaHei', sans-serif;
}

.resetPassword-box {
    width: 400px;
    padding: 30px;
    border-radius: 12px;
    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1);
}

.resetPassword-title {
    font-size: 28px;
    letter-spacing: 2px;
    color: #2c3e50;
    margin-bottom: 35px;
    font-weight: 600;
}

.link-container {
    display: flex;
    justify-content: space-between;
    margin-top: 15px;
}

.text-link {
    color: #999;
    font-size: 14px;
    text-decoration: none;
    transition: color 0.3s;
}

.text-link:hover {
    color: #409EFF;
}

.resetPassword-btn {
    width: 100%;
    height: 45px;
    font-size: 16px;
}

:deep(.el-form-item__label) {
    font-weight: 600;
    color: #5a5e66;
    font-size: 16px;
    padding-bottom: 10px !important;
    font-family: 'Segoe UI', 'PingFang SC', 'Microsoft YaHei', sans-serif;
}

:deep(.el-input__wrapper) {
    border-radius: 10px;
    box-shadow: 0 2px 6px rgba(0, 0, 0, 0.08);
    border: 1px solid #dcdfe6;
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    padding: 12px 15px;
    background: #f8f9fa;
    font-family: 'Segoe UI', 'PingFang SC', 'Microsoft YaHei', sans-serif;
}

:deep(.el-input__wrapper:hover) {
    border-color: #409EFF;
    box-shadow: 0 2px 8px rgba(64, 158, 255, 0.2);
}

:deep(.el-input__wrapper.is-focus) {
    border-color: #409EFF;
    box-shadow: 0 2px 12px rgba(64, 158, 255, 0.3);
}

:deep(.el-input__inner) {
    font-size: 15px;
    color: #2c3e50;
    letter-spacing: 0.5px;
    font-family: 'Segoe UI', 'PingFang SC', 'Microsoft YaHei', sans-serif;
}
</style>