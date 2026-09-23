import http from '@/lib/http/http';
import type { ApiResponse, PagedResult } from '@/types/shared.type';
import type { AuditLogDetail, AuditLogFilterParams, AuditLogListItem } from '@/types/auditLog.type';

export const URL_AUDIT_LOG = {
  SEARCH: '/audit-logs',
  DETAIL: (id: string) => `/audit-logs/${id}`,
};

const auditLogApi = {
  search: (params: AuditLogFilterParams) =>
    http.get<ApiResponse<PagedResult<AuditLogListItem>>>(URL_AUDIT_LOG.SEARCH, { params }),
  getById: (id: string) =>
    http.get<ApiResponse<AuditLogDetail>>(URL_AUDIT_LOG.DETAIL(id)),
};

export default auditLogApi;
