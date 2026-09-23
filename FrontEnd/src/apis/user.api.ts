import http from "@/lib/http/http";

export const URL_AUTH = {
    ME: '/profile',
}

const userApi = {
    getMe: () => {
        return http.get(URL_AUTH.ME)
    }
};

export default userApi;