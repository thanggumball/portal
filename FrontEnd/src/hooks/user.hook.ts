import { useCallback, useEffect, useState } from 'react';
import userApi from '@/apis/user.api';
import type { UserFilterParams, UserResponse } from '@/types/user.type';

export const useUsers = (initialParams: UserFilterParams = {}) => {
  const [users, setUsers] = useState<UserResponse[]>([]);
  const [total, setTotal] = useState(0);
  const [params, setParams] = useState<UserFilterParams>({
    page: 1,
    pageSize: 10,
    ...initialParams,
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<unknown>(null);

  const fetchUsers = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await userApi.search(params);
      const paged = response.data.data;
      setUsers(paged?.items ?? []);
      setTotal(paged?.total ?? 0);
    } catch (e) {
      setError(e);
      setUsers([]);
      setTotal(0);
    } finally {
      setLoading(false);
    }
  }, [params]);

  useEffect(() => {
    fetchUsers();
  }, [fetchUsers]);

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
    users,
    total,
    params,
    getMe,
    setParams,
    loading,
    error,
    refetch: fetchUsers,
  };
};
