<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import type { AxiosError } from 'axios'
import { useUserStore } from '@/store/useUserStore'
import useApi from '@/hooks/useApi'
import useBroadcastHub from '@/hooks/useBroadcastHub'
import {
    House, Folder, VideoCamera, Upload,
    Setting,
} from '@element-plus/icons-vue'
import type { broadcastResponseInter } from '@/types'
import { reactive } from 'vue'
import type { FormInstance, FormRules } from 'element-plus'
import { fi } from 'element-plus/es/locales.mjs'

interface BroadcastFormInter {
    message: string
}

const router = useRouter()
const { isLoggedIn, logout } = useUserStore()
const { userInfo, getBroadcasts, broadcast } = useApi()
const { connect, receivedBroadcast } = useBroadcastHub()

let name = ref('')
let avatar = ref('https://cube.elemecdn.com/3/7c/3ea6beec64369c2642b92c6726f1epng.png')

let broadcastInfo = reactive({
    broadcastGrip: [] as broadcastResponseInter[],
    broadcastUnreadCount: 0,
    broadcastTableVisible: false,
    broadcastGripRowClassName: ({ row, rowIndex }: { row: broadcastResponseInter, rowIndex: number }) => {
        if (!row.isRead) {
            return 'warning-row'
        }
        return ''
    },
    showFormButton: false
})
let broadcastFormInfo = reactive({
    broadcastFormVisible: false,
    loading: false,
    broadcastForm: {
        message: ''
    } as BroadcastFormInter,
    rules: {
        message: [
            { required: true, message: '请输入公告内容', trigger: 'blur' },
            { max: 500, message: '公告内容长度不能超过500', trigger: 'blur' }
        ]
    } as FormRules<BroadcastFormInter>
})

async function broadcastInfoShow() {
    broadcastInfo.broadcastGrip = await getBroadcasts()
    broadcastInfo.broadcastUnreadCount = 0
    broadcastInfo.broadcastTableVisible = true
}

function broadcastFormShow() {
    broadcastInfo.broadcastTableVisible = false
    broadcastFormInfo.broadcastFormVisible = true
}

const broadcastFormRef = ref<FormInstance>()
const broadcastFormSubmit = async (formEl: FormInstance | undefined) => {
    if (!formEl) return
    await formEl.validate(async (valid, fields) => {
        if (valid) {
            broadcastFormInfo.loading = true
            try {
                await broadcast(broadcastFormInfo.broadcastForm.message)
                broadcastFormInfo.broadcastForm.message = ''
                broadcastFormInfo.broadcastFormVisible = false
                ElMessage({
                    message: '公告发布成功',
                    type: 'success'
                })
            } catch (error) {
                ElMessage({
                    message: (error as AxiosError).response?.data as string,
                    type: 'error'
                })
            }
            broadcastFormInfo.loading = false
        }
    })
}

const loading = ref(true)
onMounted(async () => {
    if (!isLoggedIn()) {
        router.replace('/login')
        return
    }

    try {
        let info = await userInfo()
        name.value = info.name
        if (info.roles.includes('administrator')) {
            broadcastInfo.showFormButton = true
        }
    } catch (error) {
        let axiosError = error as AxiosError
        if (axiosError.response?.status != 401) {
            ElMessage({
                message: axiosError.response?.data as string,
                type: 'error',
            })
        }
        logout()
        router.replace('/login')
        return
    } finally {
        loading.value = false
    }

    try {
        let broadcasts = await getBroadcasts()
        for (let broadcast of broadcasts) {
            if (!broadcast.isRead) {
                broadcastInfo.broadcastUnreadCount += 1
            }
        }

        connect()

        receivedBroadcast(message => {
            broadcastInfo.broadcastUnreadCount += 1
            console.log('收到新公告:', message)
        })
    } catch (error) {
        console.error('获取公告失败:', error)
    }
})

interface Activity {
    content: string
    timestamp: string
}

const colors = [
    { color: '#5cb87a', percentage: 20 },
    { color: '#e6a23c', percentage: 40 },
    { color: '#f56c6c', percentage: 80 },
]

const activities = ref<Activity[]>([
    {
        content: '上传了 family_photo.jpg',
        timestamp: '2024-02-20 14:00',
    },
    {
        content: '备份了 Documents 文件夹',
        timestamp: '2024-02-19 09:30',
    },
    {
        content: '新增媒体文件 15 个',
        timestamp: '2024-02-18 16:45',
    },
])

const handleCommand = (command: string | number | object) => {
    if (command === 'logout') {
        logout()
        router.replace('/login')
    }
}
</script>

