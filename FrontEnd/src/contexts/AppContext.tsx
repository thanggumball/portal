
import type { AppContextInterface } from '@/types/common/app-context.type'
import { clearLocalStorage, getAccessTokenFromLocalStorage, LocalStorageEventTarget } from '@/utils/auth'
import {
    createContext,
    useEffect,
    useState,
    type ReactNode
} from 'react'

const initialAppContext: AppContextInterface = {
    isAuthenticated: Boolean(getAccessTokenFromLocalStorage()),
    setIsAuthenticated: () => { },
    resetAuth: () => { }
}

export const AppContext =
    createContext<AppContextInterface>(initialAppContext)

export const AppProvider = ({ children }: { children: ReactNode }) => {
    const [isAuthenticated, setIsAuthenticated] = useState(
        initialAppContext.isAuthenticated
    )

    const resetAuth = () => {
        clearLocalStorage()
        setIsAuthenticated(false)
    }

    useEffect(() => {
        const handleClearLS = () => {
            setIsAuthenticated(false)
        }

        LocalStorageEventTarget.addEventListener('clearLS', handleClearLS)

        return () => {
            LocalStorageEventTarget.removeEventListener('clearLS', handleClearLS)
        }
    }, [])

    return (
        <AppContext.Provider
            value={{
                isAuthenticated,
                setIsAuthenticated,
                resetAuth
            }}
        >
            {children}
        </AppContext.Provider>
    )
}
