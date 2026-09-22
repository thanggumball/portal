import type { SuccessResponse } from "./utils.type";

export type AuthResponse = SuccessResponse<{
    accessToken: string;
    refreshToken: string;
    expiresAt: string;
}>;

export type RefreshTokenResponse = SuccessResponse<{
    accessToken: string;
    refreshToken: string;
    expiresAt: string;
}>;

export type JwtPayload = {
    sub: string;
    email: string;
    role: string;
    exp: number;
    iat?: number;
};