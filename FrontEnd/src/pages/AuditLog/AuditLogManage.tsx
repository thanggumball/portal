import { useMemo } from 'react';
import dayjs from 'dayjs';
import { DataTable, DataTableHeader, TablePagination } from '@/components/common/Common';
import type { Attribute, Filter } from '@/types/components/attribute';
import { useAuditLogs } from '@/hooks/auditLog.hook';
import {
  AUDIT_ACTION_OPTIONS,
  AUDIT_ENTITY_OPTIONS,
  type AuditLogFilterParams,
} from '@/types/auditLog.type';
import path from '@/constants/path';

const auditLogFilterMapper: Attribute[] = [
  { key: 'action',     title: 'Action', type: 'select', options: AUDIT_ACTION_OPTIONS },
  { key: 'entityName', title: 'Entity', type: 'select', options: AUDIT_ENTITY_OPTIONS },
  { key: 'from',       title: 'From',   type: 'date' },
  { key: 'to',         title: 'To',     type: 'date' },
];

const auditLogColumnMapper: Attribute[] = [
  { key: 'createdAt',  title: 'Time',      type: 'datetime' },
  { key: 'userName',   title: 'User',      type: 'string' },
  { key: 'action',     title: 'Action',    type: 'string' },
  { key: 'entityName', title: 'Entity',    type: 'string' },
  { key: 'entityId',   title: 'Entity ID', type: 'string' },
  { key: 'ipAddress',  title: 'IP',        type: 'string' },
];

export default function AuditLogManage() {
  const { logs, total, params, setParams, loading, refetch } = useAuditLogs();

  const filters: Filter[] = useMemo(
    () =>
      Object.entries(params)
        .filter(([k, v]) => v !== undefined && v !== '' && k !== 'page' && k !== 'pageSize')
        .map(([key, value]) => ({ key, value })),
    [params]
  );

  const applyFilters = (next: Filter[]) => {
    const patch: AuditLogFilterParams = { page: 1, pageSize: params.pageSize };
    for (const f of next) {
      let value = f.value;
      // "From 24-09" means from 00:00, "To 24-09" means up to the end of that day
      if (f.key === 'from') value = dayjs(value as string).startOf('day').toISOString();
      if (f.key === 'to')   value = dayjs(value as string).endOf('day').toISOString();
      (patch as Record<string, unknown>)[f.key] = value;
    }
    setParams(patch);
  };

  return (
    <div style={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      <DataTableHeader
        filter
        filters={filters}
        filterAttributeMapper={auditLogFilterMapper}
        updateFilterFunction={applyFilters}
        filterFunction={refetch}
        loading={loading}
      />
      <div style={{ flex: 1, overflow: 'hidden' }}>
        <DataTable
          name="audit log"
          dataSource={logs}
          attributeMapper={auditLogColumnMapper}
          reference={{ path: path.auditLogs.detail, key: 'id' }}
          pageNumber={params.page ?? 1}
          pageSize={params.pageSize ?? 20}
          loading={loading}
        />
      </div>
      <TablePagination
        totalItem={total}
        pageSize={params.pageSize ?? 20}
        pageNumber={params.page ?? 1}
        pageSizeOptions={[10, 20, 50, 100]}
        onChangeFunction={(p, s) =>
          setParams((prev) => ({ ...prev, page: s !== prev.pageSize ? 1 : p, pageSize: s }))
        }
        loading={loading}
        entryName="logs"
      />
    </div>
  );
}
