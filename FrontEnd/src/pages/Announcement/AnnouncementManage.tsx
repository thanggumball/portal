import { useState, useMemo } from "react";
import { useAnnouncements } from "@/hooks/announcement.hook";
import {
  AnnouncementFilterBar,
  AnnouncementList,
  AnnouncementDetailModal,
  type CategoryOption,
} from "@/components/Announcement";
import { TablePagination } from "@/components/common/Common";
import type { AnnouncementItem } from "@/types/announcement.type";

import "./AnnouncementManage.scss";

export default function AnnouncementManage() {
  const [selectedAnnouncement, setSelectedAnnouncement] =
    useState<AnnouncementItem | null>(null);
  const [detailModalOpen, setDetailModalOpen] = useState(false);

  // Hook handles query state, pagination, caching/abort controller, loading & error
  const {
    announcements,
    total,
    page,
    pageSize,
    loading,
    error,
    filters,
    setPage,
    setPageSize,
    setKeyword,
    setCategoryName,
    setDateRange,
    resetFilters,
    refetch,
  } = useAnnouncements({
    initialFilters: {
      page: 1,
      pageSize: 10,
    },
  });

  // Extract unique categories dynamically from fetched announcements to enrich filter options
  const categoryOptions = useMemo<CategoryOption[]>(() => {
    const set = new Set<string>();
    announcements.forEach((a) => {
      a.categories?.forEach((c) => {
        if (c.name) set.add(c.name);
      });
    });

    const dynamicOptions: CategoryOption[] = [
      { value: "", label: "All Categories" },
    ];

    // Standard baseline categories
    const baseline = ["General", "Academic", "Event", "Urgent"];
    baseline.forEach((cat) => set.add(cat));

    set.forEach((name) => {
      dynamicOptions.push({ value: name, label: name });
    });

    return dynamicOptions;
  }, [announcements]);

  const handleCardClick = (item: AnnouncementItem) => {
    setSelectedAnnouncement(item);
    setDetailModalOpen(true);
  };

  const handleCloseModal = () => {
    setDetailModalOpen(false);
    setSelectedAnnouncement(null);
  };

  return (
    <div className="ann-manage-page">
      {/* ── Top Bar: Search, Category & Refresh ── */}
      <div className="ann-manage-page__header">
        <div>
          <h2 className="ann-manage-page__title">Announcements</h2>
          <p className="ann-manage-page__subtitle">
            Stay updated with the latest campus notices, academic schedules, and news.
          </p>
        </div>

        <AnnouncementFilterBar
          keyword={filters.keyword || ""}
          categoryName={filters.categoryName || ""}
          categories={categoryOptions}
          loading={loading}
          onSearchChange={setKeyword}
          onCategoryChange={setCategoryName}
          onDateRangeChange={setDateRange}
          onReset={resetFilters}
          onRefresh={refetch}
        />
      </div>

      {/* ── Content Feed ── */}
      <div className="ann-manage-page__body">
        <AnnouncementList
          announcements={announcements}
          loading={loading}
          error={error}
          onSelect={handleCardClick}
          onRetry={refetch}
          emptyMessage="No announcements match your search criteria."
        />
      </div>

      {/* ── Pagination ── */}
      {total > 0 && (
        <div className="ann-manage-page__footer">
          <TablePagination
            totalItem={total}
            pageSize={pageSize}
            pageNumber={page}
            pageSizeOptions={[5, 10, 20, 50]}
            onChangeFunction={(newPage, newPageSize) => {
              if (newPageSize !== pageSize) {
                setPageSize(newPageSize);
              } else {
                setPage(newPage);
              }
            }}
            loading={loading}
            entryName="announcements"
          />
        </div>
      )}

      {/* ── Reusable Detail Modal ── */}
      <AnnouncementDetailModal
        announcement={selectedAnnouncement}
        open={detailModalOpen}
        onClose={handleCloseModal}
      />
    </div>
  );
}
