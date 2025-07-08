import axios from "axios"
import type { authenticationDtoInter, authenticationExtendDtoInter, authenticationResponseInter, forgotPasswordTokenDtoInter, forgotPasswordTokenExtendDtoInter, refreshAccessTokenResponseInter, registerDtoInter, resetPasswordDtoInter } from "@/types"

export default function () {
    // const authDomain = import.meta.env.VITE_AUTH_DOMAIN as string
    // const callbackDomain = import.meta.env.VITE_WEB_DOMAIN as string
    const audience = 'smart_home'
    const emailQueryParam = 'email'
    const tokenQueryParam = 'token'
    const registerPath = '/api/v1/account/register'
    const authenticationPath = '/api/v1/account/jwttoken'
    const refreshAccessTokenPath = '/api/v1/account/refreshaccessToken'
    const forgotPasswordTokenPath = '/api/v1/account/forgotpasswordToken'
    // const forgotPasswordCallbackPath = '/resetpassword'
    const resetPasswordPath = '/api/v1/account/resetpassword'

    async function register(dto: registerDtoInter): Promise<void> {
        // await axios.post(`${authDomain}${registerPath}`, dto)
        await axios.post(registerPath, dto)
    }

    async function authenticate(dto: authenticationDtoInter): Promise<authenticationResponseInter> {
        let requestDto: authenticationExtendDtoInter = {
            audience,
            nameOrEmail: dto.nameOrEmail,
            password: dto.password
        }
        // let { data: { name, accessToken, refreshToken } } = await axios.put(`${authDomain}${authenticationPath}`, requestDto)
        let { data: { name, accessToken, refreshToken } } = await axios.put(authenticationPath, requestDto)
        return { name, accessToken, refreshToken }
    }

    async function refreshAccessToken(refreshToken: string): Promise<refreshAccessTokenResponseInter> {
        // let { data: { accessToken } } = await axios.get(`${authDomain}${refreshAccessTokenPath}?refreshtoken=${refreshToken}`);
        let { data: { accessToken } } = await axios.get(`${refreshAccessTokenPath}?refreshtoken=${refreshToken}`);
        return { accessToken }
    }

    async function forgotPasswordToken(dto: forgotPasswordTokenDtoInter): Promise<string> {
        let requestDto: forgotPasswordTokenExtendDtoInter = {
            // callbackUrl: `${callbackDomain}${forgotPasswordCallbackPath}`,
            callbackUrl: `${window.location.origin}/resetpassword`,
            nameOrEmail: dto.nameOrEmail,
            emailQueryParam: emailQueryParam,
            tokenQueryParam: tokenQueryParam
        }
        // let { data } = await axios.put(`${authDomain}${forgotPasswordTokenPath}`, requestDto)
        let { data } = await axios.put(forgotPasswordTokenPath, requestDto)
        return data as string
    }

    async function resetPassword(dto: resetPasswordDtoInter): Promise<void> {
        // await axios.put(`${authDomain}${resetPasswordPath}`, dto)
        await axios.put(resetPasswordPath, dto)
    }

    return { register, authenticate, refreshAccessToken, forgotPasswordToken, resetPassword, emailQueryParam, tokenQueryParam }
}