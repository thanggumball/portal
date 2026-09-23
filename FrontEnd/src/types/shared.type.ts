export interface ApiResponse<T> {
  success: boolean;
  message: string | null;
  data: T | null;
}

export interface PagedResult<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}