import axios from "axios"
import type { authenticationDtoInter, authenticationExtendDtoInter, authenticationResponseInter, forgotPasswordTokenDtoInter, forgotPasswordTokenExtendDtoInter, refreshAccessTokenResponseInter, registerDtoInter, resetPasswordDtoInter } from "@/types"

export default function () {
    const authDomain = 'https://localhost:5151'
    const callbackDomain = 'http://localhost:5173'
    const audience = 'smart_home'
    const emailQueryParam = 'email'
    const tokenQueryParam = 'token'
    const registerPath = '/Account/Register'
    const authenticationPath = '/Account/JwtToken'
    const refreshAccessTokenPath = '/Account/RefreshAccessToken'
    const forgotPasswordTokenPath = '/Account/ForgotPasswordToken'
    const forgotPasswordCallbackPath = '/resetPassword'
    const resetPasswordPath = '/Account/ResetPassword'

    async function register(dto: registerDtoInter): Promise<void> {
        await axios.post(`${authDomain}${registerPath}`, dto)
    }

    async function authenticate(dto: authenticationDtoInter): Promise<authenticationResponseInter> {
        let requestDto: authenticationExtendDtoInter = {
            audience,
            nameOrEmail: dto.nameOrEmail,
            password: dto.password
        }
        let { data: { name, accessToken, refreshToken } } = await axios.put(`${authDomain}${authenticationPath}`, requestDto)
        return { name, accessToken, refreshToken }
    }

    async function refreshAccessToken(refreshToken: string): Promise<refreshAccessTokenResponseInter> {
        let { data: { accessToken } } = await axios.get(`${authDomain}${refreshAccessTokenPath}?refreshToken=${refreshToken}`);
        return { accessToken }
    }

    async function forgotPasswordToken(dto: forgotPasswordTokenDtoInter): Promise<string> {
        let requestDto: forgotPasswordTokenExtendDtoInter = {
            callbackUrl: `${callbackDomain}${forgotPasswordCallbackPath}`,
            nameOrEmail: dto.nameOrEmail,
            emailQueryParam: emailQueryParam,
            tokenQueryParam: tokenQueryParam
        }
        let { data } = await axios.put(`${authDomain}${forgotPasswordTokenPath}`, requestDto)
        return data as string
    }

    async function resetPassword(dto: resetPasswordDtoInter): Promise<void> {
        await axios.put(`${authDomain}${resetPasswordPath}`, dto)
    }

    return { register, authenticate, refreshAccessToken, forgotPasswordToken, resetPassword, emailQueryParam, tokenQueryParam }
}