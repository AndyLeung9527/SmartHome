<script setup lang="ts">
import type { FormInstance, FormRules } from 'element-plus'
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import useAuthentication from '@/hooks/useAuthentication'
import type { AxiosError } from 'axios'

interface forgotPasswordFormInter {
    nameOrEmail: string
}

const router = useRouter()
const { forgotPassword } = useAuthentication()
let loading = ref(false)
const forgotPasswordFormRef = ref<FormInstance>()
const forgotPasswordForm = reactive<forgotPasswordFormInter>({
    nameOrEmail: ''
})
const rules = reactive<FormRules<forgotPasswordFormInter>>({
    nameOrEmail: [
        { required: true, message: '请输入用户名或邮箱', trigger: 'blur' },
        { max: 100, message: '用户名或邮箱长度不能超过100', trigger: 'blur' }
    ]
})
const submitForm = async (formEl: FormInstance | undefined) => {
    if (!formEl) return
    await formEl.validate(async (valid, fields) => {
        if (valid) {
            loading.value = true
            try {
                let email = await forgotPassword({
                    nameOrEmail: forgotPasswordForm.nameOrEmail
                })
                router.push({
                    name: 'message',
                    params: { content: `请前往邮箱确认重置密码：${email}` }
                })
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
    <div class="forgotPassword-container">
        <el-card class="forgotPassword-box">
            <h2 class="forgotPassword-title">忘记密码</h2>
            <el-form ref="forgotPasswordFormRef" :model="forgotPasswordForm" :rules="rules" label-width="auto"
                label-position="top">
                <el-form-item label="用户名/邮箱" prop="nameOrEmail">
                    <el-input v-model="forgotPasswordForm.nameOrEmail" placeholder="输入用户名或邮箱" clearable />
                </el-form-item>

                <el-form-item>
                    <el-button :plain="true" :loading="loading" type="primary" class="forgotPassword-btn"
                        @click="submitForm(forgotPasswordFormRef)">
                        重置密码
                    </el-button>
                </el-form-item>

                <div class="link-container">
                    <RouterLink class="text-link" to="/register">立即注册</RouterLink>
                    <RouterLink class="text-link" to="/login">已有账号</RouterLink>
                </div>
            </el-form>
        </el-card>
    </div>
</template>

<style scoped>
.forgotPassword-container {
    min-height: 100vh;
    display: flex;
    justify-content: center;
    align-items: center;
    background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
    font-family: 'Segoe UI', 'PingFang SC', 'Microsoft YaHei', sans-serif;
}

.forgotPassword-box {
    width: 400px;
    padding: 30px;
    border-radius: 12px;
    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1);
}

.forgotPassword-title {
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

.forgotPassword-btn {
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