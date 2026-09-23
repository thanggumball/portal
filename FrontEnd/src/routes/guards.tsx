import path from "@/constants/path"
import { AppContext } from "@/contexts/AppContext"
import { useContext } from "react"
import { Navigate, Outlet } from "react-router"


export function ProtectedRoute() {
    const { isAuthenticated } = useContext(AppContext)
    return isAuthenticated ? <Outlet /> : <Navigate to={path.login} />
}


export function RejectedRoute() {
    const { isAuthenticated } = useContext(AppContext)
    return !isAuthenticated ? <Outlet /> : <Navigate to={path.users.manage} />
}