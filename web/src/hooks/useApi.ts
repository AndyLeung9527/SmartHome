import axios from "axios"
import { useRouter } from 'vue-router'
import { useUserStore } from "@/store/useUserStore"
import { type userInfoResponseInter } from "@/types"
import useAuthentication from "./useAuthentication"

export default function () {
    const apiDomain = 'https://localhost:5050'
    const apiAxios = axios.create({
        baseURL: apiDomain,
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
        var { data: { name } } = await apiAxios.get('/User/Info')
        return { name }
    }

    return { userInfo }
}