import { jwtDecode } from "jwt-decode";

const ACCESS_TOKEN_KEY = 'token';
const REFRESH_TOKEN_KEY = 'refreshToken';

export const LocalStorageEventTarget = new EventTarget()

export const setAccessTokenToLocalStorage = (token: string) => {
    localStorage.setItem(ACCESS_TOKEN_KEY, token);
};

export const setRefreshTokenToLocalStorage = (token: string) => {
    localStorage.setItem(REFRESH_TOKEN_KEY, token);
};

export const clearLocalStorage = () => {
    localStorage.removeItem(ACCESS_TOKEN_KEY)
    localStorage.removeItem(REFRESH_TOKEN_KEY)
    LocalStorageEventTarget.dispatchEvent(new Event('clearLS'))
}

export const getAccessTokenFromLocalStorage = () => {
    return localStorage.getItem(ACCESS_TOKEN_KEY);
};

export const getRefreshTokenFromLocalStorage = () => {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
};

export const removeRefreshTokenFromLocalStorage = () => {
    localStorage.removeItem(REFRESH_TOKEN_KEY);
};

type JwtPayload = {
    sub: string;
    email: string;
    role?: string;
    exp: number;
    iat?: number;
    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"?: string;
};

export const getCurrentUserFromToken = () => {
    const token = getAccessTokenFromLocalStorage();

    if (!token) {
        return null;
    }

    try {
        const decoded = jwtDecode<JwtPayload>(token);

        return {
            id: decoded.sub,
            email: decoded.email,
            role:
                decoded.role ??
                decoded[
                    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                ] ??
                "",
            expiresAt: decoded.exp,
        };
    } catch {
        return null;
    }
};