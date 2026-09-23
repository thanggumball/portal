import http from '@/lib/http/http';
import type { ApiResponse } from '@/types/shared.type';
import type { RoleResponse } from '@/types/role.type';

export const URL_ROLE = {
  GET_ALL: '/roles',
};

const roleApi = {
  getAll: () => http.get<ApiResponse<RoleResponse[]>>(URL_ROLE.GET_ALL),
};

export default roleApi;