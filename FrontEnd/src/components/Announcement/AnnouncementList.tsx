import React from "react";
import { Skeleton, Empty, Alert, Button } from "antd";
import { ReloadOutlined } from "@ant-design/icons";
import type { AnnouncementItem } from "@/types/announcement.type";
import AnnouncementCard from "./AnnouncementCard";

export interface AnnouncementListProps {
  announcements: AnnouncementItem[];
  loading?: boolean;
  error?: string | null;
  onSelect?: (announcement: AnnouncementItem) => void;
  onRetry?: () => void;
  emptyMessage?: string;
  skeletonCount?: number;
  className?: string;
}

export const AnnouncementList: React.FC<AnnouncementListProps> = ({
  announcements,
  loading = false,
  error = null,
  onSelect,
  onRetry,
  emptyMessage = "No announcements found.",
  skeletonCount = 5,
  className = "",
}) => {
  if (error) {
    return (
      <div className="ann-list__error">
        <Alert
          message="Failed to load announcements"
          description={error}
          type="error"
          showIcon
          action={
            onRetry && (
              <Button size="small" type="primary" danger onClick={onRetry} icon={<ReloadOutlined />}>
                Retry
              </Button>
            )
          }
        />
      </div>
    );
  }

  if (loading && announcements.length === 0) {
    return (
      <div className="ann-list__skeletons">
        {Array.from({ length: skeletonCount }).map((_, idx) => (
          <div key={idx} className="ann-card ann-card--skeleton">
            <Skeleton active title={{ width: "60%" }} paragraph={{ rows: 2, width: ["100%", "80%"] }} />
          </div>
        ))}
      </div>
    );
  }

  if (announcements.length === 0) {
    return (
      <div className="ann-list__empty">
        <Empty description={emptyMessage} image={Empty.PRESENTED_IMAGE_SIMPLE} />
      </div>
    );
  }

  return (
    <div className={`ann-list ${className}`}>
      {announcements.map((announcement) => (
        <AnnouncementCard
          key={announcement.id}
          announcement={announcement}
          onClick={onSelect}
        />
      ))}
    </div>
  );
};

export default AnnouncementList;
