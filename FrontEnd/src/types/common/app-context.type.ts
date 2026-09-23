import type { User } from "../user.type";

export interface AppContextInterface {
    user: User | null;
    setUser: (user: User | null) => void
    isLoading: boolean;
    isAuthenticated: boolean;
    setIsAuthenticated: (isAuthenticated: boolean) => void;
    resetAuth: () => void;
}