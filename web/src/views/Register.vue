<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { type FormInstance, type FormRules } from 'element-plus'
import useValidator from '@/hooks/useValidator'
import useAuthentication from '@/hooks/useAuthentication'
import type { AxiosError } from 'axios'

interface RegisterFormInter {
    email: string,
    name: string,
    password: string,
    confirmPassword: string,
    dateOfBirth: string
}

const { validateEmail } = useValidator()
const { register } = useAuthentication()

const registerFormRef = ref<FormInstance>()
const registerForm = reactive<RegisterFormInter>({
    email: '',
    name: '',
    password: '',
    confirmPassword: '',
    dateOfBirth: ''
})
const equalToPassword = (rule: any, value: any, callback: any) => {
    if (value === '') {
        callback(new Error('请再次输入密码'))
    } else if (value !== registerForm.password) {
        callback(new Error('两次输入密码不一致'))
    } else {
        callback()
    }
}
const rules = reactive<FormRules<RegisterFormInter>>({
    email: [
        { required: true, message: '请输入邮箱', trigger: 'blur' },
        { validator: validateEmail, trigger: 'blur' },
        { max: 100, message: '邮箱长度不能超过100字符', trigger: 'blur' }
    ],
    name: [
        { required: true, message: '请输入用户名', trigger: 'blur' },
        { max: 50, message: '用户名长度不能超过50字符', trigger: 'blur' }
    ],
    password: [
        { required: true, message: '请输入密码', trigger: 'blur' }
    ],
    confirmPassword: [
        { required: true, validator: equalToPassword, trigger: 'blur' }
    ]
})
let loading = ref(false)
const router = useRouter()
const submitForm = async (formEl: FormInstance | undefined) => {
    if (!formEl) {
        return
    }
    await formEl.validate(async (valid, fields) => {
        if (valid) {
            loading.value = true
            try {
                await register({
                    email: registerForm.email,
                    name: registerForm.name,
                    password: registerForm.password,
                    confirmPassword: registerForm.confirmPassword,
                    dateOfBirth: registerForm.dateOfBirth
                })
                router.push({
                    name: 'message',
                    params: { content: `注册成功，请前往邮箱确认：${registerForm.email}` }
                })
            }
            catch (error) {
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
    <div class="register-container">
        <el-card class="register-box">
            <h2 class="register-title">欢迎注册</h2>
            <el-form ref="registerFormRef" :model="registerForm" :rules="rules" label-width="auto" label-position="top">
                <el-form-item label="邮箱" prop="email">
                    <el-input v-model="registerForm.email" placeholder="输入邮箱" clearable />
                </el-form-item>

                <el-form-item label="用户名" prop="name">
                    <el-input v-model="registerForm.name" placeholder="输入用户名" clearable />
                </el-form-item>

                <el-form-item label="密码" prop="password">
                    <el-input v-model="registerForm.password" placeholder="输入密码" type="password" clearable
                        show-password />
                </el-form-item>

                <el-form-item label="确认密码" prop="confirmPassword">
                    <el-input v-model="registerForm.confirmPassword" placeholder="再次输入密码" type="password" clearable
                        show-password />
                </el-form-item>

                <el-form-item label="出生日期" prop="dateOfBirth">
                    <el-date-picker v-model="registerForm.dateOfBirth" type="date" size="large" placeholder="输入出生日期" />
                </el-form-item>

                <el-form-item>
                    <el-button :plain="true" :loading="loading" type="primary" class="register-btn"
                        @click="submitForm(registerFormRef)" @keydown.enter="submitForm(registerFormRef)">
                        注册
                    </el-button>
                </el-form-item>

                <div class="link-container">
                    <RouterLink class="text-link" to="/forgotPassword">忘记密码?</RouterLink>
                    <RouterLink class="text-link" to="/login">已有帐号</RouterLink>
                </div>
            </el-form>
        </el-card>
    </div>
</template>

<style scoped>
.register-container {
    min-height: 100vh;
    display: flex;
    justify-content: center;
    align-items: center;
    background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
    font-family: 'Segoe UI', 'PingFang SC', 'Microsoft YaHei', sans-serif;
}

.register-box {
    width: 400px;
    padding: 30px;
    border-radius: 12px;
    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1);
}

.register-title {
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

.register-btn {
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