<template>
    <div v-if="loading">
        <span>正在登录，请稍候...</span>
    </div>
    <div v-else>
        <el-container class="home-container">
            <!-- 左侧导航 -->
            <el-aside width="200px" class="sidebar">
                <div class="logo">智能家庭中心</div>
                <el-menu active-text-color="#409EFF" background-color="#304156" class="vertical-menu" default-active="1"
                    text-color="#bfcbd9">
                    <el-menu-item index="1">
                        <el-icon>
                            <House />
                        </el-icon>
                        <span>首页概览</span>
                    </el-menu-item>
                    <el-menu-item index="2">
                        <el-icon>
                            <Folder />
                        </el-icon>
                        <span>文件管理</span>
                    </el-menu-item>
                    <el-menu-item index="3">
                        <el-icon>
                            <VideoCamera />
                        </el-icon>
                        <span>媒体库</span>
                    </el-menu-item>
                    <el-menu-item index="4">
                        <el-icon>
                            <Upload />
                        </el-icon>
                        <span>备份中心</span>
                    </el-menu-item>
                    <el-menu-item index="5">
                        <el-icon>
                            <Setting />
                        </el-icon>
                        <span>系统设置</span>
                    </el-menu-item>
                </el-menu>
            </el-aside>

            <el-container>
                <!-- 头部 -->
                <el-header class="header">
                    <div class="header-nav">
                        <el-dropdown @command="handleCommand">
                            <span class="el-dropdown-link">
                                <el-avatar :size="30" :src="avatar" />
                                <span class="username">{{ name }}</span>
                                <el-icon class="el-icon--right"><arrow-down /></el-icon>
                            </span>
                            <template #dropdown>
                                <el-dropdown-menu>
                                    <el-dropdown-item command="userCenter">个人中心</el-dropdown-item>
                                    <el-dropdown-item command="logout">退出登录</el-dropdown-item>
                                </el-dropdown-menu>
                            </template>
                        </el-dropdown>
                    </div>
                    <div class="header-nav">
                        <el-badge :value="broadcastInfo.broadcastUnreadCount" class="item" type="primary">
                            <el-icon @click="broadcastInfoShow">
                                <Message />
                            </el-icon>
                        </el-badge>
                    </div>
                </el-header>

                <!-- 主要内容 -->
                <el-main class="main-content">
                    <el-row :gutter="20">
                        <el-col :span="8">
                            <el-card class="stats-card">
                                <template #header>
                                    <div class="card-header">
                                        <span>存储空间</span>
                                    </div>
                                </template>
                                <el-progress type="dashboard" :percentage="26.5" :color="colors" />
                                <div class="storage-info">
                                    <span>已用 765GB / 共 1TB</span>
                                </div>
                            </el-card>
                        </el-col>

                        <el-col :span="16">
                            <el-card class="recent-activity">
                                <template #header>
                                    <div class="card-header">
                                        <span>最近活动</span>
                                    </div>
                                </template>
                                <el-timeline>
                                    <el-timeline-item v-for="(activity, index) in activities" :key="index"
                                        :timestamp="activity.timestamp">
                                        {{ activity.content }}
                                    </el-timeline-item>
                                </el-timeline>
                            </el-card>
                        </el-col>
                    </el-row>
                </el-main>
            </el-container>
        </el-container>

        <el-dialog v-model="broadcastInfo.broadcastTableVisible" title="公告" width="800">
            <el-table :data="broadcastInfo.broadcastGrip" :row-class-name="broadcastInfo.broadcastGripRowClassName"
                height="400">
                <el-table-column property="createdAt" label="发布时间" width="120" />
                <el-table-column property="message" label="内容" width="500" />
                <el-table-column property="publishUserName" label="发布者" width="120" />
            </el-table>
            <div v-if="broadcastInfo.showFormButton">
                <el-button type="primary" @click="broadcastFormShow"> 发布 </el-button>
            </div>
        </el-dialog>

        <el-dialog v-model="broadcastFormInfo.broadcastFormVisible" title="发布" width="500">
            <el-form ref="broadcastFormRef" :model="broadcastFormInfo.broadcastForm" :rules="broadcastFormInfo.rules">
                <el-form-item label="内容" prop="message">
                    <el-input v-model="broadcastFormInfo.broadcastForm.message" type="textarea" placeholder="输入公告消息"
                        clearable />
                </el-form-item>
            </el-form>
            <template #footer>
                <div class="dialog-footer">
                    <el-button type="primary" :loading="broadcastFormInfo.loading"
                        @click="broadcastFormSubmit(broadcastFormRef)">
                        提交
                    </el-button>
                </div>
            </template>
        </el-dialog>
    </div>
</template>

<style lang="scss" scoped>
.home-container {
    height: 100vh;

    .sidebar {
        background-color: #304156;

        .logo {
            height: 60px;
            line-height: 60px;
            text-align: center;
            color: #fff;
            font-size: 18px;
            background-color: #2b2f3a;
        }

        .vertical-menu {
            border-right: none;
        }
    }

    .header {
        display: flex;
        align-items: center;
        background-color: #fff;
        box-shadow: 0 1px 4px rgba(0, 21, 41, 0.08);
        justify-content: flex-end;

        .header-nav {
            padding: 10px;

            .el-dropdown-link {
                display: flex;
                align-items: center;
                cursor: pointer;

                .username {
                    margin: 0 8px;
                    font-size: 14px;
                }
            }
        }
    }

    .main-content {
        background-color: #f0f2f5;

        .stats-card {
            .storage-info {
                text-align: center;
                margin-top: 20px;
                color: #909399;
            }
        }

        .recent-activity {
            height: 450px;
        }

        .card-header {
            font-weight: 600;
            color: #303133;
        }
    }
}
</style>

<style>
.el-table .warning-row {
    --el-table-tr-bg-color: var(--el-color-warning-light-9);
}
</style>