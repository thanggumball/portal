import { useState } from "react";
import authApi from "@/apis/auth.api";
import type { AuthResponse } from "@/types/auth.type";

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

            return response.data;
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
