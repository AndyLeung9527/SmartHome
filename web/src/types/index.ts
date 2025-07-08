export interface callbackDtoInter {
    callbackUrl: string
    emailQueryParam: string
    tokenQueryParam: string
}

export interface registerDtoInter {
    email: string
    name: string
    password: string
    confirmPassword: string
    dateOfBirth: string
}

export interface registerExtendDtoInter extends registerDtoInter, callbackDtoInter { }

export interface confirmEmailDtoInter {
    email: string
    token: string
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

export interface forgotPasswordDtoInter {
    nameOrEmail: string
}

export interface forgotPasswordExtendDtoInter extends forgotPasswordDtoInter, callbackDtoInter { }

export interface resetPasswordDtoInter {
    email: string
    password: string
    confirmPassword: string
    token: string
}

export interface userInfoResponseInter {
    id: string,
    name: string,
    email: string,
    dateOfBirth: Date,
    roles: string[]
}

export interface broadcastResponseInter {
    publishUserName: string,
    message: string,
    createdAt: Date,
    isRead: boolean
}