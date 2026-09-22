import { URL_AUTH } from '@/apis/auth.api'
import { GlobalToast } from '@/components/Toast/GlobalToast'
import config from '@/constants/config'
import HttpStatusCode from '@/constants/httpStatuscode'
import type { AuthResponse, RefreshTokenResponse } from '@/types/auth.type'
import type { ErrorResponse } from '@/types/utils.type'
import { clearLocalStorage, getAccessTokenFromLocalStorage, getRefreshTokenFromLocalStorage, setAccessTokenToLocalStorage, setRefreshTokenToLocalStorage } from '@/utils/auth'
import { isAxiosExpiredTokenError, isAxiosUnauthorizedError } from '@/utils/axiosError'
import axios, {
    type AxiosError,
    type AxiosInstance,
    type AxiosResponse
} from 'axios'


class Http {
    instance: AxiosInstance
    private refreshTokenRequest: Promise<string> | null = null

    constructor() {
        this.instance = axios.create({
            baseURL: config.baseUrl,
            timeout: 10000,
            headers: {
                'Content-Type': 'application/json'
            }
        })

        console.log('🔗 API Base URL:', this.instance.defaults.baseURL)

        this.setupRequestInterceptor()
        this.setupResponseInterceptor()
    }

    // ================= REQUEST =================

    private setupRequestInterceptor() {
        this.instance.interceptors.request.use(
            (config) => {
                const accessToken = getAccessTokenFromLocalStorage()

                if (accessToken) {
                    config.headers.Authorization = `Bearer ${accessToken}`
                }

                return config
            },
            (error) => Promise.reject(error)
        )
    }

    // ================= RESPONSE =================

    private setupResponseInterceptor() {
        this.instance.interceptors.response.use(
            (response) => {
                this.handleSuccessResponse(response)

                return response
            },
            (error: AxiosError) => this.handleResponseError(error)
        )
    }

    private handleSuccessResponse(response: AxiosResponse) {
        const { url } = response.config

        // LOGIN
        if (url === URL_AUTH.LOGIN) {
            const data = response.data as AuthResponse

            const {
                accessToken,
                refreshToken,
            } = data.data

            setAccessTokenToLocalStorage(accessToken)
            setRefreshTokenToLocalStorage(refreshToken)
        }

        // LOGOUT
        if (url === URL_AUTH.LOGOUT) {
            clearLocalStorage()
        }
    }

    // ================= ERROR =================

    private async handleResponseError(error: AxiosError) {
        const status = error.response?.status

        // Những lỗi này để component tự xử lý
        const ignoreToastStatus = new Set<number>([
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.Unauthorized
        ])

        if (!ignoreToastStatus.has(status ?? 0)) {
            const data = error.response?.data as ErrorResponse<unknown> | undefined

            GlobalToast.error(data?.message || error.message)
        }

        // ================= 401 =================

        if (isAxiosUnauthorizedError(error)) {
            const originalRequest = error.response?.config

            if (!originalRequest) {
                return Promise.reject(error)
            }

            const { url } = originalRequest

            // Token expired → refresh
            if (
                isAxiosExpiredTokenError(error) &&
                url !== URL_AUTH.REFRESH_TOKEN
            ) {
                this.refreshTokenRequest =
                    this.refreshTokenRequest ||
                    this.handleRefreshToken().finally(() => {
                        this.refreshTokenRequest = null
                    })

                return this.refreshTokenRequest.then((accessToken) => {
                    return this.instance({
                        ...originalRequest,
                        headers: {
                            ...originalRequest.headers,
                            Authorization: `Bearer ${accessToken}`
                        }
                    })
                })
            }

            // 401 khác → logout
            clearLocalStorage()

            GlobalToast.error(
                (error.response?.data as ErrorResponse<unknown>)?.message ||
                'Unauthorized'
            )
        }

        return Promise.reject(error)
    }

    // ================= REFRESH TOKEN =================

    private async handleRefreshToken(): Promise<string> {
        const refreshToken = getRefreshTokenFromLocalStorage()

        if (!refreshToken) {
            clearLocalStorage()
            throw new Error('Refresh token not found')
        }

        try {
            const response = await this.instance.post<RefreshTokenResponse>(
                URL_AUTH.REFRESH_TOKEN,
                {
                    refreshToken
                }
            )

            const {
                accessToken,
                refreshToken: newRefreshToken
            } = response.data.data

            // Lưu cả AT mới
            setAccessTokenToLocalStorage(accessToken)

            // Lưu RT mới vì BE đang rotation refresh token
            setRefreshTokenToLocalStorage(newRefreshToken)

            return accessToken
        } catch (error) {
            clearLocalStorage()
            throw error
        }
    }
}

const http = new Http().instance

export default http
