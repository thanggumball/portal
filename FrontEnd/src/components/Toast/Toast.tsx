import type { ReactNode } from "react";
import {
    CheckCircle2,
    CircleAlert,
    XCircle,
    X,
} from "lucide-react";

import "./Toast.scss";

export type ToastType = "success" | "warning" | "error";

export interface ToastItem {
    id: string;
    type: ToastType;
    message: ReactNode;
    duration?: number;
}

interface ToastProps {
    toast: ToastItem;
    onClose: (id: string) => void;
}

const icons = {
    success: CheckCircle2,
    warning: CircleAlert,
    error: XCircle,
};

export function Toast({ toast, onClose }: ToastProps) {
    const Icon = icons[toast.type];

    return (
        <div
            className={`toast toast--${toast.type}`}
            role="alert"
        >
            <Icon className="toast__icon" size={20} />

            <div className="toast__content">
                <span className="toast__message">{toast.message}</span>
            </div>

            <button
                className="toast__close"
                onClick={() => onClose(toast.id)}
                aria-label="Close notification"
            >
                <X size={16} />
            </button>

            {toast.duration !== 0 && (
                <div
                    className="toast__progress"
                    style={{
                        animationDuration: `${toast.duration ?? 4000}ms`,
                    }}
                />
            )}
        </div>
    );
}
