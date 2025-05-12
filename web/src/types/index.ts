export interface registerDtoInter {
    email: string
    name: string
    password: string
    confirmPassword: string
    dateOfBirth: string
}

export interface authenticationDtoInter {
    nameOrEmail: string
    password: string
}

export interface authenticationExtendDtoInter extends authenticationDtoInter {
    audience: string
}

export interface authenticationResponseInter {
    name: string
    accessToken: string
    refreshToken: string
}

export interface refreshAccessTokenResponseInter {
    accessToken: string
}

export interface forgotPasswordTokenDtoInter {
    nameOrEmail: string
}

export interface forgotPasswordTokenExtendDtoInter extends forgotPasswordTokenDtoInter {
    callbackUrl: string
    emailQueryParam: string
    tokenQueryParam: string
}

export interface resetPasswordDtoInter {
    email: string
    password: string
    confirmPassword: string
    token: string
}

export interface userInfoResponseInter {
    name: string
}