import React from "react";
import { Tag, Badge, Tooltip } from "antd";
import { ClockCircleOutlined, CalendarOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import relativeTime from "dayjs/plugin/relativeTime";
import type { AnnouncementItem } from "@/types/announcement.type";

dayjs.extend(relativeTime);

export interface AnnouncementCardProps {
  announcement: AnnouncementItem;
  onClick?: (announcement: AnnouncementItem) => void;
  compact?: boolean;
  className?: string;
}

const CATEGORY_COLOR_PALETTE = [
  "blue",
  "cyan",
  "geekblue",
  "purple",
  "magenta",
  "orange",
  "green",
];

export const getCategoryColor = (name: string): string => {
  let hash = 0;
  for (let i = 0; i < name.length; i++) {
    hash = name.charCodeAt(i) + ((hash << 5) - hash);
  }
  const index = Math.abs(hash) % CATEGORY_COLOR_PALETTE.length;
  return CATEGORY_COLOR_PALETTE[index];
};

export const AnnouncementCard: React.FC<AnnouncementCardProps> = ({
  announcement,
  onClick,
  compact = false,
  className = "",
}) => {
  const displayDate = announcement.publishedAt || announcement.createdAt;
  const isRecent = displayDate
    ? Date.now() - new Date(displayDate).getTime() < 3 * 24 * 60 * 60 * 1000
    : false;

  return (
    <article
      className={`ann-card ${compact ? "ann-card--compact" : ""} ${className}`}
      onClick={() => onClick?.(announcement)}
      tabIndex={0}
      role="button"
      onKeyDown={(e) => {
        if (e.key === "Enter" || e.key === " ") {
          e.preventDefault();
          onClick?.(announcement);
        }
      }}
    >
      {/* Top Header: Categories + Time */}
      <div className="ann-card__top">
        <div className="ann-card__badges">
          {announcement.categories && announcement.categories.length > 0 ? (
            announcement.categories.map((cat) => (
              <Tag key={cat.id} color={getCategoryColor(cat.name)}>
                {cat.name}
              </Tag>
            ))
          ) : (
            <Tag color="default">General</Tag>
          )}

          {isRecent && (
            <Badge
              count="NEW"
              style={{
                backgroundColor: "#52c41a",
                fontSize: "0.68rem",
                fontWeight: 700,
                boxShadow: "none",
              }}
            />
          )}
        </div>

        {displayDate && (
          <Tooltip title={dayjs(displayDate).format("DD MMMM YYYY, HH:mm")}>
            <span className="ann-card__time">
              <ClockCircleOutlined style={{ marginRight: 4 }} />
              {dayjs(displayDate).fromNow()}
            </span>
          </Tooltip>
        )}
      </div>

      {/* Title */}
      <h3 className="ann-card__title">{announcement.title}</h3>

      {/* Summary */}
      {announcement.summary && (
        <p className="ann-card__summary">{announcement.summary}</p>
      )}

      {/* Card Footer */}
      {!compact && displayDate && (
        <div className="ann-card__footer">
          <span className="ann-card__date">
            <CalendarOutlined style={{ marginRight: 6 }} />
            {dayjs(displayDate).format("DD MMM YYYY")}
          </span>
          <span className="ann-card__action">Read more →</span>
        </div>
      )}
    </article>
  );
};

export default AnnouncementCard;
