import { useState } from "react";
import authApi from "@/apis/auth.api";
import type { AuthResponse } from "@/types/auth.type";
import {
    setAccessTokenToLocalStorage,
    setRefreshTokenToLocalStorage,
    clearLocalStorage,
} from "@/utils/auth";

export const useAuth = () => {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<unknown>(null);

    const login = async (
        email: string,
        password: string
    ): Promise<AuthResponse | null> => {
        try {
            setLoading(true);
            setError(null);

            const response = await authApi.login({
                email,
                password,
            });

            const data = response.data;

            setAccessTokenToLocalStorage(data.data.accessToken);
            setRefreshTokenToLocalStorage(data.data.refreshToken);

            return data;
        } catch (error) {
            setError(error);
            return null;
        } finally {
            setLoading(false);
        }
    };

    const logout = async () => {
        try {
            setLoading(true);
            setError(null);

            await authApi.logout();
        } catch (error) {
            setError(error);
        } finally {
            clearLocalStorage();
            setLoading(false);
        }
    };

    return {
        login,
        logout,
        loading,
        error,
    };
};