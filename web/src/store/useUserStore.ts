import { defineStore } from "pinia"

export const useUserStore = defineStore('user', () => {
    let nameKey = 'smart_home_name'
    let accessTokenKey = 'smart_home_access_token'
    let refreshTokenKey = 'smart_home_refresh_token'

    function isLoggedIn() {
        return (localStorage.getItem(nameKey) && localStorage.getItem(nameKey) !== ''
            && localStorage.getItem(accessTokenKey) && localStorage.getItem(accessTokenKey) !== ''
            && localStorage.getItem(refreshTokenKey) && localStorage.getItem(refreshTokenKey) !== '')
            || (sessionStorage.getItem(nameKey) && sessionStorage.getItem(nameKey) !== ''
                && sessionStorage.getItem(accessTokenKey) && sessionStorage.getItem(accessTokenKey) !== ''
                && sessionStorage.getItem(refreshTokenKey) && sessionStorage.getItem(refreshTokenKey) !== '')
    }

    function login(name: string, accessToken: string, refreshToken: string, remember: boolean) {
        if (remember) {
            sessionStorage.removeItem(nameKey)
            sessionStorage.removeItem(accessTokenKey)
            sessionStorage.removeItem(refreshTokenKey)
            localStorage.setItem(nameKey, name)
            localStorage.setItem(accessTokenKey, accessToken)
            localStorage.setItem(refreshTokenKey, refreshToken)
        } else {
            localStorage.removeItem(nameKey)
            localStorage.removeItem(accessTokenKey)
            localStorage.removeItem(refreshTokenKey)
            sessionStorage.setItem(nameKey, name)
            sessionStorage.setItem(accessTokenKey, accessToken)
            sessionStorage.setItem(refreshTokenKey, refreshToken)
        }
    }

    function getName(): string | null {
        let name = sessionStorage.getItem(nameKey)
        if (name && name !== '') {
            return name
        }
        return localStorage.getItem(nameKey)
    }

    function getAccessToken(): string | null {
        let accessToken = sessionStorage.getItem(accessTokenKey)
        if (accessToken) {
            return accessToken
        }
        return localStorage.getItem(accessTokenKey)
    }

    function getRefreshToken(): string | null {
        let refreshToken = sessionStorage.getItem(refreshTokenKey)
        if (refreshToken) {
            return refreshToken
        }
        return localStorage.getItem(refreshTokenKey)
    }

    function setAccessToken(accessToken: string) {
        if (sessionStorage.getItem(accessTokenKey)) {
            sessionStorage.setItem(accessTokenKey, accessToken)
        }
        if (localStorage.getItem(accessTokenKey)) {
            localStorage.setItem(accessTokenKey, accessToken)
        }
    }

    function logout() {
        sessionStorage.removeItem(nameKey)
        sessionStorage.removeItem(accessTokenKey)
        sessionStorage.removeItem(refreshTokenKey)
        localStorage.removeItem(nameKey)
        localStorage.removeItem(accessTokenKey)
        localStorage.removeItem(refreshTokenKey)
    }

    // let broadcasts = reactive<broadcastResponseInter[]>([])
    // let achivedBroadcasts = false

    // async function getBroadcasts(): Promise<broadcastResponseInter[]> {
    //     if (achivedBroadcasts) {
    //         return broadcasts
    //     }
    //     let { getBroadcasts } = useApi()
    //     broadcasts = await getBroadcasts()
    //     achivedBroadcasts = true
    //     return broadcasts
    // }

    return { isLoggedIn, login, getName, getAccessToken, getRefreshToken, setAccessToken, logout }
})