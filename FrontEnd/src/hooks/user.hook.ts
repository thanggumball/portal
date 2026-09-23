import userApi from '@/apis/user.api'
import { useState } from 'react'

export const useUser = () => {
    const [loading, setLoading] = useState(false)
    const [error, setError] = useState<unknown>(null)

    const getMe = async () => {
        try {
            setLoading(true)
            setError(null)

            const response = await userApi.getMe()

            return response.data.data
        } catch (error) {
            setError(error)
            return null
        } finally {
            setLoading(false)
        }
    }

    return {
        getMe,
        loading,
        error
    }
}
