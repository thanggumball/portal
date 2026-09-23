import http from "@/lib/http/http";
import type {
  AnnouncementDetail,
  AnnouncementFilterParams,
  AnnouncementItem,
  PagedResult,
} from "@/types/announcement.type";

export const URL_ANNOUNCEMENT = {
  BASE: "/announcements",
  DETAIL: (id: string) => `/announcements/${id}`,
};

export const announcementApi = {
  getAnnouncements: async (
    params?: AnnouncementFilterParams,
    signal?: AbortSignal
  ): Promise<PagedResult<AnnouncementItem>> => {
    // Clean up empty params before sending
    const cleanParams: Record<string, unknown> = {};
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== "") {
          cleanParams[key] = value;
        }
      });
    }

    const response = await http.get<PagedResult<AnnouncementItem> | { data: PagedResult<AnnouncementItem> }>(
      URL_ANNOUNCEMENT.BASE,
      {
        params: cleanParams,
        signal,
      }
    );

    const rawData = response.data as Record<string, unknown>;
    // Handle both direct PagedResult and wrapped { data: PagedResult } formats
    if (rawData && typeof rawData === "object" && "data" in rawData && (rawData.data as Record<string, unknown>)?.items) {
      return rawData.data as PagedResult<AnnouncementItem>;
    }

    return response.data as PagedResult<AnnouncementItem>;
  },

  getAnnouncementById: async (
    id: string,
    signal?: AbortSignal
  ): Promise<AnnouncementDetail> => {
    const response = await http.get<AnnouncementDetail | { data: AnnouncementDetail }>(
      URL_ANNOUNCEMENT.DETAIL(id),
      { signal }
    );

    const rawData = response.data as Record<string, unknown>;
    if (rawData && typeof rawData === "object" && "data" in rawData && (rawData.data as Record<string, unknown>)?.id) {
      return rawData.data as AnnouncementDetail;
    }

    return response.data as AnnouncementDetail;
  },
};

export default announcementApi;
