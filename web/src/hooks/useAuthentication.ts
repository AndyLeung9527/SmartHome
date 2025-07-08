import axios from "axios"
import type { authenticationDtoInter, authenticationExtendDtoInter, authenticationResponseInter, forgotPasswordExtendDtoInter, forgotPasswordDtoInter, refreshAccessTokenResponseInter, registerDtoInter, registerExtendDtoInter, confirmEmailDtoInter, resetPasswordDtoInter } from "@/types"

export default function () {
    const audience = 'smart_home'
    const emailQueryParam = 'email'
    const tokenQueryParam = 'token'
    const registerPath = '/api/v1/account/register'
    const confirmEmailPath = '/api/v1/account/confirmemail'
    const authenticationPath = '/api/v1/account/jwttoken'
    const refreshAccessTokenPath = '/api/v1/account/refreshaccessToken'
    const forgotPasswordPath = '/api/v1/account/forgotpassword'
    const resetPasswordPath = '/api/v1/account/resetpassword'

    async function register(dto: registerDtoInter): Promise<void> {
        let requestDto: registerExtendDtoInter = {
            callbackUrl: `${window.location.origin}/confirmemail`,
            emailQueryParam: emailQueryParam,
            tokenQueryParam: tokenQueryParam,
            ...dto
        }
        await axios.post(registerPath, requestDto)
    }

    async function confirmEmail(dto: confirmEmailDtoInter) {
        await axios.put(confirmEmailPath, dto)
    }

    async function authenticate(dto: authenticationDtoInter): Promise<authenticationResponseInter> {
        let requestDto: authenticationExtendDtoInter = {
            audience,
            nameOrEmail: dto.nameOrEmail,
            password: dto.password
        }
        let { data: { name, accessToken, refreshToken } } = await axios.put(authenticationPath, requestDto)
        return { name, accessToken, refreshToken }
    }

    async function refreshAccessToken(refreshToken: string): Promise<refreshAccessTokenResponseInter> {
        let { data: { accessToken } } = await axios.get(`${refreshAccessTokenPath}?refreshtoken=${refreshToken}`);
        return { accessToken }
    }

    async function forgotPassword(dto: forgotPasswordDtoInter): Promise<string> {
        let requestDto: forgotPasswordExtendDtoInter = {
            callbackUrl: `${window.location.origin}/resetpassword`,
            emailQueryParam: emailQueryParam,
            tokenQueryParam: tokenQueryParam,
            ...dto
        }
        let { data } = await axios.put(forgotPasswordPath, requestDto)
        return data as string
    }

    async function resetPassword(dto: resetPasswordDtoInter): Promise<void> {
        await axios.put(resetPasswordPath, dto)
    }

    return { register, confirmEmail, authenticate, refreshAccessToken, forgotPassword, resetPassword, emailQueryParam, tokenQueryParam }
}