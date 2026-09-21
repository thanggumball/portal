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