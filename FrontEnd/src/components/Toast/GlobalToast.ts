import type { ReactNode } from 'react'

export type ToastType = 'success' | 'warning' | 'error'

export interface ToastItem {
    id: string
    type: ToastType
    message: ReactNode
    duration: number
}

type Listener = (toasts: ToastItem[]) => void

let toasts: ToastItem[] = []

const listeners = new Set<Listener>()
const timers = new Map<string, number>()

const emit = () => {
    const snapshot = [...toasts]

    listeners.forEach((listener) => {
        listener(snapshot)
    })
}

const remove = (id: string) => {
    const timer = timers.get(id)

    if (timer) {
        window.clearTimeout(timer)
        timers.delete(id)
    }

    toasts = toasts.filter((toast) => toast.id !== id)

    emit()
}

const show = (
    type: ToastType,
    message: ReactNode,
    duration = 4000
) => {
    const id = crypto.randomUUID()

    toasts = [
        ...toasts,
        {
            id,
            type,
            message,
            duration
        }
    ]

    emit()

    if (duration > 0) {
        const timer = window.setTimeout(() => {
            remove(id)
        }, duration)

        timers.set(id, timer)
    }
}

export const GlobalToast = {
    success(message: ReactNode, duration?: number) {
        show('success', message, duration)
    },

    warning(message: ReactNode, duration?: number) {
        show('warning', message, duration)
    },

    error(message: ReactNode, duration?: number) {
        show('error', message, duration)
    },

    dismiss(id: string) {
        remove(id)
    },

    dismissAll() {
        timers.forEach((timer) => {
            window.clearTimeout(timer)
        })

        timers.clear()
        toasts = []

        emit()
    }
}

export const GlobalToastStore = {
    getState() {
        return [...toasts]
    },

    subscribe(listener: Listener) {
        listeners.add(listener)

        listener([...toasts])

        return () => {
            listeners.delete(listener)
        }
    }
}
