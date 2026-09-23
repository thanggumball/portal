import { useAuth } from '@/hooks/auth.hook'
import { useContext, useState } from 'react'
import { AppContext } from '@/contexts/AppContext'
import { useUsers } from '@/hooks/user.hook'

export default function Login() {
    const { login, loading } = useAuth()
    const { setIsAuthenticated, setUser } = useContext(AppContext)
    const { getMe } = useUsers()

    const [email, setEmail] = useState('')
    const [password, setPassword] = useState('')

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()

        const data = await login(email, password)

        if (!data) return

        const user = await getMe()

        if (!user) return

        setUser(user)
        setIsAuthenticated(true)
    }

    return (
        <form onSubmit={handleSubmit}>
            <input
                value={email}
                onChange={(e) => setEmail(e.target.value)}
            />

            <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
            />

            <button disabled={loading}>
                {loading ? 'Đang đăng nhập...' : 'Đăng nhập'}
            </button>

        </form>
    )
}
