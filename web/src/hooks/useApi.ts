import axios from "axios"
import { useRouter } from 'vue-router'
import { useUserStore } from "@/store/useUserStore"
import { type userInfoResponseInter, type broadcastResponseInter } from "@/types"
import useAuthentication from "./useAuthentication"

export default function () {
    const apiAxios = axios.create({
        timeout: 10000
    })
    const { refreshAccessToken } = useAuthentication()
    const { getAccessToken, getRefreshToken, setAccessToken, logout } = useUserStore()
    const router = useRouter()
    apiAxios.interceptors.request.use(config => {
        let accessToken = getAccessToken()
        if (accessToken) {
            config.headers.Authorization = `Bearer ${accessToken}`
        }
        return config
    }, error => {
        return Promise.reject(error)
    })
    apiAxios.interceptors.response.use(response => {
        return response
    }, async error => {
        const originalRequest = error.config
        if (error.response.status === 401) {
            try {
                let refreshToken = getRefreshToken()
                if (refreshToken) {
                    let { accessToken } = await refreshAccessToken(refreshToken)
                    setAccessToken(accessToken)
                    originalRequest.headers.Authorization = `Bearer ${accessToken}`
                    return axios(originalRequest)
                }
            } catch (error) {
                logout()
                ElMessage({
                    message: '身份过期，请重新登录',
                    type: 'warning',
                })
                router.push('/login')
            }
        }
        return Promise.reject(error)
    })

    async function userInfo(): Promise<userInfoResponseInter> {
        let { data: { id, name, email, dateOfBirth, roles } } = await apiAxios.get('/api/v1/user/info')
        var dateOfBirthDate = new Date(0)
        if (!isNaN(Date.parse(dateOfBirth))) {
            dateOfBirthDate = new Date(dateOfBirth)
        }
        return { id, name, email, dateOfBirth, roles }
    }

    async function getBroadcasts(): Promise<broadcastResponseInter[]> {
        let { data } = await apiAxios.get('/api/v1/broadcast')
        return data.map((item: any) => {
            var createdAt = new Date(0)
            if (!isNaN(Date.parse(item.createdAt))) {
                createdAt = new Date(item.createdAt)
            }
            return {
                publishUserName: item.publishUserName,
                message: item.message,
                createdAt: createdAt,
                isRead: item.isRead
            }
        })
    }

    async function broadcast(message: string): Promise<void> {
        await apiAxios.post('/api/v1/broadcast', { message })
    }

    return { userInfo, getBroadcasts, broadcast }
}