import http from '@/lib/http/http';
import type { ApiResponse, PagedResult } from '@/types/shared.type';
import type { UserFilterParams, UserResponse } from '@/types/user.type';

export const URL_USER = {
  SEARCH: '/users',
};

const userApi = {
  search: (params: UserFilterParams) =>
    http.get<ApiResponse<PagedResult<UserResponse>>>(URL_USER.SEARCH, { params }),
};

export default userApi;