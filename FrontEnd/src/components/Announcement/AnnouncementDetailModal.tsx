import React, { useEffect } from "react";
import { Modal, Tag, Typography, Spin, Divider } from "antd";
import {
  CalendarOutlined,
  ClockCircleOutlined,
} from "@ant-design/icons";
import dayjs from "dayjs";
import type { AnnouncementItem } from "@/types/announcement.type";
import { useAnnouncementDetail } from "@/hooks/announcement.hook";
import { getCategoryColor } from "./AnnouncementCard";

const { Title, Paragraph, Text } = Typography;

export interface AnnouncementDetailModalProps {
  announcement: AnnouncementItem | null;
  open: boolean;
  onClose: () => void;
}

export const AnnouncementDetailModal: React.FC<AnnouncementDetailModalProps> = ({
  announcement,
  open,
  onClose,
}) => {
  const { detail, loading, fetchDetail, clearDetail } = useAnnouncementDetail();

  useEffect(() => {
    if (open && announcement?.id) {
      fetchDetail(announcement.id);
    } else if (!open) {
      clearDetail();
    }
  }, [open, announcement?.id, fetchDetail, clearDetail]);

  const displayItem = detail || announcement;
  const displayDate = displayItem?.publishedAt || displayItem?.createdAt;

  return (
    <Modal
      open={open}
      onCancel={onClose}
      footer={null}
      width={720}
      destroyOnClose
      centered
      className="ann-detail-modal"
    >
      {loading && !displayItem ? (
        <div className="ann-detail-modal__loading">
          <Spin size="large" />
        </div>
      ) : displayItem ? (
        <div className="ann-detail-modal__content">
          {/* Categories */}
          <div className="ann-detail-modal__categories">
            {displayItem.categories && displayItem.categories.length > 0 ? (
              displayItem.categories.map((cat) => (
                <Tag key={cat.id} color={getCategoryColor(cat.name)}>
                  {cat.name}
                </Tag>
              ))
            ) : (
              <Tag color="default">General</Tag>
            )}
          </div>

          {/* Title */}
          <Title level={3} className="ann-detail-modal__title">
            {displayItem.title}
          </Title>

          {/* Metadata Bar */}
          <div className="ann-detail-modal__meta">
            {displayDate && (
              <>
                <span>
                  <CalendarOutlined style={{ marginRight: 6 }} />
                  {dayjs(displayDate).format("DD MMMM YYYY")}
                </span>
                <span>•</span>
                <span>
                  <ClockCircleOutlined style={{ marginRight: 6 }} />
                  {dayjs(displayDate).format("HH:mm")}
                </span>
              </>
            )}
          </div>

          <Divider style={{ margin: "1rem 0" }} />

          {/* Loading spinner overlay when fetching full content */}
          {loading && (
            <div style={{ textAlign: "center", padding: "1rem" }}>
              <Spin tip="Loading full content..." />
            </div>
          )}

          {/* Content Body */}
          <div className="ann-detail-modal__body">
            {detail?.content ? (
              <Paragraph style={{ fontSize: "1rem", lineHeight: 1.8, whiteSpace: "pre-wrap" }}>
                {detail.content}
              </Paragraph>
            ) : (
              <Paragraph style={{ fontSize: "1rem", lineHeight: 1.8, color: "var(--muted-foreground)" }}>
                {displayItem.summary || "No additional content available."}
              </Paragraph>
            )}
          </div>
        </div>
      ) : (
        <Text type="secondary">Announcement not found.</Text>
      )}
    </Modal>
  );
};

export default AnnouncementDetailModal;
