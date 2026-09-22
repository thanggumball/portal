import {
    useEffect,
    useState,
    type ReactNode
} from 'react'

import { Toast } from './Toast'
import {
    GlobalToast,
    GlobalToastStore,
    type ToastItem
} from './GlobalToast'

interface ToastProviderProps {
    children: ReactNode
}

export function ToastProvider({
    children
}: ToastProviderProps) {
    const [toasts, setToasts] = useState<ToastItem[]>([])

    useEffect(() => {
        return GlobalToastStore.subscribe(setToasts)
    }, [])

    return (
        <>
            {children}

            <div className="toast-container">
                {toasts.map((item) => (
                    <Toast
                        key={item.id}
                        toast={item}
                        onClose={GlobalToast.dismiss}
                    />
                ))}
            </div>
        </>
    )
}
