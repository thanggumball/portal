export const AnnouncementStatus = {
  Draft: 1,
  Published: 2,
  Archived: 3,
} as const;

export type AnnouncementStatusType = (typeof AnnouncementStatus)[keyof typeof AnnouncementStatus];

export const AnnouncementRoleReceived = {
  All: 1,
  Student: 2,
  Staff: 3,
} as const;

export type AnnouncementRoleReceivedType = (typeof AnnouncementRoleReceived)[keyof typeof AnnouncementRoleReceived];

export interface CategoryItem {
  id: string;
  name: string;
}

export interface AnnouncementItem {
  id: string;
  title: string;
  summary?: string | null;
  status: AnnouncementStatusType;
  publishedAt?: string | null;
  createdAt: string;
  categories: CategoryItem[];
}

export interface AnnouncementDetail extends AnnouncementItem {
  content: string;
}

export interface AnnouncementFilterParams {
  page?: number;
  pageSize?: number;
  keyword?: string;
  status?: AnnouncementStatusType;
  categoryId?: string;
  categoryName?: string;
  startDate?: string;
  endDate?: string;
}

export interface PagedResult<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
