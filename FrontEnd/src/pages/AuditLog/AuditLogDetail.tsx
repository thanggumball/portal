import { useMemo } from 'react';
import { useNavigate, useParams } from 'react-router';
import { Alert, Button, Card, Descriptions, Empty, Spin, Table, Tag, Typography } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import { useAuditLogDetail } from '@/hooks/auditLog.hook';
import { useRoles } from '@/hooks/role.hook';
import { USER_STATUS_OPTIONS } from '@/types/user.type';
import { AnnouncementRoleReceived, AnnouncementStatus } from '@/types/announcement.type';

dayjs.extend(utc);

const { Text } = Typography;

const ACTION_COLORS: Record<string, string> = {
  Create: 'green',
  Update: 'blue',
  Delete: 'red',
  SoftDelete: 'orange',
};

// A brand-new row always has these values, so showing them on Create tells the reader nothing
const HIDDEN_ON_CREATE = new Set(['IsDeleted']);

const FIELD_LABELS: Record<string, string> = {
  RoleId: 'Role',
  RoleReceived: 'Audience',
};

// OldValue / NewValue are JSON strings like {"FullName":"An","Status":1}
const parseJson = (raw: string | null): Record<string, unknown> => {
  if (!raw) return {};
  try {
    return JSON.parse(raw) as Record<string, unknown>;
  } catch {
    return {};
  }
};

// "FullName" -> "Full Name"
const fieldLabel = (field: string) => FIELD_LABELS[field] ?? field.replace(/([a-z])([A-Z])/g, '$1 $2');

const enumName = (enumObj: Record<string, number>, v: unknown) =>
  Object.keys(enumObj).find((k) => enumObj[k] === v);

interface ChangeRow {
  field: string;
  oldValue: string;
  newValue: string;
}

export default function AuditLogDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { log, loading, error } = useAuditLogDetail(id);
  const { roles } = useRoles();

  // Hooks must run before any early return below
  const changes = useMemo<ChangeRow[]>(() => {
    if (!log) return [];

    // Turn raw ids and enum numbers into something a person can read
    const showValue = (field: string, v: unknown): string => {
      if (v === undefined) return '—';
      if (v === null || v === '') return '(empty)';
      if (field === 'RoleId') return roles.find((r) => r.id.toLowerCase() === String(v).toLowerCase())?.name ?? String(v);
      if (field === 'Status' && log.entityName === 'User') return USER_STATUS_OPTIONS.find((o) => o.value === v)?.label ?? String(v);
      if (field === 'Status' && log.entityName === 'Announcement') return enumName(AnnouncementStatus, v) ?? String(v);
      if (field === 'RoleReceived') return enumName(AnnouncementRoleReceived, v) ?? String(v);
      if (typeof v === 'boolean') return v ? 'Yes' : 'No';
      if (typeof v === 'object') return JSON.stringify(v);
      return String(v);
    };

    const oldObj = parseJson(log.oldValue);
    const newObj = parseJson(log.newValue);
    const fields = Array.from(new Set([...Object.keys(oldObj), ...Object.keys(newObj)]));

    return fields
      // Older logs (written before the backend fix) also list columns that did not change
      .filter((f) => JSON.stringify(oldObj[f]) !== JSON.stringify(newObj[f]))
      .filter((f) => !(log.action === 'Create' && HIDDEN_ON_CREATE.has(f)))
      .map((field) => ({
        field,
        oldValue: showValue(field, oldObj[field]),
        newValue: showValue(field, newObj[field]),
      }));
  }, [log, roles]);

  if (loading) {
    return <div style={{ textAlign: 'center', padding: '3rem' }}><Spin size="large" /></div>;
  }

  if (error) {
    return <Alert type="error" showIcon title="Failed to load audit log" />;
  }

  if (!log) {
    return <Empty description="Audit log not found" />;
  }

  // Create has no old value and Delete has no new value, so an Old/New pair would leave one column all "—"
  const fieldColumn = { key: 'field', title: 'Field', dataIndex: 'field', width: 200, render: (f: string) => fieldLabel(f) };
  const changeColumns: ColumnsType<ChangeRow> =
    log.action === 'Create'
      ? [fieldColumn, { key: 'newValue', title: 'Value', dataIndex: 'newValue' }]
      : log.action === 'Delete'
        ? [fieldColumn, { key: 'oldValue', title: 'Value', dataIndex: 'oldValue' }]
        : [
            fieldColumn,
            { key: 'oldValue', title: 'Old value', dataIndex: 'oldValue', render: (v: string) => <Text type="secondary">{v}</Text> },
            { key: 'newValue', title: 'New value', dataIndex: 'newValue', render: (v: string) => <Text strong>{v}</Text> },
          ];

  return (
    <div style={{ height: '100%', overflow: 'auto', display: 'flex', flexDirection: 'column', gap: '1rem' }}>
      <div>
        <Button onClick={() => navigate(-1)}>Back</Button>
      </div>

      <Card title="Log information">
        <Descriptions
          column={2}
          items={[
            { key: 'time',     label: 'Time',      children: dayjs.utc(log.createdAt).local().format('DD-MM-YYYY HH:mm:ss') },
            { key: 'action',   label: 'Action',    children: <Tag color={ACTION_COLORS[log.action] ?? 'default'}>{log.action}</Tag> },
            { key: 'user',     label: 'User',      children: log.userName ?? '(system)' },
            { key: 'ip',       label: 'IP',        children: log.ipAddress ?? '—' },
            { key: 'entity',   label: 'Entity',    children: log.entityName },
            { key: 'entityId', label: 'Entity ID', children: log.entityId ?? '—' },
          ]}
        />
      </Card>

      <Card title="Changes">
        <Table<ChangeRow>
          rowKey="field"
          dataSource={changes}
          pagination={false}
          locale={{ emptyText: 'No field values actually changed' }}
          columns={changeColumns}
        />
      </Card>
    </div>
  );
}
