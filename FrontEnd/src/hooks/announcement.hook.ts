import { useState, useEffect, useCallback, useRef } from "react";
import announcementApi from "@/apis/announcement.api";
import type {
  AnnouncementDetail,
  AnnouncementFilterParams,
  AnnouncementItem,
} from "@/types/announcement.type";

export interface UseAnnouncementsOptions {
  initialFilters?: AnnouncementFilterParams;
  autoFetch?: boolean;
}

export const useAnnouncements = (options: UseAnnouncementsOptions = {}) => {
  const { initialFilters = {}, autoFetch = true } = options;

  const [announcements, setAnnouncements] = useState<AnnouncementItem[]>([]);
  const [total, setTotal] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [filters, setFiltersState] = useState<AnnouncementFilterParams>({
    page: 1,
    pageSize: 10,
    ...initialFilters,
  });

  const abortControllerRef = useRef<AbortController | null>(null);

  const fetchAnnouncements = useCallback(
    async (customFilters?: AnnouncementFilterParams) => {
      // Abort previous in-flight request if any
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
      abortControllerRef.current = new AbortController();

      const queryParams = customFilters ?? filters;
      setLoading(true);
      setError(null);

      try {
        const result = await announcementApi.getAnnouncements(
          queryParams,
          abortControllerRef.current.signal
        );

        setAnnouncements(result.items ?? []);
        setTotal(result.total ?? 0);
        setTotalPages(result.totalPages ?? 0);
      } catch (err: unknown) {
        if (err instanceof Error && err.name === "CanceledError") {
          return;
        }
        const errorMsg =
          err instanceof Error
            ? err.message
            : "Failed to fetch announcements. Please try again later.";
        setError(errorMsg);
      } finally {
        setLoading(false);
      }
    },
    [filters]
  );

  useEffect(() => {
    if (autoFetch) {
      fetchAnnouncements();
    }
    return () => {
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, [fetchAnnouncements, autoFetch]);

  const setFilters = useCallback((newFilters: Partial<AnnouncementFilterParams>) => {
    setFiltersState((prev) => ({
      ...prev,
      ...newFilters,
      // If filtering criteria changes (not just page), reset to page 1
      page: newFilters.page !== undefined ? newFilters.page : 1,
    }));
  }, []);

  const setPage = useCallback((page: number) => {
    setFiltersState((prev) => ({ ...prev, page }));
  }, []);

  const setPageSize = useCallback((pageSize: number) => {
    setFiltersState((prev) => ({ ...prev, pageSize, page: 1 }));
  }, []);

  const setKeyword = useCallback((keyword: string) => {
    setFiltersState((prev) => ({
      ...prev,
      keyword: keyword.trim() || undefined,
      page: 1,
    }));
  }, []);

  const setCategoryName = useCallback((categoryName?: string) => {
    setFiltersState((prev) => ({
      ...prev,
      categoryName: categoryName || undefined,
      page: 1,
    }));
  }, []);

  const setDateRange = useCallback((startDate?: string, endDate?: string) => {
    setFiltersState((prev) => ({
      ...prev,
      startDate: startDate || undefined,
      endDate: endDate || undefined,
      page: 1,
    }));
  }, []);

  const resetFilters = useCallback(() => {
    setFiltersState({
      page: 1,
      pageSize: 10,
      ...initialFilters,
    });
  }, [initialFilters]);

  return {
    announcements,
    total,
    totalPages,
    page: filters.page ?? 1,
    pageSize: filters.pageSize ?? 10,
    loading,
    error,
    filters,
    setFilters,
    setPage,
    setPageSize,
    setKeyword,
    setCategoryName,
    setDateRange,
    resetFilters,
    refetch: fetchAnnouncements,
  };
};

export const useAnnouncementDetail = (id?: string | null) => {
  const [detail, setDetail] = useState<AnnouncementDetail | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchDetail = useCallback(async (targetId: string) => {
    setLoading(true);
    setError(null);
    try {
      const data = await announcementApi.getAnnouncementById(targetId);
      setDetail(data);
      return data;
    } catch (err: unknown) {
      const errorMsg =
        err instanceof Error ? err.message : "Failed to load announcement details.";
      setError(errorMsg);
      return null;
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (id) {
      fetchDetail(id);
    } else {
      setDetail(null);
    }
  }, [id, fetchDetail]);

  return {
    detail,
    loading,
    error,
    fetchDetail,
    clearDetail: () => setDetail(null),
  };
};
