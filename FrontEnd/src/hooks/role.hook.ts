import { useEffect, useState } from 'react';
import roleApi from '@/apis/role.api';
import type { RoleResponse } from '@/types/role.type';

export const useRoles = () => {
  const [roles, setRoles] = useState<RoleResponse[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<unknown>(null);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        setLoading(true);
        setError(null);
        const response = await roleApi.getAll();
        if (!cancelled) setRoles(response.data.data ?? []);
      } catch (e) {
        if (!cancelled) setError(e);
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => { cancelled = true; };
  }, []);

  return { roles, loading, error };
};