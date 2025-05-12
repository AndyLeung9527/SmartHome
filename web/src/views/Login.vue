<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import type { AxiosError } from 'axios'
import { type FormInstance, type FormRules } from 'element-plus'
import useAuthentication from '@/hooks/useAuthentication'
import { useUserStore } from '@/store/useUserStore'

interface LoginFormInter {
    nameOrEmail: string,
    password: string,
    remember: false
}

const { authenticate } = useAuthentication()
const router = useRouter()
const userStore = useUserStore()
let loading = ref(false)

if (userStore.isLoggedIn()) {
    router.replace('/')
}

const loginFormRef = ref<FormInstance>()
const loginForm = reactive<LoginFormInter>({
    nameOrEmail: '',
    password: '',
    remember: false
})
const rules = reactive<FormRules<LoginFormInter>>({
    nameOrEmail: [
        { required: true, message: '请输入用户名或邮箱', trigger: 'blur' },
        { max: 100, message: '用户名或邮箱长度不能超过100', trigger: 'blur' }
    ],
    password: [
        { required: true, message: '请输入密码', trigger: 'blur' }
    ]
})
const submitForm = async (formEl: FormInstance | undefined) => {
    if (!formEl) return
    await formEl.validate(async (valid, fields) => {
        if (valid) {
            loading.value = true
            try {
                let info = await authenticate({
                    nameOrEmail: loginForm.nameOrEmail,
                    password: loginForm.password
                })
                userStore.login(info.name, info.accessToken, info.refreshToken, loginForm.remember)
                router.replace('/')
            } catch (error) {
                ElMessage({
                    message: (error as AxiosError).response?.data as string,
                    type: 'error',
                })
            }
            loading.value = false
        }
    })
}
</script>

<template>
    <div class="login-container">
        <el-card class="login-box">
            <h2 class="login-title">用户登录</h2>
            <el-form ref="loginFormRef" :model="loginForm" :rules="rules" label-width="auto" label-position="top">
                <el-form-item label="用户名/邮箱" prop="nameOrEmail">
                    <el-input v-model="loginForm.nameOrEmail" placeholder="输入用户名或邮箱" clearable />
                </el-form-item>

                <el-form-item label="密码" prop="password">
                    <el-input v-model="loginForm.password" placeholder="输入密码" type="password" clearable show-password />
                </el-form-item>

                <el-form-item>
                    <el-checkbox v-model="loginForm.remember">记住密码</el-checkbox>
                </el-form-item>

                <el-form-item>
                    <el-button :plain="true" :loading="loading" type="primary" class="login-btn"
                        @click="submitForm(loginFormRef)">
                        立即登录
                    </el-button>
                </el-form-item>

                <div class="link-container">
                    <RouterLink class="text-link" to="/forgotPassword">忘记密码?</RouterLink>
                    <RouterLink class="text-link" to="/register">立即注册</RouterLink>
                </div>
            </el-form>
        </el-card>
    </div>
</template>

<style scoped>
.login-container {
    min-height: 100vh;
    display: flex;
    justify-content: center;
    align-items: center;
    background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
    font-family: 'Segoe UI', 'PingFang SC', 'Microsoft YaHei', sans-serif;
}

.login-box {
    width: 400px;
    padding: 30px;
    border-radius: 12px;
    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1);
}

.login-title {
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

.login-btn {
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