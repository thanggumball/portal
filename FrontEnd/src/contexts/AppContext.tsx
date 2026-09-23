import type { AppContextInterface } from '@/types/common/app-context.type'
import { useUsers } from '@/hooks/user.hook'
import {
    clearLocalStorage,
    getAccessTokenFromLocalStorage,
    LocalStorageEventTarget
} from '@/utils/auth'
import {
    createContext,
    useEffect,
    useState,
    type ReactNode
} from 'react'

const initialAppContext: AppContextInterface = {
    user: null,
    isAuthenticated: false,
    isLoading: true,
    setUser: () => { },
    setIsAuthenticated: () => { },
    resetAuth: () => { }
}

export const AppContext =
    createContext<AppContextInterface>(initialAppContext)

export const AppProvider = ({ children }: { children: ReactNode }) => {
    const [user, setUser] = useState<AppContextInterface['user']>(null)

    const [isAuthenticated, setIsAuthenticated] = useState(false)

    const [isLoading, setIsLoading] = useState(true)

    const { getMe } = useUsers()

    const resetAuth = () => {
        clearLocalStorage()

        setUser(null)
        setIsAuthenticated(false)
    }

    // ================= INIT AUTH =================

    useEffect(() => {
        const initAuth = async () => {
            const accessToken = getAccessTokenFromLocalStorage()

            // Không có token → chưa login
            if (!accessToken) {
                setIsLoading(false)
                return
            }

            try {
                const user = await getMe()

                if (!user) {
                    setUser(null)
                    setIsAuthenticated(false)
                    return
                }

                setUser(user)
                setIsAuthenticated(true)
            } finally {
                setIsLoading(false)
            }
        }

        initAuth()
    }, [])

    // ================= CLEAR AUTH =================

    useEffect(() => {
        const handleClearLS = () => {
            setUser(null)
            setIsAuthenticated(false)
        }

        LocalStorageEventTarget.addEventListener(
            'clearLS',
            handleClearLS
        )

        return () => {
            LocalStorageEventTarget.removeEventListener(
                'clearLS',
                handleClearLS
            )
        }
    }, [])

    return (
        <AppContext.Provider
            value={{
                user,
                isAuthenticated,
                isLoading,
                setUser,
                setIsAuthenticated,
                resetAuth
            }}
        >
            {children}
        </AppContext.Provider>
    )
}
