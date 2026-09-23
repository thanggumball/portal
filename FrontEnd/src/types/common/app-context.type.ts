
export interface AppContextInterface {
    isAuthenticated: boolean;
    setIsAuthenticated: (isAuthenticated: boolean) => void;
    resetAuth: () => void;
}