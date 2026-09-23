import React, { useState, useEffect } from "react";
import { Input, Select, Button, Space, DatePicker } from "antd";
import {
  SearchOutlined,
  ReloadOutlined,
  FilterOutlined,
  ClearOutlined,
} from "@ant-design/icons";
import type { Dayjs } from "dayjs";

const { RangePicker } = DatePicker;

export interface CategoryOption {
  value: string;
  label: string;
}

export interface AnnouncementFilterBarProps {
  keyword?: string;
  categoryName?: string;
  categories?: CategoryOption[];
  loading?: boolean;
  onSearchChange: (keyword: string) => void;
  onCategoryChange: (category?: string) => void;
  onDateRangeChange?: (startDate?: string, endDate?: string) => void;
  onReset: () => void;
  onRefresh?: () => void;
  showDateFilter?: boolean;
  className?: string;
}

const DEFAULT_CATEGORIES: CategoryOption[] = [
  { value: "", label: "All Categories" },
  { value: "General", label: "General" },
  { value: "Academic", label: "Academic" },
  { value: "Event", label: "Event" },
  { value: "Urgent", label: "Urgent" },
];

export const AnnouncementFilterBar: React.FC<AnnouncementFilterBarProps> = ({
  keyword = "",
  categoryName = "",
  categories = DEFAULT_CATEGORIES,
  loading = false,
  onSearchChange,
  onCategoryChange,
  onDateRangeChange,
  onReset,
  onRefresh,
  showDateFilter = false,
  className = "",
}) => {
  const [localKeyword, setLocalKeyword] = useState(keyword);

  // Sync external keyword prop if changed externally (e.g. on reset)
  useEffect(() => {
    setLocalKeyword(keyword);
  }, [keyword]);

  // Debounce search input
  useEffect(() => {
    const timer = setTimeout(() => {
      if (localKeyword !== keyword) {
        onSearchChange(localKeyword);
      }
    }, 350);

    return () => clearTimeout(timer);
  }, [localKeyword, keyword, onSearchChange]);

  const handleDateChange = (dates: [Dayjs | null, Dayjs | null] | null) => {
    if (!onDateRangeChange) return;
    if (dates && dates[0] && dates[1]) {
      onDateRangeChange(
        dates[0].startOf("day").toISOString(),
        dates[1].endOf("day").toISOString()
      );
    } else {
      onDateRangeChange(undefined, undefined);
    }
  };

  const hasActiveFilters = Boolean(keyword || categoryName);

  return (
    <div className={`ann-filter-bar ${className}`}>
      <div className="ann-filter-bar__left">
        <Input
          placeholder="Search by title, summary, or content..."
          prefix={<SearchOutlined style={{ color: "var(--muted-foreground)" }} />}
          allowClear
          value={localKeyword}
          onChange={(e) => setLocalKeyword(e.target.value)}
          className="ann-filter-bar__search"
        />

        <Select
          value={categoryName || ""}
          options={categories}
          onChange={(val) => onCategoryChange(val || undefined)}
          suffixIcon={<FilterOutlined />}
          className="ann-filter-bar__category"
          placeholder="Filter by Category"
        />

        {showDateFilter && onDateRangeChange && (
          <RangePicker
            onChange={handleDateChange}
            className="ann-filter-bar__dates"
            format="DD/MM/YYYY"
          />
        )}

        {hasActiveFilters && (
          <Button
            icon={<ClearOutlined />}
            onClick={() => {
              setLocalKeyword("");
              onReset();
            }}
          >
            Clear
          </Button>
        )}
      </div>

      <div className="ann-filter-bar__right">
        {onRefresh && (
          <Space>
            <Button
              icon={<ReloadOutlined spin={loading} />}
              onClick={onRefresh}
              loading={loading}
            >
              Refresh
            </Button>
          </Space>
        )}
      </div>
    </div>
  );
};

export default AnnouncementFilterBar;
