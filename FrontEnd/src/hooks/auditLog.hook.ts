import { useCallback, useEffect, useState } from 'react';
import auditLogApi from '@/apis/auditLog.api';
import type { AuditLogDetail, AuditLogFilterParams, AuditLogListItem } from '@/types/auditLog.type';

export const useAuditLogs = (initialParams: AuditLogFilterParams = {}) => {
  const [logs, setLogs] = useState<AuditLogListItem[]>([]);
  const [total, setTotal] = useState(0);
  const [params, setParams] = useState<AuditLogFilterParams>({
    page: 1,
    pageSize: 20,
    ...initialParams,
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<unknown>(null);

  // Bumping this number is how "Refresh" asks the effect below to run again
  const [reloadKey, setReloadKey] = useState(0);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        setLoading(true);
        setError(null);
        const response = await auditLogApi.search(params);
        if (cancelled) return;
        const paged = response.data.data;
        setLogs(paged?.items ?? []);
        setTotal(paged?.total ?? 0);
      } catch (e) {
        if (cancelled) return;
        setError(e);
        setLogs([]);
        setTotal(0);
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => { cancelled = true; };
  }, [params, reloadKey]);

  const refetch = useCallback(() => setReloadKey((k) => k + 1), []);

  return { logs, total, params, setParams, loading, error, refetch };
};

export const useAuditLogDetail = (id?: string) => {
  const [log, setLog] = useState<AuditLogDetail | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<unknown>(null);

  useEffect(() => {
    if (!id) return;
    let cancelled = false;
    (async () => {
      try {
        setLoading(true);
        setError(null);
        const response = await auditLogApi.getById(id);
        if (!cancelled) setLog(response.data.data);
      } catch (e) {
        if (!cancelled) setError(e);
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => { cancelled = true; };
  }, [id]);

  return { log, loading, error };
};
